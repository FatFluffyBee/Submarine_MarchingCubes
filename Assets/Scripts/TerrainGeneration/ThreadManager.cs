using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

//Thread manager used to create noise data and mesh data in realtime without impacting performance
//mesh and noise data could be regrouped in future if no operations happenned in between in endless terrain
public class ThreadManager : MonoBehaviour
{
    [SerializeField] private int maxCallbackPerFrames;

    Queue<MapThreadInfo<float[,,]>> noiseMapThreadInfoQueue = new Queue<MapThreadInfo<float[,,]>>(); //queue who stocks finish results to send back out
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    //called in endlessterrain
    public void RequestNoiseMap(Action<float[,,]> callback, Vector3 offset, ChunkSettingsSO chunkSO) {
        ThreadStart threadStart = delegate {
            NoiseMapThread(callback, offset, chunkSO);
        };
        new Thread(threadStart).Start();
    }

    void NoiseMapThread(Action<float[,,]> callback, Vector3 offset, ChunkSettingsSO chunkSO){
        float[,,] noiseMap = NoiseGenerator.Generate3DNoiseMap(chunkSO, offset);
        lock (noiseMapThreadInfoQueue) {
            noiseMapThreadInfoQueue.Enqueue(new MapThreadInfo<float[,,]>(callback, noiseMap));
        }
    }

    //called in endlessterrain
    public void RequestMeshData(Action<MeshData> callback, float[,,] noiseMap, ChunkSettingsSO chunkSO){
        ThreadStart threadStart = delegate {
            MeshDataThread(callback, noiseMap, chunkSO);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(Action<MeshData> callback, float[,,] noiseMap, ChunkSettingsSO chunkSO) {
        MeshData meshData = MarchingCubes.GenerateMeshDataFrom3DNoiseMap(noiseMap, chunkSO.groundValue, chunkSO.invert, chunkSO.interpolation);
        lock (noiseMapThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    //we limit the max number of callback per frame to avoid lag spike //todo a little janky cause allow double the amount but not prioritary
    private void Update() {
        if (noiseMapThreadInfoQueue.Count > 0) {
            for (int i = 0; i < (noiseMapThreadInfoQueue.Count > maxCallbackPerFrames ? maxCallbackPerFrames : noiseMapThreadInfoQueue.Count) ; i++) {
                MapThreadInfo<float[,,]> threadInfo = noiseMapThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < (meshDataThreadInfoQueue.Count > maxCallbackPerFrames ? maxCallbackPerFrames : meshDataThreadInfoQueue.Count); i++) {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public struct MapThreadInfo<T> {
        public Action<T> callback;
        public T parameter;

        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
