using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour {

    public event Action onPlayerShouting;
    public event Action onPlayerNotInControl;
    public event Action<GameObject> onEnemyDeath;

    public void PlayerShouting() {
        if (onPlayerShouting != null) {
            onPlayerShouting.Invoke();
        }
    }
    public void PlayerNotInControl() {
        if (onPlayerNotInControl != null) {
            onPlayerNotInControl.Invoke();
        }
    }
    public void EnemyDeath(GameObject enemy) {
        if (onEnemyDeath != null) {
            onEnemyDeath.Invoke(enemy);
        }
    }
}
