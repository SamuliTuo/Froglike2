using UnityEngine;

public class WaterTrigger : MonoBehaviour {

    private Vector3 point;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            point = other.ClosestPoint(transform.position + Vector3.up * (transform.localScale.y * 0.5f));
            Singleton.instance.VFXManager.SpawnVFX(
                VFXType.WATER_SPLASH,
                new Vector3(point.x, transform.position.y + transform.localScale.y * 0.501f, point.z));
        }
    }
}
