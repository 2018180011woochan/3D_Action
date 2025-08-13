using System.Collections;
using UnityEngine;

public class MutantPhase2 : MonoBehaviour
{
    public MutantAI mutantAI;
    Transform player;
    bool started = false;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mutantAI.agent.updateRotation = true;
        mutantAI.agent.isStopped = true;
        StartCoroutine(BeginAfterDelay());
    }

    IEnumerator BeginAfterDelay() { yield return new WaitForSeconds(4f); started = true; }

    void Update()
    {
        if (!started || !player) return;
    }
}
