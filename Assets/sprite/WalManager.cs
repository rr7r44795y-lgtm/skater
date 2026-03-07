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

    public List<string> RoleName = new List<string>();

    private Skater skater;
    private int MaxCustom = 25;
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
                if (!RoleName.Contains(skater.name))
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
            if (GameManager.Instance.isPause) continue;
            int count = Random.Range(1, 5);
            for (int i = 0; i < count; i++)
            {
                if (CurrentCustom < MaxCustom) CopyPrefab();
            }
        }
    }

    private void CopyPrefab(Skater skater = null)
    {
        //赋值预制体，这个是你给我写的
        GameObject guest = Instantiate(CustomPrefab,transform);
        guest.transform.position = new Vector3(
    Random.Range(-5f, 5f),
    Random.Range(-3f, 3f),
    0
);
        Walk walk = guest.GetComponent<Walk>();
        walk.waypoints = waypoints;
        walk.trainingWaypoints = trainingWaypoints;
        walk.exitPoint = exitPoint;
        walk.enterPoint = enterPoint;
        walk.relaxPoint = relaxPoint;
        walk.TrainingPoint = TrainingPoint;
        walk.skater = skater;
        if (skater == null) CurrentCustom++;
        if (skater != null) walk.speed = skater.speed;
    }

}
