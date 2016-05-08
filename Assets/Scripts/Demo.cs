using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;

public class Demo : MonoBehaviour {

	private FileInfo m_SourceFile;
	private StreamReader m_Reader;
	private string m_string = " ";

	public EnhancedText text;
	public Button testButton;

	void Start () 
	{
		m_SourceFile = new FileInfo ("Assets/Resources/test.txt");
		m_Reader = m_SourceFile.OpenText ();
		m_string = m_Reader.ReadToEnd ();

		text.Text = m_string;

	}

	void OnEnable()
	{
		var star = MakeIcon ("Sprites/star");
		text.Prefabs.Add(star);

		var stamina = MakeIcon ("Sprites/stamina");
		text.Prefabs.Add (stamina);

		var heart = MakeIcon ("Sprites/heart");
		text.Prefabs.Add (heart);
		text.Prefabs.Add (Resources.Load<GameObject>("Prefabs/RotatingGameObject"));
	}

	GameObject MakeIcon(string spriteName)
	{
		var sprite = Resources.Load<Sprite>(spriteName);

		if (sprite == null)
			return null;

		var go = new GameObject ();
		go.name = sprite.name;
		var img = go.AddComponent<Image> ();
		img.sprite = sprite;
		img.SetNativeSize ();

		return go;
	}
}
