using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject ComperPanel;
    [SerializeField] private GameObject ComperPrefab;
    [SerializeField] private TMP_Dropdown ChooseTab;
    [SerializeField] private GameObject ChoosePanel;//派遣选手面板

    private CompetitionList data;

    void Start()
    {
        TextAsset json = Resources.Load<TextAsset>("Competitions");
        data = JsonUtility.FromJson<CompetitionList>(json.text);
        ChooseTab.onValueChanged.AddListener(CompMonth);
        DropText();
    }

    private void DropText()
    {
        List<string> options = new List<string>{"1月", "2月", "3月", "4月", "5月", "6月", "7月","8月", "9月", "10月", "11月", "12月"};
        ChooseTab.ClearOptions();
        ChooseTab.AddOptions(options);
    }

    private void CompMonth(int month)
    {
        foreach (Transform child in ComperPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var C in data.competitions)
        {
            if (C.month == month+1)
            {
                GameObject card = Instantiate(ComperPrefab, ComperPanel.transform);
                card.GetComponent<CompCard>().Init(C);
                card.GetComponent<CompCard>().ChoosePanel = ChoosePanel;
            }
        }
    }

}
