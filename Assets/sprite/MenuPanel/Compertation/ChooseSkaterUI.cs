using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChooseSkaterUI : MonoBehaviour
{
    private Competition comp;
    public List<Skater> selectedList = new List<Skater>();

    [Header ("选手预制体")]
    [SerializeField] private GameObject skaterPrafab;
    [Header("预制体创建的根物体")]
    [SerializeField] private GameObject Root;
    [Header("比赛的Information")]
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI GroupText;
    [SerializeField] private TextMeshProUGUI LevelText;
    [SerializeField] private TextMeshProUGUI AwardText;
    [SerializeField] private TextMeshProUGUI FeeText;
    [Header("比赛的相关描述")]
    [SerializeField] private TextMeshProUGUI DescribeText;
    [Header("确定按钮")]
    [SerializeField] private Button confirmBtn;

    void Start()
    {
        confirmBtn.onClick.AddListener(Confirm);
    }

    public void Init(Competition c)
    {
        selectedList.Clear();
        comp = c;
        
        NameText.text = c.name;
        FeeText.text = $"报名费: {c.fee}";
        AwardText.text = $"奖金: {c.award}";
        LevelText.text = $"{c.compType}";
        GroupText.text = c.sexType== Sex.boy ? "男子组" : "女子组";
        DescribeText.text = c.description;

        foreach (Transform child in Root.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var s in GameManager.Instance.currentSaveData.Skaters)
        {
            if (s.age <= comp.MaxAge && s.age >= comp.MiniAge && s.sex == comp.sexType)
            {
                GameObject card = Instantiate(skaterPrafab, Root.transform);
                card.GetComponent<SkaterCard>().InitForCompetation(s, this);
            }
        }
    }

    private void Confirm()
    {
        if (selectedList.Count == 0) return;
        string names = "";
        foreach (var s in selectedList)
        {
            names += s.name + " ";
        }
        PopupManager.Instance.ChoicePop(
            $"确定派遣 {names}参赛吗？",
            "确定", "取消",
            () => {
                if (GameManager.Instance.currentSaveData.money >= comp.fee * selectedList.Count)
                {
                    GameManager.Instance.currentSaveData.money -= comp.fee * selectedList.Count;

                    if (GameManager.Instance.currentSaveData.gameMonth == comp.month)
                    {
                        ActiveComp ac = new ActiveComp();
                        ac.compID = comp.compID;
                        ac.compName = comp.name;
                        ac.compType = comp.compType;
                        ac.award = comp.award;
                        ac.remainingDays = 30;
                        foreach (var s in selectedList)
                        {
                            s.isCompeting = true;
                            ac.skaterNames.Add(s.name);
                        }
                        GameManager.Instance.currentSaveData.activeComps.Add(ac);
                        List<string> msg = new List<string> { "派遣成功！" };
                        PopupManager.Instance.MessagePop(msg);
                    }
                    else
                    {
                        foreach (var s in selectedList)
                        {
                            ScheduledComp sc = new ScheduledComp();
                            sc.compID = comp.compID;
                            sc.compName = comp.name;
                            sc.compMonth = comp.month;
                            sc.compType = comp.compType;
                            sc.award = comp.award;
                            sc.skaterName = s.name;
                            GameManager.Instance.currentSaveData.scheduledComps.Add(sc);
                        }
                        List<string> msg = new List<string> { "预约成功！选手会在当月自动出发！" };
                        PopupManager.Instance.MessagePop(msg);
                    }
                }
                else
                {
                    List<string> msg = new List<string> { "金钱不足！" };
                    PopupManager.Instance.MessagePop(msg);
                }
                selectedList.Clear();
                MenuManager.Instance.ClosePanel();
            },
            () => { }
        );
    }
}
