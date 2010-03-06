using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;

/*
    Coded for Unity Summer of Code 2009 by Michal Mandrysz
    Feel free to modify for your own needs
    http://masteranza.wordpress.com/unity/lightmapping/
    http://unity3d.com
*/

class PrepareBatchScript
{
    static public void PrepareMax()
    {
    	string sciezka;
    	if (Application.platform == RuntimePlatform.WindowsEditor)
        {
        	sciezka = "MaxFiles\\" +Path.GetFileNameWithoutExtension(EditorApplication.currentScene)+".ms";
        }
        else{
        	sciezka = "MaxFiles/" +Path.GetFileNameWithoutExtension(EditorApplication.currentScene)+".ms";
        }
        using (StreamWriter sw = new StreamWriter(sciezka))
        {
            StringBuilder sb = new StringBuilder();
            
        
            sb.Append("if (preserveLights==undefined) then\r\n");
            sb.Append("(\r\n");
            sb.Append(" fileIn(\"" + Application.dataPath + "/LightmappingTools/maxscriptEngine.ms\")\r\n");
            sb.Append(")\r\n");
            
            sb.Append("global SaveDir=\"" + Application.dataPath + LightmappingTool.LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene)) + "\"\r\n");
            sb.Append("global presetDir=\"" + Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "MaxFiles/\"\r\n");
            sb.Append("global sceneName=\"" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "\"\r\n");
            sb.Append("global fileFormat=\"" + LightmappingTool.fileFormat + "\"\r\n");

            sb.Append("if (not fileExists(presetDir + sceneName + \".max\")) then (\r\n");
                sb.Append(" saveMaxFile (presetDir + sceneName + \".max\") clearNeedSaveFlag:false useNewFile:true quiet:true");
                sb.Append(" rIndex=-1\r\n");
	            sb.Append(" for i=1 to RendererClass.classes.count while (rIndex==-1) do (if ((findString (RendererClass.classes[i] as string) \"mental\")!=undefined) then rIndex=i)\r\n");
	            sb.Append(" if(rIndex!=-1) then(\r\n");
		        sb.Append("     renderers.current=RendererClass.classes[rIndex]()\r\n");
		        sb.Append("     messagebox(\"Mental Ray renderer assigned!\")\r\n");
	            sb.Append(" )\r\n");
	            sb.Append(" else(\r\n");
		        sb.Append("     messagebox(\"Mental Ray was not found in your system.\")\r\n");
	            sb.Append(" )\r\n");
            sb.Append(")\r\n");
            sb.Append("if ((maxFileName == undefined) or (maxFileName == \"\")) then (loadMaxFile (presetDir + sceneName + \".max\"))\r\n");
            sb.Append("global conversorPath=\"" + Application.dataPath + "/LightmappingTools/conversor.mse\"\r\n");
            sb.Append("global resArray=#(");
            foreach (System.Object item in LightmappingTool.res)
            {
                try{
					sb.Append(Convert.ToInt32(128 * Math.Pow(2, (Convert.ToInt32(item)))));
					sb.Append(",");
				}
				catch{}
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")\r\n");

            //Removing all materials from Material Library
            sb.Append("toRemove =#()\r\n");
			sb.Append("for i in currentMaterialLibrary do (append toRemove i.name)\r\n");
			sb.Append("for i in toRemove do (deleteItem currentMaterialLibrary(i))\r\n");

            //Add materials to Material Library and remove geometry
            sb.Append("toRemove =#()\r\n");
            sb.Append("for i in Geometry do\r\n");
	        sb.Append("(\r\n");
		    sb.Append("    if ((findString i.name \"ImportedObj\")!=undefined) then\r\n");
		    sb.Append("    (\r\n");
			sb.Append("	    try\r\n");
			sb.Append("	    (\r\n");
			sb.Append("		    append currentMaterialLibrary i.material\r\n");
			sb.Append("		    print (i.material.name + \" Material added to Material Library\")\r\n");
			sb.Append("	    )\r\n");
			sb.Append("	    catch()\r\n");
			sb.Append("		append toRemove i\r\n");
			sb.Append("    )\r\n");
            sb.Append(")\r\n");
            sb.Append("for i in toRemove do (delete i)\r\n");
            

