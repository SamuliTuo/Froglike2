using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class NPC_Sieni_RangedCommunication : MonoBehaviour {

    private bool affectedByRanged = false;
    private HashSet<int> affectors = new HashSet<int>();
    private GameObject target;


    public void SetAffectedByRanged(bool newValue) {
        this.affectedByRanged = newValue;
    }

    public bool GetAffectedByRanged() {
        return this.affectedByRanged;
    }

    public HashSet<int> GetAffectors() {
        return this.affectors;
    }

    public void SetAffectors(HashSet<int> newAffectors) {
        this.affectors = newAffectors;
    }

    public void AddAffector(int newAff) {
        this.affectors.Add(newAff);
    }
    
    public void RemoveAffector(int affector) {
        if(affectors.Contains(affector)) {            
            affectors.Remove(affector);
        }
    }

    public void GiveNewTarget(GameObject target) {
        this.target = target;
    }

    public void CallAggroEvent() {
        CustomEvent.Trigger(transform.parent.gameObject, "RangedAggro", this.target);
    }

    /*
        Myöhemmin vaikutusalueelle tulevat sienet ei aggroonnu atm
        callaggroevent voisi tehdä jotain ennen kuin sanoo triggerjutun
            -tsekkaa ollaanko aggrossa jotenkin
    */

}

