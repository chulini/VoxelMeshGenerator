using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simulates a bidimentional int array using a texture2D.
/// Every pixel color can be converted to an int value an viceversa.
/// Very useful to serialize a voxel layers on a quick and compressed way.
/// </summary>
public class IntArrayFromTexture{
	//The max value can be stored on a RGB color = white
	public static readonly int maxValue = ColorToInt(Color.white); 
	public int width = -1;
	public int height = -1;
	public Color[] pixels
	{
		get
		{
			return _pixels;
		}
	}
	Color[] _pixels;
	public IntArrayFromTexture()
	{
		_pixels = new Color[1];
		_pixels[0] = Color.black;
	}
	public void SetTexture(Texture2D texture)
	{
		_pixels = texture.GetPixels(0, 0, texture.width, texture.height);
		width = texture.width;
		height = texture.height;
	}
	
	public Texture2D GetTexture(bool replaceBlackWithTransparent = false)
	{
		Texture2D texture = new Texture2D(width, height);

		Color[] temp = _pixels;
		if (replaceBlackWithTransparent)
		{
			temp = _pixels.Clone() as Color[];
			for (int i = 0; i < temp.Length; i++)
				if (temp[i] == new Color32(0, 0, 0, 255))
					temp[i] = new Color32(0, 0, 0, 0);
		}
		texture.SetPixels(temp);
		texture.filterMode = FilterMode.Point;
		texture.Apply();
		return texture;
	} 
	
	/// <param name="x">X from left to right</param>
	/// <param name="y">Y from bottom to top</param>
	/// <returns>Returns the integer value of the color in the x,y coordinate</returns>
	public int GetInt(int x, int y)
	{
		if (width > 0)
		{
			y = width - 1 - y;
			if (((width) * y) + x < _pixels.Length)
			{
				Color32 color = _pixels[((width) * y) + x];
				return ColorToInt(color);
			}

			return 0;
		}
		return 0;
	}
	
	public void SetInt(int x, int y, int value)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
		{
			y = width - 1 - y;
			_pixels[((width) * y) + x] = IntToColor(value);
		}
	}
	
	public static Color32 IntToColor(int i)
	{
		byte r = (byte)(i / 65536);
		int rem = (i % 65536);

		byte g = (byte)(rem / 256);
		rem = (rem % 256);

		byte b = (byte)rem;
		Color32 col = new Color32(r, g, b, 255);
		return col;
	}

	public static int ColorToInt(Color32 color)
	{
		return color.r * 65536 + color.g * 256 + color.b;
	}
}