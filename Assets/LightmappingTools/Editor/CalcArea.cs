using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
    Coded for Unity Summer of Code 2009 by Michal Mandrysz
    Feel free to modify for your own needs
    http://masteranza.wordpress.com/unity/lightmapping/
    http://unity3d.com
*/

class CalcArea
{
    static public int whichLightmap=1;
    public static Rect[] CalculateArea(MeshFilter[] mf, int resolution, ArrayList transformedVerticles)
    {
        ArrayList surfaces = new ArrayList();
        MeshFilter temp;
        float totalArea = 0;
        Vector3[] transVert;
        for (int i = 0; i < mf.Length; i++)
        {
            transVert = (Vector3[]) transformedVerticles[i];
            temp = (MeshFilter)mf[i];
            float area = 0;
            int count = temp.sharedMesh.triangles.Length / 3;
            for (int j = 0; j < count; j++)
            {
                area += Math.Abs(Vector3.Cross(transVert[temp.sharedMesh.triangles[j * 3]] - transVert[temp.sharedMesh.triangles[j * 3 + 1]], transVert[temp.sharedMesh.triangles[j * 3]] - transVert[temp.sharedMesh.triangles[j * 3 + 2]]).magnitude);
            }
            surfaces.Add(area);
            totalArea += Math.Abs(area);
        }
        return PackObjects(mf, surfaces, totalArea, 1.00f, resolution);
    }

