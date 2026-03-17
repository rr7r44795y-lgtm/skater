using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CompetitionManager : MonoBehaviour
{
    public static CompetitionManager Instance { get; private set; }
    private CompetitionList compData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCompetitions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadCompetitions()
    {
        TextAsset json = Resources.Load<TextAsset>("Competitions");
        if (json != null)
            compData = JsonUtility.FromJson<CompetitionList>(json.text);
    }

    public void SettleCompetition(List<Skater> mySkaters, ActiveComp comp)
    {
        if (comp.compType == CompType.Exhibition)
        {
            SettleExhibition(mySkaters, comp);
            return;
        }
        // ===== 1. 生成对手填满16人 =====
        List<Skater> allSkaters = new List<Skater>(mySkaters);
        while (allSkaters.Count < 16)
        {
            Skater opponent = GenerateOpponents(comp);
            if (opponent != null) allSkaters.Add(opponent);
            else break;
        }

        // ===== 2. 洗牌 =====
        for (int i = allSkaters.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = allSkaters[i];
            allSkaters[i] = allSkaters[j];
            allSkaters[j] = temp;
        }

        // ===== 3. 模拟表演 =====
        Dictionary<Skater, float> scores = new Dictionary<Skater, float>();

        for (int i = 0; i < allSkaters.Count; i++)
        {
            var S = allSkaters[i];
            float totalScore = 0f;

            float k = 0.5f;
            int order = i + 1;
            if (order % 4 == 1) k -= 0.1f;
            else if (order % 4 == 0) k -= 0.15f;
            else k += 0.05f;

            // 节目单
            List<JumpData> program;
            if (S.ShowList.Count > 0)
            {
                program = S.ShowList;
            }
            else if (S.jumpTypes.Count > 0)
            {
                program = new List<JumpData>(S.jumpTypes);
                program.Sort((a, b) => b.baseScore.CompareTo(a.baseScore));
            }
            else
            {
                program = new List<JumpData>();
            }

            foreach (var jump in program)
            {
                S.stamina -= jump.staminaCost;

                // 体力影响成功率
                float staminaPenalty = 0f;
                if (S.stamina < 30) staminaPenalty = -0.2f;
                else if (S.stamina < 60) staminaPenalty = -0.1f;

                // 技能
                float skillBonus = 0f;
                // TODO: skill改成Class后再填

                float finalK = k + skillBonus + staminaPenalty;

                float roll = Random.Range(0f, 1f);
                float goe = 0f;

                if (roll > finalK)
                {
                    float failRoll = Random.Range(0f, 1f);
                    if (failRoll > finalK - 0.1f)
                        goe = -3f;
                    else
                        goe = -1f;
                }
                else
                {
                    float successRoll = Random.Range(0f, 1f);
                    if (successRoll < 0.5f)
                        goe = 1f;
                    else if (successRoll < 0.75f)
                        goe = 2f;
                    else if (successRoll < 0.9f)
                        goe = 3f;
                    else
                        goe = 5f;
                }

                float jumpScore = jump.baseScore * (1f + goe / 10f);
                totalScore += jumpScore;
            }

            float pcs = Mathf.Clamp(S.dance / 30f, 0f, 10f);
            totalScore += pcs;

            scores[S] = totalScore;
        }

        // ===== 4. 排名 =====
        allSkaters.Sort((a, b) => scores[b].CompareTo(scores[a]));

        // ===== 5. 发奖励 =====
        for (int i = 0; i < allSkaters.Count; i++)
        {
            if (!mySkaters.Contains(allSkaters[i])) continue;

            var S = allSkaters[i];
            int rank = i + 1;
            if (rank == 1)
            {
                GameManager.Instance.currentSaveData.money += comp.award;
                S.fans += 500;
                var club = GameManager.Instance.currentSaveData.club;
                switch (comp.compType)
                {
                    case CompType.Local: club.AwardLocal++; break;
                    case CompType.District: club.AwardDistrict++; break;
                    case CompType.Provincial: club.AwardProvincial++; break;
                    case CompType.National: club.AwardNational++; break;
                    case CompType.International: club.AwardInternational++; break;
                }
            }
            else if (rank == 2)
            {
                GameManager.Instance.currentSaveData.money += (int)(comp.award * 0.6f);
                S.fans += 300;
            }
            else if (rank == 3)
            {
                GameManager.Instance.currentSaveData.money += (int)(comp.award * 0.3f);
                S.fans += 100;
            }
            else
            {
                S.fans += 10;
            }

            S.LiveList.Add($"{comp.compName} 第{rank}名 得分:{scores[S]:F1}\n");
        }

        // ===== 6. 弹窗 =====
        string msg = "";
        for (int i = 0; i < allSkaters.Count; i++)
        {
            string marker = mySkaters.Contains(allSkaters[i]) ? "★" : "";
            msg += ($"第{i + 1}名: {scores[allSkaters[i]]:F1}分 {marker} {allSkaters[i].name}\n");
        }
        PopupManager.Instance.CompPop(comp.compName,msg);
    }

    private Skater GenerateOpponents(ActiveComp comp)
    {
        // 从配置里查比赛信息
        Competition CompInfo = null;
        if (compData != null)
        {
            foreach (var item in compData.competitions)
            {
                if (comp.compID == item.compID)
                {
                    CompInfo = item;
                    break;
                }
            }
        }
        if (CompInfo == null) return null;

        int min = 0, max = 0;
        switch (comp.compType)
        {
            case CompType.Local: min = 60; max = 180; break;
            case CompType.District: min = 150; max = 250; break;
            case CompType.Provincial: min = 235; max = 350; break;
            case CompType.National: min = 335; max = 500; break;
            case CompType.International: min = 500; max = 1000; break;
        }

        Skater s = new Skater();
        s.sex = CompInfo.sexType;
        s.name = SkaterManager.Instance.ReadAndWrite(s.sex);
        s.stamina = Random.Range(90, 120);
        s.jump = Random.Range(min, max);
        s.spin = Random.Range(min, max);
        s.dance = Random.Range(min, max);
        s.fans = Random.Range(min, max);
        s.age = Random.Range(CompInfo.MiniAge, CompInfo.MaxAge);
        s.speed = Random.Range(320, 401);

        // 给对手生成跳跃，根据级别
        switch (comp.compType)
        {
            case CompType.Local:
                s.jumpTypes.Add(new JumpData(jumpType.Toeloop, 1, 2, 8, 4f));
                s.jumpTypes.Add(new JumpData(jumpType.Salchow, 1, 2, 10, 5f));
                break;
            case CompType.District:
                s.jumpTypes.Add(new JumpData(jumpType.Toeloop, 2, 3, 10, 8f));
                s.jumpTypes.Add(new JumpData(jumpType.Salchow, 2, 3, 12, 9f));
                s.jumpTypes.Add(new JumpData(jumpType.Loop, 1, 2, 10, 6f));
                break;
            case CompType.Provincial:
                s.jumpTypes.Add(new JumpData(jumpType.Toeloop, 2, 3, 10, 8f));
                s.jumpTypes.Add(new JumpData(jumpType.Salchow, 2, 3, 12, 9f));
                s.jumpTypes.Add(new JumpData(jumpType.Flip, 2, 3, 14, 11f));
                s.jumpTypes.Add(new JumpData(jumpType.Loop, 2, 3, 12, 9f));
                break;
            case CompType.National:
                s.jumpTypes.Add(new JumpData(jumpType.Lutz, 3, 4, 16, 15f));
                s.jumpTypes.Add(new JumpData(jumpType.Flip, 3, 4, 14, 13f));
                s.jumpTypes.Add(new JumpData(jumpType.Salchow, 3, 4, 12, 11f));
                s.jumpTypes.Add(new JumpData(jumpType.Loop, 2, 3, 12, 9f));
                s.jumpTypes.Add(new JumpData(jumpType.Axel, 2, 3, 14, 12f));
                break;
            case CompType.International:
                s.jumpTypes.Add(new JumpData(jumpType.Axel, 3, 4, 18, 18f));
                s.jumpTypes.Add(new JumpData(jumpType.Lutz, 3, 4, 16, 15f));
                s.jumpTypes.Add(new JumpData(jumpType.Flip, 3, 4, 14, 13f));
                s.jumpTypes.Add(new JumpData(jumpType.Salchow, 3, 4, 12, 11f));
                s.jumpTypes.Add(new JumpData(jumpType.Loop, 3, 4, 14, 12f));
                s.jumpTypes.Add(new JumpData(jumpType.Toeloop, 3, 4, 10, 10f));
                break;
        }
        // TODO: 根据比赛级别给更多/更强的跳跃

        // 节目单用会的跳跃填充
        foreach (var j in s.jumpTypes)
        {
            s.ShowList.Add(j);
        }

        return s;
    }

    private void SettleExhibition(List<Skater> mySkaters, ActiveComp comp)
    {
        List<string> resultMsg = new List<string>();
        foreach (var s in mySkaters)
        {
            s.fans += 50;
            s.LiveList.Add($"{comp.compName} 表演赛出演");
            resultMsg.Add($"{s.name} 参加了{comp.compName}，粉丝+50");
        }
        GameManager.Instance.currentSaveData.money += comp.award;
        PopupManager.Instance.MessagePop(resultMsg);
    }
}