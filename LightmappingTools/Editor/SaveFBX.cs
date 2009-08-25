using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;


class SaveFBX
{
    public static ArrayList offsetsArray;
    public bool ExportFBX(ref ArrayList bigArray, ref ArrayList materialsArray, ref ArrayList uniqueMaterialsArray, ref ArrayList totalUniqueMaterials, ref Light[] lights, int textures, int totalCount)
    {

        offsetsArray = new ArrayList();
        using (StreamWriter sw = new StreamWriter("MaxFiles\\" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".fbx"))
        {
            EditorUtility.DisplayProgressBar("Exporting FBX", "Setting headers...", 0.1f);
            StringBuilder sb = new StringBuilder();
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
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing model data" + model, 0.1f);
                sb.Append("    Model: \"Model::Object" + Convert.ToString(model) + "\", \"Mesh\" {\r\n");
                sb.Append("        Version: 232\r\n");
                sb.Append("        Properties60:  {\r\n");
                sb.Append("            Property: \"GeometricScaling\", \"Vector3D\", \"\",-1,1,1\r\n");
                sb.Append("            Property:\"Show\",\"bool\",\"\",1\r\n");
                sb.Append("            Property:\"NegativePercentShapeSupport\",\"bool\",\"\",1\r\n");
                sb.Append("            Property:\"DefaultAttributeIndex\",\"int\",\"\",0\r\n");
                sb.Append("            Property:\"Visibility\",\"Visibility\",\"A+\",1\r\n");
                sb.Append("            Property:\"Color\",\"ColorRGB\",\"N\",0.23921568627451,0.52156862745098,0.0235294117647059\r\n");
                sb.Append("            Property: \"BBoxMin\",\"Vector3D\",\"N\",0,0,0\r\n");
                sb.Append("            Property: \"BBoxMax\",\"Vector3D\",\"N\",0,0,0\r\n");
                sb.Append("        }\r\n");
                sb.Append("        MultiLayer: 0\r\n");
                sb.Append("        MultiTake: 1\r\n");
                sb.Append("        Shading: T\r\n");
                sb.Append("        Culling: \"CullingOff\"\r\n");
                sb.Append("        Vertices: ");

                int[] vertexoffsets = new int[mf.Length + 1];
                int vertexoffset = 0;
                vertexoffsets[0] = 0;
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing vertices's of model " + model, 0.2f);

//############## EXPORTING VERTICLES
                Vector3 vrt;
                for (int j = 0; j < mf.Length; j++)
                {
                    int i = 0;
                    foreach (Vector3 vert in mf[j].sharedMesh.vertices)
                    {
                        vrt = mf[j].transform.TransformPoint(vert);
                        sb.Append(Convert.ToString(vrt.x).Replace(",", ".") + ",").Append(Convert.ToString(vrt.y).Replace(",", ".") + ",").Append(Convert.ToString(vrt.z).Replace(",", ".") + ",");
                        if (i % 3 == 0)
                        {
                            sb.Append("\r\n        ");
                        }
                        i++;
                    }
                    vertexoffset += ((MeshFilter)mf[j]).sharedMesh.vertexCount;
                    vertexoffsets[j + 1] = vertexoffset;
                }
                sb.Append("\r\n        PolygonVertexIndex: ");

//############## EXPORTING TRIANGLES
                int k = 0;
                int[] triangles;                
                foreach (MeshFilter mesh in mf)
                {
                    triangles = mesh.sharedMesh.triangles;
                    for (int i = 0; i < triangles.Length; i+=3)
                    {
                        sb.Append((triangles[i] + vertexoffsets[k]) + ",").Append((triangles[i+1] + vertexoffsets[k]) + ",").Append((-(triangles[i+2] + vertexoffsets[k] + 1)) + ",");
                        if (i % 9 == 0) sb.Append("\r\n        ");

                    }
                    k++;
                }
                sb.Append("\r\n        GeometryVersion: 124\r\n");
                sb.Append("        LayerElementNormal: 0 {\r\n");
                sb.Append("            Version: 101\r\n");
                sb.Append("            Name: \"\"\r\n");
                sb.Append("            MappingInformationType: \"ByVertice\"\r\n");
                sb.Append("            ReferenceInformationType: \"Direct\"\r\n");

//############## NORMALS EXPORTING
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing normals of model " + model, 0.5f);
                sb.Append("            Normals: ");
                Matrix4x4 transformMatrix;
                Vector3[] normals;

                foreach (MeshFilter mesh in mf)
                {
                    normals = mesh.sharedMesh.normals;
                    transformMatrix = mesh.transform.localToWorldMatrix.inverse.transpose;
                    for (int i = 0; i < normals.Length; i++)
                    {
                        vrt = transformMatrix.MultiplyVector(normals[i]);
                        sb.Append(Convert.ToString(vrt.x).Replace(",", ".") + ",").Append(Convert.ToString(vrt.y).Replace(",", ".") + ",").Append(Convert.ToString(vrt.z).Replace(",", ".") + ",");
                        if (i % 3 == 0) sb.Append("\r\n            ");
                    }
                }
                sb.Append("\r\n        }\r\n");


//############## UV Exporting
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing UV's of model " + model, 0.7f);
                sb.Append("        LayerElementUV: 0 {\r\n");
                sb.Append("            Version: 101\r\n");
                sb.Append("            Name: \"UVChannel_1\"\r\n");
                sb.Append("            MappingInformationType: \"ByVertice\"\r\n");
                sb.Append("            ReferenceInformationType: \"Direct\"\r\n");

                sb.Append("            UV: ");
                foreach (MeshFilter mesh in mf)
                {
                    if (mesh.sharedMesh.uv.Length < 1)
                    {
                        for (int j = 0; j < mesh.sharedMesh.vertexCount; j++)
                        {
                            sb.Append("0.0,0.0,0.0,0.0,");
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
                            sb.Append(Convert.ToString(v.x).Replace(",", ".") + ",").Append(Convert.ToString(v.y).Replace(",", ".") + ",");
                            if (i % 7 == 0)
                            {
                                sb.Append("\r\n            ");
                            }
                            i++;
                        }
                    }
                    
                }

                sb.Append("\r\n        }\r\n");



//############## UV2 Exporting
                sb.Append("        LayerElementUV: 1 {\r\n");
                sb.Append("            Version: 101\r\n");
                sb.Append("            Name: \"UVChannel_2\"\r\n");
                sb.Append("            MappingInformationType: \"ByVertice\"\r\n");
                sb.Append("            ReferenceInformationType: \"Direct\"\r\n");

                sb.Append("            UV: ");
                foreach (MeshFilter mesh in mf)
                {
                    if (mesh.sharedMesh.uv2.Length < 1)
                    {
                        for (int j = 0; j < mesh.sharedMesh.vertexCount; j++)
                        {
                            sb.Append("0.0,0.0,0.0,0.0,");
                            if (j % 4 == 0)
                            {
                                sb.Append("\r\n            ");
                            }
                        }
                    }
                    else
                    {
                        int i = 0;
                        foreach (Vector2 v in mesh.sharedMesh.uv2)
                        {
                            sb.Append(Convert.ToString(v.x).Replace(",", ".") + ",").Append(Convert.ToString(v.y).Replace(",", ".") + ",");
                            if (i % 7 == 0)
                            {
                                sb.Append("\r\n            ");
                            }
                            i++;
                        }
                    }
                }
                sb.Append("\r\n        }\r\n");

//############## UV3 Exporting
                sb.Append("        LayerElementUV: 2 {\r\n");
                sb.Append("            Version: 101\r\n");
                sb.Append("            Name: \"UVChannel_3\"\r\n");
                sb.Append("            MappingInformationType: \"ByVertice\"\r\n");
                sb.Append("            ReferenceInformationType: \"Direct\"\r\n");
                sb.Append("            UV: ");

//############## Preparing UV3 coordinates
                Rect[][] calculated = CalcArea.CalculateArea(mf, Convert.ToInt32(128 * Math.Pow(2,(Convert.ToInt32((int) LightmappingTool.res[model])))));
                offsetsArray.Add(calculated);
                Rect[] uvs = calculated[0];
                if (uvs == null)
                {
                    return false;
                }
                Rect[] offsets = calculated[1];

//############## UV3 Exporting
                int kk = 0;
                foreach (MeshFilter mesh in mf)
                {
                    if (mesh.sharedMesh.uv2.Length < 1)
                    {
                        for (int j = 0; j < mesh.sharedMesh.vertexCount; j++)
                        {
                            sb.Append("0.0,0.0,0.0,0.0,");
                            if (j % 4 == 0)
                            {
                                sb.Append("\r\n            ");
                            }
                        }
                    }
                    else
                    {
                        int i = 0;
                        //Debug.Log(Math.Floor((double)((uvs[kk].x + ((mesh.sharedMesh.uv2[0].x - offsets[kk].xMin) * uvs[kk].width / (offsets[kk].xMax - offsets[kk].xMin))) * 2048.0)));
                        //Debug.Log((uvs[kk].y + ((mesh.sharedMesh.uv2[0].y - offsets[kk].yMin) * uvs[kk].height / (offsets[kk].yMax - offsets[kk].yMin))) * 2048.0);
                        foreach (Vector2 v in mesh.sharedMesh.uv2)
                        {
                            try
                            {
                                
                                sb.Append(Convert.ToString(Math.Round((double)((uvs[kk].x + ((v.x - offsets[kk].xMin) * uvs[kk].width / (offsets[kk].xMax - offsets[kk].xMin)))))).Replace(",", ".") + ",").Append(Convert.ToString(Math.Round((double)(uvs[kk].y + ((v.y - offsets[kk].yMin) * uvs[kk].height / (offsets[kk].yMax - offsets[kk].yMin))))).Replace(",", ".") + ",");
                            }
                            catch
                            {
                                sb.Append("0.0,0.0,0.0,0.0,");
                                Debug.Log("Non-Valid UV2 on " + mesh.name);
                            }
                            if (i % 1 == 0) sb.Append("\r\n            ");
                            i++;
                        }
                    }
                    kk++;
                    sb.Append("\r\n            ");
                }
                sb.Append("\r\n        }\r\n");

                //GC should clean that
                uvs = null;
                offsets = null;
                calculated = null;

//############## MATERIAL DISTRIBUTION PART
                EditorUtility.DisplayProgressBar("Exporting FBX", "Writing materials of model " + model, 0.8f);
                sb.Append("        LayerElementMaterial: 0 {\r\n");
                sb.Append("            Version: 101\r\n");
                sb.Append("            Name: \"\"\r\n");
                sb.Append("            MappingInformationType: \"ByPolygon\"\r\n");
                sb.Append("            ReferenceInformationType: \"IndexToDirect\"\r\n");
                sb.Append("            Materials: ");

                int countner = 0;
                int value;
                for (int z = 0; z < mf.Length; z++)
                {
                    for (int t = 0; t < ((MeshFilter)mf[z]).sharedMesh.subMeshCount; t++)
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
                        for (int i = 0; i < ((MeshFilter)mf[z]).sharedMesh.GetTriangles(t).Length; i++)
                        {
                            if (i % 3 == 2)
                            {
                                sb.Append(value + ",");

                                if (i % 20 == 0) sb.Append("\r\n            ");
                            }
                        }
                    }
                }


                sb.Append("\r\n        }\r\n");

// ############# Texture part

                sb.Append("        LayerElementTexture: 0 {\r\n");
                sb.Append("            Version: 101\r\n");
                sb.Append("            Name: \"\"\r\n");
                sb.Append("            MappingInformationType: \"ByPolygon\"\r\n");
                sb.Append("            ReferenceInformationType: \"IndexToDirect\"\r\n");
                sb.Append("            BlendMode: \"Translucent\"\r\n");
                sb.Append("            TextureAlpha: 1\r\n");
                sb.Append("            TextureId: ");

                int mapOffset = 0;
                countner = 0;
                value = 0;
                ArrayList nonMap = new ArrayList();
                ArrayList inserted = new ArrayList();

                for (int z = 0; z < mf.Length; z++)
                {
                    for (int t = 0; t < ((MeshFilter)mf[z]).sharedMesh.subMeshCount; t++)
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
                        for (int i = 0; i < ((MeshFilter)mf[z]).sharedMesh.GetTriangles(t).Length; i++)
                        {
                            if (i % 3 == 2)
                            {
                                sb.Append(value + ",");
                            }
                        }
                        sb.Append("\r\n            ");
                    }
                }
                sb.Append("\r\n        }\r\n");


