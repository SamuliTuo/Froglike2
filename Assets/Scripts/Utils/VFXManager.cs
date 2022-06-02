using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum VFXType {
    NULL,
    WATER_SPLASH,
    LEEK_IMPACT
};

public enum ParticleType {
    NULL,
};

public class VFXManager : MonoBehaviour {

    [SerializeField] private VisualEffect waterSplash = null;
    [SerializeField] private VisualEffect leekImpact = null;
    //[SerializeField] private ParticleSystem

    private Dictionary<VFXType, VisualEffect> visualEffects = 
        new Dictionary<VFXType, VisualEffect>();
    private Dictionary<ParticleType, ParticleSystem> particles =
        new Dictionary<ParticleType, ParticleSystem>();


    void Start() {
        ListEffects();
    }

    public void SpawnVFX(VFXType type, Vector3 pos) {
        if (type != VFXType.NULL) {
            VisualEffect system;
            visualEffects.TryGetValue(type, out system);
            system.transform.position = pos;
            system.SendEvent("OnTrigger");
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
        visualEffects.Add(VFXType.LEEK_IMPACT, leekImpact);
    }
}