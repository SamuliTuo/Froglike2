using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Loot Instance")]
public class LootInstanceSettings : ScriptableObject {

    [Header("MAKE ALL LIST LENGTHS OF EQUAL LENGTHS! Ie. 3, 3 and 3.")]
    public List<LootType> types = new List<LootType>();
    [Header("N:st element of counts tells how many N:st element of types will spawn.")]
    public List<int> counts = new List<int>();
    [Header("The final amount spawned of type N is:")]
    [Header("counts(N) + random(-variations(N), variations(N))")]
    public List<int> variations = new List<int>();

    public Dictionary<LootType, int> loots = new Dictionary<LootType, int>();

    private int randomCount;


    public Dictionary<LootType, int> GetLoots() {
        loots.Clear();
        for (int i = 0; i < types.Count; i++) {
            if (!loots.ContainsKey(types[i])) {
                randomCount = counts[i] + Random.Range(-variations[i], variations[i] + 1);
                loots.Add(types[i], randomCount < 0 ? 0 : randomCount);
            }
        }
        return loots;
    }
}
