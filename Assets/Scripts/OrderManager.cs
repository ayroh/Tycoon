using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Enums;

public class OrderManager : Singleton<OrderManager>
{
    [Header("References")]
    [SerializeField] private List<Exhibition> exhibitions;
    private List<IEnumerator> exhibitionNumerators = new();

    private Queue<Visitor> patrollingVisitors = new();


    public void AddVisitor(Visitor visitor)
    {
        patrollingVisitors.Enqueue(visitor);
    }

    private void Start()
    {
        IEnumerator tempNumerator;

        for(int i = 0;i < exhibitions.Count;i++)
        {
            StartCoroutine(tempNumerator = StartExhibitionOrder(exhibitions[i], 1f));
            exhibitionNumerators.Add(tempNumerator);
            exhibitions[i].SetState(ExhibitionState.Waiting);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)) {
            foreach(var exhibition  in exhibitions)
            {
                exhibition.StartExhibition();
            }
            StopAllCoroutines();
        }
    }

    private IEnumerator StartExhibitionOrder(Exhibition exhibition, float visitorCallTime)
    {
        WaitForSeconds callTimeDelay = new WaitForSeconds(visitorCallTime);

        while(enabled)
        {
            if(patrollingVisitors.Count != 0 && !exhibition.IsEntryLineFilled)
            {
                Visitor visitor = patrollingVisitors.Dequeue();
                visitor.GetInEntryPath(exhibition);
                exhibition.AddVisitorToEntryQueue(visitor);
            }
            yield return callTimeDelay;
        }
        
    }

}