            //Set FBX importer parameters
            sb.Append("try\r\n");
	        sb.Append("(\r\n");
            sb.Append("    FbxImporterSetParam \"Mode\" \"merge\"\r\n");
	        sb.Append(")\r\n");
	        sb.Append("catch(\r\n");
		    sb.Append("    print \"There was a problem with setting 'merge' option in the FBX importer. Try downloading a newer version.\"\r\n");
	        sb.Append(")\r\n");

            sb.Append("try\r\n");
            sb.Append("(\r\n");
            sb.Append("    FbxImporterSetParam \"SmoothingGroups\" false\r\n");
            sb.Append(")\r\n");
            sb.Append("catch(\r\n");
            sb.Append("    print \"There was a problem with setting of re-evalute normals option off\"\r\n");
            sb.Append(")\r\n");

	        sb.Append("try\r\n");
	        sb.Append("(\r\n");
			sb.Append(" FbxImporterSetParam \"Lights\" true\r\n");
		    sb.Append(")\r\n");
	        sb.Append("catch(\r\n");
		    sb.Append("    print \"There was a problem with setting 'Lights' option in the FBX importer. Try downloading a newer version.\"\r\n");
            sb.Append(")\r\n");

            //Import FBX file
            sb.Append("importFile (presetDir + sceneName + \".fbx\") #noPrompt using:FBXIMP\r\n");

            //Set back materials from the Material Library by name
            sb.Append("assignMaterials()\r\n");

            //Set shadowGenerator to shadowMap and bias to 0 in imported lights if possible
            sb.Append("for i in Lights where ((findString i.name \"ImportedLight\")!=undefined) do(\r\n");
            sb.Append(" try(\r\n");
            sb.Append("  i.shadowGenerator = shadowMap()\r\n");
            sb.Append("  i.shadowGenerator.mapbias = 0.0\r\n");
            sb.Append(" )\r\n");
            sb.Append(" catch()\r\n");
            sb.Append(")\r\n");

