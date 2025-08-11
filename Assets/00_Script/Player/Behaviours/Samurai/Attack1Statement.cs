using UnityEngine;

public class Attack1Statement : StateMachineBehaviour
{
    [Header("Hit timings (frames)")]
    public float clipFps = 60f;        // 클립 프레임레이트(보통 60)
    public int firstHitFrame = 10;     // 0:10  => 10프레임
    public int secondHitFrame = 68;    // 1:08  => 60+8 = 68프레임

    private bool fired1, fired2;
    private SlashSpawner spawner;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        fired1 = fired2 = false;
        spawner = animator.GetComponentInChildren<SlashSpawner>(true);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (spawner == null) return;

        float t = (stateInfo.normalizedTime % 1f) * stateInfo.length;

        float t1 = firstHitFrame / clipFps;   
        float t2 = secondHitFrame / clipFps;

        if (!fired1 && t >= t1) { spawner.SpawnSlash(); fired1 = true; }
        if (!fired2 && t >= t2) { spawner.SpawnSlash(); fired2 = true; }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var combat = animator.GetComponent<SamuraiCombat>();
        if (combat) combat.OnAttackEnd();
    }
}
