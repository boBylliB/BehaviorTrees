using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Ranged,
    Melee
}

[System.Serializable]
public class Card
{
    public string cardName;
    public int health;
    public int powerLevel;
    public int skillLevel;
    public int xpDropped = 0;
    public AttackType attackType;
    public Color outerColor;
    public Color innerColor;
    public bool favorite;
}
