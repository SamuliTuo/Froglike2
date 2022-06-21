using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggro : MonoBehaviour
{
    public void Aggroed()
    {
        Singleton.instance.GameState.AddAggroedEnemy(gameObject);
    }
    public void DeAggroed()
    {
        Singleton.instance.GameState.RemoveAggroedEnemy(gameObject);
    }
}
