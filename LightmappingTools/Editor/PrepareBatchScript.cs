using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
class PrepareBatchScript
{
    static public void Prepare()
    {
        using (StreamWriter sw = new StreamWriter("MaxFiles\\"+Path.GetFileNameWithoutExtension(EditorApplication.currentScene)+".ms"))
        {
            StringBuilder sb = new StringBuilder();
            
            if (LightmapAdvanced.selectedApp != 0)
            {
                sb.Append("if (preserveMaterials==undefined) then\r\n");
                sb.Append("(\r\n");
                sb.Append(" fileIn(\"" + Application.dataPath + "/LightmappingTools/maxscriptEngine.ms\")\r\n");
                sb.Append(")\r\n");
            }
            sb.Append("global SaveDir=\"" + Application.dataPath + LightmapAdvanced.LMdir.Replace("<sceneName>", Path.GetFileNameWithoutExtension(EditorApplication.currentScene)) + "\"\r\n");
            sb.Append("global presetDir=\"" + Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "MaxFiles/\"\r\n");
            sb.Append("global sceneName=\"" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "\"\r\n");
            sb.Append("global fileFormat=\"" + LightmapAdvanced.fileFormat[LightmapAdvanced.formatChoice] + "\"\r\n");
            sb.Append("if (presetFile==\"\") then (\r\n");
            if (LightmapAdvanced.presetFile != 0)
            {
                sb.Append("global presetFile=\"" + LightmapAdvanced.presetArray[LightmapAdvanced.presetFile - 1] + "\"\r\n");
            }
            //else
            //{
            //    sb.Append("global presetFile=\"\"\r\n");
            //}
            sb.Append(")\r\n");

            sb.Append("if (matLibFile==\"\") then (\r\n");
            if (LightmapAdvanced.matLibFile || LightmapAdvanced.presetFile!=0)
            {
                sb.Append("global matLibFile=\"" + Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "MaxFiles/" + Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + "_matlib.mat\"\r\n");

            }
            //else
            //{
            //    sb.Append("global matLibFile=\"\"\r\n");
            //}
            sb.Append(")\r\n");
            sb.Append("try(\r\n");
            sb.Append("loadMaterialLibrary matLibFile\r\n");
            sb.Append(")\r\n");
            sb.Append("catch()\r\n");

            sb.Append("global conversorPath=\"" + Application.dataPath + "/LightmappingTools/conversor.mse\"\r\n");
            sb.Append("global resArray=#(");
            foreach (int item in LightmappingTool.res)
            {
                sb.Append(Convert.ToInt32(128 * Math.Pow(2, (Convert.ToInt32(item)))));
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")\r\n");
           
            if (!LightmappingTool.haveBeenExported)
            {
                sb.Append("if (presetFile!=\"\") then \r\n");
                sb.Append("(\r\n");
                sb.Append(" ImportScene()\r\n");
                sb.Append(")\r\n");
                sb.Append("else\r\n");
                sb.Append("(\r\n");
                sb.Append(" createDialog inputBox\r\n");
                sb.Append(")\r\n");
            }
            if (LightmapAdvanced.rendererChoice != 0 && LightmapAdvanced.presetFile != 0)
            {
                sb.Append("if (presetFile==\"\") then \r\n");
                sb.Append("(\r\n");
                sb.Append(" assignRenderer()\r\n");
                sb.Append(" assignMaterials()\r\n");
                sb.Append(")\r\n");
            }
            /*
            sb.Append("try(\r\n");
            sb.Append("closeUtility MyUtil\r\n");
            sb.Append(")\r\n");
            sb.Append("catch()\r\n");
            sb.Append("try(\r\n");
            sb.Append("openUtility MyUtil\r\n");
            sb.Append(")\r\n");
            sb.Append("catch()\r\n");
             */
            if (LightmapAdvanced.autostartBaking)
            {
                sb.Append("BakeObjects()\r\n");
            }
            sw.Write(sb.ToString());
            sw.Close();
        }

    }
}