            sw.Write(sb.ToString());
            sw.Close();
        }

    }


    static public void PrepareMaya()
    {
        //Starter
        string sciezka;
    	if (Application.platform == RuntimePlatform.WindowsEditor)
        {
        	sciezka = "MaxFiles\\" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "_startup.mel";
        }
        else{
        	sciezka = "MaxFiles/" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "_startup.mel";
        }
        using (StreamWriter sw = new StreamWriter(sciezka))
        {
            StringBuilder sb = new StringBuilder();
            StreamReader s = File.OpenText(Application.dataPath + "/LightmappingTools/melEngine.mel");
            sb.AppendLine("global int $resArray[];");
            

            sb.AppendLine("global proc string savePath(){ return \"" + Application.dataPath + LightmappingTool.LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene)) + "\";}\n");
            sb.AppendLine("global proc string fbxFile(){ return \"" + LightmappingTool.MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".fbx" + "\";}\n");
            sb.AppendLine("global proc string melFile(){ return \"" + LightmappingTool.MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mel" + "\";}\n");
            sb.AppendLine("global proc string logoFile(){ return \"" + Application.dataPath + "/LightmappingTools/logo2.png" + "\";}\n");
            sb.AppendLine("global proc createFile(){");
            sb.AppendLine(" int $exists = `file -q -ex \"" + LightmappingTool.MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mb\"`;");
            sb.AppendLine(" if ($exists)");
            sb.AppendLine(" {");
            sb.AppendLine("     file -o \"" + LightmappingTool.MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mb\";");
            sb.AppendLine(" }");
            sb.AppendLine(" else { ");
            sb.AppendLine("     file -ea -type mayaBinary \"" + LightmappingTool.MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mb\";");
            sb.AppendLine("     file -o \"" + LightmappingTool.MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mb\";");
            sb.AppendLine(" }");
            sb.AppendLine("}");
            sb.AppendLine("evalDeferred -lp \"python(\\\"import sys\\\");python(\\\"import os\\\");python(\\\"sys.path.append('" + Application.dataPath + "/LightmappingTools/" + "')\\\");python(\\\"import timer\\\");\";\n\n");
            sb.AppendLine("evalDeferred -lp \"createFile\";");

            sb.Append(s.ReadToEnd());
            
            sb.AppendLine("global proc checkFBXdate()");
             sb.AppendLine("{");
             sb.AppendLine("	global float $fbxModDate;");
             sb.AppendLine("	print(\"Tick!\");");
             sb.AppendLine("	float $newDate = python(\"os.path.getmtime(\\\"\" + `fbxFile` + \"\\\")\");");
             sb.AppendLine("	if ($newDate!=$fbxModDate){");
             sb.AppendLine("		evalDeferred -lp \"source \\\"" + LightmappingTool.MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mel" + "\\\";\";");
             
             //sb.AppendLine("		source `melFile`;");
             sb.AppendLine("		$fbxModDate = $newDate;");
             sb.AppendLine("        evalDeferred -lp \"catchQuiet(`enableRT`)\";");
             sb.AppendLine("	}");
             sb.AppendLine("	startTimer(5, \"checkFBXdate;\");");
             sb.AppendLine("}");
            sb.AppendLine("evalDeferred -lp \"BatchBake\";");
            sb.AppendLine("evalDeferred -lp \"startTimer(0, \\\"checkFBXdate;\\\")\";");
            //sb.AppendLine("evalDeferred -lp \"source \\\"" + LightmappingTool.MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mel\\\";\";");
            sw.Write(sb.ToString());
            sw.Close();
        }




		if (Application.platform == RuntimePlatform.WindowsEditor)
        {
        	sciezka = "MaxFiles\\" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mel";
        }
        else{
        	sciezka = "MaxFiles/" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".mel";
        }

        using (StreamWriter sw = new StreamWriter(sciezka))
        {
            StringBuilder sb = new StringBuilder();
            
            
            sb.AppendLine("global int $resArray[];");
            int k = 0;
            foreach (System.Object item in LightmappingTool.res)
            {
                try
                {
                    sb.AppendLine("$resArray[" + k + "]=" + Convert.ToInt32(128 * Math.Pow(2, (Convert.ToInt32(item))))+";");
                    k++;
                    //sb.Append(",");
                }
                catch { }
            }
            
            sb.AppendLine("");
            sb.AppendLine("catchQuiet(`select -r \"ImportedObject*\"`);");
            sb.AppendLine("catchQuiet(`delete`);");

            sb.AppendLine("FBXImportMergeBackNullPivots -v false;");
            sb.AppendLine("FBXImport -file \"" + LightmappingTool.MaxFiles + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".fbx\";");

        	sb.AppendLine("string $allLights[] = `ls -type \"light\"`;");
        	sb.AppendLine("if (size($allLights)>0){");
    		sb.AppendLine(" for ($LiteNum=0; $LiteNum < size($allLights); $LiteNum++)");
    		sb.AppendLine(" {");
    		sb.AppendLine("     setAttr ($allLights[$LiteNum]+\".useRayTraceShadows\") true;");
    		sb.AppendLine(" }");
        	sb.AppendLine("}");

           
                        //sb.Append("catch(`evalDeferred -lp \"dummyProc\"`);\n");
            //sb.Append("evalDeferred -lp \"\";");
            //sb.Append("evalDeferred -lp \"if (catch(`dummyProc`)){source \\\"" + Application.dataPath + "/LightmappingTools/melEngine.mel\\\";}\";");
            //sb.Append("evalDeferred -lp \"source \\\"" + Application.dataPath + "/LightmappingTools/melEngine.mel\\\";\";");
            //sb.Append("evalDeferred -lp \"reimport;\";");
            sw.Write(sb.ToString());
            sw.Close();
        }
        
    }
}

