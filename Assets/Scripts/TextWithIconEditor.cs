using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TextWithIcon))]
public class TextWithIconEditor : Editor {

	public SerializedProperty sprites;

}
