using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClubType
{
    Low,
    Middle,
    High,
    Perfect
}

[Serializable]
public class ClubData
{
    public ClubType clubLevel=ClubType.Low;
    public int MaxSkater=6;
    public int MaxStruct=6;
    public int upgradeDays;
    public bool isUpgrading;
    public int AwardLocal;
    public int AwardDistrict;
    public int AwardProvincial;
    public int AwardNational;
    public int AwardInternational;
}

public class LevelUPManager : MonoBehaviour
{
    public static LevelUPManager Instance { get; private set; }

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

    private ClubType currentType;
    public static bool LevelUp = false;
    // Start is called before the first frame update
    void Start()
    {
        currentType = GameManager.Instance.currentSaveData.club.clubLevel;
}

    // 不用协程，放AdvanceDay里调
    public void CheckUpgrade()
    {
        if (!GameManager.Instance.currentSaveData.club.isUpgrading) return;
        GameManager.Instance.currentSaveData.club.upgradeDays--;
        var club = GameManager.Instance.currentSaveData.club;
        if (club.upgradeDays <= 0)
        {
            switch (currentType)
            {
                case ClubType.Low:
                    club.clubLevel = ClubType.Middle;
                    currentType = ClubType.Middle;
                    club.MaxSkater = 9;
                    club.MaxStruct = 10;
                    break;
                case ClubType.Middle:
                    club.clubLevel = ClubType.High;
                    currentType = ClubType.High;
                    club.MaxSkater = 12;
                    club.MaxStruct = 15;
                    break;
                case ClubType.High:
                    club.clubLevel = ClubType.Perfect;
                    currentType = ClubType.Perfect;
                    club.MaxSkater = 16;
                    club.MaxStruct = 20;
                    break;
            }
            club.isUpgrading = false;
            GameManager.Instance.currentSaveData.club.isUpgrading = false;
        }
    }

    //用来放在UI给玩家提示的
    public void CheckIsUpgrade()
    {
        LevelUp = false;
        var club = GameManager.Instance.currentSaveData.club;
        switch (club.clubLevel)
        {
            case ClubType.Low:
                if (GameManager.Instance.currentSaveData.Skaters.Count >= 6
                    && GameManager.Instance.currentSaveData.money >= 100000
                    && club.AwardLocal >= 1
                    && !club.isUpgrading)
                {
                    LevelUp = true;
                }
                break;
            case ClubType.Middle:
                if (GameManager.Instance.currentSaveData.Skaters.Count >= 9
                   && GameManager.Instance.currentSaveData.money >= 150000
                   && club.AwardDistrict >= 1    // 改成区级
                   && !club.isUpgrading)
                {
                    LevelUp = true;
                }
                break;
            case ClubType.High:
                if (GameManager.Instance.currentSaveData.Skaters.Count >= 12
                   && GameManager.Instance.currentSaveData.money >= 250000
                   && club.AwardProvincial >= 1  // 改成省级
                   && !club.isUpgrading)
                {
                    LevelUp = true;
                }
                break;
            case ClubType.Perfect:
                break;
        }
    }

    //点击升级按钮
    public void ChangeUpgrad()
    {
        var club = GameManager.Instance.currentSaveData.club;
        int M = 0;
        switch (currentType)
        {
            case ClubType.Low:
                M = 100000;
                break;
            case ClubType.Middle:
                M = 150000;
                break;
            case ClubType.High:
                M = 250000;
                break;
        }
        if (LevelUp)
        {
        PopupManager.Instance.ChoicePop($"当前升级需要30天|{M}金\n是否升级?", "确定", "取消", () =>
        {
            club.isUpgrading = true;
            club.upgradeDays = 30;
            GameManager.Instance.currentSaveData.money -= M;
            UIManager.Instance?.RefreshUI();
        },
        () =>
        {
        });
        }
        else
        {
            List<string> msg = new List<string>();
            msg.Add("当前还不能升级!");
            PopupManager.Instance.MessagePop(msg);
        }
    }
}
