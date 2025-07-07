using UnityEngine;
public class AttackBehaviour : StateMachineBehaviour
{
    [Tooltip("히트박스 ON 시작 (초)")]
    public float hitTimeStart = 1f + 8f / 30f;   
    [Tooltip("히트박스 OFF (초)")]
    public float hitTimeEnd = 1f + 20f / 30f;  

    Transform hitBox;
    bool isEnabled;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hitBox = animator.transform.Find("hand.r/AttackHitBox");
        if (hitBox) hitBox.gameObject.SetActive(false);
        isEnabled = false;

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitBox == null) return;

        float clipLength = stateInfo.length;
        float time = (stateInfo.normalizedTime % 1f) * clipLength;

        if (!isEnabled && time >= hitTimeStart && time < hitTimeEnd)
        {
            hitBox.gameObject.SetActive(true);
            isEnabled = true;
        }
        else if (isEnabled && time >= hitTimeEnd)
        {
            hitBox.gameObject.SetActive(false);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitBox) hitBox.gameObject.SetActive(false);
    }
}
