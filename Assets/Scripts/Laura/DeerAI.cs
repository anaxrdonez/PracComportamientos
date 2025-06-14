// DeerAI.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DeerAI : MonoBehaviour
{
    [Header("NavMesh Wander")]
    public float wanderRadius = 20f;
    public float walkSpeed = 2.5f;
    public float waitMin = 1f;
    public float waitMax = 2f;

    [Header("Flee")]
    public float fleeSpeedMultiplier = 2.5f;
    public float fleeDuration = 5f;

    [Header("Animation")]
    public Animator animator;
    public RuntimeAnimatorController defaultController;
    public RuntimeAnimatorController fleeController;

    enum State { Wander, Flee }
    State state = State.Wander;

    NavMeshAgent agent;
    Vector3 homePosition;
    Transform threat;
    Coroutine routine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position;
        agent.speed = walkSpeed;

        // Arrancamos con la animación por defecto
        if (animator != null && defaultController != null)
            animator.runtimeAnimatorController = defaultController;

        routine = StartCoroutine(WanderRoutine());
    }

    public void OnSensorEnter(DeerSensor.SensorType type, Transform other)
    {
        if (other.CompareTag("Tiger") && state != State.Flee)
        {
            threat = other;
            SwitchState(State.Flee);
        }
    }

    void SwitchState(State newState)
    {
        if (routine != null) StopCoroutine(routine);
        state = newState;

        // Cambiamos animación según el nuevo estado
        if (animator != null)
        {
            switch (state)
            {
                case State.Wander:
                    if (defaultController != null)
                        animator.runtimeAnimatorController = defaultController;
                    agent.speed = walkSpeed;
                    routine = StartCoroutine(WanderRoutine());
                    break;

                case State.Flee:
                    if (fleeController != null)
                        animator.runtimeAnimatorController = fleeController;
                    agent.speed = walkSpeed * fleeSpeedMultiplier;
                    routine = StartCoroutine(FleeRoutine());
                    break;
            }
        }
        else
        {
            // Si no hay animator, igual arrancamos la corutina
            agent.speed = (state == State.Wander ? walkSpeed : walkSpeed * fleeSpeedMultiplier);
            routine = StartCoroutine(state == State.Wander ? WanderRoutine() : FleeRoutine());
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

    IEnumerator FleeRoutine()
    {
        float endTime = Time.time + fleeDuration;
        while (Time.time < endTime && state == State.Flee)
        {
            Vector3 dir = (transform.position - threat.position).normalized;
            Vector3 raw = transform.position + dir * wanderRadius;
            Vector3 dest = RandomNavMeshPoint(raw, wanderRadius * 0.5f);
            agent.SetDestination(dest);
            while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                   && Time.time < endTime)
                yield return null;
        }
        if (state == State.Flee)
            SwitchState(State.Wander);
    }

    Vector3 RandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 rand = center + Random.insideUnitSphere * radius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(rand, out hit, radius, NavMesh.AllAreas))
            return hit.position;
        return center;
    }

    public void OnSensorExit(DeerSensor.SensorType type, Transform other)
    {
        // opcional: aquí podrías resetear algo o ignorar
    }
}
