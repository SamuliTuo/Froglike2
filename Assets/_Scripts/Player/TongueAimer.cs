using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TongueAimer : MonoBehaviour {

    [HideInInspector] public List<Transform> tongueTargets = new List<Transform>();

    [SerializeField] private float aimerDistance = 1f;
    [SerializeField] private float refreshInterval = 0.15f;
    [SerializeField] private Image highlightImage = null;
    [SerializeField] private LayerMask layerMask = 0;

    private Camera cam;
    private Transform camTrans;
    private PlayerController control;
    private TongueController tongControl;
    private Vector3 rayDir;
    private float targetDistToLine = 100;


    void Start() {
        Singleton.instance.GameEvents.onEnemyDeath += RemoveEnemyOnDeath;
        cam = Helpers.Cam;
        camTrans = cam.transform;
        control = GetComponentInParent<PlayerController>();
        tongControl = GetComponentInParent<TongueController>();
        StartCoroutine(ChooseTarget());
    }

    void LateUpdate() {
        transform.rotation = camTrans.rotation;
        if (tongControl.GetTarget() != null && control.state == PlayerStates.NORMAL) {
            highlightImage.rectTransform.position =
                cam.WorldToScreenPoint(tongControl.GetTarget().position);
            highlightImage.enabled = true;
        }
        else {
            highlightImage.enabled = false;
        }
    }

    IEnumerator ChooseTarget() {
        if (tongueTargets.Count > 0 && control.state == PlayerStates.NORMAL) {
            targetDistToLine = 100;
            foreach (Transform obj in tongueTargets) {
                rayDir = obj.position - tongControl.mouth.position;
                RaycastHit hit;
                Debug.DrawRay(tongControl.mouth.position, rayDir.normalized * 100, Color.red, 0.2f);
                if (Physics.Raycast(tongControl.mouth.position, rayDir.normalized, out hit, 100, layerMask, QueryTriggerInteraction.Ignore)) {
                    if (hit.transform.root == obj.root) {
                        float objDistToLine = MathUtils.DistanceLineSegmentPoint(
                            transform.position,
                            transform.position - transform.GetChild(0).right * aimerDistance,
                            obj.position);
                        if (objDistToLine < targetDistToLine) {
                            tongControl.SetTarget(obj);
                            targetDistToLine = objDistToLine;
                        }
                    }
                    else if (tongControl.GetTarget() == obj) {
                        tongControl.SetTarget(null);
                        targetDistToLine = 100;
                    }
                }
            }
        }
        else if (control.state != PlayerStates.TONGUE_PULL) {
            tongControl.SetTarget(null);
            targetDistToLine = 100;
        }
        yield return Helpers.GetWait(refreshInterval);
        StartCoroutine(ChooseTarget());
    }

    void RemoveEnemyOnDeath(GameObject enemy) {
        Transform tran = enemy.transform.root.GetComponent<Enemy_TongueInteraction>().tongueTargetTransform;
        if (tongueTargets.Contains(tran)) {
            tongueTargets.Remove(tran);
        }
    }
    void OnDisable() {
        Singleton.instance.GameEvents.onEnemyDeath -= RemoveEnemyOnDeath;
    }
}
