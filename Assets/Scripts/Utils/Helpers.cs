using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers {

    private static Camera cam;
    public static Camera Cam {
        get {
            if (cam == null)
                cam = Camera.main;
            return cam;
        }
    }

    private static readonly Dictionary<float, WaitForSeconds> waitDictionary = new Dictionary<float, WaitForSeconds>();
    public static WaitForSeconds GetWait(float time) {
        if (waitDictionary.TryGetValue(time, out var wait))
            return wait;

        waitDictionary[time] = new WaitForSeconds(time);
        return waitDictionary[time];
    }

    public static void DeleteChildren(this Transform t) {
        foreach (Transform child in t) {
            Object.Destroy(child.gameObject);
        }
    }
}
