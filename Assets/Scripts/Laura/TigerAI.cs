// TigerAI.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TigerAI : MonoBehaviour
{
    [Header("NavMesh Wander")]
    public float wanderRadius = 25f;
    public float walkSpeed = 1.5f;
    public float waitMin = 2f;
    public float waitMax = 4f;

    [Header("Chase")]
    public float chaseSpeed = 5f;
    public float chaseDuration = 7f;

    [Header("Search")]
    public float searchDuration = 3f;
    public float rotateSpeed = 150f;

    [Header("Animation")]
    public Animator animator;
    public RuntimeAnimatorController defaultController;
    public RuntimeAnimatorController chaseController;

    enum State { Wander, Chase, Search }
    State state = State.Wander;

    NavMeshAgent agent;
    Vector3 homePosition;
    Transform preyTarget;
    Coroutine routine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position;
        agent.speed = walkSpeed;

        if (animator != null && defaultController != null)
            animator.runtimeAnimatorController = defaultController;

        routine = StartCoroutine(WanderRoutine());
    }

    public void OnSeePrey(Transform prey)
    {
        if (state != State.Chase)
            SwitchState(State.Chase, prey);
    }

    public void OnLosePrey(Transform prey)
    {
        if (state == State.Chase && prey == preyTarget)
            SwitchState(State.Search, null);
    }

    void SwitchState(State newState, Transform target)
    {
        if (routine != null) StopCoroutine(routine);
        state = newState;

        // Animación y velocidad
        if (animator != null)
        {
            switch (state)
            {
                case State.Wander:
                case State.Search:
                    if (defaultController != null)
                        animator.runtimeAnimatorController = defaultController;
                    agent.speed = walkSpeed;
                    break;
                case State.Chase:
                    if (chaseController != null)
                        animator.runtimeAnimatorController = chaseController;
                    agent.speed = chaseSpeed;
                    preyTarget = target;
                    break;
            }
        }
        else if (state == State.Chase) preyTarget = target;

        // Arranca la rutina correspondiente
        switch (state)
        {
            case State.Wander:
                routine = StartCoroutine(WanderRoutine());
                break;
            case State.Chase:
                routine = StartCoroutine(ChaseRoutine());
                break;
            case State.Search:
                routine = StartCoroutine(SearchRoutine());
                break;
        }
    }

    IEnumerator WanderRoutine()
    {
        while (state == State.Wander)
        {
            Vector3 dest = RandomNavMeshPoint(homePosition, wanderRadius);
            agent.SetDestination(dest);
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                yield return null;
            yield return new WaitForSeconds(Random.Range(waitMin, waitMax));
        }
    }

    IEnumerator ChaseRoutine()
    {
        float endTime = Time.time + chaseDuration;
        while (Time.time < endTime && state == State.Chase)
        {
            if (preyTarget == null) break;
            agent.SetDestination(preyTarget.position);
            yield return null;
        }
        if (state == State.Chase)
            SwitchState(State.Search, null);
    }

    IEnumerator SearchRoutine()
    {
        float endTime = Time.time + searchDuration;
        float total = 180f, turned = 0f, perSec = total / searchDuration;
        while (Time.time < endTime && state == State.Search)
        {
            float step = perSec * Time.deltaTime;
            transform.Rotate(0f, step, 0f);
            turned += step;
            if (turned >= total) break;
            yield return null;
        }
        if (state == State.Search)
            SwitchState(State.Wander, null);
    }

    Vector3 RandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 rand = center + Random.insideUnitSphere * radius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(rand, out hit, radius, NavMesh.AllAreas))
            return hit.position;
        return center;
    }
}
