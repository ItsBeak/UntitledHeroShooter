using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Damage
{
    public float amount;
    public DamageType type;
    public NetworkObject damageSource;

    /// <summary>
    /// Damage datatype for tracking damage
    /// </summary>
    /// <param name="damageAmount">Amount of damage</param>
    /// <param name="damageType">Type of damage</param>
    /// <param name="source">The source of the damage</param>
    public Damage(float damageAmount, DamageType damageType, NetworkObject source = null)
    {
        amount = damageAmount;
        type = damageType;
        damageSource = source;
    }
}

public enum DamageType
{
    General,
    Blunt,
    Slicing,
    Bullet,
    Fire,
    Explosive,
    Poison
}
