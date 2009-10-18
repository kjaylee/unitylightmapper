using UnityEngine;
using UnityEditor;
using System.Collections;

public class DeletePrefs : MonoBehaviour {

	[MenuItem("Tools/DeletePreferences")]
	static void Delete(){
		Debug.Log("Settings Deleted");
		EditorPrefs.DeleteKey("LMT_InstalledApps");
	}
	
	// Use this for initialization
}
