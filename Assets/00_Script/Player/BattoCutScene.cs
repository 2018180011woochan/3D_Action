using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class BattoCutScene : MonoBehaviour
{
    [Header("카메라 설정")]
    public CinemachineCamera battojutsuCamera;
    public CinemachineCamera mainCamera;

    [Header("컷씬 설정")]
    public float cutsceneDuration = 2f;
    public float closeUpFOV = 35f;
    public float normalFOV = 60f;

    public void TriggerBattojutsuCutscene()
    {
        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        // 발도술 카메라 활성화
        battojutsuCamera.Priority = 100;

        // FOV 조정으로 확대 효과
        battojutsuCamera.Lens.FieldOfView = closeUpFOV;

        // 컷씬 지속시간 대기
        yield return new WaitForSeconds(cutsceneDuration);

        // 원래 카메라로 복귀
        battojutsuCamera.Priority = 0;
    }
}
