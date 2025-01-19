
#if REW_LEGACY

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if LITJSON
using LitJson;
#endif

public class SkillEffect
{
    /// <summary>
    /// 10-物理伤害(固定)   11物理伤害(自身属性)    20-魔法伤害（固定）   21魔法伤害（自身属性）    3 - 治疗  4- 驱散   99 - buff
    /// </summary>
    public int type;

    /// <summary>
    /// 技能等级加成倍数
    /// </summary>
    public float rankFac;

    /// <summary>
    /// BUFF加成系数
    /// </summary>
    public float buffMultipFac;

    /// <summary>
    /// BUFF叠加层
    /// </summary>
    public int buffCompositionNumber;

    /// <summary>
    /// 值
    /// </summary>
    public float value;


    /// <summary>
    /// 计算显示在UI上数字的乘数
    /// </summary>
    /// <param name="eff"></param>
    /// <returns></returns>
    public static float CalDisplayMultip(Skill skill, SkillEffect eff, int skillRank)
    {
        float rankMultip = 1f + (skill.GetRankAddFac(skillRank) * eff.rankFac);

        return rankMultip;
    }
}

public class Skill
{
    // ****************** Serializable *******************

    /// <summary>
    /// ID
    /// </summary>
    public int id;

    /// <summary>
    /// 名称
    /// </summary>
    public string name;

    /// <summary>
    /// 描述替换数字
    /// </summary>
    public int[] descriptionValue;

    /// <summary>
    /// 描述文本
    /// </summary>
    public string description;


    /// <summary>
    /// 触发方式  0主动(主技能)   1-平A   2-被攻击时    3-战斗时秒数CD     4-脱离离战数秒CD    5-释放主动技能后
    /// </summary>
    public int trigger;

    /// <summary>
    /// 0-非指向性   1-指向   2-当前攻击目标
    /// </summary>
    public int type;

    /// <summary>
    /// CD
    /// </summary>
    public int cd;
    
    /// <summary>
    /// 1-自己   10-随机队友 11-血量少的队友 13-队伍第一个   20-随机敌人  21-血量少的敌人   30-友方群体  40-敌方群体
    /// </summary>
    public int aiSelect;

    /// <summary>
    /// 距离
    /// </summary>
    public int range;

    /// <summary>
    /// 半径
    /// </summary>
    public int radius;

    /// <summary>
    /// 分散度
    /// </summary>
    public int scatter;


    /// <summary>
    /// 施放特效
    /// </summary>
    public string muzzle;

    /// <summary>
    /// 抛射物
    /// </summary>
    public string projectile;

    /// <summary>
    /// 抛射类型  0-直线 1-抛出 2回旋
    /// </summary>
    public int projectileType;

    /// <summary>
    /// 抛射体基础数量
    /// </summary>
    public int projectileCount;

    /// <summary>
    /// 抛射物速度
    /// </summary>
    public int projectileSpeed;

    /// <summary>
    /// 0-己方受影响 1-敌方受影响 2-无差别效果
    /// </summary>
    public int influence;

    /// <summary>
    /// 效果
    /// </summary>
    public SkillEffect[] effects;

    /// <summary>
    /// 特效
    /// </summary>
    public string fx;








    public float GetRankAddFac(int skillRank)
    {
        return (0.25f * skillRank);
    }


#if LITJSON
    public static Skill[] Deserialize(string json)
    {
        JsonData jSkills = JsonMapper.ToObject(json)["skills"];

        Skill[] skills = new Skill[jSkills.Count];

        for (int i = 0; i < jSkills.Count; i++)
        {
            skills[i] = new Skill();
            skills[i].id = (int)jSkills[i]["id"];
            skills[i].name = (string)jSkills[i]["name"];
            skills[i].descriptionValue = jSkills[i].ContainsKey("descriptionValue") ? UtilsLitJson.GetArray<int>(jSkills[i]["descriptionValue"]) : new int[] {};
            skills[i].description = jSkills[i].ContainsKey("description") ? (string)jSkills[i]["description"] : "";
            skills[i].trigger = System.Convert.ToInt32( (string)jSkills[i]["trigger"] );
            skills[i].type = (int)jSkills[i]["type"];
            skills[i].cd = (int)jSkills[i]["cd"];
            skills[i].aiSelect = (int)jSkills[i]["aiSelect"];
            skills[i].range = (int)jSkills[i]["range"];
            skills[i].radius = (int)jSkills[i]["radius"];
            skills[i].scatter = (int)jSkills[i]["scatter"];
            skills[i].muzzle = (string)jSkills[i]["muzzle"];
            skills[i].projectile = (string)jSkills[i]["projectile"];
            skills[i].projectileType = (int)jSkills[i]["projectileType"];
            skills[i].projectileCount = (int)jSkills[i]["projectileCount"];
            skills[i].projectileSpeed = (int)jSkills[i]["projectileSpeed"];
            skills[i].influence = (int)jSkills[i]["influence"];

            skills[i].effects = new SkillEffect[jSkills[i]["effects"].Count];
            for (int j = 0; j < jSkills[i]["effects"].Count; j++)
            {
                skills[i].effects[j] = new SkillEffect();
                skills[i].effects[j].type = (int)jSkills[i]["effects"][j]["type"];
                skills[i].effects[j].rankFac = jSkills[i]["effects"][j].ContainsKey("rankFac") ? Convert.ToSingle((string)jSkills[i]["effects"][j]["rankFac"]) : 0f;
                skills[i].effects[j].buffMultipFac = jSkills[i]["effects"][j].ContainsKey("buffMultipFac") ? Convert.ToSingle( (string)jSkills[i]["effects"][j]["buffMultipFac"] ) : 0f;
                skills[i].effects[j].buffCompositionNumber = jSkills[i]["effects"][j].ContainsKey("buffCompositionNumber") ? (int)jSkills[i]["effects"][j]["buffCompositionNumber"] : 0;
                skills[i].effects[j].value = Convert.ToSingle( (string)jSkills[i]["effects"][j]["value"] );
            }

            skills[i].fx = (string)jSkills[i]["fx"];
        }

        return skills;
    }
#endif
}


