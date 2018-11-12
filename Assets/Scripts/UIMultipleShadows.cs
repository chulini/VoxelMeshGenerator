using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMultipleShadows : MonoBehaviour
{
	[Range(2,6)]
	[SerializeField] int shadows = 6;
	[SerializeField] Color shadowColor = Color.yellow;
	void Start () {
		for (int i = 0; i < shadows; i++)
		{
			Shadow shadow = gameObject.AddComponent<Shadow>();
			shadow.effectDistance = new Vector2(i+1,-(i+1));
			shadow.effectColor = shadowColor;
		}	
	}
	
	
}
