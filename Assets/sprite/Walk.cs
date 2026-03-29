using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;
using TMPro;

[Serializable]
public class JumpRotationData
{
    public int rotation;
    public float baseScore;
    public int staminaCost;
}

[Serializable]
public class JumpConfig
{
    public jumpType jumpName;
    public List<JumpRotationData> rotations;
}

[Serializable]
public class JumpConfigList
{
    public List<JumpConfig> JumpConfigs;
}

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
    public float speed = 120f;
    public Image outline;
    public Image innerImage;

    private bool isLeaving = false;
    private bool isCustom = true;
    private Transform target;
    List<int> visited = new List<int>();
    private Sprite[] frames;

    #region 初始化
    void Start()
    {
        if (skater != null) isCustom = false;

        if (isCustom)
        {
            frames = Resources.LoadAll<Sprite>("Sprites/Custom/custom_"+Random.Range(1,9).ToString("D3"));
            outline.enabled = false;
            innerImage.color = Color.white;
            target = enterPoint;
            StartCoroutine(PlayAnimation());
            StartCoroutine(CustomLoop());
        }
        else
        {
            frames = Resources.LoadAll<Sprite>("Sprites/skater/" +skater.spriteID);
            skaterBtn = GetComponent<Button>();
            innerImage.color = Color.white;
            outline.enabled = false;
            skaterBtn.enabled = true;
            skaterBtn.onClick.AddListener(OnClicked);
            StartCoroutine(PlayAnimation());
            StartCoroutine(SkaterLoop());
        }
    }

    void Update()
    {
        if (GameManager.isPause) return;
        if (target == null) return;
        transform.position = Vector3.MoveTowards(
            transform.position, target.position, speed * Time.deltaTime);
        if (isLeaving && Vector3.Distance(transform.position, target.position) < 0.1f)
            Destroy(gameObject);
    }
    #endregion

    #region 选手主循环
    IEnumerator SkaterLoop()
    {
        // 1. 走到入口
        target = enterPoint;
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position) < 0.1f);

        // 2. 主循环
        while (true)
        {
            // 检查比赛
            if (skater.isCompeting)
            {
                target = exitPoint;
                isLeaving = true;
                yield break;
            }

            // 3. 根据状态选点
            if (skater.current == currentType.training)
            {
                if (trainingWaypoints != null && trainingWaypoints.Length > 0)
                    target = trainingWaypoints[Random.Range(0, trainingWaypoints.Length)];
                else
                    target = TrainingPoint;
            }
            else
            {
                target = relaxPoint;
            }

            // 4. 等走到
            yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position) < 0.1f);

            // 5. 再检查一次比赛（走的过程中可能变了）
            if (skater.isCompeting)
            {
                target = exitPoint;
                isLeaving = true;
                yield break;
            }

            // 6. 执行对应逻辑
            target = null;
            if (skater.current == currentType.training)
            {
                if (skater.stamina > 0 && Random.value < 0.98f)
                {
                    Training(skater);
                }
                else
                {
                    skater.current = currentType.relax;
                }
                yield return new WaitForSeconds(3f);
            }
            else
            {
                Relax(skater);
                // 20% 概率闲逛一下再回去
                if (skater.stamina < 100 && Random.value < 0.2f)
                {
                    target = waypoints[Random.Range(0, waypoints.Length)];
                    yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position) < 0.1f);
                    target = null;
                    yield return new WaitForSeconds(Random.Range(2f, 5f));
                }
                else
                {
                    yield return new WaitForSeconds(2f);
                }
            }
        }
    }
    #endregion

    #region 路人主循环
    IEnumerator CustomLoop()
    {
        // 走到入口
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position) < 0.1f);

        // 逛点
        while (true)
        {
            PickNewTarget();
            if (isLeaving) yield break;

            yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position) < 0.1f);
            target = null;
            yield return new WaitForSeconds(1.5f);
        }
    }

    private void PickNewTarget()
    {
        List<int> unvisited = new List<int>();
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (!visited.Contains(i)) unvisited.Add(i);
        }

        if (unvisited.Count == 0 || Random.value < 0.5f)
        {
            target = exitPoint;
            isLeaving = true;
            return;
        }

        int pick = unvisited[Random.Range(0, unvisited.Count)];
        visited.Add(pick);
        target = waypoints[pick];
    }
    #endregion

    #region 动画变换
    IEnumerator PlayAnimation()
    {
        int index = 0;
        while (true)
        {
            innerImage.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(0.15f); // 每帧间隔，越小越快
        }
    }
    #endregion

    #region 训练方法
    private void Training(Skater skater)
    {
        trainType Type = skater.trainingType;
        int num = Random.Range(1, 10);
        switch (Type)
        {
            case trainType.jump:
                {
                    if (skater.jumpTypes.Count < 8)
                    {
                        jumpType[] allJumps = { jumpType.Toeloop, jumpType.Salchow, jumpType.Loop,
                                jumpType.Flip, jumpType.Lutz, jumpType.Axel };

                        List<jumpType> unlearned = new List<jumpType>();
                        foreach (var j in allJumps)
                        {
                            bool known = false;
                            foreach (var owned in skater.jumpTypes)
                            {
                                if (owned.jumpName == j) { known = true; break; }
                            }
                            if (!known) unlearned.Add(j);
                        }

                        // 有没学的就骰概率
                        if (unlearned.Count > 0 && Random.Range(0f, 1f) < 0.3f)
                        {
                            jumpType learned = unlearned[Random.Range(0, unlearned.Count)];
                            skater.jumpTypes.Add(new JumpData(learned, 1, 2, 8, 4f));
                            List<string> msg = new List<string> { $"{skater.name}学会了{learned}!" };
                            PopupManager.Instance?.MessagePop(msg);
                            break;
                        }

                        // 6个跳跃学完了，补 Spin 和 StepSequence
                        if (unlearned.Count == 0 && skater.jumpTypes.Count < 8)
                        {
                            bool hasSpin = false, hasStep = false;
                            foreach (var owned in skater.jumpTypes)
                            {
                                if (owned.jumpName == jumpType.Spin) hasSpin = true;
                                if (owned.jumpName == jumpType.StepSequence) hasStep = true;
                            }
                            if (!hasSpin)
                            {
                                JumpRotationData data = CompetitionManager.Instance.GetJumpRotationData(jumpType.Spin, 1);
                                skater.jumpTypes.Add(new JumpData(jumpType.Spin, 1, 4, data.staminaCost, data.baseScore));
                                List<string> msg = new List<string> { $"{skater.name}学会了Spin!" };
                                PopupManager.Instance?.MessagePop(msg);
                                break;
                            }
                            if (!hasStep)
                            {
                                JumpRotationData data = CompetitionManager.Instance.GetJumpRotationData(jumpType.StepSequence, 1);
                                skater.jumpTypes.Add(new JumpData(jumpType.StepSequence, 1, 4, data.staminaCost, data.baseScore));
                                List<string> msg = new List<string> { $"{skater.name}学会了StepSequence!" };
                                PopupManager.Instance?.MessagePop(msg);
                                break;
                            }
                        }
                    }
                    else if (skater.jumpTypes.Count >= 8 && Random.Range(0f, 1f) < 0.3f)
                    {
                        JumpData picked = null;
                        foreach (var j in skater.jumpTypes)
                        {
                            if (j.rotation >= j.maxRotation) continue;
                            if (j.jumpName == skater.trainingJump)
                            {
                                picked = j;
                                break;
                            }
                        }
                        if (picked != null)
                        {
                            picked.exp += num;
                            ShowPop($"{picked.jumpName} exp+{num}");
                            if (picked.exp >= picked.expToNext)
                            {
                                RotationLvUP(skater, picked.jumpName);
                                List<string> msg = new List<string> { $"{skater.name}的{picked.jumpName}升到{picked.rotation}圈!" };
                                PopupManager.Instance?.MessagePop(msg);
                            }
                            break;
                        }
                    }
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        skater.jump += num;
                        skater.stamina -= 10;
                        ShowPop($"jump+{num}");
                    }
                    break;
                }
            case trainType.spin:
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    skater.spin += num;
                    skater.stamina -= 10;
                    ShowPop($"spin+{num}");
                }
                break;
            case trainType.dance:
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    skater.dance += num;
                    skater.stamina -= 10;
                    ShowPop($"dance+{num}");
                }
                break;
        }
    }
    #endregion

    #region 休息方法
    private void Relax(Skater skater)
    {
        skater.current = currentType.relax;
        skater.stamina = Mathf.Min(skater.stamina + 10, 100);
        if (skater.stamina >= 100)
        {
            skater.stamina = 100;
            skater.current = currentType.training;
        }
    }
    #endregion

    #region 圈数升级
    private void RotationLvUP(Skater s, jumpType jump)
    {
        JumpData currentJump = null;
        foreach (var item in s.jumpTypes)
        {
            if (item.jumpName == jump)
            {
                currentJump = item;
                break;
            }
        }
        if (currentJump == null) return;
        if (currentJump.rotation >= currentJump.maxRotation) return;

        currentJump.rotation++;
        JumpRotationData newData = CompetitionManager.Instance.GetJumpRotationData(jump, currentJump.rotation);
        if (newData != null)
        {
            currentJump.baseScore = newData.baseScore;
            currentJump.staminaCost = newData.staminaCost;
        }
        currentJump.exp = 0;
        currentJump.expToNext += 50;
    }
    #endregion

    #region 点击事件
    private void OnClicked()
    {
        PopupManager.Instance?.SkaterInfoPop(skater);
    }
    #endregion

    #region 销毁
    void OnDestroy()
    {
        if (isCustom) WalManager.CurrentCustom--;
        if(!isCustom)WalManager.RoleName.Remove(skater.name);
    }
    #endregion

    #region 信息显示
    [Header("属性增长弹窗")]
    Queue<string> msgQueue = new Queue<string>();
    [SerializeField] private TextMeshProUGUI popText;
    private bool isShowing = false;

    public void ShowPop(string msg)
    {
        msgQueue.Enqueue(msg);
        if (!isShowing) StartCoroutine(PopLoop());
    }

    IEnumerator PopLoop()
    {
        isShowing = true;
        while (msgQueue.Count > 0)
        {
            popText.text = msgQueue.Dequeue();
            popText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
        }
        popText.gameObject.SetActive(false);
        isShowing = false;
    }
    #endregion

}
