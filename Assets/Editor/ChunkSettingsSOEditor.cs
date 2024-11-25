using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChunkSettingsSO))] // true is needed so it can be used on classes inheriting from updatable data too
public class ChunkSettingsSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ChunkSettingsSO data = (ChunkSettingsSO)target;
        
        if(GUILayout.Button("Update"))
        {
            data.NotifyOfUpdateValues();
        }

        if(GUILayout.Button("Update Render Settings"))
        {
            data.UpdateRenderSettingsEvent();
        }
    }
}
