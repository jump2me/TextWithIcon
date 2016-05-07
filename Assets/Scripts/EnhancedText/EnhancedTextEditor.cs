using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(EnhancedText))]
public class EnhancedTextEditor : Editor {

	SerializedProperty iconAutoFit;
	SerializedProperty scaleMultiplier;
	SerializedProperty offset;

	SerializedProperty m_ObjectPool;
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();
	}
}
