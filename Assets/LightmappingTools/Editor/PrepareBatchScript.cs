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
    static public void Prepare()
    {
        using (StreamWriter sw = new StreamWriter("MaxFiles\\"+Path.GetFileNameWithoutExtension(EditorApplication.currentScene)+".ms"))
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
		        sb.Append("     messagebox(\"Keep on mind that lightmapping tool doesn't support Scanline renderer, while Mental Ray was not found in your system.\")\r\n");
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
}

