using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
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
    public personType personality;
    public currentType current = currentType.training;
    public trainType trainingType = trainType.jump;
    public List<jumpType> jumpTypes = new List<jumpType>();
    public List<skill> skills = new List<skill>();
}

public enum createType
{
    low,
    middle,
    high
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
    Jump1,
    jump2
}

public enum skill
{
    Skill1,
    Skill2
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
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public SaveData currentSaveData;
    private string saveFileName;
    private string savePath;
    private Coroutine dayLoopCoroutine;
    public bool isPause = false;

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
        currentSaveData.gameDay += 1;
        if (currentSaveData.gameMonth == 1 || currentSaveData.gameMonth == 3 || currentSaveData.gameMonth == 5 || currentSaveData.gameMonth == 7 || currentSaveData.gameMonth == 8 || currentSaveData.gameMonth == 10 || currentSaveData.gameMonth == 12)
        {
            if (currentSaveData.gameDay > 31)
            {
                currentSaveData.gameMonth += 1;
                currentSaveData.gameDay = 1;
            }
        }

        if (currentSaveData.gameMonth == 2)
        {
            if (currentSaveData.gameDay > 28)
            {
                currentSaveData.gameMonth += 1;
                currentSaveData.gameDay = 1;
            }
        }

        if (currentSaveData.gameMonth == 4 || currentSaveData.gameMonth == 6 || currentSaveData.gameMonth == 9 || currentSaveData.gameMonth == 11)
        {
            if (currentSaveData.gameDay > 30)
            {
                currentSaveData.gameMonth += 1;
                currentSaveData.gameDay = 1;
            }
        }

        if (currentSaveData.gameMonth > 12)
        {
            currentSaveData.gameMonth = 1;
            currentSaveData.gameYear += 1;
            SkaterManager.Instance.AgeHelp();
        }
    }
    #endregion

    #region 暂停和继续
    // ��ͣ
    public void PauseGame()
    {
        isPause = true;
        if (dayLoopCoroutine != null)
            StopCoroutine(dayLoopCoroutine);
    }

    // ����
    public void ResumeGame()
    {
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
}