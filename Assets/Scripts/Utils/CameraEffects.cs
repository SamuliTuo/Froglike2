using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraEffects : MonoBehaviour {

    private Camera cam;

    void Start() {
        cam = GetComponentInChildren<Camera>();
    }

    void Update() {
        if (Keyboard.current.digit9Key.wasPressedThisFrame) {
            Time.timeScale = 0.1f;
        }
        if (Keyboard.current.digit0Key.wasPressedThisFrame) {
            Time.timeScale = 1;
        }
    }
}
