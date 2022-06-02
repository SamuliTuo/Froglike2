using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Sieni_Animations : MonoBehaviour {

    [SerializeField] private Animator anim = null;

    void Awake() {
        anim = GetComponentInChildren<Animator>();
    }

    public void PlayAnimation(object animation, float animStartPerc, float playbackSpeed = 1) {
        anim.speed = playbackSpeed;
        string clip = animation.ToString();
        anim.Play(clip, 0, animStartPerc);
    }
}