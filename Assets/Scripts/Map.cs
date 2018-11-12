using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;


public class Map {
	//Dimensions of the mesh in voxels
	public const int height = 64;
	public const int width = 64;
	public const int depth = 4;
	
	/// <summary>
	/// Layers that can be saved/loaded as png and represents blockIDs of the mesh that will be generated
	/// layers[0] is the bottom layer, layers[(depth-1)] is the top layer
	/// </summary>
	public static IntArrayFromTexture[] layers { get; private set; }
	
	
	public Map()
	{
		layers = new IntArrayFromTexture[depth];
		for (int i = 0; i < layers.Length; i++)
		{
			layers[i] = new IntArrayFromTexture();
			//Load the texture from hard drive if it exist
			Texture2D layer = HDDResources.LoadImage("layer" + i);
			if (layer)
			{
				layers[i].SetTexture(layer);
			}
			else //Create a black texture otherwise
			{
				layer = new Texture2D(width, height);
				Color[] colors = new Color[width*height];
				for (int j = 0; j < colors.Length; j++)
					colors[j] = Color.black;					

				layer.SetPixels(colors);
				layers[i].SetTexture(layer);
			}
		}
	}
	/// <summary>
	/// Save all layers as a texture in the hard drive as a png image
	/// </summary>
	public void SaveToHDD()
	{
		for (int i = 0; i < layers.Length; i++)
			HDDResources.SaveImage(layers[i].GetTexture(), "layer" + i);
	}
}
