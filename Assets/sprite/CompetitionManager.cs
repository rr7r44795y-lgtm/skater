using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

    #region skill配置
//这里是skill配置，gameManager是skater里的skill
[Serializable]
public class skillList
{
    public List<skillData> SkillInfo;
}

[Serializable]
public class skillData
{
    public string skillID;
    public string skillName;
    public skillType Type;
    public jumpType matchJump;
    public List<SkillLevelData> levels;
    public string description;
}

[Serializable]
public class SkillLevelData
{
    public float triggerRate;//触发概率
    public float number;//数值
}

[Serializable]
public enum skillType
{
    stamina, jump, step, score
}
#endregion

public class CompetitionManager : MonoBehaviour
{
    public static CompetitionManager Instance { get; private set; }
    private CompetitionList compData;
    private skillList SkillData;
    private JumpConfigList jumpConfigData;

    #region 初始化
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCompetitions();
            LoadSkill();
            jumpConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region 竞赛读取
    private void LoadCompetitions()
    {
        TextAsset json = Resources.Load<TextAsset>("Competitions");
        if (json != null)
            compData = JsonUtility.FromJson<CompetitionList>(json.text);
    }
    #endregion

    #region 比赛开始
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
                float currentBase = jump.baseScore;
                int currentCost = jump.staminaCost;

                // 超绝爆发：概率提升一圈
                if (jump.rotation < jump.maxRotation && Random.Range(0f, 1f) < 0.05f)
                {
                    JumpRotationData upData = CompetitionManager.Instance.GetJumpRotationData(jump.jumpName, jump.rotation + 1);
                    if (upData != null)
                    {
                        currentBase = upData.baseScore;
                        currentCost = upData.staminaCost;
                    }
                }

                S.stamina -= currentCost;

                // 体力影响成功率
                float staminaPenalty = 0f;
                if (S.stamina < 30) staminaPenalty = -0.2f;
                else if (S.stamina < 60) staminaPenalty = -0.1f;

                // 技能
                float skillBonus = 0f;
                float extraScore = 0f;
                ApplySkills(S, jump, ref skillBonus, ref extraScore);

                //属性
                float attrBonus = 0f;
                if (jump.jumpName <= jumpType.Axel)
                    attrBonus = S.jump * 0.0001f;
                else if (jump.jumpName == jumpType.Spin)
                    attrBonus = S.spin * 0.0001f;
                else if (jump.jumpName == jumpType.StepSequence)
                    attrBonus = S.dance * 0.0001f;

                if (attrBonus > 0.3f) attrBonus = 0.3f;
                float finalK = k + skillBonus + staminaPenalty + attrBonus;

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

                float jumpScore = (currentBase + extraScore) * (1f + goe / 10f);
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
    #endregion

    #region 生成对手
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
        JumpListGener(s, comp);

        //生成技能
        SkillListGener(s, comp);
        // 节目单用会的跳跃填充
        foreach (var j in s.jumpTypes)
        {
            s.ShowList.Add(j);
        }

