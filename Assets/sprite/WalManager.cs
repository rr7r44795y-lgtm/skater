using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WalManager : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform[] trainingWaypoints;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private GameObject CustomPrefab;
    [SerializeField] private Transform TrainingPoint;
    [SerializeField] private Transform relaxPoint;

    public static List<string> RoleName = new List<string>();

    private Skater skater;
    private int MaxCustom;
    public static int CurrentCustom;

    void Start()
    {
        StartCoroutine(SpawnLoop());
        StartCoroutine(CreateRoleLoop());
    }
    
    IEnumerator CreateRoleLoop()
    {
        while (true)
        {
            foreach (Skater skater in GameManager.Instance.currentSaveData.Skaters)
            {
                if (!RoleName.Contains(skater.name)&&!(skater.isCompeting) )
                {
                    CopyPrefab(skater);
                    RoleName.Add(skater.name);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            MaxCustom = Mathf.Max(GameManager.Instance.currentSaveData.structures.Count * 10,25);
            if (GameManager.isPause) continue;
            int count = Random.Range(1, 5);
            for (int i = 0; i < count; i++)
            {
                if (CurrentCustom < MaxCustom) CopyPrefab();
            }
        }
    }

    private void CopyPrefab(Skater skater = null)
    {
        CustomPrefab.SetActive(false); // 先关掉，Instantiate 不会触发 Start
        GameObject guest = Instantiate(CustomPrefab, transform);
        CustomPrefab.SetActive(true); // 预制体恢复，不影响下次用

        Walk walk = guest.GetComponent<Walk>();
        walk.waypoints = waypoints;
        walk.trainingWaypoints = trainingWaypoints;
        walk.exitPoint = exitPoint;
        walk.enterPoint = enterPoint;
        walk.relaxPoint = relaxPoint;
        walk.TrainingPoint = TrainingPoint;
        walk.skater = skater;
        if (skater == null) CurrentCustom++;
        if (skater != null) { walk.speed = skater.speed; }
        else { walk.speed = Random.Range(95,145); }

            guest.SetActive(true); // 赋值完再激活，这时候 Start 才跑
    }

}
