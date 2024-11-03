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


    private void Start()
    {
        GoToQueue();
    }

 

    public void SetState(VisitorState newState) 
    {

        switch (visitorState)
        {
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

        if (visitorState == newState)
            return;


        switch (newState)
        {
            case VisitorState.Patrol:
                SetCurrentSpeed(Constants.visitorMoveSpeed);
                GoToQueue();
                Animate(VisitorAnimationState.Walking);
                isMoving = true;
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

        visitorState = newState;
    }

    public void GetInWaitingPoint(Exhibition exhibition)
    {
        SetState(VisitorState.GoingToLine);

        currentExhibition = exhibition;
        currentPath = new List<Transform>()
        {
            exhibition.GetWaitingPoint()
        };

        indexInPath = currentPath.Count;

        ClearNextActions();
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

        if(visitorState == VisitorState.GoingToLine || visitorState == VisitorState.WaitingInLine || visitorState == VisitorState.Visiting)
        {
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
                    SetPatrolPoint(new Vector3(currentPath[indexInPath].position.x + Extentions.Noise(.005f, 75),
                                                currentPath[indexInPath].position.y,
                                               currentPath[indexInPath].position.z + Extentions.Noise(.005f, 75)
                    ));
                }
                else
                {
                    SetPatrolPoint(currentPath[indexInPath].position);
                }
            }
            return;
        }
        else
        {
            Debug.LogError("Visitor: GetNextInPath, no paths found!");
        }

        SetPatrolPoint(default);
    }


    protected override void FindTarget()
    {
        if (currentPath != null)
        {
            GetNextInPath();
        }
        else if (visitorState == VisitorState.Patrol)
        {
            SetPatrolPoint(Extentions.GetRandomPatrolPoint());
        }
    }

    public void GoToQueue()
    {
        OrderManager.instance.AddVisitor(this);
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
