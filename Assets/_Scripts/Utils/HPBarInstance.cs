using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class HPBarInstance : MonoBehaviour {

    public void SetPool(IObjectPool<GameObject> pool) => this.pool = pool;

    [SerializeField] private float timeBeforeDisappearing = 10f;

    private Coroutine coroutine = null;
    private Transform cam, parent;
    private NPC_Sieni_CombatStats parentScript;
    private RectTransform canvas;
    private Image barImage;
    private IObjectPool<GameObject> pool;
    private float posOffset, t;


    void Awake() {
        cam = Helpers.Cam.transform;
        canvas = GetComponentInChildren<RectTransform>();
        barImage = transform.GetChild(0).GetChild(1).GetComponent<Image>();
    }

    public void Init(Transform parent, NPC_Sieni_CombatStats parentScript, float posOffset) {
        this.parent = parent;
        this.parentScript = parentScript;
        this.posOffset = posOffset;
        t = timeBeforeDisappearing;
        coroutine = StartCoroutine(HpBarUpdater());
    }

    public void SetBarValue(float perc) {
        barImage.fillAmount = perc;
        t = timeBeforeDisappearing;
    }

    IEnumerator HpBarUpdater() {
        while (t > 0 && parent != null) {
            canvas.position = parent.position + Vector3.up * posOffset;
            canvas.LookAt(cam.position + cam.rotation * Vector3.back, cam.rotation * Vector3.up);
            t -= Time.deltaTime;
            yield return null;
        }
        Deactivate();
    }

    public void Deactivate() {
        if (coroutine != null) {
            StopAllCoroutines();
        }
        coroutine = null;
        parent = null;
        if (parentScript != null) {
            parentScript.DeactivateHPBar();
        }
        parentScript = null;

        if (pool != null) {
            pool.Release(this.gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
}
