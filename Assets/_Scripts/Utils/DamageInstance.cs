using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DamageInstance : MonoBehaviour {

    public void SetPool(IObjectPool<GameObject> pool) => this.pool = pool;

    private IObjectPool<GameObject> pool;
    private Collider[] hitCols;
    private List<Transform> affected; 
    private float lifetime;
    private float dmgInterval;
    private float dmgPerInterval;
    private float poiseDmgPerInterval;
    private float t, tSinceLastDmgInterval, radius;


    void Awake() {
        affected = new List<Transform>();
    }

    public void Init(float lifetime, float dmgInterval, float dmgPerInterval, float poiseDmgPerInterval, float radius) {
        this.lifetime = lifetime;
        this.dmgInterval = dmgInterval;
        this.dmgPerInterval = dmgPerInterval;
        this.poiseDmgPerInterval = poiseDmgPerInterval;
        this.radius = radius;
        t = 0;
        StartCoroutine(UpdateDamageInstance());
    }

    IEnumerator UpdateDamageInstance() {
        while (t < lifetime) {
            if (tSinceLastDmgInterval < dmgInterval) {
                tSinceLastDmgInterval += Time.deltaTime;
            }
            else {
                affected.Clear();
                hitCols = Physics.OverlapSphere(transform.position, radius);
                for (int i = 0; i < hitCols.Length; i++) {
                    if (hitCols[i].CompareTag("Enemy")) {
                        if (!affected.Contains(hitCols[i].transform.root)) {
                            hitCols[i].transform.root.GetComponent<Enemy_LeekHurt>().TakeDmg(
                                hitCols[i].transform.position, null, dmgPerInterval, poiseDmgPerInterval, 0.1f);
                            affected.Add(hitCols[i].transform.root);
                        }
                    }
                }
                tSinceLastDmgInterval = 0;
            }
            t += Time.deltaTime;
            yield return null;
        }
        Deactivate();
    }

    void Deactivate() {
        if (pool != null) {
            pool.Release(this.gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
}
