using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utilities.Constants;
using Utilities.Enums;

public class OrderManager : Singleton<OrderManager>
{
    [Header("References")]
    [SerializeField] private List<Exhibition> exhibitions;
    private List<IEnumerator> exhibitionNumerators = new();

    private Queue<Visitor> patrollingVisitors = new();

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
            StartCoroutine(tempNumerator = StartExhibitionOrder(exhibitions[i], 1f));
            exhibitionNumerators.Add(tempNumerator);
            exhibitions[i].SetState(ExhibitionState.Waiting);
        }

        Constants.fixedUpdateFrameInterval = Time.fixedDeltaTime;
        print("FixedDeltaTime: " +(Constants.fixedUpdateFrameInterval = Time.fixedDeltaTime));
    }

    //private void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.S)) {
    //        foreach(var exhibition  in exhibitions)
    //        {
    //            exhibition.StartExhibition();
    //        }
    //    }
    //}

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

}
