using System.Collections;
using System.Collections.Generic;
using Utilities.Enums;
using UnityEngine;
using Utilities.Constants;
using Pool;
using System;

public class Visitor : Character, IPoolable
{
    PoolObjectType IPoolable.PoolObjectType => PoolObjectType.Visitor;
    public VisitorState visitorState { get; private set; } = VisitorState.Idle;
    public VisitorAnimationState visitorAnimationState { get; private set; } = VisitorAnimationState.Standing;

    // Exhibition
    private Exhibition currentExhibition;

    private List<Vector3> patrolPath = new();


    private void Start()
    {
        SetState(VisitorState.Patrol);
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
    //        {
    //            visitorState = VisitorState.Patrol;
    //            SetCurrentSpeed(Constants.visitorMoveSpeed);
    //            Animate(VisitorAnimationState.Walking);
    //            isMoving = true;
    //            OrderManager.instance.SetPath(patrolPath, transform.position, Extentions.Vector3ZeroY(hit.point));
    //            indexInPath = patrolPath.Count;
    //            endIndexInPath = 0;
    //            SetNextTarget();
    //        }
    //    }
    //}

    public void SetState(VisitorState newState) 
    {
        if (visitorState == newState)
            return;

        switch (visitorState)
        {
            case VisitorState.Patrol:
                patrolPath.Clear();
                break;

            case VisitorState.Visiting:
                currentPath = null;
                break;

            case VisitorState.WaitingInLine:
                currentPath = null;
                break;

            case VisitorState.GoingToLine:
                currentPath = null;
                break;
        }

        visitorState = newState;

        switch (newState)
        {
            case VisitorState.Patrol:
                SetCurrentSpeed(Constants.visitorMoveSpeed);
                GoToQueue();
                Animate(VisitorAnimationState.Walking);
                isMoving = true;
                SetPatrolPath();
                SetNextTarget();
                break;

            case VisitorState.Idle:
                Animate(VisitorAnimationState.Standing);
                targetPosition = Vector3.zero;
                targetVector = Vector3.zero;
                isMoving = false;
                break;

            case VisitorState.WaitingInLine:
                //Animate(VisitorAnimationState.Standing);
                break;

            case VisitorState.Visiting:
                Animate(VisitorAnimationState.Walking, Mathf.Lerp(0f, 1f, (currentSpeed / 2) / Constants.visitorMoveSpeed));
                isMoving = true;
                break;

            case VisitorState.GoingToLine:
                Animate(VisitorAnimationState.Walking);
                isMoving = true;
                break;
        }

    }

    public void GetInWaitingPoint(Exhibition exhibition)
    {
        SetState(VisitorState.GoingToLine);

        currentExhibition = exhibition;
        OrderManager.instance.SetPath(patrolPath, transform.position, exhibition.GetWaitingPoint().position);
        indexInPath = patrolPath.Count;

        AddNextAction(() => { GetInEntryPath(currentExhibition); }, true);
        SetNextTarget();
    }

    public void GetInEntryPath(Exhibition exhibition)
    {
        if (exhibition.IsEntryQueueFilled)
        {
            SetState(VisitorState.Patrol);
            return;
        }

        SetState(VisitorState.WaitingInLine);

        currentPath = exhibition.entryPath;
        indexInPath = currentPath.Count - 1;

        GetEndOfTheEntryPath(null);
        exhibition.AddVisitorToEntryQueue(this); 
    }


    public void GetEndOfTheEntryPath(List<Transform> insidePath)
    {
        if(visitorState != VisitorState.WaitingInLine)
        {
            Debug.LogError("Visitor: GetEndOfTheEntryPath, Requesting end of the entry path but visitor not inside the waiting line!");
            return;
        }

        ClearNextActions();

        if (insidePath != null)
        {
            endIndexInPath = 0;
            Animate(VisitorAnimationState.Walking);
            AddNextAction(() => GetInInsidePath(insidePath, currentExhibition.ExhibitionSpeed));
        }
        else
        {
            endIndexInPath = currentExhibition.FirstEmptyEntryPathIndex;
            AddNextAction(() => {
                Rotate(currentPath[endIndexInPath].eulerAngles, false);
                Animate(VisitorAnimationState.Standing);
            });
        }

        indexInPath++;
        SetNextTarget();
    }

    public void GetInInsidePath(List<Transform> insidePath, float exhibitionSpeed)
    {
        SetState(VisitorState.Visiting);

        currentPath = insidePath;
        indexInPath = currentPath.Count;

        SetCurrentSpeed(exhibitionSpeed);
        AddNextAction(() => { SetState(VisitorState.Patrol);}, true);
        SetNextTarget();
    }

    public void GetNextInPath()
    {
        --indexInPath;
        if(visitorState == VisitorState.WaitingInLine || visitorState == VisitorState.Visiting)
        {
            if(currentPath == null)
            {
                Debug.LogError("Visitor: GetNextInPath, currentPath is empty!");
                return;
            }

            if (indexInPath < endIndexInPath)
            {
                indexInPath = 0;
                isMoving = false;
                SetPatrolPoint(default);
                DoNextAction();
            }
            else
            {
                if(visitorState == VisitorState.Visiting)
                {
                    SetPatrolPoint(new Vector3(currentPath[indexInPath].position.x + Extentions.Noise(.005f, 75), currentPath[indexInPath].position.y,currentPath[indexInPath].position.z + Extentions.Noise(.005f, 75)                    ));
                }
                else
                {
                    SetPatrolPoint(currentPath[indexInPath].position);
                }
            }
        }
        else if (visitorState == VisitorState.Patrol || visitorState == VisitorState.GoingToLine)
        {
            if(patrolPath.Count == 0)
            {
                Debug.LogError("Visitor: GetNextInPath, patrolPath is empty!");
                return;
            }

            if (indexInPath < endIndexInPath)
            {
                indexInPath = 0;
                isMoving = false;
                DoNextAction();
            }
            else
            {
                SetPatrolPoint(patrolPath[indexInPath]);
            }
        }
        else
        {
            Debug.LogError("Visitor: GetNextInPath, no paths found!");
            indexInPath = 0;
            SetPatrolPoint(default);
        }
    }


    protected override void FindTarget()
    {
        if (visitorState == VisitorState.Idle)
            return;

        GetNextInPath();
    }

    public void GoToQueue()
    {
        OrderManager.instance.AddVisitor(this);
    }

    private void SetPatrolPath()
    {
        OrderManager.instance.SetPatrolPath(patrolPath, transform.position);
        indexInPath = patrolPath.Count;
        endIndexInPath = 0;
        AddNextAction(() => SetPatrolPath(), true);
    }



    #region Animation

    private void Animate(VisitorAnimationState newAnimationState, float animationSpeed = -1)
    {
        visitorAnimationState = newAnimationState;
        switch (visitorAnimationState)
        {
            case VisitorAnimationState.Standing:
                animator.SetFloat("MoveBlend", 0f);
                break;
            case VisitorAnimationState.Walking:
                animator.SetFloat("MoveBlend", (animationSpeed != -1) ? animationSpeed : .5f);
                break;
            case VisitorAnimationState.Running:
                animator.SetFloat("MoveBlend", (animationSpeed != -1) ? animationSpeed : 1f);
                break;
        }

    }

    #endregion

    #region Pool

    public override void Initialize(Transform parent = null)
    {
        SetState(VisitorState.Idle);
        transform.SetParent(parent);
    }

    public override void ResetObject(Transform parent = null)
    {
    }

    #endregion
}
