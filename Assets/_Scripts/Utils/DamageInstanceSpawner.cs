using UnityEngine;
using UnityEngine.Pool;

public class DamageInstanceSpawner : MonoBehaviour {

    [SerializeField] private GameObject dmgInstanceSquare = null;
    [SerializeField] private GameObject dmgInstanceSphere = null;

    private IObjectPool<GameObject> dmgInstancePool;
    private GameObject clone;

    public void SpawnDamageInstance_Sphere(
        Vector3 position, Quaternion rotation, float radius,
        float lifetime, float dmgInterval, float dmgPerInterval, float poiseDmgPerInterval) 
    {
        clone = dmgInstancePool.Get();
        clone.transform.position = position;
        clone.transform.localScale = new(radius, radius, radius);
        clone.transform.rotation = rotation;
        clone.GetComponent<DamageInstance>().Init(lifetime, dmgInterval, dmgPerInterval, poiseDmgPerInterval, radius);
        clone = null;
    }

    public void ResetPools() {
        dmgInstancePool.Clear();
    }

    void Awake() {
        dmgInstancePool = new ObjectPool<GameObject>(CreateDmgInstance, OnTakeDmgInstanceFromPool, OnReturnDmgInstanceToPool);
    }
    GameObject CreateDmgInstance() {
        var instance = Instantiate(dmgInstanceSphere) as GameObject;
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
