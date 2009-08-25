using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
class StorePreferences
{
    /*  Things to save:
     * [project specific (not saved here)]
     * installedApplications
     * 
     * [scene specific]
     *  run
     *  resolution
     *  export scale
     *  export lights
     *  tag
     *  light multipler
     *  open with
     *  monitor status
     *  folder
     *  format
     *  padding
     */
    static string sceneName = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);

    static public void Load()
    {
        if (sceneName.Length > 0)
        {

            if (EditorPrefs.HasKey("LMT_" + sceneName + "_resolutions"))
            {
                string temp = EditorPrefs.GetString("LMT_" + sceneName + "_resolutions", "nothing");
                if (temp != "nothing")
                {
                    string[] array = temp.Split(new char[1] { System.Convert.ToChar("|") });
                    LightmappingTool.res = new ArrayList();
                    array.ToString();
                    foreach (string i in array)
                    {
                        try
                        {
                            LightmappingTool.res.Add(System.Convert.ToInt32(i));
                        }
                        catch
                        {
                            //Debug.Log(i.ToString());
                            //Debug.Log(i + "cannot be converted to int");
                            LightmappingTool.res.Add(LightmappingTool.resolutions[LightmapAdvanced.defaultRes]);
                        }
                    }
                }
            }
            if (EditorPrefs.HasKey("LMT_" + sceneName + "_run"))
            {
                LightmappingTool.run = EditorPrefs.GetBool("LMT_" + sceneName + "_run", false);
            }

            if (EditorPrefs.HasKey("LMT_" + sceneName + "_scale"))
            {
                LightmapAdvanced.exportScale = EditorPrefs.GetFloat("LMT_" + sceneName + "_scale", 100.0f);
            }

            if (EditorPrefs.HasKey("LMT_" + sceneName + "_exportLights"))
            {
                LightmapAdvanced.exportLight = EditorPrefs.GetBool("LMT_" + sceneName + "_exportLights", true);
            }

            if (EditorPrefs.HasKey("LMT_" + sceneName + "_lightTag"))
            {
                LightmapAdvanced.tagged = EditorPrefs.GetString("LMT_" + sceneName + "_lightTag", "Untagged");
                
            }

            if (EditorPrefs.HasKey("LMT_" + sceneName + "_lightMultipler"))
            {
                LightmapAdvanced.lightMultipler = EditorPrefs.GetFloat("LMT_" + sceneName + "_lightMultipler", 1.0f);
            }

            if (EditorPrefs.HasKey("LMT_" + sceneName + "_openWith"))
            {
                LightmapAdvanced.selectedApp = EditorPrefs.GetInt("LMT_" + sceneName + "_openWith", 0);
            }

            if (EditorPrefs.HasKey("LMT_" + sceneName + "_monitorAllTheTime"))
            {
                LightmapAdvanced.lookAllTheTime= EditorPrefs.GetBool("LMT_" + sceneName + "_monitorAllTheTime", true);
            }
            if (EditorPrefs.HasKey("LMT_" + sceneName + "_folder"))
            {
                string folder= EditorPrefs.GetString("LMT_" + sceneName + "_folder", "nothing");
                if (folder != "nothing")
                {
                    LightmapAdvanced.LMdir = folder;
                }
            }
            if (EditorPrefs.HasKey("LMT_" + sceneName + "_format"))
            {
                LightmapAdvanced.formatChoice = EditorPrefs.GetInt("LMT_" + sceneName + "_format", 0);
            }
            if (EditorPrefs.HasKey("LMT_" + sceneName + "_padding"))
            {
                LightmapAdvanced.padding = EditorPrefs.GetInt("LMT_" + sceneName + "_padding", 5);
            }

        }
    }
    static public void SaveMain()
    {
        if (sceneName.Length > 0)
        {
            EditorPrefs.SetBool("LMT_" + sceneName + "_run", LightmappingTool.run);
            LightmappingTool.res.ToString();

            string toSave = "";
            if (LightmappingTool.res.Count > 0)
            {
               
                foreach (System.Object i in LightmappingTool.res)
                {
                    toSave += System.Convert.ToString(i) + "|";
                }
                toSave.Substring(0, toSave.Length - 1);
            }

            EditorPrefs.SetString("LMT_" + sceneName + "_resolutions", toSave);
        }

    }

    static public void Save()
    {
        if (sceneName.Length > 0)
        { 
            EditorPrefs.SetFloat("LMT_" + sceneName + "_scale", LightmapAdvanced.exportScale);
            EditorPrefs.SetBool("LMT_" + sceneName + "_exportLights", LightmapAdvanced.exportLight );
            EditorPrefs.SetString("LMT_" + sceneName + "_lightTag", LightmapAdvanced.tagged);
            EditorPrefs.SetFloat("LMT_" + sceneName + "_lightMultipler", LightmapAdvanced.lightMultipler);
            EditorPrefs.SetInt("LMT_" + sceneName + "_openWith", LightmapAdvanced.selectedApp);
            EditorPrefs.SetBool("LMT_" + sceneName + "_monitorAllTheTime", LightmapAdvanced.lookAllTheTime);
            EditorPrefs.SetString("LMT_" + sceneName + "_folder", LightmapAdvanced.LMdir);
            EditorPrefs.SetInt("LMT_" + sceneName + "_format", LightmapAdvanced.formatChoice);
            EditorPrefs.SetInt("LMT_" + sceneName + "_padding", LightmapAdvanced.padding);
        }
    }
}
