using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructSlot : MonoBehaviour
{
    [SerializeField] private Button slotBtn;
    [SerializeField] private Image slotImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Sprite emptySprite;  // 空坑位的默认图片

    private StructConfig config = null;
    private int slotIndex;
    private int currentCount = 0;
    private Coroutine blinkCoroutine;

    void Start()
    {
        // 用 sibling index 作为坑位编号
        slotIndex = transform.GetSiblingIndex();
        slotBtn.onClick.AddListener(OnClick);
        StartBlink();
    }

    private void OnClick()
    {
        if (config == null)
        {
            // 空位，弹建造面板
            StructManager.Instance.OpenBuildPanel(slotIndex);
        }
        else
        {
            // 已有建筑，弹拆除确认
            PopupManager.Instance.ChoicePop(
                $"{config.structName}\n容量:{config.maxCapacity}\n{config.description}\n\n是否拆除?",
                "拆除", "取消",
                () => { StructManager.Instance.RemoveStructure(slotIndex); },
                () => { }
            );
        }
    }

    // ===== 建造 =====
    public void Build(StructConfig c)
    {
        config = c;
        StopBlink();

        // 加载建筑图片
        Sprite sp = Resources.Load<Sprite>("Sprites/Struct/" + c.imageID);
        if (sp != null) slotImage.sprite = sp;
        slotImage.color = Color.white;

        if (nameText != null) nameText.text = c.structName;
    }

    // ===== 拆除 =====
    public void Remove()
    {
        config = null;
        currentCount = 0;
        slotImage.sprite = emptySprite;
        slotImage.color = Color.white;
        if (nameText != null) nameText.text = "+";
        StartBlink();
    }

    // ===== 空位闪烁 =====
    private void StartBlink()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkLoop());
    }

    private void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        slotImage.color = Color.white;
    }

    IEnumerator BlinkLoop()
    {
        while (config == null)
        {
            slotImage.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(0.8f);
            slotImage.color = Color.white;
            yield return new WaitForSeconds(0.8f);
        }
    }

    // ===== 给 Walk 用 =====
    public bool HasBuilding()
    {
        return config != null;
    }

    public StructConfig GetConfig()
    {
        return config;
    }

    public bool CanEnter()
    {
        return config != null && currentCount < config.maxCapacity;
    }

    public void Enter()
    {
        currentCount++;
    }

    public void Exit()
    {
        currentCount = Mathf.Max(0, currentCount - 1);
    }
}
