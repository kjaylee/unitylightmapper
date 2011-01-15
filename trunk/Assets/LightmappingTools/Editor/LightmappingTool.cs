using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/*
    Coded for Unity Summer of Code 2009 by Michal Mandrysz
    Feel free to modify for your own needs
    http://masteranza.wordpress.com/unity/lightmapping/
    http://unity3d.com
*/

public class LightmappingTool : EditorWindow
{
    static public string defaultShader = "Assets/LightmappingTools/ExternalLightmappingTool-Diffuse.shader";
    
    
    
    static public bool debugMode = false;
    //possible resolution array, adding new will effect in higher possible resolution
    static public string[] resolutions = new string[6] { "128x128", "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" };
    //you may change this to change the lightmap format output 
    static public string fileFormat = ".tif";
    
    //if you don't like the logo taking your precious screen space, then don't throw this one out
    //but throw logo.png instead.
    Texture logo = (Texture) AssetDatabase.LoadAssetAtPath("Assets/LightmappingTools/logo.png", typeof(Texture2D));
   
    static public ArrayList allData = new ArrayList();
    static public ArrayList bigArray;
    static public ArrayList res = new ArrayList();
    static public int numberOfLightmaps = 1;
    static public int currentLightmap = 0;
    static public string[] lightmapArray = new string[] { "Lightmap 1" };
    static public string[] compatibleShaders;
    static public Vector2 scrollValue = new Vector2();
    static public bool run = true;
    static public string MaxFiles = (Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "MaxFiles/");
    bool showAdvanced = false;
    static public bool exportLight = true;
    static public string tagged = "Untagged";
    static public float exportScale = 100.0f;
    static public float lightMultipler = 1.0f;
    static public int defaultRes = 4;
    static public ArrayList appPaths = new ArrayList();
    static public string[] appSelect = new string[1] { "Add application" };
    static public int selectedApp = 0;
    static public bool shadersLoaded = false;
    static public string tmpstring;
    static public string LMdir = @"/Lightmaps/<sceneName>/";  //always relative to Assets
    static public int padding = 5;
    static List<Shader> shaderBuffer;
    static public Vector2 offsetVector = new Vector2(0, 0);
    static public Vector2 temp;
    static string sceneName="";

    static public LightmappingTool window;

    static ArrayList cantOpen = new ArrayList();


    static public ArrayList notNormalized2 = new ArrayList();
    static public ArrayList overlapping2 = new ArrayList();
    static public ArrayList notNormalized = new ArrayList();
    static public ArrayList overlapping = new ArrayList();
    static public ArrayList usingFirstUV = new ArrayList();
    static public ArrayList noUVs = new ArrayList();

    int tmp = 1;
    static GUIContent[] mainToolbarText = new GUIContent[2]{new GUIContent("Add lightmap", "Adds a lightmap at the end of the list"), 
                                                      new GUIContent("Remove current lightmap", "Removes current lightmap and all its objects")};

    [MenuItem("Window/External Lightmapping tool")]
    
    static void Init()
    {
        LoadObjects();
        window = (LightmappingTool)EditorWindow.GetWindow(typeof(LightmappingTool));
        window.title = "External Lightmapping tool";
        window.position = new Rect(100.0f, 100.0f, 315.0f, 400.0f);
        window.minSize = new Vector2(315.0f, 400.0f);
        window.Show();

    }

    private static void LoadCurrentObjects()
    {
        allData[currentLightmap] = new ArrayList();

        Renderer[] objects = (Renderer[])Object.FindSceneObjectsOfType(typeof(Renderer));
        foreach (Renderer rn in objects)
        {
            if (rn.lightmapIndex > -1 && rn.sharedMaterial != null)
            {
                if (rn.lightmapIndex==currentLightmap)
                {
                    if (((ArrayList)allData[currentLightmap]).Count > 0)
                    {
                        if (!((ArrayList)allData[rn.lightmapIndex]).Contains(rn.transform))
                        {
                            ((ArrayList)allData[rn.lightmapIndex]).Add(rn.transform);
                        }
                    }
                    else
                    {
                        ((ArrayList)allData[rn.lightmapIndex]).Add(rn.transform);
                    }
                }
            }
        }
    }


    public static void LoadObjects()
    {
        allData = new ArrayList();
        allData.Add(new ArrayList());
        notNormalized = new ArrayList();
        notNormalized2 = new ArrayList();
        noUVs = new ArrayList();
        usingFirstUV = new ArrayList();
        if (overlapping.Count > 0)
        {
            for (int i = 1; i < overlapping.Count; i += 3)
            {
                UnityEngine.Object.DestroyImmediate((Texture2D)overlapping[i]);
            }
        }
        overlapping = new ArrayList();

        if (overlapping2.Count > 0)
        {
            for (int i = 1; i < overlapping2.Count; i += 3)
            {
                UnityEngine.Object.DestroyImmediate((Texture2D)overlapping2[i]);
            }
        }
        overlapping2 = new ArrayList();

        System.GC.Collect();

        if (res == null || res.Count < 1)
        {
            LoadResArray();
        }

        Renderer[] objects = (Renderer[])Object.FindSceneObjectsOfType(typeof(Renderer));
        foreach (Renderer rn in objects)
        {
            if (rn.lightmapIndex > -1 && rn.sharedMaterial != null)
            {
                while (rn.lightmapIndex >= allData.Count)
                {
                    allData.Add(new ArrayList());
                }
                if (((ArrayList)allData[rn.lightmapIndex]).Count > 0)
                {
                    if (!((ArrayList)allData[rn.lightmapIndex]).Contains(rn.transform))
                    {
                        MeshFilter component = (MeshFilter)rn.GetComponent(typeof(MeshFilter));
                        if (component.sharedMesh != null)
                        {
                            CalcArea.CheckIfNormalized((MeshFilter)rn.GetComponent(typeof(MeshFilter)));
                            ((ArrayList)allData[rn.lightmapIndex]).Add(rn.transform);
                        }
                        else
                        {
                            rn.lightmapIndex = -1;
                        }
                    }
                }
                else
                {
                    MeshFilter component = (MeshFilter)rn.GetComponent(typeof(MeshFilter));
                    if (component.sharedMesh != null)
                    {
                        CalcArea.CheckIfNormalized((MeshFilter)rn.GetComponent(typeof(MeshFilter)));
                        ((ArrayList)allData[rn.lightmapIndex]).Add(rn.transform);
                    }
                    else
                    {
                        rn.lightmapIndex = -1;
                    }
                }

            }
        }
        numberOfLightmaps = allData.Count;
        lightmapArray = LightmapGenerator(numberOfLightmaps);
    }

