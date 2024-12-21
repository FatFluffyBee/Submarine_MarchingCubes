using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainModifier : MonoBehaviour
{
    public Camera mainCamera;
    public float brushHeight, brushRadius;
    public float cooldownModifTerrain = 0.1f;
    float cooldownCount;
    public AnimationCurve attenuationCurve;

    /*void Update()
    {
        if((Input.GetMouseButton(0) ||Input.GetMouseButton(1)) && cooldownCount > cooldownModifTerrain)
        {
            int brushDirection = 1;
            if(Input.GetMouseButton(0)) brushDirection = -1; //swap dir if right click pressed

            Debug.Log("Mouse clicked");

            RaycastHit hit = new RaycastHit();
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit, 100f))
            {
                List<MeshFilter> meshFilters = new List<MeshFilter>();
                List<MeshCollider> meshColliders = new List<MeshCollider>();

                Collider[] collidersHit = Physics.OverlapSphere(hit.point, brushRadius);
                foreach(Collider col in collidersHit)
                {
                    meshFilters.Add(col.GetComponent<MeshFilter>());
                    meshColliders.Add(col.GetComponent<MeshCollider>());
                }
                
                UpdateMeshes(meshFilters, meshColliders, hit.point, ray.direction.normalized, brushRadius, brushHeight * brushDirection, attenuationCurve);
            }

            cooldownCount = 0;
        }
        cooldownCount+= Time.deltaTime;
    }*/
    
    public static void UpdateMeshes(List<MeshFilter> meshFilters, List<MeshCollider> meshColliders, Vector3 positionHit, Vector3 normalHit, float radius, float height, AnimationCurve attenuationCurve)
    {
        for(int i = 0; i < meshFilters.Count; i++)
        {
            MeshFilter meshFilter = meshFilters[i];
            MeshCollider meshCollider = meshColliders[i];
            Vector3 centerInMesh =  meshFilter.transform.InverseTransformPoint(positionHit);
            normalHit = meshFilter.transform.InverseTransformDirection(normalHit);
            float meshScale = meshFilter.transform.localScale.x;

            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = mesh.vertices;
            float heightModif = height / meshScale;
            float radiusModif = radius / meshScale;

            
            for(int j = 0; j < vertices.Length; j++)
            {
                if(Vector3.Distance(vertices[j], centerInMesh) < radius)
                {
                    vertices[j] += heightModif * attenuationCurve.Evaluate(1 - (Vector3.Distance(vertices[j], centerInMesh) / radiusModif)) * normalHit;
                }
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }
}
