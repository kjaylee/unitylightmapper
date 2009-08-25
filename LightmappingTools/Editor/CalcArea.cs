using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class CalcArea
{
    public static Rect[][] CalculateArea(MeshFilter[] mf, int resolution)
    {
        ArrayList surfaces = new ArrayList();
        MeshFilter temp;
        float totalArea = 0;
        Rect[] offsetRect=new Rect[mf.Length];
        float areaSize;
        for (int i = 0; i < mf.Length; i++)
        {
            temp = (MeshFilter)mf[i];
            float area = 0;

            if (CheckIfNormalized(temp))
            {
                //Calculate the mesh area

                int count = temp.sharedMesh.triangles.Length / 3;
                for (int j = 0; j < count; j++)
                {
                    area += Vector3.Cross(temp.sharedMesh.vertices[temp.sharedMesh.triangles[j * 3]] - temp.sharedMesh.vertices[temp.sharedMesh.triangles[j * 3 + 1]], temp.sharedMesh.vertices[temp.sharedMesh.triangles[j * 3]] - temp.sharedMesh.vertices[temp.sharedMesh.triangles[j * 3 + 2]]).magnitude;
                }
                //Check the offset and size
                Rect result = CheckUVOffsetAndSize(temp);
                offsetRect[i] = result;
                areaSize = Math.Abs(area * temp.transform.lossyScale.x * temp.transform.lossyScale.y * temp.transform.lossyScale.z);

                ArrayList tmp = new ArrayList();
                tmp.Add(temp);
                tmp.Add(areaSize);
                tmp.Add(result);
                surfaces.Add(tmp);

                totalArea += Math.Abs(areaSize);
            }
        }
        return new Rect[2][]{KeepProportions(mf,surfaces, totalArea, 1.00f, resolution),offsetRect};
    }

    public static bool CheckIfNormalized(MeshFilter temp)
    {
        bool valid = true;
        if (temp.sharedMesh.uv2.Length != temp.sharedMesh.vertexCount)
        {
            valid = false;
            Debug.LogWarning(temp.name +  ": A valid UV2 map should have as many UV verticles as verticles (" + temp.sharedMesh.uv2.Length + " != " +temp.sharedMesh.vertexCount); 
        }

        Vector2[] uvs = temp.sharedMesh.uv2;
        for (int i = 0; i < uvs.Length && valid; i++)
        {
            if (uvs[i].x > 1 || uvs[i].x < 0 || uvs[i].y < 0 || uvs[i].y > 1)
            {
                valid = false;
            }
        }
        return valid;
    }
    static Rect CheckUVOffsetAndSize(MeshFilter temp)
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

    static Rect[] KeepProportions(MeshFilter[] mf, ArrayList surfaces, float totalArea, float percent, int lightmapSize)
    {
        if (percent < 0.6f)
        {
            return null;
        }
        Texture2D[] textures = new Texture2D[surfaces.Count];
        ArrayList rects = new ArrayList();
        for (int j = 0; j < surfaces.Count; j++)
        {
            //Debug.Log("Area surface " + ((float)((ArrayList)surfaces[j])[1]));
            //Debug.Log("Total area surface " + totalArea);
            float spotSize = lightmapSize * lightmapSize * (((float)((ArrayList)surfaces[j])[1]) / totalArea);
            double proportion = Math.Sqrt(spotSize/((((Rect)((ArrayList)surfaces[j])[2]).height)*(((Rect)((ArrayList)surfaces[j])[2]).width)));
            
            //PODEJRZANE!
            int xsize = (int)Math.Round(spotSize / (((Rect)((ArrayList)surfaces[j])[2]).height * proportion));
            int ysize = (int)Math.Round(spotSize / (((Rect)((ArrayList)surfaces[j])[2]).width * proportion));
            rects.Add(new Area(xsize,ysize, j));
            /*
            if (xsize > lightmapSize)
            {
                xsize = lightmapSize;
                proportion = (double) 1.0*lightmapSize / xsize;
                ysize = (int) Math.Round(proportion * ysize);
            }
            else if (ysize > lightmapSize)
            {
                ysize = lightmapSize;
                proportion = (double)1.0 * lightmapSize / ysize;
                xsize =(int) Math.Round(proportion * xsize);
            }
             */


            if (((Math.Floor(xsize * percent)) < 4) ||  (Math.Floor(ysize * percent) < 4))
            {
                //I know it sucks to forward a whole array of objects as a parameter just because to display the name of the object which started an exception. Hopefuly I'll fix that soon, when I'll reorganise thinks a little bit
                Debug.LogError(mf[j].name + ": Packing failed! The size of the texture is below 4 pixels, try rearranging, or changing lightmap resolution.");
                EditorUtility.ClearProgressBar();
            }
            textures[j] = new Texture2D((int) Math.Round(xsize*percent),(int) Math.Round(ysize*percent));
        }
        Texture2D packedTexture = new Texture2D(lightmapSize, lightmapSize);
        Rect[] toReturn  = packedTexture.PackTextures(textures,LightmapAdvanced.padding,lightmapSize);
        UnityEngine.Object.DestroyImmediate(packedTexture);
        GC.Collect();
        int kkkk=0;
        foreach (Rect rc in toReturn)
        {
            Debug.Log(rc.height*lightmapSize + " vs " + Convert.ToDouble(Convert.ToString((double)rc.height))*lightmapSize);
            //Debug.Log(((rc.width) * lightmapSize) + ":" + (rc.height * lightmapSize) + " != " + textures[kkkk].width + ":" + textures[kkkk].height);
                kkkk++;
        }

        //Debug.Log((int)(toReturn[0].width * lightmapSize) + " to " + ((int)textures[0].width));
        if (((int)(toReturn[0].width * lightmapSize)) != ((int)textures[0].width))
        {
            for (int i = 0; i < textures.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(textures[i]);
                textures[i] = null;
            }
            textures = null;
            GC.Collect();
            return KeepProportions(mf,surfaces, totalArea, percent - 0.05f, lightmapSize);
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
            Debug.Log("Packing efficiency: " + percent*100 + "%");
            return toReturn;
        }

    }


}