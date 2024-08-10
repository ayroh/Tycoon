using Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using Utilities.Constants;
using Utilities.Enums;

public class Exhibition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform entryPathParent;
    [SerializeField] private Transform insidePathParent;
    [SerializeField] private Transform guidingPathParent;
    [SerializeField] private Transform waitingPoint;

    private readonly List<Transform> entryPath = new();
    private readonly List<Transform> insidePath = new();

    [Header("Values")]
    [SerializeField] private float exhibitionSpeed = .02f;


    private ExhibitionState state = ExhibitionState.Locked;

    private List<Visitor> visitors = new();
    private Queue<Visitor> entryQueue = new();

    private readonly Queue<Vector3> insidePathPositions = new();

    private Guide guide;

    public float ExhibitionSpeed => exhibitionSpeed;
    public int exhibitionMaxInsideVisitorCount { get; private set; } = 6;
    public int exhibitionMaxEntryVisitorCount { get; private set; } = 6;
    public bool IsEntryLineFilled => entryQueue.Count == exhibitionMaxEntryVisitorCount;
    public Transform GuidingPathParent => guidingPathParent;

    private int insidePathFrameLength = 0;
    private int exhibitionFrameCount = 0;

    private void Awake()
    {
        for(int i = 0;i < entryPathParent.childCount;i++)
            entryPath.Add(entryPathParent.GetChild(i));

        for (int i = 0;i < insidePathParent.childCount;i++)
            insidePath.Add(insidePathParent.GetChild(i));

        guide = (Guide)PoolManager.instance.Get(PoolObjectType.Guide);
        guide.SetGuide(this);
    }

    private void Start()
    {
        for(int i = 0;i < insidePath.Count;i++)
        {
            insidePathPositions.Enqueue(insidePath[i].position);
        }

        CalculateInsidePathFrame();
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.S)) {
            StartExhibition();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            print(entryQueue.Count);
        }

        if (state == ExhibitionState.Started)
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

        List<Transform> path = insidePath;
        path.Add(entryPath[0]);

        for(int i = 0;i < visitors.Count;++i)
        {
            Visitor visitor = visitors[i];
            visitor.GetInInsidePath(path, exhibitionSpeed);
        }

        guide.StartGuiding();

        SetState(ExhibitionState.Started);
    }

    public void EndExhibition()
    {
        visitors.Clear();
        exhibitionFrameCount = 0;

        SetState(ExhibitionState.Waiting);
    }

    public void AddVisitorToEntryQueue(Visitor newVisitor)
    {
        if (entryQueue.Count == exhibitionMaxEntryVisitorCount || entryQueue.Contains(newVisitor))
            return;

        entryQueue.Enqueue(newVisitor);
    }

    public List<Transform> GetEntryPathUntilCurrentQueue()
    {
        List<Transform> entryLine = new();
        for(int i = entryQueue.Count;i < entryPath.Count;i++)
            entryLine.Add(entryPath[i]);
        return entryLine;
    }

    public Transform GetWaitingPoint() => waitingPoint;

    public void SetState(ExhibitionState newState)
    {
        state = newState;
    }

    private void CalculateInsidePathFrame()
    {
        float insidePathLength = 0;

        insidePathLength += Vector2.Distance(Extentions.Vector3ToVector2XZ(insidePath[0].position), Extentions.Vector3ToVector2XZ(entryPath[Mathf.FloorToInt((float)entryPath.Count / 2)].position));
        for (int i = 0;i < insidePath.Count - 1;++i)
        {
            insidePathLength += Vector2.Distance(Extentions.Vector3ToVector2XZ(insidePath[i].position), Extentions.Vector3ToVector2XZ(insidePath[i + 1].position));
        }
        insidePathLength /= exhibitionSpeed;
        insidePathFrameLength = Mathf.FloorToInt(insidePathLength);
    }

    
}
