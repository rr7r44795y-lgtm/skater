using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Skater
{
    public string name;
    public int stamina;
    public int jump;
    public int spin;
    public int dance;
    public int fans;
    public int age;
    public float speed;
    public Sex sex;
    public bool isCompeting = false;
    public skaterType skaterType;
    public personType personality;
    public currentType current = currentType.training;
    public trainType trainingType = trainType.jump;
    public List<JumpData> jumpTypes = new List<JumpData>();
    public List<JumpData> ShowList = new List<JumpData>();
    public List<skill> skills = new List<skill>();
    public List<string> LiveList = new List<string>();//生平经历
}

[Serializable]
public class CompetitionList
{
    public List<Competition> competitions;
}

[Serializable]
public class Competition
{
    public int month;
    public int year;
    public int MiniAge;
    public int MaxAge;
    public string compID;
    public CompType compType;
    public Sex sexType;
    public int fee;
    public int award;
    public string name;
    public string description;
    

    public string GetCompTypeName()
    {
        switch (compType)
        {
            case CompType.Local: return "地方";
            case CompType.District: return "区级";
            case CompType.Provincial: return "省级";
            case CompType.National: return "国赛";
            case CompType.International: return "国际";
            case CompType.Exhibition: return "表演赛";
            default: return "";
        }
    }
}

public enum CompType
{
    Local,
    District,
    Provincial,
    National,
    International,
    Exhibition
}

public enum skaterType
{
    candidate,
    active,
    retired
}

public enum createType
{
    low,
    middle,
    high,
    No
}

public enum Sex
{
    girl,
    boy
}

public enum currentType
{
    training,
    relax
}

public enum trainType
{
    jump,
    spin,
    dance
}

public enum personType
{
    study,
    lazy
}

public enum jumpType
{
    Toeloop,
    Salchow,
    Loop,
    Flip,
    Lutz,
    Axel
}

public enum skill
{
    Skill1,
    Skill2
}

[Serializable]
public class JumpData
{
    public jumpType jumpName;      // 哪种跳跃
    public int rotation;           // 当前圈数（1-4）
    public int maxRotation;        // 圈数上限（初始1，可解锁）
    public int staminaCost;        // 体力消耗
    public float baseScore;        // 基础分

    public JumpData(jumpType name, int rot, int maxRot, int cost, float score)
    {
        jumpName = name;
        rotation = rot;
        maxRotation = maxRot;
        staminaCost = cost;
        baseScore = score;
    }
}

[Serializable]
public class ScheduledComp
{
    public string compName;       // 比赛名
    public string skaterName;     // 派遣的选手名
    public int compMonth;         // 比赛在哪月
    public CompType compType;     // 级别（用来生成对手强度）
    public int award;             // 奖金
    public string compID;
}

[Serializable]
public class ActiveComp
{
    public string compID;
    public List<string> skaterNames = new List<string>();
    public CompType compType;
    public int award;
    public string compName;
    public int remainingDays;  // 剩余天数
}

[Serializable]
public class SaveData
{
    public List<Skater> Skaters = new List<Skater>();
    public int gameDay;
    public int gameMonth;
    public int gameYear;
    public int money;
    public bool newGame = true;
    public createType createType = createType.No;
    public ClubData club=new ClubData();
    public int createTime;
    public List<ScheduledComp> scheduledComps = new List<ScheduledComp>();
    public List<ActiveComp> activeComps = new List<ActiveComp>();
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public SaveData currentSaveData;
    private string saveFileName;
    private string savePath;
    private Coroutine dayLoopCoroutine;
    public static bool isPause = false;

    #region 初始化
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFileName = "GameSave.json";
            savePath = Path.Combine(UnityEngine.Application.persistentDataPath, saveFileName);
            if (File.Exists(savePath))
            {
                LoadSaveData();
            }
            else
            {
                InitDefaultSaveData();
            }

            if (currentSaveData.newGame == true) StartTutorial();

