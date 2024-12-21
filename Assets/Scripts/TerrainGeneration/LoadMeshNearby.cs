using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadMeshNearby : MonoBehaviour // act like a flag who says tnis go load mesh nearby. Might add useful local var later
{
    float rangeToLoadChunk = 1f;
    private void Start() 
    {
        EndlessTerrain.instance.elementsWhoLoadMeshes.Add(this);
    }

    private void OnDestroy() 
    {
        EndlessTerrain.instance.elementsWhoLoadMeshes.Remove(this);
    }
}
