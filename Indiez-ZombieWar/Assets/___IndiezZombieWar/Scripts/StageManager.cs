using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField]
    public ZombieSpawner[] _spawnAreas;

    private void Start()
    {
        SpawnTest();
    }
    private void SpawnTest()
    {
        _spawnAreas[0].SpawnAmount(1);
    }
}