            dayLoopCoroutine = StartCoroutine(DayLoop());

        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region 数据初始化
    private void InitDefaultSaveData()
    {
        currentSaveData = new SaveData();
        currentSaveData.gameDay = 1;
        currentSaveData.gameMonth = 1;
        currentSaveData.gameYear = 1;
        currentSaveData.money = 100000;
        Skater s = new Skater();
        s.name = "甘棠";
        s.sex = Sex.girl;
        s.stamina = 100;
        s.jump = Random.Range(1, 100);
        s.dance = Random.Range(1, 100);
        s.age = Random.Range(3, 12);
        s.spin = Random.Range(1, 100);
        s.fans = Random.Range(1, 100);
        s.speed = Random.Range(320, 401);
        s.personality = personType.lazy;

        Skater a = new Skater();
        a.name = "老邓";
        a.sex = Sex.boy;
        a.stamina = 100;
        a.jump = Random.Range(1, 100);
        a.dance = Random.Range(1, 100);
        a.age = Random.Range(10, 16);
        a.spin = Random.Range(1, 100);
        a.fans = Random.Range(1, 100);
        a.speed = Random.Range(320, 401);
        a.personality = personType.lazy;
        currentSaveData.Skaters.Add(s);
        currentSaveData.Skaters.Add(a);
        SaveAllData();
    }
    #endregion

    #region 读档
    public void LoadSaveData()
    {
        try
        {
            string persistentDataPath = UnityEngine.Application.persistentDataPath;//��ȡ�浵��·��
            string currentSaveDataPath = Path.Combine(persistentDataPath, "GameSave.json");//��ȡ�浵������·��

            if (File.Exists(currentSaveDataPath))
            {
                string json = File.ReadAllText(currentSaveDataPath);//��ȡjson�ļ����������ֶΣ�
                var data = JsonUtility.FromJson<SaveData>(json);//��json���л��������л�����������

                if (data != null)
                {
                    currentSaveData = data;//�����ݶ�����ǰ��Ϸ��,currentSaveData��ȫ������
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("����ʧ�ܣ�" + e.Message);
        }
    }
    #endregion

    #region 保存
    public void SaveAllData()
    {
        try
        {
            string persistentDataPath = UnityEngine.Application.persistentDataPath;//��ȡ�浵��·��
            string currentSaveDataPath = Path.Combine(persistentDataPath, "GameSave.json");//��ȡ�浵������·��

            string json = JsonUtility.ToJson(currentSaveData);
            File.WriteAllText(currentSaveDataPath, json);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("�浵ʧ�ܣ�" + e.Message);
        }
    }
    #endregion

    #region 新游戏
    public void startNewGame()
    {
        InitDefaultSaveData();
    }
    #endregion

    #region 回合制
    IEnumerator DayLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            AdvanceDay();
            if (UIManager.Instance != null) UIManager.Instance.RefreshUI();
        }
    }

    private void AdvanceDay()
    {
        if (currentSaveData.gameDay == 1) CheckCompetitions();
        currentSaveData.gameDay += 1;
        if(currentSaveData.createTime!=0)currentSaveData.createTime--;
        CheckActiveComps();
        LevelUPManager.Instance?.CheckUpgrade();

        if (currentSaveData.gameMonth == 1 || currentSaveData.gameMonth == 3 || currentSaveData.gameMonth == 5 || currentSaveData.gameMonth == 7 || currentSaveData.gameMonth == 8 || currentSaveData.gameMonth == 10 || currentSaveData.gameMonth == 12)
        {
            if (currentSaveData.gameDay > 31)
            {
                currentSaveData.gameMonth += 1;
                currentSaveData.gameDay = 1;
                SaveAllData();
            }
        }

        if (currentSaveData.gameMonth == 2)
        {
            if (currentSaveData.gameDay > 28)
            {
                currentSaveData.gameMonth += 1;
                currentSaveData.gameDay = 1;
                SaveAllData();
            }
        }

        if (currentSaveData.gameMonth == 4 || currentSaveData.gameMonth == 6 || currentSaveData.gameMonth == 9 || currentSaveData.gameMonth == 11)
        {
            if (currentSaveData.gameDay > 30)
            {
                currentSaveData.gameMonth += 1;
                currentSaveData.gameDay = 1;
                SaveAllData();
            }
        }

        if (currentSaveData.gameMonth > 12)
        {
            currentSaveData.gameMonth = 1;
            currentSaveData.gameYear += 1;
            SkaterManager.Instance.AgeHelp();
            SaveAllData();
        }

    }
    #endregion

