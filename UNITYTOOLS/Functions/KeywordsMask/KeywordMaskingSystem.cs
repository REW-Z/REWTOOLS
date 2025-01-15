/// --- 构建屏蔽字符的赫夫曼树 --- 
///词组包含问题： 共存                                   ✔
/// --- 查找规则 --- 
///词组匹配规则： 下一节点包含'\0'代表匹配成功             ✔
///词组匹配规则： 长词组优先   




using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;


/// <summary>
/// 字符树森林
/// </summary>
public class CharacterNodeForest
{
    /// <summary>
    /// 根节点集合
    /// </summary>
    public List<CharacterNode> roots = new List<CharacterNode>();

    /// <summary>
    /// 添加根节点
    /// </summary>
    public CharacterNode AddRoot(char rootChar)
    {
        CharacterNode newRoot = new CharacterNode(rootChar);
        this.roots.Add(newRoot);

        return newRoot;
    }
}
/// <summary>
/// 字符树节点
/// </summary>
public class CharacterNode
{
    /// <summary>
    /// 字符
    /// </summary>
    public char character = '\0';

    /// <summary>
    /// 权值
    /// </summary>
    public int weight = 0;

    /// <summary>
    /// 父节点
    /// </summary>
    public CharacterNode parent = null;

    /// <summary>
    /// 子节点
    /// </summary>
    public List<CharacterNode> childs = new List<CharacterNode>();



    public CharacterNode(char c)
    {
        this.character = c;
    }
    public CharacterNode AppendChild(char childChar)
    {
        CharacterNode newchild = new CharacterNode(childChar);
        this.childs.Add(newchild);
        newchild.parent = this;

        return newchild;
    }
    public bool HasChild(char c)
    {
        return childs.Any(child => child.character == c);
    }
}













/// <summary>
/// 关键字屏蔽系统
/// </summary>
public class KeywordMaskingSystem
{
    public static CharacterNodeForest forest = new CharacterNodeForest();

    public static bool isinit = false;
    
    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init(string[] words)
    {
        if (isinit) return;

        //词组数组遍历
        foreach (var word in words)
        {
            if (word.Length == 0) { Debug.LogAssertion("有空字符串！"); break; }

            // ****** 尝试查找树根节点 ******
            char firstChar = word[0]; 
            CharacterNode root = forest.roots.FirstOrDefault(rn => rn.character == firstChar);
            if (root == null)
            {
                root = forest.AddRoot(firstChar); //new root
            }



            // ****** 逐字符遍历 ******

            //访问根节点并权重加一
            CharacterNode currentNode = root;
            currentNode.weight += 1;

            for (int i = 1; i < word.Length; i++) 
            {
                var c = word[i];

                //已有对应子节点 - 跳转到子节点
                if(currentNode.HasChild(c))
                {
                    var existChild = currentNode.childs.FirstOrDefault(chd => chd.character == c);
                    
                    //访问节点并权重加一
                    currentNode = existChild;
                    currentNode.weight += 1;
                }
                //未找到对应子节点 - 添加一个新子节点
                else
                {
                    var newchild = currentNode.AppendChild(c);

                    //访问节点并权重加一
                    currentNode = newchild;
                    currentNode.weight += 1;
                }


            }


            //末尾加上'\0'字符代表结束  （防止包含冲突问题：短字符被长字符替代）
            currentNode.AppendChild('\0');
        }


        //所有节点按权重排序（递归）
        foreach(var rt in forest.roots)
            OrderChilds(rt);

        //完成
        isinit = true;


        //输出结果
        LogOutput();
    }

    /// <summary>
    /// 排序节点
    /// </summary>
    /// <param name="node"></param>
    public static void OrderChilds(CharacterNode node)
    {
        node.childs.Sort((a, b) => { return a.weight.CompareTo(b.weight); });

        foreach(var child in node.childs)
        {
            OrderChilds(child);
        }
    }

    /// <summary>
    /// 打印输出
    /// </summary>
    public static void LogOutput()
    {
        string rootsStr = "";
        foreach(var root in forest.roots)
        {
            rootsStr += root.character;
        }
        Debug.Log("<color=#FFFF00>所有根字符:" + rootsStr + "</color>");
    }


    /// <summary>
    /// 屏蔽字符（同步）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Mask(string input)
    {
        if (!isinit) return "";

        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        string _DEBUG_LOG = "<color=#FFFF00>正在屏蔽词条：" + input + "</color>\n<color=#FFFF00>";



        bool matched = false;
        CharacterNode currentNode = null;
        for (int i = 0; i < input.Length; i++)
        {
            //寻找根节点
            if(currentNode == null)
            {
                var existRoot = forest.roots.FirstOrDefault(c => c.character == input[i]);
                if (existRoot != null)
                {
                    _DEBUG_LOG += ("找到根节点：" + existRoot.character);
                    currentNode = existRoot;
                }
            }
            //寻找下一个节点
            else
            {
                var existChild = currentNode.childs.FirstOrDefault(c => c.character == input[i]);
                if(existChild != null)
                {
                    _DEBUG_LOG += ("\n找到子节点：" + existChild.character);
                    currentNode = existChild;
                }
            }


            //是叶子节点'\0'的前一个节点 -- 检查完成
            if(i == input.Length - 1)
            {
                _DEBUG_LOG += "\n全字符串查找结束 -- 当前节点:" + (currentNode != null ?  currentNode.character.ToString() : "空");

                if (currentNode != null && currentNode.HasChild('\0'))
                {
                    _DEBUG_LOG += "(当前树查找结束 -- 当前节点:" + (currentNode != null ? currentNode.character.ToString() : "空") + ")";
                    matched = true;
                    break;
                }
                else
                {
                    _DEBUG_LOG += "(当前树未查找结束:下一节点:" + currentNode.childs.FirstOrDefault().character + ")";
                }
            }
            else
            {
                if (currentNode != null && currentNode.HasChild('\0'))
                {
                    _DEBUG_LOG += "\n当前树查找结束 -- 当前节点:" + (currentNode != null ? currentNode.character.ToString() : "空");
                    matched = true;
                    break;
                }
            }
        }





        if(matched && currentNode != null)
        {
            List<char> chars = new List<char>();
            
            while(currentNode != null)
            {
                chars.Add(currentNode.character);
                currentNode = currentNode.parent;
            }

            chars.Reverse();

            string pattern = new string(chars.ToArray());

            _DEBUG_LOG += ("\n找到最终和谐词语：" + pattern);

            string output = input.Replace(pattern, new String('*', pattern.Length));




            watch.Stop();
            _DEBUG_LOG += "\n屏蔽用时：" + watch.ElapsedMilliseconds.ToString() + "毫秒 (" + watch.ElapsedTicks.ToString() + "时钟)";


            _DEBUG_LOG += "</color>";
            Debug.Log(_DEBUG_LOG);

            return output;
        }
        else
        {
            _DEBUG_LOG += "</color>";
            Debug.Log(_DEBUG_LOG);

            return input;
        }
    }


    /// <summary>
    /// 屏蔽字符（异步）
    /// </summary>
    /// <param name="input"></param>
    /// <param name="callback"></param>
    public static void MaskAsync(string input, System.Action<string> callback = null)
    {
        if (!isinit) return;

        var thread = new System.Threading.Thread(() => {
            string output = Mask(input);
            if(callback != null) callback(output);
        });
        thread.Start();
    }
    
}
