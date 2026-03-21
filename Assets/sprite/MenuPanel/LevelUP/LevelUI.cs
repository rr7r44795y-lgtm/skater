using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private TextMeshProUGUI Text;
    [SerializeField] private Button LvUpbutton;

    void Start()
    {
        Init();
        LvUpbutton.onClick.AddListener(LvUp);
    }

    private void LvUp()
    {
        LevelUPManager.Instance.ChangeUpgrad();
    }

    private void Init()
    {
        var club = GameManager.Instance.currentSaveData.club;
        ClubType ct = club.clubLevel;

        switch (ct)
        {
            case ClubType.Low:
                TitleText.text = "初级俱乐部";
                Text.text =
                    "·升级花费：100,000金\n" +
                    "·要求奖项：地方赛冠军 ×1\n" +
                    "·要求选手：6人以上\n\n" +
                    "·升级后可得：\n" +
                    "  选手上限 → 9人\n" +
                    "  建筑上限 → 10个";
                break;
            case ClubType.Middle:
                TitleText.text = "中级俱乐部";
                Text.text =
                    "·升级花费：150,000金\n" +
                    "·要求奖项：区级赛冠军 ×1\n" +
                    "·要求选手：9人以上\n\n" +
                    "·升级后可得：\n" +
                    "  选手上限 → 12人\n" +
                    "  建筑上限 → 15个";
                break;
            case ClubType.High:
                TitleText.text = "高级俱乐部";
                Text.text =
                    "·升级花费：250,000金\n" +
                    "·要求奖项：省级赛冠军 ×1\n" +
                    "·要求选手：12人以上\n\n" +
                    "·升级后可得：\n" +
                    "  选手上限 → 16人\n" +
                    "  建筑上限 → 20个";
                break;
            case ClubType.Perfect:
                TitleText.text = "顶级俱乐部";
                Text.text = "已达到最高等级！";
                break;
        }
    }
}