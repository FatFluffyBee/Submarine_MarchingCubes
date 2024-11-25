using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BiomeInitializer : MonoBehaviour
{
    public ChunkSettingsSO cSSO;
    public Renderer waterRd;
    public Light light;
    public Camera camera;
    public Volume urpVolume;

    /*public Gradient skyBoxColor;


    public ParticleSystem cameraParticleSystem;*/

    private void OnValidate() 
    {
        if(cSSO != null)
        {
            cSSO.UpdateRenderSettings -= InitializeRenderSettings;
            cSSO.UpdateRenderSettings += InitializeRenderSettings;
        }
    }

    public void InitializeRenderSettings()
    {
        waterRd.material = cSSO.waterMaterial;

        RenderSettings.ambientSkyColor = cSSO.skyBoxColor.Evaluate(0);
        RenderSettings.ambientEquatorColor = cSSO.skyBoxColor.Evaluate(.5f);
        RenderSettings.ambientGroundColor = cSSO.skyBoxColor.Evaluate(1);
        RenderSettings.ambientIntensity = cSSO.ambientLightIntensity;
        RenderSettings.sun = light;
        RenderSettings.subtractiveShadowColor = cSSO.substractiveShadowColor;

        RenderSettings.fog = cSSO.fogActive;
        RenderSettings.fogColor = cSSO.fogColor;
        camera.backgroundColor = cSSO.cameraBackgroundColor;
        light.color = cSSO.sunColor;
        light.intensity = cSSO.sunIntensity;
        RenderSettings.fogStartDistance = cSSO.fogRange.x;
        RenderSettings.fogEndDistance = cSSO.fogRange.y;

    }
}
