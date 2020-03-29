using System;
using UnityEngine;

public class WorldGeneratorSettings : MonoBehaviour
{
    public static WorldGeneratorSettings Instance;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    public int blocksPerMeter = 1;
    public int generationRadiusInChunks = 2;
    public int voxelsPerChunkSide = 16;
    public bool IsDebug = false;
    public int maxHeightInChunks = 4;
    public float smooth = .5f;
    public int octaves = 2;
    public float persistence = .5f;
}