                countner = 0;
                value = 0;
                nonMap = new ArrayList();
                inserted = new ArrayList();

                sb.Append("        LayerElementBumpTextures: 0 {\r\n");
                sb.Append("            Version: 101\r\n");
                sb.Append("            Name: \"\"\r\n");
                sb.Append("            MappingInformationType: \"ByPolygon\"\r\n");
                sb.Append("            ReferenceInformationType: \"IndexToDirect\"\r\n");
                sb.Append("            BlendMode: \"Translucent\"\r\n");
                sb.Append("            TextureAlpha: 1\r\n");
                sb.Append("            TextureId: ");

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
                                sb.Append(value + ",");

                                if (i % 20 == 0)
                                {
                                    //sb.Append("\r\n            ");
                                }
                            }
                        }
                        sb.Append("\r\n            ");
                    }
                }

                sb.Append("\r\n        }\r\n");

                //GC should clean this:
                nonMap = null;
                inserted = null;

                sb.Append("        Layer: 0 {\r\n");
                sb.Append("            Version: 100\r\n");
                sb.Append("            LayerElement:  {\r\n");
                sb.Append("                Type: \"LayerElementNormal\"\r\n");
                sb.Append("                TypedIndex: 0\r\n");
                sb.Append("            }\r\n");
                sb.Append("            LayerElement:  {\r\n");
                sb.Append("                Type: \"LayerElementMaterial\"\r\n");
                sb.Append("                TypedIndex: 0\r\n");
                sb.Append("            }\r\n");
                sb.Append("            LayerElement:  {\r\n");
                sb.Append("                Type: \"LayerElementTexture\"\r\n");
                sb.Append("                TypedIndex: 0\r\n");
                sb.Append("            }\r\n");
                sb.Append("            LayerElement:  {\r\n");
                sb.Append("                Type: \"LayerElementBumpTextures\"\r\n");
                sb.Append("                TypedIndex: 0\r\n");
                sb.Append("            }\r\n");
                sb.Append("            LayerElement:  {\r\n");
                sb.Append("                Type: \"LayerElementUV\"\r\n");
                sb.Append("                TypedIndex: 0\r\n");
                sb.Append("            }\r\n");
                sb.Append("        }\r\n");
                sb.Append("        Layer: 1 {\r\n");
                sb.Append("            Version: 100\r\n");
                sb.Append("            LayerElement:  {\r\n");
                sb.Append("                Type: \"LayerElementUV\"\r\n");
                sb.Append("                TypedIndex: 1\r\n");
                sb.Append("            }\r\n");
                sb.Append("        }\r\n");
                sb.Append("        Layer: 2 {\r\n");
                sb.Append("            Version: 100\r\n");
                sb.Append("            LayerElement:  {\r\n");
                sb.Append("                Type: \"LayerElementUV\"\r\n");
                sb.Append("                TypedIndex: 2\r\n");
                sb.Append("            }\r\n");
                sb.Append("        }\r\n");
                sb.Append("        NodeAttributeName: \"Geometry::Object" + Convert.ToString(model) + "\"\r\n");
                sb.Append("    }\r\n");
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

                sb.Append("Model: \"Model::Light" + i + "\", \"Light\" {\r\n");
                sb.Append("        Version: 232\r\n");
                sb.Append("        Properties60:  {\r\n");
                sb.Append("            Property: \"PreRotation\", \"Vector3D\", \"\",-90,0,0\r\n");
                sb.Append("            Property: \"PostRotation\", \"Vector3D\", \"\",0,0," + Convert.ToString(-lights[i].transform.eulerAngles.z).Replace(",", ".") + "\r\n");
                sb.Append("            Property: \"RotationActive\", \"bool\", \"\",1\r\n");
                sb.Append("            Property: \"Lcl Translation\", \"Lcl Translation\", \"A+\"," + Convert.ToString(-lights[i].transform.position.x).Replace(",", ".") + "," + Convert.ToString(lights[i].transform.position.y).Replace(",", ".") + "," + Convert.ToString(lights[i].transform.position.z).Replace(",", ".") + "\r\n");
                sb.Append("            Property: \"Lcl Rotation\", \"Lcl Rotation\", \"A+\"," + Convert.ToString(lights[i].transform.eulerAngles.x).Replace(",", ".") + ",0," + Convert.ToString(-lights[i].transform.eulerAngles.y).Replace(",", ".") + " \r\n");
                sb.Append("            Property: \"Lcl Scaling\", \"Lcl Scaling\", \"A+\",1,1,1\r\n");
                sb.Append("            Property: \"Visibility\", \"Visibility\", \"A+\",1\r\n");
                sb.Append("            Property: \"Color\", \"Color\", \"A+N\"," + Convert.ToString(lights[i].color.r).Replace(",", ".") + "," + Convert.ToString(lights[i].color.g).Replace(",", ".") + "," + Convert.ToString(lights[i].color.b).Replace(",", ".") + "\r\n");
                sb.Append("            Property: \"LightType\", \"enum\", \"N\"," + Convert.ToString(enumType) + "\r\n");
                sb.Append("            Property: \"CastLightOnObject\", \"bool\", \"N\",1\r\n");
                sb.Append("            Property: \"DrawVolumetricLight\", \"bool\", \"N\",1\r\n");
                sb.Append("            Property: \"DrawGroundProjection\", \"bool\", \"N\",0\r\n");
                sb.Append("            Property: \"DrawFrontFacingVolumetricLight\", \"bool\", \"N\",0\r\n");
                sb.Append("            Property: \"Intensity\", \"Number\", \"A+N\"," + Convert.ToString(lights[i].intensity * LightmapAdvanced.lightMultipler*100).Replace(",", ".") + "\r\n");
                sb.Append("            Property: \"HotSpot\", \"Number\", \"A+N\"," + ((enumType != 0) ? lights[i].spotAngle: 0) + "\r\n");
                sb.Append("            Property: \"Cone angle\", \"Number\", \"A+N\"," + Convert.ToString(lights[i].spotAngle).Replace(",", ".") + "\r\n");
                sb.Append("            Property: \"Fog\", \"Number\", \"A+N\",0\r\n");
                sb.Append("            Property: \"DecayType\", \"enum\", \"N\"," + (lights[i].attenuate ? 1 : 0) + "\r\n");
                sb.Append("            Property: \"DecayStart\", \"Number\", \"A+N\",40\r\n");
                sb.Append("            Property: \"FileName\", \"KString\", \"N\", \"\"\r\n");
                sb.Append("            Property: \"EnableNearAttenuation\", \"bool\", \"N\",0\r\n");
                sb.Append("            Property: \"NearAttenuationStart\", \"Number\", \"A+N\",0\r\n");
                sb.Append("            Property: \"NearAttenuationEnd\", \"Number\", \"A+N\",40\r\n");
                sb.Append("            Property: \"EnableFarAttenuation\", \"bool\", \"N\",0\r\n");
                sb.Append("            Property: \"FarAttenuationStart\", \"Number\", \"A+N\",80\r\n");
                sb.Append("            Property: \"FarAttenuationEnd\", \"Number\", \"A+N\",200\r\n");
                sb.Append("            Property: \"CastShadows\", \"bool\", \"N\",1\r\n");
                sb.Append("            Property: \"ShadowColor\", \"Color\", \"A+N\",0,0,0\r\n");
                sb.Append("            Property: \"3dsMax\", \"Compound\", \"N\"\r\n");
                sb.Append("            Property: \"3dsMax|ClassIDa\", \"int\", \"N\",4113\r\n");
                sb.Append("            Property: \"3dsMax|ClassIDb\", \"int\", \"N\",0\r\n");
                sb.Append("            Property: \"3dsMax|SuperClassID\", \"int\", \"N\",48\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0\", \"Compound\", \"N\"\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Color\", \"Color\", \"AN\",1,1,1\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Multiplier\", \"Float\", \"AN\", " + Convert.ToString(lights[i].intensity * LightmapAdvanced.lightMultipler*100).Replace(",", ".") + " \r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Contrast\", \"Float\", \"AN\",0\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Diffuse Soften\", \"Float\", \"AN\",0\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Attenuation Near Start\", \"Float\", \"AN\",0\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Attenuation Near End\", \"Float\", \"AN\",40\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Attenuation Far Start\", \"Float\", \"AN\",80\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Attenuation Far End\", \"Float\", \"AN\",200\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Decay Falloff\", \"Float\", \"AN\",40\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Shadow Color\", \"Color\", \"AN\",0,0,0\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|_Unnamed_Parameter_10\", \"int\", \"N\",0\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Atmosphere Opacity\", \"Float\", \"AN\",1\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Atmosphere Color Amount\", \"Float\", \"AN\",1\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|Shadow Density\", \"Float\", \"AN\",1\r\n");
                sb.Append("            Property: \"3dsMax|ParamBlock_0|_Unnamed_Parameter_14\", \"int\", \"N\",0\r\n");
                sb.Append("        }\r\n");
                sb.Append("        MultiLayer: 0\r\n");
                sb.Append("        MultiTake: 0\r\n");
                sb.Append("        Shading: T\r\n");
                sb.Append("        Culling: \"CullingOff\"\r\n");
                sb.Append("        TypeFlags: \"Light\"\r\n");
                sb.Append("        GeometryVersion: 124\r\n");
                sb.Append("        NodeAttributeName: \"NodeAttribute::Light" + i + "\"\r\n");
                sb.Append("    }\r\n");
            }


            int tex = 0;
            foreach (Material mat in totalUniqueMaterials)
            {
                sb.Append("    Material: \"Material::" + @mat.name + "\", \"\" {\r\n");
                sb.Append("        Version: 102\r\n");
                sb.Append("        ShadingModel: \"phong\"\r\n");
                sb.Append("        MultiLayer: 0\r\n");
                sb.Append("        Properties60:  {\r\n");
                sb.Append("            Property: \"ShadingModel\", \"KString\", \"\", \"phong\"\r\n");
                sb.Append("            Property: \"MultiLayer\", \"bool\", \"\",0\r\n");
                if (mat.HasProperty("_Emission"))
                {
                    sb.Append("            Property: \"EmissiveColor\", \"ColorRGB\"," + Convert.ToString(mat.GetColor("_Emission").r).Replace(",", ".") + "," + Convert.ToString(mat.GetColor("_Emission").g).Replace(",", ".") + "," + Convert.ToString(mat.GetColor("_Emission").b).Replace(",", ".") + "\r\n");
                    sb.Append("            Property: \"EmissiveFactor\", \"double\", \"\"," + Convert.ToString(mat.GetFloat("_Shininess")).Replace(",", ".") + "\r\n");
                }
                else
                {
                    sb.Append("            Property: \"EmissiveColor\", \"ColorRGB\", \"\",0,0,0\r\n");
                    sb.Append("            Property: \"EmissiveFactor\", \"double\", \"\",0\r\n");
                }
                sb.Append("            Property: \"AmbientColor\", \"ColorRGB\", \"\"," + Convert.ToString(mat.color.r).Replace(",", ".") + "," + Convert.ToString(mat.color.g).Replace(",", ".") + "," + Convert.ToString(mat.color.b).Replace(",", ".") + "\r\n");
                sb.Append("            Property: \"AmbientFactor\", \"double\", \"\",1\r\n");
                sb.Append("            Property: \"DiffuseColor\", \"ColorRGB\", \"\"," + Convert.ToString(mat.color.r).Replace(",", ".") + "," + Convert.ToString(mat.color.g).Replace(",", ".") + "," + Convert.ToString(mat.color.b).Replace(",", ".") + "\r\n");
                sb.Append("            Property: \"DiffuseFactor\", \"double\", \"\",1\r\n");
                sb.Append("            Property: \"Bump\", \"Vector3D\", \"\",0,0,0\r\n");
                sb.Append("            Property: \"NormalMap\", \"Vector3D\", \"\",0,0,0\r\n");
                sb.Append("            Property: \"BumpFactor\", \"double\", \"\",1\r\n");
                sb.Append("            Property: \"TransparentColor\", \"ColorRGB\", \"\",1,1,1\r\n");
                sb.Append("            Property: \"TransparencyFactor\", \"double\", \"\",0\r\n");
                sb.Append("            Property: \"DisplacementColor\", \"ColorRGB\", \"\",0,0,0\r\n");
                sb.Append("            Property: \"DisplacementFactor\", \"double\", \"\",1\r\n");
                if (mat.HasProperty("_SpecColor"))
                {
                    sb.Append("            Property: \"SpecularColor\", \"ColorRGB\", \"\"," + Convert.ToString(mat.GetColor("_SpecColor").r).Replace(",", ".") + "," + Convert.ToString(mat.GetColor("_SpecColor").g).Replace(",", ".") + "," + Convert.ToString(mat.GetColor("_SpecColor").b).Replace(",", ".") + "\r\n");
                    sb.Append("            Property: \"SpecularFactor\", \"double\", \"\"," + Convert.ToString(mat.GetFloat("_Shininess")).Replace(",", ".") + "\r\n");
                }
                else
                {
                    sb.Append("            Property: \"SpecularColor\", \"ColorRGB\", \"\",0.0,0.0,0.0\r\n");
                    sb.Append("            Property: \"SpecularFactor\", \"double\", \"\",0\r\n");
                }
                sb.Append("            Property: \"ShininessExponent\", \"double\", \"\",2.0000000206574\r\n");
                sb.Append("            Property: \"ReflectionColor\", \"ColorRGB\", \"\",0,0,0\r\n");
                sb.Append("            Property: \"ReflectionFactor\", \"double\", \"\",1\r\n");
                sb.Append("            Property: \"Emissive\", \"Vector3D\", \"\",0,0,0\r\n");
                sb.Append("            Property: \"Ambient\", \"Vector3D\", \"\"," + Convert.ToString(mat.color.r).Replace(",", ".") + "," + Convert.ToString(mat.color.g).Replace(",", ".") + "," + Convert.ToString(mat.color.b).Replace(",", ".") + "\r\n");
                sb.Append("            Property: \"Diffuse\", \"Vector3D\", \"\"," + Convert.ToString(mat.color.r).Replace(",", ".") + "," + Convert.ToString(mat.color.g).Replace(",", ".") + "," + Convert.ToString(mat.color.b).Replace(",", ".") + "\r\n");
                sb.Append("            Property: \"Specular\", \"Vector3D\", \"\",0,0,0\r\n");
                sb.Append("            Property: \"Shininess\", \"double\", \"\",2.0000000206574\r\n");
                sb.Append("            Property: \"Opacity\", \"double\", \"\",1\r\n");
                sb.Append("            Property: \"Reflectivity\", \"double\", \"\",0\r\n");
                sb.Append("        }\r\n");
                sb.Append("    }\r\n");
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
                    sb.Append("    Texture: \"Texture::" + @mat.name + "_diff" + "\", \"\" {\r\n");
                    sb.Append("        Type: \"TextureVideoClip\"\r\n");
                    sb.Append("        Version: 202\r\n");
                    sb.Append("        TextureName: \"Texture::" + @mat.name + "_diff" + "\"\r\n");
                    sb.Append("        Properties60:  {\r\n");
                    sb.Append("            Property: \"TextureTypeUse\", \"enum\", \"\",0\r\n");
                    sb.Append("            Property: \"Texture alpha\", \"Number\", \"A+\",1\r\n");
                    sb.Append("            Property: \"CurrentMappingType\", \"enum\", \"\",0\r\n");
                    sb.Append("            Property: \"WrapModeU\", \"enum\", \"\",0\r\n");
                    sb.Append("            Property: \"WrapModeV\", \"enum\", \"\",0\r\n");
                    sb.Append("            Property: \"UVSwap\", \"bool\", \"\",0\r\n");
                    sb.Append("            Property: \"Translation\", \"Vector\", \"A+\"," + Convert.ToString(mat.mainTextureOffset.x).Replace(",",".") + "," + Convert.ToString(mat.mainTextureOffset.y).Replace(",",".") + ",0\r\n");
                    sb.Append("            Property: \"Rotation\", \"Vector\", \"A+\",0,0,0\r\n");
                    sb.Append("            Property: \"Scaling\", \"Vector\", \"A+\"," + Convert.ToString(mat.mainTextureScale.x).Replace(",", ".") + "," + Convert.ToString(mat.mainTextureScale.y).Replace(",", ".") + ",1\r\n");
                    sb.Append("            Property: \"TextureRotationPivot\", \"Vector3D\", \"\",0,0,0\r\n");
                    sb.Append("            Property: \"TextureScalingPivot\", \"Vector3D\", \"\",0,0,0\r\n");
                    sb.Append("            Property: \"UseMaterial\", \"bool\", \"\",1\r\n");
                    sb.Append("            Property: \"UseMipMap\", \"bool\", \"\",0\r\n");
                    sb.Append("            Property: \"CurrentTextureBlendMode\", \"enum\", \"\",1\r\n");
                    sb.Append("            Property: \"UVSet\", \"KString\", \"\", \"UVChannel_1\"\r\n");
                    sb.Append("        }\r\n");
                    //sb.Append("        Media: \"Video::" +@ta2[0] +"_diff"+ "\"\r\n");
                    sb.Append("        FileName: \"" + (ProPath + EditorUtility.GetAssetPath(tex0)) + "\"\r\n");
                    //sb.Append("        RelativeFilename: \"" +  @(ProPath + EditorUtility.GetAssetPath(tex0)) + "\"\r\n");
                    sb.Append("        ModelUVTranslation: 0,0\r\n");
                    sb.Append("        ModelUVScaling: 1,1\r\n");
                    sb.Append("        Texture_Alpha_Source: \"None\"\r\n");
                    sb.Append("        Cropping: 0,0,0,0\r\n");
                    sb.Append("    }\r\n");
                }
                if (mat.HasProperty("_BumpMap") && (tex0 = mat.GetTexture("_BumpMap")))
                {
                    sb.Append("    Texture: \"Texture::" + @mat.name + "_bump" + "\", \"\" {\r\n");
                    sb.Append("        Type: \"TextureVideoClip\"\r\n");
                    sb.Append("        Version: 202\r\n");
                    sb.Append("        TextureName: \"Texture::" + @mat.name + "_bump" + "\"\r\n");
                    sb.Append("        Properties60:  {\r\n");
                    sb.Append("            Property: \"TextureTypeUse\", \"enum\", \"\",0\r\n");
                    sb.Append("            Property: \"Texture alpha\", \"Number\", \"A+\",1\r\n");
                    sb.Append("            Property: \"CurrentMappingType\", \"enum\", \"\",0\r\n");
                    sb.Append("            Property: \"WrapModeU\", \"enum\", \"\",0\r\n");
                    sb.Append("            Property: \"WrapModeV\", \"enum\", \"\",0\r\n");
                    sb.Append("            Property: \"UVSwap\", \"bool\", \"\",0\r\n");
                    sb.Append("            Property: \"Translation\", \"Vector\", \"A+\"," + Convert.ToString(mat.GetTextureOffset("_BumpMap").x).Replace(",", ".") + "," + Convert.ToString(mat.GetTextureOffset("_BumpMap").y).Replace(",", ".") + ",0\r\n");
                    sb.Append("            Property: \"Rotation\", \"Vector\", \"A+\",0,0,0\r\n");
                    sb.Append("            Property: \"Scaling\", \"Vector\", \"A+\"," + Convert.ToString(mat.GetTextureScale("_BumpMap").x).Replace(",", ".") + "," + Convert.ToString(mat.GetTextureScale("_BumpMap").y).Replace(",", ".") + ",1\r\n");
                    sb.Append("            Property: \"TextureRotationPivot\", \"Vector3D\", \"\",0,0,0\r\n");
                    sb.Append("            Property: \"TextureScalingPivot\", \"Vector3D\", \"\",0,0,0\r\n");
                    sb.Append("            Property: \"UseMaterial\", \"bool\", \"\",1\r\n");
                    sb.Append("            Property: \"UseMipMap\", \"bool\", \"\",0\r\n");
                    sb.Append("            Property: \"CurrentTextureBlendMode\", \"enum\", \"\",1\r\n");
                    sb.Append("            Property: \"UVSet\", \"KString\", \"\", \"UVChannel_1\"\r\n");
                    sb.Append("        }\r\n");
                    //sb.Append("        Media: \"Video::" +@ta2[0]+"_bump" + "\"\r\n");
                    sb.Append("        FileName: \"" + (ProPath + EditorUtility.GetAssetPath(tex0)) + "\"\r\n");
                    //sb.Append("        RelativeFilename: \"" +  (ProPath + EditorUtility.GetAssetPath(tex0)) + "\"\r\n");
                    sb.Append("        ModelUVTranslation: 0,0\r\n");
                    sb.Append("        ModelUVScaling: 1,1\r\n");
                    sb.Append("        Texture_Alpha_Source: \"None\"\r\n");
                    sb.Append("        Cropping: 0,0,0,0\r\n");
                    sb.Append("    }\r\n");
                }
            }

