using System.Collections;
using System.Collections.Generic;
using Utilities.Enums;
using UnityEngine;
using Utilities.Constants;
using Pool;
using System;

using Random = UnityEngine.Random;
using System.Runtime.CompilerServices;

public class Visitor : MonoBehaviour, IPoolable
{
    [Header("References")]
    [SerializeField] private Animator animator;

    //[Header("Test Variables")]

    PoolObjectType IPoolable.poolObjectType => PoolObjectType.Visitor;
    public VisitorState state { get; private set; } = VisitorState.Idle;

    public bool isStateMoveable => state == VisitorState.Patrol || state == VisitorState.GoingToLine || state == VisitorState.Visiting;

    private float currentSpeed = Constants.visitorMoveSpeed;


    // Patrol
    private Queue<Vector3> patrolPositions = new Queue<Vector3>();
    private Vector3 currentPatrolPosition = default;
    private Vector3 targetVector = default;
    private int currentMoveCount = 0, targetMoveCount = 0;

    // Rotation
    private IEnumerator rotationNumerator;

    private Exhibition currentExhibition;

    private Action nextAction;

    private void Start()
    {
        GoToQueue();
    }

    void Update()
    {
        if(isStateMoveable)
        {
            if (currentMoveCount >= targetMoveCount)
            {
                SetNextCurrentPatrol();
                return;
            }

            currentMoveCount++;
            transform.position += targetVector;
        }
    }


    public void SetState(VisitorState newState) 
    {
        if (state == newState)
            return;


        switch (newState)
        {
            case VisitorState.Patrol:
                SetCurrentSpeed(Constants.visitorMoveSpeed);
                animator.SetFloat("MoveBlend", .5f);
                break;

            case VisitorState.Idle:
                animator.SetFloat("MoveBlend", 0f);
                currentPatrolPosition = Vector3.zero;
                targetVector = Vector3.zero;
                break;

            case VisitorState.WaitingInLine:
                animator.SetFloat("MoveBlend", 0f);
                break;

            case VisitorState.Visiting:
                animator.SetFloat("MoveBlend", Mathf.Lerp(0f, .5f, currentSpeed / Constants.visitorMoveSpeed));
                break;

            case VisitorState.GoingToLine:
                animator.SetFloat("MoveBlend", .5f);
                break;
        }

        state = newState;
    }

    public void GetInLine(Exhibition exhibition)
    {
        List<Transform> entryPathList = exhibition.GetEntryPath();
        
        Queue<Vector3> visitorEntryPath = new();
        for(int i = entryPathList.Count - 1;i >= exhibition.entryQueueCount;--i)
            visitorEntryPath.Enqueue(entryPathList[i].position);
        SetPatrolPoints(visitorEntryPath);

        SetState(VisitorState.GoingToLine);
        currentExhibition = exhibition;

        nextAction = () => { SetState(VisitorState.WaitingInLine); };
    }

    public void GetNextLine()
    {
        List<Transform> entryPathList = currentExhibition.GetEntryPath();

        Queue<Vector3> visitorEntryPath = new();
        for (int i = entryPathList.Count - 1;i >= currentExhibition.entryQueueCount;--i)
            visitorEntryPath.Enqueue(entryPathList[i].position);
        SetPatrolPoints(visitorEntryPath);

        SetState(VisitorState.GoingToLine);

        nextAction = () => { SetState(VisitorState.WaitingInLine); };
    }

    public void GoToQueue()
    {
        OrderManager.instance.AddVisitor(this);
    }

    public void SetCurrentSpeed(float newSpeed) => currentSpeed = newSpeed;

    private IEnumerator Rotate(Vector3 targetPosition)
    {
        float timer = 0f;
        Quaternion startAngle = transform.rotation;
        Quaternion endAngle = Quaternion.LookRotation(targetPosition - transform.position);

        while (timer < Constants.visitorForwardRotationTime)
        {
            transform.rotation = Quaternion.Lerp(startAngle, endAngle, timer / Constants.visitorForwardRotationTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endAngle;
        rotationNumerator = null;
    }

    public void SetNextAction(Action newAction) => nextAction = newAction;

    #region Patrol

    public void SetNextCurrentPatrol()
    {
        if (patrolPositions.Count == 0)
        {
            if(nextAction != null)
            {
                nextAction?.Invoke();
                return;
            }

            AddPatrolPoint(Extentions.GetRandomPatrolPoint());
            SetState(VisitorState.Patrol);
        }
        currentPatrolPosition = patrolPositions.Dequeue();
        CalculateTarget();

        if (rotationNumerator != null)
            StopCoroutine(rotationNumerator);
        StartCoroutine(rotationNumerator = Rotate(currentPatrolPosition));

        //SetState(VisitorState.Patrol);
    }

    private void CalculateTarget()
    {
        targetVector = (currentPatrolPosition - transform.position).normalized * currentSpeed;
        currentMoveCount = 0;
        targetMoveCount = Mathf.FloorToInt(Vector2.Distance(Extentions.Vector3ToVector2XZ(transform.position), Extentions.Vector3ToVector2XZ(currentPatrolPosition)) / currentSpeed);
    }

    public void AddPatrolPoint(Vector3 newPatrol, bool patrolIfIdle = true)
    {
        patrolPositions.Enqueue(newPatrol);

        if (patrolIfIdle && targetVector == default)
            SetNextCurrentPatrol();
    }

    public void SetPatrolPoints(Queue<Vector3> newPatrolPoints, bool startPatrol = true)
    {
        if(newPatrolPoints == null)
        {
            Debug.LogError("Visitor: SetPatrolPoints, null patrol points!");
            return;
        }

        patrolPositions.Clear();
        patrolPositions = newPatrolPoints;

        if (startPatrol)
            SetNextCurrentPatrol();
    }


    #endregion

    

    #region Pool

    public void Initialize(Transform parent = null)
    {
        SetState(VisitorState.Idle);

    }

    public void ResetObject(Transform parent = null)
    {
    }

    #endregion
}
