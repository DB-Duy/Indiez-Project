using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class DynamicMeshCombiner : MonoBehaviour
{
    private List<CombineInstance> currentCombineInstances = new List<CombineInstance>();
    private List<GameObject> combinedMeshObjects = new List<GameObject>();
    private int currentVertexCount = 0;
    private const int maxVertexCount = 65535;
    [HideInInspector]
    public Material ZombMaterial;

    void Start()
    {
        CreateNewCombinedMeshObject();
    }

    private void CreateNewCombinedMeshObject()
    {
        GameObject combinedMeshObject = new GameObject("CombinedMesh_" + combinedMeshObjects.Count);
        combinedMeshObject.transform.position = transform.position;
        combinedMeshObject.transform.rotation = transform.rotation;
        combinedMeshObject.transform.localScale = transform.localScale;

        combinedMeshObject.AddComponent<MeshFilter>();
        var rend = combinedMeshObject.AddComponent<MeshRenderer>();
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        combinedMeshObjects.Add(combinedMeshObject);
        currentVertexCount = 0;
    }

    public void AddMeshes()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var ragdoll = child.GetComponent<ZombieRagdoll>();
            if (ragdoll == null || !ragdoll.gameObject.activeInHierarchy || !ragdoll.CanBake()) { continue; }
            var skinnedMeshRenderer = ragdoll.SkinnedMesh;
            if (ZombMaterial == null)
            {
                ZombMaterial = new Material(skinnedMeshRenderer.material);
                ZombMaterial.SetFloat("_EmissionIntensity", 0);
            }

            Mesh mesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(mesh);

            if (currentVertexCount + mesh.vertexCount > maxVertexCount)
            {
                CombineCurrentMeshes();
                CreateNewCombinedMeshObject();
            }

            CombineInstance combineInstance = new CombineInstance();
            combineInstance.mesh = mesh;
            combineInstance.transform = skinnedMeshRenderer.transform.localToWorldMatrix;
            currentCombineInstances.Add(combineInstance);
            currentVertexCount += mesh.vertexCount;

            ragdoll.gameObject.SetActive(false);
            Destroy(mesh);
        }

        CombineCurrentMeshes();
    }

    private void CombineCurrentMeshes()
    {
        if (currentCombineInstances.Count == 0)
        {
            return;
        }

        GameObject currentCombinedMeshObject = combinedMeshObjects[combinedMeshObjects.Count - 1];
        MeshFilter combinedMeshFilter = currentCombinedMeshObject.GetComponent<MeshFilter>();
        MeshRenderer combinedMeshRenderer = currentCombinedMeshObject.GetComponent<MeshRenderer>();
        if (combinedMeshFilter.mesh != null)
        {
            CombineInstance original = new CombineInstance() { mesh = combinedMeshFilter.sharedMesh, transform = currentCombinedMeshObject.transform.localToWorldMatrix };
            currentCombineInstances.Add(original);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(currentCombineInstances.ToArray(), true, true);

        combinedMeshFilter.mesh = combinedMesh;


        combinedMeshRenderer.material = ZombMaterial;

        currentVertexCount = combinedMesh.vertexCount;
        currentCombineInstances.Clear();
    }
}