public class Skill_Inst
{
    public Skill rawSkill;

    public int rank;

    public float remainTime;

    public List<Buff> permnentBuffs = new List<Buff>();

    public Skill_Inst(Skill sk, int rank = 0)
    {
        this.rawSkill = sk;
        this.rank = rank;
        this.remainTime = 0f;
    }



    public float GetRankAddFac()
    {
        return this.rawSkill.GetRankAddFac(rank);
    }
}





//public class Skill_Buff : Skill
//{

//}
//public class Skill_OnAttack : Skill
//{

//}
//public class Skill_OnGetDamage
//{

//}






public class SkillSystem : SubComponent
{
    
    private List<Skill_Inst> skills = new List<Skill_Inst>();

    public BuffSystem buffSystem = null;

    public Skill_Inst readyActiveSkill = null;

    public SkillSystem(MonoBehaviour owner) : base(owner)
    {
    }




    /// <summary>
    /// 设置角色固定技能 （初始化固定技能）    (调用RegisterSkill)（其他也调用RegisterSkill）
    /// </summary>
    /// <param name="delay"></param>
    public void ResetPetSkillsAfter(float delay = 0f)
    {
        if(delay == 0f)
        {
            ResetPetSkills();
        }
        else
        {
            owner.StartCoroutine(CoResetPetSkills(delay));
        }
    }
    private IEnumerator CoResetPetSkills(float delay)
    {
        yield return new WaitForSeconds(delay);

        ResetPetSkills();
    }
    private void ResetPetSkills()
    {
        //pet instance
        var petinst = (this.owner as Pet).PetInstance;   if (petinst == null) { Debug.LogAssertion("错误： pet instance 为空！"); return; }

        //skills
        var petskills = (this.owner as Pet).PetInfo.skills;

        //Update Rank
        petinst.CheckSkillLvl();

        //Remove Legacy
        var legacySkill =  this.skills.Where(sk => petskills.Contains(sk.rawSkill.id)).ToArray();
        for (int i = 0; i < legacySkill.Length; i++)
        {
            UnRegisterSkill(legacySkill[i]);
        }

        //Register
        for (int i = 0; i < petskills.Length; i++)
        {
            var skillInfo = Infomanager.Instance.skills.FirstOrDefault(ski => ski.id == petskills[i]);
            int rank = petinst.skillLvl[i];
            this.RegisterSkill(skillInfo, rank);
        }
    }
    


    /// <summary>
    /// 获取所有技能
    /// </summary>
    /// <returns></returns>
    public Skill_Inst[] GetAllSkills()
    {
        return skills.ToArray();
    }

    /// <summary>
    /// 注册技能
    /// </summary>
    /// <param name="newSkill"></param>
    public void RegisterSkill(Skill newSkill, int rank = 0)
    {
        if (newSkill == null) return;
        if (skills.Any(ski => ski.rawSkill == newSkill)) { Debug.LogAssertion("不能存在相同技能"); return; }

        var skillInst = new Skill_Inst(newSkill, rank);
        skills.Add(skillInst);
        
        //初始化触发trigger==99 | 999
        if (skillInst.rawSkill.trigger == 99 || skillInst.rawSkill.trigger == 999)
        {
            bool success = SkillInvoke(skillInst, 1, 1);
            Debug.Log("<color=#00FF00>" + owner.gameObject.name + "释放被动技能：" + skillInst.rawSkill.name + "  成功？" + success + "</color>");
        }
    }
    
    /// <summary>
    /// 注销技能
    /// </summary>
    /// <param name="skill"></param>
    public void UnRegisterSkill(Skill skill)
    {
        var targetInst = skills.FirstOrDefault(sk => sk.rawSkill == skill);
        if (targetInst == null) { Debug.LogAssertion("要移除的技能不存在"); return; }

        UnRegisterSkill(targetInst);
    }
    public void UnRegisterSkill(Skill_Inst skillInstance)
    {
        //移除技能
        skills.Remove(skillInstance);

        //注销技能对应的永久BUFF
        UnRegisterSkillBuffs(skillInstance);
    }