// ######### Global Settings

            sb.Append("    GlobalSettings:  {\r\n");
            sb.Append("        Version: 1000\r\n");
            sb.Append("        Properties60:  {\r\n");
            sb.Append("            Property: \"UpAxis\", \"int\", \"\",1\r\n");
            sb.Append("            Property: \"UpAxisSign\", \"int\", \"\",1\r\n");
            sb.Append("            Property: \"FrontAxis\", \"int\", \"\",2\r\n");
            sb.Append("            Property: \"FrontAxisSign\", \"int\", \"\",1\r\n");
            sb.Append("            Property: \"CoordAxis\", \"int\", \"\",0\r\n");
            sb.Append("            Property: \"CoordAxisSign\", \"int\", \"\",1\r\n");
            sb.Append("            Property: \"OriginalUpAxis\", \"int\", \"\",2\r\n");
            sb.Append("            Property: \"OriginalUpAxisSign\", \"int\", \"\",1\r\n");
            sb.Append("            Property: \"UnitScaleFactor\", \"double\", \"\"," + LightmapAdvanced.exportScale +"\r\n");
            sb.Append("        }\r\n");
            sb.Append("    }\r\n");


            EditorUtility.DisplayProgressBar("Exporting FBX", "Writing connections...", 0.95f);
            sb.Append("}\r\n");
            sb.Append("\r\n\r\n");
            sb.Append("; Object connections\r\n");
            sb.Append(";------------------------------------------------------------------\r\n");
            sb.Append("\r\n");
            sb.Append("Connections:  {\r\n");

            for (int i = 0; i < lights.Length; i++)
            {
                sb.Append("    Connect: \"OO\", \"Model::Light" + i + "\", \"Model::Scene\"\r\n");
            }
            
            for (int obj = 0; obj < bigArray.Count; obj++)
            {
                sb.Append("    Connect: \"OO\", \"Model::Object" + Convert.ToString(obj) + "\", \"Model::Scene\"\r\n");
                foreach (Material mat in ((ArrayList)uniqueMaterialsArray[obj]))
                {
                    sb.Append("    Connect: \"OO\", \"Material::" + @mat.name + "\", \"Model::Object" + Convert.ToString(obj) + "\"\r\n");
                    if (mat.HasProperty("_MainTex") && (mat.GetTexture("_MainTex")))
                    {
                        sb.Append("    Connect: \"OO\", \"Texture::" + @mat.name + "_diff" + "\", \"Model::Object" + Convert.ToString(obj) + "\"\r\n");
                    }
                }
            }
            for (int obj = 0; obj < bigArray.Count; obj++)
            {
                foreach (Material mat in ((ArrayList)uniqueMaterialsArray[obj]))
                {
                    if (mat.HasProperty("_BumpMap") && (mat.GetTexture("_BumpMap")))
                    {
                        sb.Append("    Connect: \"OO\", \"Texture::" + @mat.name + "_bump" + "\", \"Model::Object" + Convert.ToString(obj) + "\"\r\n");
                    }
                }
            }
            sb.Append("}\r\n");
            sb.Append(FBXFooter());
            sw.Write(sb.ToString().Replace("E-0", "e-00"));
            sw.Close();
            sw.Dispose();
            EditorUtility.ClearProgressBar();

            return true;
        }
    }
    public static string FBXHeader()
    {
        return "; FBX 6.1.0 project file\r\n; Copyright (C) 1997-2008 Autodesk Inc. and/or its licensors.\r\n; All rights reserved.\r\n; ----------------------------------------------------\r\nFBXHeaderExtension:  {\r\n    FBXHeaderVersion: 1003\r\n    FBXVersion: 6100\r\n    CreationTimeStamp:  {\r\n        Version: 1000\r\n        Year: 2009\r\n        Month: 7\r\n        Day: 22\r\n        Hour: 9\r\n        Minute: 3\r\n        Second: 17\r\n        Millisecond: 354\r\n    }\r\n    Creator: \"FBX SDK/FBX Plugins version 2010.0\"\r\n    OtherFlags:  {\r\n        FlagPLE: 0\r\n    }\r\n}\r\nCreationTime: \"2009-07-22 09:03:17:354\"\r\nCreator: \"FBX SDK/FBX Plugins build 20090408\"\r\n; Document Description\r\n;------------------------------------------------------------------\r\nDocument:  {\r\n    Name: \"\"\r\n}\r\n; Document References\r\n;------------------------------------------------------------------\r\nReferences:  {\r\n}\r\n";
    }
    public static string FBXFooter()
    {
        return ";Version 5 settings\r\n;------------------------------------------------------------------\r\n\r\nVersion5:  {\r\n    AmbientRenderSettings:  {\r\n        Version: 101\r\n        AmbientLightColor: 0,0,0,1\r\n    }\r\n\r\n    FogOptions:  {\r\n        FogEnable: 0\r\n        FogMode: 0\r\n        FogDensity: 0.002\r\n        FogStart: 0.3\r\n        FogEnd: 1000\r\n        FogColor: 1,1,1,1\r\n    }\r\n    Settings:  {\r\n        FrameRate: \"30\"\r\n        TimeFormat: 1\r\n        SnapOnFrames: 0\r\n        ReferenceTimeIndex: -1\r\n        TimeLineStartTime: 0\r\n        TimeLineStopTime: 153953860000\r\n    }\r\n    RendererSetting:  {\r\n        DefaultCamera: \"Producer Perspective\"\r\n        DefaultViewingMode: 0\r\n    }\r\n}\r\n";
    }

}

