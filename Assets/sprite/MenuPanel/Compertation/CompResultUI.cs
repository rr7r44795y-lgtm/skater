using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompResultUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI RankText;
    [SerializeField] private Button closeBtn; // 预制体上自己的关闭按钮
    private System.Action onCloseCallback;

    public void Init(string name, string rank, System.Action onClose = null)
    {
        nameText.text = name;
        RankText.text = rank;
        onCloseCallback = onClose;
        closeBtn.onClick.AddListener(Close);
    }

    public void Close()
    {
        onCloseCallback?.Invoke();
        PopupManager.Instance.OnCompPopClosed();
        Destroy(gameObject);
    }
}