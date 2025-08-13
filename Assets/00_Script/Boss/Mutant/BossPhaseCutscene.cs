using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class BossPhaseCutscene : MonoBehaviour
{
    [Header("Refs")]
    public MutantAI bossAI;               
    public Transform boss;                
    public CinemachineCamera playerCam;   
    public CinemachineCamera cutsceneCam; 

    [Header("Shot (relative to boss)")]
    public float distanceBack = 6f;   
    public float height = 2f;         
    public float sideOffset = 1.5f;   

    [Header("Timing (seconds)")]
    public float blendIn = 0.6f;  
    public float hold = 2.2f;     
    public float blendOut = 0.6f; 

    [Header("Disable during cutscene")]
    public Behaviour[] disableDuringCutscene;

    private bool played = false;

    void OnEnable()
    {
        if (bossAI != null) bossAI.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDisable()
    {
        if (bossAI != null) bossAI.OnPhaseChanged -= OnPhaseChanged;
    }

    private void OnPhaseChanged(MutantPhase phase)
    {
        if (phase == MutantPhase.Phase2 && !played && isActiveAndEnabled)
            StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        played = true;

        // 1) 조작 잠금
        foreach (var b in disableDuringCutscene)
            if (b) b.enabled = false;

        if (bossAI.agent) { bossAI.agent.isStopped = true; bossAI.agent.ResetPath(); }

        // ③ 남아있을 공격 트리거/상태 정리
        if (bossAI.animator)
        {
            // 네가 쓰는 트리거 이름들에 맞춰서
            string[] attackTriggers = { "Attack1", "Attack2", "Attack3", "Attack4" };
            foreach (var t in attackTriggers) bossAI.animator.ResetTrigger(t);

            string[] hit = { "GetHit1", "GetHit2", "GetHit3", "GetHit4" };
            foreach (var t in hit) bossAI.animator.ResetTrigger(t);

            bossAI.animator.SetBool("IsWalking", false);
            bossAI.animator.SetBool("SlowWalk", false);
        }

        Vector3 pos = boss.position
                      + boss.forward * distanceBack
                      + Vector3.up * height
                      + boss.right * sideOffset;

        Quaternion rot = Quaternion.LookRotation(boss.position - pos, Vector3.up);
        cutsceneCam.transform.SetPositionAndRotation(pos, rot);

        int originalPlayerPriority = playerCam ? playerCam.Priority : 10;
        int originalCutscenePriority = cutsceneCam.Priority;

        cutsceneCam.Priority = Mathf.Max(originalPlayerPriority + 10, 100);

        var anim = boss.GetComponentInChildren<Animator>();
        anim.SetTrigger("Rage"); 
        anim.SetBool("IsPhase2", true);
        yield return new WaitForSeconds(blendIn + hold);

        cutsceneCam.Priority = originalCutscenePriority;

        yield return new WaitForSeconds(blendOut);

        foreach (var b in disableDuringCutscene)
        {
            if (b is MutantPhase1) continue;
            b.enabled = true;
        }
    }
}
