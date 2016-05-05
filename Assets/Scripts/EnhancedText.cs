using System;
using System.Xml;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnhancedText : Text
{	
	public Vector2 scale = Vector2.one;
	public Vector2 offset = Vector2.zero;

	public List<GameObject> Prefabs { get; private set; }
	public List<Sprite> Sprites { get; private set; }

	private const int VERT_COUNT_CHAR = 6;
	private const float MARGIN_PIXEL = 3f;

	private Dictionary<float, GameObject> m_GameObjectByCharIndex = new Dictionary<float, GameObject> ();
	private Dictionary<int, UIVertex> m_UIVertexByIndex = new Dictionary<int, UIVertex>();

	private Vector2 m_scaleFactorByScreenMode = Vector2.one;

	protected override void Awake ()
	{
		base.Awake ();

		Prefabs = new List<GameObject> ();
		Sprites = new List<Sprite> ();

		var canvasScaler = GameObject.FindObjectOfType<CanvasScaler> ();

		var x = 1f;
		var y = 1f;
		switch (canvasScaler.uiScaleMode) {
		case CanvasScaler.ScaleMode.ScaleWithScreenSize:
			var referenceWidth = canvasScaler.referenceResolution.x;
			var referenceHeight = canvasScaler.referenceResolution.y;
			var currentWidth = (float)Screen.width;
			var currentHeight = (float)Screen.height;
			x = referenceWidth / currentWidth;
			y = referenceHeight / currentHeight;
			break;
		}

		m_scaleFactorByScreenMode = new Vector2 (x, y);
	}

	protected override void OnPopulateMesh (VertexHelper toFill)
	{
		base.OnPopulateMesh (toFill);

		// get characters uivertex list without gameObjects' spaces.
		var uivertexList = new List<UIVertex> ();
		toFill.GetUIVertexStream (uivertexList);

		// extra spaces for gameObjects will be added.
		var modified = new List<UIVertex> ();
		GameObject prefab;

		var space = 0f;
		var lastNumOfLine = 0;
		var lineInfos = cachedTextGenerator.lines;

		for (int i = 0, max = uivertexList.Count; i < max; i++) {

			var vertex = uivertexList [i];
			var index = i / (float)VERT_COUNT_CHAR;
			var numOfLine = GetNumOfLineFromCharIndex ((int)index);
			if (lastNumOfLine < numOfLine) {
				space = 0;
				lastNumOfLine = numOfLine;
			}

			if(true == m_GameObjectByCharIndex.TryGetValue(index, out prefab)) {
				
				m_UIVertexByIndex[(int)index] = vertex;

				if (prefab != null) {
					var rect = prefab.GetComponent<RectTransform> ().rect;
					var scale = lineInfos[numOfLine].height / rect.height * m_scaleFactorByScreenMode.y;
					space += rect.width * scale + MARGIN_PIXEL;
				}
					
			}

			vertex.position.x += space;

			modified.Add (vertex);
		}

		toFill.Clear ();
		toFill.AddUIVertexTriangleStream (modified);
	}

	private int GetNumOfLineFromCharIndex(int charIndex)
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
			var prefab = Prefabs.Find(e => e.name == token);

			if (prefab != null) {
				m_GameObjectByCharIndex [charIndex] = prefab;
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
		var lastNumOfLine = 0;
		var lineInfos = cachedTextGenerator.lines;
		foreach (var pair in m_UIVertexByIndex) {

			var numOfLine = GetNumOfLineFromCharIndex (pair.Key);
			if (lastNumOfLine < numOfLine) {
				space = 0;
				lastNumOfLine = numOfLine;
			}

			var lineInfo = lineInfos[numOfLine];
			var prefab = m_GameObjectByCharIndex [pair.Key];

			var icon = Instantiate (prefab);
			var rect = icon.GetComponent<RectTransform>().rect;
			var scale = lineInfo.height / rect.height * m_scaleFactorByScreenMode.y;

			icon.transform.SetParent (transform);

			var vertexPosition = pair.Value.position;
			var posX = vertexPosition.x + rect.width * scale * 0.5f + space + offset.x;
			var posY = (lineInfo.topY - lineInfo.height * 0.5f) * m_scaleFactorByScreenMode.y + offset.y;

			icon.transform.localPosition = new Vector3(posX, posY);
			icon.transform.localScale = new Vector3 (scale, scale, scale);
			space += rect.width * scale + MARGIN_PIXEL;	
		}
	}
}