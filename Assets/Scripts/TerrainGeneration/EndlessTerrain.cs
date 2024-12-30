using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handle the creation - load - display of chunks at runtime
public class EndlessTerrain : MonoBehaviour
{
    public static EndlessTerrain instance;
    private void Awake() {
        instance = this;
    }

    private static ThreadManager threadManager;

    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private ChunkSettingsSO chunkSO;

    [Header("Chunk Loading")]
    [SerializeField] private float radiusLoadChunk;
    [SerializeField] private float radiusLoadCollider;
    [SerializeField] private float chunkUpdateFrequency;
    /*[SerializeField] private int rayNumbers;
    [SerializeField] private float rayDistance;
    [SerializeField] private float coneAngle;*/
    float chunkUpdateTimer = 0;
    
    private Dictionary<Vector3, ChunkData> chunkList = new Dictionary<Vector3, ChunkData>();
    private List<Vector3> chunkKeyProcessedLastLoop = new List<Vector3>();
    private List<LoadMeshNearby> elementsWhoLoadMeshes = new List<LoadMeshNearby>();

    private void Start() {
        threadManager = FindObjectOfType<ThreadManager>();
        UpdateChunks();
    }

    private void Update() {
        if(chunkUpdateTimer > Time.time) {
            UpdateChunks();
            chunkUpdateTimer = chunkUpdateFrequency + Time.deltaTime;
        }
    }

