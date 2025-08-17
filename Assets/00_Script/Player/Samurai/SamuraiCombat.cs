using System.Collections;
using UnityEngine;

public class SamuraiCombat : MonoBehaviour
{
    private Animator animator;
    private SamuraiMovement movementScript;

    public float doubleClickTime = 0.1f;
    private bool isWaitingDoubleClick = false;
    private float firstClickTime;

    public GameObject SkillReadyEffect;
    public GameObject[] skillEffects;
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(FireSkillRoutine());
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

    private IEnumerator FireSkillRoutine()
    {
        animator.SetTrigger("SkillReady");
        Instantiate(SkillReadyEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(4f);

        // SkillEffect1~4를 0.5초 간격으로 생성
        for (int i = 0; i < skillEffects.Length; i++)
        {
            GameObject skillObj = Instantiate(skillEffects[i], transform.position, Quaternion.identity);

            // 내부 모든 ParticleSystem 찾아서 Play 실행
            ParticleSystem[] particles = skillObj.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Play();
            }

            yield return new WaitForSeconds(0.5f); // 다음 스킬 이펙트 생성 전 대기
        }
    }

    public void OnAttackEnd()
    {
        movementScript.isBusy = false;
    }
}
