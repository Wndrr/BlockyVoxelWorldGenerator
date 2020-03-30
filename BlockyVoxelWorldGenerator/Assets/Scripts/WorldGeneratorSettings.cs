using System;
using UnityEngine;

public class WorldGeneratorSettings : MonoBehaviour
{
    public static WorldGeneratorSettings Instance;
    public static Material DefaultMaterial;

    private void Start()
    {
        if (Instance == null)
            Instance = this;

        DefaultMaterial = new Material(Shader.Find("Diffuse"));
    }

    public int blocksPerMeter = 1;
    public int generationRadiusInChunks = 2;
    public int voxelsPerChunkSide = 16;
    public int maxHeightInChunks = 4;
}