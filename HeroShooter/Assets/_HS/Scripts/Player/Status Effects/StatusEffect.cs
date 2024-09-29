using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect : MonoBehaviour
{
    [Header("Base Status Effect Settings")]
    public string statusEffectName = "Status Effect";
    public Sprite statusIcon = null;
    public Color statusColor = Color.white;
    public float duration = 5f;
    protected float timeElapsed = 0f;

    public abstract void ApplyEffect(PlayerHealth target);
    public abstract void TickEffect(PlayerHealth target);
    public abstract void EndEffect(PlayerHealth target);

    public StatusEffectUI statusEffectUI;

    public bool IsExpired()
    {
        timeElapsed += Time.deltaTime;
        return timeElapsed >= duration;
    }

    public void SetUIElement(StatusEffectUI element)
    {
        statusEffectUI = element;
        statusEffectUI.statusIcon.color = statusColor;
        statusEffectUI.statusDuration.color = statusColor;
    }

    public float GetTimeElapsed()
    {
        return timeElapsed;
    }
}
