using UnityEngine;

public class Attack1Statement : StateMachineBehaviour
{
    [Header("Hit timings (frames)")]
    public float clipFps = 60f;        
    public int firstHitFrame = 10;     
    public int secondHitFrame = 68;    

    private bool fired1, fired2;
    private SlashSpawner spawner;
    public AudioClip slashSfx;
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

        if (!fired1 && t >= t1) {
            spawner.SpawnSlash(); fired1 = true;
            PlaySfx(animator);
        }
        if (!fired2 && t >= t2) {
            spawner.SpawnSlash(); fired2 = true;
            PlaySfx(animator);
        }
    }
    private void PlaySfx(Animator animator)
    {
        if (slashSfx == null) return;
        var audio = animator.GetComponent<AudioSource>();
        if (audio)
            audio.PlayOneShot(slashSfx, 1f);  
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var combat = animator.GetComponent<SamuraiCombat>();
        if (combat) combat.OnAttackEnd();
    }
}
