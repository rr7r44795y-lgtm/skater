using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header  ("Ãæ°å")]
    [SerializeField] private GameObject MenuPanel;
    [SerializeField] private GameObject Mask;
    [SerializeField] private GameObject CompPanel;
    [SerializeField] private GameObject LevelUpPanel;
    [SerializeField] private GameObject ShopPanel;
    [SerializeField] private GameObject SkaterPanel;
    [SerializeField] private GameObject SettingPanel;

    [Header  ("°´Å¥")]
    [SerializeField] private Button OpenPanelBtn;
    [SerializeField] private Button ClosedPanelBtn;
    [SerializeField] private Button CompertationBtn;
    [SerializeField] private Button LevelUpBtn;
    [SerializeField] private Button ShopBtn;
    [SerializeField] private Button SkaterBtn;
    [SerializeField] private Button SettingBtn;

    private GameObject currentPanel;

    // Start is called before the first frame update
    void Start()
    {
        OpenPanelBtn.onClick.AddListener(OpenPanel);
        ClosedPanelBtn.onClick.AddListener(ClosePanel);
        CompertationBtn.onClick.AddListener(OpenCompPanel);
        LevelUpBtn.onClick.AddListener(OpenLevelPanel);
        ShopBtn.onClick.AddListener(OpenShopPanel);
        SkaterBtn.onClick.AddListener(OpenSkaterPanel);
        SettingBtn.onClick.AddListener(OpenSettingPanel);
    }

    private void OpenPanel()
    {
        GameManager.Instance?.PauseGame();
        Mask.gameObject.SetActive(true);
        MenuPanel.SetActive(true);
        currentPanel = MenuPanel;
    }

    private void ClosePanel()
    {
        currentPanel.SetActive(false);
        if (currentPanel != MenuPanel) { OpenPanel(); }
        else { 
            Mask.SetActive(false);
            GameManager.Instance?.ResumeGame();
        }
    }

    private void OpenCompPanel()
    {
        currentPanel.SetActive(false);
        CompPanel.SetActive(true);
        currentPanel = CompPanel;
    }

    private void OpenLevelPanel()
    {
        currentPanel.SetActive(false);
        LevelUpPanel.SetActive(true);
        currentPanel = LevelUpPanel;
    }

    private void OpenShopPanel()
    {
        currentPanel.SetActive(false);
        ShopPanel.SetActive(true);
        currentPanel = ShopPanel;
    }

    private void OpenSkaterPanel()
    {
        currentPanel.SetActive(false);
        SkaterPanel.SetActive(true);
        currentPanel = SkaterPanel;
    }

    private void OpenSettingPanel()
    {
        currentPanel.SetActive(false);
        SettingPanel.SetActive(true);
        currentPanel = SettingPanel;
    }
}
