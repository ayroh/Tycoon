using Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Constants;

public class Character : MonoBehaviour, IPoolable
{
    [Header("References")]
    [SerializeField] protected Animator animator;

    protected float currentSpeed = Constants.visitorMoveSpeed;

    // Patrol
    protected Vector3 targetPosition = default;
    protected Vector3 targetVector = default;
    protected int currentMoveCount = 0, targetMoveCount = 0;
    protected bool isMoving = false;
    protected int indexInPath = 0, endIndexInPath = 0;
    protected List<Transform> currentPath;

    // Rotation
    protected IEnumerator rotationNumerator;

    // Command
    protected Queue<Action> nextActions = new();


    protected void FixedUpdate()
    {
        if (isMoving)
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

    public void SetNextTarget()
    {
        FindTarget();
        if (targetPosition == default)
            return;

        CalculateTarget();

        Rotate(targetPosition - transform.position, true);
        isMoving = true;
    }

    protected virtual void FindTarget()
    {
        throw new NotImplementedException();
    }
    private void CalculateTarget()
    {
        targetVector = (targetPosition - transform.position).normalized * currentSpeed;
        currentMoveCount = 0;
        targetMoveCount = Mathf.FloorToInt(Vector2.Distance(Extentions.Vector3ToVector2XZ(transform.position), Extentions.Vector3ToVector2XZ(targetPosition)) / currentSpeed);
    }

    public void SetCurrentSpeed(float newSpeed) => currentSpeed = newSpeed;

    public void SetPatrolPoint(Vector3 newPatrol)
    {
        targetPosition = newPatrol;
    }

    protected void Rotate(Vector3 newRotation, bool isDirection)
    {
        if (rotationNumerator != null)
            StopCoroutine(rotationNumerator);
        StartCoroutine(rotationNumerator = RotateCoroutine(newRotation, isDirection));
    }

    private IEnumerator RotateCoroutine(Vector3 newRotation, bool isDirection)
    {
        float timer = 0f;
        Quaternion startAngle = transform.rotation;
        Quaternion endAngle = Quaternion.identity;
        if (isDirection)
            endAngle = Quaternion.LookRotation(newRotation);
        else
            endAngle = Quaternion.Euler(newRotation);

        while (timer < Constants.visitorForwardRotationTime)
        {
            transform.rotation = Quaternion.Lerp(startAngle, endAngle, timer / Constants.visitorForwardRotationTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endAngle;
        rotationNumerator = null;
    }

    #region Command
    public void AddNextAction(Action newAction, bool clearActions = false)
    {
        if (clearActions) ClearNextActions();

        nextActions.Enqueue(newAction);
    }

    protected void ClearNextActions() => nextActions.Clear();

    protected bool DoNextAction()
    {
        if (nextActions.Count > 0)
        {
            nextActions.Dequeue()?.Invoke();
            return true;
        }
        else
            return false;
    }

    #endregion

    #region Pool

    public PoolObjectType PoolObjectType => throw new System.NotImplementedException();

    public virtual void Initialize(Transform parent = null)
    {
        throw new System.NotImplementedException();
    }

    public virtual void ResetObject(Transform parent = null)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}
