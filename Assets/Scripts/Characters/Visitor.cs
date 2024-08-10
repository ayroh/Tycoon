using System.Collections;
using System.Collections.Generic;
using Utilities.Enums;
using UnityEngine;
using Utilities.Constants;
using Pool;
using System;

public class Visitor : Character, IPoolable
{
    PoolObjectType IPoolable.poolObjectType => PoolObjectType.Visitor;
    public VisitorState state { get; private set; } = VisitorState.Idle;
    
    // Exhibition
    private Exhibition currentExhibition;


    private void Start()
    {
        GoToQueue();
    }

 

    public void SetState(VisitorState newState) 
    {

        switch (state)
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

        if (state == newState)
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
                Animate(VisitorAnimationState.Walking, Mathf.Lerp(0f, .5f, currentSpeed / Constants.visitorMoveSpeed));
                isMoving = true;
                break;

            case VisitorState.GoingToLine:
                Animate(VisitorAnimationState.Walking);
                isMoving = true;
                break;
        }

        state = newState;
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

        ClearActions();
        AddNextAction(() => { GetInEntryPath(currentExhibition); }, true);
        SetNextTarget();
    }

    public void GetInEntryPath(Exhibition exhibition)
    {
        if (exhibition.IsEntryLineFilled)
        {
            SetState(VisitorState.Patrol);
            return;
        }

        SetState(VisitorState.WaitingInLine);

        currentPath = exhibition.GetEntryPathUntilCurrentQueue();
        indexInPath = currentPath.Count;
        exhibition.AddVisitorToEntryQueue(this); 
        
        AddNextAction(() => {
            Rotate(currentPath[0].eulerAngles, false);
            Animate(VisitorAnimationState.Standing);
        }, true);
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

        if(state == VisitorState.GoingToLine || state == VisitorState.WaitingInLine || state == VisitorState.Visiting)
        {
            if (indexInPath < 0)
            {
                indexInPath = 0;
                isMoving = false;
                SetPatrolPoint(default);
                DoNextAction();
            }
            else
            {
                if(state == VisitorState.Visiting)
                {
                    SetPatrolPoint(new Vector3(currentPath[indexInPath].position.x + Extentions.Noise(.01f, 75),
                                                currentPath[indexInPath].position.y,
                                               currentPath[indexInPath].position.z + Extentions.Noise(.01f, 75)
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
        else if (state == VisitorState.Patrol)
        {
            SetPatrolPoint(Extentions.GetRandomPatrolPoint());
        }
    }

    public void GoToQueue()
    {
        OrderManager.instance.AddVisitor(this);
    }


    
    

    private void Animate(VisitorAnimationState newAnimationState, float animationSpeed = -1)
    {
        switch (newAnimationState)
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


    #region Pool

    public override void Initialize(Transform parent = null)
    {
        SetState(VisitorState.Idle);

    }

    public override void ResetObject(Transform parent = null)
    {
    }

    #endregion
}
