using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Display the mesh or noise in the editor for testing purposes without play
public class DisplayMesh : MonoBehaviour
{
    public static DisplayMesh instance;
    private void Awake() {
        instance = this;
    }

    [SerializeField] private ChunkSettingsSO chunkSO;

    [SerializeField] private Transform plane;
    private Renderer planeRd;
    
    [SerializeField] private Transform meshHolder;
    private List<Renderer> meshesTestRd = new List<Renderer>();

    public enum DisplayMode { None, Noise2D, Noise3D, Mesh}
    [SerializeField] private DisplayMode displayType = DisplayMode.Noise2D;
    [SerializeField] private int yAxesTest;
    [SerializeField] private Vector3Int numberOfChunks;
    
    private void Start() {
        displayType = DisplayMode.None;
        DrawInEditor();
    }

    private void OnValidate() {
        if(numberOfChunks.x < 1) numberOfChunks.x = 1;
        if(numberOfChunks.y < 1) numberOfChunks.y = 1;
        if(numberOfChunks.z < 1) numberOfChunks.z = 1;

        if (chunkSO != null) {
            chunkSO.OnUpdateMeshSettings -= OnValuesUpdated; 
            chunkSO.OnUpdateMeshSettings += OnValuesUpdated;
        }
        DrawInEditor();
    }

    private void OnValuesUpdated() {
        if (!Application.isPlaying) {
            DrawInEditor();
        }
    }

    private void DrawInEditor() {
        for(int i = meshesTestRd.Count-1; i >= 0; i--) {
            if(meshesTestRd[i]== null)
                 meshesTestRd.RemoveAt(i);
        }

        if(planeRd == null)
            planeRd = plane.GetComponent<Renderer>();


        if(displayType == DisplayMode.None)
        {
            planeRd.enabled = false;
            foreach(Renderer rd in meshesTestRd)
                rd.enabled = false;
        }

        if(displayType == DisplayMode.Noise2D)
        {
            planeRd.enabled = true;
            foreach(Renderer rd in meshesTestRd)
                rd.enabled = false;

            float[,] noiseMap2D = NoiseGenerator.GenerateNoiseMap2D(new Vector2Int(chunkSO.chunkSize.x+1, chunkSO.chunkSize.z+1), chunkSO.noiseScale, chunkSO.persistance, chunkSO.lacunarity, 
            chunkSO.octaves, chunkSO.seed, chunkSO.posOffset, Vector2.one);

            Color[] colorMap = GenerateColorMap(noiseMap2D);
            Texture2D texture = new Texture2D(chunkSO.chunkSize.x +1, chunkSO.chunkSize.z +1);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colorMap);
            texture.Apply();
            planeRd.sharedMaterial.mainTexture = texture;
        }

