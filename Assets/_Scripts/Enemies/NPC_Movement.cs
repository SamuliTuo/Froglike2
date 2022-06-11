using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Movement : MonoBehaviour {

    public UnityEngine.AI.NavMeshAgent agent;
    //private IEnumerator coroutine;
    Vector3 targetPos_out;
    bool hasTarget = false;
    //bool destinationReached = false;
    float startSpeed;
    float startAngularSpeed;
    float startAcceleration;
    bool canRotate = true;

    void Awake() {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        this.startSpeed = agent.speed;
        this.startAngularSpeed = agent.angularSpeed;
        this.startAcceleration = agent.acceleration;
    }
    
    public void MoveTo(Vector3 targetPos, float speed) {
        if(speed > 0) {
            agent.speed = speed;
        }
        hasTarget = true;
        agent.SetDestination(targetPos);
    }
    
    public void Dash(float dashSpeed, float dashAcceleration, Vector3 dashTarget) {
        agent.speed = dashSpeed;
        agent.acceleration = dashAcceleration;
        agent.isStopped = false;
        agent.SetDestination(dashTarget);
    }
    
    public Vector3 getTargetPos() {
        return targetPos_out;
    }

    public bool getHasTarget() {
        return hasTarget;
    }

    public void setHasTarget(bool state) {
        this.hasTarget = state;
    }

    public void SetSpeed(float newSpeed) {
        agent.speed = newSpeed;
    }

    public float RemainingDist() {
        return GetPathRemainingDistance(this.agent);
    }

    public static float GetPathRemainingDistance(UnityEngine.AI.NavMeshAgent navMeshAgent)
    {
        if (navMeshAgent.pathPending ||
            navMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid ||
            navMeshAgent.path.corners.Length == 0)
            return 10000f; //oli -1

        float distance = 0.0f;
        for (int i = 0; i < navMeshAgent.path.corners.Length - 1; ++i)
        {
            distance += Vector3.Distance(navMeshAgent.path.corners[i], navMeshAgent.path.corners[i + 1]);
        }

        return distance;
    }

    public float GetSpeed() {
        return agent.speed;
    }

    public float GetStartSpeed() {
        return this.startSpeed;
    }

    public float GetStartAngularSpeed() {
        return this.startAngularSpeed;
    }

    public void SetAngularSpeed(float newAngular) {
        agent.angularSpeed = newAngular;
    }

    public float GetAcceleration() {
        return agent.acceleration;
    }

    public void SetAcceleration(float newAcceleration) {
        agent.acceleration = newAcceleration;
    }

    public float GetStartAcceleration() {
        return this.startAcceleration;
    }
    public void ResetDestination() {
        agent.destination = transform.position;
    }

    public void RotationUpdate(Vector3 target, float rotateSpeed) {
        if(canRotate) {
            Vector3 dir = target - transform.position;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotateSpeed);
        }
    }

    public void SetCanRotate(bool newValue) {
        this.canRotate = newValue;
    }
}
