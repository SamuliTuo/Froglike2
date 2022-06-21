using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum LootType {
    NULL,
    GEMSTONE_PURPLE,
    GEMSTONE_RED
}

public class LootSpawner : MonoBehaviour {

    private Dictionary<LootType, IObjectPool<GameObject>> lootPools = new Dictionary<LootType, IObjectPool<GameObject>>();
    private GameObject loot_gem_purple;
    private GameObject loot_gem_red;
    private IObjectPool<GameObject> gemPool_purple;
    private IObjectPool<GameObject> gemPool_red;


    void Awake() {
        loot_gem_purple = Resources.Load("LootInstances/LootInstance_Gem_Purple_01") as GameObject;
        loot_gem_red = Resources.Load("LootInstances/LootInstance_Gem_Red_01") as GameObject;
        gemPool_purple = new ObjectPool<GameObject>(CreateGem_Purple, OnTakeLootFromPool, OnReturnLootToPool);
        gemPool_red = new ObjectPool<GameObject>(CreateGem_Red, OnTakeLootFromPool, OnReturnLootToPool);
    }

    void Start() {
        lootPools.Add(LootType.GEMSTONE_PURPLE, gemPool_purple);
        lootPools.Add(LootType.GEMSTONE_RED, gemPool_red);
    }

    public void SpawnLoot(Vector3 pos, LootInstanceSettings loots) {
        foreach (KeyValuePair<LootType, int> item in loots.GetLoots()) {
            lootPools.TryGetValue(item.Key, out IObjectPool<GameObject> pool);
            for (int i = 0; i < item.Value; i++) {
                var obj = pool.Get();
                obj.GetComponent<LootInstance>().Init();
                obj.transform.position = pos;
                obj.GetComponent<Rigidbody>().velocity = RandomVelocity();
            }
        }
    }

    public void ClearPools() {
        gemPool_purple.Clear();
        gemPool_red.Clear();
    }

    Vector3 RandomVelocity() {
        return new Vector3(Random.Range(-4.0f, 4.0f), Random.Range(5.0f, 8.0f), Random.Range(-4.0f, 4.0f));
    }

    GameObject CreateGem_Purple() {
        var instance = Instantiate(loot_gem_purple);
        instance.GetComponent<LootInstance>().SetPool(gemPool_purple);
        return instance;
    }
    GameObject CreateGem_Red() {
        var instance = Instantiate(loot_gem_red);
        instance.GetComponent<LootInstance>().SetPool(gemPool_red);
        return instance;
    }
    void OnTakeLootFromPool(GameObject loot) {
        loot.SetActive(true);
    }
    void OnReturnLootToPool(GameObject loot) {
        loot.SetActive(false);
    }
}
