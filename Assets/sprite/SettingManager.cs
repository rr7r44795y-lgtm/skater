using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; }

    // UI 组件
    [Header("UI References")]
    public Slider musicVolumeSlider;
    public Toggle muteToggle;
    [SerializeField] private Button SaveBtn;
    [SerializeField] private Button BackToHomeBtn;

    // 数据
    private float musicVolume = 0.8f;
    private bool isMuted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 绑定 UI 事件
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            musicVolumeSlider.value = musicVolume;
        }

        if (muteToggle != null)
        {
            muteToggle.onValueChanged.AddListener(OnMuteToggled);
            muteToggle.isOn = isMuted;
        }

        ApplySettings();

        BackToHomeBtn.onClick.AddListener(ReturnToTitle);
        SaveBtn.onClick.AddListener(GameManager.Instance.SaveAllData);
    }

    #region 设置读写
    private void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplySettings()
    {
        float finalVolume = isMuted ? 0f : musicVolume;
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(finalVolume);
        }
    }
    #endregion

    #region UI 回调
    public void OnMusicVolumeChanged(float value)
    {
        musicVolume = value;
        if (!isMuted)
        {
            ApplySettings();
        }
        SaveSettings();
    }

    public void OnMuteToggled(bool value)
    {
        isMuted = value;
        ApplySettings();
        SaveSettings();

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.interactable = !isMuted;
        }
    }
    #endregion

    #region 按钮功能
    public void SaveGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveAllData();
        }
    }

    public void ReturnToTitle()
    {
        SaveSettings();
        //SceneManager.LoadScene("Title");
    }
    #endregion

    #region 公共访问方法（供其他脚本调用）
    public float GetMusicVolume()
    {
        return isMuted ? 0f : musicVolume;
    }

    public bool IsMuted()
    {
        return isMuted;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        OnMusicVolumeChanged(musicVolume);
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        OnMuteToggled(isMuted);
    }
    #endregion
}