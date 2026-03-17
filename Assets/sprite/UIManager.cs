using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Button SaveDataBtn;

    private void Awake()
    {
        // ��������GameManagerһ��
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

    private void Start()
    {
        SaveDataBtn.onClick.AddListener(OnClicked);
        SaveDataBtn.GetComponentInChildren<TextMeshProUGUI>().text = "保存";
        RefreshUI();
    }

    public void RefreshUI()
    {
        // ��GameManager.Instance.currentSaveData�����ݣ�������
        int money = GameManager.Instance.currentSaveData.money;
        int date = GameManager.Instance.currentSaveData.gameDay;
        int month = GameManager.Instance.currentSaveData.gameMonth;
        int year = GameManager.Instance.currentSaveData.gameYear;
        if (moneyText != null) moneyText.text = $"持有金钱：{money}";
        if (dateText != null) dateText.text = $"{year}年{month}月{date}日";
    }

    private void OnClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.SaveAllData();
        List<string> msg = new List<string> { "保存成功!" };
        PopupManager.Instance.MessagePop(msg);
    }
}
