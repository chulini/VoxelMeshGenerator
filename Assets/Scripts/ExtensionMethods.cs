using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class ExtensionMethods
{

	public static Color WithAlpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, alpha);
	}

	public static void ResetTransformation(this Transform trans)
	{
		trans.position = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = new Vector3(1, 1, 1);
	}

	public static Vector3 WithX(this Vector3 v, float x)
	{
		return new Vector3(x, v.y, v.z);
	}

	public static Vector3 WithY(this Vector3 v, float y)
	{
		return new Vector3(v.x, y, v.z);
	}

	public static Vector3 WithZ(this Vector3 v, float z)
	{
		return new Vector3(v.x, v.y, z);
	}

	public static Vector2 WithX(this Vector2 v, float x)
	{
		return new Vector2(x, v.y);
	}

	public static Vector2 WithY(this Vector2 v, float y)
	{
		return new Vector2(v.x, y);
	}

	public static Vector3 Round(Vector3 v)
	{
		return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
	}

	public static void LocalResetTransformation(this Transform trans)
	{
		trans.localPosition = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = new Vector3(1, 1, 1);
	}

	public static Quaternion WithEulerX(this Quaternion q, float v)
	{
		return Quaternion.Euler(v, q.eulerAngles.y, q.eulerAngles.z);
	}

	public static Quaternion WithEulerY(this Quaternion q, float v)
	{
		return Quaternion.Euler(q.eulerAngles.x, v, q.eulerAngles.z);
	}

	public static Quaternion WithEulerZ(this Quaternion q, float v)
	{
		return Quaternion.Euler(q.eulerAngles.x, q.eulerAngles.y, v);
	}

	public static IEnumerator WaitForRealSeconds(float time)
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}

	public static string FirstCharToUpper(string input)
	{
		if (String.IsNullOrEmpty(input))
		{
			Debug.Log("<color=#ff0000>ARGH!</color>");
			return "";
		}

		return input[0].ToString().ToUpper() + input.Substring(1);
	}

	public static string FirstCharToUpperEveryWord(string input)
	{
		if (String.IsNullOrEmpty(input))
		{
			return "";
		}

		string r = "";
		string[] words = input.Split(' ');
		for (int i = 0; i < words.Length; i++)
		{
			r += FirstCharToUpper(words[i]);
			if (i < words.Length - 1)
				r += " ";
		}

		return r;
	}

	public static float LerpWithoutClamp(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	public static Vector3 LerpWithoutClamp(Vector3 A, Vector3 B, float t)
	{
		return A + (B - A) * t;
	}

	//Returns deterministic gaussian random between 0 and 1
	public static float DeterministicRandomNumber(float seed)
	{
		float p = Mathf.PerlinNoise(9.9871f + seed * 1.34432f, 76.31f + seed * 1.1233f);

		return Mathf.Abs((Mathf.RoundToInt(p * 100000000f) % 1000000) / 1000000f);
	}

	public static string Capitalize(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}

		s = s.ToLower();
		char[] a = s.ToCharArray();
		a[0] = char.ToUpper(a[0]);
		return new string(a);
	}

	public static int RoundNumberToNearestMultiple(float number, int multiple)
	{
		return Mathf.CeilToInt(number / (float) multiple) * multiple;
	}

	public static string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);

		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 =
			new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);

		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";

		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}

		return hashString.PadLeft(32, '0');
	}
	

}