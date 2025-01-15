#define BIND_TO_BUFFSYSTEM

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


#if USEZQJTOOLS
public delegate List<JibanSystem> GetJibanSystems(TeamJibanSystem teamJibanSystem);

public class TeamJibanSystem : SubComponent
{
    public List<JibanSystem> jibanSystems = new List<JibanSystem>();//所有随从羁绊系统列表

    public GetJibanSystems getJibanSystemsDel = null; //所有随从羁绊系统获取方法


    //tmp
    public List<Jiban> allJibans = new List<Jiban>();

    
    public override void Update(float deltaTime) { }


    //ctor
    public TeamJibanSystem(MonoBehaviour ownr) : base(ownr)
    {
    }





    public void Recalculate() // Rcalculate when pet changes (listen pet change event)
    {
        // *** 重新获取 所有随从羁绊系统 *** 
        if (getJibanSystemsDel == null)
            throw new UnityException("未绑定所有羁绊系统的获取方法");
        else
            this.jibanSystems = this.getJibanSystemsDel(this);
        

        // *** 更新队伍全局Jiban表 *** 
        this.allJibans.Clear();
        this.jibanSystems.ForEach(jiSys => {
            allJibans.AddRange(jiSys.jibans);
        });


        // *** 设置羁绊的BUFF *** 
        {
            foreach (var jbSys in this.jibanSystems)
            {
                //清除所有羁绊Buff
                jbSys.CleanJibanBuffs();
                //清除所有羁绊技能
                jbSys.CleanJibanSkills();

                foreach(var jiban in this.allJibans.Distinct()) //不重复的所有羁绊遍历
                {
                    //加成阶段计算
                    int countSame= this.allJibans.Count(j => j == jiban);
                    int idxInArr = -1;
                    for (int i = 0; i < jiban.countNeed.Length; i++)
                    {
                        if (countSame >= jiban.countNeed[i]) idxInArr = i;
                    }
                    
                    //每个羁绊效果
                    foreach (var jbe in jiban.effects)
                    {
                        //  ------------- 技能类羁绊 ----------------------
                        if (jbe.isSkill)  
                        {
                            if(idxInArr > -1)
                            {
                               int vidx = (int)jbe.value[idxInArr];
                                
                                Skill skill = Infomanager.Instance.skills.FirstOrDefault(sk => sk.id == vidx);

                                //add skill
                                if (jbSys.jibans.Contains(jiban))
                                {
                                    jbSys.RegisterJibanSkill(skill);
                                }
                            }
                        }
                        //  ------------- Buff类羁绊 ----------------------
                        else
                        {
                            //到达最低加成级别
                            if (idxInArr > -1)
                            {
                                //加成值计算
                                float value = jbe.value[idxInArr];
                                

                                //is global(new logic)
                                switch(jbe.isGlobal)
                                {
                                    case 0:  //仅对有该羁绊的生效
                                        {
                                            if(jbSys.jibans.Contains(jiban))
                                            {
                                                jbSys.RegisterJibanBuff(jiban.name + "_" + jbe.type, jbe.typeAsBuff, jbe.isMultip, value);
                                            }
                                        }
                                        break;
                                    case 1:  //全局生效
                                        {
                                            jbSys.RegisterJibanBuff(jiban.name + "_" + jbe.type, jbe.typeAsBuff, jbe.isMultip, value);
                                        }
                                        break;
                                    case 2:   //队伍第一个
                                        {
                                            if(jbSys.IsLeader())
                                                jbSys.RegisterJibanBuff(jiban.name + "_" + jbe.type, jbe.typeAsBuff, jbe.isMultip, value);
                                        }
                                        break;
                                }
                            }
                        }
                        //当前效果设置完成
                    }

                    //当前羁绊设置完成
                }

                //当前羁绊系统设置完成
            }
        }

    }

}
#endif