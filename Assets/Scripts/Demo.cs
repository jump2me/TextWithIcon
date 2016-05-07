using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Demo : MonoBehaviour {

	public EnhancedText text;
	public Button testButton;

	private string[] m_Texts = new string[]{ "click to a demo", 
		"<icon='star' />this is a <icon='star' />.", 
		"this is a <icon='heart' />.", 
		"this is a <icon='star' /> and a <icon='heart' />.", 
		"<icon='RotatingGameObject' /> <icon='RotatingGameObject' /> <icon='RotatingGameObject' /> <icon='RotatingGameObject' />",
		"this is a <icon='star' /> and \na <icon='heart' /> with multiple lines."};

	void Start () {

		var star = MakeIcon ("Sprites/star");
		text.Prefabs.Add(star);

		var stamina = MakeIcon ("Sprites/stamina");
		text.Prefabs.Add (stamina);

		var heart = MakeIcon ("Sprites/heart");
		text.Prefabs.Add (heart);
		text.Prefabs.Add (Resources.Load<GameObject>("Prefabs/RotatingGameObject"));
	}

	void OnEnable()
	{
		text.Text = "click to demo.";
	}

	void Awake()
	{
		testButton.onClick.AddListener (Test);
	}

	int indexer = 0;
	void Test()
	{
		text.Text = m_Texts [indexer++];

		if (indexer == 6)
			indexer = 0;
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
