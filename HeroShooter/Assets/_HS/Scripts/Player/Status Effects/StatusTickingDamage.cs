using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusTickingDamage : StatusEffect
{
    [Header("Ticking Damage Settings")]
    public Damage damagePerTick = new Damage(2f, DamageType.General, null);
    public float tickInterval = 1f;
    private float timeSinceLastTick = 0f;

    public override void ApplyEffect(PlayerHealth target)
    {
        //Debug.Log("Added ticking damage status on player " + target.gameObject.name);

        target.CmdDamage(damagePerTick);
        timeSinceLastTick = 0f;

        // Add some visual to the ticking damage and possibly sound effect here, as well as any UI addition
    }

    public override void TickEffect(PlayerHealth target)
    {
        timeSinceLastTick += Time.deltaTime;

        if (timeSinceLastTick >= tickInterval)
        {
            target.CmdDamage(damagePerTick);
            timeSinceLastTick = 0f;
        }
    }

    public override void EndEffect(PlayerHealth target)
    {
        //Debug.Log("Removed ticking damage status on player " + target.gameObject.name);

        // Remove visuals and clean up UI
    }
}