        return s;
    }

    private void SkillListGener(Skater s, ActiveComp c)
    {
        float ageMultiplier = 1f;
        if (s.age <= 9) ageMultiplier = 0.2f;
        else if (s.age <= 13) ageMultiplier = 0.3f;
        else if (s.age <= 17) ageMultiplier = 0.7f;
        else if (s.age <= 21) ageMultiplier = 1.0f;
        else if (s.age <= 25) ageMultiplier = 1.3f;
        else ageMultiplier = 1.5f;

        for(int i = 0; i < 8; i++)
        {
            float k = Random.Range(0f, 1f);
            switch (c.compType)
            {
                case CompType.Local:
                    k -= 0.2f;
                    break;
                case CompType.District:
                    k -= 0.15f;
                    break;
                case CompType.Provincial:
                    k -= 0.1f;
                    break;
                case CompType.National:
                    k += 0.15f;
                    break;
                case CompType.International:
                    k += 0.2f;
                    break;
            }

            if (Random.Range(0f, 1f) < k * ageMultiplier)
            {
                int a = Random.Range(0, 30);
                skill data=new skill();
                skillData current = SkillData.SkillInfo[a];
                data.skillID = current.skillID;
                data.level = Random.Range(1, current.levels.Count);
                s.skills.Add(data);
            }
            if (Random.Range(0f, 1f) < 0.5 && s.skills.Count > 4) return;
        }
    }

    private void JumpListGener(Skater s, ActiveComp c)
    {
        float ageMultiplier = 1f;
        if (s.age <= 9) ageMultiplier = 0.3f;
        else if (s.age <= 13) ageMultiplier = 0.5f;
        else if (s.age <= 17) ageMultiplier = 0.7f;
        else if (s.age <= 21) ageMultiplier = 1.0f;
        else if (s.age <= 25) ageMultiplier = 0.8f;
        else ageMultiplier = 0.6f;

        while (s.jumpTypes.Count < 8)
        {
            float k = Random.Range(0f, 1f);
            switch (c.compType)
            {
                case CompType.Local:
                    k -= 0.2f;
                    break;
                case CompType.District:
                    k -= 0.15f;
                    break;
                case CompType.Provincial:
                    k -= 0.1f;
                    break;
                case CompType.National:
                    k += 0.15f;
                    break;
                case CompType.International:
                    k += 0.2f;
                    break;
            }


            int rotation = 1;
            if (Random.Range(0f, 1f) < k * ageMultiplier) rotation += 1;
            if (Random.Range(0f, 1f) < k * ageMultiplier - 0.1f) rotation += 1;
            if (Random.Range(0f, 1f) < k * ageMultiplier - 0.15f) rotation += 1;
            if (Random.Range(0f, 1f) < k * ageMultiplier - 0.25f) rotation += 1;
            if (rotation > 4) rotation = 4;

            switch (s.jumpTypes.Count)
            {
                case 0:
                    {
                        JumpRotationData data = GetJumpRotationData(jumpType.Toeloop, rotation);
                        s.jumpTypes.Add(new JumpData(jumpType.Toeloop, rotation, 4, data.staminaCost, data.baseScore));
                        break;
                    }
                case 1:
                    {
                        JumpRotationData data = GetJumpRotationData(jumpType.Salchow, rotation);
                        s.jumpTypes.Add(new JumpData(jumpType.Salchow, rotation, 4, data.staminaCost, data.baseScore));
                        break;
                    }
                case 2:
                    {
                        JumpRotationData data = GetJumpRotationData(jumpType.Loop, rotation);
                        s.jumpTypes.Add(new JumpData(jumpType.Loop, rotation, 4, data.staminaCost, data.baseScore));
                        break;
                    }
                case 3:
                    {
                        JumpRotationData data = GetJumpRotationData(jumpType.Flip, rotation);
                        s.jumpTypes.Add(new JumpData(jumpType.Flip, rotation, 4, data.staminaCost, data.baseScore));
                        break;
                    }
                case 4:
                    {
                        JumpRotationData data = GetJumpRotationData(jumpType.Lutz, rotation);
                        s.jumpTypes.Add(new JumpData(jumpType.Lutz, rotation, 4, data.staminaCost, data.baseScore));
                        break;
                    }
                case 5:
                    {
                        JumpRotationData data = GetJumpRotationData(jumpType.Axel, rotation);
                        s.jumpTypes.Add(new JumpData(jumpType.Axel, rotation, 4, data.staminaCost, data.baseScore));
                        break;
                    }
                default:
                    {
                        JumpRotationData d1 = GetJumpRotationData(jumpType.Spin, rotation);
                        s.jumpTypes.Add(new JumpData(jumpType.Spin, rotation, 4, d1.staminaCost, d1.baseScore));
                        JumpRotationData d2 = GetJumpRotationData(jumpType.StepSequence, rotation);
                        s.jumpTypes.Add(new JumpData(jumpType.StepSequence, rotation, 4, d2.staminaCost, d2.baseScore));
                        break;
                    }
            }
        }
    }
    #endregion

    #region 表演赛相关
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
    #endregion

    #region skill system
    private void LoadSkill()
    {
        TextAsset json = Resources.Load<TextAsset>("Skills");
        if (json != null)
            SkillData = JsonUtility.FromJson<skillList>(json.text);
    }

    //创建技能,概率问题在调用的时候调整
    public void CreateSkill(Skater S, string ID)
    {
        S.skills.Add(new skill { skillID = ID, level = 1 });
    }

    public skillData GetSkillByID(string id)
    {
        foreach (var sk in SkillData.SkillInfo)
        {
            if (sk.skillID == id) return sk;
        }
        return null;
    }

    //skill 效果
    private void ApplySkills(Skater S, JumpData jump, ref float skillBonus, ref float extraScore)
    {
        foreach (var sk in S.skills)
        {
            skillData config = GetSkillByID(sk.skillID);
            if (config == null) continue;
            if (sk.level < 1 || sk.level > config.levels.Count) continue;
            SkillLevelData lv = config.levels[sk.level - 1];

            if (Random.Range(0f, 1f) >= lv.triggerRate) continue; // 没触发就跳过

            switch (config.Type)
            {
                case skillType.jump:
                    if (jump.jumpName == config.matchJump)
                        skillBonus += lv.number;
                    break;
                case skillType.stamina:
                    S.stamina += (int)lv.number;
                    break;
                case skillType.score:
                    extraScore += lv.number;
                    break;
                case skillType.step:
                    if (jump.jumpName == config.matchJump)
                        skillBonus += lv.number;
                    break;
            }
        }
    }
    #endregion

    #region 读写jumpConfig
    public void jumpConfig()
    {
        TextAsset json = Resources.Load<TextAsset>("jumpConfigs");
        if (json != null) jumpConfigData = JsonUtility.FromJson<JumpConfigList>(json.text);
    }
    #endregion

    #region 获取json配置
    public JumpRotationData GetJumpRotationData(jumpType type, int k)
    {
        foreach (var config in jumpConfigData.JumpConfigs)
        {
            if (config.jumpName == type)
            {
                foreach (var rot in config.rotations)
                {
                    if (rot.rotation == k) return rot;
                }
            }
        }
        return null;
    }
    #endregion
}