using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JumpDataUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI rotationText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI baseScoreText;
    [SerializeField] private Slider expBar;


    public void Init(JumpData J)
    {
        NameText.text = J.jumpName.ToString();
        rotationText.text = $"{J.rotation}È¦";
        staminaText.text = $"{J.staminaCost}ÏûºÄ";
        baseScoreText.text = $"{J.baseScore}»ù´¡·Ö";
        expBar.value = J.exp / J.expToNext;
    }
}
