using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class BossPhaseCutscene : MonoBehaviour
{
    [Header("Refs")]
    public MutantAI bossAI;               // 보스 AI (MutantAI)
    public Transform boss;                // 보스 루트 Transform (Mutant)
    public CinemachineCamera playerCam;   // 플레이어용 CM 카메라
    public CinemachineCamera cutsceneCam; // 컷씬용 CM 카메라 (고정샷)

    [Header("Shot (relative to boss)")]
    public float distanceBack = 6f;   // 보스 뒤쪽 거리(카메라가 뒤에서 보스를 본다)
    public float height = 2f;         // 카메라 높이
    public float sideOffset = 1.5f;   // 약간의 측면 오프셋(= 더 멋진 각도)

    [Header("Timing (seconds)")]
    public float blendIn = 0.6f;  // 전환 들어가는 시간(Brain Default Blend와 맞추면 좋아)
    public float hold = 2.2f;     // 보스를 비추는 시간
    public float blendOut = 0.6f; // 플레이어로 돌아가는 전환 시간

    [Header("Disable during cutscene")]
    // 컷씬 동안 비활성화할 컴포넌트들(플레이어 조작/락온/입력 등)
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

        // 2) 컷씬 카메라 위치/각도 배치 (Follow/LookAt 없이 고정샷)
        Vector3 pos = boss.position
                      + boss.forward * distanceBack
                      + Vector3.up * height
                      + boss.right * sideOffset;

        Quaternion rot = Quaternion.LookRotation(boss.position - pos, Vector3.up);
        cutsceneCam.transform.SetPositionAndRotation(pos, rot);

        // 3) 컷씬 카메라로 블렌딩 (Priority 올리기)
        int originalPlayerPriority = playerCam ? playerCam.Priority : 10;
        int originalCutscenePriority = cutsceneCam.Priority;

        cutsceneCam.Priority = Mathf.Max(originalPlayerPriority + 10, 100);

        // (선택) 보스 페이즈 전환 연출 트리거
        var anim = boss.GetComponentInChildren<Animator>();
        anim.SetTrigger("Rage"); // 애니메이터에 같은 트리거가 있어야 함
        anim.SetBool("IsPhase2", true);
        yield return new WaitForSeconds(blendIn + hold);

        // 4) 다시 플레이어 카메라로
        cutsceneCam.Priority = originalCutscenePriority;

        yield return new WaitForSeconds(blendOut);

        foreach (var b in disableDuringCutscene)
        {
            if (b is MutantPhase1) continue;
            b.enabled = true;
        }
    }
}
