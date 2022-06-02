using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerAttackSpawner : MonoBehaviour
{

    private GameObject attackObject;
    private IObjectPool<GameObject> attackPool;

    // Start is called before the first frame update
    void Awake()
    {
        attackPool = new ObjectPool<GameObject>(CreateAttack, OnTakeAttackFromPool, OnReturnAttackToPool);
    }

    GameObject CreateAttack()
    {
        var instance = Instantiate(attackObject);
        instance.GetComponent<AttackInstance>().SetPool(attackPool);
        return instance;
    }
    void OnTakeAttackFromPool(GameObject attack)
    {
        attack.SetActive(true);
    }
    void OnReturnAttackToPool(GameObject attack)
    {
        attack.SetActive(false);
    }
}
