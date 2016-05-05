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

		var heart = MakeIcon ("heart");
		textWithIcon.Prefabs.Add (heart);

		textWithIcon.Text = "최대 #star#6까지 진화 가능합니다.\n최대 #heart#이 +300 올랐습니다.\n#stamina#가 부족해 입장할 수 없습니다.";
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
