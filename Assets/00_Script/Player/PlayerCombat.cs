using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("�޺� ����")]
    public int maxCombo = 3;               // �ִ� �޺� �ܰ�
    public float comboResetTime = 1f;      // ������ ���� �� �� �ð� �ȿ� Ŭ���ϸ� �޺� �̾

    private int comboStep = 0;             // ���� �޺� �ܰ� (0=Idle, 1~3=����1~3)
    private float comboTimer = 0f;         // �ܰ� ���� �� Ÿ�̸�

    [Header("Attack1 ����")]
    public float attack1MoveDuration = 0.3f;
    public float attack1MoveDistance = 0.5f;

    [Header("Attack2 ����")]
    public float attack2SpinDuration = 0.6f;
    public float attack2ForwardDistance = 3f;

    [Header("Attack3 ����")]
    public float attack3Duration = 0.8f;
    public float attack3JumpHeight = 1.5f;
    public float attack3ForwardDistance = 4f;

    private Animator animator;

    private Coroutine attackCoroutine;
    private PlayerState playerState;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerState = GetComponentInChildren<PlayerState>();
    }

    void Update()
    {
        if (comboTimer > 0f)
            comboTimer -= Time.deltaTime;
        else
            comboStep = 0;  

        if (Input.GetMouseButtonDown(0))
        {
            HandleComboClick();
            Debug.Log("����! ������ �޾ƾ� ��");
            playerState.TakeDamage(30f);
        }
    }

    void HandleComboClick()
    {
        switch (comboStep)
        {
            case 0:
                comboStep = 1;
                animator.SetTrigger("Attack1");
                break;
            case 1:
                comboStep = 2;
                animator.SetTrigger("Attack2");
                break;
            case 2:
                comboStep = 3;
                animator.SetTrigger("Attack3");
                break;
            default:
                break;
        }

        comboTimer = comboResetTime;
    }

    public void StartAttack1Effect()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(Attack1(attack1MoveDuration, attack1MoveDistance));
    }

    public void StartAttack2Effect()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(Attack2(attack2SpinDuration, attack2ForwardDistance));
    }

    public void StartAttack3Effect()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(Attack3(attack3Duration, attack3JumpHeight, attack3ForwardDistance));
    }

    public void StopAttackEffect()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    IEnumerator Attack1(float duration, float distance)
    {
        Vector3 dir = transform.forward;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.Translate(dir * (distance * Time.deltaTime / duration), Space.World);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Attack2(float duration, float distance)
    {
        Vector3 dir = transform.forward;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float delta = Time.deltaTime;
            transform.Rotate(0, 360f * delta / duration, 0, Space.World);
            transform.Translate(dir * (distance * delta / duration), Space.World);
            elapsed += delta;
            yield return null;
        }
    }

    IEnumerator Attack3(float duration, float jumpHeight, float forwardDist)
    {
        Vector3 startPos = transform.position;
        Vector3 dir = transform.forward;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float delta = Time.deltaTime;
            float t = elapsed / duration;

            // ������ ����
            float y = 4f * jumpHeight * t * (1 - t);
            var pos = transform.position;
            pos.y = startPos.y + y;
            transform.position = new Vector3(pos.x, pos.y, pos.z);

            // ȸ�� + ����
            transform.Rotate(0, 360f * delta / duration, 0, Space.World);
            transform.Translate(dir * (forwardDist * delta / duration), Space.World);

            elapsed += delta;
            yield return null;
        }

        // �����ϰ� Y ����
        var end = transform.position;
        end.y = startPos.y;
        transform.position = end;
    }
}
