using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AttackInstance : MonoBehaviour
{
    private IObjectPool<GameObject> pool;
    public void SetPool(IObjectPool<GameObject> pool) => this.pool = pool;
    private List<GameObject> objectsHit = new List<GameObject>();
    private Transform slashObj;
    private AttackHitEffects hitEffects = null;
    private float t;

    void Awake()
    {
        slashObj = transform.GetChild(0);
        if (hitEffects == null)
        {
            hitEffects = Singleton.instance.Player.GetComponent<AttackHitEffects>();
        }
    }

    public void Init(Vector3 offsetFromPlr, Quaternion rot, float width, float length, float growSpeed, float flySpeed, float slashLifeTime, float spawnDelay, float damage)
    {
        objectsHit.Clear();
        StartCoroutine(LifeTime(slashLifeTime));
        StartCoroutine(SlashCoroutine(offsetFromPlr, rot, width, length, growSpeed, flySpeed, slashLifeTime, spawnDelay));
    }

    public bool ObjectHit(GameObject obj)
    {
        if (objectsHit.Contains(obj))
        {
            return false;
        }
        objectsHit.Add(obj);
        return true;
    }

    IEnumerator LifeTime(float slashLifeTime)
    {
        yield return Helpers.GetWait(1 + slashLifeTime);
        slashObj.gameObject.SetActive(false);
        if (pool != null)
        {
            pool.Release(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator SlashCoroutine(Vector3 offsetFromPlr, Quaternion rot, float width, float length, float growSpeed, float flySpeed, float slashLifeTime, float spawnDelay)
    {
        t = 0;
        yield return Helpers.GetWait(spawnDelay);
        transform.position = Singleton.instance.Player.GetChild(0).TransformPoint(offsetFromPlr);
        slashObj.transform.localScale = new Vector3(1, width, length);
        slashObj.transform.rotation = rot;
        slashObj.gameObject.SetActive(true);
        while (t < slashLifeTime)
        {
            transform.position += slashObj.transform.forward * flySpeed * 0.5f * Time.deltaTime;
            slashObj.localScale = new Vector3(
                slashObj.localScale.x + Time.deltaTime * growSpeed,
                slashObj.localScale.y + Time.deltaTime * growSpeed,
                slashObj.localScale.z + Time.deltaTime * growSpeed);
            t += Time.deltaTime;
            yield return null;
        }
        slashObj.gameObject.SetActive(false);
    }

    void Hit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (ObjectHit(other.transform.root.gameObject))
            {
                hitEffects.EnemyHit(other);
            }
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        Hit(other);
    }
    private void OnTriggerStay(Collider other)
    {
        Hit(other);
    }
}