    //UPDATE
    public override void Update(float deltaTime)
    {
        bool anyNewSkillReady = false;
        bool anySkillReady = false;

        //FOR Each
        skills.ForEach(sk => {
            sk.remainTime -= deltaTime;

            if(sk.rawSkill.trigger == 0 && sk.remainTime <= 0f)
            {
                anySkillReady = true;

                if (sk.remainTime + deltaTime > 0f)
                    anyNewSkillReady = true;
            }
        });

        //Reset Ready
        if (anyNewSkillReady || (readyActiveSkill == null && anySkillReady))
        {
            ResetReadyActiveSkill();
        }
    }


    //RESET ACTIVE
    private void ResetReadyActiveSkill()
    {
        Debug.Log("---RESET ACTIVE SKILL");

        var skillsReady = skills
            .Where(sk => sk.rawSkill.trigger == 0)
            .Where(sk => sk.remainTime <= 0f)
            .OrderBy(sk => sk.remainTime); //可施放的主动技能 (冷却顺序排列)

        readyActiveSkill = skillsReady.FirstOrDefault();
    }

    
    //主动施放 0
    public bool OnInvoke(Skill_Inst skillInst)
    {
        if (skillInst != null)
        {
            bool success = SkillInvoke(skillInst, 
                (int)(owner as Pet).SkillRepeat, 
                skillInst.rawSkill.projectileCount + (int)(owner as Pet).SkillProjectileCount);

            if(success)
            {
                //reset cd
                skillInst.remainTime = (skillInst.rawSkill.cd * (owner as Pet).SkillCD);

                //ready reset
                if (skillInst == readyActiveSkill)
                    readyActiveSkill = null;

                //text
                if (skillInst.rawSkill.trigger == 0)
                    FloatUp.Show("[" + skillInst.rawSkill.name + "]", ((this.owner as Pet).transform.position + Vector3.up * 2f), Color.blue, 0.5f, true, false, true, false);

            }

            return success;
        }
        
        return false;
    }

    //主动释放后 5
    public void OnAfterInvoke()
    {
        skills.ForEach(s => {
            if (s.rawSkill.trigger == 5)
            {
                bool success = SkillInvoke(s,
                    (int)(owner as Pet).SkillRepeat,
                    s.rawSkill.projectileCount + (int)(owner as Pet).SkillProjectileCount);
                if (success)
                {
                    s.remainTime = (s.rawSkill.cd * (owner as Pet).SkillCD);
                }
            }
        });
    }

    //受击 2
    public void OnHit(Pet attacker = null)
    {
        if (attacker == null) return;

        skills.ForEach(s => {
            if(s.rawSkill.trigger == 2)
            {
                bool success = SkillInvoke(s,
                    (int)(owner as Pet).SkillRepeat,
                    s.rawSkill.projectileCount + (int)(owner as Pet).SkillProjectileCount, 
                    attacker);
                if (success) s.remainTime = (s.rawSkill.cd * (owner as Pet).SkillCD);
            }
        });
    }

    //攻击 1
    public void OnAttack(Pet target)
    {
        if (target == null) return;

        skills.ForEach(s => {
            if(s.rawSkill.trigger == 1)
            {
               bool success = SkillInvoke(s,
                   (int)(owner as Pet).SkillRepeat,
                   s.rawSkill.projectileCount + (int)(owner as Pet).SkillProjectileCount, 
                   target);
               if (success) s.remainTime = (s.rawSkill.cd * (owner as Pet).SkillCD);
            }
        });
    }

    //进入战斗 秒数 3
    public void OnEngageSec()
    {
        //if((this.owner as Pet).team == 0) Debug.Log("---" + this.owner.gameObject.name + " engage sec ...");

        skills.ForEach(s => {
            if(s.rawSkill.trigger == 3 && s.remainTime <= 0f)
            {
                //if ((this.owner as Pet).team == 0)  Debug.Log("---" + this.owner.gameObject.name + "✔ skill:" + s.rawSkill.name);
                bool success = SkillInvoke(s,
                    (int)(owner as Pet).SkillRepeat,
                    s.rawSkill.projectileCount + (int)(owner as Pet).SkillProjectileCount);
                if (success) s.remainTime = (s.rawSkill.cd * (owner as Pet).SkillCD);
            }
            else
            {
                //if ((this.owner as Pet).team == 0)  Debug.Log("---" + this.owner.gameObject.name + "× skill " + s.rawSkill.name + " cant use rcd:" + s.remainTime);
            }
        });
    }

    //脱离战斗 秒数 ？？
    public void OnFollowSec()
    {
        //已弃用
        //skills.ForEach(s =>
        //{
        //    if (s.rawSkill.trigger == 4 && s.remainTime <= 0f)
        //    {
        //        SkillInvoke(s,
        //            (int)(owner as Pet).SkillRepeat,
        //            s.rawSkill.projectileCount + (int)(owner as Pet).SkillProjectileCount);
        //    }
        //});
    }

    //所有状态 秒数 4
    public void OnAllTimeSec()
    {
        skills.ForEach(s => {
            if (s.rawSkill.trigger == 4 && s.remainTime <= 0f)
            {
                bool success = SkillInvoke(s,
                    (int)(owner as Pet).SkillRepeat,
                    s.rawSkill.projectileCount + (int)(owner as Pet).SkillProjectileCount);
                if (success) s.remainTime = (s.rawSkill.cd * (owner as Pet).SkillCD);
            }
        });
    }
    
