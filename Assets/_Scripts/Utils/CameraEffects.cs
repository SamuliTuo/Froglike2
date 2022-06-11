using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraEffects : MonoBehaviour {

    private Camera cam;

    void Start() {
        cam = GetComponentInChildren<Camera>();
    }

    float scale = 1;
    void Update() {
        if (Keyboard.current.digit7Key.wasPressedThisFrame) {
            scale -= 0.1f;
            Time.timeScale = scale;
        }
        if (Keyboard.current.digit8Key.wasPressedThisFrame) {
            scale += 0.1f;
            Time.timeScale = scale;
        }
        if (Keyboard.current.digit9Key.wasPressedThisFrame) {
            scale = 0.1f;
            Time.timeScale = scale;
        }
        if (Keyboard.current.digit0Key.wasPressedThisFrame) {
            scale = 1;
            Time.timeScale = scale;
        }
    }
}
