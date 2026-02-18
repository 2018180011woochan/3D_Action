using UnityEngine;

public class Attack2Statement : StateMachineBehaviour
{
    [Header("공격 타이밍")]
    public float clipFps = 60f;   
    public int hitFrame = 22;     

    private bool fired;
    private SlashSpawner spawner;
    private float targetTime;     // 초로 환산된 타이밍
    public AudioClip slashSfx;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        fired = false;
        spawner = animator.GetComponentInChildren<SlashSpawner>(true);
        targetTime = hitFrame / clipFps;
    }
    private void PlaySfx(Animator animator)
    {
        if (slashSfx == null) return;
        var audio = animator.GetComponent<AudioSource>();
        if (audio)
            audio.PlayOneShot(slashSfx, 1f);
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (fired || spawner == null) return;

        // 현재 스테이트 경과 시간(초)
        float t = (stateInfo.normalizedTime % 1f) * stateInfo.length;

        if (t >= targetTime)
        {
            spawner.SpawnSlash();
            PlaySfx(animator);
            fired = true; // 한 번만
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var combat = animator.GetComponent<SamuraiCombat>();
        if (combat) combat.OnAttackEnd();
    }
}
