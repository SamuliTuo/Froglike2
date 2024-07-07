using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NumberType { normal, crit, heal, fire, ice }
public enum Symbols
{
    number_0, number_1, number_2, number_3, number_4, number_5, number_6, number_7, number_8, number_9, number_point,
    number_crit_0, number_crit_1, number_crit_2, number_crit_3, number_crit_4, number_crit_5, number_crit_6, number_crit_7, number_crit_8, number_crit_9, number_crit_point,
    number_heal_0, number_heal_1, number_heal_2, number_heal_3, number_heal_4, number_heal_5, number_heal_6, number_heal_7, number_heal_8, number_heal_9, number_heal_point,
    number_plus, number_fire, number_ice
}

public class DamageNumberSprites : MonoBehaviour
{
    public Dictionary<Symbols, Texture2D> pairs_normal = new Dictionary<Symbols, Texture2D>();
    public Dictionary<Symbols, Texture2D> pairs_crit = new Dictionary<Symbols, Texture2D>();
    public Dictionary<Symbols, Texture2D> pairs_heal = new Dictionary<Symbols, Texture2D>();
    public Dictionary<Symbols, Texture2D> pairs_fire = new Dictionary<Symbols, Texture2D>();
    public Dictionary<Symbols, Texture2D> pairs_ice = new Dictionary<Symbols, Texture2D>();

    void Awake()
    {
        FillPairLists();
    }

    public Texture2D GetTextureFromChar(char letter, NumberType type)
    {
        Texture2D tex;
        if (letter.ToString() == ".")
            tex = FindTextureByType("point", type);
        else
            tex = FindTextureByType(letter.ToString(), type);
        return tex;
    }

    // TO DO!
    // Make stylized numbers for damage types. Ie. dot, slowing, poison etc. 
    Texture2D FindTextureByType(string letter, NumberType type)
    {
        string name;
        switch (type)
        {
            case NumberType.normal: name = "number_" + letter; break;
            case NumberType.crit: name = "number_crit_" + letter; break;
            case NumberType.heal: name = "number_heal_" + letter; break;
            case NumberType.fire: name = "number_fire"; break;
            case NumberType.ice: name = "number_ice"; break;
            default: name = "number_0"; break;
        }
        return GetTexture(ParseEnum(name), type);
    }
    Symbols ParseEnum(string name)
    {
        return (Symbols)Enum.Parse(typeof(Symbols), name);
    }

    public Texture2D GetTexture(Symbols symbol, NumberType type)
    {
        Texture2D tex;
        switch (type)
        {
            case NumberType.normal: pairs_normal.TryGetValue(symbol, out tex); break;
            case NumberType.crit: pairs_crit.TryGetValue(symbol, out tex); break;
            case NumberType.heal: pairs_heal.TryGetValue(symbol, out tex); break;
            case NumberType.fire: pairs_fire.TryGetValue(symbol, out tex); break;
            case NumberType.ice: pairs_ice.TryGetValue(symbol, out tex); break;
            default: pairs_normal.TryGetValue(symbol, out tex); break;
        }
        return tex;
    }
    void FillPairLists()
    {
        pairs_normal.Clear();
        pairs_crit.Clear();
        pairs_heal.Clear();
        pairs_fire.Clear();
        pairs_ice.Clear();
        foreach (Symbols symbol in Enum.GetValues(typeof(Symbols)))
        {
            string s = "DamageNumbers/" + symbol.ToString();
            Texture2D tex2 = Resources.Load(s) as Texture2D;
            if (s.Contains("crit"))
                pairs_crit.Add(symbol, tex2);
            else if (s.Contains("heal"))
                pairs_heal.Add(symbol, tex2);
            else if (s.Contains("fire"))
                pairs_fire.Add(symbol, tex2);
            else if (s.Contains("ice"))
                pairs_ice.Add(symbol, tex2);
            else
                pairs_normal.Add(symbol, tex2);
        }
    }
}