    //Main functions which handle the chunks 
    void UpdateChunks() {
        Debug.Log("Working");
        //generate a list of all key chunk checked this round, then its pass to next loop and all pos not checked are hidden (chunk out of rangze)
        List<Vector3> chunkCheckedThisLoop = new List<Vector3>(); 
        List<Vector3> loadCollOfChunk = new List<Vector3>();    

        Vector3 playerPos = player.position;
        Vector3 cameraPos = cameraTransform.position;
        Vector3Int chunkSize = chunkSO.chunkSize;

        //Bound box around the player
        int minX = Mathf.RoundToInt((playerPos.x - radiusLoadChunk) / (chunkSize.x * chunkSO.meshScale));
        int maxX = Mathf.RoundToInt((playerPos.x + radiusLoadChunk) / (chunkSize.x * chunkSO.meshScale));
        int minY = Mathf.RoundToInt((playerPos.y - radiusLoadChunk) / (chunkSize.y * chunkSO.meshScale));
        int maxY = Mathf.RoundToInt((playerPos.y + radiusLoadChunk) / (chunkSize.y * chunkSO.meshScale));
        int minZ = Mathf.RoundToInt((playerPos.z - radiusLoadChunk) / (chunkSize.z * chunkSO.meshScale));
        int maxZ = Mathf.RoundToInt((playerPos.z + radiusLoadChunk) / (chunkSize.z * chunkSO.meshScale));

        //check the chunks around the player by distance 
        for (int x = minX; x <= maxX; x++) 
            for(int  y = minY; y <= maxY; y++)
                for(int z = minZ; z <= maxZ; z++)
                {
                    //skip out of bounds chunks
                    if((x + 1) * chunkSize.x < chunkSO.xBound.x || (x - 1) * chunkSize.x > chunkSO.xBound.y || (y + 1) * chunkSize.y < chunkSO.yBound.x || 
                    (y - 1) * chunkSize.y > chunkSO.yBound.y || (z + 1) * chunkSize.z < chunkSO.zBound.x || (z - 1) * chunkSize.z > chunkSO.zBound.y) {
                        continue;
                    }

                    Vector3 currentKey = new Vector3(x, y, z); //get player pos and get all chunklist key in the range

                    if(Vector3.Distance(GenerateWorldPosFromKey(currentKey), playerPos) < radiusLoadChunk) {
                        chunkCheckedThisLoop.Add(currentKey);
                        if(Vector3.Distance(GenerateWorldPosFromKey(currentKey), playerPos) < radiusLoadCollider) {
                            loadCollOfChunk.Add(currentKey);
                        }
                    } 
                }

        //check all chunk within range of loadmeshnearby components
        for(int i = 0; i < elementsWhoLoadMeshes.Count; i++) {
            Vector3 middlePos = GenerateKeyFromWorldPos(elementsWhoLoadMeshes[i].transform.position); 
            int xMiddle = Mathf.RoundToInt(middlePos.x);
            int yMiddle = Mathf.RoundToInt(middlePos.y);
            int zMiddle = Mathf.RoundToInt(middlePos.z);

            //add chunk in a cube but check for repetition
            for(int x = xMiddle-1; x <= xMiddle+1;  x++) 
                for(int y = yMiddle-1; y <= yMiddle+1; y++) 
                    for(int z = zMiddle-1; z <= zMiddle+1; z++) {           
                        Vector3 currentKey = new Vector3(x, y, z);                    
                        if(!chunkCheckedThisLoop.Contains(currentKey)) {
                            chunkCheckedThisLoop.Add(currentKey);
                        }

                        if(!loadCollOfChunk.Contains(currentKey)) {
                            loadCollOfChunk.Add(currentKey);
                        }
                    }

        }
        //ChunkCheck with raycast in front (to avoid loading mesh not looked at)
        /*List<Vector3> hitlessDirection = GenerateDirectionsInCone(player.forward, rayNumbers, coneAngle); // Get all the rays that hit nothing : player can see a gap where no chunk if it looks that way

        foreach(Vector3 direction in hitlessDirection)
        {
            Debug.DrawRay(player.position, direction * rayDistance, Color.red, 0.5f);
        }

        List<Vector3> keyRaycasted = new List<Vector3>(); //we keep them separated from the other key toi keep them easier to check
        

        foreach(Vector3 direction in hitlessDirection)
        {
            Vector3 normalizedDir = direction.normalized;
            for(int i = (int)radiusLoadChunk; i < rayDistance; i+= chunkSettings.chunkSize / 8) //On v�rifie le long du ray chaque chunk positionn� pour voir si il est affich� / existe
            {
                Vector3 keyToCheck = GenerateKeyFromWorldPos(cameraPos + normalizedDir * i);

                if(!keyRaycasted.Contains(keyToCheck)) 
                { 
                    keyRaycasted.Add(keyToCheck);
                }
            }
        }

        foreach(Vector3 key in keyRaycasted)
        {
            if(!chunkCheckedThisLoop.Contains(key))
            {
                chunkCheckedThisLoop.Add(key);
            }
        }*/

        // on check tous les chunks de la liste
        foreach (Vector3 key in chunkCheckedThisLoop) {
            if (chunkList.ContainsKey(key)) { //chunk already generated
                chunkList[key].ActivateChunk();

                if(loadCollOfChunk.Contains(key)) {
                    chunkList[key].ToggleCollider(true); 
                } else {
                    chunkList[key].ToggleCollider(false);
                }
            } else { //chunk not generated
                chunkList.Add(key, new ChunkData(GenerateWorldPosFromKey(key), this, chunkSO.material));
            }

            //remove the currently updated chunks
            if (chunkKeyProcessedLastLoop.Contains(key)) { //chunk still in range
                chunkKeyProcessedLastLoop.Remove(key);
            }
        }

        //all chunk not check this update are deactivated 
        if(chunkKeyProcessedLastLoop.Count > 0)
            foreach(Vector3 key in chunkKeyProcessedLastLoop) {
                chunkList[key].DeactivateChunk();
            }

        chunkKeyProcessedLastLoop = chunkCheckedThisLoop;
    }

    /*List<Vector3> CastFibonacciSphereRays(Vector3 origin, int numberOfRays, float radius, bool debug)
    {
        List<Vector3> hitlessDirection = new List<Vector3>();

        float phi = (1 + Mathf.Sqrt(5)) / 2; // Golden ratio

        for (int i = 0; i < numberOfRays; i++)
        {
            float theta = 2 * Mathf.PI * i / phi;
            float z = 1 - (2.0f * i + 1) / numberOfRays;
            float radiusAtZ = Mathf.Sqrt(1 - z * z);

            Vector3 direction = new Vector3(radiusAtZ * Mathf.Cos(theta), radiusAtZ * Mathf.Sin(theta),z);

            RaycastHit hit;

            if (Physics.Raycast(origin, direction, out hit, radius))
            {
                if(debug) 
                    Debug.DrawRay(origin, direction * hit.distance, Color.red, chunkUpdateFrequency);
            }
            else
            {
                hitlessDirection.Add(direction);
                if(debug)
                    Debug.DrawRay(origin, direction * radius, Color.green, chunkUpdateFrequency);
            }
        }
        return hitlessDirection;
    }

    List<Vector3> GenerateDirectionsInCone(Vector3 direction, int numberOfPoints, float coneAngle)
    {
       List<Vector3> directions = new List<Vector3>();
        float phi = (1 + Mathf.Sqrt(5)) / 2; // Golden ratio

        for (int i = 0; i < numberOfPoints; i++)
        {
            float cosTheta = 1 - (float)i / (numberOfPoints - 1) * (1 - Mathf.Cos(coneAngle * Mathf.Deg2Rad));
            float sinTheta = Mathf.Sqrt(1 - cosTheta * cosTheta);
            float phiAngle = 2 * Mathf.PI * i / phi;

            float x = sinTheta * Mathf.Cos(phiAngle);
            float y = sinTheta * Mathf.Sin(phiAngle);
            float z = cosTheta;

            Vector3 point = new Vector3(x, y, z);

            // Rotate the point to align with the specified direction
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction);
            directions.Add(rotation * point);
        }

        return directions;
    }*/


