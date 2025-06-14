// TigerAI.cs
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class TigerAI : MonoBehaviour
{
    public enum State { Wander, Chase, Search }
    State state = State.Wander;

    [Header("NavMesh Wander")]
    public float wanderRadius = 25f;
    public float walkSpeed = 1.5f;
    public float waitMin = 2f;
    public float waitMax = 4f;

    [Header("Chase")]
    public float chaseSpeed = 5f;

    [Header("Search")]
    public float searchDuration = 3f;
    public float rotateSpeed = 150f;

    [Header("Animation Controllers")]
    public Animator animator;
    public RuntimeAnimatorController defaultController;
    public RuntimeAnimatorController chaseController;

    [Header("State Icon")]
    public Image stateIcon;
    public Sprite wanderIcon;
    public Sprite chaseIcon;
    public Sprite searchIcon;

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
        UpdateStateIcon();

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
        preyTarget = target;

        // cambiar controlador
        if (animator != null)
        {
            switch (state)
            {
                case State.Wander:
                case State.Search:
                    if (defaultController != null) animator.runtimeAnimatorController = defaultController;
                    break;
                case State.Chase:
                    if (chaseController != null) animator.runtimeAnimatorController = chaseController;
                    break;
            }
        }
        UpdateStateIcon();

        // iniciar rutina
        switch (state)
        {
            case State.Wander:
                agent.speed = walkSpeed;
                routine = StartCoroutine(WanderRoutine());
                break;
            case State.Chase:
                agent.speed = chaseSpeed;
                routine = StartCoroutine(ChaseRoutine());
                break;
            case State.Search:
                agent.speed = walkSpeed;
                routine = StartCoroutine(SearchRoutine());
                break;
        }
    }

    void UpdateStateIcon()
    {
        if (stateIcon == null) return;
        switch (state)
        {
            case State.Wander: stateIcon.sprite = wanderIcon; break;
            case State.Chase: stateIcon.sprite = chaseIcon; break;
            case State.Search: stateIcon.sprite = searchIcon; break;
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

    IEnumerator ChaseRoutine()
    {
        while (state == State.Chase && preyTarget != null && preyTarget.gameObject.activeInHierarchy)
        {
            agent.SetDestination(preyTarget.position);
            yield return null;
        }
        if (state == State.Chase)
            SwitchState(State.Search, null);
    }

    IEnumerator SearchRoutine()
    {
        float endTime = Time.time + searchDuration;
        float total = 180f;
        float perSec = total / searchDuration;
        float rotated = 0f;

        while (Time.time < endTime && state == State.Search)
        {
            float step = perSec * Time.deltaTime;
            transform.Rotate(0f, step, 0f);
            rotated += step;
            if (rotated >= total) break;
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
