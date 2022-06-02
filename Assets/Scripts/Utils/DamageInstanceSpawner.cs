using UnityEngine;
using UnityEngine.Pool;

public class DamageInstanceSpawner : MonoBehaviour {

    [SerializeField] private GameObject dmgInstanceObj = null;

    private IObjectPool<GameObject> dmgInstancePool;
    private GameObject clone;


    public void SpawnDamageInstance(
        Vector3 position, Vector3 size, Quaternion rotation,
        float lifetime, float dmgInterval, float dmgPerInterval, float poiseDmgPerInterval) 
    {
        clone = dmgInstancePool.Get();
        clone.transform.position = position;
        clone.transform.localScale = size;
        clone.transform.rotation = rotation;
        clone.GetComponent<DamageInstance>().Init(lifetime, dmgInterval, dmgPerInterval, poiseDmgPerInterval);
        clone = null;
    }

    public void ResetPools() {
        dmgInstancePool.Clear();
    }

    void Awake() {
        dmgInstancePool = new ObjectPool<GameObject>(CreateDmgInstance, OnTakeDmgInstanceFromPool, OnReturnDmgInstanceToPool);
    }
    GameObject CreateDmgInstance() {
        var instance = Instantiate(dmgInstanceObj) as GameObject;
        instance.GetComponent<DamageInstance>().SetPool(dmgInstancePool);
        return instance;
    }
    void OnTakeDmgInstanceFromPool(GameObject loot) {
        loot.SetActive(true);
    }
    void OnReturnDmgInstanceToPool(GameObject loot) {
        loot.SetActive(false);
    }
}
