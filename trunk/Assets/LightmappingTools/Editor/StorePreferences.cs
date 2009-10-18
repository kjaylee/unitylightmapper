using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;


/*
    Coded for Unity Summer of Code 2009 by Michal Mandrysz
    Feel free to modify for your own needs
    http://masteranza.wordpress.com/unity/lightmapping/
    http://unity3d.com
*/

class StorePreferences
{
    /*  Things to save:
     * [global info]
     * installedApplications
     * 
     * [scene specific]
     *  resolution
     *  export scale
     *  export lights
     *  tag
     *  light multipler
     *  open with
     *  padding
     */
    static string projectName = GetProjectName();


    static string GetProjectName()
    {
        DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
        string projectname = d.Name;
        return projectname;
    }


    static public void Load()
    {
        if (EditorPrefs.HasKey("LMT_InstalledApps"))
        {
            string apps = EditorPrefs.GetString("LMT_InstalledApps");
            if (apps != "")
            {
                string[] array = apps.Split(new char[1] { System.Convert.ToChar("|") });
                LightmappingTool.appPaths = new ArrayList();
                LightmappingTool.appSelect = new string[array.Length + 1];

                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] != "")
                    {
                        LightmappingTool.appPaths.Add(array[i]);
                        string[] name = ((string)LightmappingTool.appPaths[i]).Split(new char[1] { System.Convert.ToChar("/") });
                        LightmappingTool.appSelect[i] = "" + (i + 1) + ". " + name[name.Length-2] + "\\" + name[name.Length-1] + " " + ((((string)LightmappingTool.appPaths[i]).Contains("x86")) ? "(32-bit)" : "");
                    }
                }
                LightmappingTool.appSelect[array.Length] = "Add application";
            }
        }
        if (projectName.Length > 0)
        {
            
            if (EditorPrefs.HasKey("LMT_" + projectName + "_resolutions"))
            {
                string temp = EditorPrefs.GetString("LMT_" + projectName + "_resolutions", "nothing");
                if (temp != "nothing")
                {
                    string[] array = temp.Split(new char[1] { System.Convert.ToChar("|") });
                    LightmappingTool.res = new ArrayList();
                    
                    foreach (string i in array)
                    {
                        
                        try
                        {
                            LightmappingTool.res.Add(System.Convert.ToInt32(i));
                        }
                        catch
                        {
                            LightmappingTool.res.Add(LightmappingTool.defaultRes);
                        }
                    }
                }
            }
            
            if (EditorPrefs.HasKey("LMT_" + projectName + "_scale"))
            {
                LightmappingTool.exportScale = EditorPrefs.GetFloat("LMT_" + projectName + "_scale", 100.0f);
            }

            if (EditorPrefs.HasKey("LMT_" + projectName + "_exportLights"))
            {
                LightmappingTool.exportLight = EditorPrefs.GetBool("LMT_" + projectName + "_exportLights", true);
            }

            if (EditorPrefs.HasKey("LMT_" + projectName + "_lightTag"))
            {
                LightmappingTool.tagged = EditorPrefs.GetString("LMT_" + projectName + "_lightTag", "Untagged");
                
            }

            if (EditorPrefs.HasKey("LMT_" + projectName + "_lightMultipler"))
            {
                LightmappingTool.lightMultipler = EditorPrefs.GetFloat("LMT_" + projectName + "_lightMultipler", 1.0f);
            }

            if (EditorPrefs.HasKey("LMT_" + projectName + "_openWith"))
            {
                LightmappingTool.selectedApp = EditorPrefs.GetInt("LMT_" + projectName + "_openWith", 0);
            }
            if (EditorPrefs.HasKey("LMT_" + projectName + "_padding"))
            {
                LightmappingTool.padding = EditorPrefs.GetInt("LMT_" + projectName + "_padding", 5);
            }

        }
    }

    
    static public void Save()
    {
        string toSave = "";
        if (LightmappingTool.appPaths.Count > 0)
        {
            foreach (string i in LightmappingTool.appPaths)
            {
                toSave += i + "|";
            }
            toSave.Substring(0, toSave.Length - 1);
        }
        EditorPrefs.SetString("LMT_InstalledApps", toSave);
        
        toSave = "";
        if (projectName.Length > 0)
        {   
            
            if (LightmappingTool.res.Count > 0)
            {
                foreach (System.Object i in LightmappingTool.res)
                {
                    toSave += System.Convert.ToString(i) + "|";
                }
                toSave.Substring(0, toSave.Length - 1);
            }
             
            EditorPrefs.SetString("LMT_" + projectName + "_resolutions", toSave);
            EditorPrefs.SetFloat("LMT_" + projectName + "_scale", LightmappingTool.exportScale);
            EditorPrefs.SetBool("LMT_" + projectName + "_exportLights", LightmappingTool.exportLight );
            EditorPrefs.SetString("LMT_" + projectName + "_lightTag", LightmappingTool.tagged);
            EditorPrefs.SetFloat("LMT_" + projectName + "_lightMultipler", LightmappingTool.lightMultipler);
            EditorPrefs.SetInt("LMT_" + projectName + "_openWith", LightmappingTool.selectedApp);
            EditorPrefs.SetInt("LMT_" + projectName + "_padding", LightmappingTool.padding);
        }
    }
    static public void Reset()
    {
        EditorPrefs.DeleteAll();
    }
}
