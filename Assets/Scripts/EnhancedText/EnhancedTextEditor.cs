using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(EnhancedText))]
public class EnhancedTextEditor : Editor {

	SerializedProperty iconAutoFit;
	SerializedProperty scaleMultiplier;
	SerializedProperty offset;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();
	}
}
