using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;

public class JumpDataUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI rotationText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI baseScoreText;
    [SerializeField] private Slider expBar;
    [SerializeField] private Button SelectBtn;

    private bool select = false;

    public void Init(JumpData J)
    {
        NameText.text = J.jumpName.ToString();
        rotationText.text = $"{J.rotation}圈";
        staminaText.text = $"{J.staminaCost}消耗";
        baseScoreText.text = $"{J.baseScore}基础分";
        expBar.value = J.exp / J.expToNext;
    }

    public void Pick(List<JumpData> list,JumpData J)
    {
        SelectBtn.onClick.AddListener(()=>Select(list,J));
        NameText.text = J.jumpName.ToString();
        rotationText.text = $"{J.rotation}圈";
        staminaText.text = $"{J.staminaCost}消耗";
        baseScoreText.text = $"{J.baseScore}基础分";
        expBar.value = J.exp / J.expToNext;
    }

    public void ResetSelect()
    {
        select = false;
        GetComponent<Image>().color = Color.white;
    }

    private void Select(List<JumpData> list, JumpData J)
    {
        if (!select)
        {
            select = true;
            if (list.Count > 0)
            {
                if (J.jumpName == jumpType.StepSequence || J.jumpName == jumpType.Spin) 
                {
                    List<string> msg = new List<string> { $"{J.jumpName}不能加入联跳" };
                    PopupManager.Instance.MessagePop(msg);
                    select = false;
                    return;
                }
            }

            if (list.Count<=3)
            {
                list.Add(J);
            } 
            else 
            {
                List<string> msg = new List<string> { "当前联跳已经三个了!不能再添加了!" };
                select = false;
                PopupManager.Instance.MessagePop(msg);
            }
        }
        else
        {
            select = false;
            list.Remove(J);
        }

        if (!select) GetComponent<Image>().color = Color.white;
        else GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.3f);

        PopupManager.Instance.RefreshText(list);
    }
}
