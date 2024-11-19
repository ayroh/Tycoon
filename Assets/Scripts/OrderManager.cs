using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Utilities.Constants;
using Utilities.Enums;

public class OrderManager : Singleton<OrderManager>
{
    [Header("References")]
    [SerializeField] private List<Exhibition> exhibitions;
    private List<IEnumerator> exhibitionNumerators = new();

    private Queue<Visitor> patrollingVisitors = new();
    private Navigator navigator;

    private Vector3[] randomPatrolPoints = new Vector3[20];

    public void AddVisitor(Visitor visitor)
    {
        if(patrollingVisitors.Contains(visitor))
        {
            Debug.LogError("OrderManager: AddVisitor, Queue contains the visitor!");
            return;
        }

        patrollingVisitors.Enqueue(visitor);
    }

    private void Start()
    {
        IEnumerator tempNumerator;

        Application.targetFrameRate = 120;

        for(int i = 0;i < exhibitions.Count;i++)
        {
            if (!exhibitions[i].gameObject.activeInHierarchy)
                continue;

            StartCoroutine(tempNumerator = StartExhibitionOrder(exhibitions[i], 2f));
            exhibitionNumerators.Add(tempNumerator);
            exhibitions[i].SetState(ExhibitionState.Waiting);
            exhibitions[i].TryStartingExhibition();
        }

        navigator = new();
        for(int i = 0;i < randomPatrolPoints.Length;i++)
        {
            do
            {
                randomPatrolPoints[i] = Extentions.GetRandomPatrolPoint();
            }
            while (exhibitions.Any(exhibition => exhibition.IsInBounds(randomPatrolPoints[i])));
        }
    }

    private IEnumerator StartExhibitionOrder(Exhibition exhibition, float visitorCallTime)
    {
        WaitForSeconds callTimeDelay = new WaitForSeconds(visitorCallTime);

        while(enabled)
        {
            if(patrollingVisitors.Count != 0)
            {
                Visitor visitor = patrollingVisitors.Dequeue();
                visitor.GetInWaitingPoint(exhibition);
            }
            yield return callTimeDelay;
        }
        
    }

    public void SetPatrolPath(List<Vector3> patrolPath, Vector3 startPoint)
    {
        navigator.SetPatrolPath(patrolPath, startPoint, randomPatrolPoints[Random.Range(0, randomPatrolPoints.Length)]);
    }

    public void SetPath(List<Vector3> path, Vector3 startPoint, Vector3 targetPoint)
    {
        navigator.SetPatrolPath(path, startPoint, targetPoint);
    }


}