    private static void LoadShaders()
    {
        //Load shaders
        Object[] shaders = Resources.FindObjectsOfTypeAll(typeof(Shader));

        shaderBuffer = new List<Shader>();
        ArrayList temp = new ArrayList();
        temp.Add("Not compatible");
        shaderBuffer.Add(null);
        foreach (Object i in shaders)
        {
            if (debugMode) UnityEngine.Debug.Log(i.name);
            if (i.name.Contains("Lightmapped"))
            {
                temp.Add(i.name);
                shaderBuffer.Add((Shader)i);
            }
        }
        try
        {
            Shader tmp = (Shader)AssetDatabase.LoadAssetAtPath("Assets/LightmappingTools/ExternalLightmappingTool-Diffuse.shader", typeof(Shader));
            if (!temp.Contains(tmp.name))
            {
                temp.Add(tmp.name);
                shaderBuffer.Add(tmp);
            }
            tmp = (Shader)AssetDatabase.LoadAssetAtPath("Assets/LightmappingTools/ExternalLightmappingTool-VertexLit.shader", typeof(Shader));
            if (!temp.Contains(tmp.name))
            {
                temp.Add(tmp.name);
                shaderBuffer.Add(tmp);
            }
            tmp = (Shader)AssetDatabase.LoadAssetAtPath("Assets/LightmappingTools/ExternalLightmappingTool-VertexLitAlpha.shader", typeof(Shader));
            if (!temp.Contains(tmp.name))
            {
                temp.Add(tmp.name);
                shaderBuffer.Add(tmp);
            }
            tmp = (Shader)AssetDatabase.LoadAssetAtPath("Assets/LightmappingTools/ExternalLightmappingTool-BumpedDiffuse.shader", typeof(Shader));
            if (!temp.Contains(tmp.name))
            {
                temp.Add(tmp.name);
                shaderBuffer.Add(tmp);
            }
            tmp = (Shader)AssetDatabase.LoadAssetAtPath("Assets/LightmappingTools/ExternalLightmappingTool-BumpedSpecular.shader", typeof(Shader));
            if (!temp.Contains(tmp.name))
            {
                temp.Add(tmp.name);
                shaderBuffer.Add(tmp);
            }
            tmp = (Shader)AssetDatabase.LoadAssetAtPath("Assets/LightmappingTools/ExternalLightmappingTool-BumpedSpecularAlpha.shader", typeof(Shader));
            if (!temp.Contains(tmp.name))
            {
                temp.Add(tmp.name);
                shaderBuffer.Add(tmp);
            }
        }
        catch
        {
            UnityEngine.Debug.Log("Couldn't find the modified Lightmapped shaders in the LightmappingTools directory");
        }
        compatibleShaders = (string[])temp.ToArray(typeof(string));
        shadersLoaded = true;
    }
   
    static string[] LightmapGenerator(int number)
    {
        string[] LMArray = new string[number];
        for (int i = 0; i < number; i++)
        {
            LMArray[i] = "Lightmap " + (i + 1);
        }
        return LMArray;
    }

    static public void LoadResArray()
    {
        res = new ArrayList();
        for (int i = 0; i < 99; i++)
        {
            res.Add(LightmappingTool.defaultRes);
        }
    }
    void OnEnable()
    {
        cantOpen = new ArrayList();
        cantOpen.Add(".fbx");
        cantOpen.Add(".dae");
        cantOpen.Add(".notafile");
        LoadObjects();
        StorePreferences.Load();
    }

