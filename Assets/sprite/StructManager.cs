using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

// ===== 数据类 =====

// JSON 配置（静态模板，不会变）
[Serializable]
public class StructConfig
{
    public string structID;
    public string structName;
    public string imageID;
    public int maxCapacity;
    public int costPerUse;
    public int effectType;    // 0=jump, 1=spin, 2=dance, 3=stamina
    public int effectValue;
    public bool isCustom;
    public int customIncome;
    public int price;
    public string description;
}

[Serializable]
public class StructConfigList
{
    public List<StructConfig> structures;
}

// 存档数据（玩家建了什么、在哪个坑位）
[Serializable]
public class StructSaveData
{
    public string structID;
    public int slotIndex;     // 在哪个坑位
}

// ===== Manager =====

public class StructManager : MonoBehaviour
{
    public static StructManager Instance { get; private set; }

    [Header("坑位")]
    [SerializeField] private StructSlot[] slots;  // 场景里摆好的所有坑位

    [Header("建筑选择面板")]
    [SerializeField] private GameObject BuildPanel;
    [SerializeField] private GameObject BuildCardPrefab;
    [SerializeField] private Transform BuildPanelContent;
    [SerializeField] private Button BuildPanelCloseBtn;

    private StructConfigList configData;
    private int currentSlotIndex = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 关闭按钮
        if (BuildPanelCloseBtn != null)
            BuildPanelCloseBtn.onClick.AddListener(CloseBuildPanel);

        // 根据等级刷新坑位显示
        RefreshSlots();

        // 读档还原已建造的建筑
        RestoreBuildings();
    }

    // ===== 读取配置 =====
    private void LoadConfig()
    {
        TextAsset json = Resources.Load<TextAsset>("Structures");
        if (json != null)
            configData = JsonUtility.FromJson<StructConfigList>(json.text);
    }

    public StructConfig GetConfigByID(string id)
    {
        if (configData == null) return null;
        foreach (var c in configData.structures)
        {
            if (c.structID == id) return c;
        }
        return null;
    }

    // ===== 坑位管理 =====

    // 根据俱乐部等级控制显示几个坑
    public void RefreshSlots()
    {
        int max = GameManager.Instance.currentSaveData.club.MaxStruct;
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.SetActive(i < max);
        }
    }

    // 读档还原
    private void RestoreBuildings()
    {
        foreach (var save in GameManager.Instance.currentSaveData.structures)
        {
            if (save.slotIndex >= 0 && save.slotIndex < slots.Length)
            {
                StructConfig config = GetConfigByID(save.structID);
                if (config != null)
                {
                    slots[save.slotIndex].Build(config);
                }
            }
        }
    }

    // ===== 建造面板 =====

    // 坑位点击后调用
    public void OpenBuildPanel(int slotIndex)
    {
        currentSlotIndex = slotIndex;
        GameManager.Instance?.PauseGame();

        // 清旧卡片
        foreach (Transform child in BuildPanelContent)
        {
            Destroy(child.gameObject);
        }

        // 生成建筑卡片
        foreach (var config in configData.structures)
        {
            GameObject card = Instantiate(BuildCardPrefab, BuildPanelContent);
            card.GetComponent<StructCardUI>().Init(config);
        }

        BuildPanel.SetActive(true);
    }

    public void CloseBuildPanel()
    {
        BuildPanel.SetActive(false);
        currentSlotIndex = -1;
        GameManager.Instance?.ResumeGame();
    }

    // 选中某个建筑后确认建造
    public void ConfirmBuild(StructConfig config)
    {
        if (currentSlotIndex < 0) return;

        // 检查钱够不够
        if (GameManager.Instance.currentSaveData.money < config.price)
        {
            List<string> msg = new List<string> { "金币不足!" };
            PopupManager.Instance.MessagePop(msg);
            return;
        }

        // 检查建筑数上限
        if (GameManager.Instance.currentSaveData.structures.Count >=
            GameManager.Instance.currentSaveData.club.MaxStruct)
        {
            List<string> msg = new List<string> { "建筑数已达上限!" };
            PopupManager.Instance.MessagePop(msg);
            return;
        }

        PopupManager.Instance.ChoicePop($"确定要花费{config.price}建造{config.structName}吗?", "确定", "取消",
            () =>
            {
                // 扣钱
                GameManager.Instance.currentSaveData.money -= config.price;
                UIManager.Instance?.RefreshUI();

                // 存档
                StructSaveData save = new StructSaveData();
                save.structID = config.structID;
                save.slotIndex = currentSlotIndex;
                GameManager.Instance.currentSaveData.structures.Add(save);

                // 坑位显示建筑
                slots[currentSlotIndex].Build(config);

                // 关面板
                CloseBuildPanel();

                List<string> successMsg = new List<string> { $"成功建造了{config.structName}!" };
                PopupManager.Instance.MessagePop(successMsg);
            }, () =>
            {
                CloseBuildPanel();
            });       
    }

    // ===== 拆除 =====
    public void RemoveStructure(int slotIndex)
    {
        // 从存档移除
        StructSaveData toRemove = null;
        foreach (var save in GameManager.Instance.currentSaveData.structures)
        {
            if (save.slotIndex == slotIndex)
            {
                toRemove = save;
                break;
            }
        }
        if (toRemove != null)
        {
            GameManager.Instance.currentSaveData.structures.Remove(toRemove);
        }

        // 坑位重置
        slots[slotIndex].Remove();
    }

    // ===== 给 Walk 用的：获取坑位上的建筑配置 =====
    public StructConfig GetSlotConfig(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        return slots[slotIndex].GetConfig();
    }

    // 获取所有可用的建筑坑位的 Transform（给 Walk 选点用）
    public List<Transform> GetActiveStructTransforms()
    {
        List<Transform> list = new List<Transform>();
        int max = GameManager.Instance.currentSaveData.club.MaxStruct;
        for (int i = 0; i < slots.Length && i < max; i++)
        {
            if (slots[i].HasBuilding())
            {
                list.Add(slots[i].transform);
            }
        }
        return list;
    }
}
