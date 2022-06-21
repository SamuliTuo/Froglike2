using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadTrigger : MonoBehaviour {

    public string sceneName = null;
    public bool useIndexInstead = false;
    public int sceneIndex = 0;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            if (useIndexInstead)
            {
                Singleton.instance.LevelManager.LoadScene(sceneIndex);
            }
            Singleton.instance.LevelManager.LoadScene(sceneName);
        }
    }
}
