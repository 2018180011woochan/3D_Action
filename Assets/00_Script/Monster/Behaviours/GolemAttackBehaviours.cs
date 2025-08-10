using UnityEngine;

public class GolemAttackBehaviour : StateMachineBehaviour
{
    public string handPath = "";          
    public string hitBoxName = "HitBox";

    public int clipFps = 30;
    public int startSec = 1, startFrame = 0;
    public int endSec = 1, endFrame = 6;
    public float minWindow = 0.18f;

    Transform hitBox;
    bool enabledThisWindow;

    float StartTime() => startSec + (startFrame / Mathf.Max(1f, clipFps));
    float EndTime()
    {
        float s = StartTime();
        float e = endSec + (endFrame / Mathf.Max(1f, clipFps));
        return (e <= s) ? s + minWindow : e;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hitBox = null;

        if (!hitBox && !string.IsNullOrEmpty(handPath))
        {
            var hand = animator.transform.Find(handPath);
            if (hand)
            {
                var t = hand.Find(hitBoxName);
                if (t) hitBox = t;
                else
                {
                    var hb = hand.GetComponentInChildren<AttackHitBox>(true);
                    if (hb) hitBox = hb.transform;
                }
            }
        }


        if (!hitBox)
        {
            var hb = animator.GetComponentInChildren<AttackHitBox>(true);
            if (hb) hitBox = hb.transform;
        }

        if (hitBox) hitBox.gameObject.SetActive(false);
        enabledThisWindow = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!hitBox) return;

        float len = stateInfo.length;
        float t = (stateInfo.normalizedTime % 1f) * len;

        bool shouldOn = (t >= StartTime() && t < EndTime());

        if (shouldOn && !enabledThisWindow)
        {
            hitBox.gameObject.SetActive(true);  // OnEnable -> ½ºÀ® ¸®¼Â
            enabledThisWindow = true;
        }
        else if (!shouldOn && enabledThisWindow)
        {
            hitBox.gameObject.SetActive(false);
            enabledThisWindow = false;
        }

        if (stateInfo.normalizedTime >= 0.98f && enabledThisWindow)
        {
            hitBox.gameObject.SetActive(false);
            enabledThisWindow = false;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hitBox) hitBox.gameObject.SetActive(false);
        enabledThisWindow = false;
    }
}
