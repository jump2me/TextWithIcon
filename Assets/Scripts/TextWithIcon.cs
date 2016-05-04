using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TextWithIcon : Text 
{	
	private const float VERT_COUNT_CHAR = 6f;

	private Dictionary<float, GameObject> m_GameObjectByCharIndex = new Dictionary<float, GameObject> ();
	private Dictionary<int, UIVertex> m_UIVertexByIndex = new Dictionary<int, UIVertex>();

	public List<GameObject> Prefabs { get; private set; }

	protected override void Awake ()
	{
		base.Awake ();

		Prefabs = new List<GameObject> ();
	}

	protected override void OnPopulateMesh (VertexHelper toFill)
	{
		base.OnPopulateMesh (toFill);
		var uivertexList = new List<UIVertex> ();
		toFill.GetUIVertexStream (uivertexList);

		var modified = new List<UIVertex> ();
		GameObject go;
		var space = 0f;
		var lastLine = 0;
		var lineInfos = cachedTextGenerator.lines;

		for (int i = 0, max = uivertexList.Count; i < max; i++) {

			var vertex = uivertexList [i];
			var index = i / VERT_COUNT_CHAR;
			var numLine = GetNumLineFromCharIndex ((int)index);
			if (lastLine < numLine) {
				space = 0;
				lastLine = numLine;
			}

			if(true == m_GameObjectByCharIndex.TryGetValue(index, out go)) {
				
				m_UIVertexByIndex[(int)index] = vertex;

				if (go != null) {
					var rect = go.GetComponent<RectTransform> ().rect;
					var scale = cachedTextGenerator.lines[numLine].height / rect.height;
					space += rect.width * scale;
				}
					
			}

			vertex.position.x += space;

			modified.Add (vertex);
		}

		toFill.Clear ();
		toFill.AddUIVertexTriangleStream (modified);
	}

	private int GetNumLineFromCharIndex(int charIndex)
	{
		var lines = cachedTextGenerator.lines;
		for (int i = 0, max = lines.Count; i < max; i++) {
			var line = lines [i];

			if (i == max - 1 && line.startCharIdx <= charIndex) {
				return i;
			}
			else {
				var nextLine = lines [i + 1];
				if (line.startCharIdx <= charIndex && nextLine.startCharIdx > charIndex)
					return i;
			}
		}
		return -1;
	}

	public string Text {
		get {
			return text;
		}
		set {

			value = ParseText (value);

			WillUpdateText (value);
		}
	}

	void WillUpdateText(string value)
	{
		StartCoroutine (DoUpdateText (value));
	}

	IEnumerator DoUpdateText(string value)
	{
		yield return new WaitForEndOfFrame ();
		text = value;
		WillCreateGameObjects ();
	}

	private string ParseText(string input)
	{
		var delimiter = new char[]{ '#' };
		var tokens = input.Split (delimiter);

		var result = string.Empty;
		var charIndex = 0;
		for (int i = 0, max = tokens.Length; i < max; i++) {
			var token = tokens [i];
			var go = Prefabs.Find(e => e.name == token);

			if (go != null) {
				m_GameObjectByCharIndex [charIndex] = go;
			} else {
				result += token;
				charIndex += token.Length;
			}
		}

		return result + " ";
	}

	private void WillCreateGameObjects()
	{
		StartCoroutine (DoCreateGameObjects ());
	}

	private IEnumerator DoCreateGameObjects()
	{
		yield return new WaitForEndOfFrame ();

		var space = 0f;
		var lastLine = 0;
		var lineInfos = cachedTextGenerator.lines;
		foreach (var pair in m_UIVertexByIndex) {

			var numLine = GetNumLineFromCharIndex (pair.Key);
			if (lastLine < numLine) {
				space = 0;
				lastLine = numLine;
			}

			var lineInfo = lineInfos[numLine];

			var prefab = m_GameObjectByCharIndex [pair.Key];

			var icon = Instantiate (prefab);
			var rect = icon.GetComponent<RectTransform>().rect;
			var scale = lineInfo.height / rect.height;

			icon.transform.SetParent (transform);

			var vertPosition = pair.Value.position;
			var posX = vertPosition.x + rect.width * scale * 0.5f + space;
			var posY = lineInfo.topY - lineInfo.height * 0.5f;

			icon.transform.localPosition = new Vector3(posX, posY);
			icon.transform.localScale = new Vector3 (scale, scale);
			space += rect.width * scale;	
		}
	}
}