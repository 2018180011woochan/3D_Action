using UnityEngine;

public class SamuraiCombat : MonoBehaviour
{
    private Animator animator;
    private SamuraiMovement movementScript;

    void Awake()
    { 
        animator = GetComponent<Animator>();
        movementScript = GetComponent<SamuraiMovement>();
    }

    void Update()
    {
        if (movementScript.Stance == false) return;

        if (movementScript.isBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            Attack1();
        }
    }

    private void Attack1()
    {
        movementScript.isBusy = true;

        animator.SetTrigger("Attack1");
    }

    public void OnAttackEnd()
    {
        movementScript.isBusy = false;
    }
}
