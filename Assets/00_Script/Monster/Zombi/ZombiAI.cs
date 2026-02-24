using UnityEngine;
using UnityEngine.AI;

public class ZombiAI : MonoBehaviour
{
    public float attackRange = 2f;
    public float attackDuration = 2f;   // 공격 시간: 2초

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    bool started;
    bool attacking;
    public bool isDead = false;

    [Header("Boss")]
    public GameObject Boss;

    private MonsterState ms;
    private NecromanserAI bossAI;
    private bool handledBossDeath = false;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        ms = GetComponent<MonsterState>();
        agent.isStopped = true; 
        Boss = GameObject.FindWithTag("Boss");
        bossAI = Boss.GetComponent<NecromanserAI>();
    }

    void OnEnable()
    {
        StartCoroutine(BeginAI());
    }

    System.Collections.IEnumerator BeginAI()
    {
        yield return new WaitForSeconds(3f); 
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) yield break;

        player = p.transform;
        agent.isStopped = false;
        animator.SetBool("Run", true);
        started = true;
    }

    void Update()
    {
        if (!handledBossDeath && bossAI.isDead)
        {
            handledBossDeath = true;

            if (!ms.isDead)
            {
                //ms.TakeDamage(ms.currentHP);
            }
            return; 
        }
        if (isDead) return;
        if (!started || player == null) return;

        if (!attacking)
        {
            agent.SetDestination(player.position);

            if (Vector3.Distance(transform.position, player.position) <= attackRange)
                StartCoroutine(AttackRoutine());
        }

        FacePlayer();
    }

    System.Collections.IEnumerator AttackRoutine()
    {
        attacking = true;

        agent.ResetPath();
        agent.isStopped = true;
        animator.SetBool("Run", false);
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDuration);

        attacking = false;
        agent.isStopped = false;
        animator.SetBool("Run", true);
    }

    void FacePlayer()
    {
        if (!player) return;
        Vector3 dir = player.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
    }
}
