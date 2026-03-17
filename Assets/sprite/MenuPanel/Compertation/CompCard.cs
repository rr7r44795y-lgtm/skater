using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI AwardText;
    [SerializeField] private TextMeshProUGUI CompTypeText;
    [SerializeField] private TextMeshProUGUI FeeText;
    [SerializeField] private Button clickBtn;
    [SerializeField] public GameObject ChoosePanel;    

    private Competition comp;

    void Start()
    {
        clickBtn.onClick.AddListener(OnClicked);
    }

    public void Init(Competition c)
    {
        comp = c;
        string S = comp.sexType == Sex.boy ? "男子组" : "女子组";
        NameText.text = c.name;
        FeeText.text = $"{S}";
        AwardText.text = $"{c.award}";
        CompTypeText.text = c.GetCompTypeName();
    }

    private void OnClicked()
    {
        // 点击后弹出详情
        ChoosePanel.GetComponent<ChooseSkaterUI>().Init(comp);
        MenuManager.Instance.OpenPanel(ChoosePanel);
    }

}
