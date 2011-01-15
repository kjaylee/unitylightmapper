using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System;

/*
    Coded for Unity Summer of Code 2009 by Michal Mandrysz
    Feel free to modify for your own needs
    http://masteranza.wordpress.com/unity/lightmapping/
    http://unity3d.com
*/

public class LookForLightmaps : AssetPostprocessor
{

    // Use this for initialization
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        LightmappingTool.LoadObjects();
        if (LightmappingTool.window!=null) LightmappingTool.window.Repaint();

        bool Found=false;
        foreach (string i in importedAssets)
        {
            if (i.Contains("lightmap")){
                Found=true;
                break;
            }
        }
        if (!Found) return;

        ArrayList loaded = new ArrayList();

        LightmapData[] data = new LightmapData[LightmappingTool.numberOfLightmaps];
        for (int i = 0; i < LightmappingTool.numberOfLightmaps; i++)
        {
            data[i] = new LightmapData();
           
        }
        try
        {
            foreach (string f in Directory.GetFiles(Application.dataPath + LightmappingTool.LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene))))
            {
                for (int i = 1; i < LightmappingTool.numberOfLightmaps+1; i++)
                {
                    if (Path.GetFileName(f) == "lightmap" + i + LightmappingTool.fileFormat)
                    {
                        Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(f.Substring(Application.dataPath.Length - 6), typeof(Texture2D));
                        data[i-1].lightmapFar = tex;
                        loaded.Add(i);
                    }
                }
            }
            string container= "";
            foreach (int i in loaded){
                container+=""+i+", ";
            }
            try
			{
				container= container.Substring(0, container.Length - 2);
			}
			catch{}
            if (container.Length > 0)
            {
                
                Debug.Log("At " + String.Format("{0:T}", DateTime.Now) + " following lightmaps got loaded: " + container);
            }
        }
        catch (DirectoryNotFoundException)
        {
            if (LightmappingTool.LMdir[0] != Convert.ToChar("/")) LightmappingTool.LMdir = "/" + LightmappingTool.LMdir;
            System.IO.Directory.CreateDirectory(Application.dataPath + LightmappingTool.LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene)));
        }
        LightmapSettings.lightmaps = data;   
    }
}
