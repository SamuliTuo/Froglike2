using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraChanger : MonoBehaviour 
{
    private CinemachineVirtualCamera vcam_normal;
    private CinemachineVirtualCamera vcam_longJump;
    private bool longJumpCamera = false;


    public void SetReferences(CinemachineVirtualCamera vcam_normal, CinemachineVirtualCamera vcam_longJump) 
    {
        this.vcam_normal = vcam_normal;
        this.vcam_longJump = vcam_longJump;
    }

    public void ToggleLongJumpCamera(bool state) 
    {
        if (state && !longJumpCamera)
        {
            vcam_normal.Priority = 9;
            vcam_longJump.Priority = 10;
            longJumpCamera = true;
        }
        else if (!state && longJumpCamera)
        {
            vcam_normal.Priority = 10;
            vcam_longJump.Priority = 9;
            longJumpCamera = false;
        }
    }
}