    //杀敌后 - 6
    public void OnKillPet(Pet petKill)
    {
        skills.ForEach(s => {
            if (s.rawSkill.trigger == 6 && s.remainTime <= 0f)
            {
                bool success = SkillInvoke(s,
                    (int)(owner as Pet).SkillRepeat,
                    s.rawSkill.projectileCount + (int)(owner as Pet).SkillProjectileCount);
                if (success) s.remainTime = (s.rawSkill.cd * (owner as Pet).SkillCD);
            }
        });
    }





















    /// <summary>
    /// 释放技能 (初始化释放时repeat须为0)  
    /// </summary>
    /// <param name="skillInst"></param>
    /// <param name="repeat"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private bool SkillInvoke(Skill_Inst skillInst, int repeat, int projectileCount, params object[] args) //obj[0]是target或者attacker
    {
        if (skillInst.remainTime > 0f)  { return false; }

        bool success = SkillInvokeOnce(skillInst, projectileCount, args);

        owner.StartCoroutine(CoSkillInvoke( skillInst, repeat - 1, projectileCount, args ));
        
        return success;
    }
    private IEnumerator CoSkillInvoke(Skill_Inst skillInst, int remainCount, int projectileCount, params object[] args)
    {
        for (int i = 0; i < remainCount; i++)
        {
            yield return new WaitForSeconds(0.5f);

            SkillInvokeOnce(skillInst, projectileCount, args);
        }
    }

    /// <summary>
    /// 释放技能一次
    /// </summary>
    /// <param name="skillInst"></param>
    /// <param name="projectileCount"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private bool SkillInvokeOnce(Skill_Inst skillInst, int projectileCount, params object[] args) //obj[0]是target或者attacker 
    {
        // *** owener ***
        Pet ownerPet = (this.owner as Pet);


        // *** skill type *** 
        Vector3 pos = Vector3.zero;
        List<Pet> pets = null;
        switch (skillInst.rawSkill.type)
        {
            case 0: //非指向性（瞬发）
                {
                    if(skillInst.rawSkill.radius == 0) //自己
                    {
                        pos = this.owner.transform.position;
                        pets = new List<Pet> { (this.owner as Pet) };

                        //apply
                        SkillSystem.ApplySkillEffects(skillInst, ownerPet, pets, pos);

                        return true;
                    }
                    else //自己周围
                    {
                        pos = this.owner.transform.position;
                        pets = SkillOverlapPets(skillInst, pos, (owner as Pet));

                        //overlap pets
                        if (pets == null || pets.Count < 1) return false;

                        //apply
                        SkillSystem.ApplySkillEffects(skillInst, ownerPet, pets, pos);

                        return true;
                    }
                }
            case 1:  //指向性
                {
                    //AI SELECT
                    Pet tgt;
                    bool sucess = AISelect(skillInst.rawSkill, out pos, out tgt);
                    if (!sucess) return false;


                    //抛射物指向性技能
                    if (skillInst.rawSkill.projectile != "")
                    {
                        //apply to projectile

                        //单体的抛射指向技能
                        if(skillInst.rawSkill.radius == 0)
                        {
                            Pet petTrace = SkillSystem.SkillOverlapPets(skillInst, pos, ownerPet).FirstOrDefault();
                            if (petTrace == null) return false;

                            for (int i = 0; i < projectileCount; i++)
                            {
                                SkillSystem.GenerateProjectile(skillInst, ownerPet, (pos + UnityEngine.Random.onUnitSphere.OnOceanPlane() * skillInst.rawSkill.scatter), petTrace, i);
                            }
                        }
                        //范围的抛射指向技能
                        else
                        {
                            for (int i = 0; i < projectileCount; i++) 
                            {
                                SkillSystem.GenerateProjectile(skillInst, ownerPet, (pos + UnityEngine.Random.onUnitSphere.OnOceanPlane() * skillInst.rawSkill.scatter), null, i);
                            }
                        }

                        return true;
                    }
                    //瞬发指向性技能
                    else
                    {
                        //单体瞬发指向技能
                        if (skillInst.rawSkill.radius == 0)
                        {
                            pets = new List<Pet>() { tgt };
                        }
                        //群体瞬发指向技能
                        else
                        {
                            pets = SkillOverlapPets(skillInst, pos, (owner as Pet));
                        }

                        //overlap pets
                        if (pets == null || pets.Count < 1) return false;

                        //apply
                        SkillSystem.ApplySkillEffects(skillInst, ownerPet, pets, pos);

                        return true;
                    }
                }
            case 2: //当前攻击目标
                {
                    if (args.Length < 1) return false;
                    if (!(args[0] is Pet)) return false;

                    pos = (args[0] as Pet).transform.position;
                    pets = new List<Pet> { args[0] as Pet };

                    //抛射物追踪
                    if (skillInst.rawSkill.projectile != "")
                    {
                        for (int i = 0; i < projectileCount; i++)
                        {
                            SkillSystem.GenerateProjectile(skillInst, ownerPet, (pos + UnityEngine.Random.onUnitSphere.OnOceanPlane() * skillInst.rawSkill.scatter), pets[0], i);
                        }

                        return true;
                    }
                    //瞬发起效
                    else
                    {
                        SkillSystem.ApplySkillEffects(skillInst, ownerPet, pets, pos);

                        return true;
                    }

                }
            default:
                return false;
        }
    }



