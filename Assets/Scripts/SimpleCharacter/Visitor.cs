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

    //public bool isStateMoveable => state == VisitorState.Patrol || state == VisitorState.GoingToLine || state == VisitorState.Visiting || state == VisitorState.WaitingInLine;

    private float currentSpeed = Constants.visitorMoveSpeed;


    // Patrol
    private Vector3 currentPatrolPosition = default;
    private Vector3 targetVector = default;
    private int currentMoveCount = 0, targetMoveCount = 0;
    private bool isMoving = false;

    // Rotation
    private IEnumerator rotationNumerator;

    // Exhibition
    private List<Transform> exhibitionEntryPath;
    private List<Transform> exhibitionInsidePath;
    private int indexInExhibitionPath = 0;

    private Exhibition currentExhibition;

    private Action nextAction;

    private void Start()
    {
        GoToQueue();
    }

    void Update()
    {
        if(isMoving)
        {
            if (currentMoveCount >= targetMoveCount)
            {
                SetNextTarget();
                return;
            }

            currentMoveCount++;
            transform.position += targetVector;
        }
    }


    public void SetState(VisitorState newState) 
    {

        switch (state)
        {
            case VisitorState.Visiting:
                exhibitionInsidePath = null;
                break;

            case VisitorState.WaitingInLine:
                exhibitionEntryPath = null;
                break;
        }

        if (state == newState)
            return;


        switch (newState)
        {
            case VisitorState.Patrol:
                SetCurrentSpeed(Constants.visitorMoveSpeed);
                animator.SetFloat("MoveBlend", .5f);
                isMoving = true;
                SetNextTarget();
                break;

            case VisitorState.Idle:
                animator.SetFloat("MoveBlend", 0f);
                currentPatrolPosition = Vector3.zero;
                targetVector = Vector3.zero;
                isMoving = false;
                break;

            case VisitorState.WaitingInLine:
                //animator.SetFloat("MoveBlend", 0f);
                break;

            case VisitorState.Visiting:
                animator.SetFloat("MoveBlend", Mathf.Lerp(0f, .5f, currentSpeed / Constants.visitorMoveSpeed));
                isMoving = true;
                break;

            case VisitorState.GoingToLine:
                animator.SetFloat("MoveBlend", .5f);
                isMoving = true;
                break;
        }

        state = newState;
    }

    public void GetInEntryPath(Exhibition exhibition)
    {
        exhibitionEntryPath = exhibition.GetEntryLine();
        indexInExhibitionPath = exhibitionEntryPath.Count;
        currentExhibition = exhibition;

        SetState(VisitorState.GoingToLine);
        SetNextTarget();
        SetNextAction(() => { SetState(VisitorState.WaitingInLine); });
    }

    public void GetInInsidePath(List<Transform> insidePath, float exhibitionSpeed)
    {
        exhibitionEntryPath = null;

        exhibitionInsidePath = insidePath;
        indexInExhibitionPath = exhibitionInsidePath.Count;

        SetCurrentSpeed(exhibitionSpeed);
        SetState(VisitorState.Visiting);
        SetNextTarget();

        SetNextAction(() => {
            GoToQueue();
            SetNextAction(null);
            SetState(VisitorState.Patrol);
        });

    }

    public bool GetNextInPath()
    {
        --indexInExhibitionPath;

        if(exhibitionEntryPath != null)
        {
            if (indexInExhibitionPath < 0 || currentExhibition.GetIfEntryLineIndexIsOccupied(indexInExhibitionPath))
            {
                indexInExhibitionPath = 0;
                animator.SetFloat("MoveBlend", 0f);
                isMoving = false;

                if (nextAction != null)
                {
                    nextAction?.Invoke();
                    nextAction = null;
                }

                return false;
            }

            currentExhibition.FillNextLine(indexInExhibitionPath, (currentExhibition.exhibitionMaxEntryVisitorCount == indexInExhibitionPath + 1) ? (indexInExhibitionPath) : (indexInExhibitionPath + 1));
            currentPatrolPosition = exhibitionEntryPath[indexInExhibitionPath].position;

            return true;
        }
        else if(exhibitionInsidePath != null)
        {
            if (indexInExhibitionPath < 0)
            {
                indexInExhibitionPath = 0;
                animator.SetFloat("MoveBlend", 0f);
                isMoving = false;

                if (nextAction != null)
                {
                    nextAction?.Invoke();
                    nextAction = null;
                }

                return false;
            }

            currentPatrolPosition = exhibitionInsidePath[indexInExhibitionPath].position;

            return true;
        }
        else
        {
            Debug.LogError("Visitor: GetNextInPath, no paths found!");
            return false;
        }

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

    public void SetNextTarget()
    {
        if (exhibitionEntryPath != null || exhibitionInsidePath != null)
        {
            if (!GetNextInPath())
                return;
        }

        if(state == VisitorState.Patrol)
        {
            SetPatrolPoint(Extentions.GetRandomPatrolPoint());
        }
        //else
        //{
        //    if (nextAction != null)
        //    {
        //        isMoving = false;
        //        nextAction?.Invoke();
        //        return;
        //    }

        //    SetState(VisitorState.Patrol);
        //}
        CalculateTarget();

        if (rotationNumerator != null)
            StopCoroutine(rotationNumerator);
        StartCoroutine(rotationNumerator = Rotate(currentPatrolPosition));

        isMoving = true;
    }

    private void CalculateTarget()
    {
        targetVector = (currentPatrolPosition - transform.position).normalized * currentSpeed;
        currentMoveCount = 0;
        targetMoveCount = Mathf.FloorToInt(Vector2.Distance(Extentions.Vector3ToVector2XZ(transform.position), Extentions.Vector3ToVector2XZ(currentPatrolPosition)) / currentSpeed);
    }

    public void SetPatrolPoint(Vector3 newPatrol)
    {
        currentPatrolPosition = newPatrol;
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
