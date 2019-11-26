
/* This program is free software. It comes without any warranty, to
* the extent permitted by applicable law. You can redistribute it
* and/or modify it under the terms of the Do What The Fuck You Want
* To Public License, Version 2, as published by Sam Hocevar. See
* http://www.wtfpl.net/ for more details. */

using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public static class RoundingUtils
{
    public static float RoundToNearest(float valueToSnap, float snapTo)
    {
        return Mathf.Round(valueToSnap / snapTo) * snapTo;
    }

    public static Vector3 RoundToNearest(Vector3 vector3, float snapTo)
    {
        return new Vector3(RoundToNearest(vector3.x, snapTo), RoundToNearest(vector3.y, snapTo), RoundToNearest(vector3.z, snapTo));
    }

    public static void RoundToNearest(ref Vector3 vector3, float snapTo)
    {
        vector3.x = RoundToNearest(vector3.x, snapTo);
        vector3.y = RoundToNearest(vector3.y, snapTo);
        vector3.z = RoundToNearest(vector3.z, snapTo);
    }

    public static void RoundToNearest(ref Vector2 vector2, float snapTo)
    {
        vector2.x = RoundToNearest(vector2.x, snapTo);
        vector2.y = RoundToNearest(vector2.y, snapTo);
    }
}

public static class GUIUtils
{

	public static int ScreenPixelsFromWidthPercentage( float percentage ) {
	    return (int) ( Screen.width * percentage );
	}

	public static int ScreenPixelsFromHeightPercentage( float percentage ) {
	    return (int) ( Screen.height * percentage );
	}

}

public static class IOUtil
{

    public static Texture2D TextureFromFile(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(0, 0);
            tex.LoadImage(fileData); //auto-resizes the texture dimensions.
        }
        else
        {
            Debug.LogWarning("Tried to load non-existant file: " + filePath);
        }
        return tex;
    }

}

public static class MeshExtensions
{
    public static Mesh Copy(this Mesh mesh)
    {
        Mesh original = mesh;
        Mesh copy = new Mesh
        {
            vertices = original.vertices,

            //Has to be done this way, as otherwise the submeshes are not assigned their triangles correctly
            subMeshCount = original.subMeshCount
        };
        for (int i = 0; i < original.subMeshCount; i++)
        {
            copy.SetTriangles(original.GetTriangles(i), i);
        }

        copy.vertices = original.vertices;
	
	//Has to be done this way, as otherwise the submeshes are not assigned their triangles correctly
        copy.subMeshCount = original.subMeshCount;
        for (int i = 0; i < original.subMeshCount; i++) {
            copy.SetTriangles(original.GetTriangles(i),i);
	}
        
        copy.uv = original.uv;
        copy.normals = original.normals;
        copy.colors = original.colors;
        copy.tangents = original.tangents;

        //AssetDatabase.CreateAsset( copy, AssetDatabase.GetAssetPath( original ) + " copy.asset" );
        return copy;
    }
}

public static class TransformExtensions
{
    public static void DestroyChildren(this Transform root)
    {
        int childCount = root.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject.Destroy(root.GetChild(i).gameObject);
        }
    }

    public static void DestroyChildrenImmediate(this Transform root)
    {
        int childCount = root.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject.DestroyImmediate(root.GetChild(i).gameObject);
        }
    }

    public static Transform GetUpmostParent(this Transform obj)
    {
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent;
        }

        return obj;
    }
}

public static class VectorExtensions
{
    public static Vector3 FromVector2(this Vector3 vec, Vector2 vec2, float z = 0)
    {
        vec.Set(vec2.x, vec2.y, z);
        return vec;
    }

    public static void Normalize(ref this Vector3Int vec)
    {
        float magnitude = vec.magnitude;
        vec.Set(
            Mathf.RoundToInt(vec.x / magnitude),
            Mathf.RoundToInt(vec.y / magnitude),
            Mathf.RoundToInt(vec.z / magnitude)
        );
        return;
    }

    public static Vector3 GetLargestAbsDimension(ref this Vector3 vec)
    {
        Vector3 absDelta = new Vector3(
        Mathf.Abs(vec.x),
        Mathf.Abs(vec.y),
        Mathf.Abs(vec.z)
        );

        if (absDelta.x >= absDelta.y && absDelta.x >= absDelta.z)
        {
            return new Vector3(1, 0, 0);
        }
        else if (absDelta.y >= absDelta.x && absDelta.y >= absDelta.z)
        {
            return new Vector3(0, 1, 0);
        }
        else if (absDelta.z >= absDelta.x && absDelta.z >= absDelta.y)
        {
            return new Vector3(0, 0, 1);
        }
        Debug.LogError("No best coordinate");
        return new Vector3(0, 0, 0);
    }
}

public static class RandomUtils
{

    public static T ChooseRandomly<T>(this IList<T> list)
    {
        if (list.Count <= 0)
        {
            return default(T);
        }
        else
        {
            return list[Random.Range(0, list.Count)];
        }
    }
    public static T ChooseRandomly<T>(this T[] array)
    {
        if (array.Length <= 0)
        {
            return default(T);
        }
        else
        {
            return array[Random.Range(0, array.Length)];
        }
    }

    // public static T ChooseRandomly<T>(this IEnumerable<T> list)
    // {
    //     if (list <= 0)
    //     {
    //         return default(T);
    //     }
    //     else
    //     {
    //         return array[Random.Range(0, array.Length)];
    //     }
    // }
}



public static class InterpolationUtils {

    public delegate void InterpolatedMethod(float interpolationFactor);

    public static IEnumerator InterpolateAction(InterpolatedMethod method, float secondsTaken, InterpolatedMethod finalAction = null)
    {
        float start = Time.timeSinceLevelLoad;
        float end = start + secondsTaken;
        float timePercentage = 0;

        for (float currTime = Time.timeSinceLevelLoad; currTime < end; currTime = Time.timeSinceLevelLoad)
        {
            timePercentage = 1 - (end - currTime) / (end - start);
            method(timePercentage);
            yield return null;
        }

        method(1); //Make sure we call the function with a factor of 1 at some point in time

        finalAction?.Invoke(timePercentage);
    }
}

public static class DebugUtils {
    public static void PrintAllSceneObjects()
    {
        Component[] arr = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
        foreach (Component mo in arr)
        {
            Debug.LogWarning(mo.name);
        }
    }
}

static class TimeSpanExtensions
{ 
    public static string ToShortForm(this System.TimeSpan t)
    {
        string shortForm = "";
        if (t.Days > 0)
        {
            shortForm += string.Format("{0}d ", t.Hours.ToString());
        }
        if (t.Hours > 0)
        {
            shortForm += string.Format("{0}:", t.Hours.ToString());
        }
        if (t.Minutes > 0)
        {
            shortForm += string.Format("{0}:", t.Minutes.ToString());
        }
        if (t.Seconds > 0)
        {
            shortForm += string.Format("{0}", t.Seconds.ToString());
        }
        return shortForm;
    } 
} 