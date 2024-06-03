using System.Collections;
using System.Collections.Generic;
using Utilities.Enums;
using UnityEngine;
using UnityEngine.Rendering;
using System.Text;

public class SimpleCharacter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Test Variables")]
    [SerializeField] private float speed = 2;


    public SimpleCharacterState state { get; private set; } = SimpleCharacterState.Idle;

    // Patrol
    private Queue<Vector3> patrolPositions = new Queue<Vector3>();
    private Vector3 currentPatrolPosition = default;
    private Vector3 targetVector = default;

    private int currentMoveCount = 0, targetMoveCount = 0;

    void Update()
    {
        if(state == SimpleCharacterState.Patrol)
        {
            if (currentMoveCount >= targetMoveCount)
            {
                SetNextCurrentPatrol();
                return;
            }

            currentMoveCount++;
            transform.position += targetVector;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddPatrolPoint(new Vector3( Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f)));
        }
    }


    private void SetState(SimpleCharacterState newState) 
    {
        if (state == newState)
            return;

        state = newState;

        switch (newState)
        {
            case SimpleCharacterState.Patrol:
                animator.SetFloat("MoveBlend", .5f);
                break;

            case SimpleCharacterState.Idle:
                animator.SetFloat("MoveBlend", 0f);
                currentPatrolPosition = Vector3.zero;
                targetVector = Vector3.zero;
                break;

            case SimpleCharacterState.WaitingInLine:
                animator.SetFloat("MoveBlend", 0f);
                break;
        }

    }

    #region Patrol

    private void SetNextCurrentPatrol()
    {
        if (patrolPositions.Count == 0)
        {
            SetState(SimpleCharacterState.Idle);
            return;
        }
        currentPatrolPosition = patrolPositions.Dequeue();
        targetVector = (currentPatrolPosition - transform.position).normalized * speed;

        currentMoveCount = 0;
        targetMoveCount = Mathf.FloorToInt(Vector2.Distance(Extentions.Vector3ToVector2XZ(transform.position), Extentions.Vector3ToVector2XZ(currentPatrolPosition)) / targetVector.magnitude);

        transform.LookAt(currentPatrolPosition, Vector3.up);

        SetState(SimpleCharacterState.Patrol);
    }


    private void AddPatrolPoint(Vector3 newPatrol, bool patrolIfIdle = true)
    {
        patrolPositions.Enqueue(newPatrol);

        if (patrolIfIdle && targetVector == default)
            SetNextCurrentPatrol();
    }

    #endregion
}
