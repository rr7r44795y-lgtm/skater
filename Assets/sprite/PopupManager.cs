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

    [Header("弹窗相关")]
    [SerializeField] private Button BackBtn;
    [SerializeField] private Button ConFirmBtn;
    [SerializeField] private Button SaveBtn;

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
    
    private void Update(){
       if(Input.GetMouseButtonDown(0)&&!isClicking&& messageActive) {
          isClicking =true;
          ShowNextMessage();
          StartCoroutine(ResetClick());
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
        jumpBtn.onClick.AddListener(() => JumpTab(skater));
        ShowBtn.onClick.AddListener(() => ShowTab(skater));
        skillBtn.onClick.AddListener(() => SkillTab(skater));
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
    
    //这个暂时不写，我在考虑要怎么去写这个东西，因为固定十个位置，就不需要foreach吧
    //感觉可以用别的写，然后更新的话也可以用别的方法
    //感觉能写那种一个萝卜一个坑，然后要更新的话就改这个坑里的东西就行了
    private void ShowTab(Skater s)
    {
        if (currentTab == TabType.Show) return;
        currentTab = TabType.Show;
        if (Change != null) Change.SetActive(true);
        foreach (Transform child in Root.transform)
        {
            Destroy(child.gameObject);
        }
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
        if (compInstance != null) { Destroy(compInstance); compInstance = null; }
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
    public void CompPop(string name,string msg)
    {
        GameManager.Instance?.PauseGame();
        if (compInstance != null) { Destroy(compInstance); compInstance = null; }
        MaskPanel.SetActive(true);
        compInstance = Instantiate(CompPopup, transform);
        compInstance.GetComponent<CompResultUI>().Init(name, msg);
    }
    #endregion

    #region 多选择弹窗,没写，别管了，嗯……难道这里传预制体列表比较好吗

    #endregion
}