    Vector3 GenerateKeyFromWorldPos(Vector3 worldPos) {
        return new Vector3(Mathf.FloorToInt(worldPos.x / (chunkSO.chunkSize.x * chunkSO.meshScale)), Mathf.FloorToInt(worldPos.y / (chunkSO.chunkSize.y * chunkSO.meshScale))
        , Mathf.FloorToInt(worldPos.z / (chunkSO.chunkSize.z * chunkSO.meshScale)));
    }

    Vector3 GenerateWorldPosFromKey(Vector3 key) {
        return new Vector3(key.x * chunkSO.chunkSize.x, key.y * chunkSO.chunkSize.y, key.z * chunkSO.chunkSize.z) * chunkSO.meshScale;
    }

    public void AddMeshLoadingElement(LoadMeshNearby loadMesh) {
        elementsWhoLoadMeshes.Add(loadMesh);
    }

    public void RemoveMeshLoadingElement(LoadMeshNearby loadMesh) {
        if(elementsWhoLoadMeshes.Contains(loadMesh)) {
            elementsWhoLoadMeshes.Remove(loadMesh);
        }
    }

    public class ChunkData
    {
        GameObject chunkObject;
        EndlessTerrain endlessTerrain;
        public float[,,] noiseMap;
        public MeshData meshData;
        public Mesh[] meshes = new Mesh[1];
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        public Collider collider;
        Material material;
        /*public bool asRequestedNoiseMap = false;
        public bool asRequestedMeshData = false;*/

        public ChunkData(Vector3 worldPos, EndlessTerrain endlessTerrain, Material material) {
            this.endlessTerrain = endlessTerrain;
            chunkObject = new GameObject();
            chunkObject.isStatic = true;
            chunkObject.layer = 6;
            chunkObject.transform.position = worldPos;
            meshFilter = chunkObject.AddComponent<MeshFilter>();
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();
            this.material = material;

            threadManager.RequestNoiseMap(OnMapDataReceived, worldPos, endlessTerrain.chunkSO);
        }

        public void ActivateChunk() {
            if (meshData != null) {
                chunkObject.SetActive(true);
            }
        }

        public void DeactivateChunk() {
            chunkObject.SetActive(false);
            if(collider != null) {
                collider.enabled = false;
            }    
        }

        public void ToggleCollider(bool toggle) {
            if(collider != null) 
                collider.enabled = toggle;
        }

        public bool IsChunkVisible() {
            return chunkObject.activeSelf;
        }

        //On Map data received, ask for mesh data
        void OnMapDataReceived(float[,,] noiseMap) {
            this.noiseMap = noiseMap;
            threadManager.RequestMeshData(OnMeshDataReceived, noiseMap, endlessTerrain.chunkSO);
        }

        //On MeshData received, create mesh
        void OnMeshDataReceived(MeshData meshData) {
            this.meshData = meshData;

            Mesh mesh = new Mesh();
            mesh.vertices = meshData.vertices.ToArray();
            mesh.triangles = meshData.triangles.ToArray();
            mesh.RecalculateNormals();
            meshes[0] = mesh;

            meshFilter.mesh = meshes[0];
            meshRenderer.material = material;
            collider = chunkObject.AddComponent<MeshCollider>();
            collider.enabled = false;
            chunkObject.transform.localScale = Vector3.one * endlessTerrain.chunkSO.meshScale;
        }
    }

}


