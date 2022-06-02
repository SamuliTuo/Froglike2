using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AttackInstance : MonoBehaviour
{
    private IObjectPool<GameObject> pool;
    public void SetPool(IObjectPool<GameObject> pool) => this.pool = pool;


    public void Init()
    {
        StartCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        yield return Helpers.GetWait(0.2f);
        if (pool != null)
        {
            pool.Release(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
