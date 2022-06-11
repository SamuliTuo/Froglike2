using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerTeleports : MonoBehaviour {

    [SerializeField] private Transform cameraTargetPlr = null;
    [SerializeField] private CinemachineVirtualCamera cam = null;

    private PlayerController control;
    private Rigidbody rb;
    private Transform cameraTargetKillcam;
    

    private void Awake() {
        control = GetComponent<PlayerController>();
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();
        cameraTargetKillcam = new GameObject().transform;
        cameraTargetKillcam.name = "killBoxCamTarget";
        cam = Helpers.Cam.transform.parent.GetChild(1).GetComponent<CinemachineVirtualCamera>();
    }

    public void TeleportToLastRespawn() {
        Singleton.instance.CameraChanger.ToggleLongJumpCamera(false);
        control.state = PlayerStates.NOT_IN_CONTROL;
        rb.velocity = Vector3.zero;
        cameraTargetKillcam.position = cameraTargetPlr.position;
        cam.Follow = cameraTargetKillcam;
        CutoutFade.current.StartFade(1, 0.3f, 1.5f);
        StartCoroutine(TeleportAfterTime(1.15f));
    }

    IEnumerator TeleportAfterTime(float time) {
        //make a checkpoint-handler or something that can do this nicer
        yield return Helpers.GetWait(time);
        cam.Follow = cameraTargetPlr;
        transform.position = new Vector3(0, 1.5f, 0);
        control.ResetPlayer();
        control.state = PlayerStates.NORMAL;
    }
}
