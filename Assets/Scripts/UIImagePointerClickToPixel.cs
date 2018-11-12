using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIImagePointerClickToPixel : MonoBehaviour, IPointerClickHandler, IDragHandler
{
	public UIImagePointerClickToPixelEvent OnPointerDown = new UIImagePointerClickToPixelEvent();
	public UIImagePointerClickToPixelEvent OnPointerDrag = new UIImagePointerClickToPixelEvent();

	RectTransform _myRectTransform;

	/// <summary>
	/// Lazy assign myTexture
	/// </summary>
	Texture2D _myTexture = null;
	Texture2D myTexture
	{
		get
		{
			if (_myTexture == null)
			{
				if (GetComponent<RawImage>())
					_myTexture = GetComponent<RawImage>().texture as Texture2D;	
				else if (GetComponent<Image>())
					_myTexture = GetComponent<Image>().sprite.texture;
			}
			return _myTexture;
		}
	}


	void Awake()
	{
		_myRectTransform = GetComponent<RectTransform>();

	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int x, y;
		Color clickedColor;
		GetClickedPixel(eventData, out x, out y, out clickedColor);
		this.OnPointerDown.Invoke(x, y, clickedColor);
	}

	public void OnDrag(PointerEventData eventData)
	{
		int x, y;
		Color clickedColor;
		GetClickedPixel(eventData, out x, out y, out clickedColor);
		this.OnPointerDrag.Invoke(x, y, clickedColor);
	}

	/// <summary>
	/// Sets x, y coordinates and color clicked in eventData
	/// </summary>
	/// <param name="eventData">EventData of the click event</param>
	/// <param name="x">X Coordinate clicked</param>
	/// <param name="y">Y Coordinate clicked</param>
	/// <param name="clickedColor">Color clicked</param>
	void GetClickedPixel(PointerEventData eventData, out int x, out int y, out Color clickedColor)
	{
		x = -1;
		y = -1;
		clickedColor = Color.magenta;

		Vector2 positionInRect;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_myRectTransform, eventData.position,
			eventData.pressEventCamera,
			out positionInRect))
		{
			//positionInRect is relative to the center so we change it to top left
			positionInRect = positionInRect.WithY(positionInRect.y);
			positionInRect += new Vector2(_myRectTransform.rect.width, _myRectTransform.rect.height) * .5f;

			//With positionInRect we calculate the pixel coordenate that is been clicked
			x = Mathf.FloorToInt((positionInRect.x / _myRectTransform.rect.width) * (float) myTexture.width);
			y = Mathf.FloorToInt((positionInRect.y / _myRectTransform.rect.height) * (float) myTexture.height);
			
			clickedColor = myTexture.GetPixel(x, y);
		}
	}

	public class UIImagePointerClickToPixelEvent : UnityEvent<int, int, Color>
	{
	}

}
