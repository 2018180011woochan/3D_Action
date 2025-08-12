using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MutantPhase1 : MonoBehaviour
{
    public float detectionRange = 30f;
    public float stopDistance = 6f;
    public float walkSpeed = 2.5f;
    public float closeStopDistance = 2f; 
    public float slowWalkSpeed = 1.2f;

    enum State { Idle, Approach, SlowApproach, Stop }
    State state = State.Idle;

    NavMeshAgent agent;
    Animator animator;
    Transform player;
    bool started = false;
    bool playedSlowAnim = false;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;


        agent.stoppingDistance = stopDistance;
        agent.updateRotation = true;
        agent.isStopped = true;

        StartCoroutine(BeginAfterDelay());
    }

    IEnumerator BeginAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        started = true;
    }

    void Update()
    {
        if (!started || !player) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Idle:
                animator.SetBool("IsWalking", false);
                if (dist <= detectionRange) state = State.Approach;
                break;

            case State.Approach:
                agent.speed = walkSpeed;
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator.SetBool("IsWalking", true);

                if (dist <= stopDistance)
                {
                    state = State.SlowApproach;
                    playedSlowAnim = false;
                }
                break;

            case State.SlowApproach:
                agent.speed = slowWalkSpeed;
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.stoppingDistance = closeStopDistance;
                agent.SetDestination(player.position);

                if (!playedSlowAnim)
                {
                    animator.SetBool("IsWalking", false);
                    animator.SetBool("SlowWalk", true);
                }

                if (dist <= closeStopDistance)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    animator.SetBool("SlowWalk", false);
                    playedSlowAnim = false;
                    state = State.Stop;
                }

                if (dist >= stopDistance)
                {
                    animator.SetBool("SlowWalk", false);
                    animator.SetBool("IsWalking", true);

                    agent.speed = walkSpeed;
                    agent.stoppingDistance = stopDistance;
                    state = State.Approach;
                    playedSlowAnim = false;                 
                    break;
                }
                break;
            case State.Stop:
                FacePlayer();
                if (dist > stopDistance + 0.5f)
                    state = State.Approach;
                break;
        }
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 7f);
    }
}