using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;

public class LightmapAdvanced : EditorWindow
{
    static public bool exportLight = true;
    static public string tagged = "Untagged";
    static public float exportScale = 100.0f;
    static public float lightMultipler = 1.0f;
    static public int defaultRes = 4;

    static public ArrayList appPaths = new ArrayList();
    static public string[] appSelect = new string[1] { "None, just export"};
    static public int selectedApp = 0;

    static public string tmp;
    static public string LMdir = @"/Lightmaps/<sceneName>/";  //always relative to Assets
    static public bool autostartBaking = false;
    static public int presetFile = 0;
    static public bool matLibFile = true;
    
    static public string[] renderers = new string[7]{"Don't change","Default Scanline","Mental Ray","Vray", "Final Render","Brazil","Maxwell"};
    static public string[] renderersSearch = new string[7] { "", "Scanline", "mental", "V_Ray", "Final", "Brazil", "Maxwell" };
    static public int rendererChoice = 0;
    static public string[] fileFormat = new string[4] { ".dds", ".png", ".tga", ".jpg" };
    static public int formatChoice = 0;
    static public Vector2 offsetVector = new Vector2(0,0);
    static public Vector2 temp;
    static public int padding = 5;
    static public string[] presetArray;
    static public string[] presetArrayNames;
    static public bool lookAllTheTime = true;

    
    static void loadApps()
    {
        if (EditorPrefs.HasKey("LMT_InstalledApps"))
        {
            string apps = EditorPrefs.GetString("LMT_InstalledApps");
            if (apps != "")
            {
                string[] array = apps.Split(new char[1] { Convert.ToChar("|") });
                appPaths = new ArrayList();
                appSelect = new string[array.Length + 1];
                appSelect[0]="None, just export";
                for(int i=0;i<array.Length;i++)
                {
                    appPaths.Add(array[i]);
                    appSelect[i + 1] = "" + (i + 1) + ". " + Path.GetFileNameWithoutExtension(array[i]);
                }
            }
        }
    }
    static void saveApps()
    {
        string toSave="";
        if (appPaths.Count>0)
        {
            foreach(string i in appPaths){
                toSave+=i+"|";
            }
            toSave.Substring(0, toSave.Length - 1);
        }
        EditorPrefs.SetString("LMT_InstalledApps", toSave);
    }