    #region 暂停和继续
    // ��ͣ
    public void PauseGame()
    {
        if (isPause) return;  // 已经暂停了就别再停
        isPause = true;
        if (dayLoopCoroutine != null)
            StopCoroutine(dayLoopCoroutine);
        dayLoopCoroutine = null;
    }

    // ����
    public void ResumeGame()
    {
        if (!isPause) return;  // 已经在跑了就别再开
        isPause = false;
        dayLoopCoroutine = StartCoroutine(DayLoop());
    }
    #endregion

    #region 新手引导
    private void StartTutorial()
    {
        StartCoroutine(WaitForTutorial());
    }

    IEnumerator WaitForTutorial()
    {
        while (PopupManager.Instance == null)
        {
            yield return null;
        }
        List<string> Message = new List<string>();
        Message.Add("你是一名有着花滑爱好的富二代，恳求父亲很久才终于拿到这一家滑冰俱乐部的经营权。");
        Message.Add("虽然是富二代，但是父亲告诉你，如果赚不到钱，也不用回去找他要；这个俱乐部是你自己非要自负盈亏的。");
        Message.Add("而在你拿到这家俱乐部时，还有两名为追梦而不愿意离去的选手。");
        Message.Add("选手在平时也会努力训练，但很多时候也需要你给他们专业性的指导。");
        Message.Add("点击选手，可查看个人信息；点击场馆设施可选择升级；后续也会开放更多游戏内容，敬请期待");
        PopupManager.Instance.MessagePop(Message);
        currentSaveData.newGame = false;    }
    #endregion

    #region 检查是否预约
    private void CheckCompetitions()
    {
        List<ScheduledComp> DeleteList = new List<ScheduledComp>();
        // compID → 选手名字列表
        Dictionary<string, List<string>> compGroups = new Dictionary<string, List<string>>();
        // compID → 比赛信息
        Dictionary<string, ScheduledComp> compInfos = new Dictionary<string, ScheduledComp>();

        foreach (var C in currentSaveData.scheduledComps)
        {
            if (C.compMonth == currentSaveData.gameMonth)
            {
                // 按compID分组
                if (!compGroups.ContainsKey(C.compID))
                {
                    compGroups[C.compID] = new List<string>();
                    compInfos[C.compID] = C;
                }
                compGroups[C.compID].Add(C.skaterName);
                DeleteList.Add(C);
            }
        }

        // 每场比赛创建一个ActiveComp
        foreach (var pair in compGroups)
        {
            ActiveComp ac = new ActiveComp();
            ac.compID = pair.Key;
            ac.compName = compInfos[pair.Key].compName;
            ac.compType = compInfos[pair.Key].compType;
            ac.award = compInfos[pair.Key].award;
            ac.skaterNames = pair.Value;
            ac.remainingDays = 30;
            currentSaveData.activeComps.Add(ac);

            // 设选手状态
            foreach (var name in pair.Value)
            {
                foreach (var S in currentSaveData.Skaters)
                {
                    if (S.name == name) S.isCompeting = true;
                }
            }
        }

        foreach (var item in DeleteList)
        {
            currentSaveData.scheduledComps.Remove(item);
        }
    }

    private void CheckActiveComps()
    {
        List<ActiveComp> finished = new List<ActiveComp>();
        foreach (var ac in currentSaveData.activeComps)
        {
            ac.remainingDays--;
            if (ac.remainingDays <= 0)
            {
                // 收集选手
                List<Skater> skaters = new List<Skater>();
                foreach (var name in ac.skaterNames)
                {
                    foreach (var s in currentSaveData.Skaters)
                    {
                        if (s.name == name) skaters.Add(s);
                    }
                }
                // 结算
                CompetitionManager.Instance.SettleCompetition(skaters, ac);
                // 重置状态
                foreach (var s in skaters)
                {
                    s.isCompeting = false;
                }
                finished.Add(ac);
            }
        }
        foreach (var f in finished)
        {
            currentSaveData.activeComps.Remove(f);
        }
    }
    #endregion
}