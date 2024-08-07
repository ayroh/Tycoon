using System;
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
    [SerializeField] private Transform waitingPoint;

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

    public bool[] entryQueueOccupations;

    private int insidePathFrameLength = 0;
    private int exhibitionFrameCount = 0;

    private void Awake()
    {
        for(int i = 0;i < entryPathParent.childCount;i++)
            entryPath.Add(entryPathParent.GetChild(i));

        for (int i = 0;i < insidePathParent.childCount;i++)
            insidePath.Add(insidePathParent.GetChild(i));

        entryQueueOccupations = new bool[exhibitionMaxEntryVisitorCount];
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
                visitor.GetInEntryPath(this);
                AddVisitorToEntryQueue(visitor);
                continue;
            }

            visitor.GetInInsidePath(insidePath, exhibitionSpeed);
        }

        Array.Fill(entryQueueOccupations, false);
        SetState(ExhibitionState.Started);
    }

    public void EndExhibition()
    {
        visitors.Clear();

        SetState(ExhibitionState.Waiting);
    }

    public void AddVisitorToEntryQueue(Visitor newVisitor)
    {
        if (entryQueue.Count == exhibitionMaxEntryVisitorCount)
            return;

        entryQueue.Enqueue(newVisitor);
    }

    public List<Transform> GetEntryLine() => entryPath;
    public List<Transform> GetInsideLine() => insidePath;

    public void SetState(ExhibitionState newState)
    {
        state = newState;
    }

    public bool GetIfEntryLineIndexIsOccupied(int index)
    {
        if (!Extentions.IsIndexWithinBounds(index, entryQueueOccupations))
        {
            Debug.LogError("Exhibition: GetIfEntryLineIndexIsOccupied, Index out of size!");
            return false;
        }

        return entryQueueOccupations[index];
    }


    public void FillNextLine(int nextLineIndex, int previousLineIndex)
    {
        if(!Extentions.IsIndexWithinBounds(nextLineIndex, entryQueueOccupations) || !Extentions.IsIndexWithinBounds(previousLineIndex, entryQueueOccupations))
        {
            Debug.LogError("Exhibition: FillNextLine, Index out of size!");
            return;
        }

        entryQueueOccupations[previousLineIndex] = false;
        entryQueueOccupations[nextLineIndex] = true;
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
