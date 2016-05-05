using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(EnhancedText))]
public class EnhancedTextEditor : Editor {

	[Header("Icon Offset")]
	SerializedProperty scale;
	SerializedProperty offset;
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();
	}
}
