using UnityEngine;

// 이 스크립트는 에디터에서 애니메이션 이벤트를 수동으로 추가하기 위한 '도우미'입니다.
public class AnimationEventHelper : MonoBehaviour
{
    [Header("이벤트 추가할 애니메이션 클립")]
    public AnimationClip targetClip; // 여기에 'slash1' 애니메이션 클립을 넣어주세요.

    [Header("호출할 함수 정보")]
    public string functionName = "OnAttackAnimationEnd";

    [Tooltip("이벤트를 추가할 시간 (초). 예: 1초 길이 클립의 끝은 1.0")]
    public float eventTime = 0.9f;

    // Inspector 창에서 이 스크립트의 ... 메뉴를 누르면 "Add Event To Clip" 이라는 버튼이 나타납니다.
    [ContextMenu("Add Event To Clip")]
    void AddEvent()
    {
        if (targetClip == null)
        {
            Debug.LogError("Target Clip이 비어있습니다! 애니메이션 클립을 넣어주세요.");
            return;
        }

        // 기존에 있던 이벤트들을 모두 지우고 싶다면 아래 줄의 주석을 푸세요.
        // targetClip.events = new AnimationEvent[0];

        // 새로운 애니메이션 이벤트를 생성합니다.
        AnimationEvent newEvent = new AnimationEvent();

        // 이벤트의 속성을 설정합니다.
        newEvent.time = eventTime; // 이벤트가 발생할 시간
        newEvent.functionName = functionName; // 호출할 함수의 이름

        // 애니메이션 클립에 우리가 만든 새로운 이벤트를 추가합니다.
        targetClip.AddEvent(newEvent);

        Debug.Log($"성공! '{targetClip.name}' 클립의 {eventTime}초에 '{functionName}' 함수 호출 이벤트를 추가했습니다.");
    }
}