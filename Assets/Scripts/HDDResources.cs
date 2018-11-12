using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Is in charge of load and save string, texture2d and meshes to/from the hard drive
/// </summary>
public static class HDDResources
{
    public static readonly bool verbose = true;
    public static void SaveImage(Texture2D texture, string filename)
    {

        filename = filename.Replace(":", "-");


        string path = Application.persistentDataPath + "/" + filename + ".png";
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        if(verbose)
            Debug.Log("Image saved @ " + path);


    }

    public static Texture2D LoadImage(string filename)
    {

        filename = filename.Replace(":", "-");

        string path = Application.persistentDataPath + "/" + filename + ".png";
        byte[] bytes;
        try
        {
            bytes = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            if(verbose)
                Debug.Log("Image loaded from " + path);
            return texture;
        }
        catch (System.Exception e)
        {
            if(verbose)
                Debug.Log("Exception: " + e.Message);
            return null;
        }


    }

    public static void SaveTextFile(string filename, string[] lines)
    {
        filename = filename.Replace(":", "-");
        string path = Application.persistentDataPath + "/" + filename + ".txt";
        System.IO.File.WriteAllLines(path, lines);
        if(verbose)
            Debug.Log("Saved text file @ " + path);
    }

    public static string[] LoadTextFile(string filename)
    {

        try
        {
            filename = filename.Replace(":", "-");
            string path = Application.persistentDataPath + "/" + filename + ".txt";
            return System.IO.File.ReadAllLines(path);
        }
        catch (System.Exception e)
        {
            if(verbose)
                Debug.Log("LoadTextFile Exception: " + e.Message);
            return null;
        }
    }

    public static void SaveMesh(string filename, Mesh mesh)
    {
        filename = filename.Replace(":", "-");
        try
        {
            Byte[] bytes = MeshSerializer.WriteMesh(mesh, true);
            string path = Application.persistentDataPath + "/" + filename + ".mesh";
            System.IO.File.WriteAllBytes(path, bytes);
            if(verbose)
                Debug.Log("Mesh saved @ " + path);
        }
        catch (System.Exception e)
        {
            if(verbose)
                Debug.Log("Save Mesh Exception: " + e.Message);
        }

    }

    public static Mesh LoadMesh(string filename)
    {

        filename = filename.Replace(":", "-");
        string path = Application.persistentDataPath + "/" + filename + ".mesh";
        try
        {
            Byte[] bytes = System.IO.File.ReadAllBytes(path);
            return MeshSerializer.ReadMesh(bytes);
        }
        catch (Exception e)
        {
            if(verbose)
                Debug.Log("Load Mesh Exception: " + e.Message);
        }

        return null;
    }
}
