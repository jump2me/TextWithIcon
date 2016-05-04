using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Testing : MonoBehaviour {

	public TextWithIcon textWithIcon;
	void Start () {
	
		var star = MakeIcon ("star");
		textWithIcon.Prefabs.Add(star);

		var stamina = MakeIcon ("stamina");
		textWithIcon.Prefabs.Add (stamina);

		textWithIcon.Text = "You need #star# 6\n You don't have enough #stamina# #star# #star#";
	}

	GameObject MakeIcon(string spriteName)
	{
		var sprite = Resources.Load<Sprite>(spriteName);

		if (sprite == null)
			return null;

		var go = new GameObject ();
		go.name = spriteName;
		var img = go.AddComponent<Image> ();
		img.sprite = sprite;
		img.SetNativeSize ();

		return go;
	}
}
