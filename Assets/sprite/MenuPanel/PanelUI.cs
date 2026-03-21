using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI NumberText;
    [SerializeField] private TextMeshProUGUI describText;
    [SerializeField] private Button CloseBtn;
    [SerializeField] private GameObject MaskPanel;
    [SerializeField] private GameObject Root;

    void Start()
    {
        CloseBtn.onClick.AddListener(Closed);
    }

    public void SkillSet(skillData sk, int level)
    {
        Root.SetActive(true);
        NameText.text = sk.skillName + $" Lv.{level}";
        SkillLevelData lv = sk.levels[level - 1];
        NumberText.text = $"´¥·¢¸ÅÂÊ{lv.triggerRate * 100}%";
        describText.text = sk.description;
    }

    private void Closed()
    {
        Root.SetActive(false);
        MaskPanel.SetActive(false);
    }
}
