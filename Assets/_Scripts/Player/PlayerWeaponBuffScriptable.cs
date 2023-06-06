using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/PlayerWeaponBuff")]
public class PlayerWeaponBuffScriptable : ScriptableObject 
{
    public float timeBeforeBuffStarts = 1;
    public float animationLength = 1;
    public float buffDuration = 5;
    public Elements buffElement = Elements.NULL;
    public float poiseDmg = 1;
    public float kb = 1;
    public float damage;
}
