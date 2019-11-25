using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoredCubeRotations : MonoBehaviour
{
    
}

// Taking a vector on all the functions that rely on this would have been cleaner
// but I prefer to have this in such a way that you can't rotate the cube using
// weird Axii like negative ones to flip direction or having non-normalized
// ones wreaking havoc.
[SerializeField]
public enum CubeRotationAxis {
    XAxis,
    YAxis,
    ZAxis
}

public struct CubeRotation{
    public CubeRotationAxis RotationAxis;
    public int pivotIndex;
    ///<summary>If true, it's either rotating up or left depending on the rotation axis</summary>
    public bool direction;
}