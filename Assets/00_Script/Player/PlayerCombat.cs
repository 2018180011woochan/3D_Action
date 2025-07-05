using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("�޺� ����")]
    public int maxCombo = 3;               // �ִ� �޺� �ܰ�
    public float comboResetTime = 1f;      // ������ ���� �� �� �ð� �ȿ� Ŭ���ϸ� �޺� �̾

    private int comboStep = 0;             // ���� �޺� �ܰ� (0=Idle, 1~3=����1~3)
    private float comboTimer = 0f;         // �ܰ� ���� �� Ÿ�̸�

    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 1) Ÿ�̸� ���� & ���� �� �޺� �ʱ�ȭ
        if (comboTimer > 0f)
            comboTimer -= Time.deltaTime;
        else
            comboStep = 0;  // Idle ���·�

        // 2) ��Ŭ�� �Է� ó��
        if (Input.GetMouseButtonDown(0))
            HandleComboClick();
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
                // comboStep == 3 �� ���� �� �̻� �ܰ� ����
                return;
        }

        // Ŭ���� ������ Ÿ�̸� ����
        comboTimer = comboResetTime;
    }
}
