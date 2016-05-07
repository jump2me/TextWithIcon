using System;
using System.Xml;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class EnhancedText : Text
{	
	//SerializedProperties
	public bool iconAutoFit = false;
	public Vector2 scaleMultiplier = Vector2.one;
	public Vector2 offset = Vector2.zero;

	//Icon Objects to use
	public List<GameObject> Prefabs = new List<GameObject>();
	public List<Sprite> Sprites = new List<Sprite> ();

	//Fields
	private const int VERT_COUNT_CHAR = 6;

	private List<GameObject> m_ObjectPool = new List<GameObject>();
	private List<GameObject> m_using = new List<GameObject> ();
	private Dictionary<float, GameObject> m_PrefabByCharIndex = new Dictionary<float, GameObject> ();
	private Dictionary<int, UIVertex> m_UIVertexByIndex = new Dictionary<int, UIVertex>();

	private Vector2 m_scaleFactorByScreenMode = Vector2.one;

	protected override void Awake ()
	{
		base.Awake ();

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

		m_UIVertexByIndex.Clear ();

		var uivertexList = new List<UIVertex> ();
		toFill.GetUIVertexStream (uivertexList);

		var modifiedVertexList = new List<UIVertex> ();
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

			if(true == m_PrefabByCharIndex.TryGetValue(index, out prefab)) {
				
				m_UIVertexByIndex[(int)index] = vertex;

				if (prefab != null) {
					var rect = prefab.GetComponent<RectTransform> ().rect;
					var scale = GetScaleRatio(lineInfos[numOfLine].height, rect.height);
					space += rect.width * scale;
				}
			}

			vertex.position.x += space;

			modifiedVertexList.Add (vertex);
		}
			
		toFill.Clear ();
		toFill.AddUIVertexTriangleStream (modifiedVertexList);
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
			value = ParseText (value + " ");

			WillUpdateText (value);
		}
	}

	void WillUpdateText(string value)
	{
		text = string.Empty;
		StartCoroutine (DoUpdateText (value));
	}

	IEnumerator DoUpdateText(string value)
	{
		text = value;

		yield return new WaitForEndOfFrame ();

		WillCreateGameObjects ();
	}

	private string ParseText(string input)
	{
		m_PrefabByCharIndex.Clear ();

		var pattern = @"<icon='(.+?)' />";
		var matches = Regex.Matches (input, pattern);
		if (matches.Count == 0)
			return input;

		var result = string.Empty;
		var lengthOfLastMatch = 0;

		foreach (Match match in matches) {
			
			var length = match.ToString ().Length;

			for (int i = 0, max = match.Groups.Count; i < max; i++) {

				var group = match.Groups [i];
				var value = group.Value;

				var prefab = Prefabs.Find (e => e.name == value);
				if (prefab != null) {
					
					result = Regex.Replace (input, pattern, string.Empty);

					m_PrefabByCharIndex [match.Index - lengthOfLastMatch] = prefab;

					lengthOfLastMatch += length;
				}
			}
		}

		return result;
	}

	private void WillCreateGameObjects()
	{
		StartCoroutine (DoCreateGameObjects ());
	}

	private IEnumerator DoCreateGameObjects()
	{
		m_ObjectPool.ForEach (e => e.SetActive (false));
		m_using.Clear ();

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
			GameObject prefab;
			if (false == m_PrefabByCharIndex.TryGetValue (pair.Key, out prefab)) {
				Debug.Log(string.Format("cannot find a prefab with the index of [{0}]", pair.Key));
				yield break;
			}

			var icon = FetchGameObject(prefab);
			m_using.Add (icon);

			var rect = icon.GetComponent<RectTransform>().rect;
			var scale = GetScaleRatio(lineInfo.height, rect.height);

			icon.transform.SetParent (transform);

			var vertexPosition = pair.Value.position;
			var posX = vertexPosition.x + rect.width * scale * 0.5f + space + offset.x;
			var posY = (lineInfo.topY - lineInfo.height * 0.5f) * m_scaleFactorByScreenMode.y + offset.y;

			icon.transform.localPosition = new Vector3(posX, posY);
			icon.transform.localScale = new Vector3 (scale * scaleMultiplier.x, scale * scaleMultiplier.y, scale);
			space += rect.width * scale;	
		}
	}

	private float GetScaleRatio(float heightOfLine, float heightOfIcon)
	{
		if (iconAutoFit == false)
			return 1f;

		return heightOfLine / heightOfIcon * m_scaleFactorByScreenMode.y;
	}

	private GameObject FetchGameObject(GameObject prefab)
	{
		var go = m_ObjectPool.Find (e => m_using.Contains(e) == false && e.name == prefab.name);
		if (go == null) {
			return InstantiateGameObject (prefab);
		} else if (go.activeInHierarchy == false){
			go.SetActive (true);
			return go;
		}

		return InstantiateGameObject (prefab);
	}

	private GameObject InstantiateGameObject(GameObject prefab)
	{
		var go = Instantiate (prefab);
		go.name = prefab.name;
		m_ObjectPool.Add (go);
		return go;
	}

}