    /// <summary>
    /// 生成技能抛射物（目前无效字段：pets）
    /// </summary>
    /// <param name="skillInst"></param>
    /// <param name="sysowner"></param>
    /// <param name="pets"></param>
    /// <param name="pos"></param>
    public static void GenerateProjectile(Skill_Inst skillInst, Pet sysowner, Vector3 pos, Pet tracePet = null, int projectileIdx = 0)
    {
        //MUZZLE
        if(skillInst.rawSkill.muzzle != null)
        {
            GameObject muzzlePrefab = Resources.Load<GameObject>("Prefabs/FX/Muzzles/" + skillInst.rawSkill.muzzle);
            if(muzzlePrefab != null)
            {
                GameObject muzzleFx = GameObject.Instantiate(muzzlePrefab, sysowner.FirePos, Quaternion.LookRotation(pos - sysowner.FirePos), sysowner.transform);
                GameObject.Destroy(muzzleFx, 2f);
            }
        }

        //GEN
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Projectiles/SkillProjectile"), sysowner.FirePos, Quaternion.LookRotation(pos - sysowner.FirePos), null);
        SkillProjectile projectileScript = obj.GetComponent<SkillProjectile>();

        projectileScript.skillInst = skillInst;
        projectileScript.targetPosition = pos;
        projectileScript.invoker = (sysowner);
        projectileScript.trace = tracePet;

        projectileScript.Initialize(projectileIdx);
    }


    /// <summary>
    /// 应用技能效果
    /// </summary>
    /// <param name="skillInst">技能实例</param>
    /// <param name="sysowner">技能释放者</param>
    /// <param name="pets">释放目标对象</param>
    /// <param name="pos">生效位置（特效用）</param>
    public static void ApplySkillEffects(Skill_Inst skillInst, Pet sysowner, List<Pet> pets, Vector3 pos, bool callbyprojectile = false)
    {
        try
        {
            // *** effect *** 
            foreach (var eff in skillInst.rawSkill.effects)
            {
                float rankMultip = 1f + (skillInst.GetRankAddFac() * eff.rankFac);

                switch (eff.type)
                {
                    case 10: //物理伤害 -- 固定
                        {
                            pets.ForEach(p => {
                                p.GetPhysicDamage((int)(eff.value * rankMultip), sysowner.gameObject);
                            });
                        }
                        break;

                    case 11: //物理伤害 -- 攻击力倍数
                        {
                            pets.ForEach(p => {
                                p.GetPhysicDamage((int)(eff.value * rankMultip * sysowner.Atk), sysowner.gameObject);
                            });
                        }
                        break;
                    case 12: //物理伤害 -- 敌人攻击力倍数
                        {
                            pets.ForEach(p => {
                                p.GetPhysicDamage((int)(eff.value * rankMultip * p.Atk), sysowner.gameObject);
                            });
                        }
                        break;
                    case 13: //物理伤害 -- 敌人最大生命值
                        {
                            pets.ForEach(p => {
                                p.GetPhysicDamage((int)(eff.value * rankMultip * p.MaxHp), sysowner.gameObject);
                            });
                        }
                        break;


                    case 20: //魔法伤害 -- 固定
                        {
                            pets.ForEach(p => {
                                p.GetMagicDamage((int)(eff.value * rankMultip), sysowner.gameObject);
                            });
                        }
                        break;
                    case 21: //魔法伤害 -- 法强倍数
                        {
                            pets.ForEach(p => {
                                p.GetMagicDamage((int)(eff.value * rankMultip * sysowner.MagicStrength), sysowner.gameObject);
                            });
                        }
                        break;
                    case 22: //魔法伤害 -- 敌人法强
                        {
                            pets.ForEach(p => {
                                p.GetMagicDamage((int)(eff.value * rankMultip * p.MagicStrength), sysowner.gameObject);
                            });
                        }
                        break;
                    case 23: //魔法伤害 -- 敌人最大生命值
                        {
                            pets.ForEach(p => {
                                p.GetMagicDamage((int)(eff.value * rankMultip * p.MaxHp), sysowner.gameObject);
                            });
                        }
                        break;
                    case 24: //魔法伤害 -- 敌人损失生命值
                        {
                            pets.ForEach(p => {
                                p.GetMagicDamage((int)(eff.value * rankMultip * (p.MaxHp - p.Hp)), sysowner.gameObject);
                            });
                        }
                        break;

                    case 3: //治疗
                        {
                            pets.ForEach(p => {
                                p.GetHeal((int)(eff.value * rankMultip * sysowner.MagicStrength));
                            });
                        }
                        break;
                    case 31:
                        {
                            pets.ForEach(p => {
                                p.GetHeal((int)(eff.value * p.MaxHp * rankMultip));
                            });
                        }
                        break;

                    case 4: //驱散
                        {
                            pets.ForEach(p => {
                                p.QuSan();
                            });
                        }
                        break;

                    case 5: //消蓝
                        {
                            pets.ForEach(p => {
                                p.ManaLost((int)(eff.value * rankMultip));
                            });
                        }
                        break;

                    case 6: //回蓝
                        {
                            pets.ForEach(p => {
                                p.ManaGet((int)(eff.value * rankMultip));
                            });
                        }
                        break;

                    case 7: //位移
                        {
                            pets.ForEach(p => {
                                p.Impact((p.transform.position - pos).normalized * eff.value * rankMultip);
                            });
                        }
                        break;

                    case 99: //BUFF
                    case 999: // BUFF (层数)
                        {
                            pets.ForEach(p => {
                                p.skillSystem.RegisterSkillBuff(sysowner, skillInst, eff);
                            });
                        }
                        break;



                    case 666:
                        {
                            sysowner.GenChild((int)eff.value, sysowner.PetInstance.lvl, pos, 1f, 1f, eff.buffCompositionNumber);
                        }
                        break;


                    default:
                        break;
                }
            }



            //fx
            var fxPrefab = Resources.Load<GameObject>("Prefabs/FX/Skills/" + skillInst.rawSkill.fx);
            if (fxPrefab != null)
            {
                GameObject fxObj = GameObject.Instantiate(fxPrefab, pos, Quaternion.LookRotation(Vector3.up), null);

                var particalSys = fxObj.GetComponentInChildren<ParticleSystem>(true);

                GameObject.Destroy(fxObj, particalSys != null ? particalSys.main.duration : 5f);
            }
            else
            {
                //Debug.Log("---未找到特效");
            }
        }
        catch
        {

        }
    }






