using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System;

public class LookForLightmaps : AssetPostprocessor
{

    // Use this for initialization
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (LightmappingTool.startLooking || LightmapAdvanced.lookAllTheTime)
        {

            LightmapData[] data = new LightmapData[LightmappingTool.numberOfLightmaps];
            for (int i = 0; i < LightmappingTool.numberOfLightmaps; i++)
            {
                data[i] = new LightmapData();
               
            }
            try
            {
                foreach (string f in Directory.GetFiles(Application.dataPath + LightmapAdvanced.LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene))))
                {

                    for (int i = 0; i < LightmappingTool.numberOfLightmaps; i++)
                    {
                        if (Path.GetFileName(f) == "lightmap" + i + LightmapAdvanced.fileFormat[LightmapAdvanced.formatChoice])
                        {
                            Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(f.Substring(Application.dataPath.Length - 6), typeof(Texture2D));
                            data[i].lightmap = tex;
                            Debug.Log("Lighmap " + i + " just got loaded");
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                if (LightmapAdvanced.LMdir[0] != Convert.ToChar("/")) LightmapAdvanced.LMdir = "/" + LightmapAdvanced.LMdir;
                System.IO.Directory.CreateDirectory(Application.dataPath + LightmapAdvanced.LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene)));
            }
            LightmapSettings.lightmaps = data;
            
        }
        LightmappingTool.startLooking = false;
        
    }

}
