using DG.Tweening;
using GPUInstancer.CrowdAnimations;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieInstancingManager : MonoBehaviour
{
    public static ZombieInstancingManager Instance;
    [SerializeField, HideInInspector]
    private ZombieSpawner[] _spanwers;
    [SerializeField, HideInInspector]
    private GPUICrowdManager _crowdManager;
    public int RagdollLimitPerPrefab = -1;
    public Dictionary<string, Transform> _ragdollParents = new Dictionary<string, Transform>();
    public Dictionary<string, GameObject> PrototypeNameDict = new Dictionary<string, GameObject>();
    public Dictionary<string, List<ZombieInstance>> SpawnListDict = new Dictionary<string, List<ZombieInstance>>();
    public Dictionary<string, List<ZombieRagdoll>> RagdollListDict = new Dictionary<string, List<ZombieRagdoll>>();
    private DynamicMeshCombiner[] _combiners;

    private void OnValidate()
    {
        _spanwers = FindObjectsOfType<ZombieSpawner>();
        _crowdManager = FindObjectOfType<GPUICrowdManager>();
    }
    private void Awake()
    {
        Physics.reuseCollisionCallbacks = true;
        if (Instance == null)
        {
            Instance = this;
        }
        Init();
        if (RagdollLimitPerPrefab > 0)
        {
            CreateZombies();
        }
    }

    private void CreateZombies()
    {
        for (int i = 0; i < _crowdManager.prefabList.Count; i++)
        {
            var prototype = _crowdManager.prefabList[i];
            for (int j = 0; j < RagdollLimitPerPrefab; j++)
            {
                var listInstance = SpawnListDict[prototype.name];
                var zomb = Instantiate(prototype).GetComponent<ZombieInstance>();
                zomb.agent.enabled = false;
                zomb.prototypeName = prototype.name;
                zomb.gameObject.SetActive(false);
                listInstance.Add(zomb);

                var listRagdoll = RagdollListDict[prototype.name];
                var rag = Instantiate(prototype.GetComponent<ZombieInstance>().RagdollPrefab).GetComponent<ZombieRagdoll>();
                rag.gameObject.SetActive(false);
                listRagdoll.Add(rag);
            }
        }
    }

    private void Init()
    {
        List<DynamicMeshCombiner> list = new List<DynamicMeshCombiner>();
        for (int i = 0; i < _crowdManager.prefabList.Count; i++)
        {
            var prototype = _crowdManager.prefabList[i];
            GameObject transformParent = new GameObject(prototype.name + " Ragdolls");
            _ragdollParents.Add(prototype.name + "(Clone)", transformParent.transform);
            SpawnListDict.Add(prototype.name, new List<ZombieInstance>());
            RagdollListDict.Add(prototype.name, new List<ZombieRagdoll>());
            PrototypeNameDict.Add(prototype.name, prototype);
            if (!(prototype.GetComponent<ZombieInstance>().RagdollPrefab.GetComponent<ZombieRagdoll>().OnDeath == RagdollDeathBehavior.Dissolve))
            {
                var combiner = transformParent.AddComponent<DynamicMeshCombiner>();
                list.Add(combiner);
            }
        }
        _combiners = list.ToArray();
        DOTween.Sequence()
           .AppendInterval(3f)
           .AppendCallback(AddMeshesToCombine).SetLoops(-1);
    }
    private void AddMeshesToCombine()
    {
        for (int i = 0; i < _combiners.Length; i++)
        {
            _combiners[i].AddMeshes();
        }
    }
    public Transform GetRagdollParent(string PrototypeName)
    {
        if (_ragdollParents.TryGetValue(PrototypeName, out Transform parent))
        {
            return parent;
        }
        return null;
    }
    public ZombieInstance GetAvailableZombie(GameObject prototype)
    {
        var list = SpawnListDict[prototype.name];
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].gameObject.activeInHierarchy)
            {
                return list[i];
            }
        }
        var zomb = Instantiate(prototype).GetComponent<ZombieInstance>();
        zomb.agent.enabled = false;
        zomb.gameObject.SetActive(false);
        list.Add(zomb);
        zomb.prototypeName = prototype.name;
        return zomb;
    }
    public ZombieRagdoll GetAvailableRagdoll(string PrototypeName)
    {
        var list = RagdollListDict[PrototypeName];
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].gameObject.activeInHierarchy)
            {
                list[i].ResetRagdoll();
                return list[i];
            }
        }
        if (RagdollLimitPerPrefab > 0 && list.Count > RagdollLimitPerPrefab)
        {
            print("Ragdoll limit reached, using existing ragdoll");
            var randomRagdoll = list[Random.Range(0, list.Count)];
            randomRagdoll.gameObject.SetActive(false);
            randomRagdoll.ResetRagdoll();
            return randomRagdoll;
        }
        var rag = Instantiate(PrototypeNameDict[PrototypeName].GetComponent<ZombieInstance>().RagdollPrefab).GetComponent<ZombieRagdoll>();
        rag.gameObject.SetActive(false);
        list.Add(rag);
        return rag;
    }
}
