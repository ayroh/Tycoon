using Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Utilities.Constants;
using Utilities.Enums;

public class Exhibition : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private UIManager uiManager;

    [Header("References")]
    [SerializeField] private Transform entryPathParent;
    [SerializeField] private Transform insidePathParent;
    [SerializeField] private Transform guidingPathParent;
    [SerializeField] private Transform waitingPoint;

    [Header("Values")]
    [SerializeField] private float exhibitionTime = 20f;
    [SerializeField] private float minimumExhibitionTime = 10f;
    [SerializeField] private float initialCost = 9f;
    //[SerializeField] private float nextCostLevelDivider = 50f;
    [SerializeField] private float coefficient = 1.16f;
    [SerializeField] private int levelJumpInterval = 10;
    [SerializeField] private float incomeDivider = 20f;
    [SerializeField] private int maximumVisitor = 8;
    [SerializeField] private float levelJumpUpgradePercentage = 5f;

    private int level = 1;

    public float Income => CalculateIncome(level, visitors.Count);
    public float UpgradeCost => CalculateCost(level);

    private readonly List<Transform> entryPath = new();
    private readonly List<Transform> insidePath = new();

    private ExhibitionState state = ExhibitionState.Locked;

    private List<Visitor> visitors = new();
    private Queue<Visitor> entryQueue = new();
    private Guide guide;

    private readonly Queue<Vector3> insidePathPositions = new();


    public float ExhibitionSpeed { get; private set; } = 0f;
    public int CurrentMaximumVisitor { get; private set; } = 4;
    public bool IsEntryQueueFilled => (entryQueue.Count == CurrentMaximumVisitor);
    public Transform GuidingPathParent => guidingPathParent;


    private float exhibitionTimeFrame = 0f;
    private int exhibitionFrameCount = 0;
    private float insidePathLength = 0;


    private void Awake()
    {
        for(int i = 0;i < entryPathParent.childCount;i++)
            entryPath.Add(entryPathParent.GetChild(i));

        for (int i = 0;i < insidePathParent.childCount;i++)
            insidePath.Add(insidePathParent.GetChild(i));

        for(int i = 0;i < insidePath.Count;i++)
            insidePathPositions.Enqueue(insidePath[i].position);

        CalculateExhibitionValues();

        guide = (Guide)PoolManager.instance.Get(PoolObjectType.Guide);
        guide.SetGuide(this);

    }
    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.S)) {
            StartExhibition();
        }

        if (state == ExhibitionState.Started)
        {
            if (++exhibitionFrameCount > exhibitionTimeFrame)
                EndExhibition();
        }
    }

    public void StartExhibition()
    {
        if (state != ExhibitionState.Waiting || entryQueue.All(visitor => visitor.visitorState != VisitorState.WaitingInLine))
            return;

        visitors.Clear();

        int visitorCount = Mathf.Min(entryQueue.Count, CurrentMaximumVisitor);
        for (int i = 0;i < visitorCount;i++)
            visitors.Add(entryQueue.Dequeue());

        List<Transform> entryPathUntilVisitorIndexTemp = new();

        for (int i = 0;i < visitors.Count;++i)
        {
            entryPathUntilVisitorIndexTemp.Add(entryPath[i]);
            List<Transform> entryPathUntilVisitorIndex = new List<Transform>(entryPathUntilVisitorIndexTemp);

            Visitor visitor = visitors[i];
            if(visitor.visitorAnimationState != VisitorAnimationState.Standing)
            {
                for(int j = i;j < visitors.Count;++j)
                {
                    Visitor visitorToEntryQueue = visitors[j];
                    visitorToEntryQueue.GetEndOfTheEntryPath(entryPathUntilVisitorIndex, null);
                    AddVisitorToEntryQueue(visitorToEntryQueue);
                }
                break;
            }

            visitor.GetEndOfTheEntryPath(entryPathUntilVisitorIndex, insidePath);
            //visitor.GetInInsidePath(path, ExhibitionSpeed);
        }

        guide.SetCurrentSpeed(ExhibitionSpeed);
        guide.StartGuiding();

        SetState(ExhibitionState.Started);
    }

    public void EndExhibition()
    {
        exhibitionFrameCount = 0;
        SetState(ExhibitionState.Waiting);

        float income = Income * 100;
        uiManager.AddMoney(income);
        Player.instance.EarnMoney(income);
        visitors.Clear();
    }

    public void AddVisitorToEntryQueue(Visitor newVisitor)
    {
        if (entryQueue.Count == CurrentMaximumVisitor || entryQueue.Contains(newVisitor))
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

    private void CalculateExhibitionValues()
    {
        exhibitionTimeFrame = exhibitionTime / Constants.fixedUpdateFrameInterval;

        if(insidePathLength == 0)
        {
            insidePathLength += Vector2.Distance(Extentions.Vector3ToVector2XZ(insidePath[0].position), Extentions.Vector3ToVector2XZ(entryPath[Mathf.FloorToInt((float)entryPath.Count / 2)].position));
            for (int i = 0;i < insidePath.Count - 1;++i)
            {
                insidePathLength += Vector2.Distance(Extentions.Vector3ToVector2XZ(insidePath[i].position), Extentions.Vector3ToVector2XZ(insidePath[i + 1].position));
            }
        }

        ExhibitionSpeed = insidePathLength / exhibitionTimeFrame;
    }


    public void IncrementCurrentMaxVisitor()
    {
        if(entryPathParent.childCount == CurrentMaximumVisitor)
        {
            Debug.LogError("Exhition: IncrementMaxEntryVisitorCount, There is not enough entry queue points!");
            return;
        }
        else if(CurrentMaximumVisitor == maximumVisitor)
        {
            Debug.LogError("Exhition: IncrementMaxEntryVisitorCount, Reached maximum visitor number!");
            return;
        }

        CurrentMaximumVisitor++;
    }

    public void DecrementTime()
    {
        if(exhibitionTime == minimumExhibitionTime)
        {
            Debug.LogError("Exhition: DecrementTime, Reached minimum exhibition time!");
            return;
        }

        exhibitionTime--; 
        CalculateExhibitionValues();
    }

    public void IncrementLevel()
    {
        //if (UpgradeCost > Player.instance.Money)
        //    return;

        //Player.instance.SpendMoney(Mathf.FloorToInt(UpgradeCost));
        level++;
        print($"lv{level}: " + CalculateIncome(level, maximumVisitor) + " | " + CalculateCost(level));
    }

    private float CalculateIncome(float desiredLevel, int desiredVisitorCount) => initialCost + (Mathf.Pow(coefficient, Mathf.Floor(desiredLevel / levelJumpInterval)) * desiredLevel / incomeDivider) * desiredVisitorCount;

    private float CalculateCost(int desiredLevel) 
    {
        float floorLevel = desiredLevel - (desiredLevel % levelJumpInterval);
        float ceilingLevel = desiredLevel + levelJumpInterval - (desiredLevel % levelJumpInterval) - 0.01f;
        float upgradeValue = (Mathf.Floor((desiredLevel / levelJumpInterval)) * levelJumpUpgradePercentage / 100);

        float floor = CalculateIncome(floorLevel, maximumVisitor);
        float ceiling = CalculateIncome(ceilingLevel, maximumVisitor);
        float gap = ceiling - floor;
        print("value: " + upgradeValue);
        float newFloor = floor + (gap / 3) + (gap / 2) * upgradeValue;
        float newCeiling = ceiling - (gap / 3) + (gap / 2) * upgradeValue;

        newFloor += newFloor * upgradeValue;
        newCeiling += newCeiling * upgradeValue;

        return Mathf.Lerp(newFloor, newCeiling, (float)(desiredLevel % levelJumpInterval) / levelJumpInterval);
    }

}
