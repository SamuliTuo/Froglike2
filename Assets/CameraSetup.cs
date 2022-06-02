using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSetup : MonoBehaviour 
{

    [SerializeField] private CinemachineVirtualCamera vcam_normal;
    [SerializeField] private CinemachineVirtualCamera vcam_longJump;


    private void Start()
    {
        Singleton.instance.CameraChanger.SetReferences(vcam_normal, vcam_longJump);
    }
}
