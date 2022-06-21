using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Pool;

public enum VFXType {
    NULL,
    WATER_SPLASH,
    LEEK_IMPACT,
    DAMAGE_NUMBERS
};

public enum ParticleType {
    NULL,
};

public class VFXManager : MonoBehaviour {

    [SerializeField] private Vector2 sizeMinMax = new(0.1f, 1);
    [SerializeField] private float numberSpacing = 0;

    private GameObject vfx_instance;
    private IObjectPool<GameObject> vfxPool;
    private VisualEffectAsset waterSplash;
    //private VisualEffectAsset leekImpact;
    private VisualEffectAsset damageNumbers;
    private DamageNumberSprites symbols;
    private Dictionary<VFXType, VisualEffectAsset> visualEffects = new Dictionary<VFXType, VisualEffectAsset>();
    private Dictionary<ParticleType, ParticleSystem> particles = new Dictionary<ParticleType, ParticleSystem>();
    private bool lastCharWasDot;
    private Transform player;


    void Awake()
    {
        vfx_instance = Resources.Load("VFX/VFX_pooledObject") as GameObject;
        waterSplash = Resources.Load("VFX/waterRings_VFXg_01") as VisualEffectAsset;
        damageNumbers = Resources.Load("VFX/damageEffects_VFX_01") as VisualEffectAsset;
        symbols = GetComponent<DamageNumberSprites>();
        vfxPool = new ObjectPool<GameObject>(CreateVFX, OnTakeVFXFromPool, OnReturnVFXFromPool);
    }

    void Start() {
        ListEffects();
        player = Singleton.instance.Player;
    }

    public void SpawnVFX(VFXType type, Vector3 pos) {
        if (type != VFXType.NULL) {
            VisualEffectAsset system;
            visualEffects.TryGetValue(type, out system);
            var clone = vfxPool.Get();
            var vfx = clone.GetComponent<VisualEffect>();
            clone.transform.position = pos;
            clone.SetActive(true);
            vfx.visualEffectAsset = system;
            clone.GetComponent<VFXInstance>().Init();
            vfx.SendEvent("OnTrigger");
        }
    }

    public void SpawnParticles(ParticleType type, Vector3 pos) {
        if (type != ParticleType.NULL) {
            ParticleSystem system;
            particles.TryGetValue(type, out system);
            system.transform.position = pos;
            system.Play();
        }
    }

    void ListEffects() {
        visualEffects.Add(VFXType.WATER_SPLASH, waterSplash);
        //visualEffects.Add(VFXType.LEEK_IMPACT, leekImpact);
        visualEffects.Add(VFXType.DAMAGE_NUMBERS, damageNumbers);
    }

    Vector2 currCharSize;
    public void SpawnDamageNumbers(
        Vector3 attackStartPos, Vector3 pos, Vector3 velo, float damage, NumberType attackType)
    {
        lastCharWasDot = false;
        float perc = Mathf.InverseLerp(0, 150, damage);
        float roundedDamage = Mathf.Round(damage * 10) * 0.1f;
        char[] charArray = roundedDamage.ToString().ToCharArray();
        Vector3 offsetDir = Helpers.Cam.transform.right;
        Vector3 dir = (pos - attackStartPos).normalized;
        for (int i = 0; i < charArray.Length; i++)
        {
            var obj = vfxPool.Get();
            var vfx = obj.GetComponent<VisualEffect>();
            vfx.visualEffectAsset = damageNumbers;
            float multiplier = (i - (charArray.Length * 0.5f)) * numberSpacing;
            Vector3 spawnPos = pos + (offsetDir * multiplier);
            obj.transform.position = spawnPos;
            vfx.SetVector3("SpawnPosition", spawnPos);
            vfx.SetVector3("SpawnVelocity", 
                velo * Mathf.Lerp(1.1f, 1.4f, perc) + 
                dir * Mathf.Lerp(5, 7, perc) * Mathf.Lerp(2, 1, Vector3.Distance(pos, attackStartPos) * 0.1f));
            vfx.SetTexture("symbol", symbols.GetTextureFromChar(charArray[i], attackType));
            vfx.SetBool("isCrit", attackType == NumberType.crit ? true : false);
            currCharSize = lastCharWasDot ? sizeMinMax * 0.65f : sizeMinMax;
            vfx.SetVector2("sizeMinMax", currCharSize);
            obj.SetActive(true);
            obj.GetComponent<VFXInstance>().Init();
            if (charArray[i].ToString() == ",")
            {
                lastCharWasDot = true;
            }
        }

    }

    void SpawnNumber()
    {

    }

    public void ClearPool()
    {
        vfxPool.Clear();
    }
    GameObject CreateVFX()
    {
        var instance = Instantiate(vfx_instance);
        instance.GetComponent<VFXInstance>().SetPool(vfxPool);
        return instance;
    }
    void OnTakeVFXFromPool(GameObject vfx)
    {
        //vfx.SetActive(true);
    }
    void OnReturnVFXFromPool(GameObject vfx)
    {
        vfx.SetActive(false);
    }
}