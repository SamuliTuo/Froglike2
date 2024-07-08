using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticleEffects : MonoBehaviour
{
    public ParticleSystem smokeSpawner;
    private ParticleSystem.MainModule smokeMain;
    public Vector2 continuousSmoke_spawnRateMinMax = Vector2Int.one;
    public Vector2Int continuousSmoke_spawnCountMinMax = Vector2Int.one;

    public ParticleSystem bitsSpawner;

    private void Start()
    {
        //if (smokeSpawner != null)
        //    smokeMain = smokeSpawner.GetComponent<ParticleSystem.MainModule>();
    }

    private void Update()
    {
        if (t > 0)
        {
            t -= Time.deltaTime;
            if (t <= 0)
            {
                canSpawnSmoke = true;
            }
        }
    }

    public float downOffset = 1.0f;
    public void SpawnSmoke(Vector3 pos, Vector3 dir)
    {
        smokeSpawner.transform.position = pos;
        smokeSpawner.transform.rotation = Quaternion.LookRotation(dir);
        //smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(10f, 12f);
        smokeSpawner.Play();
        //smokeSpawner.Emit(1);
    }

    float t = 0;
    bool canSpawnSmoke = true;
    public void SpawnContinuousSmoke(Vector3 pos, Vector3 dir)
    {
        if (canSpawnSmoke)
        {
            bitsSpawner.transform.position = pos;
            if (dir != Vector3.zero)
            {
                bitsSpawner.transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                bitsSpawner.transform.rotation = Quaternion.LookRotation(Vector3.up);
            }
            bitsSpawner.Emit(Random.Range(continuousSmoke_spawnCountMinMax.x, continuousSmoke_spawnCountMinMax.y + 1));

            canSpawnSmoke = false;
            t = Random.Range(continuousSmoke_spawnRateMinMax.x, continuousSmoke_spawnRateMinMax.y);
        }
    }




    public void Reset()
    {
        //damageNumbers.Reset();
    }


    void PlayParticle(ParticleSystem s, Vector3 pos, Vector3 forw)
    {
        s.transform.position = pos;
        s.transform.LookAt(pos + forw, Vector3.up);
        s.Play();
        var source = s.GetComponent<AudioSource>();

        if (source == null)
            return;

        source.pitch = Random.Range(0.80f, 1.20f);
        source.Play();
    }
}