    public static void CheckIfNormalized(MeshFilter temp)
    {
        
        
        //Path search
        Transform current = temp.transform;
        while (current.parent!=null)
        {
            current = current.parent;
        }
        UnityEngine.Object prefabed = EditorUtility.GetPrefabParent(current);
        string path = AssetDatabase.GetAssetPath(prefabed);
        if (path.Length<1) path = "primitive.notAFile";


        if (temp.sharedMesh.uv2.Length == temp.sharedMesh.vertexCount)
        {
            Vector2[] uvs = temp.sharedMesh.uv2;
            bool normalized = true;
            for (int j = 0; j < uvs.Length && normalized; j++)
            {
                if (uvs[j].x > 1 || uvs[j].x < 0 || uvs[j].y < 0 || uvs[j].y > 1)
                {
                    LightmappingTool.notNormalized2.Add(path);
                    LightmappingTool.notNormalized2.Add(temp.transform);
                    normalized = false;
                }
            }


            //Overlapping test
            if (SystemInfo.supportsRenderTextures)
            {
                Shader finded = Shader.Find("ExternalLightmappingTool/uvDebug");
                if (finded == null)
                {
                    Debug.LogError("Cannot find uvDebug shader within LightmappingTools folder!");
                    return;
                }

                int res = 128;



                RenderTexture rt = RenderTexture.GetTemporary(res, res, 0);
                RenderTexture.active = rt;


                if (Camera.current == null)
                {
                    Camera.SetupCurrent(Camera.main);
                }

                RenderTexture oldRT = RenderTexture.active;
                rt = RenderTexture.GetTemporary(res, res, 0);
                RenderTexture.active = rt;

                GL.Clear(true, true, Color.black);

                Material mat = new Material(finded);

                Mesh mesh = temp.sharedMesh;
                mat.SetPass(0);
                GL.PushMatrix();
                GL.LoadOrtho();
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                GL.PopMatrix();
                UnityEngine.Object.DestroyImmediate(mat);

                Texture2D texture = new Texture2D(res, res, TextureFormat.ARGB32, false);
                texture.ReadPixels(new Rect(0, 0, res, res), 0, 0);
                texture.Apply();
                RenderTexture.active = oldRT;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(rt);
                //byte[] bytes = texture.EncodeToPNG();
                //File.WriteAllBytes(Application.dataPath + "/../test.png", bytes);
                Color[] tmp = texture.GetPixels();

                int i = 0;
                int incorrect = 0;
                for (; i < tmp.Length; i++)
                {
                    if (tmp[i].r > 0.6f)
                    {
                        incorrect++;
                        texture.SetPixel(i % res, i / res, Color.red);
                    }
                }


                if (incorrect * 300 > res * res)
                {
                    texture.Apply();
                    LightmappingTool.overlapping2.Add(path);
                    LightmappingTool.overlapping2.Add(texture);
                    LightmappingTool.overlapping2.Add(temp.transform);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
            //UnityEngine.Object.DestroyImmediate(texture);
        }
        else if (temp.sharedMesh.uv.Length == temp.sharedMesh.vertexCount)
        {
            Vector2 tempek = Vector2.zero;
            for (int jj = 0; jj < temp.sharedMesh.uv.Length && tempek.Equals(Vector2.zero); jj++)
            {
                tempek = temp.sharedMesh.uv[jj];
            }
            if (tempek.Equals(Vector2.zero))
            {
                LightmappingTool.noUVs.Add(path);
                LightmappingTool.noUVs.Add(temp.transform);
            }

            LightmappingTool.usingFirstUV.Add(temp.transform);
            Vector2[] uvs = temp.sharedMesh.uv;
            bool normalized = true;
            for (int j = 0; j < uvs.Length && normalized; j++)
            {
                if (uvs[j].x > 1 || uvs[j].x < 0 || uvs[j].y < 0 || uvs[j].y > 1)
                {
                    LightmappingTool.notNormalized.Add(path);
                    LightmappingTool.notNormalized.Add(temp.transform);
                    normalized = false;
                }
            }
            if (SystemInfo.supportsRenderTextures)
            {
                //Overlapping test
                Shader finded = Shader.Find("ExternalLightmappingTool/uvDebug");
                if (finded == null)
                {
                    Debug.LogError("Cannot find uvDebug shader within LightmappingTools folder!");
                    return;
                }

                int res = 128;



                RenderTexture rt = RenderTexture.GetTemporary(res, res, 0);
                RenderTexture.active = rt;


                if (Camera.current == null)
                {
                    Camera.SetupCurrent(Camera.main);
                }

                RenderTexture oldRT = RenderTexture.active;
                rt = RenderTexture.GetTemporary(res, res, 0);
                RenderTexture.active = rt;

                GL.Clear(true, true, Color.black);

                Material mat = new Material(finded);

                Mesh mesh = temp.sharedMesh;
                mat.SetPass(0);
                GL.PushMatrix();
                GL.LoadOrtho();
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                GL.PopMatrix();
                UnityEngine.Object.DestroyImmediate(mat);

                Texture2D texture = new Texture2D(res, res, TextureFormat.ARGB32, false);
                texture.ReadPixels(new Rect(0, 0, res, res), 0, 0);
                texture.Apply();
                RenderTexture.active = oldRT;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(rt);
                //byte[] bytes = texture.EncodeToPNG();
                //File.WriteAllBytes(Application.dataPath + "/../test.png", bytes);
                Color[] tmp = texture.GetPixels();

                int i = 0;
                int incorrect = 0;
                for (; i < tmp.Length; i++)
                {
                    if (tmp[i].r > 0.6f)
                    {
                        incorrect++;
                        texture.SetPixel(i % res, i / res, Color.red);
                    }
                }


                if (incorrect * 300 > res * res)
                {
                    texture.Apply();
                    LightmappingTool.overlapping.Add(path);
                    LightmappingTool.overlapping.Add(texture);
                    LightmappingTool.overlapping.Add(temp.transform);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            }
            //UnityEngine.Object.DestroyImmediate(texture);
        }
        else
        {
            LightmappingTool.noUVs.Add(path);
            LightmappingTool.noUVs.Add(temp.transform);
        }
    }


    static public void NormalizeUV2(MeshFilter temp)
    {
        Rect currentBounding = CheckUV2OffsetAndSize(temp);

        Vector2[] newuv2 = temp.sharedMesh.uv2;
        
        float k = Math.Max(currentBounding.width,currentBounding.height);    
        for(int i=0; i<newuv2.Length;i++)
        {
            newuv2[i].x = (newuv2[i].x - currentBounding.xMin) / k;
            newuv2[i].y = (newuv2[i].y - currentBounding.yMin) / k;
        }
        temp.sharedMesh.uv2 = newuv2;
    }


    static public void NormalizeUV(MeshFilter temp)
    {
        Rect currentBounding = CheckUVOffsetAndSize(temp);

        Vector2[] newuv = temp.sharedMesh.uv;

        float k = Math.Max(currentBounding.width, currentBounding.height);
        for (int i = 0; i < newuv.Length; i++)
        {
            newuv[i].x = (newuv[i].x - currentBounding.xMin) / k;
            newuv[i].y = (newuv[i].y - currentBounding.yMin) / k;
        }
        temp.sharedMesh.uv = newuv;
    }

    static Rect CheckUV2OffsetAndSize(MeshFilter temp)
    {
        float left = 1;
        float right = 0;
        float up = 0;
        float down = 1;

        foreach (Vector2 a in temp.sharedMesh.uv2)
        {
            if (a.x < left) { left = a.x; }
            if (a.x > right) { right = a.x; }
            if (a.y < down) { down = a.y; }
            if (a.y > up) { up = a.y; }
        }
        return new Rect(left, down, Math.Abs(right - left), Math.Abs(up - down));
    }

    static Rect CheckUVOffsetAndSize(MeshFilter temp)
    {
        float left = 1;
        float right = 0;
        float up = 0;
        float down = 1;

        foreach (Vector2 a in temp.sharedMesh.uv)
        {
            if (a.x < left) { left = a.x; }
            if (a.x > right) { right = a.x; }
            if (a.y < down) { down = a.y; }
            if (a.y > up) { up = a.y; }
        }
        return new Rect(left, down, Math.Abs(right - left), Math.Abs(up - down));
    }

    static Rect[] PackObjects(MeshFilter[] mf, ArrayList surfaces, float totalArea, float percent, int lightmapSize)
    {
        if (percent < 0.6f)
        {
            return null;
        }
        Texture2D[] textures = new Texture2D[surfaces.Count];
        for (int j = 0; j < surfaces.Count; j++)
        {
            double spotSize = (double)((((float)(surfaces[j])) / totalArea)*(lightmapSize * lightmapSize) );
            double size = Math.Sqrt(spotSize);

            if (((Math.Floor(size * percent)) < 4) ||  (Math.Floor(size * percent) < 4))
            {
                Debug.LogError(mf[j].name + ": Packing failed! The size of the texture is below 4 pixels, try rearranging, or changing lightmap resolution.");
                EditorUtility.ClearProgressBar();
				return null;
            }
            textures[j] = new Texture2D((int) Math.Ceiling(size*percent),(int) Math.Ceiling(size*percent));
        }
        Texture2D packedTexture = new Texture2D(lightmapSize, lightmapSize);
        Rect[] toReturn  = packedTexture.PackTextures(textures,LightmappingTool.padding,lightmapSize);
        UnityEngine.Object.DestroyImmediate(packedTexture);
        GC.Collect();

        if (((int)(toReturn[0].width * lightmapSize)) != ((int)textures[0].width))
        {
            for (int i = 0; i < textures.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(textures[i]);
                textures[i] = null;
            }
            textures = null;
            GC.Collect();
            return PackObjects(mf,surfaces, totalArea, percent - 0.05f, lightmapSize);
        }
        else
        {
            for (int i = 0; i < textures.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(textures[i]);
                textures[i] = null;
            }
            textures = null;
            GC.Collect();
            Debug.Log("Packing efficiency of lightmap " + whichLightmap + " is : " + percent*100 + "%");
            whichLightmap++;
            return toReturn;
        }

    }


}