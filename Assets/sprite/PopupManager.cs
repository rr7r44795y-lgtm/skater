using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }
    
    public GameObject popupPrefab;
    public GameObject messagePrefab;
    public GameObject CompPopup;
    public GameObject ChoosePop;

    [Header("属性面板")]
    [SerializeField] private GameObject skaterInfoPanel;
    [SerializeField] private GameObject MessagePopRoot;
    [SerializeField] private GameObject MaskPanel;
    [SerializeField] private GameObject SkaterInfoMaskPanel;
    [SerializeField] private TextMeshProUGUI jumpText;
    [SerializeField] private TextMeshProUGUI spinText;
    [SerializeField] private TextMeshProUGUI danceText;
    [SerializeField] private TextMeshProUGUI fansText;
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private TextMeshProUGUI sexText;
    [SerializeField] private TextMeshProUGUI personalityText;
    [SerializeField] private Button TrainingBtn;
    [SerializeField] private TMP_Dropdown AutoTraining;
    [SerializeField] private Image RoleImage;
    [SerializeField] private Button CloseBtn;

    [Header("跳跃、技能、生平的scrollView")]
    [SerializeField] private TextMeshProUGUI LiveText;//生平
    [SerializeField] private GameObject jumpPrefab;//jump的预制体,其实就是个名字
    [SerializeField] private Transform Root;
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private GameObject SkillInfoPanel;
    [SerializeField] private Button jumpBtn;
    [SerializeField] private Button skillBtn;
    [SerializeField] private Button ShowBtn;
    [SerializeField] private GameObject Change;
    [SerializeField] private Button ChangeBtn;//ShowList修改
    [SerializeField] private GameObject ShowListPrefab;
    [Tooltip("showlist里选择跳跃的根物体")]
    [SerializeField] private GameObject PanelToPickJump;
    [Tooltip("showlist里选择跳跃的滑动视图，用于盛放跳跃的预制体")]
    [SerializeField] private GameObject PanelToPickJumpContent;
    [Tooltip("选择跳跃时的清空选项，清空当前坑位列表")]
    [SerializeField] private Button ClearBtn;
    [Tooltip("选择节目单时标题的text")]
    [SerializeField] private TextMeshProUGUI ShowListText;

    [Header("弹窗相关")]
    [SerializeField] private Button BackBtn;
    [SerializeField] private Button ConFirmBtn;
    [SerializeField] private Button SaveBtn;
    [SerializeField] private Button pickJumpMaskBtn;
    [SerializeField] private Button pickJumpConfirmBtn;

    private GameObject currentMessage;
    private bool isClicking = false;
    private bool messageActive =false;
    private List<string> messageQueue =new List<string>();
    private int Index = 0;
    private Skater currentSkater;
    private System.Action onConfirm;
    private System.Action onCancel;
    private GameObject compInstance;

    private enum TabType { Jump, Show, Skill }
    private TabType currentTab;

    #region 初始化
    private void Awake(){
      if(Instance==null)
      {
        Instance=this;
        DontDestroyOnLoad(gameObject);
      }else{
        Destroy(gameObject);
      }

        BackBtn.onClick.AddListener(ClosedPanel);
        CloseBtn.onClick.AddListener(ClosedPanel);
    }

    private Vector2 downPos;

    private void Update()
    {
        if (!messageActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            downPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && !isClicking)
        {
            float dist = Vector2.Distance(downPos, Input.mousePosition);
            if (dist < 10f)
            {
                isClicking = true;
                ShowNextMessage();
                StartCoroutine(ResetClick());
            }
        }
    }
    #endregion

    #region TMPro弹窗
    List<GameObject> activePopups = new List<GameObject>();

    public void TMProPop(string name, string type, int num)
    {
        string text = $"{name} {type}{(num > 0 ? "+" : "")} {num}";
        StartCoroutine(DelayedPop(text, activePopups.Count * 0.5f));
    }

    IEnumerator DelayedPop(string text, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var p in activePopups)
        {
            if (p != null) p.transform.localPosition += new Vector3(0, 50, 0);
        }

        GameObject popup = Instantiate(popupPrefab, transform);
        popup.GetComponentInChildren<TextMeshProUGUI>().text = text;
        activePopups.Add(popup);
        Destroy(popup, 3f);
        StartCoroutine(RemoveFromList(popup, 2f));
    }

    IEnumerator RemoveFromList(GameObject popup, float delay)
    {
        yield return new WaitForSeconds(delay);
        activePopups.Remove(popup);
    }
    #endregion

    #region 消息对话弹窗
    public void MessagePop( List<string> messages){
      MaskPanel.SetActive(true);
      GameManager.Instance?.PauseGame();
      messageQueue = messages;
      Index = 0;
      ShowNextMessage();
    }
      
    IEnumerator ResetClick()
    {
        yield return new WaitForSeconds(0.5f);
        isClicking = false;
    }
      
    private void ShowNextMessage(){
        if (currentMessage != null) Destroy(currentMessage);
      
         if (Index<messageQueue.Count){
            currentMessage = Instantiate(messagePrefab,MessagePopRoot.transform);
            currentMessage.GetComponentInChildren<TextMeshProUGUI>().text=$"{messageQueue[Index]}";
            Index++;
            messageActive = true;
         }else
         {
            messageActive = false;
            TryHideMask();
            currentMessage =null;
         }
    }
    #endregion

    #region 属性弹窗
    public void SkaterInfoPop(Skater skater){
        GameManager.Instance?.PauseGame();
        TrainingBtn.GetComponentInChildren<TextMeshProUGUI>().text = skater.skaterType == skaterType.active ? "训练" : "邀请";
        TrainingBtn.onClick.RemoveAllListeners();

        if (skater.skaterType == skaterType.active)
        {
            AutoTraining.gameObject.SetActive(true);
            TrainingBtn.onClick.AddListener(() => {
                List<string> msg = new List<string>();
                msg.Add("当前未开放此功能!");
                MessagePop(msg);
            });
        }
        else
        {
            AutoTraining.gameObject.SetActive(false);
            TrainingBtn.onClick.AddListener(() => {
                currentSkater.skaterType = skaterType.active;
                if (!GameManager.Instance.currentSaveData.Skaters.Contains(currentSkater))
                {
                    if(GameManager.Instance.currentSaveData.Skaters.Count<= GameManager.Instance.currentSaveData.club.MaxSkater)
                    {
                        GameManager.Instance.currentSaveData.Skaters.Add(currentSkater);
                        SkaterManager.Instance.CandidateSkater.Remove(currentSkater);
                        List<string> msg = new List<string>();
                        msg.Add("邀请成功");
                        MessagePop(msg);
                    }
                    else
                    {
                        List<string> msg = new List<string>();
                        msg.Add("当前俱乐部人数已达上限，请升级后再试!");
                        MessagePop(msg);
                    }
                }
                else
                {
                    List<string> msg = new List<string>();
                    msg.Add("该选手已在队伍中!");
                    MessagePop(msg);
                }
                ClosedPanel();
            });
        }

        skaterInfoPanel.SetActive(true);
        SkaterInfoMaskPanel.SetActive(true);

        NameText.text = skater.name;
        fansText.text = "粉丝数:  "+skater.fans.ToString();
        danceText.text = "dance:  "+skater.dance.ToString();
        spinText.text ="spin:  "+ skater.spin.ToString();
        jumpText.text = "jump:  "+skater.jump.ToString();
        ageText.text = "年龄:"+skater.age.ToString();
        sexText.text = "性别："+skater.sex.ToString();
        personalityText.text = "性格："+skater.personality.ToString();
        string live=" ";
        foreach(var s in skater.LiveList)
        {
            live += s;
        }
        LiveText.text = live;
        currentSkater = skater;
        List<string> options;
        if (AutoTraining == null) AutoTraining = GetComponent<TMP_Dropdown>();
        AutoTraining.ClearOptions();
        if (skater.jumpTypes.Count >= 8)
        {
            options = new List<string> { "Toeloop", "Salchow", "Loop", "Flip", "Lutz", "Axel", "Spin", "StepSequence", "属性:spin", "属性:dance" };
        }
        else
        {
            options = new List<string> { "jump", "spin", "dance" };
        }
        AutoTraining.AddOptions(options);
        AutoTraining.onValueChanged.RemoveAllListeners();
        AutoTraining.onValueChanged.AddListener((index) => {
                if (skater.jumpTypes.Count >= 8)
                {
                    if (index <= 7)
                    {
                        skater.trainingJump = (jumpType)index;
                        skater.trainingType = trainType.jump;
                    }
                    else if (index == 8)
                    {
                        skater.trainingType = trainType.spin;
                    }
                    else if (index == 9)
                    {
                        skater.trainingType = trainType.dance;
                    }
                }
                else
                {
                    skater.trainingType = (trainType)index;
                }
            });
        jumpBtn.onClick.RemoveAllListeners();
        ShowBtn.onClick.RemoveAllListeners();
        skillBtn.onClick.RemoveAllListeners();
        ChangeBtn.onClick.RemoveAllListeners();
        jumpBtn.onClick.AddListener(() => JumpTab(skater));
        ShowBtn.onClick.AddListener(() => ShowTab(skater));
        skillBtn.onClick.AddListener(() => SkillTab(skater));
        ChangeBtn.onClick.AddListener(() => CompetitionManager.Instance.AutoShowList(skater));
        currentTab = TabType.Show;
        JumpTab(skater);
    }

    private void JumpTab(Skater s)
    {
        if (currentTab == TabType.Jump) return;
        currentTab = TabType.Jump;
        if (Change != null) Change.SetActive(false);
        foreach(Transform child in Root.transform)
        {
            Destroy(child.gameObject);
        }

        foreach(var J in s.jumpTypes)
        {
            GameObject card = Instantiate(jumpPrefab, Root.transform);
            card.GetComponent<JumpDataUI>().Init(J);
        }
    }

    public void RefreshShowList(Skater s)
    {
        currentTab = TabType.Jump;
        ShowTab(s);
    }

    private void ShowTab(Skater s)
    {
        if (currentTab == TabType.Show) return;
        currentTab = TabType.Show;
        if (Change != null) Change.SetActive(true);

        foreach (Transform child in Root.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 10; i++)
        {
            GameObject card = Instantiate(ShowListPrefab, Root.transform);
            string display = (s.ShowList[i].jump.Count > 0) ? ShowElementToString(s.ShowList[i]) : "空";
            card.GetComponentInChildren<TextMeshProUGUI>().text = $"{i + 1}. {display}";

            int index = i; // 闭包要用局部变量
            card.GetComponent<Button>().onClick.AddListener(() => JumpPickPop(s, index));
        }
    }

    private string ShowElementToString(ShowElement element)
    {
        string combo = "";
        for (int j = 0; j < element.jump.Count; j++)
        {
            if (j > 0) combo += "+";
            combo += JumpShortName(element.jump[j]);
        }
        return combo;
    }

    private string JumpShortName(JumpData j)
    {
        string name = "";
        switch (j.jumpName)
        {
            case jumpType.Toeloop: name = "T"; break;
            case jumpType.Salchow: name = "S"; break;
            case jumpType.Loop: name = "Lo"; break;
            case jumpType.Flip: name = "F"; break;
            case jumpType.Lutz: name = "Lz"; break;
            case jumpType.Axel: name = "A"; break;
            case jumpType.Spin: name = "Sp"; break;
            case jumpType.StepSequence: name = "StSq"; break;
        }
        return $"{j.rotation}{name}";
    }

    private void SkillTab(Skater s)
    {
        if (currentTab == TabType.Skill) return;
        currentTab = TabType.Skill;
        if (Change != null) Change.SetActive(false);
        foreach (Transform child in Root.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var skill in s.skills)
        {
            GameObject card = Instantiate(skillPrefab, Root.transform);
            card.GetComponent<SkillDataUI>().Init(skill,SkillInfoPanel);
        }
    }
    #endregion

    #region 公共方法
    private void TryHideMask()
    {
        if (skaterInfoPanel.activeSelf) return;
        if (ChoosePop.activeSelf) return;
        if (compInstance != null) return;
        if (messageActive) return;

        MaskPanel.SetActive(false);
        GameManager.Instance?.ResumeGame();
    }

    private void ClosedPanel()
    {
        skaterInfoPanel.SetActive(false);
        SkaterInfoMaskPanel.SetActive(false);
        ChoosePop.SetActive(false);
        currentSkater = null;
        TryHideMask();
    }

    #endregion

    #region （?确认：取消）选择弹窗
    public void ChoicePop(string message,string confirm,string cancel, System.Action confirmAction, System.Action cancelAction)
    {
        GameManager.Instance?.PauseGame();
        ChoosePop.GetComponentInChildren<TextMeshProUGUI>().text = message;
        ConFirmBtn.GetComponentInChildren<TextMeshProUGUI>().text = confirm;
        SaveBtn.GetComponentInChildren<TextMeshProUGUI>().text = cancel;

        ConFirmBtn.onClick.RemoveAllListeners();
        SaveBtn.onClick.RemoveAllListeners();

        ConFirmBtn.onClick.AddListener(() => {
            confirmAction?.Invoke();
            ChoosePop.SetActive(false);
            TryHideMask();
        });

        SaveBtn.onClick.AddListener(() => {
            cancelAction?.Invoke();
            ChoosePop.SetActive(false);
            TryHideMask();
        });

        MaskPanel.SetActive(true);
        ChoosePop.SetActive(true);
    }
    #endregion

    #region 比赛弹窗
    public void CompPop(string name,string msg,System.Action onClose = null)
    {
        GameManager.Instance?.PauseGame();
        if (compInstance != null) { Destroy(compInstance); compInstance = null; }
        MaskPanel.SetActive(true);
        compInstance = Instantiate(CompPopup, transform);
        compInstance.GetComponent<CompResultUI>().Init(name, msg,onClose);
    }

    public void OnCompPopClosed()
    {
        compInstance = null;
        TryHideMask();
    }
    #endregion

    #region 多选择弹窗，选择跳跃
    private void JumpPickPop(Skater s, int slotIndex)
    {
        foreach (Transform child in PanelToPickJumpContent.transform)
        {
            Destroy(child.gameObject);
        }
        PanelToPickJump.SetActive(true);
        pickJumpMaskBtn.onClick.RemoveAllListeners();
        pickJumpMaskBtn.onClick.AddListener(ClosePickPop);
        ClearBtn.onClick.RemoveAllListeners();
        List<JumpData> SelectJump = new List<JumpData>();
        foreach (var item in s.jumpTypes)
        {
            GameObject card = Instantiate(jumpPrefab, PanelToPickJumpContent.transform);
            card.GetComponent<JumpDataUI>().Pick(SelectJump, item);
        }
        ShowListText.text = "请选择你要编排的节目~";
        pickJumpConfirmBtn.onClick.RemoveAllListeners();
        pickJumpConfirmBtn.onClick.AddListener(() => PickPopConfirm(SelectJump, s, slotIndex));
        ClearBtn.onClick.AddListener(() => {
            SelectJump.Clear();
            foreach (Transform child in PanelToPickJumpContent.transform)
            {
                var ui = child.GetComponent<JumpDataUI>();
                if (ui != null) ui.ResetSelect();
            }
            ShowListText.text = "请选择你要编排的节目~";
        });
    }

    public void RefreshText(List<JumpData> list)
    {
        string text = "当前选择跳跃为:";
        foreach(var item in list)
        {
            text += $"{item.rotation}" + CompetitionManager.Instance.GetText(item.jumpName)+"+";
        }
        text = text.TrimEnd('+');
        ShowListText.text = text;
    }

    private void PickPopConfirm(List<JumpData> J, Skater s, int slotIndex)
    {
        var (allow, reason) = CheckIsAllow(J, s, slotIndex);
        if (!allow)
        {
            List<string> msg = new List<string> { reason };
            MessagePop(msg);
            return;
        }

        ShowElement se = new ShowElement();
        foreach (var item in J)
        {
            se.jump.Add(item);
        }
        s.ShowList[slotIndex] = se;  // 漏了
        ClosePickPop();               // 漏了
        RefreshShowList(s);
    }

    private void ClosePickPop()
    {
        PanelToPickJump.SetActive(false);
    }

    private (bool, string) CheckIsAllow(List<JumpData> newJumps, Skater s, int slotIndex)
    {
        int jumpTime = 0, stepTime = 0, spinTime = 0;
        int combineJump2 = 0, combineJump3 = 0;
        int TP = 0, SC = 0, LP = 0, FP = 0, LZ = 0, AL = 0;

        // 统计现有节目单，跳过要替换的坑
        for (int idx = 0; idx < s.ShowList.Count; idx++)
        {
            if (idx == slotIndex) continue;
            var item = s.ShowList[idx];
            if (item.jump.Count == 0) continue;

            if (item.jump.Count == 2) combineJump2++;
            else if (item.jump.Count == 3) combineJump3++;

            for (int i = 0; i < item.jump.Count; i++)
            {
                var jump = item.jump[i];

                if (i == 0 || jump.jumpName == jumpType.Spin || jump.jumpName == jumpType.StepSequence)
                {
                    switch (jump.jumpName)
                    {
                        case jumpType.Toeloop: TP++; jumpTime++; break;
                        case jumpType.Salchow: SC++; jumpTime++; break;
                        case jumpType.Loop: LP++; jumpTime++; break;
                        case jumpType.Flip: FP++; jumpTime++; break;
                        case jumpType.Lutz: LZ++; jumpTime++; break;
                        case jumpType.Axel: AL++; jumpTime++; break;
                        case jumpType.Spin: spinTime++; break;
                        case jumpType.StepSequence: stepTime++; break;
                    }
                }
            }
        }

        // 连跳第二三跳只能是 Toeloop 或 Loop
        for (int i = 1; i < newJumps.Count; i++)
        {
            if (newJumps[i].jumpName != jumpType.Toeloop && newJumps[i].jumpName != jumpType.Loop)
                return (false, "连跳第二三跳只能是Toeloop或Loop!");
        }

        // 连跳数量检查
        if (newJumps.Count == 3 && combineJump3 >= 1)
            return (false, "三连跳最多只能有1组!");
        if (newJumps.Count >= 2 && combineJump2 + combineJump3 >= 3)
            return (false, "连跳最多只能有3组!");

        // 跳跃总数
        if (newJumps[0].jumpName <= jumpType.Axel && jumpTime >= 7)
            return (false, "跳跃已达上限7个!");

        // Spin 和 StepSequence 上限
        if (newJumps[0].jumpName == jumpType.Spin && spinTime >= 1)
            return (false, "Spin最多只能有1个!");
        if (newJumps[0].jumpName == jumpType.StepSequence && stepTime >= 2)
            return (false, "StepSequence最多只能有2个!");

        // 同种跳跃最多2次
        switch (newJumps[0].jumpName)
        {
            case jumpType.Toeloop: if (TP >= 2) return (false, "Toeloop已使用2次!"); break;
            case jumpType.Salchow: if (SC >= 2) return (false, "Salchow已使用2次!"); break;
            case jumpType.Loop: if (LP >= 2) return (false, "Loop已使用2次!"); break;
            case jumpType.Flip: if (FP >= 2) return (false, "Flip已使用2次!"); break;
            case jumpType.Lutz: if (LZ >= 2) return (false, "Lutz已使用2次!"); break;
            case jumpType.Axel: if (AL >= 2) return (false, "Axel已使用2次!"); break;
        }

        return (true, "");
    }
    #endregion
}