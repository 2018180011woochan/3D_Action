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
    public float playerCameraFOV = 100f;

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

        SetupBossCamera();
        yield return new WaitForSeconds(bossShowDuration);

        SetupPlayerCamera();
        yield return new WaitForSeconds(playerShowDuration);

        RestoreMainCamera();

        isPlayingCutscene = false;
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
            Vector3 bossPos = bossTransform.position;

            Vector3 cameraPos = bossPos + bossTransform.forward * 8f + Vector3.up * 8f;

            bossCamera.transform.position = cameraPos;

            Vector3 lookAtPos = bossPos + Vector3.up * 1f;
            bossCamera.transform.LookAt(lookAtPos);

            yield return null; 
        }
    }

    private void SetupPlayerCamera()
    {
        bossCamera.Priority = 0;
        playerCamera.Priority = 100;
        playerCamera.Lens.FieldOfView = playerCameraFOV;

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