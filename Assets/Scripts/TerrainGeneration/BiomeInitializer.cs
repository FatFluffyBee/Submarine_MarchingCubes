using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//Update the render settings of the scene from the csso, from light to urp settings
//todo does it works?
public class BiomeInitializer : MonoBehaviour
{
    [SerializeField] private ChunkSettingsSO cSSO;
    [SerializeField] private Renderer waterRd;
    [SerializeField] private Light sun;
    [SerializeField] private Camera cam;
    [SerializeField] private Volume urpVolume;

    /*[SerializeField] private Gradient skyBoxColor;
    [SerializeField] private ParticleSystem cameraParticleSystem;*/

    private void OnValidate() 
    {
            if(cSSO != null) {
            cSSO.OnUpdateRenderSettings -= UpdateRenderSettings;
            cSSO.OnUpdateRenderSettings += UpdateRenderSettings;
        }
    }

    public void UpdateRenderSettings(){
        RenderSettings.sun = sun;
        sun.color = cSSO.sunColor;
        sun.intensity = cSSO.sunIntensity;
        RenderSettings.subtractiveShadowColor = cSSO.substractiveShadowColor;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientLight = cSSO.topAmbientLightColor;
        RenderSettings.ambientEquatorColor = cSSO.sideAmbientLightColor;
        RenderSettings.ambientGroundColor = cSSO.botAmbientLightColor;
        RenderSettings.ambientIntensity = 1.25f;

        RenderSettings.fog = cSSO.fogActive;
        RenderSettings.fogColor = cSSO.fogColor;
        RenderSettings.fogStartDistance = cSSO.fogRange.x;
        RenderSettings.fogEndDistance = cSSO.fogRange.y;

    }
}
