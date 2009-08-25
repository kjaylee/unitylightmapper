using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


public class LightmappingTool : EditorWindow
{
    enum avShaders { NotCompatible,BumpedDiffuse, BumpedSpecular, Diffuse, Specular, VertexLit };
    static public string[] resolutions = new string[6] { "128x128", "256x256", "512x512", "1024x1024", "2048x2048", "4096x4096" };
    static public ArrayList res = new ArrayList();
    static public int numberOfLightmaps = 1;
    static public int currentLightmap = 0;
    int tmp = 1;  
    static public ArrayList RenderedLightmaps = new ArrayList();
    static public ArrayList allData = new ArrayList();
    static public string[] lightmapArray = new string[] { "Lightmap 1" };
    //static public string[] compatibleShaders = new string[] { "Not Compatible","Bumped Diffuse", "Bumped Specular", "Diffuse", "Specular", "VertexLit" };
    static public Vector2 scrollValue = new Vector2();

    static public bool haveBeenExported = false;
    static public bool startLooking = false;
    static public ArrayList bigArray;
    static public string MainButtonText = "Export FBX";
    static public bool run = true;
    
    //static Texture xTex = (Texture) AssetDatabase.LoadAssetAtPath("Assets/LightmappingTools/x.png", typeof(Texture2D));
    [MenuItem("Window/Lightmapping")]
    static void Init()
    {
        allData = new ArrayList();
        allData.Add(new ArrayList());
        if (res==null || res.Count < 1)
        {
            loadResArray();
        }
        StorePreferences.Load();
        Renderer[] objects = (Renderer[])Object.FindSceneObjectsOfType(typeof(Renderer));
        foreach (Renderer rn in objects)
        {
            if (rn.lightmapIndex > -1 && rn.sharedMaterial!=null)
            {
				while(rn.lightmapIndex>=allData.Count){
					allData.Add(new ArrayList());
				}
				if (((ArrayList)allData[rn.lightmapIndex]).Count>0){
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
        numberOfLightmaps = allData.Count;
        lightmapArray = lightmapGenerator(numberOfLightmaps);

        LightmappingTool window = (LightmappingTool)EditorWindow.GetWindow(typeof(LightmappingTool));
        window.Show();
    }
    

    public void Awake()
    {
        if (currentLightmap > numberOfLightmaps) currentLightmap = 0;
        
        //skin =(GUISkin)Resources.Load("lightmapSkin", typeof(GUISkin));
        //UnityEngine.Debug.Log("Elo");
    }
    static string[] lightmapGenerator(int number)
    {
        string[] LMArray = new string[number];
        for (int i = 0; i < number; i++)
        {
            LMArray[i] = "Lightmap " + (i + 1);
        }
        return LMArray;
    }
    static public void loadResArray()
    {
        res = new ArrayList();
        for (int i = 0; i < 99; i++)
        {
            res.Add(LightmapAdvanced.defaultRes);
        }
    }
    void OnGUI()
    {
        if (currentLightmap > numberOfLightmaps) currentLightmap = 0;

        if (res == null || res.Count < numberOfLightmaps) Init();
        
        GUI.Box(new Rect(0, 0, this.position.width, 50 + 20 * ((numberOfLightmaps-1) / 3)), "");
        //GUILayout.Box(new GUIContent[2]{new GUIContent("Pick selected", "Takes selected objects and put them on the list"), 
        //                                              new GUIContent("Clear list", "Clears the current lightmap object list")});


        //GUILayout.Label("Number of lightmaps:", "BoldLabel");
        //EditorGUILayout.PrefixLabel("Number of lightmaps:");
        if ((tmp = EditorGUILayout.IntSlider("Lightmap amount",numberOfLightmaps, 1, 99)) != numberOfLightmaps)
        {
            numberOfLightmaps = tmp;
            lightmapArray = lightmapGenerator(tmp);
        }
        //EditorGUILayout.BeginScrollView(new Vector2(0, 0));

		if ((tmp = GUILayout.SelectionGrid(currentLightmap, lightmapArray, 3)) != currentLightmap)
		{
			currentLightmap = tmp;
			ArrayList toRemove = new ArrayList();
			foreach (Transform i in (ArrayList)allData[tmp])
			{
				if (i.renderer.lightmapIndex != tmp) toRemove.Add(i);
			}
			foreach (Transform i in toRemove)
			{
				((ArrayList)allData[tmp]).Remove(i);
			}
		}


        

        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
            if ((tmp = EditorGUILayout.Popup("Resolution:", (int)res[currentLightmap], resolutions)) != (int)res[currentLightmap])
            {
                res[currentLightmap] = tmp;
                StorePreferences.SaveMain();
            }
        EditorGUILayout.EndHorizontal();


        
        Rect dropOnLM = new Rect(0, 20, this.position.width,20+ 20 * ((numberOfLightmaps - 1) / 3));
        Rect dropZone = new Rect(0, 100 + 20 * ((numberOfLightmaps-1) / 3), this.position.width, this.position.height - 100);
        
        if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;

            if (Event.current.type == EventType.DragPerform)
            {
                if (dropOnLM.Contains(Event.current.mousePosition))
                {
                    int coordy = System.Convert.ToInt16(System.Math.Floor((Event.current.mousePosition.y-20) / 20));
                    int coordx = System.Convert.ToInt16(System.Math.Floor(Event.current.mousePosition.x / (dropOnLM.width/3)));
                    UnityEngine.Debug.Log("Drop on lightmap: " + (coordy * 3 + coordx));
                    addObjects((coordy * 3 + coordx), DragAndDrop.objectReferences);                    
                    

                }
                else if (dropZone.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.AcceptDrag();
                    // These are the objects which have been dragged and dropped
                    
                    addObjects(currentLightmap, DragAndDrop.objectReferences);
                    
                }
            }

            Event.current.Use();
        }


        tmp = GUI.Toolbar(new Rect(0, 70 + 20 * ((numberOfLightmaps-1) / 3), this.position.width, 20), -1, new GUIContent[2]{new GUIContent("Pick selected", "Takes selected objects and put them on the list"), 
                                                      new GUIContent("Clear list", "Clears the current lightmap object list")});
        //GUI.backgroundColor = Color.white;
        if (tmp == 0)
        {
            PickSelected();
        }
        if (tmp == 1)
        {
            ClearList();
        }

        //EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        //Display list
        avShaders tmps;
        scrollValue = EditorGUILayout.BeginScrollView(scrollValue);

        if (numberOfLightmaps > allData.Count)
        {
            for (int i = 0; i < numberOfLightmaps - allData.Count + 1; i++)
            {
                allData.Add(new ArrayList());
            }
        }
        //EditorGUIUtility.LookLikeInspector();
        if (allData.Count > 0){
            try
            {
                if (((ArrayList)allData[currentLightmap]).Count > 0)
                {
                    ArrayList toRemove = new ArrayList();
                    foreach (Transform tr in ((ArrayList)allData[currentLightmap]))
                    {

                        Rect r = EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(18), GUILayout.ExpandWidth(false)))
                        {
                            toRemove.Add(tr);
                            tr.renderer.lightmapIndex = -1;
                        }
                        GUILayout.Label(tr.name, GUILayout.ExpandWidth(true), GUILayout.Width(this.position.width - 150));
                        EditorGUILayout.BeginVertical();
                        foreach (Material m in tr.renderer.sharedMaterials)
                        {
                            avShaders choice = avShaders.NotCompatible;
                            GUI.color = Color.white;

                            switch (m.shader.name)
                            {
                                case "Lightmapped/Bumped Diffuse":
                                    choice = avShaders.BumpedDiffuse;
                                    break;
                                case "Lightmapped/Bumped Specular":
                                    choice = avShaders.BumpedSpecular;
                                    break;
                                case "Lightmapped/Diffuse":
                                    choice = avShaders.Diffuse;
                                    break;
                                case "Lightmapped/Specular":
                                    choice = avShaders.Specular;
                                    break;
                                case "Lightmapped/VertexLit":
                                    choice = avShaders.VertexLit;
                                    break;
                                default:
                                    GUI.color = new Color(0.9f, 0.45f, 0.45f);
                                    //choice = (avShaders)EditorGUILayout.EnumPopup(choice, GUILayout.Width(100));
                                    break;

                            }

                            if (choice != (tmps = (avShaders)EditorGUILayout.EnumPopup(choice, GUILayout.ExpandWidth(true), GUILayout.Width(100))))
                            {
                                choice = tmps;
                                Undo.RegisterUndo(m, "Shader change on " + tr.name);
                                switch (choice)
                                {
                                    case avShaders.BumpedDiffuse:
                                        m.shader = Shader.Find("Lightmapped/Bumped Diffuse");
                                        break;
                                    case avShaders.BumpedSpecular:
                                        m.shader = Shader.Find("Lightmapped/Bumped Specular");
                                        break;
                                    case avShaders.Diffuse:
                                        m.shader = Shader.Find("Lightmapped/Diffuse");
                                        break;
                                    case avShaders.Specular:
                                        m.shader = Shader.Find("Lightmapped/Specular");
                                        break;
                                    case avShaders.VertexLit:
                                        m.shader = Shader.Find("Lightmapped/VertexLit");
                                        break;
                                    case avShaders.NotCompatible:
                                        m.shader = Shader.Find("Bumped Specular");
                                        break;
                                }


                            }
                            GUI.color = Color.white;
                            //meeting = (avShaders)EditorGUILayout.EnumPopup(meeting, GUILayout.Width(100));

                        }
                        EditorGUILayout.EndVertical();

                        if (Event.current.type == EventType.MouseDown)
                        {

                            if (r.Contains(Event.current.mousePosition))
                            {
                                Selection.activeTransform = tr;
                                //EditorGUIUtility.PingObject(tr);
                                DragAndDrop.visualMode = DragAndDropVisualMode.Move;

                                //DragAndDrop.SetGenericData(tr.name, new Object[1] { tr });
                                DragAndDrop.PrepareStartDrag();
                                DragAndDrop.objectReferences = new Object[1] { tr };
                                //DragAndDrop.SetGenericData(tr.name, new Object[1] { tr });
                                DragAndDrop.StartDrag(tr.name);
                                //DragAndDrop.SetGenericData(tr.name, new Object[1]{tr});
                                Event.current.Use();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        //Handles.DrawCube(tr.GetInstanceID(), tr.position, Quaternion.identity, 1.0f);
                        //Handles.color = Color.red;
                        //Gizmos.color = Color.yellow;
                        //Gizmos.DrawWireCube(tr.position, new Vector3(1, 1, 1));
                        // We need to match all BeginGroup calls with an EndGroup

                        //GUILayout.EndArea();
                        //GUILayout.Label(tr.name);
                        //EditorGUILayout.InspectorTitlebar(false, tr.renderer);
                        //EditorGUI.InspectorTitlebar(new Rect(0, 400, 100, 40), true, tr);
                        //EditorGUILayout.LabelField(tr.name, "");


                    }

                    foreach (Transform i in toRemove)
                    {
                        ((ArrayList)allData[currentLightmap]).Remove(i);
                    }
                    //UnityEngine.Debug.Log("Koniec");
                }
            }
            catch (System.NullReferenceException)
            {
                currentLightmap = 0;
                Init();
            }
            //    currentLightmap = 0;
            //}
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Advanced", GUILayout.Width(80)))
        {
            //WizardCreateLight wiz = WizardCreateLight.GetWindow(typeof(WizardCreateLight)) as WizardCreateLight;
            LightmapAdvanced.Init();
            //LightmapAdvanced Advanced = LightmapAdvanced.GetWindow(typeof(LightmapAdvanced)) as LightmapAdvanced;
        }
        //GUILayout.BeginArea(new Rect(0, this.position.height - 30, this.position.width, 30));
        //showAdvanced = EditorGUI.Toggle(new Rect(0,0, 100,15),"Advanced", showAdvanced);
        /*
        if (LightmapAdvanced.selectedApp != 0)
        {
            MainButtonText = "Bake in" + LightmapAdvanced.appSelect[LightmapAdvanced.selectedApp].Substring(LightmapAdvanced.appSelect[LightmapAdvanced.selectedApp].IndexOf(".")+1);
        }
        else
        {
            if (haveBeenExported)
            {
                MainButtonText = "Reexport";
                //LightmapAdvanced.searchForPresets();
                //LightmapAdvanced.presetFile = EditorGUILayout.Popup(LightmapAdvanced.presetFile, LightmapAdvanced.presetArrayNames, GUILayout.Width(100));
            }
        }
         */
        if (GUILayout.Button(MainButtonText, GUILayout.Width(this.position.width-80-((LightmapAdvanced.selectedApp!=0)?70:10))))
        {
            //new Rect(100,0,this.position.width-100,30),
            ExportFBX();
        }
        if (LightmapAdvanced.selectedApp != 0)
        {
            bool tempek;
            if (run != (tempek = GUILayout.Toggle(run, " & Run", GUILayout.Width(70))))
            {
                run = tempek;
                StorePreferences.SaveMain();
            }

        }
        EditorGUILayout.EndHorizontal();
    }

    void addObjects(int lightmapIndex, Object[] draggedObjects)
    {
        if ((lightmapIndex < numberOfLightmaps) && draggedObjects!=null)
        {
			if (draggedObjects[0] is Transform){    
                UnityEngine.Debug.Log("This is transform" + draggedObjects[0].name);
                ((ArrayList)allData[currentLightmap]).Remove((Transform)draggedObjects[0]);
                ((Transform)draggedObjects[0]).renderer.lightmapIndex = lightmapIndex;
                ((ArrayList)allData[lightmapIndex]).Add((Transform)draggedObjects[0]);
                Init();
                OnGUI();
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
                        else
                        {
                            if (!CalcArea.CheckIfNormalized((MeshFilter)i.GetComponent(typeof(MeshFilter))))
                            {
                                EditorUtility.DisplayDialog("Non-valid UV2", "The object " + i.name + " has a non-valid UV2, it will not be added to the list!", "OK");
                            }
                            else if (((Renderer)i).sharedMaterial == null)
                            {
                                if (EditorUtility.DisplayDialog("The object " + i.name + " doesn't have a material assigned and cannot be added to the list without it.", "", "Give material and add", "Don't add"))
                                {
                                    Material mat = new Material(Shader.Find("Lightmapped/Bumped Diffuse"));
                                    mat.name = "AutoLightmapMaterial";
                                    //UnityEngine.Debug.Log("Material saved at: " + EditorApplication.applicationContentsPath + mat.name + ".mat");
                                    AssetUtility.CreateAsset(mat, mat.name, "mat");
                                    
                                    ((Renderer)i).sharedMaterial = mat;
                                    ((ArrayList)allData[lightmapIndex]).Add(((Renderer)i).transform);
                                    ((Renderer)i).lightmapIndex = lightmapIndex;
                                    //OnGUI();
                                }
                            }
                            else
                            {
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
        Init();
    }
    void PickSelected()
    {
        //Renderer[] selected = (Renderer) ;

        lightmapGenerator(numberOfLightmaps);
        if (((ArrayList)allData[currentLightmap]) == null)
        {
            allData[currentLightmap] = new ArrayList();
        }
        
        foreach (Renderer item in Selection.GetFiltered(typeof(Renderer), SelectionMode.Deep))
        {
            if (((ArrayList)allData[currentLightmap]).Count > 0 && ((ArrayList)allData[currentLightmap]).Contains(item.transform))
            { ;}
            else
            {
                if (!CalcArea.CheckIfNormalized((MeshFilter)item.GetComponent(typeof(MeshFilter))))
                {
                    EditorUtility.DisplayDialog("Non-valid UV2", "The object " + item.name + " has a non-valid UV2, it will not be added to the list!", "OK");
                }
                else if (item.renderer.sharedMaterial == null)
                {
                    if (EditorUtility.DisplayDialog("The object " + item.name + " doesn't have a material assigned and cannot be added to the list without it.", "", "Give material and add", "Don't add"))
                    {
                        
                        Material mat = new Material(Shader.Find("Lightmapped/Bumped Diffuse"));
                        mat.name = "AutoLightmapMaterial";
                        //UnityEngine.Debug.Log("Material saved at: " + EditorApplication.applicationContentsPath + mat.name + ".mat");
                        AssetUtility.CreateAsset(mat, mat.name, "mat");
                        item.renderer.sharedMaterial = mat;
                        ((ArrayList)allData[currentLightmap]).Add(item.transform);
                        item.lightmapIndex = currentLightmap;
                        //OnGUI();
                    }
                }
                else
                {
                    ((ArrayList)allData[currentLightmap]).Add(item.transform);
                    item.lightmapIndex = currentLightmap;
                    OnGUI();
                }
            }

        }
    }
    void ClearList()
    {
        foreach (Transform i in ((ArrayList)allData[currentLightmap]))
        {
            i.renderer.lightmapIndex = -1;
        }
        allData[currentLightmap] = new ArrayList(); ;
    }

    void makeUniqueNames(ArrayList matArray)
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


    void ExportFBX()
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
            if (LightmapAdvanced.LMdir[0] != System.Convert.ToChar("/"))LightmapAdvanced.LMdir = "/" +LightmapAdvanced.LMdir;
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
        LightmapAdvanced.temp = new Vector2();
        LightmapAdvanced.offsetVector = new Vector2();
        Init();
        
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
                    res.Add(LightmapAdvanced.defaultRes);
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
        lightmapArray = lightmapGenerator(numberOfLightmaps);

        Light[] lights;
        if (LightmapAdvanced.exportLight)
        {
            lights = (Light[])Object.FindSceneObjectsOfType(typeof(Light));
            if (LightmapAdvanced.tagged!="Untagged"){
                ArrayList temporary = new ArrayList();
                foreach (Light i in lights)
                {
                    if (i.CompareTag(LightmapAdvanced.tagged))
                    {
                        temporary.Add(i);
                        UnityEngine.Debug.Log(i.name);
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

                //if (selection!=null){
                foreach (Transform tx in selection)
                {
                    //Component[] meshfilter = tx.GetComponentsInChildren(typeof(MeshFilter));
                    Component[] meshfilter = tx.GetComponents(typeof(MeshFilter));
                    for (int m = 0; m < meshfilter.Length; m++)
                    {
                        mfList.Add(meshfilter[m]);
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
        makeUniqueNames(totalUniqueMaterials);

        if (bigArray.Count > 0)
        {
            //Needed preparations before looking for lightmaps
            startLooking = true;
            SaveFBX save = new SaveFBX();
            bool Done = save.ExportFBX(ref bigArray, ref materialsArray, ref uniqueMaterialsArray, ref totalUniqueMaterials, ref lights ,textures, totalCount);
            if (Done)
            {
                save = null;
                float xsize;
                float ysize;
                for (int i = 0; i < bigArray.Count; i++)
                {
                    for (int j = 0; j < ((MeshFilter[])bigArray[i]).Length; j++)
                    {
                        //((MeshFilter[])bigArray[i])[j].renderer.lightmapIndex = i;
                        //Counting the scales
                        xsize = ((Rect[])((Rect[][])SaveFBX.offsetsArray[i])[0])[j].width / ((Rect[])((Rect[][])SaveFBX.offsetsArray[i])[1])[j].width;
                        ysize = ((Rect[])((Rect[][])SaveFBX.offsetsArray[i])[0])[j].height / ((Rect[])((Rect[][])SaveFBX.offsetsArray[i])[1])[j].height;
                        ((MeshFilter[])bigArray[i])[j].renderer.lightmapTilingOffset = new Vector4(xsize, ysize, ((Rect[])((Rect[][])SaveFBX.offsetsArray[i])[0])[j].x - xsize * ((Rect[])((Rect[][])SaveFBX.offsetsArray[i])[1])[j].xMin, ((Rect[])((Rect[][])SaveFBX.offsetsArray[i])[0])[j].y - ysize * ((Rect[])((Rect[][])SaveFBX.offsetsArray[i])[1])[j].yMin);
                    }
                }
                //Application.OpenURL(@"C:\Program Files\Autodesk\3ds Max 2009\3dsmax.exe test.fbx");

                if (LightmapAdvanced.selectedApp != 0 && run)
                {
                    EditorUtility.DisplayDialog("Exporting succesfull", "An external app will now start and attempt to import the fbx file.\nPlease wait.", "OK");
                    if (LightmapAdvanced.appSelect[LightmapAdvanced.selectedApp].Contains("3dsmax")) //This is done only to show how to extend the system to next rendering tools
                    {
                        PrepareBatchScript.Prepare();   
                        string MaxParameters = " -q -U MAXScript \"" + Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "\\MaxFiles\\" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".ms";
                        Process myProc = Process.Start((string)LightmapAdvanced.appPaths[LightmapAdvanced.selectedApp - 1], MaxParameters);
                        //if (maxPresetFile =="") LightmapAdvanced.presetFile = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "\\MaxFiles\\" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "_preset.max";
                    }
                    //LightmapAdvanced.selectedApp = 0;
                    haveBeenExported = true;
                    //LightmapAdvanced.presetFile = 0;
                    //LightmapAdvanced.rendererChoice = 0;
                    //LightmapAdvanced.exportLight = false;
                }
                else
                {
                    EditorUtility.DisplayDialog("Everything's alright. You can find the fbx file in your project folder called " + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + " .fbx", "Exporting succesfull", "OK");
                }
            }
            else
            {
                save = null;
                EditorUtility.DisplayDialog("Cannot continue", "The UV packing cannot be done efficiently, consider a different lightmap arrangement", "OK");
                EditorUtility.ClearProgressBar();
                UnityEngine.Debug.LogError("The UV packing cannot be done or either cannot be done efficiently, consider a different lightmap arrangement");
            }
        }
        
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }

}