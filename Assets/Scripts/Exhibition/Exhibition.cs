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
    [Header("References")]
    [SerializeField] private Transform entryPathParent;
    [SerializeField] private Transform insidePathParent;
    [SerializeField] private Transform guidingPathParent;
    [SerializeField] private Transform carpetParent;
    [SerializeField] private Transform waitingPoint;
    [SerializeField] private Canvas worldCanvas;

    [Header("Values")]
    [SerializeField] private float minimumExhibitionTime = 10f;
    [SerializeField] private float initialCost = 9f;
    [SerializeField] private float coefficient = 1.16f;
    [SerializeField] private float incomeDivider = 20f;
    [SerializeField] private float levelJumpUpgradePercentage = 5f;
    [SerializeField] private int levelJumpInterval = 10;
    [SerializeField] private ExhibitionType type;

    private ExhibitionState state = ExhibitionState.Locked;
    private Data data;

    public float Income => CalculateIncome(data.level, visitors.Count);
    public float UpgradeCost => CalculateUpgradeCost(data.level);
    public float CapacityCost => CalculateCapacityCost(data.capacity - Constants.InitialExhibitionCapacity + 1);
    public float TimeCost => CalculateTimeCost(Constants.InitialExhibitionTime - data.time + 1);
    public bool IsEntryQueueFilled => (entryQueue.Count == data.capacity);
    public int FirstEmptyEntryPathIndex => IsEntryQueueFilled ? -1 : entryQueue.Count;
    public int Capacity => data.capacity;

    public float ExhibitionSpeed { get; private set; } = 0f;

    private Guide guide;
    private List<Visitor> visitors = new();
    private Queue<Visitor> entryQueue = new();


    private void Awake()
    {
        data = Player.instance.LoadExhibition(type);

        for(int i = 0;i < data.capacity;++i)
            carpetParent.GetChild(i).gameObject.SetActive(true);

        for(int i = 0;i < entryPathParent.childCount;i++)
            entryPath.Add(entryPathParent.GetChild(i));

        for (int i = 0;i < insidePathParent.childCount;i++)
            insidePath.Add(insidePathParent.GetChild(i));

        for(int i = 0;i < insidePath.Count;i++)
            insidePathPositions.Enqueue(insidePath[i].position);

        CalculateExhibitionValues();

        bounds = new OrientedBounds(transform.position, Constants.ExhibitionSize, transform.rotation);

        guide = (Guide)PoolManager.instance.Get(PoolObjectType.Guide);
        guide.SetGuide(this, guidingPathParent);

        upgradeMenuController.Initialise(new StatsMenu.Stats(data.level, Income, data.capacity, data.time), UpgradeCost, CapacityCost, TimeCost) ;
    }

    private void FixedUpdate()
    {
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

  
    #region Upgrades

    [Header("Upgrade")]
    [SerializeField] private Image timerInsideImage;
    [SerializeField] private Canvas upgradeMenuCanvas;
    [SerializeField] private UpgradeMenuController upgradeMenuController;

    public void IncrementLevel()
    {
        if (UpgradeCost > Player.instance.Money)
            return;

        Player.instance.SpendMoney(Mathf.FloorToInt(UpgradeCost));

        data.level++;

        upgradeMenuController.SetUpgradeCost(UpgradeType.Exhibition, UpgradeCost);
        upgradeMenuController.SetStat(UpgradeType.Exhibition, data.level, CalculateIncome(data.level, data.capacity));
    }

    public void IncrementCapacity()
    {
        if(entryPathParent.childCount == data.capacity)
        {
            Debug.LogError("Exhibition: IncrementMaxEntryVisitorCount, There is not enough entry queue points!");
            return;
        }
        else if(data.capacity == Constants.MaximumExhibitionCapacity)
        {
            Debug.LogError("Exhibition: IncrementMaxEntryVisitorCount, Reached maximum visitor number!");
            return;
        }

        carpetParent.GetChild(data.capacity).gameObject.SetActive(true);
        Player.instance.SpendMoney(Mathf.FloorToInt(CapacityCost));

        data.capacity++;

        upgradeMenuController.SetUpgradeCost(UpgradeType.Capacity, data.capacity != Constants.MaximumExhibitionCapacity ? CapacityCost : -1);
        upgradeMenuController.SetStat(UpgradeType.Capacity, data.capacity);

    }

    public void DecrementTime()
    {
        if(data.time == minimumExhibitionTime)
        {
            Debug.LogError("Exhibition: DecrementTime, Reached minimum exhibition time!");
            return;
        }

        Player.instance.SpendMoney(Mathf.FloorToInt(TimeCost));

        data.time--; 
        CalculateExhibitionValues();

        upgradeMenuController.SetUpgradeCost(UpgradeType.Time, data.time != minimumExhibitionTime ? TimeCost : -1);
        upgradeMenuController.SetStat(UpgradeType.Time, data.time);
    }


    private float CalculateIncome(float desiredLevel, int desiredVisitorCount) => initialCost + (Mathf.Pow(coefficient, Mathf.Floor(desiredLevel / levelJumpInterval)) * desiredLevel / incomeDivider) * desiredVisitorCount;

    private float CalculateUpgradeCost(int desiredLevel)
    {
        // Calculation of levels at the beginning and the end of the Jump
        float floorLevel = desiredLevel - (desiredLevel % levelJumpInterval);
        float ceilingLevel = desiredLevel + levelJumpInterval - (desiredLevel % levelJumpInterval) - 0.01f;

        // Costs at the beginning and the end of the Jump
        float floor = CalculateIncome(floorLevel, Constants.MaximumExhibitionCapacity);
        float ceiling = CalculateIncome(ceilingLevel, Constants.MaximumExhibitionCapacity);
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

    private float CalculateCapacityCost(int desiredLevel) => initialCost * Mathf.Pow(2.25f, desiredLevel - 1);

    private float CalculateTimeCost(int desiredLevel) => initialCost * Mathf.Pow(1.75f, desiredLevel - 1);


    public void OpenUpgradeMenu()
    {
        upgradeMenuCanvas.gameObject.SetActive(true);
    }

    #endregion

    #region Bounds

    private OrientedBounds bounds;

    public bool IsInBounds(Vector3 point) => bounds.Contains(point);

    #endregion

    #region Path

    public List<Transform> EntryPath => entryPath;
    public Transform WaitingPoint => waitingPoint;

    private readonly List<Transform> entryPath = new();
    private readonly List<Transform> insidePath = new();
    private readonly Queue<Vector3> insidePathPositions = new();

    private int exhibitionTimeFrame = 0;
    private int exhibitionFrameCount = 0;
    private float insidePathLength = 0;

    private float currentExhibitionTimeFrame = 0f;

    private void CalculateExhibitionValues()
    {
        exhibitionTimeFrame = (int)(data.time / Constants.fixedUpdateFrameInterval);

        if (insidePathLength == 0)
        {
            insidePathLength += Vector2.Distance(Extentions.Vector3ToVector2XZ(insidePath[0].position), Extentions.Vector3ToVector2XZ(entryPath[Mathf.FloorToInt((float)entryPath.Count / 2)].position));
            for (int i = 0;i < insidePath.Count - 1;++i)
            {
                insidePathLength += Vector2.Distance(Extentions.Vector3ToVector2XZ(insidePath[i].position), Extentions.Vector3ToVector2XZ(insidePath[i + 1].position));
            }
        }

        ExhibitionSpeed = insidePathLength / exhibitionTimeFrame;
    }

    #endregion

    public void AddVisitorToEntryQueue(Visitor newVisitor)
    {
        if (entryQueue.Count == data.capacity || entryQueue.Contains(newVisitor))
            return;

        entryQueue.Enqueue(newVisitor);
    }

    private void FaceUIToCamera(Quaternion rotation) => worldCanvas.transform.rotation = rotation;

    private void OnEnable()
    {
        Signals.OnFaceCanvasToCamera += FaceUIToCamera;
    }

    private void OnDisable()
    {
        Signals.OnFaceCanvasToCamera -= FaceUIToCamera;
    }

    public class Data
    {
        public int level;
        public int capacity;
        public int time;
    }
}