    public static void Init()
    {
        if (Path.GetFileNameWithoutExtension(EditorApplication.currentScene).Length == 0)
        {
            EditorUtility.DisplayDialog("Please save your scene first", "Information", "OK");
            return;
        }
        try
        {
            if (!System.IO.Directory.Exists(Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "\\MaxFiles"))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "\\MaxFiles");
            }
        }
        catch
        {
            UnityEngine.Debug.LogError("Failed creating 'MaxFiles' directory in the project folder");
            return;
        }
        try
        {
            if (LightmapAdvanced.LMdir[0] != System.Convert.ToChar("/")) LightmapAdvanced.LMdir = "/" + LightmapAdvanced.LMdir;
            if (!System.IO.Directory.Exists(Application.dataPath + LightmapAdvanced.LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene))))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + LightmapAdvanced.LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene)));
            }
        }
        catch
        {
            UnityEngine.Debug.LogError("Failed creating a specifed directory for lightmaps");
            return;
        }

        ArrayList toRemove = new ArrayList();
        
        loadApps();
        StorePreferences.Load();
        foreach (string i in appPaths){
            if (!System.IO.File.Exists(i))
            {
                toRemove.Add(i);
            }
        }
        foreach (string i in toRemove)
        {
            appPaths.Remove(i);
        }
        if (appPaths.Count + 1 != appSelect.Length)
        {
            appSelect = new string[appPaths.Count + 1];
            appSelect[0] = "None, just export";
            for (int i = 0; i < appPaths.Count; i++)
            {
                appSelect[i + 1] = "" + (i + 1) + ". " + Path.GetFileNameWithoutExtension((string)appPaths[i]);
            }
            saveApps();

        }

        searchForPresets();
        LightmapAdvanced window = (LightmapAdvanced)EditorWindow.GetWindow(typeof(LightmapAdvanced));
        window.Show();
    }

    public static void searchForPresets()
    {
        presetArray = System.IO.Directory.GetFiles(Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "MaxFiles/", Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "_*.max");
        if (presetArray != null)
        {
            presetArrayNames = new string[presetArray.Length + 1];
            presetArrayNames[0] = "No preset";
            for (int i = 0; i < presetArray.Length; i++)
            {
                presetArrayNames[i + 1] = Path.GetFileNameWithoutExtension(presetArray[i]);
            }
        }
        else
        {
            presetArrayNames = new string[1];
            presetArrayNames[0] = "No preset";
        }
        if (presetFile > presetArray.Length) presetFile = 0;
        //matLibArray = new string[files.Length + 1];
        //presetArray[0] = "None";
    }

    void OnLostfFocus()
    {
        StorePreferences.Save();
    }
    void OnDestroy()
    {
        StorePreferences.Save();
    }
    void OnGUI()
    {
        
        //Main Export options
        if (EditorGUILayout.Foldout(true, "Main Export options: ", "BoldLabel"))
        {
            exportScale = EditorGUILayout.Slider("Export scale", exportScale, 0.1f, 1000.0f);
            exportLight = EditorGUILayout.BeginToggleGroup("Export Unity Lights", exportLight);
            tagged = EditorGUILayout.TagField("Tagged: ",tagged);
            lightMultipler = EditorGUILayout.Slider("Light Multipler", lightMultipler, 0.1f, 1000.0f);
            EditorGUILayout.EndToggleGroup();
        }
        
        EditorGUILayout.Separator();


        //External tool options
        if (EditorGUILayout.Foldout(true, "External Tool options: ", "BoldLabel"))
        {
            EditorGUILayout.BeginHorizontal();
            selectedApp = EditorGUILayout.Popup("Open with: ", selectedApp, appSelect);
            if (selectedApp != 0) LightmappingTool.haveBeenExported = false;
            if (GUILayout.Button("Add 3dsmax", GUILayout.MaxWidth(200))) { addApp("3dsmax"); }
            //If you'd like to extend this software add a button here and then go and search for ^_^ there will be some stuff to do too ;)

            EditorGUILayout.EndHorizontal();

            if (appSelect[selectedApp].Contains("3dsmax"))
            {
                EditorGUILayout.BeginHorizontal();
                presetFile = EditorGUILayout.Popup("Choose a preset", presetFile, presetArrayNames);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (presetFile == 0)
                {
                    rendererChoice = EditorGUILayout.Popup("Renderer: ", rendererChoice, renderers);
                    matLibFile = EditorGUILayout.Toggle("Load MatLib", matLibFile);
                }
                autostartBaking = EditorGUILayout.Toggle("Autostart baking", autostartBaking);
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Separator();



        //Lightmap Textures options
        if (EditorGUILayout.Foldout(true, "Lightmap Textures options: ", "BoldLabel"))
        {
            EditorGUILayout.BeginHorizontal();
            
            LMdir = EditorGUILayout.TextField(new GUIContent("Folder", "Path realtive to assets folder. You can use <sceneName> to make a naming scheme for whole project."), LMdir, GUILayout.Width(this.position.width-140));

            lookAllTheTime = GUILayout.Toggle(lookAllTheTime, "Monitor all the time");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            defaultRes = EditorGUILayout.Popup("Resolution", defaultRes, LightmappingTool.resolutions);
            if (GUILayout.Button("Apply to all", GUILayout.MaxWidth(170)))
            {
                LightmappingTool.loadResArray();
                StorePreferences.SaveMain();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            formatChoice = EditorGUILayout.Popup("Format", formatChoice, fileFormat, GUILayout.MaxWidth(200));
            padding = EditorGUILayout.IntSlider(new GUIContent("Padding", "The distanse between packed textures"), padding, 0, 15);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Separator();



        if (EditorGUILayout.Foldout(true, "Correction tools: ", "BoldLabel"))
        {
            if (offsetVector != (temp = EditorGUILayout.Vector2Field("Offset field", offsetVector)))
            {
                Vector4 tmp;
                foreach (Transform i in (ArrayList)LightmappingTool.allData[LightmappingTool.currentLightmap])
                {
                    tmp = i.renderer.lightmapTilingOffset;
                    i.renderer.lightmapTilingOffset = new Vector4(tmp.x, tmp.y, tmp.z - (offsetVector.x - temp.x) * 0.01f, tmp.w - (offsetVector.y - temp.y) * 0.01f);
                }
                offsetVector = temp;
            }
        }
    }

    private void addApp(string p)
    {
        string toAdd = EditorUtility.OpenFilePanel("Point the 3dsmax application", Application.dataPath, "exe");
        LightmapAdvanced window = (LightmapAdvanced)EditorWindow.GetWindow(typeof(LightmapAdvanced));
        window.Show();
        if (toAdd.Length != 0)
        {
            if (appPaths.Count > 0)
            {
                if (appPaths.Contains(toAdd)) return;
            }
            appPaths.Add(toAdd);
            appSelect = new string[appPaths.Count + 1];
            appSelect[0] = "None, just export";
            for (int i = 0; i < appPaths.Count; i++)
            {
                appSelect[i + 1] = "" + (i + 1) + ". " + Path.GetFileNameWithoutExtension((string)appPaths[i]);
            }
            selectedApp = appSelect.Length - 1;
            saveApps();
        }
        else
        {
            return;
        }

    }
}