    void OnGUI()
    {
        
        if (sceneName != Path.GetFileNameWithoutExtension(EditorApplication.currentScene))
        {
            sceneName = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
            LoadObjects();
        }
        if (!shadersLoaded)
        {
            LoadShaders();
        }
         
        if (currentLightmap > numberOfLightmaps) currentLightmap = 0;
        if (res == null || res.Count < numberOfLightmaps) LoadObjects();

        Rect dropZone = new Rect(0, 85 + (logo != null ? logo.height : 0) + 20 * ((numberOfLightmaps - 1) / 3), this.position.width, this.position.height - (logo != null ?  logo.height : 0) - (showAdvanced ? 290 + (debugMode ? 40 : 0) : 200) - 20 * ((numberOfLightmaps - 1) / 3));
        GUI.Box(dropZone, "");
        GUILayout.BeginHorizontal();
        if (logo!=null) GUILayout.Label(logo);
        GUI.color = new Color(0.75f, 0.9f, 0.75f);
        if (GUILayout.Button("Help",GUILayout.Width(38))){
            Application.OpenURL("http://masteranza.wordpress.com/unity/lightmapping/");
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        tmp = GUILayout.Toolbar(-1, mainToolbarText);
        EditorGUILayout.Separator();
        if (tmp == 0)
        {
            numberOfLightmaps++;
            lightmapArray = LightmapGenerator(numberOfLightmaps);
            try
            {
                res[numberOfLightmaps - 1] = res[currentLightmap];
            }
            catch
            {
                res.Add(res[currentLightmap]);
            }
        }
        if (tmp == 1)
        {
            if (numberOfLightmaps > 1) numberOfLightmaps--;
            ClearList();
            allData.RemoveAt(currentLightmap);
            lightmapArray = LightmapGenerator(numberOfLightmaps);
            res.RemoveAt(currentLightmap);
        }

		if ((tmp = GUILayout.SelectionGrid(currentLightmap, lightmapArray, 3)) != currentLightmap)
		{
			currentLightmap = tmp;
            LoadCurrentObjects();
		}

		EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        if ((tmp = EditorGUILayout.Popup("Resolution:", (int)res[currentLightmap], resolutions)) != (int)res[currentLightmap])
        {
            res[currentLightmap] = tmp;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal("toolbar");
        GUILayout.Label("Object", "toolbarButton", GUILayout.Width(this.position.width - 200));
        GUILayout.Label("Shader", "toolbarButton");
        EditorGUILayout.EndHorizontal();

        Rect dropOnLM = new Rect(0, 40+ (logo != null ? logo.height: 0), this.position.width, 16 + 20 * ((numberOfLightmaps - 1) / 3));
        if ( Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;

            if (Event.current.type == EventType.DragPerform)
            {
                if (dropOnLM.Contains(Event.current.mousePosition))
                {
                    int coordy = System.Convert.ToInt16(System.Math.Floor((Event.current.mousePosition.y-dropOnLM.y) / 20));
                    int coordx = System.Convert.ToInt16(System.Math.Floor(Event.current.mousePosition.x / (dropOnLM.width/3)));
                    if (debugMode) UnityEngine.Debug.Log("Drop on lightmap: " + (coordy * 3 + coordx));
                    AddObjects((coordy * 3 + coordx), DragAndDrop.objectReferences);                    
                }
                else if (dropZone.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.AcceptDrag();
                    AddObjects(currentLightmap, DragAndDrop.objectReferences);
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.None;
                }
            }
            Event.current.Use();
        }



        int tmps;        
        scrollValue = EditorGUILayout.BeginScrollView(scrollValue);
        if (numberOfLightmaps > allData.Count)
        {
            for (int i = 0; i < numberOfLightmaps - allData.Count + 1; i++)
            {
                allData.Add(new ArrayList());
            }
        }
        bool incompatible = false;
        if (allData.Count > 0){
            try
            {
				if (currentLightmap>=allData.Count) currentLightmap = allData.Count -1;
                if (((ArrayList)allData[currentLightmap]).Count > 0)
                {

                    ArrayList toRemove = new ArrayList();
                    foreach (Transform tr in ((ArrayList)allData[currentLightmap]))
                    {
                        if (tr != null)
                        {
                            
                            Rect p = EditorGUILayout.BeginVertical();
                            int displayMessage = -1;
                            int indexInside;
                            if ((indexInside =overlapping2.IndexOf(tr))!=-1) displayMessage=0;
                            else if ((indexInside =notNormalized2.IndexOf(tr))!=-1) displayMessage=1;
                            else if ((indexInside =overlapping.IndexOf(tr))!=-1) displayMessage=2;
                            else if ((indexInside = notNormalized.IndexOf(tr)) != -1) displayMessage = 3;
                            //else if (usingFirstUV.Contains(tr)) displayMessage = 4;
                            else if ((indexInside=noUVs.IndexOf(tr))!=-1) displayMessage = 4;

                            if (displayMessage > -1)
                            {
                                p.y -= 2;
                                GUI.color = new Color(0.9f, 0.7f, 0.7f);
                                GUI.Box(p, "");
                                GUI.color = Color.white;
                            }

                            Rect r = EditorGUILayout.BeginHorizontal();
                            
                            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(18), GUILayout.ExpandWidth(false)))
                            {
                                toRemove.Add(tr);
                                tr.renderer.lightmapIndex = -1;
                                /*
                                foreach (Material m in tr.renderer.sharedMaterials)
                                {
                                    if (m.shader.name.Contains("Lightmapped"))
                                    {
                                        //You can change this thing if you'd like to change the shader to which the object returns
                                        m.shader = Shader.Find("Bumped Diffuse");
                                    }
                                }
                                */
                            }
                            GUILayout.Label(tr.name,GUILayout.Height(20), GUILayout.ExpandWidth(true), GUILayout.Width(this.position.width - 200));
                            EditorGUILayout.BeginVertical();
                            foreach (Material m in tr.renderer.sharedMaterials)
                            {
								if (m==null){ 
									// UnityEngine.Debug.Log(tr.name + " has a missing material in MeshRenderer!");
									GUI.color = new Color(0.9f, 0.7f, 0.7f);
									GUILayout.Box("Missing a material");
									GUI.color = Color.white;
								}
								else{
									int choice = 0;
									GUI.color = Color.white;
									choice = shaderBuffer.IndexOf(m.shader);

									if (choice == -1) choice = 0;
									if (choice == 0)
									{
										GUI.color = new Color(0.9f, 0.45f, 0.45f);
										incompatible = true;
									}
									if (choice != (tmps = EditorGUILayout.Popup(choice, compatibleShaders, GUILayout.Width(150))))
									{
										choice = tmps;
										Undo.RegisterUndo(m, "Shader change on " + tr.name);
										if (choice == 0) m.shader = Shader.Find("Bumped Diffuse");
										else
										{
											m.shader = (Shader)shaderBuffer[choice];
										}
									}
									GUI.color = Color.white;
								}
                            }
                            EditorGUILayout.EndVertical();

                            if (Event.current.type == EventType.MouseDown)
                            {

                                if (r.Contains(Event.current.mousePosition))
                                {
                                    Selection.activeTransform = tr;
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                                    DragAndDrop.PrepareStartDrag();
                                    DragAndDrop.objectReferences = new Object[1] { tr };
                                    DragAndDrop.StartDrag(tr.name);
                                    Event.current.Use();
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            switch (displayMessage)
	                        {
	                            case 0:
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.BeginVertical(GUILayout.Width(this.position.width-150));
                                    GUILayout.Label(" ");
                                    GUILayout.Label(" This object will not get exported. Its uv2 map has overlapping areas (red areas on the image)", "WordWrappedLabel");
                                    //GUILayout.Label(" This object will not get exported");
                                    //GUILayout.Label(" Its uv2 map has overlapping areas");
                                    //GUILayout.Label(" (red areas on the image)");
                                    if (notNormalized2.Contains(tr))
                                    {
                                        GUILayout.Label(" and it's not normalized.");
                                    }
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.ObjectField((Texture2D)overlapping2[indexInside - 1], typeof(Texture2D), GUILayout.Height(100), GUILayout.Width(100));
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.BeginHorizontal();
                                    if (((string)overlapping2[indexInside - 2]) != "primitive.notAFile")
                                    {
                                            if (!cantOpen.Contains(Path.GetExtension((string)overlapping2[indexInside - 2]).ToLower()))
                                            {
                                                if (GUILayout.Button("Open file"))
                                                {
                                                    Application.OpenURL(Application.dataPath + "/../" + (string)overlapping2[indexInside - 2]);
                                                }
                                            }
                                    
                                        GUILayout.Label("You need to manually correct this by editing " + ((string)overlapping2[indexInside - 2]), "WordWrappedLabel");
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    
                                    break;
                                case 1:
                                    //GUILayout.Label(" ");
                                    GUILayout.Label(" This object will not get exported. Its uv2 map isn't normalized", "WordWrappedLabel");
                                    EditorGUILayout.BeginHorizontal();
                                    if (((string)notNormalized2[indexInside - 1]) != "primitive.notAFile")
                                    {
                                        if (!cantOpen.Contains(Path.GetExtension((string)notNormalized2[indexInside - 1]).ToLower()))
                                        {
                                            if (GUILayout.Button("Open file"))
                                            {
                                                Application.OpenURL(Application.dataPath + "/../" + (string)notNormalized2[indexInside - 1]);
                                            }
                                        }
                                    
                                        GUILayout.Label("You need to manually correct this by editing " + ((string)notNormalized2[indexInside - 1]), "WordWrappedLabel");
                                    }
                                    /*if (GUILayout.Button("Normalize within Unity"))
                                    {
                                        CalcArea.NormalizeUV2((MeshFilter)tr.GetComponent(typeof(MeshFilter)));
                                        notNormalized.Remove(tr);
                                    }
                                     */
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    break;
                            
                                case 2:
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.BeginVertical(GUILayout.Width(this.position.width - 150));
                                    GUILayout.Label(" ");
                                    GUILayout.Label(" This object will not get exported. Its uv map has overlapping areas (red areas on the image)", "WordWrappedLabel");
                                    if (notNormalized.Contains(tr))
                                    {
                                        GUILayout.Label(" and it's not normalized.");
                                    }
                                    

                                    EditorGUILayout.EndVertical();

                                    EditorGUILayout.ObjectField((Texture2D)overlapping[indexInside - 1], typeof(Texture2D), GUILayout.Height(100), GUILayout.Width(100));
                                    EditorGUILayout.EndHorizontal();
                                    
                                    EditorGUILayout.BeginHorizontal();
                                    if (((string)overlapping[indexInside - 2]) != "primitive.notAFile")
                                    {
                                        if (!cantOpen.Contains(Path.GetExtension((string)overlapping[indexInside - 2]).ToLower()))
                                        {

                                            if (GUILayout.Button("Open file"))
                                            {
                                                Application.OpenURL(Application.dataPath + "/../" + (string)overlapping[indexInside - 2]);
                                            }
                                        }
                                    
                                        GUILayout.Label("You need to manually correct this by editing " + ((string)overlapping[indexInside - 2]), "WordWrappedLabel");
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    
                                    break;
                                case 3:
                                    //GUILayout.Label(" ");
                                    GUILayout.Label(" This object will not get exported. Its uv map isn't normalized.", "WordWrappedLabel");
                                    EditorGUILayout.BeginHorizontal();
                                    if (((string)notNormalized[indexInside - 1]) != "primitive.notAFile")
                                    {
                                        if (!cantOpen.Contains(Path.GetExtension((string)notNormalized[indexInside - 1]).ToLower()))
                                        {
                                            if (GUILayout.Button("Open file"))
                                            {
                                                Application.OpenURL(Application.dataPath + "/../" + (string)notNormalized[indexInside - 1]);
                                            }
                                        }
                                    
                                        GUILayout.Label("You need to manually correct this by editing " + ((string)notNormalized[indexInside - 1]), "WordWrappedLabel");
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    /*
                                    if (GUILayout.Button("Normalize within Unity"))
                                    {
                                        CalcArea.NormalizeUV((MeshFilter)tr.GetComponent(typeof(MeshFilter)));
                                        notNormalized.Remove(tr);
                                    }
                                    
                                     */
                                    //EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    
                                     
                                    break;
                                /*
                                case 4:

                                    GUILayout.Label(" This object has no uv2, however first uv");
                                    GUILayout.Label(" looks usable, so we'll use that");
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    break;
                                 */
                                
                                case 4:
                                    GUILayout.Label(" This object will not get exported. It doesn't have any valid uv", "WordWrappedLabel");
                                    EditorGUILayout.BeginHorizontal();
                                    if (((string)noUVs[indexInside - 1]) != "primitive.notAFile")
                                    {
                                        if (!cantOpen.Contains(Path.GetExtension((string)noUVs[indexInside - 1]).ToLower()))
                                        {
                                            if (GUILayout.Button("Open file"))
                                            {
                                                Application.OpenURL(Application.dataPath + "/../" + (string)noUVs[indexInside - 1]);
                                            }
                                        }
                                    
                                        GUILayout.Label("You need to manually correct this by editing " + ((string)noUVs[indexInside - 1]), "WordWrappedLabel");
                                    }
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Separator();
                                    EditorGUILayout.Separator();
                                    
                                    break;
                                default:
                                    break;
                            }
                            EditorGUILayout.EndVertical();
                            if (displayMessage > -1) EditorGUILayout.Separator();
                         
                        }
                        else
                        {
                            toRemove.Add(tr);
                        }
                    }

                    foreach (Transform i in toRemove)
                    {
                        ((ArrayList)allData[currentLightmap]).Remove(i);
                    }
                }
            }
            catch (System.NullReferenceException)
            {
                currentLightmap = 0;
                LoadObjects();
            }
        }
        EditorGUILayout.EndScrollView();

        GUI.color = new Color(0.9f, 0.45f, 0.45f);
        if (incompatible && GUILayout.Button("Change all incompatible to a lightmapped shader"))
        {
            ChangeIncompatible();
        }
        GUI.color = Color.white;
        tmp = GUILayout.Toolbar(-1, new GUIContent[2]{new GUIContent("Add selected", "Takes selected objects and put them on the list"), 
                                                      new GUIContent("Clear list", "Clears the current lightmap object list")});
        if (tmp == 0)
        {
            PickSelected();
        }
        if (tmp == 1)
        {
            ClearList();
        }

        EditorGUILayout.Separator();
        
        if (EditorGUILayout.Foldout(true, "External Tool options: ", "BoldLabel"))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export scene"))
            {
                ExportFBX();
            }
            //if (appPaths.Count==0) GUI.enabled=false;
            //run = GUILayout.Toggle(run, " & Open", GUILayout.Width(60));
            //GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Separator();
            if (appPaths.Count == 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Open with:");
                if (GUILayout.Button("Add application"))
                {
                    AddApp();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (selectedApp != (tmp = EditorGUILayout.Popup("Open with:", selectedApp, appSelect)))
                {
                    if (tmp == appSelect.Length - 1)
                    {
                        AddApp();
                    }
                    else
                    {
                        selectedApp = tmp;
                    }
                }
            }

            
            
            showAdvanced = GUILayout.Toggle(showAdvanced, "Show Advanced");
        }

        if (showAdvanced){
            if (EditorGUILayout.Foldout(true, "Main Export options: ", "BoldLabel"))
            {
                padding = EditorGUILayout.IntSlider(new GUIContent("Packing spacing", "The distanse between packed uvs on lightmap atlas"), padding, 0, 15);
                exportScale = EditorGUILayout.Slider("Export scale", exportScale, 0.1f, 1000.0f);
                exportLight = EditorGUILayout.BeginToggleGroup("Export Unity Lights", exportLight);
                tagged = EditorGUILayout.TagField("Tagged: ", tagged);
                //lightMultipler = EditorGUILayout.Slider("Light Multipler", lightMultipler, 0.1f, 1000.0f);
                    
                
                EditorGUILayout.EndToggleGroup();
                
            }
            
            if (debugMode)
            {
                if (EditorGUILayout.Foldout(true, "Correction tools: ", "BoldLabel"))
                {
                    if (offsetVector != (temp = EditorGUILayout.Vector2Field("Offset field", offsetVector)))
                    {
                        Vector4 correct;
                        foreach (Transform i in (ArrayList)LightmappingTool.allData[LightmappingTool.currentLightmap])
                        {
                            correct = i.renderer.lightmapTilingOffset;
                            i.renderer.lightmapTilingOffset = new Vector4(correct.x, correct.y, correct.z - (offsetVector.x - temp.x) * 0.01f, correct.w - (offsetVector.y - temp.y) * 0.01f);
                        }
                        offsetVector = temp;
                    }
                }
            }
        }
        if (GUI.changed)
        {
            StorePreferences.Save();
        }
    }

    private void ChangeIncompatible()
    {
        if (allData[currentLightmap] != null && ((ArrayList)allData[currentLightmap]).Count > 0)
        {
            
            //Shader sh = (Shader)AssetDatabase.LoadAssetAtPath("Assets/LightmappingTools/ExternalLightmappingTool-Diffuse.shader", typeof(Shader));
            Shader sh = Shader.Find(defaultShader);
            if (sh == null)
            {
                try
                {
                    sh = (Shader)AssetDatabase.LoadAssetAtPath(defaultShader, typeof(Shader));
                }
                catch
                {
                    UnityEngine.Debug.LogWarning(defaultShader + " shader not found!");
                    sh = Shader.Find("Lightmapped/Bumped Diffuse");
                }
                
            }


            ArrayList temp = new ArrayList();
            foreach (Transform i in (ArrayList)allData[currentLightmap])
            {
                foreach (Material m in i.renderer.sharedMaterials)
                {
                    if (!m.shader.name.Contains("Lightmapped"))
                    {
                        temp.Add(m);
                    }
                }
            }

            //Undo.RegisterUndo(new Object(), (UnityEngine.Object[]) temp.ToArray(typeof(UnityEngine.Object)), "changing incompatible shaders");
            foreach (Transform i in (ArrayList)allData[currentLightmap])
            {
                foreach (Material m in i.renderer.sharedMaterials)
                {
                    if (!m.shader.name.Contains("Lightmapped"))
                    {
                        //Default shader to which incompatible shaders are changed
                        m.shader = sh;
                    }
                }
            }
        }
    }

    void AddObjects(int lightmapIndex, Object[] draggedObjects)
    {
        if ((lightmapIndex < numberOfLightmaps) && draggedObjects!=null)
        {
			if (draggedObjects[0] is Transform){
                if ((((ArrayList)allData[lightmapIndex]).Count>0) && (((ArrayList)allData[lightmapIndex]).Contains((Transform) draggedObjects[0]))){
                    return;
                }
                ((ArrayList)allData[currentLightmap]).Remove((Transform)draggedObjects[0]);
                ((Transform)draggedObjects[0]).renderer.lightmapIndex = lightmapIndex;
                ((ArrayList)allData[lightmapIndex]).Add((Transform)draggedObjects[0]);
                return;
			}
			
            foreach (Object obj in draggedObjects)
            {
                Component[] all = ((GameObject)obj).GetComponentsInChildren(typeof(Renderer));
                if (all != null && all.Length > 0)
                {
                    foreach (Component i in all)
                    {
                        if (((ArrayList)allData[lightmapIndex]).Count > 0 && ((ArrayList)allData[lightmapIndex]).Contains(((Renderer)i).transform))
                        {
                            ;
                        }
                        else if (((Renderer)i).GetComponents(typeof(MeshFilter)).Length < 1) { ;}
                        else
                        {
                            if (((Renderer)i).sharedMaterial == null)
                            {
                                if (EditorUtility.DisplayDialog("The object " + i.name + " doesn't have a material assigned and cannot be added to the list without it.", "", "Give material and add", "Don't add"))
                                {
                                    Material mat = new Material(Shader.Find("Lightmapped/Bumped Diffuse"));
                                    mat.name = "AutoLightmapMaterial";
                                    AssetUtility.CreateAsset(mat, mat.name, "mat");
                                    ((Renderer)i).sharedMaterial = mat;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            MeshFilter component = (MeshFilter)((Renderer)i).GetComponent(typeof(MeshFilter));
                            if (component.sharedMesh != null)
                            {
                                CalcArea.CheckIfNormalized(component);
                                ((ArrayList)allData[lightmapIndex]).Add(((Renderer)i).transform);
                                ((Renderer)i).lightmapIndex = lightmapIndex;
                            }
                        }
                    }
                }
            }
        }
    }

    void OnHierarchyChange()
    {
        if (numberOfLightmaps < 2) LoadObjects();
        else
        {
            LoadCurrentObjects();
        }
        if (window != null) window.Repaint();
    }
    void PickSelected()
    {
        LightmapGenerator(numberOfLightmaps);
        if (((ArrayList)allData[currentLightmap]) == null)
        {
            allData[currentLightmap] = new ArrayList();
        }
        Object[] toAdd = Selection.GetFiltered(typeof(Renderer), SelectionMode.Deep);
        foreach (Renderer item in toAdd)
        {
            if (((ArrayList)allData[currentLightmap]).Count > 0 && ((ArrayList)allData[currentLightmap]).Contains(item.transform))
            { ;}
            else if (item.GetComponents(typeof(MeshFilter)).Length < 1) { ;}
            else
            {
                if (item.renderer.sharedMaterial == null)
                {
                    if (EditorUtility.DisplayDialog("The object " + item.name + " doesn't have a material assigned and cannot be added to the list without it.", "", "Give material and add", "Don't add"))
                    {

                        Material mat = new Material(Shader.Find("Lightmapped/Bumped Diffuse"));
                        mat.name = "AutoLightmapMaterial";
                        //UnityEngine.Debug.Log("Material saved at: " + EditorApplication.applicationContentsPath + mat.name + ".mat");
                        AssetUtility.CreateAsset(mat, mat.name, "mat");
                        item.renderer.sharedMaterial = mat;
                    }
                    else
                    {
                        continue;
                    }
                }
                MeshFilter component = (MeshFilter)item.GetComponent(typeof(MeshFilter));
                if (component.sharedMesh != null)
                {
                    CalcArea.CheckIfNormalized(component);
                    ((ArrayList)allData[currentLightmap]).Add(item.transform);
                    item.lightmapIndex = currentLightmap;
                }

                
            }

        }
    }
    void ClearList()
    {
        foreach (Transform i in ((ArrayList)allData[currentLightmap]))
        {
            i.renderer.lightmapIndex = -1;
            /*foreach (Material m in i.renderer.sharedMaterials)
            {
                if (m.shader.name.Contains("Lightmapped"))
                {
                    m.shader = Shader.Find("Bumped Diffuse");
                }
            }*/
        }
        allData[currentLightmap] = new ArrayList(); ;
    }

    static void MakeUniqueNames(ArrayList matArray)
    {
        if (matArray.Count > 0)
        {
            for (int i = 0; i < matArray.Count; i++ )
            {
                int k = 0;
                for (int j = i+1; j < matArray.Count; j++)
                {
                    if (((Material)matArray[i]).name == ((Material)matArray[j]).name)
                    {
                        ((Material)matArray[j]).name = ((Material)matArray[j]).name + System.Convert.ToString(k);
                        k++;
                    }
                }
            }
        }
    }
  
    void Awake()
    {
        cantOpen = new ArrayList();
        cantOpen.Add(".fbx");
        cantOpen.Add(".dae");
        cantOpen.Add(".notAFile");
        
        StorePreferences.Load();
        LoadObjects();
        Repaint();

        ArrayList toRemove = new ArrayList();
        foreach (string i in appPaths)
        {
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
            for (int i = 0; i < appPaths.Count; i++)
            {
                string[] name = ((string)appPaths[i]).Split(new char[1] { System.Convert.ToChar("/") });
                appSelect[i] = "" + (i + 1) + ". " + name[name.Length - 2] + "\\" + name[name.Length - 1] + " " + ((((string)appPaths[i]).Contains("x86")) ? "(32-bit)" : "");
            }
            appSelect[appSelect.Length - 1] = "Add application";
            StorePreferences.Save();
        }
        
    }

    static void ExportFBX()
    {
        if (Path.GetFileNameWithoutExtension(EditorApplication.currentScene).Length == 0)
        {
            EditorUtility.DisplayDialog("Please save your scene first", "Information", "OK");
            return;
        }
        try
        {
            if (!System.IO.Directory.Exists(MaxFiles))
            {
                System.IO.Directory.CreateDirectory(MaxFiles);
            }
        }
        catch
        {
            UnityEngine.Debug.LogError("Failed creating 'MaxFiles' directory in the project folder");
            return;
        }
        try
        {
            if (LMdir[0] != System.Convert.ToChar("/"))LMdir = "/" +LMdir;
            if (!System.IO.Directory.Exists(Application.dataPath +LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene))))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene)));
            }
        }
        catch
        {
            UnityEngine.Debug.LogError("Failed creating a specifed directory for lightmaps");
            return;
        }
        temp = new Vector2();
        offsetVector = new Vector2();
        LoadObjects();
        
        //Collapse unused lightmaps
        ArrayList toRemove = new ArrayList();
        int empties = 0;
        int delIndex = 0;
        foreach (ArrayList i in (ArrayList)allData)
        {
            if (i.Count < 1)
            {
                toRemove.Add(i);
                empties++;
                try
                {
                    res.RemoveAt(delIndex);
                    delIndex--;
                    res.Add(defaultRes);
                }
                catch { }
            }
            else
            {
                if (empties > 0)
                {
                    foreach (Transform j in i)
                    {
                        j.renderer.lightmapIndex -= empties;
                    }
                }
            }
            delIndex++;
        }
        foreach (ArrayList i in toRemove)
        {
            allData.Remove(i);
        }
        
        
        numberOfLightmaps = allData.Count;
        lightmapArray = LightmapGenerator(numberOfLightmaps);

        Light[] lights;
        if (exportLight)
        {
            lights = (Light[])Object.FindSceneObjectsOfType(typeof(Light));
            if (tagged!="Untagged"){
                ArrayList temporary = new ArrayList();
                foreach (Light i in lights)
                {
                    if (i.CompareTag(tagged))
                    {
                        temporary.Add(i);
                    }
                }
                lights = (Light[])temporary.ToArray(typeof(Light));
            }
        }
        else
        {
            lights = new Light[0];
        }


        int totalCount = 0;
        int textures = 0;

        ArrayList materialsArray = new ArrayList();
        bigArray = new ArrayList();
        ArrayList uniqueMaterialsArray = new ArrayList();
        ArrayList totalUniqueMaterials = new ArrayList();
        ArrayList uniqueMaterials;
        ArrayList mfList;

        int ii = 0;
        foreach (ArrayList selection in allData)
        {
            if (selection != null)
            {

                uniqueMaterials = new ArrayList();
                mfList = new ArrayList();

                foreach (Transform tx in selection)
                {
                    if (!overlapping.Contains(tx) && !overlapping2.Contains(tx) && !notNormalized.Contains(tx) && !notNormalized2.Contains(tx) && !noUVs.Contains(tx))
                    {
                        Component[] meshfilter = tx.GetComponents(typeof(MeshFilter));
                        for (int m = 0; m < meshfilter.Length; m++)
                        {
                            mfList.Add(meshfilter[m]);
                        }
                    }
                }
                MeshFilter[] mf = new MeshFilter[mfList.Count];

                Material[][] tmp0 = new Material[mfList.Count][];

                //The pass for Lightmap i
                for (int i = 0; i < mfList.Count; i++)
                {
                    mf[i] = (MeshFilter)mfList[i];

                    Material[] tmp1 = ((MeshFilter)mfList[i]).renderer.sharedMaterials;

                    int k = 0;
                    bool isUnique = true;
                    foreach (Material mat in ((MeshFilter)mfList[i]).renderer.sharedMaterials)
                    {
						if (mat ==null){
							UnityEngine.Debug.LogError(((MeshFilter) mfList[i]).transform.name + " has a missing material in its MeshRenderer! Fix it before exporting!");
							return;
						}
                        if (totalUniqueMaterials.Contains(mat))
                        {
                            isUnique = false;
                            if (!uniqueMaterials.Contains(mat))
                            {
                                uniqueMaterials.Add(mat);
                            }
                        }
                        else
                        {
                            uniqueMaterials.Add(mat);
                            totalUniqueMaterials.Add(mat);
                        }

                        if (mat.HasProperty("_MainTex") && (mat.GetTexture("_MainTex")))
                        {
                            if (isUnique) textures++;
                        }
                        if (mat.HasProperty("_BumpMap") && (mat.GetTexture("_BumpMap")))
                        {
                            if (isUnique) textures++;
                        }
                        k++;
                    }
                    tmp0[i] = tmp1;

                }
                materialsArray.Add(tmp0);
                if (mf.Length>0) bigArray.Add(mf);
                uniqueMaterialsArray.Add(uniqueMaterials);
            }
            ii++;
            if (ii > numberOfLightmaps - 1) break;
        }
        totalCount += lights.Length;
        totalCount += bigArray.Count;
        totalCount += totalUniqueMaterials.Count;
        totalCount += textures;
        MakeUniqueNames(totalUniqueMaterials);

        if (bigArray.Count > 0)
        {
            //Needed preparations before looking for lightmaps
            SaveFBX save = new SaveFBX();
            bool Done = save.ExportFBX(ref bigArray, ref materialsArray, ref uniqueMaterialsArray, ref totalUniqueMaterials, ref lights, textures, totalCount);
            if (Done)
            {
                save = null;
                float sizee;
                for (int i = 0; i < bigArray.Count; i++)
                {
                    for (int j = 0; j < ((MeshFilter[])bigArray[i]).Length; j++)
                    {
                        sizee = ((Rect[])SaveFBX.offsetsArray[i])[j].width;
                        ((MeshFilter[])bigArray[i])[j].renderer.lightmapTilingOffset = new Vector4(sizee, sizee, ((((Rect[])SaveFBX.offsetsArray[i]))[j].x), (((Rect[])SaveFBX.offsetsArray[i])[j].y));
                    }
                }
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    
                    
                    if ((appPaths.Count > 0))
                    {
                        
                        if (appSelect[selectedApp].Contains("3dsmax")) //This is done only to show how to extend the system to next rendering tools
                        {
                            PrepareBatchScript.PrepareMax();
                            bool turnedToMax = false;
                            try
                            {
                                turnedToMax = SwitchWindows.TurnTo(Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".max");
                            }
                            catch
                            {
                                EditorUtility.DisplayDialog("Switching to 3dsmax failed", "Exporting succesful, but unluckily you've got to switch to max manually and press Reimport button", "ok");
                            }
                            if (!turnedToMax)
                            {
                                EditorUtility.DisplayDialog("Exporting succesfull", "3dsmax will now start and try to import the fbx file.\nPlease wait.", "OK");
                                string MaxParameters = " -U MAXScript \"" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".ms\"";
                                if (((string)appPaths[selectedApp]).Length + MaxParameters.Length >= 256)
                                {
                                    UnityEngine.Debug.LogWarning("Your scene name or 3d application path name may be too long to export the scene properly.");
                                }
                                //UnityEngine.Debug.Log(MaxParameters);
                                ProcessStartInfo startInfo = new ProcessStartInfo((string)appPaths[selectedApp], MaxParameters);
                                startInfo.UseShellExecute = true;
                                startInfo.WorkingDirectory = MaxFiles;
                                Process.Start(startInfo);
                                GUIUtility.ExitGUI();
                            }
                        }
                        else if (appSelect[selectedApp].Contains("maya"))
                        {
                            PrepareBatchScript.PrepareMaya();
                            bool turnedToMaya = false;
                            try
                            {
                                turnedToMaya = SwitchWindows.TurnTo(Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mb");
                            }
                            catch
                            {
                                EditorUtility.DisplayDialog("Switching to Maya failed", "Exporting succesful, but unluckily you've got to switch to maya manually and press Reimport button", "ok");
                            }
                            if (!turnedToMaya)
                            {
                                EditorUtility.DisplayDialog("Exporting succesfull", "Maya will now start and try import the fbx file.\nPlease wait.", "OK");
                                string Parameters = " -script \"" + MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "_startup.mel\"";
                                //string Parameters = " -script \"" + MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".py\"";
                                if (((string)appPaths[selectedApp]).Length + Parameters.Length >= 256)
                                {
                                    UnityEngine.Debug.LogWarning("Your scene name or 3d application path name may be too long to export the scene properly.");
                                }
                                UnityEngine.Debug.Log((string)appPaths[selectedApp]);
                                //System.Diagnostics.Processes.Start("")
                                ProcessStartInfo startInfo = new ProcessStartInfo((string)appPaths[selectedApp], Parameters);
                                startInfo.UseShellExecute = false;
                                startInfo.WorkingDirectory = MaxFiles;
                                //Process.Start(startInfo);
                                Process.Start(startInfo);
                                UnityEngine.Debug.Log("Exporting using maya!");
                                GUIUtility.ExitGUI();
                            }
                        }
                    }
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    if (appPaths.Count > 0)
                    {
                        if (appSelect[selectedApp].Contains("Maya")) //This is done only to show how to extend the system to next rendering tools
                        {
                        	PrepareBatchScript.PrepareMaya();
                        	
                        	
                        	//Checking if Maya is open :)
                        	Process p = new Process();
							p.StartInfo.UseShellExecute = false;
							p.StartInfo.RedirectStandardOutput = true;
							p.StartInfo.WorkingDirectory = "/usr/bin/";
							p.StartInfo.Arguments = "-e 'tell application \"System Events\"' -e 'set runningApps to name of every application process' -e 'end tell' -e 'if runningApps does not contain \"Maya\" then return 0' -e 'tell application \"Maya\" to activate' -e 'return 1'";
							//p.StartInfo.FileName = Application.dataPath + "/LightmappingTools/findMaya.app" + "/Contents/MacOS/applet";
							//p.StartInfo.FileName = "/usr/bin/osascript \""+Application.dataPath + "/LightmappingTools/findMaya3.scptd\"";
							//p.StartInfo.FileName = "osascript 
							
							//p.StartInfo.FileName = "osascript \"return 1\"";
							p.StartInfo.FileName = "osascript";
							p.Start();
							string output = p.StandardOutput.ReadToEnd();
							p.WaitForExit();	
							//output = p.StandardOutput.ReadToEnd();
							
							//p.StartInfo.FileName = Application.dataPath + "/LightmappingTools/findMaya.app";
							//p.Start();
							//string output = Process.StandardOutput.ReadToEnd();
							
							UnityEngine.Debug.Log("Seeks maya and outputs: #" + output + "#");
							if (output=="0\n"){
							
                            	EditorUtility.DisplayDialog("Exporting succesfull", "Maya will now start and try import the fbx file.\nPlease wait.", "OK");
                            	string Parameters = " -script \"" + MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "_startup.mel\"";
                            	//string Parameters = " -script \"" + MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".py\"";
                            	if (((string)appPaths[selectedApp]).Length + Parameters.Length >= 256)
                            	{
                            	    UnityEngine.Debug.LogWarning("Your scene name or 3d application path name may be too long to export the scene properly.");
                            	}
                            	UnityEngine.Debug.Log("open " +(string)appPaths[selectedApp] + "/Contents/MacOS/Maya");
                            	UnityEngine.Debug.Log(Parameters);
                            	//System.Diagnostics.Processes.Start("")
                            	ProcessStartInfo startInfo = new ProcessStartInfo((string)appPaths[selectedApp] + "/Contents/MacOS/Maya", Parameters);
                            	startInfo.UseShellExecute = false;
                            	startInfo.WorkingDirectory = MaxFiles;
                            	//Process.Start(startInfo);
                            	Process.Start(startInfo);
                            	UnityEngine.Debug.Log("Exporting using maya!");
                            	GUIUtility.ExitGUI();
							}
                        }
                    }

                }
                /*else
                {
                    EditorUtility.DisplayDialog("Everything's alright. You can find the fbx file in your project folder called " + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + " .fbx", "Exporting succesfull", "OK");
                }*/
            }
            else
            {
                save = null;
                EditorUtility.DisplayDialog("Cannot continue", "The UV packing cannot be done efficiently, consider a different lightmap arrangement", "OK");
                EditorUtility.ClearProgressBar();
                UnityEngine.Debug.LogError("The UV packing cannot be done or cannot be done efficiently, consider a different lightmap arrangement");
            }
        }
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }

    private void AddApp()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor) AddWinApp();
        else if (Application.platform == RuntimePlatform.OSXEditor) AddOSXApp();
        else EditorUtility.DisplayDialog("Not compatible system", "Sorry, you're system isn't currently supported by the Lightmapping Tool.", "OK");
        GUIUtility.ExitGUI();

    }


    private void AddWinApp()
    {
        string toAdd = EditorUtility.OpenFilePanel("Point the 3dsmax or Maya application", Application.dataPath, "exe");

        if (toAdd.Length != 0)
        {
            if (toAdd.Contains("3dsmax"))
            {
                if (appPaths.Count > 0)
                {
                    if (appPaths.Contains(toAdd)) return;
                }
                appPaths.Add(toAdd);
                appSelect = new string[appPaths.Count + 1];
                for (int i = 0; i < appPaths.Count; i++)
                {
                    string[] name = ((string)appPaths[i]).Split(new char[1] { System.Convert.ToChar("/") });
                    appSelect[i] = "" + (i + 1) + ". " + name[name.Length - 2] + "\\" + name[name.Length - 1] + " " + ((((string)appPaths[i]).Contains("x86")) ? "(32-bit)" : "");
                }
                selectedApp = appSelect.Length - 2;
                appSelect[appPaths.Count] = "Add application";
                StorePreferences.Save();
            }
            else if (toAdd.Contains("maya"))
            {
                if (appPaths.Count > 0)
                {
                    if (appPaths.Contains(toAdd)) return;
                }
                appPaths.Add(toAdd);
                appSelect = new string[appPaths.Count + 1];
                for (int i = 0; i < appPaths.Count; i++)
                {
                    string[] name = ((string)appPaths[i]).Split(new char[1] { System.Convert.ToChar("/") });
                    appSelect[i] = "" + (i + 1) + ". " + name[name.Length - 2] + "\\" + name[name.Length - 1] + " " + ((((string)appPaths[i]).Contains("x86")) ? "(32-bit)" : "");
                }
                selectedApp = appSelect.Length - 2;
                appSelect[appPaths.Count] = "Add application";
                StorePreferences.Save();
            }
            else
            {
                EditorUtility.DisplayDialog("Not compatible application", "Currently only 3dsmax and Maya works with the Lightmapping system on Windows", "OK");
            }

        }
        else
        {
            return;
        }
    }

    private void AddOSXApp()
    {
        string toAdd = EditorUtility.OpenFilePanel("Point the Maya application", Application.dataPath, "app");

        if (toAdd.Length != 0)
        {
            if (toAdd.Contains("Maya"))
            {
                if (appPaths.Count > 0)
                {
                    if (appPaths.Contains(toAdd)) return;
                }
                appPaths.Add(toAdd);
                appSelect = new string[appPaths.Count + 1];
                for (int i = 0; i < appPaths.Count; i++)
                {
                    string[] name = ((string)appPaths[i]).Split(new char[1] { System.Convert.ToChar("/") });
                    appSelect[i] = "" + (i + 1) + ". " + name[name.Length - 2] + "\\" + name[name.Length - 1];
                }
                selectedApp = appSelect.Length - 2;
                appSelect[appPaths.Count] = "Add application";
                StorePreferences.Save();
            }

            else
            {
                EditorUtility.DisplayDialog("Not compatible application", "Currently only Maya works with the Lightmapping system on OSX", "OK");
            }

        }
        else
        {
            return;
        }
    }

}