using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header  ("面板")]
    [SerializeField] private GameObject MenuPanel;
    [SerializeField] private GameObject Mask;
    [SerializeField] private GameObject CompPanel;
    [SerializeField] private GameObject LevelUpPanel;
    [SerializeField] private GameObject ShopPanel;
    [SerializeField] private GameObject SkaterPanel;
    [SerializeField] private GameObject SkaterCreatePanel;
    [SerializeField] private GameObject SkaterChildPanel;
    [SerializeField] private GameObject SettingPanel;
    [SerializeField] private GameObject PrivacyPanel;

    [Header  ("按钮")]
    [SerializeField] private Button OpenPanelBtn;
    [SerializeField] private Button ClosedPanelBtn;
    [SerializeField] private Button CompertationBtn;
    [SerializeField] private Button LevelUpBtn;
    [SerializeField] private Button ShopBtn;
    [SerializeField] private Button SkaterBtn;
    [SerializeField] private Button SetActiveSkaterBtn;//打开子菜单
    [SerializeField] private Button CreateSkaterBtn;
    [SerializeField] private Button SettingBtn;
    [SerializeField] private Button MenuBtn;
    [SerializeField] private Button PrivacyBtn;

    [Header("红点红点！！")]
    [SerializeField] private GameObject RedTips;

    private GameObject currentPanel;
    private Stack<GameObject> Panel = new Stack<GameObject>();

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

    // Start is called before the first frame update
    void Start()
    {
        OpenPanelBtn.onClick.AddListener(() => OpenPanel(MenuPanel));
        ClosedPanelBtn.onClick.AddListener(ClosePanel);
        CompertationBtn.onClick.AddListener(() => OpenPanel(CompPanel));
        LevelUpBtn.onClick.AddListener(()=>OpenPanel(LevelUpPanel));
        ShopBtn.onClick.AddListener(() => OpenPanel(ShopPanel));
        SkaterBtn.onClick.AddListener(() => OpenPanel(SkaterPanel));
        SettingBtn.onClick.AddListener(() => OpenPanel(SettingPanel));
        SetActiveSkaterBtn.onClick.AddListener(() => OpenPanel(SkaterChildPanel));
        CreateSkaterBtn.onClick.AddListener(() => OpenPanel(SkaterCreatePanel));
        MenuBtn.onClick.AddListener(() => OpenPanel(MenuPanel));
        PrivacyBtn.onClick.AddListener(() => OpenPanel(PrivacyPanel));
    }

    void Update()
    {
        LevelUPManager.Instance?.CheckIsUpgrade();
        if (LevelUPManager.LevelUp && !RedTips.activeSelf) RedTips.SetActive(true);
        else if (!LevelUPManager.LevelUp && RedTips.activeSelf) RedTips.SetActive(false);
    }

    public void OpenPanel(GameObject newPanel)
    {
        OpenPanelBtn.enabled = false;
        GameManager.Instance?.PauseGame();
        Mask.SetActive(true);
        if (currentPanel != null)
        {
            Panel.Push(currentPanel);
            currentPanel.SetActive(false);
        }
        newPanel.SetActive(true);
        currentPanel = newPanel;
    }

    public void ClosePanel()
    {
        currentPanel.SetActive(false);
        if (Panel.Count > 0)
        {
            currentPanel = Panel.Pop();  // 拿出上一个
            currentPanel.SetActive(true);
        }
        else
        {
            Mask.SetActive(false);
            OpenPanelBtn.enabled = true;
            currentPanel = null;
            GameManager.Instance?.ResumeGame();
        }
    }

}
