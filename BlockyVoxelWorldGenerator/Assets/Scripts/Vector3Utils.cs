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
    
    public static Vector3 ToVector3(this Vector3Int input)
    {
        return new Vector3()
        {
            x = input.x,
            y = input.y,
            z = input.z,
        };
    }
}