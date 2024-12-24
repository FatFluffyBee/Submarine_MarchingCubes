using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//add as a component to object which need to load and keep nearby chunk visible (flare for example)
public class LoadMeshNearby : MonoBehaviour 
{
    //float rangeToLoadChunk = 1f;
    private void Start() {
        EndlessTerrain.instance.AddMeshLoadingElement(this);
    }

    private void OnDestroy() {
        EndlessTerrain.instance.RemoveMeshLoadingElement(this);
    }
}
