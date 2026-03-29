using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructCardUI : MonoBehaviour
{
    [SerializeField] private Image structImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Button selectBtn;

    private StructConfig config;

    public void Init(StructConfig c)
    {
        config = c;
        nameText.text = c.structName;
        priceText.text = $"{c.price}金";
        if (descText != null) descText.text = c.description;

        // 加载图片
        Sprite sp = Resources.Load<Sprite>("Sprites/Struct/" + c.imageID);
        if (sp != null) structImage.sprite = sp;

        selectBtn.onClick.AddListener(OnSelect);
    }

    private void OnSelect()
    {
        StructManager.Instance.ConfirmBuild(config);
    }
}
