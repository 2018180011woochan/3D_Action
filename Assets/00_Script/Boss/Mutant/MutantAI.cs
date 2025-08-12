using System;
using UnityEngine;

public enum MutantPhase { Phase1, Phase2 }
public class MutantAI : MonoBehaviour
{
    private MonsterState monsterState;
    public MonoBehaviour phase1Controller;
    public MonoBehaviour phase2Controller;
    public float phaseSwapHP = 50f;

    public MutantPhase CurrentPhase { get; private set; } = MutantPhase.Phase1;
    public Action<MutantPhase> OnPhaseChanged;

    void Awake()
    {
        monsterState = GetComponent<MonsterState>();
    }

    void Start()
    {
        EvaluatePhase(true);
    }

    void Update()
    {
        EvaluatePhase();
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
}
