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
        List<string> TextToShow = new List<string>();
        
        for (int i = 0; i < allSkaters.Count; i++)
        {
            var S = allSkaters[i];
            if (S.stamina < 50) S.stamina = 100;
            float totalScore = 0f;
            string text = $"{S.name}的表演开始啦--\n";

            float k = 0.5f;
            int order = i + 1;
            if (order % 4 == 1) k -= 0.1f;
            else if (order % 4 == 0) k -= 0.15f;
            else k += 0.05f;

            // 节目单
            List<ShowElement> program;
            bool hasProgram = false;
            foreach (var se in S.ShowList)
            {
                if (se.jump.Count > 0) { hasProgram = true; break; }
            }

            if (hasProgram)
            {
                program = S.ShowList;
            }
            else if (S.jumpTypes.Count > 0)
            {
                program = new List<ShowElement>();
                List<JumpData> sorted = new List<JumpData>(S.jumpTypes);
                sorted.Sort((a, b) => b.baseScore.CompareTo(a.baseScore));
                foreach (var j in sorted)
                {
                    ShowElement se = new ShowElement();
                    se.jump.Add(j);
                    program.Add(se);
                }
            }
            else
            {
                program = new List<ShowElement>();
            }

            foreach (var element in program)
            {
                if (element.jump.Count == 0) continue;
                if (element.jump.Count > 1)
                {
                    text += "【联跳】\n";
                }
                for (int e = 0; e < element.jump.Count; e++)
                {
                    var jump = element.jump[e];
                    float comboMultiplier = (e == 0) ? 1f : 0.8f;

                    float currentBase = jump.baseScore;
                    int currentCost = jump.staminaCost;
                    int currentRotation = jump.rotation;

                    // 超绝爆发：概率提升一圈
                    if (jump.rotation < jump.maxRotation && Random.Range(0f, 1f) < 0.05f)
                    {
                        text += "想要超常发挥!\n";
                        currentRotation += 1;
                        JumpRotationData upData = GetJumpRotationData(jump.jumpName, currentRotation);
                        if (upData != null)
                        {
                            currentBase = upData.baseScore;
                            currentCost = upData.staminaCost;
                        }
                    }

                    while (S.stamina <= currentCost&&currentRotation > 1)
                    {
                        currentRotation -= 1;
                        JumpRotationData upData = GetJumpRotationData(jump.jumpName,currentRotation);
                        if (upData != null)
                        {
                            currentBase = upData.baseScore;
                            currentCost = upData.staminaCost;
                        }
                        else break;
                    }

                    S.stamina -= currentCost;

                    // 体力影响成功率
                    float staminaPenalty = 0f;
                    if (S.stamina < 30) staminaPenalty = -0.2f;
                    else if (S.stamina < 60) staminaPenalty = -0.1f;

                    // 技能
                    float skillBonus = 0f;
                    float extraScore = 0f;
                    ApplySkills(S, jump, ref skillBonus, ref extraScore,ref text);

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
                    text += $"{currentRotation}" + GetText(jump.jumpName);
                    if (element.jump.Count > 1&& element.jump.Count != e + 1) text += "+";
                    if (roll > finalK)
                    {
                        float failRoll = Random.Range(0f, 1f);
                        if (failRoll > finalK - 0.1f)
                        {
                            goe = -3f;
                        }
                        else  
                        { 
                            goe = -1f;
                        }
                    }
                    else
                    {
                        float successRoll = Random.Range(0f, 1f);
                        if (successRoll < 0.5f)
                            { goe = 1f;  }
                        else if (successRoll < 0.75f)
                            {goe = 2f;
                        }
                        else if (successRoll < 0.9f)
                            { goe = 3f; }
                        else
                            { goe = 5f; }
                    }

                    if(element.jump.Count==e+1)
                    {
                        switch (goe)
                        {
                            case -3f:
                                text += "❌摔倒...\n";
                                break;
                            case -1f:
                                text += "⚠️落冰不稳\n";
                                break;
                            case 1f:
                                text += "✓落冰成功\n";
                                break;
                            case 2f:
                                text += "✓✓完美落冰\n";
                                break;
                            case 3f:
                                text += "✨发挥超棒\n";
                                break;
                            case 5f:
                                text += "🌟🌟🌟GOE满分！\n";
                                break;
                        }
                    }
                    
                    float jumpScore = (currentBase * comboMultiplier + extraScore) * (1f + goe / 10f);
                    totalScore += jumpScore;
                }
            }

            foreach (var item in GameManager.Instance.currentSaveData.Skaters)
            {
                if(item==S) {
                    TextToShow.Add(text);
                    continue;
                }
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

        foreach(var item in comp.skaterNames)
        {
            foreach(var s in GameManager.Instance.currentSaveData.Skaters)
            {
                if (item == s.name) s.isCompeting = false;
            }
        }

        // ===== 6. 弹窗 =====
        string msg = "";
        for (int i = 0; i < allSkaters.Count; i++)
        {
            string marker = mySkaters.Contains(allSkaters[i]) ? "★" : "";
            msg += ($"第{i + 1}名: {scores[allSkaters[i]]:F1}分 {marker} {allSkaters[i].name}\n");
        }
        PopupManager.Instance.CompPop(comp.compName, msg, () =>
       { PopupManager.Instance.MessagePop(TextToShow); });
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
        ShowListGener(s);

        return s;
    }

    private JumpData FindJump(Skater s, jumpType type)
    {
        foreach (var j in s.jumpTypes)
        {
            if (j.jumpName == type) return j;
        }
        return null;
    }

    private JumpData FindBest(Skater s, int TP, int SC, int LP, int FP, int LZ, int AL, int spinTime, int stepTime, int jumpTime)
    {
        JumpData best = null;
        foreach (var j in s.jumpTypes)
        {
            // 跳过已用满的
            if (j.jumpName == jumpType.Toeloop && TP >= 2) continue;
            if (j.jumpName == jumpType.Salchow && SC >= 2) continue;
            if (j.jumpName == jumpType.Loop && LP >= 2) continue;
            if (j.jumpName == jumpType.Flip && FP >= 2) continue;
            if (j.jumpName == jumpType.Lutz && LZ >= 2) continue;
            if (j.jumpName == jumpType.Axel && AL >= 2) continue;
            if (j.jumpName == jumpType.Spin && spinTime >= 1) continue;
            if (j.jumpName == jumpType.StepSequence && stepTime >= 2) continue;
            // 跳跃已满7个就跳过跳跃类
            if (jumpTime >= 7 && j.jumpName <= jumpType.Axel) continue;

            if (best == null || j.baseScore > best.baseScore) best = j;
        }
        return best;
    }

    public void ShowListGener(Skater s)
    {
        // 清空所有坑
        foreach (var se in s.ShowList)
        {
            se.jump.Clear();
        }

        float ageMultiplier = 1f;
        if (s.age <= 9) ageMultiplier = 0.3f;
        else if (s.age <= 13) ageMultiplier = 0.5f;
        else if (s.age <= 17) ageMultiplier = 0.8f;
        else if (s.age <= 21) ageMultiplier = 1.25f;
        else if (s.age <= 25) ageMultiplier = 0.9f;
        else ageMultiplier = 0.6f;

        int jumpTime = 0, stepTime = 0, spinTime = 0;
        int combineJump2 = 0, combineJump3 = 0;
        int TP = 0, SC = 0, LP = 0, FP = 0, LZ = 0, AL = 0;

        for (int i = 0; i < s.ShowList.Count; i++)
        {
            var se = s.ShowList[i];

            // ===== 最后3个坑：优先填 Spin 和 StepSequence =====
            if (i >= 7)
            {
                if (spinTime < 1)
                {
                    JumpData spin = FindJump(s, jumpType.Spin);
                    if (spin != null) { se.jump.Add(spin); spinTime++; continue; }
                }
                else if (stepTime < 2)
                {
                    JumpData step = FindJump(s, jumpType.StepSequence);
                    if (step != null) { se.jump.Add(step); stepTime++; continue; }
                }
                continue; // 没有就留空
            }

            // ===== 第7个跳跃坑：如果还没有Axel，强制放一个 =====
            if (i == 6 && AL == 0)
            {
                JumpData axel = FindJump(s, jumpType.Axel);
                if (axel != null)
                {
                    se.jump.Add(axel);
                    AL++; jumpTime++;
                    continue;
                }
            }

            // ===== 决定这个坑是单跳还是连跳 =====
            int loop = 1;
            if (combineJump3 < 1 && Random.Range(0f, 1f) < 0.3f * ageMultiplier)
                loop = 3;
            else if (combineJump2 < 2 && Random.Range(0f, 1f) < 0.4f * ageMultiplier)
                loop = 2;

            // 连跳数已满就强制单跳
            if (combineJump2 + combineJump3 >= 3) loop = 1;

            // ===== 填跳跃 =====
            for (int m = 0; m < loop; m++)
            {
                JumpData best;
                if (m == 0)
                {
                    // 第一跳：选基础分最高的
                    best = FindBest(s, TP, SC, LP, FP, LZ, AL, spinTime, stepTime, jumpTime);
                }
                else
                {
                    // 连跳第二、三跳：只能是 Toeloop 或 Loop
                    if (Random.Range(0f, 1f) < 0.5f)
                        best = FindJump(s, jumpType.Toeloop);
                    else
                        best = FindJump(s, jumpType.Loop);
                }

                if (best == null) break; // 没得选了就停

                // 计数
                if (best.jumpName == jumpType.Toeloop) { TP++; jumpTime++; }
                else if (best.jumpName == jumpType.Salchow) { SC++; jumpTime++; }
                else if (best.jumpName == jumpType.Loop) { LP++; jumpTime++; }
                else if (best.jumpName == jumpType.Flip) { FP++; jumpTime++; }
                else if (best.jumpName == jumpType.Lutz) { LZ++; jumpTime++; }
                else if (best.jumpName == jumpType.Axel) { AL++; jumpTime++; }

                se.jump.Add(best);
            }

            // 记录连跳数
            if (loop == 2) combineJump2++;
            else if (loop == 3) combineJump3++;
        }
    }

    public void AutoShowList(Skater s)
    {
        ShowListGener(s);
        PopupManager.Instance.RefreshShowList(s);
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

        int times = 0;

        while (s.jumpTypes.Count < 8&&times<12)
        {
            times++;
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
    private void ApplySkills(Skater S, JumpData jump, ref float skillBonus, ref float extraScore,ref string text)
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
            text += $"{config.skillName}发动成功!\n"+$"{config.description}\n";
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

    #region 获取缩写
    public string GetText(jumpType j)
    {
        switch (j)
        {
            case jumpType.Toeloop:
                return "T";
            case jumpType.Salchow:
                return "S";
            case jumpType.Loop:
                return "Lo";
            case jumpType.Flip:
                return "F";
            case jumpType.Lutz:
                return "Lz";
            case jumpType.Axel:
                return "A";
            default:
                return "";
        }
    }
    #endregion
}