    /// <summary>
    /// AI选择
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="pos"></param>
    /// <param name="tgt"></param>
    /// <returns></returns>
    private bool AISelect(Skill skill, out Vector3 pos, out Pet tgt)
    {
        pos = new Vector3();

        switch (skill.aiSelect)
        {
            case 1: //自己
                {
                    pos = this.owner.transform.position;
                    tgt = this.owner.gameObject.GetComponent<Pet>();
                    return true;
                }
            case 10://随机队友
                {
                    int myTeam = (this.owner as Pet).team;
                    int[] targetTeams = myTeam == 0 ? new int[] { 0 } : new int[] { 1, 2 };

                    var petsInRange = RadiusOverlapPets(this.owner.transform.position, skill.range, targetTeams);

                    if (petsInRange.Count > 0)
                    {
                        tgt = petsInRange[UnityEngine.Random.Range(0, petsInRange.Count)];
                        pos = tgt.transform.position;
                        return true;
                    }
                    else
                    {
                        pos = this.owner.transform.position;
                        tgt = null;
                        return false;
                    }
                }
            case 11://血量少的队友
                {
                    int myTeam = (this.owner as Pet).team;
                    int[] targetTeams = myTeam == 0 ? new int[] { 0 } : new int[] { 1, 2 };

                    var petsInRange = RadiusOverlapPets(this.owner.transform.position, skill.range, targetTeams);

                    if (petsInRange.Count > 0)
                    {
                        tgt = petsInRange.OrderBy(p => ((float)p.Hp / (float)p.MaxHp)).ToArray()[0];
                        pos = tgt.transform.position;
                        return true;
                    }
                    else
                    {
                        pos = this.owner.transform.position;
                        tgt = null;
                        return false;
                    }
                }
            case 12://第一个队友
                {
                    int myTeam = (this.owner as Pet).team;
                    int[] targetTeams = myTeam == 0 ? new int[] { 0 } : new int[] { 1, 2 };

                    var petsInRange = RadiusOverlapPets(this.owner.transform.position, skill.range, targetTeams);
                    
                    var firstPetInHost = (this.owner as Pet).host.FirstPet;

                    if (firstPetInHost != null && petsInRange.Contains(firstPetInHost))
                    {
                        tgt = firstPetInHost;
                        pos = firstPetInHost.transform.position;
                        return true;
                    }
                    else
                    {
                        pos = this.owner.transform.position;
                        tgt = null;
                        return false;
                    }
                }
            case 20: //随机敌人
                {
                    int myTeam = (this.owner as Pet).team;
                    int[] targetTeams = myTeam == 0 ? new int[] { 1, 2 } : new int[] { 0 };

                    //var petsInRange = RadiusOverlapPets(this.owner.transform.position, skill.range, targetTeams);
                    //***Get Closest Target Host
                    var targetHost = GetClosestHost(targetTeams, true);
                    if (targetHost == null)
                    {
                        tgt = null;
                        pos = new Vector3();
                        return false;
                    }
                    var petsInRange = targetHost.AlivePets
                        .Where(p => (p.transform.position - owner.transform.position).sqrMagnitude < (skill.range * skill.range))
                        .ToList();


                    if (petsInRange.Count > 0)
                    {
                        tgt = petsInRange[UnityEngine.Random.Range(0, petsInRange.Count)];
                        pos = tgt.transform.position;
                        return true;
                    }
                    else
                    {
                        pos = this.owner.transform.position;
                        tgt = null;
                        return false;
                    }
                }
            case 21: //血量少的敌人
                {
                    int myTeam = (this.owner as Pet).team;
                    int[] targetTeams = myTeam == 0 ? new int[] { 1, 2 } : new int[] { 0 };

                    //var petsInRange = RadiusOverlapPets(this.owner.transform.position, skill.range, targetTeams);

                    //***Get Closest Target Host
                    var targetHost = GetClosestHost(targetTeams, true);
                    if (targetHost == null)
                    {
                        tgt = null;
                        pos = new Vector3();
                        return false;
                    }
                    var petsInRange = targetHost.AlivePets
                        .Where(p => (p.transform.position - owner.transform.position).sqrMagnitude < (skill.range * skill.range))
                        .ToList();


                    if (petsInRange.Count > 0)
                    {
                        tgt = petsInRange.OrderBy(p => ((float)p.Hp / (float)p.MaxHp)).ToArray()[0];
                        pos = tgt.transform.position;
                        return true;
                    }
                    else
                    {
                        pos = this.owner.transform.position;
                        tgt = null;
                        return false;
                    }
                }
            case 22: //当前攻击敌人
                {
                    int myTeam = (this.owner as Pet).team;
                    int[] targetTeams = myTeam == 0 ? new int[] { 1, 2 } : new int[] { 0 };
                    
                    if((this.owner as Pet).CurrentAttackTarget != null)
                    {
                        tgt = (this.owner as Pet).CurrentAttackTarget;
                        pos = (this.owner as Pet).CurrentAttackTarget.transform.position;
                        return true;
                    }
                    else
                    {
                        tgt = null;
                        pos = this.owner.transform.position;
                        return false;
                    }

                }
            default:
                {
                    pos = this.owner.transform.position;
                    tgt = null;
                    return false;
                }
        }
    }


