using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLootTrigger : MonoBehaviour {

    public Transform chosenGem;

    private RectTransform canvas;
    private GameObject highlighter;
    private Transform cam;
    private List<Transform> gemsInRange = new List<Transform>();
    private float closestRange, currentRange;

    void Start() {
        cam = Helpers.Cam.transform;
        highlighter = transform.GetChild(0).gameObject;
        canvas = GetComponentInChildren<RectTransform>();
    }

    public void GemLooted() {
        highlighter.SetActive(false);
        if (chosenGem != null) {
            gemsInRange.Remove(chosenGem);
            chosenGem = null;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == 9) {
            other.GetComponentInParent<LootInstance>()?.LootMe(transform.parent);

            if (other.transform.parent.name.Contains("UpgradeGem") && !gemsInRange.Contains(other.transform.parent)) {
                gemsInRange.Add(other.transform.parent);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == 9) {
            if (other.transform.parent.name.Contains("UpgradeGem") && gemsInRange.Contains(other.transform.parent)) {
                gemsInRange.Remove(other.transform.parent);
            }
        }
    }

    void LateUpdate() {
        ChooseAndHighlightGem();
    }

    void ChooseAndHighlightGem() {
        chosenGem = null;
        closestRange = 100;
        if (gemsInRange.Count > 0) {
            for (int i = 0; i < gemsInRange.Count; i++) {
                currentRange = Vector3.Distance(gemsInRange[i].position, transform.parent.position);
                if (currentRange < closestRange) {
                    chosenGem = gemsInRange[i];
                    closestRange = currentRange;
                }
            }
            HighlightChosen();
        }
        else {
            highlighter.SetActive(false);
        }
    }

    void HighlightChosen() {
        canvas.position = chosenGem.position + Vector3.up;
        canvas.LookAt(cam.position + cam.rotation * Vector3.back, cam.rotation * Vector3.up);
        if (!highlighter.activeSelf) {
            highlighter.SetActive(true);
        }
    }
}
