using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadTrigger : MonoBehaviour {

    [SerializeField] private string sceneToLoad = null;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            Singleton.instance.LevelManager.LoadScene(sceneToLoad);
        }
    }
}
