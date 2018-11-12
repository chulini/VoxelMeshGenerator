using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIImagePointerClickToPixel ))]
public class UIColorPicker : MonoBehaviour
{
	[SerializeField] Image _previewImage;
	[SerializeField] Text _colorDetailsText;
	
	UIImagePointerClickToPixel _pointerPixelEvents;
	public static int paintingBlockID;
	void Awake()
	{
		_pointerPixelEvents = GetComponent<UIImagePointerClickToPixel>();
		_pointerPixelEvents.OnPointerDown.AddListener((x, y, color) =>
		{
			SelectColor(color);
		});
		_pointerPixelEvents.OnPointerDrag.AddListener((x, y, color) =>
		{
			SelectColor(color);
		});
		_pointerPixelEvents.OnPointerDown.Invoke(2,2,Palette.CoordToColor(2,2));
	}

	void SelectColor(Color color)
	{
		_previewImage.color = color;
		paintingBlockID = IntArrayFromTexture.ColorToInt(color);
		_colorDetailsText.text = IntArrayFromTexture.IntToColor(paintingBlockID).ToString() + "\t\tint(" + paintingBlockID.ToString()+")";
	}
}
