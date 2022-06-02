using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;

public class NPC_SieniRanged_PikkuSieniFinder : MonoBehaviour {

    public float shroomSearchRadius = 30f;
    private float t = 0f;
    private List<GameObject> effectShrooms = new List<GameObject>();


    void Start() {
        Singleton.instance.GameEvents.onEnemyDeath += RemoveFromListOnEffectShroomDeath;
    }

    void Update() {
        t += Time.deltaTime;
        if(t > 1.5f){
            t = 0f;
            FindShrooms();
            SetShroomsAsAffected();
        }
    }

    private void RemoveFromListOnEffectShroomDeath(GameObject enemy) {
        if(effectShrooms.Contains(enemy.transform.root.gameObject)) {
            effectShrooms.Remove(enemy.transform.root.gameObject);
        }
    }

    void OnDestroy() {
        Singleton.instance.GameEvents.onEnemyDeath -= RemoveFromListOnEffectShroomDeath;
    }

    /*
        HUOM: Lisää aggrottavien listaan funktion sisällä määritellyn stringin perusteella
    */
    private void FindShrooms() {
        //int layerMask = 1 << 10; //vain enemyt
        Collider[] hitColliders = Physics.OverlapSphere(transform.root.position, shroomSearchRadius);
        for(int i = 0; i < hitColliders.Length; i++) {
            if (hitColliders[i].transform.root == this.transform.root) {
                continue;
            }
            if(hitColliders[i].transform.root.gameObject.name.Contains("Enemy_shroom")) {
                if(!effectShrooms.Contains(hitColliders[i].transform.root.gameObject)) {
                    effectShrooms.Add(hitColliders[i].transform.root.gameObject);
                }
            }     
        }
        //Debug.Log("post ps count: "+pikkusienet.Count);
    }

    private void SetShroomsAsAffected() {
        if(effectShrooms.Count > 0) {
            foreach(GameObject ps in effectShrooms) {
                ps.GetComponentInChildren<NPC_Sieni_RangedCommunication>().SetAffectedByRanged(true);
                ps.GetComponentInChildren<NPC_Sieni_RangedCommunication>().AddAffector(GetInstanceID());
            }
        }
    }

    /* Kuollessa käydään vaikutetut pikkusienet läpi ja tsekataan ollaanko ainoita vaikuttajia -
        jos ollaan niin laitetaan affected falseksi
    */ 
    public void OnDeathAffectedOff() {
        if (effectShrooms.Count > 0) {
            foreach(GameObject ps in effectShrooms) {
                var comms = ps.GetComponentInChildren<NPC_Sieni_RangedCommunication>();
                foreach(int affector in comms.GetAffectors()) {
                }
                comms.RemoveAffector(GetInstanceID());
                if(comms.GetAffectors().Count == 0) {
                    comms.SetAffectedByRanged(false);
                }
            }
        }
        //Debug.Log("post ps count: " + pikkusienet.Count);
    }

    public string DebugGetAffected() {
        string s = "";
        foreach(GameObject ps in effectShrooms) {
            s += ps.GetComponentInChildren<NPC_Sieni_RangedCommunication>().GetAffectedByRanged();
            s += " ";
        }
        return s;
    }

    //feedaa targetti
    public void CallAffectedShroomsAggros(GameObject target) {
        if(target == null) {
            return;
        }
        foreach (GameObject ps in effectShrooms) {
            ps.GetComponentInChildren<NPC_Sieni_RangedCommunication>().GiveNewTarget(target);
            ps.GetComponentInChildren<NPC_Sieni_RangedCommunication>().CallAggroEvent();
        }
    }
}
