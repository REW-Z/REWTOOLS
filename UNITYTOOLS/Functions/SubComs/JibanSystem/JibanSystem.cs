using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

# if LITJSON
using LitJson;
#endif



#if USEZQJTOOLS
//羁绊效果(静态信息)
public class JibanEffect
{
    public bool isSkill;//是否是技能

    public string type;
    public BuffType typeAsBuff; //类型

    public int isMultip; //是固定值还是倍数

    public float[] value;//加成数值

    public int isGlobal;//是否对全队伍生效



    //static
    public static BuffType[] allTypes = System.Enum.GetValues(typeof(BuffType)) as BuffType[];
}


//羁绊 (静态信息)  
//( JSON 序列化格式：  )
public class Jiban
{
    public int id;                  //ID
    public string name;             //羁绊名称
    public string description;      //描述
    public string description2;
    public List<string[]> description2Value;
    public int[] countNeed;      //描述
    public JibanEffect[] effects;   //效果列表 （如果是非数值型羁绊，则是空列表）


#if LITJSON
    // ------------- JIBAN Deserializer --------------
    public static class JiBanDeserializer
    {
        public static Jiban[] Deserialize(string json)
        {
            JsonData jJiban = JsonMapper.ToObject(json)["jibans"];

            Jiban[] jibanArr = new Jiban[jJiban.Count];
            for (int i = 0; i < jibanArr.Length; i++)
            {
                jibanArr[i] = new Jiban();
                jibanArr[i].id = (int)jJiban[i]["id"];
                jibanArr[i].name = (string)jJiban[i]["name"];
                jibanArr[i].countNeed = UtilsLitJson.GetArray<int>(jJiban[i]["countNeed"]);

                jibanArr[i].description = (string)jJiban[i]["description"];
                jibanArr[i].description2 = (string)jJiban[i]["description2"];

                jibanArr[i].description2Value = new List<string[]>();
                for (int k = 0; k < jJiban[i]["description2Value"].Count; k++)
                {
                    int nCount = jJiban[i]["description2Value"][k].Count;
                    string[] strs = new string[nCount];
                    for (int l = 0; l < nCount; l++)
                    {
                        strs[l] = (string)jJiban[i]["description2Value"][k][l];
                    }

                    jibanArr[i].description2Value.Add(strs);
                }


                jibanArr[i].effects = new JibanEffect[jJiban[i]["effects"].Count];
                for (int j = 0; j < jJiban[i]["effects"].Count; j++)
                {
                    jibanArr[i].effects[j] = new JibanEffect();
                    jibanArr[i].effects[j].isSkill = (bool)jJiban[i]["effects"][j]["isSkill"];
                    jibanArr[i].effects[j].type = (string)jJiban[i]["effects"][j]["type"];
                    jibanArr[i].effects[j].typeAsBuff = jibanArr[i].effects[j].isSkill ? BuffType.Other : (BuffType)System.Enum.Parse(typeof(BuffType), jibanArr[i].effects[j].type);
                    jibanArr[i].effects[j].isMultip = (int)jJiban[i]["effects"][j]["isMultip"];
                    jibanArr[i].effects[j].value = UtilsLitJson.GetArray<float>(jJiban[i]["effects"][j]["value"]);
                    jibanArr[i].effects[j].isGlobal = (int)jJiban[i]["effects"][j]["isGlobal"];
                }
            }

            return jibanArr;
        }
    }
#endif
}



//角色羁绊系统
public class JibanSystem : SubComponent
{
    public TeamJibanSystem teamJibanSystem; //队伍羁绊系统
    public BuffSystem buffSystem;
    public SkillSystem skillSystem;



    public Jiban[] jibans = null; //角色羁绊数组

    public Dictionary<BuffType, float> mulDic;
    public float AtkMultip => this.mulDic[BuffType.Atk];
    public float HpMultip => this.mulDic[BuffType.Hp];




    //ctor
    public JibanSystem(MonoBehaviour owner) : base(owner)
    {
        this.mulDic = new Dictionary<BuffType, float>();
        InitDic();
    }


    public override void Update(float deltaTime)
    {//doNothing
    }


    // ---------------- 外部接口 ----------------------------
    
    public void InitDic()
    {
        foreach(var t in JibanEffect.allTypes)
        {
            this.mulDic[t] = 1f;
        }
    }





    
    public List<Skill> skillsRegistered = new List<Skill>();

    public void RegisterJibanBuff(string buffName, BuffType type, int isMultip, float value)
    {
        if(buffSystem.HasBuff(buffName))
            buffSystem.RemoveBuff(buffName);

        Buff b = new Buff();
        //标记Buff用于删除
        b.buffName = "JIBAN_羁绊名：" + buffName;
        b.buffGroup = BuffGroup.BBase;
        b.buffType = type;
        b.isMultip = isMultip;
        b.value = value;
        b.isPermanent = true;

        //添加BUFF
        buffSystem.AddBuff(b);
    }
    public void CleanJibanBuffs()
    {
        var jibanBuffs = buffSystem.GetAllBuffs()
            .Where(b => b.buffGroup == BuffGroup.BBase)
            .Where(b => b.buffName.Contains("JIBAN_"))
            .ToArray();

        buffSystem.RemoveBuffs(jibanBuffs);
    }
    public void RegisterJibanSkill(Skill skill)
    {
        skillsRegistered.Add(skill);

        skillSystem.RegisterSkill(skill);
    }

    public void CleanJibanSkills()
    {
        foreach(var skill in skillsRegistered)
        {
            skillSystem.UnRegisterSkill(skill);
        }

        skillsRegistered.Clear();
    }

}









public static class JIbanSystenExtension
{
    public static void SetPetInfo(this JibanSystem jibanSys, PetInfo pinfo)
    {
        jibanSys.jibans = new Jiban[pinfo.jibans.Length];
        for (int i = 0; i < pinfo.jibans.Length; i++)
        {
            jibanSys.jibans[i] = Infomanager.Instance.jibans.FirstOrDefault(jb => jb.id == pinfo.jibans[i]);
        }
    }

    public static bool IsLeader(this JibanSystem jibanSys)
    {
        int petId = (jibanSys.owner as Pet).PetInstance.id;
        int indexInTeam = Infomanager.Instance.userdata.carryPets.Contains(petId)
            ? Infomanager.Instance.userdata.carryPets.IndexOf(petId)
            : -1;

        Debug.Log("---index in team:" + indexInTeam);

        return indexInTeam == 0;
    }
}


#endif