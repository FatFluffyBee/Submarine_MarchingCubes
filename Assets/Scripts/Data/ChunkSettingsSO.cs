using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

//This scriptable serve to hold and store the data of differnt biomes presets, from mesh generation data to render settings and bounds
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ChunkSettingSO", order = 1)]
public class ChunkSettingsSO : ScriptableObject
{
    public event System.Action OnUpdateMeshSettings;
    public event System.Action OnUpdateRenderSettings;
    public bool autoUpdate;   

[Header("Chunk General Settings")]
    public Vector3Int chunkSize;
    public float meshScale;
    [Range(0, 1f)]public float groundValue;
    public Material material;
    public bool invert = false;
    public bool interpolation = true;
    public bool clampEdgeNoiseValues = false;

[Header("Main Noise Settings")]
    public NoiseType noiseType = NoiseType.Perlin3D;
    public float noiseScale;

    [Range (0, 1f)]
    public float persistance;
    public float lacunarity;
    public int octaves;
    public int seed;
    public Vector3 posOffset;
    public Vector3 noiseSquichiness = Vector3.one; // multiply coordinate of the sample in one ore more axis to give more flat or high noise (plateau or cliffs for example)

[Header("2D Height Map Cutout Top")]
    public bool addTopCutout = false;
    public float cutoutNoiseScale;
    [Range(0, 1f)] public float cutoutNoisePersistance;
    public float cutoutNoiseLacunarity;
    public int cutoutNoiseOctaves;
    public Vector2 cutoutMapSquichiness;
    public Vector2 cutoutNoiseOffset;
    public int cutoutThresholdLerpLength;
    public Vector2 cutoutThresholdRange;

[Header("Plateau Settings")]
    public bool addPlateau = false;
    public int plateauModulo;
    [Range(0.5f, 2)] public float plateauModifMax;
    public int topPlateauWidth, bottomPlateauWidth;

[Header("Easing Curve Main Noise")]
    public bool useEaseCurve;
    public AnimationCurve noiseEaseCurve;

[Header("Edge Map Lerp Settings")]
    public bool addEdgeMapLerp = false;
    public int lerpRange;
    //each Vector4 correspons to its corresponding axis (x, y, z) For Vector 4, x and y = min and max bounds of axis / z ans w if the edge need to be filed full (1) or empty (0)
    public Vector4 xBound, yBound, zBound;  

[Header("Render Settings")]
    public Gradient skyBoxColor;
    public float ambientLightIntensity;
    public float sunIntensity;
    public Color sunColor;
    public Color substractiveShadowColor;
    public Material waterMaterial;
    public VolumeProfile postProcessVolume; 
    public bool fogActive = true;
    public Vector2 fogRange;
    public Color fogColor;
    public Color cameraBackgroundColor;
    public ParticleSystem cameraParticleSystem;

    protected void OnValidate() {
        if(chunkSize.x < 1) chunkSize.x = 1;
        if(chunkSize.y < 1) chunkSize.y = 1;
        if(chunkSize.z < 1) chunkSize.z = 1;
        if(meshScale < 0.1f) meshScale = 0.1f;

        if(noiseScale < 0.1f) noiseScale = 0.1f;
        if(lacunarity < 1) lacunarity = 1;
        if(octaves < 1) octaves = 1;

        if(noiseSquichiness.x < 0f) noiseSquichiness.x = 0.1f;
        if(noiseSquichiness.y < 0f) noiseSquichiness.y = 0.1f;
        if(noiseSquichiness.z < 0f) noiseSquichiness.z = 0.1f;

        if(plateauModulo < 1) plateauModulo = 1;
        if(topPlateauWidth < 0) topPlateauWidth = 0;
        if(bottomPlateauWidth < 0) bottomPlateauWidth = 0;

        if(cutoutNoiseScale < 0.1f) cutoutNoiseScale = 0.1f;
        if(cutoutNoiseLacunarity < 1) cutoutNoiseLacunarity = 1;
        if(cutoutNoiseOctaves < 1) cutoutNoiseOctaves = 1;

         if(autoUpdate)
            UpdateAll();
    }

    public void UpdateAll(){
        UpdateMeshSettings();
        UpdateRenderSettingsEvent();
    }

    public void UpdateMeshSettings() {
        if(OnUpdateMeshSettings != null) {
            OnUpdateMeshSettings();
        }
    }

    public void UpdateRenderSettingsEvent(){
        if(OnUpdateRenderSettings != null) {
            OnUpdateRenderSettings();
        }    
    }
}
    public enum NoiseType { Perlin3D, Shore_Plateau};
