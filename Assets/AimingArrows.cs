using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingArrows : MonoBehaviour
{
    public Vector3 force;

    [SerializeField] private GameObject arrowBody = null;
    [SerializeField] private GameObject arrowTip = null;
    [SerializeField] private float minLen = 1.0f;
    [SerializeField] private float maxLen = 1.0f;

    
    void Start()
    {
        arrowBody.SetActive(false);
        arrowTip.SetActive(false);
    }

    private Transform target;
    PlayerSpecialScriptable attack;
    public void ActivateArrow(Vector3 pos1, Vector3 pos2, Transform target, PlayerSpecialScriptable attack)
    {
        this.attack = attack;
        this.target = target;
        control = target.GetComponent<PlayerController>();
        Vector3 dir = pos1 - pos2;
        arrowBody.transform.position = pos1;
        arrowBody.transform.LookAt(arrowBody.transform.position + dir);
        arrowTip.transform.LookAt(arrowBody.transform.position + dir);
        arrowTip.transform.position = pos2;

        arrowBody.SetActive(true);
        arrowTip.SetActive(true);
    }

    public float multiplier = 0.01f;
    public void UpdateArrow(Vector3 pos1, Vector3 pos2, float perc)
    {
        var actualDir = CalculateTheActualDirection(perc);
        actualDir *= multiplier;
        Vector3 dir = pos2 - pos1;
        dir *= Mathf.Lerp(minLen, maxLen, perc);
        arrowBody.transform.position = pos1 + actualDir * 0.5f;
        arrowBody.transform.LookAt(arrowBody.transform.position + actualDir * 100);
        arrowBody.transform.localScale = new(
            arrowBody.transform.localScale.x, arrowBody.transform.localScale.y, actualDir.magnitude);
        arrowTip.transform.LookAt(arrowBody.transform.position + actualDir * 100);
        arrowTip.transform.position = pos1 + actualDir;
    }

    PlayerController control;
    Vector3 CalculateTheActualDirection(float perc)
    {
        force = target.forward;
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            target.rotation = Quaternion.LookRotation(control.GetInput());
        }

        Vector2 playerInput;
        playerInput.x = Mathf.Abs(control.xAxis) > control.deadzone ? control.xAxis : 0;
        playerInput.y = Mathf.Abs(control.yAxis) > control.deadzone ? control.yAxis : 0;
        playerInput = playerInput.normalized; // Vector2.ClampMagnitude(playerInput, 1f);
        Transform inputSpace = control.GetPlrInputSpace();

        if (inputSpace)
        {
            Vector3 inputSpaceForward = inputSpace.forward;
            if (inputSpace.forward.y < 0)
            {
                if (control.PlayerGrounded)
                {
                    inputSpaceForward.y = 0;
                    inputSpaceForward = inputSpaceForward.normalized;
                }
                else
                {
                    perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
                    inputSpaceForward.y *= perc;
                    inputSpaceForward = inputSpaceForward.normalized;
                }
            }
            if (control.PlayerGrounded == false && control.GetInput().sqrMagnitude < control.deadzoneSquared)
                force += target.forward * Mathf.Lerp(attack.stepForceMinMax.x, attack.stepForceMinMax.y, perc);
            else
                force += (inputSpaceForward * playerInput.y + inputSpace.right * playerInput.x).normalized *
                        Mathf.Lerp(attack.stepForceMinMax.x, attack.stepForceMinMax.y, perc);
        }
        else
        {
            force += (target.forward * playerInput.y + target.right * playerInput.x) *
                Mathf.Lerp(attack.stepForceMinMax.x, attack.stepForceMinMax.y, perc);
        }
        return force;
    }

    public void DeactivateArrow()
    {
        arrowBody.SetActive(false);
        arrowTip.SetActive(false);
    }
}
