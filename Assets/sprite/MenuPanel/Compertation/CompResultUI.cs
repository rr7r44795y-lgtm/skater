using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompResultUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI RankText;

    public void Init(string name,string rank)
    {
        nameText.text = name;
        RankText.text = rank;
    }
}
