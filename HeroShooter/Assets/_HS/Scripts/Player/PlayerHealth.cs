using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using FishNet.Object;
using FishNet.Object.Synchronizing;

using FishNet;
using NUnit.Framework.Constraints;
using FishNet.Component.Animating;

public enum PlayerHealthState
{
    Alive,
    Unconscious,
    Dead
}

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] float maxHealth = 100f;
    public readonly SyncVar<float> currentHealth = new SyncVar<float>(100f);
    public readonly SyncVar<PlayerHealthState> currentHealthState = new SyncVar<PlayerHealthState>(PlayerHealthState.Alive);
    public readonly SyncDictionary<DamageType, float> resistances = new SyncDictionary<DamageType, float>();

    [Header("Status Effect Settings")]
    [SerializeField] Transform statusEffectContainer;
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    public GameObject statusEffectUIPrefab;

    [Header("Events")]
    public UnityEvent<float, float> OnHealthChangedEvent;
    public UnityEvent<PlayerHealthState> OnHealthStateChangedEvent;
    public UnityEvent OnDeathEvent;
    public UnityEvent OnUnconsciousEvent;
    public UnityEvent OnReviveEvent;

    [Header("Components")]
    public PlayerHealthUI healthUI;
    public NetworkAnimator animator;
    PlayerEquipment equipment;

    [Header("TEMPORARY")]
    public NetworkObject bleedingEffectPrefab;
    public NetworkObject poisonEffectPrefab;

    private void Awake()
    {
        currentHealth.OnChange += OnHealthChanged;
        currentHealthState.OnChange += OnHealthStateChanged;
        InitializeResistances();
        equipment = GetComponent<PlayerEquipment>();
    }

    void Start()
    {
        if (IsServerInitialized)
        {
            currentHealth.Value = maxHealth;
            currentHealthState.Value = PlayerHealthState.Alive;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            healthUI.gameObject.SetActive(true);
        }
        else
        {
            healthUI.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            CmdDamage(new Damage(10, DamageType.General, NetworkObject));
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            CmdHeal(10);
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            this.CmdAddStatusEffect(bleedingEffectPrefab);
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            this.CmdAddStatusEffect(poisonEffectPrefab);
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            this.CmdModifyResistance(DamageType.General, 0.25f);
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            this.CmdModifyResistance(DamageType.General, -0.25f);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            this.CmdRevive(false);
        }

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].statusEffectUI)
            {
                activeEffects[i].statusEffectUI.statusDuration.fillAmount = 1 - (activeEffects[i].GetTimeElapsed() / activeEffects[i].duration);
            }


            activeEffects[i].TickEffect(this);

            if (activeEffects[i].IsExpired())
            {
                RemoveStatusEffect(activeEffects[i]);
            }
        }
    }

    #region Base Health Functions

    [ServerRpc]
    public void CmdDamage(Damage damage)
    {
        Damage(damage);
    }

    [Server]
    public void Damage(Damage damage)
    {
        if (currentHealthState.Value == PlayerHealthState.Dead)
            return;

        float damageToTake = CalculateDamage(damage);

        damageToTake = Mathf.Clamp(damageToTake, 0, maxHealth);

        currentHealth.Value -= damageToTake;

        Debug.Log(NetworkObject.name + " took " + damageToTake + " " + damage.type.ToString() + " damage.");

        EvaluateHealthState();
    }

    float CalculateDamage(Damage damage)
    {
        float damageToTake = damage.amount;

        // Check for any resistances or vulnerabilities the player has applied to them

       
        damageToTake *= GetResistanceMultiplier(damage.type);

        if (currentHealthState.Value == PlayerHealthState.Unconscious)
        {
            damageToTake *= 2;
        }

        return damageToTake;
    }

    [ServerRpc]
    public void CmdHeal(float healAmount)
    {
        Heal(healAmount);
    }

    [Server]
    public void Heal(float healAmount)
    {
        if (currentHealthState.Value == PlayerHealthState.Dead)
            return;

        float healthToHeal = currentHealth.Value + healAmount;
        healthToHeal = Mathf.Clamp(healthToHeal, 0, maxHealth);

        currentHealth.Value = healthToHeal;

        EvaluateHealthState();
    }

    [Server]
    void EvaluateHealthState()
    {
        if (currentHealth.Value <= 0 && currentHealthState.Value != PlayerHealthState.Dead)
        {
            currentHealthState.Value = PlayerHealthState.Dead;
            OnDeathEvent?.Invoke();
        }
        else if (currentHealth.Value > 0 && currentHealthState.Value == PlayerHealthState.Dead)
        {
            currentHealthState.Value = PlayerHealthState.Alive;
            OnReviveEvent?.Invoke();
        }

    }

    void OnHealthChanged(float oldValue, float newValue, bool asServer)
    {
        OnHealthChangedEvent?.Invoke(oldValue, newValue);

        if (!IsOwner)
            return;
        
        //if (asServer)
        //{
        //    Debug.Log("SERVER: Health changed from " + oldValue + " to " + newValue + ".");
        //}
        //else
        //{
        //    Debug.Log("CLIENT: Health changed from " + oldValue + " to " + newValue + ".");
        //}

        healthUI.healthText.text = newValue.ToString();
        healthUI.healthFill.fillAmount = currentHealth.Value / maxHealth;

        if (oldValue > newValue)
        {
            healthUI.PlayDamageEffect();
        }
    }

    void OnHealthStateChanged(PlayerHealthState oldState, PlayerHealthState newState, bool asServer)
    {
        OnHealthStateChangedEvent?.Invoke(newState);

        if (newState == PlayerHealthState.Dead)
        {
            animator.Animator.SetBool("isDead", true);
           
            if (asServer)
            {
                equipment.RpcDropItem();
            }
        }
        else if (newState == PlayerHealthState.Alive)
        {
            animator.Animator.SetBool("isDead", false);
        }

    }

    [ServerRpc]
    public void CmdRevive(bool atFullHealth)
    {
        Revive(atFullHealth);
    }

    [Server]
    public void Revive(bool atFullHealth)
    {
        if (currentHealthState.Value != PlayerHealthState.Dead)
            return;

        currentHealth.Value = atFullHealth ? maxHealth : maxHealth * 0.2f;
        currentHealthState.Value = PlayerHealthState.Alive;
    }



    #endregion

    #region Status Effect Functions

    [ServerRpc(RequireOwnership = false)]
    public void CmdAddStatusEffect(NetworkObject effectPrefab)
    {
        if (!IsOwner)
        {
            RpcAddStatusEffect(effectPrefab);
        }
        else
        {
            AddStatusEffect(effectPrefab.GetComponent<StatusEffect>());
        }
    }

    [ObserversRpc]
    void RpcAddStatusEffect(NetworkObject effectPrefab)
    {
        AddStatusEffect(effectPrefab.GetComponent<StatusEffect>());
    }

    void AddStatusEffect(StatusEffect effect)
    {
        if (!activeEffects.Contains(effect))
        {
            StatusEffect newEffect = Instantiate(effect, statusEffectContainer);
            InstanceFinder.ServerManager.Spawn(newEffect.gameObject);
            activeEffects.Add(newEffect);
            newEffect.ApplyEffect(this);

            GameObject statusUI = Instantiate(statusEffectUIPrefab, healthUI.statusEffectUIContainer);
            statusUI.GetComponent<StatusEffectUI>().SetStatusData(effect.statusIcon, effect.statusEffectName);
            newEffect.SetUIElement(statusUI.GetComponent<StatusEffectUI>());

        }
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        if (activeEffects.Contains(effect))
        {
            Destroy(effect.statusEffectUI.gameObject);

            effect.EndEffect(this);
            activeEffects.Remove(effect);
            InstanceFinder.ServerManager.Despawn(effect.gameObject);
            Destroy(effect.gameObject);
        }
    }



    #endregion

    #region Resistances & Vulnerabilities Functions

    void InitializeResistances()
    {
        //resistances = new SyncDictionary<DamageType, float>()
        //{
        //    {DamageType.General, 0.0f},
        //    {DamageType.Blunt, 0.0f},
        //    {DamageType.Slicing, 0.0f},
        //    {DamageType.Bullet, 0.0f},
        //    {DamageType.Fire, 0.0f},
        //    {DamageType.Explosive, 0.0f},
        //    {DamageType.Poison, 0.0f}
        //};
    }

    float GetResistanceMultiplier(DamageType type)
    {
        float resistance = 0.0f;

        if (resistances.TryGetValue(type, out resistance))
        {
            return 1 - resistance;
        }

        return 1;
    }

    [ServerRpc]
    public void CmdSetResistance(DamageType type, float value)
    {
        RpcSetResistance(type, value);
    }

    [ObserversRpc]
    public void RpcSetResistance(DamageType type, float value)
    {
        SetResistance(type, value);
    }

    public void SetResistance(DamageType type, float value)
    {
        if (resistances.ContainsKey(type))
        {
            resistances[type] = value;
        }
        else
        {
            resistances.Add(type, value);
        }    
    }

    [ServerRpc]
    public void CmdModifyResistance(DamageType type, float value)
    {
        RpcModifyResistance(type, value);
    }

    [ObserversRpc]
    public void RpcModifyResistance(DamageType type, float value)
    {
        ModifyResistance(type, value);
    }

    public void ModifyResistance(DamageType type, float value)
    {
        if (resistances.ContainsKey(type))
        {
            resistances[type] += value;
            //resistances[type] = Mathf.Clamp(resistances[type], 0, 1);
        }
        else
        {
            resistances.Add(type, value);
        }
    }

    [ServerRpc]
    public void CmdClearResistance(DamageType type)
    {
        RpcClearResistance(type);
    }

    [ObserversRpc]
    public void RpcClearResistance(DamageType type)
    {
        ClearResistance(type);
    }

    public void ClearResistance(DamageType type)
    {
        if (resistances.ContainsKey(type))
        {
            resistances[type] = 0;
        }
        else
        {
            resistances.Add(type, 0);
        }
    }

    #endregion

}
