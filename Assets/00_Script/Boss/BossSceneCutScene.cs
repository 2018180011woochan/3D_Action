using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class BossSceneCutscene : MonoBehaviour
{
    [Header("카메라 설정")]
    public CinemachineCamera bossCamera;
    public CinemachineCamera playerCamera;

    [Header("타겟 설정")]
    public Transform bossTransform;
    public Transform playerTransform;

    [Header("컷씬 타이밍")]
    public float bossShowDuration = 3f;
    public float playerShowDuration = 1f;
    public float cameraTransitionTime = 1f;

    [Header("카메라 FOV 설정")]
    public float bossCameraFOV = 40f;
    public float playerCameraFOV = 50f;

    private bool isPlayingCutscene = false;

    void Start()
    {
        StartBossSceneCutscene();
    }

    public void StartBossSceneCutscene()
    {
        if (isPlayingCutscene) return;
        StartCoroutine(PlayBossSceneCutscene());
    }

    private IEnumerator PlayBossSceneCutscene()
    {
        isPlayingCutscene = true;

        Debug.Log("보스 컷씬 시작!");

        // 1단계: 보스 비추기
        SetupBossCamera();
        yield return new WaitForSeconds(bossShowDuration);

        // 2단계: 플레이어 비추기
        SetupPlayerCamera();
        yield return new WaitForSeconds(playerShowDuration);

        // 3단계: 원래 카메라들 비활성화
        RestoreMainCamera();

        isPlayingCutscene = false;
        Debug.Log("보스 컷씬 끝!");
    }

    private void SetupBossCamera()
    {
        bossCamera.Priority = 100;
        bossCamera.Lens.FieldOfView = bossCameraFOV;

        StartCoroutine(FollowBoss());
    }

    private IEnumerator FollowBoss()
    {
        while (bossCamera != null && bossCamera.Priority > 0 && bossTransform != null)
        {
            // 보스 위치에 따라 카메라 위치 업데이트
            Vector3 bossPos = bossTransform.position;
            Vector3 cameraPos = bossPos + bossTransform.forward * 4f + Vector3.up * 2f;

            bossCamera.transform.position = cameraPos;
            bossCamera.transform.LookAt(bossPos + Vector3.up * 1.5f);

            yield return null; // 매 프레임마다 업데이트
        }
    }

    private void SetupPlayerCamera()
    {
        bossCamera.Priority = 0;
        playerCamera.Priority = 100;
        playerCamera.Lens.FieldOfView = playerCameraFOV;

        // 플레이어 앞쪽에서 플레이어를 바라보도록 설정
        Vector3 playerPos = playerTransform.position;
        Vector3 cameraPos = playerPos + playerTransform.forward * 3f + Vector3.up * 1.5f;

        playerCamera.transform.position = cameraPos;
        playerCamera.transform.LookAt(playerPos + Vector3.up * 1.5f);
    }

    private void RestoreMainCamera()
    {
        if (bossCamera != null) bossCamera.Priority = 0;
        if (playerCamera != null) playerCamera.Priority = 0;
    }
}