        if (displayType == DisplayMode.Noise3D)
        {
            planeRd.enabled = true;
           foreach(Renderer rd in meshesTestRd)
                rd.enabled = false;

            float[,] noiseMap2D = new float[chunkSO.chunkSize.x+1, chunkSO.chunkSize.z+1];
            float[,,] noiseMap3D = NoiseGenerator.Generate3DNoiseMap(chunkSO, planeRd.transform.position);

            for(int x = 0; x < noiseMap3D.GetLength(0);  x++) 
                for(int z = 0; z < noiseMap3D.GetLength(2); z++)
                {
                    noiseMap2D[x, z] = noiseMap3D[x, yAxesTest, z];
                }

            Color[] colorMap = GenerateColorMap(noiseMap2D);
            Texture2D texture = new Texture2D(noiseMap2D.GetLength(0), noiseMap2D.GetLength(1));
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colorMap);
            texture.Apply();
            planeRd.sharedMaterial.mainTexture = texture;
        }

        if (displayType == DisplayMode.Mesh)
        {
            planeRd.enabled = false;

            int goCount = 0;
            int goCountMax = numberOfChunks.x * numberOfChunks.y * numberOfChunks.z;

            for(int x = 0; x < numberOfChunks.x; x++)
                for(int y = 0; y < numberOfChunks.y; y++)
                    for(int z = 0; z < numberOfChunks.z; z++)
                    {
                        Vector3 pos = new Vector3(x * chunkSO.chunkSize.x, y * chunkSO.chunkSize.y, z * chunkSO.chunkSize.z) * chunkSO.meshScale;

                        float[,,] noiseMap3D = NoiseGenerator.Generate3DNoiseMap(chunkSO, pos);

                        // We clamp the chunk taking into account which ones are on the side and exclude those who dont share side borders
                        if(chunkSO.clampEdgeNoiseValues) 
                        {
                            Vector3 minClampSide = Vector3.zero;
                            Vector3 maxClampSide = Vector3.zero;
                            Vector3Int key = new Vector3Int(x, y, z);

                            if(x == 0) minClampSide.x = 1; 
                            if(x == numberOfChunks.x - 1) maxClampSide.x = 1;
                            if(y == 0) minClampSide.y = 1; 
                            if(y == numberOfChunks.y - 1) maxClampSide.y = 1;
                            if(z == 0) minClampSide.z = 1; 
                            if(z == numberOfChunks.z - 1) maxClampSide.z = 1;
                            
                            if(minClampSide != Vector3.zero || maxClampSide != Vector3.zero)
                                ClampEdgeNoiseMapValues(ref noiseMap3D, minClampSide, maxClampSide, chunkSO.invert);
                        }

                        MeshData meshData = MarchingCubes.GenerateMeshDataFrom3DNoiseMap(noiseMap3D, chunkSO.groundValue, chunkSO.invert, chunkSO.interpolation);

                        Mesh mesh = new Mesh();
                        mesh.vertices = meshData.vertices.ToArray();
                        mesh.triangles = meshData.triangles.ToArray();
                        mesh.RecalculateNormals();

                        //Generate new gameobject or reuse existing ones. It SHOULD reduce the time between simulations
                        if(goCount < goCountMax)
                            if(meshesTestRd.Count > goCount)
                            {
                                meshesTestRd[goCount].enabled = true;
                                meshesTestRd[goCount].transform.position = pos;
                                meshesTestRd[goCount].GetComponent<MeshFilter>().sharedMesh = mesh;
                                meshesTestRd[goCount].sharedMaterial = chunkSO.material;
                                meshesTestRd[goCount].transform.localScale = Vector3.one * chunkSO.meshScale;
                            }
                            else
                            {
                                GameObject chunk = new GameObject("Chunk Display " + goCount);
                                chunk.transform.parent = meshHolder;
                                chunk.transform.position = pos;
                                meshesTestRd.Add(chunk.AddComponent<MeshRenderer>());
                                meshesTestRd[goCount].sharedMaterial = chunkSO.material;
                                chunk.AddComponent<MeshFilter>().sharedMesh = mesh;
                                chunk.transform.localScale = Vector3.one * chunkSO.meshScale;
                            }

                        goCount++;
                    }

            if(goCount < meshesTestRd.Count)
                {
                    for(int i = goCount; i < meshesTestRd.Count; i++)
                    {
                        meshesTestRd[i].enabled = false;
                    }
                }
        }
    }

    public static Color[] GenerateColorMap(float[,] noiseMap)
    {
        Color[] colorMap = new Color[noiseMap.GetLength(0) * noiseMap.GetLength(0)];

        for (int i = 0; i < noiseMap.GetLength(0); i++)
        {
            for (int j = 0; j < noiseMap.GetLength(0); j++)
            {
                colorMap[i * noiseMap.GetLength(0) + j] = Color.Lerp(Color.white, Color.black, noiseMap[i, j]);
            }
        }

        return colorMap;
    }

    public void ClampEdgeNoiseMapValues(ref float[,,] noiseMap3D, Vector3 minClamp, Vector3 maxClamp, bool invert)
    {
        float value = invert? 1 : 0;

        if(minClamp.x > 0)
        {
            for(int y = 0; y < noiseMap3D.GetLength(1); y++)
                for(int z = 0; z < noiseMap3D.GetLength(2); z++)
                    noiseMap3D[0, y, z] = value;
        }

         if(maxClamp.x > 0)
        {
            for(int y = 0; y < noiseMap3D.GetLength(1); y++)
                for(int z = 0; z < noiseMap3D.GetLength(2); z++)
                    noiseMap3D[chunkSO.chunkSize.x, y, z] = value;
        }

         if(minClamp.y > 0)
        {
            for(int x = 0; x < noiseMap3D.GetLength(0); x++)
                for(int z = 0; z < noiseMap3D.GetLength(2); z++)
                    noiseMap3D[x, 0, z] = value;
        }

         if(maxClamp.y > 0)
        {
            for(int x = 0; x < noiseMap3D.GetLength(0); x++)
                for(int z = 0; z < noiseMap3D.GetLength(2); z++)
                    noiseMap3D[x, chunkSO.chunkSize.y, z] = value;
        }

         if(minClamp.z > 0)
        {
            for(int x = 0; x < noiseMap3D.GetLength(0); x++)
                for(int y = 0; y < noiseMap3D.GetLength(1); y++)
                    noiseMap3D[x, y, 0] = value;
        }

         if(maxClamp.z > 0)
        {
            for(int x = 0; x < noiseMap3D.GetLength(0); x++)
                for(int y = 0; y < noiseMap3D.GetLength(1); y++)
                    noiseMap3D[x, y, chunkSO.chunkSize.z] = value;
        }                  
    }
}
