using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Utilities.Constants;
using Utilities.Enums;
using Cysharp.Threading.Tasks;
using Pool;
using UnityEngine.UI;
using Utilities.Signals;

public class Exhibition : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private UIManager uiManager;

    [Header("References")]
    [SerializeField] private Transform entryPathParent;
    [SerializeField] private Transform insidePathParent;
    [SerializeField] private Transform guidingPathParent;
    [SerializeField] private Transform waitingPoint;
    [SerializeField] private Image timerInsideImage;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private UpgradeMenuController upgradeMenuController;

    [Header("Values")]
    [SerializeField] private float exhibitionTime = 20f;
    [SerializeField] private float minimumExhibitionTime = 10f;
    [SerializeField] private float initialCost = 9f;
    //[SerializeField] private float nextCostLevelDivider = 50f;
    [SerializeField] private float coefficient = 1.16f;
    [SerializeField] private int levelJumpInterval = 10;
    [SerializeField] private float incomeDivider = 20f;
    //[SerializeField] private int maximumVisitor = 8;
    [SerializeField] private float levelJumpUpgradePercentage = 5f;

    private int level = 1;

    public float Income => CalculateIncome(level, visitors.Count);
    public float UpgradeCost => CalculateCost(level);
    public float CapacityCost => 2;
    public float TimeCost => 3;

    // 3 6.75 22.5 110 825

    public readonly List<Transform> entryPath = new();
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


    private int exhibitionTimeFrame = 0;
    private int exhibitionFrameCount = 0;
    private float insidePathLength = 0;

    private float currentExhibitionTimeFrame = 0f;

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

        StatsMenu.Stats newStats = new StatsMenu.Stats()
        {
            level = level,
            income = Income,
            capacity = CurrentMaximumVisitor,
            time = exhibitionTime
        };
        upgradeMenuController.Initialise(newStats, UpgradeCost, 2, 3);
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.S)) {
            TryStartingExhibition();
        }

        if (state == ExhibitionState.Started)
        {
            timerInsideImage.fillAmount = exhibitionFrameCount / currentExhibitionTimeFrame;
            if (++exhibitionFrameCount > currentExhibitionTimeFrame)
                EndExhibition();
        }
    }

    #region State

    private void StartExhibition()
    {
        if (state != ExhibitionState.Waiting || !entryQueue.Any(visitor => visitor.visitorAnimationState == VisitorAnimationState.Standing))
            return;

        visitors.Clear();

        int visitorCount = entryQueue.Count;
        for (int i = 0;i < visitorCount;i++)
            visitors.Add(entryQueue.Dequeue());

        int visitorIndex = 0;
        Visitor visitor;

        for (visitorIndex = 0;visitorIndex < visitors.Count;++visitorIndex)
        {
            visitor = visitors[visitorIndex];
            if (visitor.visitorAnimationState != VisitorAnimationState.Standing)
                break;

            visitor.GetEndOfTheEntryPath(insidePath);
        }

        if(visitorIndex != visitors.Count)
        {
            int removeStartIndex = visitorIndex;
            for (;visitorIndex < visitors.Count;++visitorIndex)
            {
                visitor = visitors[visitorIndex];
                visitor.GetEndOfTheEntryPath(null);
                AddVisitorToEntryQueue(visitor);
            }
            visitors.RemoveRange(removeStartIndex, visitors.Count - removeStartIndex);
        }

        currentExhibitionTimeFrame = exhibitionTimeFrame;

        guide.SetCurrentSpeed(ExhibitionSpeed);
        guide.StartGuiding();

        SetState(ExhibitionState.Started);
    }

    public void EndExhibition()
    {
        exhibitionFrameCount = 0;
        SetState(ExhibitionState.Waiting);

        Player.instance.EarnMoney(Income);
        uiManager.RefreshMoney();
        visitors.Clear();

        TryStartingExhibition();
    }

    public async void TryStartingExhibition()
    {
        while(state != ExhibitionState.Started)
        {
            StartExhibition();
            await UniTask.Delay(3000);
        }
    }
    public void SetState(ExhibitionState newState)
    {
        state = newState;
    }

    #endregion

  

    private void CalculateExhibitionValues()
    {
        exhibitionTimeFrame = (int)(exhibitionTime / Constants.fixedUpdateFrameInterval);

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

    #region Upgrades
    public void IncrementLevel()
    {
        if (UpgradeCost > Player.instance.Money)
            return;

        level++;
        Player.instance.SpendMoney(Mathf.FloorToInt(UpgradeCost));

        upgradeMenuController.SetUpgradeCost(UpgradeType.Exhibition, UpgradeCost);
        upgradeMenuController.SetStat(UpgradeType.Exhibition, level, CalculateIncome(level, CurrentMaximumVisitor));
        print(CalculateIncome(level, CurrentMaximumVisitor));
    }

    public void IncrementCurrentMaxVisitor()
    {
        if(entryPathParent.childCount == CurrentMaximumVisitor)
        {
            Debug.LogError("Exhibition: IncrementMaxEntryVisitorCount, There is not enough entry queue points!");
            return;
        }
        else if(CurrentMaximumVisitor == Constants.MaximumExhibitionVisitor)
        {
            Debug.LogError("Exhibition: IncrementMaxEntryVisitorCount, Reached maximum visitor number!");
            return;
        }

        CurrentMaximumVisitor++;
        Player.instance.SpendMoney(Mathf.FloorToInt(CapacityCost));

        upgradeMenuController.SetUpgradeCost(UpgradeType.Capacity, CurrentMaximumVisitor != Constants.MaximumExhibitionVisitor ? 2 : -1);
        upgradeMenuController.SetStat(UpgradeType.Capacity, CurrentMaximumVisitor);
    }

    public void DecrementTime()
    {
        if(exhibitionTime == minimumExhibitionTime)
        {
            Debug.LogError("Exhibition: DecrementTime, Reached minimum exhibition time!");
            return;
        }

        exhibitionTime--; 
        CalculateExhibitionValues();
        Player.instance.SpendMoney(Mathf.FloorToInt(TimeCost));

        upgradeMenuController.SetUpgradeCost(UpgradeType.Time, exhibitionTime != minimumExhibitionTime ? 3 : -1);
        upgradeMenuController.SetStat(UpgradeType.Time, exhibitionTime);
    }


    private float CalculateIncome(float desiredLevel, int desiredVisitorCount) => initialCost + (Mathf.Pow(coefficient, Mathf.Floor(desiredLevel / levelJumpInterval)) * desiredLevel / incomeDivider) * desiredVisitorCount;

    private float CalculateCost(int desiredLevel)
    {
        // Calculation of levels at the beginning and the end of the Jump
        float floorLevel = desiredLevel - (desiredLevel % levelJumpInterval);
        float ceilingLevel = desiredLevel + levelJumpInterval - (desiredLevel % levelJumpInterval) - 0.01f;

        // Costs at the beginning and the end of the Jump
        float floor = CalculateIncome(floorLevel, Constants.MaximumExhibitionVisitor);
        float ceiling = CalculateIncome(ceilingLevel, Constants.MaximumExhibitionVisitor);
        float gap = ceiling - floor;

        // gap / 3 is for narrowing the gap
        float newFloor = floor + (gap / 3);
        float newCeiling = ceiling - (gap / 3);

        // Upgrade values is for making income and cost higher at every Jump
        float upgradeValue = gap * Mathf.Floor(desiredLevel / levelJumpInterval) * levelJumpUpgradePercentage / 100;
        newFloor += newFloor * upgradeValue;
        newCeiling += newCeiling * upgradeValue;

        return Mathf.Lerp(newFloor, newCeiling, (float)(desiredLevel % levelJumpInterval) / levelJumpInterval);
    }
    public void OpenUpgradeMenu()
    {

    }

    #endregion


    public void AddVisitorToEntryQueue(Visitor newVisitor)
    {
        if (entryQueue.Count == CurrentMaximumVisitor || entryQueue.Contains(newVisitor))
            return;

        entryQueue.Enqueue(newVisitor);
    }

    public int FirstEmptyEntryPathIndex => IsEntryQueueFilled ? -1 : entryQueue.Count;

    public Transform GetWaitingPoint() => waitingPoint;


    private void FaceUIToCamera(Quaternion rotation) => worldCanvas.transform.rotation = rotation;

    private void OnEnable()
    {
        Signals.OnFaceCanvasToCamera += FaceUIToCamera;
    }

    private void OnDisable()
    {
        Signals.OnFaceCanvasToCamera -= FaceUIToCamera;
    }

}
