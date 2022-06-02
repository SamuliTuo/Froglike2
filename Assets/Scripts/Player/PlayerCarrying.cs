using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCarrying : MonoBehaviour {

    [SerializeField] private Transform carryPos = null;
    [SerializeField] private float pickUpLerpSpeed = 1f;
    [SerializeField] private float dropLayerWeightLerpSpeed = 1f;
    [SerializeField] private float throwTime = 1f;
    [SerializeField] private float throwForceForward = 10f;
    [SerializeField] private float throwForceUp = 0.33f;
    [SerializeField] private float dropForceForward = 1f;
    [SerializeField] private float dropForceUp = 1f;

    private PlayerController control;
    private PlayerLootTrigger lootTrigger;
    private Animator anim;
    private Transform objHeld = null;
    private Rigidbody rb;
    private Rigidbody objRB;
    private Collider col;
    private Quaternion startRot;
    private Vector3 startPos;
    private bool throwing = false;
    private float posOffset;
    private float t, t2;


    void Start() {
        control = GetComponent<PlayerController>();
        lootTrigger = GetComponentInChildren<PlayerLootTrigger>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    void OnAttack() {
        if (objHeld && !throwing) {
            ThrowObj();
        }
    }

    void OnCarry() {
        if (control.state == PlayerStates.NOT_IN_CONTROL) {
            return;
        }

        if (objHeld == null) {
            if (lootTrigger.chosenGem != null) {
                lootTrigger.chosenGem.GetComponent<UpgradeGem>().LootMe();
                lootTrigger.GemLooted();
            }
            else {
                TryToPickUp();
            }
        }
        else {
            DropObj();
        }
        
    }

    void TryToPickUp() {
        if (control.state != PlayerStates.NORMAL) {
            return;
        }
        Collider[] objects = Physics.OverlapSphere(transform.position + transform.GetChild(0).forward * 0.6f, 1.5f);
        Transform closestObj = null;
        foreach (var obj in objects) {
            if (obj.CompareTag("Carryable")) {
                if (closestObj == null) {
                    closestObj = obj.transform;
                }
                else if ((transform.position - obj.transform.position).sqrMagnitude <
                    (transform.position - closestObj.position).sqrMagnitude) {
                    closestObj = obj.transform;
                }
            }
        }
        if (closestObj != null) {
            PickUpObj(closestObj);
        }
    }

    public void PickUpObj(Transform obj) {
        if (objHeld != null) {
            return;
        }
        objHeld = obj;
        objRB = objHeld.GetComponent<Rigidbody>();
        objRB.useGravity = false;

        // Get the pos offset on different colliders
        var objCol = objHeld.GetComponent<Collider>();
        if (objCol is BoxCollider) {
            posOffset = ((BoxCollider)objCol).size.z * 0.5f;
        }
        else if (objCol is SphereCollider) {
            posOffset = ((SphereCollider)objCol).radius;
        }
        else if (objCol is CapsuleCollider) {
            posOffset = ((CapsuleCollider)objCol).radius;
        }
        else if (objCol is MeshCollider) {
            posOffset = ((MeshCollider)objCol).sharedMesh.bounds.size.magnitude * 0.5f;
        }
        //posOffset = objHeld.GetComponent<Collider>().bounds.extents.magnitude * 0.5f;
        startRot = objHeld.rotation;
        startPos = objHeld.position;
        Physics.IgnoreCollision(col, objHeld.GetComponent<Collider>(), true);
        control.state = PlayerStates.CARRYING;
        t = 0;
    }

    public void CarryObj() {
        if (t < 1) {
            t += pickUpLerpSpeed * Time.deltaTime;
            float perc = Mathf.Sin(t * Mathf.PI * 0.5f);
            objHeld.transform.rotation = 
                Quaternion.Lerp(startRot, carryPos.rotation, perc);
            objHeld.transform.position = 
                Vector3.Lerp(startPos, carryPos.position + Vector3.up * posOffset, perc);
            anim.SetLayerWeight(1, t);
            if (t >= 1) {
                anim.SetLayerWeight(1, 1);
            }
            return;
        }
        objHeld.transform.rotation = carryPos.rotation;
        objHeld.transform.position = carryPos.position + Vector3.up * posOffset;
    }

    void ThrowObj() {
        anim.Play("throw", 1, 0);
        throwing = true;
        StartCoroutine(ThrowRoutine());
    }

    void DropObj() {
        Physics.IgnoreCollision(col, objHeld.GetComponent<Collider>(), false);
        objRB.useGravity = true;
        objRB.velocity =
            transform.GetChild(0).forward * dropForceForward -
            transform.GetChild(0).right * dropForceForward +
            Vector3.up * dropForceUp;
        objHeld = null;
        control.state = PlayerStates.NORMAL;
        StartCoroutine(DropAnimWeight());
    }

    IEnumerator DropAnimWeight() {
        t2 = 0;
        while (t2 < 1) {
            t2 += dropLayerWeightLerpSpeed * Time.deltaTime;
            anim.SetLayerWeight(1, 1 - t2);
            yield return null;
        }
        anim.SetLayerWeight(1, 0);
    }

    IEnumerator ThrowRoutine() {
        yield return Helpers.GetWait(throwTime);
        Physics.IgnoreCollision(col, objHeld.GetComponent<Collider>(), false);
        objRB.useGravity = true;
        objRB.velocity = transform.GetChild(0).forward * throwForceForward + Vector3.up * throwForceUp + rb.velocity * 0.8f;
        objHeld = null;
        control.state = PlayerStates.NORMAL;
        StartCoroutine(DropAnimWeight());
        throwing = false;
    }
}
