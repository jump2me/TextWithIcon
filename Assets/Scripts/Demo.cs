using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Demo : MonoBehaviour {

	public EnhancedText text;
	void Start () {
	
		var star = MakeIcon ("Sprites/star");
		text.Prefabs.Add(star);

		var stamina = MakeIcon ("Sprites/stamina");
		text.Prefabs.Add (stamina);

		var heart = MakeIcon ("Sprites/heart");
		text.Prefabs.Add (heart);

		text.Prefabs.Add (Resources.Load<GameObject>("Prefabs/RotatingGameObject"));

		text.Text = "최대 #RotatingGameObject#6까지 진화#star# 가능합니다.\n최대 #heart#이 +300 올랐습니다.\n#stamina#가 부족해 입장할 수 없습니다.";
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
