using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LootInstance : MonoBehaviour {

    [SerializeField] private LootType type = LootType.NULL;
    [SerializeField, Min(0.2f)] private float canLootTimer = 0.5f;

    private IObjectPool<GameObject> pool;
    private Transform player;
    private Rigidbody rb;
    private Coroutine coroutine;
    private bool canLoot;
    private float speed;
    
    public void SetPool(IObjectPool<GameObject> pool) => this.pool = pool;

    public void Init() {
        canLoot = false;
        coroutine = null;
        StartCoroutine(SetCanLoot());
    }

    public void LootMe(Transform player) {
        if (coroutine == null) {
            this.player = player;
            speed = 0;
            rb = GetComponent<Rigidbody>();
            coroutine = StartCoroutine(Looted());
        }
    }
    
    IEnumerator SetCanLoot() {
        yield return Helpers.GetWait(Random.Range(canLootTimer - 0.1f, canLootTimer + 0.1f));
        canLoot = true;
    }
    IEnumerator Looted() {
        while (!canLoot) {
            yield return null;
        }

        Vector3 dist = player.position - transform.position;
        rb.useGravity = false;
        rb.isKinematic = true;

        while (dist.sqrMagnitude > 0.5f) {
            if (speed < 8) {
                speed += Time.deltaTime * 0.2f;
            }
            transform.position += dist.normalized * speed;
            dist = player.position - transform.position;
            yield return null;
        }

        rb.useGravity = true;
        rb.isKinematic = false;
        if (pool != null) {
            Singleton.instance.PlayerInventory.AddItem(type);
            pool.Release(this.gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
}
