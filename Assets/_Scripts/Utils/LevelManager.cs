using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

    [SerializeField] private GameObject loaderCanvas;
    [SerializeField] private Image progressBar;


    public void LoadScene(string sceneName) 
    {
        var scene = SceneManager.LoadSceneAsync(sceneName);
        ActivateLoad(scene);
    }
    public void LoadScene(int sceneIndex)
    {
        var scene = SceneManager.LoadSceneAsync(sceneIndex);
        ActivateLoad(scene);
    }

    void ActivateLoad(AsyncOperation scene)
    {
        scene.allowSceneActivation = false;
        loaderCanvas.SetActive(true);

        do
        {
            progressBar.fillAmount = scene.progress * 0.9f;
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        loaderCanvas.SetActive(false);
        ClearPools();
    }

    void ClearPools() {
        print("Pools have been cleaned up.");
        Singleton.instance.RebootSingleton();
    }
}
