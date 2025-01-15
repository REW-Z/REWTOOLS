using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilsReflection
{
    public static object Call(string pattern)
    {
        //(scene)__Console__.UIConsole.Test()

        Debug.LogAssertion("Invoke：" + pattern);

        string purepattern = pattern.Replace(";", "");

        string[] tokens = purepattern.Split('.');
        if (tokens.Length < 2) return false;



        string firstToken = tokens[0];

        object obj = null;
        string typeName = null;
        string methodExpression = null;


        // ---------------------------- 示例 / 类型查找 ---------------------------
        //实例方法（按类型查找第一个示例）    
        if(Regex.IsMatch(firstToken, "\\(findobj\\).*"))
        {
            string instTypeName = firstToken.Replace("(findobj)", "");

            var type =Type.GetType(instTypeName);
            if(type != null)
            {
                obj = GameObject.FindObjectOfType(type);
                typeName = instTypeName;
                methodExpression = tokens[1];

                Debug.LogAssertion("IsFindObj:" + type.FullName + "   method expr:" + methodExpression);
            }
        }
        //实例方法（从场景查找）  
        else if(Regex.IsMatch(firstToken, "\\(scene\\).*"))
        {
            string path = firstToken.Replace("(scene)", "");

            Debug.LogAssertion("scenePath:" + path);

            var node = UtilsScene.Find(path);
            if(node != null)
            {
                var type = Type.GetType(tokens[1]);
                if (type != null)
                {
                    obj = node.GetComponent(type);
                    typeName = tokens[1];
                    methodExpression = tokens[2];

                    Debug.LogAssertion("Is Find scene:" + typeName + "   method expr:" + methodExpression);
                }
            }
        }
        //单例方法  
        else if (Regex.IsMatch(firstToken, "\\(singleton\\).*"))
        {
            string instTypeName = firstToken.Replace("(singleton)", "");
            var type = Type.GetType(instTypeName);
            if (type != null)
            {
                var propertyInfo = type.GetProperty("Instance", BindingFlags.Instance | BindingFlags.Public);
                var inst = propertyInfo.GetValue(null);

                obj = inst;
                typeName = instTypeName;
                methodExpression = tokens[1];

                Debug.LogAssertion("Is Instance:" + instTypeName + "   method expr:" + methodExpression);
            }
        }
        //静态方法  
        else
        {
            string instTypeName = firstToken;

            obj = null;
            typeName = instTypeName;
            methodExpression = tokens[1];

            Debug.LogAssertion("Is Static:" + instTypeName + "   method expr:" + methodExpression);
        }


        // ---------------------------------- 调用 ————————————————————————————————————————————

        //方法处理  
        var argsMatch = Regex.Match(methodExpression, "\\(.*\\)");
        if (argsMatch != null && argsMatch.Success == true)
        {
            string rawArgsStr = argsMatch.Value.Substring(1, argsMatch.Length - 2);
            string[] rawArgs;
            if (string.IsNullOrEmpty(rawArgsStr))
                rawArgs = new string[0];
            else
                rawArgs = rawArgsStr.Split(',');

            object[] trueArgs = ProcessArgs(rawArgs);
            string methodName = methodExpression.Replace(argsMatch.Value, "");
            
            Debug.LogAssertion((obj != null ? (typeName + "类实例") : (typeName + "类")) + "   调用方法名：" + methodName + "   参数长度：" + trueArgs.Length + "  原始参数列表：" + string.Concat(rawArgs.Select(arg => arg.ToString() + ",")) + "   参数列表：" + string.Concat(trueArgs.Select(arg => "(" + arg.GetType().Name + ")" + arg.ToString() + ",")));

            //静态方法调用
            if (obj == null && typeName != null)
            {
                Debug.LogAssertion("是静态方法...");
                Type t = Type.GetType(typeName);
                if (t != null)
                {
                    Debug.LogAssertion("类已找到");
                    var mInfo = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(m => m.Name == methodName);
                    if(mInfo != null)
                    {
                        Debug.LogAssertion("方法已经找到-调用静态方法中...");
                        return mInfo.Invoke(null, trueArgs);
                    }
                }
            }
            //实例方法调用  
            else
            {
                Type t = obj.GetType();
                if (t != null)
                {
                    var mInfo = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (mInfo != null)
                    {
                        Debug.LogAssertion("调用实例方法中...");
                        return mInfo.Invoke(obj, trueArgs);
                    }
                }
            }
        }
        else
        {
            Debug.LogAssertion("方法格式不对！");
        }


        return null;
    }


    public static Type GetTypeByName(string typeName)
    {
        switch(typeName)
        {
            //常用结构体    
            case "Vector2":
                return typeof(Vector2);
            case "Vector3":
                return typeof(Vector3);
            case "Vector2Int":
                return typeof(Vector2Int);
            case "Vector3Int":
                return typeof(Vector3Int);
            case "Vector4":
                return typeof(Vector4);
            default:
                {
                    //本程序集查找
                    var type = Type.GetType(typeName);
                    if(type != null)
                    {
                        return type;
                    }

                    //全程序集查找  
                    var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach(var assm in assemblies)
                    {
                        var typeinAsm = assm.GetType(typeName);
                        if (typeinAsm != null)
                        {
                            return typeinAsm;
                        }
                    }

                }
                return null;
        }
    }

    public static object[] ProcessArgs(string[] rawArgs)
    {
        object[] finalArgs = new object[rawArgs.Length];

        for (int i = 0; i < rawArgs.Length; ++i)
        {
            finalArgs[i] = ProcessArg(rawArgs[i]);
        }

        return finalArgs;
    }


    public static object ProcessArg(string rawArg)
    {
        int intValue;
        float floatValue;

        //int
        if (int.TryParse(rawArg.Trim(), out intValue))
        {
            return intValue;
        }
        //float  
        else if (float.TryParse(rawArg.Trim(), out floatValue))
        {
            return floatValue;
        }
        //字符串  
        else if (Regex.IsMatch(rawArg.Trim(), "\".*\""))
        {
            return rawArg.Trim().Replace("\"", "");
        }
        //new表达式  
        else if (rawArg.Trim().StartsWith("new "))
        {
            var tmp = rawArg.Trim().Substring(4);
            int idxlb = tmp.IndexOf("(");
            int idxrb = tmp.IndexOf(")");

            if(idxlb > -1 && idxrb > -1)
            {
                string typeName = tmp.Substring(0, idxlb);
                string argsString = tmp.Substring(idxlb + 1, (idxrb - idxlb - 1));

                Type type = GetTypeByName(typeName);
                if (type != null)
                {
                    object[] trueargs = ProcessArgs(argsString.Split(',').Select(arg => arg.Trim()).ToArray());
                    var newobject = Activator.CreateInstance(type, trueargs);

                    return newobject;
                }
            }
        }
        //UtilsReflection表达式  
        else if (rawArg.Contains("."))
        {
            return UtilsReflection.Call(rawArg.Trim());
        }
        else
        {
            return null;
        }


        return null;
    }
}
