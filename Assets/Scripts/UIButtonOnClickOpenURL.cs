using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonOnClickOpenURL : MonoBehaviour
{

	[SerializeField] string _url;
	Button _button;

	void Awake()
	{
		_button = GetComponent<Button>();
	}

	void Start()
	{
		_button.onClick.AddListener(() =>
		{
			Application.OpenURL(_url);
		});
	}
	
}
