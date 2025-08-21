using System.Collections;
using Unity.Cinemachine;
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
    public GameObject ExplosionEffect;
    public GameObject DustEffect;

    [Header("Camera")]
    public CinemachineCamera playerCamera;
    public CinemachineCamera skillCutsceneCamera;

    [Header("Sound")]
    public AudioClip skillReadySfx;   // 준비 상태 시작
    public AudioClip dashSfx;         // 대시 시작
    public AudioClip explosionSfx;    // 폭발 순간
    private AudioSource sfx;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movementScript = GetComponent<SamuraiMovement>();

        sfx = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.loop = false;
        sfx.spatialBlend = 0f; // 2D
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
        movementScript.isBusy = true;

        animator.SetTrigger("SkillReady");
        Instantiate(SkillReadyEffect, transform.position, Quaternion.identity);
        if (skillReadySfx) sfx.PlayOneShot(skillReadySfx, 1f);   // 준비 사운드

        playerCamera.Priority = 0;
        skillCutsceneCamera.Priority = 10;
        skillCutsceneCamera.transform.position = transform.position - transform.forward * 7f + Vector3.up * 3.5f;
        skillCutsceneCamera.transform.LookAt(transform);

        yield return new WaitForSeconds(4f);

        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

        for (int i = 0; i < skillEffects.Length; i++)
        {
            GameObject skillObj = Instantiate(skillEffects[i], spawnPos, Quaternion.identity);
            foreach (var ps in skillObj.GetComponentsInChildren<ParticleSystem>())
                ps.Play();

            yield return new WaitForSeconds(0.2f);
        }

        StartCoroutine(MoveCameraAfterDelay());
        if (sfx.isPlaying) sfx.Stop();
        // 대시 사운드
        if (dashSfx) sfx.PlayOneShot(dashSfx, 1f);
        yield return StartCoroutine(DashForward(3f, 0.15f));

        StartCoroutine(SpawnAfterEffects(spawnPos));

        yield return new WaitForSeconds(2f);
        skillCutsceneCamera.Priority = 0;
        playerCamera.Priority = 10;

        movementScript.Stance = false;
        animator.SetBool("IsStance", false);
        movementScript.isBusy = false;
    }

    private IEnumerator MoveCameraAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        skillCutsceneCamera.transform.position = transform.position + transform.forward * 10f + Vector3.up * 1.2f;
        skillCutsceneCamera.transform.LookAt(transform);
    }

    private IEnumerator DashForward(float distance, float duration)
    {
        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }

    private IEnumerator SpawnAfterEffects(Vector3 originPos)
    {
        yield return new WaitForSeconds(2f);

        GameObject explosion = Instantiate(ExplosionEffect, originPos, Quaternion.identity);
        foreach (var ps in explosion.GetComponentsInChildren<ParticleSystem>())
            ps.Play();

        // 폭발 사운드
        if (explosionSfx) sfx.PlayOneShot(explosionSfx, 1f);

        yield return new WaitForSeconds(0.5f);

        GameObject dust = Instantiate(DustEffect, originPos, Quaternion.identity);
        foreach (var ps in dust.GetComponentsInChildren<ParticleSystem>())
            ps.Play();
    }

    public void OnAttackEnd()
    {
        movementScript.isBusy = false;
    }
}
