using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyHPBars : MonoBehaviour {

    [SerializeField] private GameObject hpBarPrefab = null;

    private IObjectPool<GameObject> barPool;


    public GameObject SpawnHPBar() {
        return barPool.Get();
    }

    public void ResetPools() {
        barPool.Clear();
    }

    void Awake() {
        barPool = new ObjectPool<GameObject>(CreateHPBar, OnTakeBarFromPool, OnReturnBarToPool);
    }
    GameObject CreateHPBar() {
        var instance = Instantiate(hpBarPrefab) as GameObject;
        instance.GetComponent<HPBarInstance>().SetPool(barPool);
        return instance;
    }
    void OnTakeBarFromPool(GameObject loot) {
        loot.SetActive(true);
    }
    void OnReturnBarToPool(GameObject loot) {
        loot.SetActive(false);
    }
}