    /// <summary>
    /// 最近的Host
    /// </summary>
    /// <param name="targetTeams">队伍列表</param>
    /// <param name="currentAttackingFirst"></param>（当前攻击的Host优先）team>
    /// <returns></returns>
    public IHost GetClosestHost(int[] targetTeams, bool currentAttackingFirst = false)
    {
        //优先当前攻击的目标队伍
        if(currentAttackingFirst && (owner as Pet).CurrentAttackTarget != null && targetTeams.Contains( (owner as Pet).CurrentAttackTarget.team) )
        {
            return (owner as Pet).CurrentAttackTarget.host;
        }

        //最近的队伍
        List<IHost> hosts = new List<IHost>();
        hosts.AddRange(GameObject.FindObjectsOfType<Spawner>());
        hosts.AddRange(GameObject.FindObjectsOfType<PlayerCharacter>());
        hosts.AddRange(GameObject.FindObjectsOfType<ArenaGridCell>().Where(ac => ac.PetId > -1));

        var myPos = (owner as Pet).hostObject.transform.position;
        var ordered = hosts
            .Where(h => h.gameObject.activeInHierarchy == true)
            .Where(h => targetTeams.Contains( h.Team ))
            .Where(h => h != (owner as Pet).host)
            .OrderBy(h => (h.gameObject.transform.position - myPos).sqrMagnitude)
            .ToArray();

        if (ordered.Length > 0)
            return ordered[0];
        else
            return null;
    }


    /// <summary>
    /// 半径覆盖
    /// </summary>
    public static List<Pet> RadiusOverlapPets(Vector3 pos, float radius, int[] teams)
    {
        var colliders = Physics.OverlapSphere(pos, radius);
        List<Pet> petList = new List<Pet>();
        foreach (var col in colliders)
        {
            Pet pet = col.GetComponent<Pet>();
            if (pet != null && !pet.IsDead && !pet.IsNeutral && teams.Contains(pet.team) && !petList.Contains(pet))
            {
                petList.Add(pet);
            }
        }
        return petList;
    }

    /// <summary>
    /// 技能影响的队伍
    /// </summary>
    /// <param name="skill_inst"></param>
    /// <param name="invoker"></param>
    /// <returns></returns>
    public static int[] SkillEffectTeams(Skill_Inst skill_inst, Pet invoker)
    {
        switch (skill_inst.rawSkill.influence)//0 - 己方受影响 1 - 敌方受影响 2 - 无差别效果
        {
            case 0:
                return invoker.team == 0 ? new int[] { 0 } : new int[] { 1, 2 };
            case 1:
                return invoker.team == 0 ? new int[] { 1, 2 } : new int[] { 0 };
            default:
                return new int[] { 0, 1, 2 };
        }
    }

