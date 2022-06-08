using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesUnpacker : MonoBehaviour {

    List<Transform> children = new List<Transform>();

    void Awake() {
        for (int i = 0; i < transform.childCount; i++) {
            children.Add(transform.GetChild(i));
        }
        foreach (var child in children) {
            child.SetParent(null);
        }
    }
}