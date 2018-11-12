using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public static class Palette
{
	static Color32[] _colors;
	public static Color32[] colors
	{
		get
		{
			if (_colors == null)
				SetColorsArray();
			return _colors;
		}
	}
	public static readonly int cols = 5;
	public static readonly int rows = 4;

	/// <summary>
	/// Saves a custom color palette in persistence data path as a png file
	/// </summary>
	public static void SavePaletteInHardDrive()
	{

		SetColorsArray();
		Texture2D texture = new Texture2D(cols, rows);
		texture.SetPixels32(colors);
		texture.filterMode = FilterMode.Point;
		texture.Apply();
		HDDResources.SaveImage(texture, "palette");
	}

	static void SetColorsArray()
	{
		_colors = new Color32[20];
		_colors[0] = Color.black;
		_colors[1] = new Color32((byte) 244, (byte) 66, (byte) 54, (byte) 255);
		_colors[2] = new Color32((byte) 234, (byte) 30, (byte) 99, (byte) 255);
		_colors[3] = new Color32((byte) 156, (byte) 40, (byte) 177, (byte) 255);
		_colors[4] = new Color32((byte) 103, (byte) 59, (byte) 183, (byte) 255);
		_colors[5] = new Color32((byte) 63, (byte) 81, (byte) 181, (byte) 255);
		_colors[6] = new Color32((byte) 3, (byte) 169, (byte) 245, (byte) 255);
		_colors[7] = new Color32((byte) 0, (byte) 188, (byte) 213, (byte) 255);
		_colors[8] = new Color32((byte) 0, (byte) 151, (byte) 136, (byte) 255);
		_colors[9] = new Color32((byte) 76, (byte) 176, (byte) 80, (byte) 255);
		_colors[10] = new Color32((byte) 139, (byte) 194, (byte) 74, (byte) 255);
		_colors[11] = new Color32((byte) 205, (byte) 220, (byte) 57, (byte) 255);
		_colors[12] = new Color32((byte) 255, (byte) 235, (byte) 60, (byte) 255);
		_colors[13] = new Color32((byte) 254, (byte) 193, (byte) 7, (byte) 255);
		_colors[14] = new Color32((byte) 255, (byte) 151, (byte) 0, (byte) 255);
		_colors[15] = new Color32((byte) 244, (byte) 66, (byte) 54, (byte) 255);
		_colors[16] = new Color32((byte) 254, (byte) 87, (byte) 34, (byte) 255);
		_colors[17] = new Color32((byte) 121, (byte) 85, (byte) 71, (byte) 255);
		_colors[18] = new Color32((byte) 158, (byte) 158, (byte) 158, (byte) 255);
		_colors[19] = new Color32((byte) 96, (byte) 125, (byte) 139, (byte) 255);
	}

	public static Color32 CoordToColor(int x, int y)
	{
		SetColorsArray();
		return colors[(y * cols) + x];
	}

	/// <summary>
	/// Returns the coordinates of a color in the palette
	/// </summary>
	/// <param name="color"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	public static void ColorToCoord(Color32 color, out int x, out int y)
	{
		x = y = 0;
		for (int i = 0; i < colors.Length; i++)
		{
			if (color.r == colors[i].r && color.g == colors[i].g && color.b == colors[i].b && color.a == colors[i].a)
			{
				x = i % cols;
				y = i / cols;
				return;
			}
		}
	}
}
