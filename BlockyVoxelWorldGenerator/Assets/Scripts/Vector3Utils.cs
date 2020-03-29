using UnityEngine;

public static class Vector3Utils
{
    public static Vector3Int ToVector3Int(this Vector3 input)
    {
        return new Vector3Int()
        {
            x = (int) input.x,
            y = (int) input.y,
            z = (int) input.z,
        };
    }
}