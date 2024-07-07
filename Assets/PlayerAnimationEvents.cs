using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public Transform leftFoot, rightFoot;

    public void LeftFootDown()
    {
        Singleton.instance.ParticleEffects.SpawnContinuousSmoke(leftFoot.transform.position, -leftFoot.transform.forward);
    }
    public void RightFootDown()
    {
        Singleton.instance.ParticleEffects.SpawnContinuousSmoke(rightFoot.transform.position, -rightFoot.transform.forward);
    }
    public void LeftFootKick()
    {
        Singleton.instance.ParticleEffects.SpawnContinuousSmoke(leftFoot.transform.position, -leftFoot.transform.forward);
    }
    public void RightFootKick()
    {
        Singleton.instance.ParticleEffects.SpawnContinuousSmoke(rightFoot.transform.position, -rightFoot.transform.forward);
    }
}
