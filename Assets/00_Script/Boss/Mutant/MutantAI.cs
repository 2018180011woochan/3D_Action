using System;
using UnityEngine;
using UnityEngine.AI;

public enum MutantPhase { Phase1, Phase2 }
public class MutantAI : MonoBehaviour
{
    private MonsterState monsterState;
    public MonoBehaviour phase1Controller;
    public MonoBehaviour phase2Controller;
    public float phaseSwapHP = 50f;

    public MutantPhase CurrentPhase { get; private set; } = MutantPhase.Phase1;
    public Action<MutantPhase> OnPhaseChanged;

    public NavMeshAgent agent;
    public Animator animator;

    public bool IsDeathHandled { get; private set; } = false;

    void Awake()
    {
        monsterState = GetComponent<MonsterState>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        EvaluatePhase(true);
    }

    void Update()
    {
        if (!IsDeathHandled) EvaluatePhase();
    }


    private void EvaluatePhase(bool force = false)
    {
        float hp = monsterState.currentHP;
        MutantPhase next = (hp < phaseSwapHP) ? MutantPhase.Phase2 : MutantPhase.Phase1;

        if (force || next != CurrentPhase)
        {
            CurrentPhase = next;
            ApplyPhaseControllers(CurrentPhase);
            OnPhaseChanged?.Invoke(CurrentPhase);
        }
    }

    private void ApplyPhaseControllers(MutantPhase phase)
    {
        if (phase1Controller) phase1Controller.enabled = (phase == MutantPhase.Phase1);
        if (phase2Controller) phase2Controller.enabled = (phase == MutantPhase.Phase2);
    }
    public void HandleDeath(float dissolveDelay = 5f, float dissolveDuration = 1f)
    {
        if (IsDeathHandled) return;
        IsDeathHandled = true;

        if (phase1Controller) phase1Controller.enabled = false;
        if (phase2Controller) phase2Controller.enabled = false;

        if (agent)
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.enabled = false; 
            }
        }

        var cols = GetComponentsInChildren<Collider>(includeInactive: true);
        foreach (var c in cols) c.enabled = false;

        if (animator)
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("SlowWalk", false);
        }

        var dissolve = GetComponent<MutantSpawnDissolve>();
        if (dissolve) dissolve.PlayOut(dissolveDelay, dissolveDuration);

        enabled = false;
    }
}
