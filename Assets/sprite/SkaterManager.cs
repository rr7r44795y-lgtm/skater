using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SkaterManager : MonoBehaviour
{
    
    public static SkaterManager Instance { get; private set; }

    [Header("原Skater")]
    public int lowNumber;
    public int middleNumber;
    public int highNumber;
    public List<Skater> CandidateSkater = new List<Skater>();
    public List<Skater> DeletList = new List<Skater>();

    private bool waitingForChoice = false;
    private bool isPopSave = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region 创建skater,CreatorSkater(createType type),type{low,middle,high}
    public void CreatorSkater(createType type)
    {
        switch (type)
        {
            case createType.low:
                for (int i = Random.Range(1, 4); i > 0; i--)
                {
                    Skater s = new Skater();
                    int k = Random.Range(0, 2);
                    if (k == 0) { s.sex = Sex.girl; }
                    else { s.sex = Sex.boy; }
                    s.name = ReadAndWrite(s.sex);
                    s.personality = personType.lazy;

                    s.stamina = 100;
                    s.jump = Random.Range(1, lowNumber);
                    s.dance = Random.Range(1, lowNumber);
                    s.age = Random.Range(10, 18);
                    s.spin = Random.Range(1, lowNumber);
                    s.fans = Random.Range(1, lowNumber);
                    s.skaterType = skaterType.candidate;
                    s.speed = Random.Range(320, 401);
                    CandidateSkater.Add(s);
                }
                break;
            case createType.middle:
                for (int i = Random.Range(3, 5); i > 0; i--)
                {
                    Skater s = new Skater();
                    int k = Random.Range(0, 2);
                    if (k == 0) { s.sex = Sex.girl; }
                    else { s.sex = Sex.boy; }
                    s.name = ReadAndWrite(s.sex);
                    s.personality = personType.lazy;

                    s.stamina = 100;
                    s.skaterType = skaterType.candidate;
                    s.jump = Random.Range(1, middleNumber);
                    s.dance = Random.Range(1, middleNumber);
                    s.age = Random.Range(6, 18);
                    s.spin = Random.Range(1, middleNumber);
                    s.fans = Random.Range(1, middleNumber);
                    s.speed = Random.Range(320, 401);
                    CandidateSkater.Add(s);
                }
                break;
            case createType.high:
                for (int i = Random.Range(4, 6); i > 0; i--)
                {
                    Skater s = new Skater();
                    int k = Random.Range(0, 2);

                    if (k == 0) { s.sex = Sex.girl; }
                    else { s.sex = Sex.boy; }

                    s.name = ReadAndWrite(s.sex);

                    s.skaterType = skaterType.candidate;
                    s.personality = personType.lazy;

                    s.stamina = 100;
                    s.jump = Random.Range(1, highNumber);
                    s.dance = Random.Range(1, highNumber);
                    s.age = Random.Range(3, 15);
                    s.spin = Random.Range(1, highNumber);
                    s.fans = Random.Range(1, highNumber);
                    s.speed = Random.Range(320, 401);
                    CandidateSkater.Add(s);
                }
                break;
        }

    }
    #endregion

    #region 清除list
    public void ClearList()
    {
        CandidateSkater.Clear();
    }
    #endregion

    #region 年龄控制器
    public void AgeHelp()
    {
        StartCoroutine(AddAge());

    }

    IEnumerator AddAge()
    {
        foreach (var s in GameManager.Instance.currentSaveData.Skaters)
        {
            s.age++;
            if (s.age > 12 && s.age < 18 && s.sex == Sex.girl)
            {
                if (Random.value < 0.3f)
                {
                    s.jump = s.jump / 2;
                    s.spin = s.spin / 2;
                    s.dance = s.dance / 2;
                }
            }
            else if (s.age > 14 && s.age < 20 && s.sex == Sex.boy)
            {
                if (Random.value < 0.3f)
                {
                    s.jump = s.jump / 2;
                    s.spin = s.spin / 2;
                    s.dance = s.dance / 2;
                }
            }

            if (s.sex == Sex.girl)
            {
                switch (s.age)
                {
                    case 21:
                        if (Random.value < 0.3f) isPopSave = true;
                        break;
                    case 22:
                        if (Random.value < 0.4f) isPopSave = true;
                        break;
                    case 23:
                        if (Random.value < 0.5f) isPopSave = true;
                        break;
                    case 24:
                        if (Random.value < 0.6f) isPopSave = true;
                        break;
                    case 25:
                        isPopSave = true;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (s.age)
                {
                    case 22:
                        if (Random.value < 0.3f) isPopSave = true;
                        break;
                    case 23:
                        if (Random.value < 0.4f) isPopSave = true;
                        break;
                    case 24:
                        if (Random.value < 0.5f) isPopSave = true;
                        break;
                    case 25:
                        if (Random.value < 0.6f) isPopSave = true;
                        break;
                    case 26:
                        isPopSave = true;
                        break;
                    default:
                        break;
                }
            }

            if (isPopSave == true)
            {
                waitingForChoice = true;
                exitSkater(s);
                while (waitingForChoice)
                {
                    yield return null; // 等玩家点完
                }
            }
        }
        DeletSkater(DeletList);
    }
    #endregion

    #region 退役控制器+删除函数//相关扣钱逻辑没写
    private void exitSkater(Skater s)
    {
        PopupManager.Instance?.ChoicePop($"{s.name}想要退役，你是否要挽留？",
            "挽留",
            "不管",
            () => { 
                /* 挽留逻辑，比如扣钱 */
                waitingForChoice = false;
                isPopSave = false;
            },
            () => { 
                DeletList.Add(s);
                waitingForChoice = false;
                isPopSave = false;
            });
    }

    private void DeletSkater(List<Skater> d)
    {
        foreach (var s in DeletList)
        {
            GameManager.Instance.currentSaveData.Skaters.Remove(s);
        }
        DeletList.Clear();
    }
    #endregion


    [Header("宣传招聘skater")]
    public int lowMoney;
    public int middleMoney;
    public int highMoney;
    public int lowDay;
    public int middleDay;
    public int highDay;

    [SerializeField] private Button LowBtn;
    [SerializeField] private Button MiddleBtn;
    [SerializeField] private Button HighBtn;
    [SerializeField] private Button ConfirmBtn;

    [SerializeField] private GameObject CandidateSkaterPanel;
    [SerializeField] private GameObject CandidateSkaterPrefab;
    [SerializeField] private GameObject Mask;
    [SerializeField] private GameObject currentPanel;

    #region 初始化
    void Start()
    {
        LowBtn.onClick.AddListener(() => OnClicked(createType.low));
        MiddleBtn.onClick.AddListener(() => OnClicked(createType.middle));
        HighBtn.onClick.AddListener(() => OnClicked(createType.high));
        ConfirmBtn.onClick.AddListener(OnConfirm);
        if(GameManager.Instance.currentSaveData.createType!=createType.No)
          StartCoroutine(ConFirmToCreateSkater(GameManager.Instance.currentSaveData.createType));
    }
    #endregion

    #region Clicked事件
    private void OnClicked(createType L)
    {
        if (GameManager.Instance.currentSaveData.createType == createType.No)
        {
            string S = " ", Confirm = "确定", Cancel = "取消";
            int Day = 0, Money = 0;
            switch (L)
            {
                case createType.low:
                    S = $"当前选择的是线下传单宣传，将消耗{lowMoney}金，为期{lowDay}天";
                    Money = lowMoney;
                    Day = lowDay;
                    break;
                case createType.middle:
                    S = $"当前选择的是就近省市宣传，将消耗{middleMoney}金，为期{middleDay}天";
                    Day = middleDay;
                    Money = middleMoney;
                    break;
                case createType.high:
                    S = $"当前选择的是全国范围宣传，将消耗{highMoney}金，为期{highDay}天";
                    Day = highDay;
                    Money = highMoney;
                    break;
            }

            PopupManager.Instance?.ChoicePop(S, Confirm, Cancel,

                       () =>
                       {
                           GameManager.Instance.currentSaveData.createType = L;
                           GameManager.Instance.currentSaveData.createTime = Day;
                           GameManager.Instance.currentSaveData.money -= Money;
                           StartCoroutine(ConFirmToCreateSkater(L));
                       },
                       () =>
                       {

                       }
                       );//取消就什么也不做
        }
        else
        {
            List<string> message = new List<string>();
            message.Add("当前正在宣传中!请耐心等待!");
            PopupManager.Instance.MessagePop(message);
        }
    }
    #endregion

    #region 关闭选择页面
    private void OnConfirm()
    {
        CandidateSkaterPanel.gameObject.SetActive(false);
        Mask.gameObject.SetActive(false);
        CandidateSkater.Clear();
        GameManager.Instance.ResumeGame();
        GameManager.Instance.currentSaveData.createType = createType.No;
    }
    #endregion

    #region  宣传中的协程
    IEnumerator ConFirmToCreateSkater(createType L)
    {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.currentSaveData.createType = L;
        while (GameManager.Instance.currentSaveData.createTime > 0)
        {
            yield return new WaitForSeconds(3f);
        }
        CreatorSkater(L);
        GameManager.Instance.PauseGame();
        Mask.gameObject.SetActive(true);
        CandidateSkaterPanel.gameObject.SetActive(true);
        foreach (Transform child in currentPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var skater in SkaterManager.Instance.CandidateSkater)
        {
            GameObject card = Instantiate(CandidateSkaterPrefab, currentPanel.transform);
            card.GetComponent<SkaterCard>().Init(skater);
        }
    }
    #endregion

    [Serializable]
    public class NameList
    {
        public List<string> first;
        public List<string> last;
    }

    #region 读写文件的那个公共函数
    public string ReadAndWrite(Sex sex)
    {
        string file = sex.ToString();
        TextAsset json = Resources.Load<TextAsset>($"{file}");
        NameList data = JsonUtility.FromJson<NameList>(json.text);
        int a = Random.Range(1, 3);
        string name = data.first[Random.Range(0, data.first.Count)];
        switch(a)
            {
            case 1:
                name += data.last[Random.Range(0, data.last.Count)];
                break;
            case 2:
                name += data.last[Random.Range(0, data.last.Count)]+ data.last[Random.Range(0, data.last.Count)];
                break;
        }
            return name;
        }
    #endregion
}
