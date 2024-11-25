using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainModifierWeapon : WeaponSystem
{
    public Transform launchPoint;
    public float brushHeight, brushRadius;
    public float cooldownModifTerrain = 0.1f;
    float cooldownCount;
    public AnimationCurve attenuationCurve;
    int brushDirection = 1;

    public override void FireOne()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(launchPoint.position, launchPoint.forward);

        if(Physics.Raycast(ray, out hit, 100f))
        {
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            List<MeshCollider> meshColliders = new List<MeshCollider>();

            Collider[] collidersHit = Physics.OverlapSphere(hit.point, brushRadius, 1<<6);
            foreach(Collider col in collidersHit)
            {
                meshFilters.Add(col.GetComponent<MeshFilter>());
                meshColliders.Add(col.GetComponent<MeshCollider>());
            }
            
            TerrainModifier.UpdateMeshes(meshFilters, meshColliders, hit.point, ray.direction.normalized, brushRadius, brushHeight * brushDirection, attenuationCurve);
        }

        cooldownCount = Time.time + cooldownModifTerrain;
        brushDirection = 1;
    }

    public override void FireTwo()
    {
        brushDirection = -1;
    }

    public override void ReleaseOne()
    {

    }

    public override void ReleaseTwo()
    {

    }
}
