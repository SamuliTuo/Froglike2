using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VFXInstance : MonoBehaviour
{
    private IObjectPool<GameObject> pool;
    public void SetPool(IObjectPool<GameObject> pool) => this.pool = pool;


    public void Init()
    {
        StartCoroutine(Lifetime());
    }

    IEnumerator Lifetime()
    {
        yield return Helpers.GetWait(10);
        EndLife();
    }

    void EndLife()
    {
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
