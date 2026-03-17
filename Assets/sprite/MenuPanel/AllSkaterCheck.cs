using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllSkaterCheck : MonoBehaviour
{
    [SerializeField] private GameObject Prefab;
    [SerializeField] private GameObject SkaterPanel;
    // Start is called before the first frame update

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in SkaterPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var s in GameManager.Instance.currentSaveData.Skaters)
        {
            GameObject card = Instantiate(Prefab, SkaterPanel.transform);
            card.GetComponent<SkaterCard>().Init(s);
        }
    }
}
