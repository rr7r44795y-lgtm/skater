using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillDataUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private Button SkillBtn;

    private int currentLevel;
    public GameObject SkillInfoPanel;
    private skillData sk;
    
    public void Init(skill s,GameObject Panel)
    {
        currentLevel = s.level;
        SkillInfoPanel = Panel;
        sk = CompetitionManager.Instance.GetSkillByID(s.skillID);
        if (sk == null) { NameText.text = "null"; return; }
        NameText.text = $"{sk.skillName}"+$"Lv.{currentLevel}";
        SkillBtn.onClick.AddListener(() => PanelSet(sk));
    }

    private void PanelSet(skillData sk)
    {
        SkillInfoPanel.SetActive(true);
        SkillInfoPanel.GetComponent<PanelUI>().SkillSet(sk, currentLevel);
    }
}
