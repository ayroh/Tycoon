using Cysharp.Threading.Tasks;
using Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utilities.Constants;
using Utilities.Enums;

public class Guide : Character, IPoolable
{
    PoolObjectType IPoolable.PoolObjectType => PoolObjectType.Guide;

    public GuideState state { get; private set; } = GuideState.Waiting;

    private Exhibition exhibition;


    public async void StartGuiding()
    {
        if(exhibition == null)
        {
            Debug.LogError("Guide: StartGuiding, There is no exhibition!");
            return;
        }

        indexInPath = currentPath.Count;
        AddNextAction(() => SetState(GuideState.Waiting), true);
        
        await UniTask.Delay(500);
        SetState(GuideState.Guiding);
    }

    protected override void FindTarget()
    {
        if (state == GuideState.Guiding)
        {
            GetNextInPath();
        }
    }

    public void GetNextInPath()
    {
        --indexInPath;

        if (indexInPath < 0)
        {
            indexInPath = 0;
            isMoving = false;
            SetPatrolPoint(default);
            DoNextAction();
        }
        else
        {
            SetPatrolPoint(currentPath[indexInPath].position);
        }
    }

    public void SetGuidingPath(List<Transform> newGuidingPath) => currentPath = newGuidingPath;

    private void SetState(GuideState newState)
    {
        state = newState;

        switch (newState)
        {
            case GuideState.Waiting:
                isMoving = false;
                Animate(GuideAnimationState.Standing);
                break;
            case GuideState.Guiding:
                isMoving = true;
                Animate(GuideAnimationState.Walking, Mathf.Lerp(0f, 1f, (currentSpeed / 2) / Constants.visitorMoveSpeed));
                break;
        }
    }

    private void Animate(GuideAnimationState newAnimationState, float animationSpeed = -1)
    {
        switch (newAnimationState)
        {
            case GuideAnimationState.Standing:
                animator.SetFloat("MoveBlend", 0f);
                break;
            case GuideAnimationState.Walking:
                animator.SetFloat("MoveBlend", (animationSpeed != -1) ? animationSpeed : .5f);
                break;
            case GuideAnimationState.Running:
                animator.SetFloat("MoveBlend", (animationSpeed != -1) ? animationSpeed : 1f);
                break;
        }
    }

    public void SetGuide(Exhibition newExhibition)
    {
        exhibition = newExhibition;
        Transform guidingPathParent = exhibition.GuidingPathParent;

        if (guidingPathParent.childCount < 2)
        {
            Debug.LogError("Guide: SetExhibition, Guiding parent has not enough child!");
            return;
        }

        currentPath = new();
        currentPath.Add(guidingPathParent.GetChild(guidingPathParent.childCount - 1));
        for (int i = 0;i < guidingPathParent.childCount;i++)
            currentPath.Add(guidingPathParent.GetChild(i));

        transform.position = currentPath[0].position;

        SetCurrentSpeed(exhibition.ExhibitionSpeed);
    }

    #region Pool

    public override void Initialize(Transform parent = null)
    {
        SetState(GuideState.Waiting);
    }

    public override void ResetObject(Transform parent = null)
    {
    }

    #endregion

}
