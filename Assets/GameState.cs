using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State { SAFE, COMBAT, IN_MENUS, }
public class GameState : MonoBehaviour
{
    public State state = State.SAFE;
    List<GameObject> aggroedEnemies = new List<GameObject>();

    public void SetState(State state)
    {
        this.state = state;
    }

    public void AddAggroedEnemy(GameObject enemy)
    {
        if (aggroedEnemies.Contains(enemy) == false)
        {
            aggroedEnemies.Add(enemy);
        }
        state = State.COMBAT;
    }

    public void RemoveAggroedEnemy(GameObject enemy)
    {
        if (aggroedEnemies.Contains(enemy))
        {
            aggroedEnemies.Remove(enemy);
        }
        if (aggroedEnemies.Count == 0)
        {
            state = State.SAFE;
        }
    }
}
