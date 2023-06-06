using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerAttackSpawner : MonoBehaviour
{

    private GameObject attackObject;
    private IObjectPool<GameObject> attackPool;
    public void ClearPool() { attackPool.Clear(); }

    // Start is called before the first frame update
    void Awake()
    {
        attackObject = Resources.Load("PlayerAttacks/AttackInstance") as GameObject;
        attackPool = new ObjectPool<GameObject>(CreateAttack, OnTakeAttackFromPool, OnReturnAttackToPool);
    }

    public AttackInstance SpawnAttack(PlayerAttackScriptable attackScript, Transform model)
    {
        var obj = attackPool.Get();
        obj.GetComponent<AttackInstance>().Init(attackScript, model);
        return obj.GetComponent<AttackInstance>();
    }
    public AttackInstance SpawnAttack(PlayerSpecialScriptable attackScript, Transform model, float perc)
    {
        var obj = attackPool.Get();
        obj.GetComponent<AttackInstance>().Init(attackScript, model, perc);
        return obj.GetComponent<AttackInstance>();
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
