using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkaterCard : MonoBehaviour
{
    public Skater skater;
    private bool isClicked = false;

    [SerializeField] private Button ClickBtn;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private TextMeshProUGUI sexText;
    [SerializeField] private TextMeshProUGUI personalityText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnClicked()
    {
        PopupManager.Instance.SkaterInfoPop(skater);
    }

    public void Init(Skater skater)
    {
        ClickBtn.onClick.AddListener(OnClicked);
        this.skater = skater;
        nameText.text = skater.name;
        ageText.text = skater.age.ToString();
        sexText.text = skater.sex.ToString();
        personalityText.text = skater.personality.ToString();
    }

    public void InitForCompetation(Skater skater, ChooseSkaterUI parent)
    {
        this.skater = skater;
        nameText.text = skater.name;
        ageText.text = skater.age.ToString();
        sexText.text = skater.sex.ToString();
        personalityText.text = skater.isCompeting ? "²ÎÈüÖÐ" : "¿ÕÏÐ";

        ClickBtn.onClick.AddListener(() => {
            isClicked = !isClicked;
            if (isClicked)
                parent.selectedList.Add(skater);
            else
                parent.selectedList.Remove(skater);
            GetComponent<Image>().color = isClicked ? new Color(0.6f, 0.4f, 1f) : Color.white;
        });
    }
}
