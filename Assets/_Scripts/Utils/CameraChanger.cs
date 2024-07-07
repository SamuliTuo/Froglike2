using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum cameras
{
    NORMAL, ROLL, LONGJUMP,
}

public class CameraChanger : MonoBehaviour 
{
    public cameras current;

    private CinemachineVirtualCamera vcam_normal;
    private CinemachineVirtualCamera vcam_roll;
    private CinemachineVirtualCamera vcam_longJump;
    private bool longJumpCamera = false;


    public void SetReferences(CinemachineVirtualCamera vcam_normal, CinemachineVirtualCamera vcam_longJump, CinemachineVirtualCamera vcam_roll) 
    {
        this.vcam_normal = vcam_normal;
        this.vcam_longJump = vcam_longJump;
        this.vcam_roll = vcam_roll;
    }

    public void ToggleCamera(cameras c)
    {
        switch (c)
        {
            case cameras.NORMAL:
                current = c;
                vcam_normal.Priority = 10;
                vcam_longJump.Priority = 9;
                vcam_roll.Priority = 9;
                break;

            case cameras.ROLL:
                current = c;
                vcam_normal.Priority = 9;
                vcam_longJump.Priority = 9;
                vcam_roll.Priority = 10;
                break;

            case cameras.LONGJUMP:
                vcam_normal.Priority = 9;
                vcam_longJump.Priority = 10;
                vcam_roll.Priority = 9;
                break;

            default: break;
        }
    }
}
