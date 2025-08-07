using UnityEngine;

public class SamuraiCombat : MonoBehaviour
{
    private Animator animator;
    private SamuraiMovement movementScript;

    public float doubleClickTime = 0.1f;
    private bool isWaitingDoubleClick = false;
    private float firstClickTime;

    void Awake()
    { 
        animator = GetComponent<Animator>();
        movementScript = GetComponent<SamuraiMovement>();
    }

    void Update()
    {
        if (movementScript.Stance == false) return;
        if (movementScript.isBusy) return;

        if (isWaitingDoubleClick && Time.time - firstClickTime > doubleClickTime)
        {
            isWaitingDoubleClick = false;
            Attack1();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!isWaitingDoubleClick)
            {
                isWaitingDoubleClick = true;
                firstClickTime = Time.time;
            }
            else
            {
                Attack3();
                isWaitingDoubleClick = false;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Attack2();
        }
    }


    private void Attack1()
    {
        movementScript.isBusy = true;
        animator.SetTrigger("Attack1");
    }

    private void Attack2()
    {
        movementScript.isBusy = true;
        animator.SetTrigger("Attack2");
    }

    private void Attack3()
    {
        movementScript.isBusy = true;
        animator.SetTrigger("Attack3");
    }

    public void OnAttackEnd()
    {
        movementScript.isBusy = false;
    }
}