    /// <summary>
    /// 技能覆盖
    /// </summary>
    public static List<Pet> SkillOverlapPets(Skill_Inst skill_inst, Vector3 pos, Pet invoker)
    {
        //influence team
        int[] teams = SkillSystem.SkillEffectTeams(skill_inst, invoker);

        //is single
        bool isSingle = skill_inst.rawSkill.radius == 0;

        //radius
        float radius = isSingle ? 1f : skill_inst.rawSkill.radius;

        //overlap pets
        var colliders = Physics.OverlapSphere(pos, radius);
        List<Pet> petList = new List<Pet>();
        foreach (var col in colliders)
        {
            Pet pet = col.GetComponent<Pet>();
            if (pet != null && !pet.IsDead && !pet.IsNeutral && teams.Contains(pet.team) && !petList.Contains(pet))
            {
                petList.Add(pet);
            }
        }
        
        //return
        if (isSingle) //单体选择技能
        {
            if (petList.Count > 0)
            {
                return new List<Pet>
                {
                    petList.OrderBy(p => (p.transform.position - pos).sqrMagnitude).First()
                };
            }
            else
            {
                return new List<Pet>();
            }
        }
        else //范围选择
        {
            return petList;
        }

    }


    

    /// <summary>
    /// 给对象添加Buff
    /// </summary>
    /// <param name="adder">添加者</param>
    /// <param name="eff">效果</param>
    private void RegisterSkillBuff(Pet adder, Skill_Inst skillInstance, SkillEffect eff)
    {
        if (eff == null) return;

        Buff bS = Infomanager.Instance.buffs.FirstOrDefault(b => b.id == (int)eff.value);

        Buff newBuff = new Buff(bS);


        //技能的永久Buff列表 += 新Buff
        if (newBuff.isPermanent && newBuff.buffGroup == BuffGroup.BBase)
        {
            skillInstance.permnentBuffs.Add(newBuff);
            //Debug.LogAssertion("注册永久技能BUFF:" + skillInstance.rawSkill.name); 
        }


        //Magic Strength Effect BuffAdd.  
        if(eff.buffMultipFac > 0f)
        {
            float rankMul = 1f + (skillInstance.GetRankAddFac() * eff.rankFac);
            float fac = rankMul * eff.buffMultipFac * adder.MagicStrength;

            //法强加成Value 或者 层数
            if (eff.type == 99)
            {
                newBuff.value *= fac;
                newBuff.remainValue *= fac;
            }
        }

        //Duration
        newBuff.duration *= (adder as Pet).SkillDuration;
        newBuff.remainTime *= (adder as Pet).SkillDuration;



        //Add buff
        if (eff.type == 99) //99
        {
            buffSystem.AddBuff(newBuff, eff.buffCompositionNumber);
        }
        else //999
        {
            float rankMul = 1f + (skillInstance.GetRankAddFac() * eff.rankFac);
            float fac = rankMul * eff.buffMultipFac * adder.MagicStrength;
            buffSystem.AddBuff(newBuff, (int)(eff.buffCompositionNumber * fac));
        }

    }

    /// <summary>
    /// 移除技能对应的永久BUFF（仅对自己生效）（一般不会给其他怪物添加永久BUFF）  
    /// </summary>
    /// <param name="skillInstance"></param>
    private void UnRegisterSkillBuffs(Skill_Inst skillInstance)
    {
        var thisBuffsystemAllBuffs = buffSystem.GetAllBuffs();

        var buffs = skillInstance.permnentBuffs
                .Where(b => thisBuffsystemAllBuffs.Contains(b))
                .ToArray();

        if (buffs.Length > 0)
        {
            buffSystem.RemoveBuffs(buffs);
        }
    }
}




//  **id** (int) ID
//  trigger (int) 触发方式   0主动(主技能)   1-平A   2-被攻击时    3-{秒數}战斗时秒数CD 3-5表達戰斗時5秒放一次   4-所有状态数秒CD    5-释放主动技能后   99-初始1次
//  type:     0-非指向性   1-指向   2-当前攻击目标
//  aiSelect (int)：   （指向技能目标选取）    1-自己   10-随机队友 11-血量少的队友 12-队伍第一个   20-随机敌人  21-血量少的敌人 22-当前攻击敌人  30-友方群体  40-敌方群体
//  radius: int     范围 (0==单体)
//  scatter: int    多投射物的分散范围 (0==单体)
//  influence :    0-己方受影响 1-敌方受影响 2-无差别效果
//  projectileType：    0-直线       1-抛出      2-直线碰撞AOE      3-固定区域持续AOE(projectileSpeed此时为AOE持续时间)   4-天上掉落(projectileSpeed掉落速度)
//  effects:
//  {
//      value : string
//      type : 10-物理伤害(固定)   
//              11物理伤害(自身属性) 
//              12物理伤害（敌人攻击力倍数） 
//              13物理伤害（敌人生命值倍数） 
//              20-魔法伤害（固定）   
//              21魔法伤害（自身法强）
//              22魔法伤害(敌人法强)    
//              23魔法伤害（敌人生命值倍数）（无法强加成） 
//              24魔法伤害（敌人损失生命值） （无法强加成） 
//              3 - 治疗  4- 驱散  5-消蓝   6-回蓝   7-位移  
//              99 - buff (法强加成value)   999 - buff(法强加成层数) 
//              666-召唤（compositionNumber消失时间）
//      compositionNumber：  叠加层数
//      buffMultipFac:  如果为0，Skill添加的Buff不受自身攻击或法强影响，如果不为0，则用来加成Buff。  
//  }
//  fx : string 表現形式 

#endif