using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SkaterManager : MonoBehaviour
{
    public static SkaterManager Instance { get; private set; }
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
                    s.name = "甘棠";//随机名字吧，读取一下json字段之类的？
                    int k = Random.Range(0, 2);
                    if (k == 0) { s.sex = Sex.girl; }
                    else { s.sex = Sex.boy; }
                    s.stamina = 100;
                    s.jump = Random.Range(1, lowNumber);
                    s.dance = Random.Range(1, lowNumber);
                    s.age = Random.Range(10, 18);
                    s.spin = Random.Range(1, lowNumber);
                    s.fans = Random.Range(1, lowNumber);
                    s.speed = Random.Range(320, 401);
                    CandidateSkater.Add(s);
                }
                break;
            case createType.middle:
                for (int i = Random.Range(2, 5); i > 0; i--)
                {
                    Skater s = new Skater();
                    s.name = "甘棠";//随机名字吧，读取一下json字段之类的？
                    int k = Random.Range(0, 2);
                    if (k == 0) { s.sex = Sex.girl; }
                    else { s.sex = Sex.boy; }
                    s.stamina = 100;
                    s.jump = Random.Range(1, middleNumber);
                    s.dance = Random.Range(1, middleNumber);
                    s.age = Random.Range(10, 18);
                    s.spin = Random.Range(1, middleNumber);
                    s.fans = Random.Range(1, middleNumber);
                    s.speed = Random.Range(320, 401);
                    CandidateSkater.Add(s);
                }
                break;
            case createType.high:
                for (int i = Random.Range(2, 5); i > 0; i--)
                {
                    Skater s = new Skater();
                    s.name = "甘棠";//随机名字吧，读取一下json字段之类的？
                    int k = Random.Range(0, 2);
                    if (k == 0) { s.sex = Sex.girl; }
                    else { s.sex = Sex.boy; }
                    s.stamina = 100;
                    s.jump = Random.Range(1, highNumber);
                    s.dance = Random.Range(1, highNumber);
                    s.age = Random.Range(10, 18);
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

    #region 退役控制器+删除函数
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
}
