using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using static System.Net.Mime.MediaTypeNames;
using Random = UnityEngine.Random;

public class Walk : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform[] trainingWaypoints;
    public Transform exitPoint;
    public Transform enterPoint;
    public Transform relaxPoint;
    public Transform TrainingPoint;
    public Skater skater;
    public Button skaterBtn;
    public float speed = 350f;
    public Image outline;
    public Image innerImage;

    private bool isWandering = false;
    private bool isLeaving = false;
    private bool isWaiting = false;
    private bool isEntered = true;
    private bool isInTrainingArea = false;
    private bool isTraing;
    private bool isCustom = true;
    private Transform target;
    List<int> visited = new List<int>();

    #region 初始化函数
    // Start is called before the first frame update
    void Start()
    {
        if (skater != null) isCustom = false;

        if(isCustom){
            innerImage.color = new Color(
   Random.Range(0.4f, 1f),
    Random.Range(0.4f, 1f),
    Random.Range(0.4f, 1f),0.5f
    );
            outline.enabled = false;
        }
        else
        {
            innerImage.color = new Color(
   Random.Range(0.6f, 1f),
    Random.Range(0.6f, 1f),
    Random.Range(0.6f, 1f)
    );

            StartCoroutine(GetRoleType());
            skaterBtn = GetComponent<Button>();
            skaterBtn.enabled = true;
            skaterBtn.onClick.AddListener(OnClicked);
            StartCoroutine(ColorChange());
        }

        if (isEntered)
        {
            target = enterPoint;
        }
        else
        {
            PickNewTarget();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (GameManager.isPause) return;

        if (isCustom)
        {
            if (target == null) return;
        transform.position = Vector3.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime
        );

            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                if (isLeaving)
                {
                    Destroy(gameObject); // 到出口了，销毁
                }
                else if (isEntered)
                {
                    isEntered = false;
                    PickNewTarget();
                }
                else
                {
                    if (!isWaiting) StartCoroutine(WaitThenMove());
                }
            }
        }
        else
        {
            if (target == null) return;
            transform.position = Vector3.MoveTowards(
                transform.position, target.position, speed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                if (isLeaving)
                {
                    Destroy(gameObject);
                }
                else if (isTraing)
                {
                    target = null;
                    if (!isWaiting && isInTrainingArea) StartCoroutine(DoTraining());
                    else if (!isWaiting && !isInTrainingArea) StartCoroutine(Wander());
                }
                else if (isTraing == false)
                {
                    target = null;
                    if (isWandering)
                    {
                        if (!isWaiting) StartCoroutine(WanderRest());
                    }
                    else
                    {
                        if (!isWaiting) StartCoroutine(DoRelax());
                    }
                }
            }
        }
    }
    #endregion

    #region 选择地点和角色状态的函数
    IEnumerator WaitThenMove()
    {
        isWaiting = true;
        target = null; // 停下来
        yield return new WaitForSeconds(1.5f);
        if(Random.value< 0.5f)
        { 
            target = exitPoint;
            isLeaving = true;
        }
        else
        {
            PickNewTarget();
        }
        isWaiting = false;
    }

    IEnumerator GetRoleType()
    {
        while (true)
        {
                if (target == null) // 到了才切换
                {
                    if (skater.current == currentType.training)
                    {
                        isTraing = true;

                        if (trainingWaypoints != null && trainingWaypoints.Length > 0)
                             {
                        isInTrainingArea = true;
                        target = trainingWaypoints[Random.Range(0, trainingWaypoints.Length)]; }
                        else
                              target = TrainingPoint;
                    }
                    else
                    {
                        isTraing = false;
                        isInTrainingArea = false;
                        target = relaxPoint;
                    }
                }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ColorChange()
    {
        while (true)
        {
            outline.color = (outline.color == Color.white)
              ? new Color(1f, 0.84f, 0f)
              : Color.white;
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }
    #endregion

    #region 选择地点
    private void PickNewTarget()
    {
        if(isCustom)
        {
            List<int> unvisited = new List<int>();
            for(int i = 0; i < waypoints.Length; i++)
            {
            if (!visited.Contains(i)) unvisited.Add(i);
            }

            if(unvisited.Count == 0)
      
            {
                target = exitPoint;
                isLeaving = true;
                return;
            }

            int pick = unvisited[Random.Range(0, unvisited.Count)];
            visited.Add(pick);
            target = waypoints[pick];
        }
        else
        {
            target = waypoints[Random.Range(0, waypoints.Length)];
        }
        
    }
    #endregion

    #region 训练和休息的协程函数
    IEnumerator DoTraining()
    {
        isWaiting = true;
        if (skater.stamina > 0 && Random.value < 0.98f)
        {
            if (Random.value < 0.98f)
            {
                Training(skater);
            }
            else
                PickNewTarget();
        }
        else
        {
            skater.current = currentType.relax;
        }

        yield return new WaitForSeconds(3f);
        isWaiting = false;
    }

    IEnumerator DoRelax()
    {
        isWaiting = true;
        Relax(skater);

        if (skater.stamina < 100 && Random.value < 0.2f)
        {
            isWandering = true;
            PickNewTarget(); // 20% 概率去逛一圈
        }

        yield return new WaitForSeconds(2f);
        isWaiting = false;
    }

    IEnumerator Wander()
    {
        isWaiting = true;
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        isInTrainingArea = true; // 回归训练状态，GetRoleType 会分配训练点
        isWaiting = false;
    }

    IEnumerator WanderRest()
    {
        isWaiting = true;
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        isWandering = false;
        isWaiting = false;
    }
    #endregion

    #region 训练方法
    private void Training(Skater skater)
    {
        trainType Type = skater.trainingType;
        int num = UnityEngine.Random.Range(1, 5);
        switch (Type)
        {
            case trainType.jump:
                skater.jump += num;
                PopupManager.Instance?.TMProPop(skater.name, "跳跃", num);
                skater.stamina -= 10;
                break;
            case trainType.spin:
                skater.spin += num;
                PopupManager.Instance?.TMProPop(skater.name, "旋转", num);
                skater.stamina -= 10;
                break;
            case trainType.dance:
                skater.dance += num;
                PopupManager.Instance?.TMProPop(skater.name, "舞蹈", num);
                skater.stamina -= 10;
                break;
        }
    }
    #endregion

    #region 休息方法
    private void Relax(Skater skater)
    {
        skater.current = currentType.relax;
        skater.stamina = Mathf.Min(skater.stamina + 10, 100);
        if (skater.stamina == 100) skater.current = currentType.training;
        PopupManager.Instance?.TMProPop(skater.name, "体力", 10);
    }
    #endregion

    #region 时机函数
    void OnDestroy()
    {
        if(isCustom)WalManager.CurrentCustom--;
    }

    private void OnClicked()
    {
        PopupManager.Instance?.SkaterInfoPop(skater);
    }
    #endregion
}
