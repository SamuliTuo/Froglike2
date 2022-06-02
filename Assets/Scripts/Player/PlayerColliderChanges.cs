using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum colTypes { BIG, SMALL }

public class PlayerColliderChanges : MonoBehaviour {

    public colTypes currentCol = colTypes.BIG;

    [SerializeField] private LayerMask getUpLayermask;

    private CapsuleCollider col;
    private float rayLength;


    void Start() {
        col = GetComponent<CapsuleCollider>();
        rayLength = 1.15f - 0.001f;
    }


    public void ChangeToSmallCollider() {
        if (currentCol != colTypes.SMALL)
        {
            currentCol = colTypes.SMALL;
            col.height = 1;
            col.center = new Vector3(0, -0.5f, 0);
        }
    }

    public bool TryToStandUp() {
        RaycastHit hit;
        if (Physics.SphereCast(
            transform.position + col.center,
            col.radius,
            Vector3.up,
            out hit,
            rayLength,
            getUpLayermask))
        {
            return false;
        }
        else {
            return true;
        }
    }

    public void ChangeToStandUpColliders() {
        currentCol = colTypes.BIG;
        col.height = 2f;
        col.center = Vector3.zero;
    }

    /* // debugging the rays:
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Debug.DrawRay(transform.position + col.center, Vector3.up * rayLength, Color.red);
        Gizmos.DrawWireSphere(transform.position - Vector3.up * 0.5f + Vector3.up * rayLength, 0.35f);
    }*/
}
