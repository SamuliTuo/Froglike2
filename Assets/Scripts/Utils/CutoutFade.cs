using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class CutoutFade : MonoBehaviour {

    public static CutoutFade current;

    [SerializeField] private RectTransform cutout = null;

    private bool fadeInProcess = false;
    private float fadeOutTime;
    private float fadeInTime;
    private float blackPeriod;
    private float t;


    void Start() {
        current = this;
        if (cutout.gameObject.activeSelf) {
            cutout.gameObject.SetActive(false);
        }
    }

    public void StartFade(
        float fadeOutTime = 1f,
        float blackPeriodDuration = 0.6f,
        float fadeInTime = 0.5f)
    {
        if (fadeInProcess) {
            return;
        }
        this.fadeOutTime = fadeOutTime;
        this.blackPeriod = blackPeriodDuration;
        this.fadeInTime = fadeInTime;
        fadeInProcess = true;
        t = 0;
        cutout.gameObject.SetActive(true);
        StartCoroutine(Fade());
    }

    IEnumerator Fade() {
        while (t < fadeOutTime) {
            t += Time.deltaTime;
            float perc = Mathf.Sin((t / fadeOutTime) * Mathf.PI * 0.5f);
            float size = Mathf.Lerp(3000, 0, perc);
            cutout.sizeDelta = new Vector2(size, size);
            yield return null;
        }
        yield return Helpers.GetWait(blackPeriod);
        while (t >= fadeOutTime && t < fadeOutTime + fadeInTime) {
            t += Time.deltaTime;
            float perc = 1f - Mathf.Cos(((t - fadeOutTime) / fadeInTime) * Mathf.PI * 0.5f);
            float size = Mathf.Lerp(0, 3000, perc);
            cutout.sizeDelta = new Vector2(size, size);
            yield return null;
        }
        cutout.gameObject.SetActive(false);
        fadeInProcess = false;
    }
}
