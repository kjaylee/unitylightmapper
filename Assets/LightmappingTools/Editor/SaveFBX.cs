using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/*
    Coded for Unity Summer of Code 2009 by Michal Mandrysz
    Feel free to modify for your own needs
    http://masteranza.wordpress.com/unity/lightmapping/
    http://unity3d.com
*/

class SaveFBX
{
    public static ArrayList offsetsArray;
    public bool ExportFBX(ref ArrayList bigArray, ref ArrayList materialsArray, ref ArrayList uniqueMaterialsArray, ref ArrayList totalUniqueMaterials, ref Light[] lights, int textures, int totalCount)
    {
        CalcArea.whichLightmap = 1;
        offsetsArray = new ArrayList();

        //TextWriter stringWriter = new StringWriter();
        string sciezka;
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
        	sciezka = "MaxFiles\\" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".fbx";
        }
        else{
        	sciezka = "MaxFiles/" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".fbx";
        }
        using (TextWriter sw = new StreamWriter(sciezka))
        {
            EditorUtility.DisplayProgressBar("Exporting FBX", "Setting headers...", 0.1f);
            StringBuilder sb = new StringBuilder();
			//StringBuilder sb2 = new StringBuilder();
			//sb.Append(sb2);
            sb.Append(FBXHeader());
            //FBX Definitions
            sb.Append("; Object definitions\r\n");
            sb.Append(";------------------------------------------------------------------\r\n");
            sb.Append("Definitions:  {\r\n");
            sb.Append("    Version: 100\r\n");
            sb.Append("    Count: ");

            sb.Append(totalCount+1);  //One is for Global Settings
            //First, primary type: Model
            sb.Append("\r\n    ObjectType: \"Model\" {\r\n");
            sb.Append("        Count: ");
            sb.Append(bigArray.Count + lights.Length);
            sb.Append("\r\n    }\r\n");

            //Secondly, type: Material
            sb.Append("\r\n    ObjectType: \"Material\" {\r\n");
            sb.Append("        Count: ");
            sb.Append(totalUniqueMaterials.Count);
            sb.Append("\r\n    }\r\n");

            //Textures
            sb.Append("\r\n    ObjectType: \"Texture\" {\r\n");
            sb.Append("        Count: ");
            sb.Append(textures);
            sb.Append("\r\n    }\r\n");

            sb.Append("\r\n    ObjectType: \"GlobalSettings\" {\r\n");
            sb.Append("        Count: 1\r\n");
            sb.Append("    }\r\n");
            //Video
            /*
            sb.Append("\r\n    ObjectType: \"Video\" {\r\n");
            sb.Append("        Count: ");
            sb.Append(textures);
            sb.Append("\r\n    }\r\n");
            */
            
            sb.Append("}\r\n\r\n");
            sb.Append("; Object properties\r\n");
            sb.Append(";------------------------------------------------------------------\r\n");
            sb.Append("Objects:  {\r\n");

            int model = 0;

            foreach (MeshFilter[] mf in bigArray)
            {
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing model data" + (model+1), 0.1f);
                sb.Append("    Model: \"Model::ImportedObject" + Convert.ToString(model+1) + "\", \"Mesh\" {\r\n");
                sb.Append("        Version: 232\r\n");
                sb.Append("        Properties60:  {\r\n");
                sb.Append("            Property: \"GeometricScaling\", \"Vector3D\", \"\",1,1,1\r\n");
                sb.Append("            Property:\"Show\",\"bool\",\"\",1\r\n");
                sb.Append("            Property:\"NegativePercentShapeSupport\",\"bool\",\"\",1\r\n");
                sb.Append("            Property:\"DefaultAttributeIndex\",\"int\",\"\",0\r\n");
                sb.Append("            Property:\"Visibility\",\"Visibility\",\"A+\",1\r\n");
                sb.Append("            Property:\"Color\",\"ColorRGB\",\"N\",0.23921568627451,0.52156862745098,0.0235294117647059\r\n");
                sb.Append("            Property: \"BBoxMin\",\"Vector3D\",\"N\",0,0,0\r\n");
                sb.Append("            Property: \"BBoxMax\",\"Vector3D\",\"N\",0,0,0\r\n");
                sb.Append("        }\r\n");
                sb.Append("        MultiLayer: 1\r\n");
                sb.Append("        MultiTake: 1\r\n");
                sb.Append("        Shading: T\r\n");
                sb.Append("        Culling: \"CullingOff\"\r\n");
                sb.Append("        Vertices: ");
                sw.Write(sb.ToString()); //new
                sb.Length = 0; //new


                int[] vertexoffsets = new int[mf.Length + 1];
                int vertexoffset = 0;
                vertexoffsets[0] = 0;
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing vertices's for lightmap " + (model+1), 0.2f);

//############## EXPORTING VERTICLES
                Vector3 vrt;
                Vector3[] vecArray;
                ArrayList transformedVerticles = new ArrayList();
                for (int j = 0; j < mf.Length; j++)
                {
                    int i = 0;

                    vecArray = new Vector3[mf[j].sharedMesh.vertexCount];
                    foreach (Vector3 vert in mf[j].sharedMesh.vertices)
                    {
                        vrt = mf[j].transform.TransformPoint(vert);
                        vecArray[i] = vrt;
                        sw.Write(Convert.ToString((double)-vrt.x).Replace(',', '.'));
                        sw.Write(',');
                        sw.Write(Convert.ToString((double)vrt.y).Replace(',', '.'));

                        sw.Write(',');
                        sw.Write(Convert.ToString((double)vrt.z).Replace(',', '.'));
                        sw.Write(',');
                        if (i % 3 == 0)
                        {
                            sw.Write("\r\n        ");
                        }
                        i++;
                    }
                    transformedVerticles.Add(vecArray);
                    vertexoffset += ((MeshFilter)mf[j]).sharedMesh.vertexCount;
                    vertexoffsets[j + 1] = vertexoffset;
                }
                sw.Write("\r\n        PolygonVertexIndex: ");

//############## EXPORTING TRIANGLES
                int k = 0;
                int[] triangles;                
                foreach (MeshFilter mesh in mf)
                {
                    triangles = mesh.sharedMesh.triangles;
                    for (int i = 0; i < triangles.Length; i+=3)
                    {
                        //Mental Ray + VRay
                        sw.Write((triangles[i] + vertexoffsets[k]));
                        sw.Write(',');
                        sw.Write((triangles[i+2] + vertexoffsets[k]));
                        sw.Write(',');
                        sw.Write((-(triangles[i+1] + vertexoffsets[k] + 1)));
                        sw.Write(',');
                        
                        //Scanline
                        //sb.Append((triangles[i] + vertexoffsets[k])).Append(',').Append((triangles[i + 2] + vertexoffsets[k])).Append(',').Append((-(triangles[i + 1] + vertexoffsets[k] + 1))).Append(',');
                        //Experiment
                        //sb.Append((triangles[i + 2] + vertexoffsets[k])).Append(',').Append((triangles[i] + vertexoffsets[k])).Append(',').Append((-(triangles[i+1] + vertexoffsets[k] + 1))).Append(',');
                        if (i % 9 == 0) sw.Write("\r\n        ");

                    }
                    k++;
                }
                sw.Write("\r\n        GeometryVersion: 124\r\n");
                sw.Write("        LayerElementNormal: 0 {\r\n");
                sw.Write("            Version: 101\r\n");
                sw.Write("            Name: \"\"\r\n");
                sw.Write("            MappingInformationType: \"ByVertice\"\r\n");
                sw.Write("            ReferenceInformationType: \"Direct\"\r\n");

//############## NORMALS EXPORTING
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing normals for lightmap " + (model+1), 0.5f);
                sw.Write("            Normals: ");
                Matrix4x4 transformMatrix;
                Vector3[] normals;

                foreach (MeshFilter mesh in mf)
                {
                    normals = mesh.sharedMesh.normals;
                    transformMatrix = mesh.transform.localToWorldMatrix.inverse.transpose;
                    for (int i = 0; i < normals.Length; i++)
                    {
                        vrt = transformMatrix.MultiplyVector(normals[i]);

                        sw.Write(Convert.ToString((double)-vrt.x).Replace(',', '.'));
                        sw.Write(',');
                        sw.Write(Convert.ToString((double)vrt.y).Replace(',', '.'));
                        sw.Write(',');
                        sw.Write(Convert.ToString((double)vrt.z).Replace(',', '.'));
                        sw.Write(',');
                        if ((i & 3) == 0) sw.Write("\r\n            ");
                    }
                }
                sw.Write("\r\n        }\r\n");


//############## UV Exporting
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing UV's for lightmap " + (model+1), 0.7f);
                sw.Write("        LayerElementUV: 0 {\r\n");
                sw.Write("            Version: 101\r\n");
                sw.Write("            Name: \"UVChannel_1\"\r\n");
                sw.Write("            MappingInformationType: \"ByVertice\"\r\n");
                sw.Write("            ReferenceInformationType: \"Direct\"\r\n");

                sw.Write("            UV: ");
                foreach (MeshFilter mesh in mf)
                {
                    if (mesh.sharedMesh.uv.Length < mesh.sharedMesh.vertexCount)
                    {
                        for (int j = 0; j < mesh.sharedMesh.vertexCount; j++)
                        {
                            sb.Append("0.0!0.0!");
                            if (j % 4 == 0)
                            {
                                sb.Append("\r\n            ");
                            }
                        }
                    }
                    else
                    {
                        int i = 0;
                        foreach (Vector2 v in mesh.sharedMesh.uv)
                        {
                            sb.Append((double)v.x).Append('!').Append((double)v.y).Append('!');
                            if ((i & 7) == 0)
                            {
                                sb.Append("\r\n            ");
                            }
                            i++;
                        }
                    }
                    
                }
				sb.Replace(',','.');
				sb.Replace('!',',');
				//sb.Append(sb2);
                sw.Write(sb.ToString());
				sb.Length = 0;
                sw.Write("\r\n        }\r\n");



//############## UV2 Exporting
                sw.Write("        LayerElementUV: 1 {\r\n");
                sw.Write("            Version: 101\r\n");
                sw.Write("            Name: \"UVChannel_2\"\r\n");
                sw.Write("            MappingInformationType: \"ByVertice\"\r\n");
                sw.Write("            ReferenceInformationType: \"Direct\"\r\n");

                sw.Write("            UV: ");
                foreach (MeshFilter mesh in mf)
                {
                    if (mesh.sharedMesh.uv2.Length < mesh.sharedMesh.vertexCount)
                    {
                        Debug.Log(mesh.name + " has less uv2 verticles then mesh verticles");
                        for (int j = 0; j < mesh.sharedMesh.vertexCount; j++)
                        {
                            sb.Append("0.0!0.0!");
                            //if (j % 4 == 0)
                            //{
                                sb.Append("\r\n            ");
                            //}
                        }
                    }
                    else
                    {
                        int i = 0;
                        foreach (Vector2 v in mesh.sharedMesh.uv2)
                        {
                            sb.Append((double)v.x).Append('!').Append((double)v.y).Append('!');
                            //if ((i & 2) == 0)
                            //{
                                sb.Append("\r\n            ");
                            //}
                            i++;
                        }
                        sb.Append("\r\n            \r\n            ");
                    }
                }
				sb.Replace(',','.');
				sb.Replace('!',',');
				//sb.Append(sb2);
                sw.Write(sb.ToString());
				sb.Length = 0;
                sw.Write("\r\n        }\r\n");

//############## UV3 Exporting
                sw.Write("        LayerElementUV: 2 {\r\n");
                sw.Write("            Version: 101\r\n");
                sw.Write("            Name: \"UVChannel_3\"\r\n");
                sw.Write("            MappingInformationType: \"ByVertice\"\r\n");
                sw.Write("            ReferenceInformationType: \"Direct\"\r\n");
                sw.Write("            UV: ");

//############## Preparing UV3 coordinates
                Rect[] uvs = CalcArea.CalculateArea(mf, Convert.ToInt32(128 * Math.Pow(2,(Convert.ToInt32((int) LightmappingTool.res[model])))),transformedVerticles);
                offsetsArray.Add(uvs);
                if (uvs == null)
                {
                    return false;
                }

//############## UV3 Exporting
                int kk = 0;
                foreach (MeshFilter mesh in mf)
                {
                    Vector2[] toUse;
                    if (mesh.sharedMesh.uv2.Length == mesh.sharedMesh.vertexCount)
                    {
                        toUse = mesh.sharedMesh.uv2;
                    }
                    else
                    {
                        //Debug.Log(mesh.name + " rendering using uv1");
                        toUse = mesh.sharedMesh.uv;
                    }
                    int i = 0;
                        //Debug.Log(Math.Floor((double)((uvs[kk].x + ((mesh.sharedMesh.uv2[0].x - offsets[kk].xMin) * uvs[kk].width / (offsets[kk].xMax - offsets[kk].xMin))) * 2048.0)));
                        //Debug.Log((uvs[kk].y + ((mesh.sharedMesh.uv2[0].y - offsets[kk].yMin) * uvs[kk].height / (offsets[kk].yMax - offsets[kk].yMin))) * 2048.0);
                    foreach (Vector2 v in toUse)
                    {
                        try
                        {
                            sb.Append(((double)uvs[kk].x + v.x*uvs[kk].width)).Append('!').Append(((double)uvs[kk].y + v.y*uvs[kk].height)).Append('!');
                            //sb2.Append(Math.Round((double)((uvs[kk].x + ((v.x - offsets[kk].xMin) * uvs[kk].width / (offsets[kk].xMax - offsets[kk].xMin)))))).Append('!').Append(Math.Round((double)(uvs[kk].y + ((v.y - offsets[kk].yMin) * uvs[kk].height / (offsets[kk].yMax - offsets[kk].yMin))))).Append('!');
                        }
                        catch
                        {
                            sb.Append("0.0!0.0!");
                            Debug.Log("Non-Valid UV on " + mesh.name);
                        }
                        if ((i & 3) == 0) sb.Append("\r\n            ");
                        i++;
                    }

                    kk++;
                    sb.Append("\r\n            ");
                }
				sb.Replace(',','.');
				sb.Replace('!',',');
				//sb.Append(sb2);
                sw.Write(sb.ToString());
				sb.Length = 0;
                sw.Write("\r\n        }\r\n");

                //GC should clean that
                uvs = null;

//############## MATERIAL DISTRIBUTION PART
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing materials for lightmap " + (model +1), 0.8f);
                sw.Write("        LayerElementMaterial: 0 {\r\n");
                sw.Write("            Version: 101\r\n");
                sw.Write("            Name: \"\"\r\n");
                sw.Write("            MappingInformationType: \"ByPolygon\"\r\n");
                sw.Write("            ReferenceInformationType: \"IndexToDirect\"\r\n");
                sw.Write("            Materials: ");

                int countner = 0;
                int value = 0;
                for (int z = 0; z < mf.Length; z++)
                {
					int ln;
					ln = mf[z].sharedMesh.subMeshCount;
                    for (int t = 0; t < ln; t++)
                    {
                        if ((((Material[][])materialsArray[model])[z][t]) == null)
                        {
                            value = -1;
                        }
                        else if (((ArrayList)uniqueMaterialsArray[model]).Contains(((Material[][])materialsArray[model])[z][t]))
                        {
                            value = ((ArrayList)uniqueMaterialsArray[model]).IndexOf(((Material[][])materialsArray[model])[z][t]);
                        }
                        else
                        {
                            value = countner;
                            countner++;
                        }
						string str = Convert.ToString(value) + ",";
						int ln2;
						ln2 = mf[z].sharedMesh.GetTriangles(t).Length;
                        for (int i = 0; i < ln2; i+=3)
                        {
                            //if (i % 3 == 2)
                            //{
                                sw.Write(str);

                                if ((i & 15) == 0) sw.Write("\r\n            ");
                            //}
                        }
                    }
                }


                sw.Write("\r\n        }\r\n");

// ############# Texture part

                sw.Write("        LayerElementTexture: 0 {\r\n");
                sw.Write("            Version: 101\r\n");
                sw.Write("            Name: \"\"\r\n");
                sw.Write("            MappingInformationType: \"ByPolygon\"\r\n");
                sw.Write("            ReferenceInformationType: \"IndexToDirect\"\r\n");
                sw.Write("            BlendMode: \"Translucent\"\r\n");
                sw.Write("            TextureAlpha: 1\r\n");
                sw.Write("            TextureId: ");

                int mapOffset = 0;
                countner = 0;
                value = 0;
                ArrayList nonMap = new ArrayList();
                ArrayList inserted = new ArrayList();

                for (int z = 0; z < mf.Length; z++)
                {
					int ln = mf[z].sharedMesh.subMeshCount;
                    for (int t = 0; t < ln; t++)
                    {

                        if (((Material[][])materialsArray[model])[z][t].HasProperty("_MainTex") && (((Material[][])materialsArray[model])[z][t].GetTexture("_MainTex")))
                        {
                            if (inserted.Contains(((Material[][])materialsArray[model])[z][t]))
                            {
                                value = inserted.IndexOf(((Material[][])materialsArray[model])[z][t]);
                            }
                            else if (((ArrayList)uniqueMaterialsArray[model]).Contains(((Material[][])materialsArray[model])[z][t]))
                            {
                                value = ((ArrayList)uniqueMaterialsArray[model]).IndexOf(((Material[][])materialsArray[model])[z][t]) - nonMap.Count;
                                inserted.Add(((Material[][])materialsArray[model])[z][t]);
                            }
                            if (value >= mapOffset) mapOffset = value + 1;
                        }
                        else
                        {
                            value = -1;
                            if (!nonMap.Contains(((Material[][])materialsArray[model])[z][t]))
                            {
                                nonMap.Add(((Material[][])materialsArray[model])[z][t]);
                            }
                        }
						int ln2 = mf[z].sharedMesh.GetTriangles(t).Length;
						string str = value + ",";
                        for (int i = 0; i < ln2; i+=3)
                        {
                            //if (i % 3 == 2)
                            //{
                                sw.Write(str);
                            //}
                        }
                        sw.Write("\r\n            ");
                    }
                }
                sw.Write("\r\n        }\r\n");


                countner = 0;
                value = 0;
                nonMap = new ArrayList();
                inserted = new ArrayList();

                sw.Write("        LayerElementBumpTextures: 0 {\r\n");
                sw.Write("            Version: 101\r\n");
                sw.Write("            Name: \"\"\r\n");
                sw.Write("            MappingInformationType: \"ByPolygon\"\r\n");
                sw.Write("            ReferenceInformationType: \"IndexToDirect\"\r\n");
                sw.Write("            BlendMode: \"Translucent\"\r\n");
                sw.Write("            TextureAlpha: 1\r\n");
                sw.Write("            TextureId: ");

                for (int z = 0; z < mf.Length; z++)
                {
                    for (int t = 0; t < ((MeshFilter)mf[z]).sharedMesh.subMeshCount; t++)
                    {
                        if (((Material[][])materialsArray[model])[z][t].HasProperty("_BumpMap") && (((Material[][])materialsArray[model])[z][t].GetTexture("_BumpMap")))
                        {
                            if (inserted.Contains(((Material[][])materialsArray[model])[z][t]))
                            {
                                value = mapOffset + inserted.IndexOf(((Material[][])materialsArray[model])[z][t]);
                            }
                            else if (((ArrayList)uniqueMaterialsArray[model]).Contains(((Material[][])materialsArray[model])[z][t]))
                            {
                                value = mapOffset + ((ArrayList)uniqueMaterialsArray[model]).IndexOf(((Material[][])materialsArray[model])[z][t]) - nonMap.Count;
                                inserted.Add(((Material[][])materialsArray[model])[z][t]);
                            }
                        }
                        else
                        {
                            value = -1;
                            if (!nonMap.Contains(((Material[][])materialsArray[model])[z][t]))
                            {
                                nonMap.Add(((Material[][])materialsArray[model])[z][t]);
                            }
                        }
                        for (int i = 0; i < ((MeshFilter)mf[z]).sharedMesh.GetTriangles(t).Length; i++)
                        {
                            if (i % 3 == 2)
                            {
                                sw.Write(value + ",");

                                if (i % 20 == 0)
                                {
                                    //sw.Write("\r\n            ");
                                }
                            }
                        }
                        sw.Write("\r\n            ");
                    }
                }

                sw.Write("\r\n        }\r\n");

                //GC should clean this:
                nonMap = null;
                inserted = null;

                sw.Write("        Layer: 0 {\r\n");
                sw.Write("            Version: 100\r\n");
                sw.Write("            LayerElement:  {\r\n");
                sw.Write("                Type: \"LayerElementNormal\"\r\n");
                sw.Write("                TypedIndex: 0\r\n");
                sw.Write("            }\r\n");
                sw.Write("            LayerElement:  {\r\n");
                sw.Write("                Type: \"LayerElementMaterial\"\r\n");
                sw.Write("                TypedIndex: 0\r\n");
                sw.Write("            }\r\n");
                sw.Write("            LayerElement:  {\r\n");
                sw.Write("                Type: \"LayerElementTexture\"\r\n");
                sw.Write("                TypedIndex: 0\r\n");
                sw.Write("            }\r\n");
                sw.Write("            LayerElement:  {\r\n");
                sw.Write("                Type: \"LayerElementBumpTextures\"\r\n");
                sw.Write("                TypedIndex: 0\r\n");
                sw.Write("            }\r\n");
                sw.Write("            LayerElement:  {\r\n");
                sw.Write("                Type: \"LayerElementUV\"\r\n");
                sw.Write("                TypedIndex: 0\r\n");
                sw.Write("            }\r\n");
                sw.Write("        }\r\n");
                sw.Write("        Layer: 1 {\r\n");
                sw.Write("            Version: 100\r\n");
                sw.Write("            LayerElement:  {\r\n");
                sw.Write("                Type: \"LayerElementUV\"\r\n");
                sw.Write("                TypedIndex: 1\r\n");
                sw.Write("            }\r\n");
                sw.Write("        }\r\n");
                sw.Write("        Layer: 2 {\r\n");
                sw.Write("            Version: 100\r\n");
                sw.Write("            LayerElement:  {\r\n");
                sw.Write("                Type: \"LayerElementUV\"\r\n");
                sw.Write("                TypedIndex: 2\r\n");
                sw.Write("            }\r\n");
                sw.Write("        }\r\n");
                sw.Write("        NodeAttributeName: \"Geometry::ImportedObject" + Convert.ToString(model+1) + "\"\r\n");
                sw.Write("    }\r\n");
                model++;
            }

// ######### LIGHTS Exporting
            int enumType;
            for (int i = 0; i < lights.Length; i++ )
            {
                switch (lights[i].light.type)
                {
                    case LightType.Point:
                        enumType = 0;
                        break;
                    case LightType.Directional:
                        enumType = 1;
                        break;
                    case LightType.Spot:
                        enumType = 2;
                        break;
                    default:
                        enumType = 0;
                        break;
                }

                sw.Write("Model: \"Model::ImportedLight" + (i+1) + "\", \"Light\" {\r\n");
                sw.Write("        Version: 232\r\n");
                sw.Write("        Properties60:  {\r\n");
                
                sw.Write("            Property: \"PreRotation\", \"Vector3D\", \"\",-90,0,0\r\n");
                sw.Write("            Property: \"PostRotation\", \"Vector3D\", \"\",0,0," + Convert.ToString(-lights[i].transform.eulerAngles.z).Replace(",", ".") + "\r\n");
                sw.Write("            Property: \"RotationActive\", \"bool\", \"\",1\r\n");
                sw.Write("            Property: \"Lcl Translation\", \"Lcl Translation\", \"A+\"," + Convert.ToString(-lights[i].transform.position.x).Replace(",", ".") + "," + Convert.ToString(lights[i].transform.position.y).Replace(",", ".") + "," + Convert.ToString(lights[i].transform.position.z).Replace(",", ".") + "\r\n");
                sw.Write("            Property: \"Lcl Rotation\", \"Lcl Rotation\", \"A+\"," + Convert.ToString(lights[i].transform.eulerAngles.x).Replace(",", ".") + ",0," + Convert.ToString(-lights[i].transform.eulerAngles.y).Replace(",", ".") + " \r\n");
                sw.Write("            Property: \"Lcl Scaling\", \"Lcl Scaling\", \"A+\",1,1,1\r\n");
                sw.Write("            Property: \"Visibility\", \"Visibility\", \"A+\",1\r\n");
                sw.Write("            Property: \"Color\", \"Color\", \"A+N\"," + Convert.ToString(lights[i].color.r).Replace(",", ".") + "," + Convert.ToString(lights[i].color.g).Replace(",", ".") + "," + Convert.ToString(lights[i].color.b).Replace(",", ".") + "\r\n");
                sw.Write("            Property: \"LightType\", \"enum\", \"N\"," + Convert.ToString(enumType) + "\r\n");
                sw.Write("            Property: \"CastLightOnObject\", \"bool\", \"N\",1\r\n");
                sw.Write("            Property: \"DrawVolumetricLight\", \"bool\", \"N\",1\r\n");
                sw.Write("            Property: \"DrawGroundProjection\", \"bool\", \"N\",0\r\n");
                sw.Write("            Property: \"DrawFrontFacingVolumetricLight\", \"bool\", \"N\",0\r\n");
                sw.Write("            Property: \"Intensity\", \"Number\", \"A+N\"," + Convert.ToString(lights[i].intensity * LightmappingTool.lightMultipler*100).Replace(",", ".") + "\r\n");
                sw.Write("            Property: \"HotSpot\", \"Number\", \"A+N\"," + ((enumType != 0) ? lights[i].spotAngle: 0) + "\r\n");
                sw.Write("            Property: \"Cone angle\", \"Number\", \"A+N\"," + Convert.ToString(lights[i].spotAngle).Replace(",", ".") + "\r\n");
                sw.Write("            Property: \"Fog\", \"Number\", \"A+N\",0\r\n");
                sw.Write("            Property: \"DecayType\", \"enum\", \"N\",1\r\n");
                sw.Write("            Property: \"DecayStart\", \"Number\", \"A+N\",40\r\n");
                sw.Write("            Property: \"FileName\", \"KString\", \"N\", \"\"\r\n");
                sw.Write("            Property: \"EnableNearAttenuation\", \"bool\", \"N\",0\r\n");
                sw.Write("            Property: \"NearAttenuationStart\", \"Number\", \"A+N\",0\r\n");
                sw.Write("            Property: \"NearAttenuationEnd\", \"Number\", \"A+N\",40\r\n");
                sw.Write("            Property: \"EnableFarAttenuation\", \"bool\", \"N\",0\r\n");
                sw.Write("            Property: \"FarAttenuationStart\", \"Number\", \"A+N\",80\r\n");
                sw.Write("            Property: \"FarAttenuationEnd\", \"Number\", \"A+N\",200\r\n");
                sw.Write("            Property: \"CastShadows\", \"bool\", \"N\",1\r\n");
                sw.Write("            Property: \"ShadowColor\", \"Color\", \"A+N\",0,0,0\r\n");
                
                sw.Write("            Property: \"3dsMax\", \"Compound\", \"N\"\r\n");
                sw.Write("            Property: \"3dsMax|ClassIDa\", \"int\", \"N\",4113\r\n");
                sw.Write("            Property: \"3dsMax|ClassIDb\", \"int\", \"N\",0\r\n");
                sw.Write("            Property: \"3dsMax|SuperClassID\", \"int\", \"N\",48\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0\", \"Compound\", \"N\"\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Color\", \"Color\", \"AN\",1,1,1\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Multiplier\", \"Float\", \"AN\", " + Convert.ToString(lights[i].intensity * LightmappingTool.lightMultipler*100).Replace(",", ".") + " \r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Contrast\", \"Float\", \"AN\",0\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Diffuse Soften\", \"Float\", \"AN\",0\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Attenuation Near Start\", \"Float\", \"AN\",0\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Attenuation Near End\", \"Float\", \"AN\",40\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Attenuation Far Start\", \"Float\", \"AN\",80\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Attenuation Far End\", \"Float\", \"AN\",200\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Decay Falloff\", \"Float\", \"AN\",40\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Shadow Color\", \"Color\", \"AN\",0,0,0\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|_Unnamed_Parameter_10\", \"int\", \"N\",0\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Atmosphere Opacity\", \"Float\", \"AN\",1\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Atmosphere Color Amount\", \"Float\", \"AN\",1\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|Shadow Density\", \"Float\", \"AN\",1\r\n");
                sw.Write("            Property: \"3dsMax|ParamBlock_0|_Unnamed_Parameter_14\", \"int\", \"N\",0\r\n");
                
                sw.Write("        }\r\n");
                sw.Write("        MultiLayer: 0\r\n");
                sw.Write("        MultiTake: 0\r\n");
                sw.Write("        Shading: T\r\n");
                sw.Write("        Culling: \"CullingOff\"\r\n");
                sw.Write("        TypeFlags: \"Light\"\r\n");
                sw.Write("        GeometryVersion: 124\r\n");
                sw.Write("        NodeAttributeName: \"NodeAttribute::ImportedLight" + (i+1) + "\"\r\n");
                sw.Write("    }\r\n");
            }


            int tex = 0;
            foreach (Material mat in totalUniqueMaterials)
            {
                sw.Write("    Material: \"Material::" + @mat.name + "\", \"\" {\r\n");
                sw.Write("        Version: 102\r\n");
                sw.Write("        ShadingModel: \"phong\"\r\n");
                sw.Write("        MultiLayer: 0\r\n");
                sw.Write("        Properties60:  {\r\n");
                sw.Write("            Property: \"ShadingModel\", \"KString\", \"\", \"phong\"\r\n");
                sw.Write("            Property: \"MultiLayer\", \"bool\", \"\",0\r\n");
                if (mat.HasProperty("_Emission"))
                {
                    sw.Write("            Property: \"EmissiveColor\", \"ColorRGB\"," + Convert.ToString(mat.GetColor("_Emission").r).Replace(",", ".") + "," + Convert.ToString(mat.GetColor("_Emission").g).Replace(",", ".") + "," + Convert.ToString(mat.GetColor("_Emission").b).Replace(",", ".") + "\r\n");
                    sw.Write("            Property: \"EmissiveFactor\", \"double\", \"\"," + Convert.ToString(mat.GetFloat("_Shininess")).Replace(",", ".") + "\r\n");
                }
                else
                {
                    sw.Write("            Property: \"EmissiveColor\", \"ColorRGB\", \"\",0,0,0\r\n");
                    sw.Write("            Property: \"EmissiveFactor\", \"double\", \"\",0\r\n");
                }
                sw.Write("            Property: \"AmbientColor\", \"ColorRGB\", \"\"," + Convert.ToString(mat.color.r).Replace(",", ".") + "," + Convert.ToString(mat.color.g).Replace(",", ".") + "," + Convert.ToString(mat.color.b).Replace(",", ".") + "\r\n");
                sw.Write("            Property: \"AmbientFactor\", \"double\", \"\",1\r\n");
                sw.Write("            Property: \"DiffuseColor\", \"ColorRGB\", \"\"," + Convert.ToString(mat.color.r).Replace(",", ".") + "," + Convert.ToString(mat.color.g).Replace(",", ".") + "," + Convert.ToString(mat.color.b).Replace(",", ".") + "\r\n");
                sw.Write("            Property: \"DiffuseFactor\", \"double\", \"\",1\r\n");
                sw.Write("            Property: \"Bump\", \"Vector3D\", \"\",0,0,0\r\n");
                sw.Write("            Property: \"NormalMap\", \"Vector3D\", \"\",0,0,0\r\n");
                sw.Write("            Property: \"BumpFactor\", \"double\", \"\",1\r\n");
                sw.Write("            Property: \"TransparentColor\", \"ColorRGB\", \"\",1,1,1\r\n");
                sw.Write("            Property: \"TransparencyFactor\", \"double\", \"\",0\r\n");
                sw.Write("            Property: \"DisplacementColor\", \"ColorRGB\", \"\",0,0,0\r\n");
                sw.Write("            Property: \"DisplacementFactor\", \"double\", \"\",1\r\n");
                if (mat.HasProperty("_SpecColor"))
                {
                    sw.Write("            Property: \"SpecularColor\", \"ColorRGB\", \"\"," + Convert.ToString(mat.GetColor("_SpecColor").r).Replace(",", ".") + "," + Convert.ToString(mat.GetColor("_SpecColor").g).Replace(",", ".") + "," + Convert.ToString(mat.GetColor("_SpecColor").b).Replace(",", ".") + "\r\n");
                    sw.Write("            Property: \"SpecularFactor\", \"double\", \"\"," + Convert.ToString(mat.GetFloat("_Shininess")).Replace(",", ".") + "\r\n");
                }
                else
                {
                    sw.Write("            Property: \"SpecularColor\", \"ColorRGB\", \"\",0.0,0.0,0.0\r\n");
                    sw.Write("            Property: \"SpecularFactor\", \"double\", \"\",0\r\n");
                }
                sw.Write("            Property: \"ShininessExponent\", \"double\", \"\",2.0000000206574\r\n");
                sw.Write("            Property: \"ReflectionColor\", \"ColorRGB\", \"\",0,0,0\r\n");
                sw.Write("            Property: \"ReflectionFactor\", \"double\", \"\",1\r\n");
                sw.Write("            Property: \"Emissive\", \"Vector3D\", \"\",0,0,0\r\n");
                sw.Write("            Property: \"Ambient\", \"Vector3D\", \"\"," + Convert.ToString(mat.color.r).Replace(",", ".") + "," + Convert.ToString(mat.color.g).Replace(",", ".") + "," + Convert.ToString(mat.color.b).Replace(",", ".") + "\r\n");
                sw.Write("            Property: \"Diffuse\", \"Vector3D\", \"\"," + Convert.ToString(mat.color.r).Replace(",", ".") + "," + Convert.ToString(mat.color.g).Replace(",", ".") + "," + Convert.ToString(mat.color.b).Replace(",", ".") + "\r\n");
                sw.Write("            Property: \"Specular\", \"Vector3D\", \"\",0,0,0\r\n");
                sw.Write("            Property: \"Shininess\", \"double\", \"\",2.0000000206574\r\n");
                sw.Write("            Property: \"Opacity\", \"double\", \"\",1\r\n");
                sw.Write("            Property: \"Reflectivity\", \"double\", \"\",0\r\n");
                sw.Write("        }\r\n");
                sw.Write("    }\r\n");
                tex++;
            }

            //Texture export
            tex = 0;
            string ProPath = Application.dataPath;
            ProPath = ProPath.Substring(0, ProPath.Length - 6);

            foreach (Material mat in totalUniqueMaterials)
            {
                Texture tex0;
                if (mat.HasProperty("_MainTex") && (tex0 = mat.GetTexture("_MainTex")))
                {
                    sw.Write("    Texture: \"Texture::" + @mat.name + "_diff" + "\", \"\" {\r\n");
                    sw.Write("        Type: \"TextureVideoClip\"\r\n");
                    sw.Write("        Version: 202\r\n");
                    sw.Write("        TextureName: \"Texture::" + @mat.name + "_diff" + "\"\r\n");
                    sw.Write("        Properties60:  {\r\n");
                    sw.Write("            Property: \"TextureTypeUse\", \"enum\", \"\",0\r\n");
                    sw.Write("            Property: \"Texture alpha\", \"Number\", \"A+\",1\r\n");
                    sw.Write("            Property: \"CurrentMappingType\", \"enum\", \"\",0\r\n");
                    sw.Write("            Property: \"WrapModeU\", \"enum\", \"\",0\r\n");
                    sw.Write("            Property: \"WrapModeV\", \"enum\", \"\",0\r\n");
                    sw.Write("            Property: \"UVSwap\", \"bool\", \"\",0\r\n");
                    sw.Write("            Property: \"Translation\", \"Vector\", \"A+\"," + Convert.ToString(mat.mainTextureOffset.x).Replace(",",".") + "," + Convert.ToString(mat.mainTextureOffset.y).Replace(",",".") + ",0\r\n");
                    sw.Write("            Property: \"Rotation\", \"Vector\", \"A+\",0,0,0\r\n");
                    sw.Write("            Property: \"Scaling\", \"Vector\", \"A+\"," + Convert.ToString(mat.mainTextureScale.x).Replace(",", ".") + "," + Convert.ToString(mat.mainTextureScale.y).Replace(",", ".") + ",1\r\n");
                    sw.Write("            Property: \"TextureRotationPivot\", \"Vector3D\", \"\",0,0,0\r\n");
                    sw.Write("            Property: \"TextureScalingPivot\", \"Vector3D\", \"\",0,0,0\r\n");
                    sw.Write("            Property: \"UseMaterial\", \"bool\", \"\",1\r\n");
                    sw.Write("            Property: \"UseMipMap\", \"bool\", \"\",0\r\n");
                    sw.Write("            Property: \"CurrentTextureBlendMode\", \"enum\", \"\",1\r\n");
                    sw.Write("            Property: \"UVSet\", \"KString\", \"\", \"UVChannel_1\"\r\n");
                    sw.Write("        }\r\n");
                    //sw.Write("        Media: \"Video::" +@ta2[0] +"_diff"+ "\"\r\n");
                    sw.Write("        FileName: \"" + (ProPath + AssetDatabase.GetAssetPath(tex0)) + "\"\r\n");
                    //sw.Write("        RelativeFilename: \"" +  @(ProPath + EditorUtility.GetAssetPath(tex0)) + "\"\r\n");
                    sw.Write("        ModelUVTranslation: 0,0\r\n");
                    sw.Write("        ModelUVScaling: 1,1\r\n");
                    sw.Write("        Texture_Alpha_Source: \"None\"\r\n");
                    sw.Write("        Cropping: 0,0,0,0\r\n");
                    sw.Write("    }\r\n");
                }
                if (mat.HasProperty("_BumpMap") && (tex0 = mat.GetTexture("_BumpMap")))
                {
                    sw.Write("    Texture: \"Texture::" + @mat.name + "_bump" + "\", \"\" {\r\n");
                    sw.Write("        Type: \"TextureVideoClip\"\r\n");
                    sw.Write("        Version: 202\r\n");
                    sw.Write("        TextureName: \"Texture::" + @mat.name + "_bump" + "\"\r\n");
                    sw.Write("        Properties60:  {\r\n");
                    sw.Write("            Property: \"TextureTypeUse\", \"enum\", \"\",0\r\n");
                    sw.Write("            Property: \"Texture alpha\", \"Number\", \"A+\",1\r\n");
                    sw.Write("            Property: \"CurrentMappingType\", \"enum\", \"\",0\r\n");
                    sw.Write("            Property: \"WrapModeU\", \"enum\", \"\",0\r\n");
                    sw.Write("            Property: \"WrapModeV\", \"enum\", \"\",0\r\n");
                    sw.Write("            Property: \"UVSwap\", \"bool\", \"\",0\r\n");
                    sw.Write("            Property: \"Translation\", \"Vector\", \"A+\"," + Convert.ToString(mat.GetTextureOffset("_BumpMap").x).Replace(",", ".") + "," + Convert.ToString(mat.GetTextureOffset("_BumpMap").y).Replace(",", ".") + ",0\r\n");
                    sw.Write("            Property: \"Rotation\", \"Vector\", \"A+\",0,0,0\r\n");
                    sw.Write("            Property: \"Scaling\", \"Vector\", \"A+\"," + Convert.ToString(mat.GetTextureScale("_BumpMap").x).Replace(",", ".") + "," + Convert.ToString(mat.GetTextureScale("_BumpMap").y).Replace(",", ".") + ",1\r\n");
                    sw.Write("            Property: \"TextureRotationPivot\", \"Vector3D\", \"\",0,0,0\r\n");
                    sw.Write("            Property: \"TextureScalingPivot\", \"Vector3D\", \"\",0,0,0\r\n");
                    sw.Write("            Property: \"UseMaterial\", \"bool\", \"\",1\r\n");
                    sw.Write("            Property: \"UseMipMap\", \"bool\", \"\",0\r\n");
                    sw.Write("            Property: \"CurrentTextureBlendMode\", \"enum\", \"\",1\r\n");
                    sw.Write("            Property: \"UVSet\", \"KString\", \"\", \"UVChannel_1\"\r\n");
                    sw.Write("        }\r\n");
                    //sw.Write("        Media: \"Video::" +@ta2[0]+"_bump" + "\"\r\n");
                    sw.Write("        FileName: \"" + (ProPath + AssetDatabase.GetAssetPath(tex0)) + "\"\r\n");
                    //sw.Write("        RelativeFilename: \"" +  (ProPath + EditorUtility.GetAssetPath(tex0)) + "\"\r\n");
                    sw.Write("        ModelUVTranslation: 0,0\r\n");
                    sw.Write("        ModelUVScaling: 1,1\r\n");
                    sw.Write("        Texture_Alpha_Source: \"None\"\r\n");
                    sw.Write("        Cropping: 0,0,0,0\r\n");
                    sw.Write("    }\r\n");
                }
            }

// ######### Global Settings

            sw.Write("    GlobalSettings:  {\r\n");
            sw.Write("        Version: 1000\r\n");
            sw.Write("        Properties60:  {\r\n");
            sw.Write("            Property: \"UpAxis\", \"int\", \"\",1\r\n");
            sw.Write("            Property: \"UpAxisSign\", \"int\", \"\",1\r\n");
            sw.Write("            Property: \"FrontAxis\", \"int\", \"\",2\r\n");
            sw.Write("            Property: \"FrontAxisSign\", \"int\", \"\",1\r\n");
            sw.Write("            Property: \"CoordAxis\", \"int\", \"\",0\r\n");
            sw.Write("            Property: \"CoordAxisSign\", \"int\", \"\",1\r\n");
            sw.Write("            Property: \"OriginalUpAxis\", \"int\", \"\",1\r\n");
            sw.Write("            Property: \"OriginalUpAxisSign\", \"int\", \"\",1\r\n");
            sw.Write("            Property: \"UnitScaleFactor\", \"double\", \"\"," + LightmappingTool.exportScale +"\r\n");
            sw.Write("        }\r\n");
            sw.Write("    }\r\n");


            EditorUtility.DisplayProgressBar("Exporting FBX", "Writing connections...", 0.95f);
            sw.Write("}\r\n");
            sw.Write("\r\n\r\n");
            sw.Write("; Object connections\r\n");
            sw.Write(";------------------------------------------------------------------\r\n");
            sw.Write("\r\n");
            sw.Write("Connections:  {\r\n");

            for (int i = 0; i < lights.Length; i++)
            {
                sw.Write("    Connect: \"OO\", \"Model::ImportedLight" + (i+1) + "\", \"Model::Scene\"\r\n");
            }
            
            for (int obj = 0; obj < bigArray.Count; obj++)
            {
                sw.Write("    Connect: \"OO\", \"Model::ImportedObject" + Convert.ToString(obj+1) + "\", \"Model::Scene\"\r\n");
                foreach (Material mat in ((ArrayList)uniqueMaterialsArray[obj]))
                {
                    sw.Write("    Connect: \"OO\", \"Material::" + @mat.name + "\", \"Model::ImportedObject" + Convert.ToString(obj+1) + "\"\r\n");
                    if (mat.HasProperty("_MainTex") && (mat.GetTexture("_MainTex")))
                    {
                        sw.Write("    Connect: \"OO\", \"Texture::" + @mat.name + "_diff" + "\", \"Model::ImportedObject" + Convert.ToString(obj+1) + "\"\r\n");
                    }
                }
            }
            for (int obj = 0; obj < bigArray.Count; obj++)
            {
                foreach (Material mat in ((ArrayList)uniqueMaterialsArray[obj]))
                {
                    if (mat.HasProperty("_BumpMap") && (mat.GetTexture("_BumpMap")))
                    {
                        sw.Write("    Connect: \"OO\", \"Texture::" + @mat.name + "_bump" + "\", \"Model::ImportedObject" + Convert.ToString(obj+1) + "\"\r\n");
                    }
                }
            }
            sw.Write("}\r\n");
            sw.Write(FBXFooter());
            //sw.Write(sb.ToString().Replace("E-0", "e-00"));
            sw.Close();
            sw.Dispose();
            EditorUtility.ClearProgressBar();

            return true;
        }
    }
    public static string FBXHeader()
    {
        return "; FBX 6.1.0 project file\r\n; Copyright (C) 1997-2008 Autodesk Inc. and/or its licensors.\r\n; All rights reserved.\r\n; ----------------------------------------------------\r\nFBXHeaderExtension:  {\r\n    FBXHeaderVersion: 1003\r\n    FBXVersion: 6100\r\n    CreationTimeStamp:  {\r\n        Version: 1000\r\n        Year: 2009\r\n        Month: 7\r\n        Day: 22\r\n        Hour: 9\r\n        Minute: 3\r\n        Second: 17\r\n        Millisecond: 354\r\n    }\r\n    Creator: \"FBX SDK/FBX Plugins version 2010.2\"\r\n    OtherFlags:  {\r\n        FlagPLE: 0\r\n    }\r\n}\r\nCreationTime: \"2009-07-22 09:03:17:354\"\r\nCreator: \"FBX SDK/FBX Plugins build 20090731\"\r\n; Document Description\r\n;------------------------------------------------------------------\r\nDocument:  {\r\n    Name: \"\"\r\n}\r\n; Document References\r\n;------------------------------------------------------------------\r\nReferences:  {\r\n}\r\n";
    }
    public static string FBXFooter()
    {
        return ";Version 5 settings\r\n;------------------------------------------------------------------\r\n\r\nVersion5:  {\r\n    AmbientRenderSettings:  {\r\n        Version: 101\r\n        AmbientLightColor: 0,0,0,1\r\n    }\r\n\r\n    FogOptions:  {\r\n        FogEnable: 0\r\n        FogMode: 0\r\n        FogDensity: 0.002\r\n        FogStart: 0.3\r\n        FogEnd: 1000\r\n        FogColor: 1,1,1,1\r\n    }\r\n    Settings:  {\r\n        FrameRate: \"30\"\r\n        TimeFormat: 1\r\n        SnapOnFrames: 0\r\n        ReferenceTimeIndex: -1\r\n        TimeLineStartTime: 0\r\n        TimeLineStopTime: 153953860000\r\n    }\r\n    RendererSetting:  {\r\n        DefaultCamera: \"Producer Perspective\"\r\n        DefaultViewingMode: 0\r\n    }\r\n}\r\n";
    }

}

