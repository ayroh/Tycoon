using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utilities.Constants;
using Utilities.Enums;

public class Exhibition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform entryPathParent;
    [SerializeField] private Transform insidePathParent;

    private readonly List<Transform> entryPath = new();
    private readonly List<Transform> insidePath = new();

    [Header("Values")]
    [SerializeField] private float exhibitionSpeed = .02f;


    private ExhibitionState state = ExhibitionState.Locked;

    private List<Visitor> visitors = new();
    private Queue<Visitor> entryQueue = new();

    private readonly Queue<Vector3> insidePathPositions = new();

    public int entryQueueCount => entryQueue.Count;
    public int exhibitionMaxInsideVisitorCount { get; private set; } = 6;
    public int exhibitionMaxEntryVisitorCount { get; private set; } = 6;
    public bool IsEntryLineFilled => entryQueue.Count == exhibitionMaxEntryVisitorCount;

    private int insidePathFrameLength = 0;
    private int exhibitionFrameCount = 0;

    private void Awake()
    {
        for(int i = 0;i < entryPathParent.childCount;i++)
            entryPath.Add(entryPathParent.GetChild(i));

        for (int i = 0;i < insidePathParent.childCount;i++)
            insidePath.Add(insidePathParent.GetChild(i));
    }

    private void Start()
    {
        for(int i = 0;i < insidePath.Count;i++)
        {
            insidePathPositions.Enqueue(insidePath[i].position);
        }

        CalculateInsidePathFrame();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) {
            StartExhibition();
        }

        if(state == ExhibitionState.Started)
        {
            if (++exhibitionFrameCount > insidePathFrameLength)
                EndExhibition();
        }
    }

    public void StartExhibition()
    {
        if (state != ExhibitionState.Waiting || entryQueue.All(visitor => visitor.state != VisitorState.WaitingInLine))
            return;

        visitors.Clear();

        int visitorCount = Mathf.Min(entryQueue.Count, exhibitionMaxInsideVisitorCount);
        for (int i = 0;i < visitorCount;i++)
            visitors.Add(entryQueue.Dequeue());

        for(int i = 0;i < visitors.Count;++i)
        {
            Visitor visitor = visitors[i];
            if (visitor.state != VisitorState.WaitingInLine)
            {
                visitor.GetInLine(this);
                AddVisitorToEntryQueue(visitor);
                continue;
            }
            /////////////////////////// Bunun yerine readonly transform kullanmak daha iyi geibi
            Queue<Vector3> path = new();
            Queue<Vector3> insidePath = new Queue<Vector3>(insidePathPositions);
            path.Enqueue(entryPath[0].position);
            int count = insidePath.Count;
            for (int j = 0;j < count;j++)
                path.Enqueue(insidePath.Dequeue());
            visitor.SetCurrentSpeed(exhibitionSpeed);
            visitor.SetPatrolPoints(path);

            visitor.SetState(VisitorState.Visiting);
            visitor.SetNextAction(() => { 
                visitor.GoToQueue();
                visitor.SetState(VisitorState.Patrol);
                visitor.AddPatrolPoint(Extentions.GetRandomPatrolPoint());
                visitor.SetNextAction(null);
                visitor.SetNextCurrentPatrol();
            });
        }

        //print("Basldi");
        SetState(ExhibitionState.Started);
    }

    public void EndExhibition()
    {
        for (int i = 0;i < visitors.Count;++i)
        {
            visitors[i].SetCurrentSpeed(Constants.visitorMoveSpeed);
            visitors[i].SetState(VisitorState.Patrol);
        }

        visitors.Clear();

        SetState(ExhibitionState.Waiting);
    }

    public void AddVisitorToEntryQueue(Visitor newVisitor)
    {
        if (entryQueue.Count == exhibitionMaxEntryVisitorCount)
            return;

        entryQueue.Enqueue(newVisitor);
    }

    public List<Transform> GetEntryPath() => new List<Transform>(entryPath);

    public void SetState(ExhibitionState newState)
    {
        state = newState;
    }

    private void CalculateInsidePathFrame()
    {
        float insidePathLength = 0;

        insidePathLength += Vector2.Distance(Extentions.Vector3ToVector2XZ(insidePath[0].position), Extentions.Vector3ToVector2XZ(entryPath[^1].position));
        for (int i = 0;i < insidePath.Count - 1;++i)
        {
            insidePathLength += Vector2.Distance(Extentions.Vector3ToVector2XZ(insidePath[i].position), Extentions.Vector3ToVector2XZ(insidePath[i + 1].position));
        }
        insidePathLength /= exhibitionSpeed;
        insidePathFrameLength = Mathf.FloorToInt(insidePathLength);
    }

}
