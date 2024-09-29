using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using FishNet.Object;
using RootMotion.FinalIK;
using FIMSpace.FLook;

public class PlayerBody : NetworkBehaviour
{
    [Header("IK Components")]
    Transform lookAtTarget;
    public ArmIK leftArmIK, rightArmIK;

    [Header("Components")]
    public PlayerController controller;
    public FLookAnimator lookAnimator;
    public PlayerHealth health;


    private void Awake()
    {
        //bodyAnimator = GetComponent<Animator>();
        controller = GetComponentInParent<PlayerController>();
        lookAtTarget = controller.playerCamera.m_LookAt;
        health = GetComponentInParent<PlayerHealth>();
        //isRagdolled.OnChange += OnRagdollStateChanged;

        health.OnDeathEvent.AddListener(OnDeath);
        health.OnReviveEvent.AddListener(OnRevive);
    }

    void Update()
    {
        if (health.currentHealthState.Value == PlayerHealthState.Dead)
        {
            leftArmIK.solver.IKPositionWeight = 0;
            leftArmIK.solver.IKRotationWeight = 0;
            rightArmIK.solver.IKPositionWeight = 0;
            rightArmIK.solver.IKRotationWeight = 0;
            return;
        }

        if (leftArmIK.solver.arm.target != null)
        {
            leftArmIK.solver.IKPositionWeight = 1;
            leftArmIK.solver.IKRotationWeight = 1;
        }
        else
        {
            leftArmIK.solver.IKPositionWeight = 0;
            leftArmIK.solver.IKRotationWeight = 0;
        }

        if (rightArmIK.solver.arm.target != null)
        {
            rightArmIK.solver.IKPositionWeight = 1;
            rightArmIK.solver.IKRotationWeight = 1;
        }
        else
        {
            rightArmIK.solver.IKPositionWeight = 0;
            rightArmIK.solver.IKRotationWeight = 0;
        }

    }

    public void SetArmIKTargets(Transform leftArmTarget, Transform rightArmTarget)
    {
        leftArmIK.solver.arm.target = leftArmTarget;
        rightArmIK.solver.arm.target = rightArmTarget;
    }

    public void ClearArmIKTargets()
    {
        leftArmIK.solver.arm.target = null;
        rightArmIK.solver.arm.target = null;
    }

    void OnDeath()
    {
        lookAnimator.enabled = false;
    }

    void OnRevive()
    {
        lookAnimator.enabled = true;
    }

}
