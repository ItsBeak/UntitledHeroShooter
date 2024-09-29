// This script is optional, only for the demo scene

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinIglesias
{
    public class BasicMotionsRootmotionResetSMB : StateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(animator.GetComponent<BasicMotionsRootmotionReset>() != null)
            {
                animator.GetComponent<BasicMotionsRootmotionReset>().ResetPositionFromSMB();
            }
        }
    }
}
