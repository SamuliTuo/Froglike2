using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile_Fire : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 velo;
    private float speed;
    private VisualEffectAsset effect;

    public void Init(Rigidbody rb)
    {
        this.rb = rb;
        //effect = 
    }

    void Update()
    {
        velo = rb.velocity;
        //transform.rotation = Quaternion.LookRotation(velo);
    }
}
