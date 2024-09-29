using UnityEngine;

using FishNet.Object;

public class PlayerEquipment : NetworkBehaviour
{
    [Header("Components")]
    public PlayerBody playerBody;
    public EquipSlot itemEquipSlot;
    ItemBase currentItem;

    [Header("Interaction")]
    [SerializeField]
    float interactionRange = 2f;
    [SerializeField]
    LayerMask interactableLayers;

    [Header("Keycodes")]
    KeyCode interactKey = KeyCode.F;
    KeyCode dropKey = KeyCode.Q;
    KeyCode useKey = KeyCode.Mouse0;

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            TryPickupItem();
        }
        else if (Input.GetKeyDown(dropKey))
        {
            TryDropItem();
        }
        else if (Input.GetKeyDown(useKey))
        {
            TryUseItem();
        }
    }

    void TryPickupItem()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionRange, interactableLayers))
        {
            ItemBase item = hit.transform.GetComponent<ItemBase>();
            if (item != null && item.CanBePickedUp())
            {
                //CmdPickupItem(item);
                currentItem = item;
                playerBody.SetArmIKTargets(((currentItem.ikType == ItemIKType.Left) || (currentItem.ikType == ItemIKType.Both)) ? currentItem.leftHandTarget : null, ((currentItem.ikType == ItemIKType.Right) || (currentItem.ikType == ItemIKType.Both)) ? currentItem.rightHandTarget : null);

                item.OnPickup(itemEquipSlot);

                CmdSetItem(currentItem);
            }
        }
    }

    [ObserversRpc]
    public void RpcDropItem()
    {
        TryDropItem();
    }

    void TryDropItem()
    {
        if (currentItem != null)
        {
            playerBody.ClearArmIKTargets();

            currentItem.OnDrop();
            currentItem = null;

            CmdClearItem();
        }
    }

    void TryUseItem()
    {
        if (currentItem != null)
        {
            currentItem.UseItem();
        }
    }

    [ServerRpc]
    public void CmdSetItem(ItemBase item)
    {
        RpcSetItem(item);
    }


    [ObserversRpc(ExcludeOwner = true)]
    public void RpcSetItem(ItemBase item)
    {
        if (currentItem == null)
        {
            currentItem = item;
            playerBody.SetArmIKTargets(((currentItem.ikType == ItemIKType.Left) || (currentItem.ikType == ItemIKType.Both)) ? currentItem.leftHandTarget : null, ((currentItem.ikType == ItemIKType.Right) || (currentItem.ikType == ItemIKType.Both)) ? currentItem.rightHandTarget : null);
            Debug.Log("Toggling IK on remote client");
        }
        else
        {
            Debug.LogWarning("Player already has an item");
        }
    }

    [ServerRpc]
    public void CmdClearItem()
    {
        RpcClearItem();
    }

    [ObserversRpc(ExcludeOwner = true)]
    public void RpcClearItem()
    {
        playerBody.ClearArmIKTargets();
        currentItem = null;
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void CmdPickupItem(ItemBase item)
    //{
    //    item.NetworkObject.GiveOwnership(Owner);
    //
    //}

}
