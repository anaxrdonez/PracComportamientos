// DeerAI.cs
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class DeerAI : MonoBehaviour
{
    public enum State { Wander, Alert, Flee }
    State state = State.Wander;

    [Header("NavMesh Wander")]
    public float wanderRadius = 20f;
    public float walkSpeed = 2.5f;
    public float waitMin = 1f;
    public float waitMax = 2f;

    [Header("Alert")]
    public float alertDuration = 3f;
    public float alertRotateSpeed = 120f;

    [Header("Flee")]
    public float fleeSpeedMultiplier = 2.5f;
    public float fleeDuration = 5f;

    [Header("Animation Controllers")]
    public Animator animator;
    public RuntimeAnimatorController defaultController;
    public RuntimeAnimatorController alertController;
    public RuntimeAnimatorController fleeController;

    [Header("State Icon")]
    public Image stateIcon;
    public Sprite wanderIcon;
    public Sprite alertIcon;
    public Sprite fleeIcon;

    NavMeshAgent agent;
    Vector3 homePosition;
    Transform threat;
    Coroutine routine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position;
        agent.speed = walkSpeed;

        // Inicializar animador e icono
        if (animator != null && defaultController != null)
            animator.runtimeAnimatorController = defaultController;
        UpdateStateIcon();

        routine = StartCoroutine(WanderRoutine());
    }

    public void OnSensorEnter(DeerSensor.SensorType type, Transform other)
    {
        if (!other.CompareTag("Tiger")) return;

        if (type == DeerSensor.SensorType.Ear && state == State.Wander)
            SwitchState(State.Alert, other);
        else if ((type == DeerSensor.SensorType.LeftEye || type == DeerSensor.SensorType.RightEye)
                 && state != State.Flee)
            SwitchState(State.Flee, other);
    }

    void SwitchState(State newState, Transform newThreat)
    {
        if (routine != null) StopCoroutine(routine);
        state = newState;
        threat = newThreat;
        // cambiar controlador
        if (animator != null)
        {
            switch (state)
            {
                case State.Wander:
                    if (defaultController != null) animator.runtimeAnimatorController = defaultController;
                    break;
                case State.Alert:
                    if (alertController != null) animator.runtimeAnimatorController = alertController;
                    break;
                case State.Flee:
                    if (fleeController != null) animator.runtimeAnimatorController = fleeController;
                    break;
            }
        }
        UpdateStateIcon();

        // iniciar rutina
        switch (state)
        {
            case State.Wander:
                agent.speed = walkSpeed;
                agent.isStopped = false;
                routine = StartCoroutine(WanderRoutine());
                break;
            case State.Alert:
                agent.isStopped = true;
                routine = StartCoroutine(AlertRoutine());
                break;
            case State.Flee:
                agent.isStopped = false;
                agent.speed = walkSpeed * fleeSpeedMultiplier;
                routine = StartCoroutine(FleeRoutine());
                break;
        }
    }

    void UpdateStateIcon()
    {
        if (stateIcon == null) return;
        switch (state)
        {
            case State.Wander: stateIcon.sprite = wanderIcon; break;
            case State.Alert: stateIcon.sprite = alertIcon; break;
            case State.Flee: stateIcon.sprite = fleeIcon; break;
        }
        stateIcon.enabled = true;
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

    IEnumerator AlertRoutine()
    {
        float endTime = Time.time + alertDuration;
        while (Time.time < endTime && state == State.Alert)
        {
            transform.Rotate(0f, alertRotateSpeed * Time.deltaTime, 0f);
            yield return null;
        }
        if (state == State.Alert)
            SwitchState(State.Wander, null);
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

    public void OnSensorExit(DeerSensor.SensorType type, Transform other)
    {
        // opcional: aquí podrías resetear algo o ignorar
    }
}
