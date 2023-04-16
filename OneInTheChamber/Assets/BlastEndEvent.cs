using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastEndEvent : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Destroy(animator.gameObject);
        Debug.Log("destroy");
    }
}
