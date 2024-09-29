using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.CodeGenerating;

public enum ItemState { World, Held }
public enum ItemIKType { None, Left, Right, Both }

public abstract class ItemBase : NetworkBehaviour
{
    public string ItemName { get; private set; }
    [AllowMutableSyncType] SyncVar<ItemState> CurrentState = new SyncVar<ItemState>(ItemState.World);
    Rigidbody rb;

    [Header("IK Settings")]
    public ItemIKType ikType;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        CurrentState.OnChange += OnItemStateChanged;
    }

    private void OnItemStateChanged(ItemState oldState, ItemState newState, bool asServer)
    {

    }

    public virtual void OnPickup(EquipSlot equipParent)
    {
        CurrentState.Value = ItemState.Held;
        CmdSetTransformParent(equipParent);
        SetKinematic(true);
    }

    public virtual void OnDrop()
    {
        CurrentState.Value = ItemState.World;
        CmdSetTransformParent(null);
        SetKinematic(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void CmdSetTransformParent(EquipSlot parent)
    {
        if (IsServerInitialized)
        {
            NetworkObject.SetParent(parent);

            if (parent != null)
            {
                CurrentState.Value = ItemState.Held;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                RpcSetKinematic(true);
            }
            else
            {
                RpcSetKinematic(false);
                CurrentState.Value = ItemState.World;
            }
        }
    }

    void SetKinematic(bool isKinematic)
    {
        if (rb != null)
        {
            rb.isKinematic = isKinematic;
        }
    }

    [ObserversRpc]
    void RpcSetKinematic(bool isKinematic)
    {
        if (rb != null)
        {
            rb.isKinematic = isKinematic;
        }
    }

    public ItemState GetItemState()
    {
        return CurrentState.Value;
    }

    public bool CanBePickedUp()
    {
        return CurrentState.Value == ItemState.World;
    }

    public abstract void UseItem();

}
