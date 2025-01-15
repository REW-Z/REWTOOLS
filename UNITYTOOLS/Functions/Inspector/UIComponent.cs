using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SimpleInspectpr
{
    public class UIComponent : MonoBehaviour
    {
        public Component currentCom = null;

        void Start()
        {

        }

        public void Bind(Component com)
        {
            this.currentCom = com;

            var type = com.GetType();
            var cell = this.GetComponent<RecursiveCell>();

            //Text Name  
            cell.CellThis.GetComponentInChildren<TMPro.TextMeshProUGUI>(true).text = type.Name;


            //Toggle    
            var toggle = cell.CellThis.Find("ToggleEnable").GetComponent<Toggle>();
            if(com is Behaviour)
            {
                toggle.gameObject.SetActive(true);
                toggle.isOn = (com as Behaviour).enabled;
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((isOn) => {
                    (com as Behaviour).enabled = isOn;
                });
            }
            else
            {
                toggle.gameObject.SetActive(false);
            }



            
            switch(type.Name)
            {
                case "Transform":
                    AsTransform();
                    break;
                default:
                    AsDefault();
                    break;
            }
        }


        private void AsTransform()
        {
            if (currentCom == null) return;

            var type = currentCom.GetType();
            var cell = this.GetComponent<RecursiveCell>();

            var members = new MemberInfo[3];

            members[0] = type.GetMember("localPosition").FirstOrDefault();
            members[1] = type.GetMember("localEulerAngles").FirstOrDefault();
            members[2] = type.GetMember("localRotation").FirstOrDefault();

            BindMembers(members);
        }

        public void AsDefault()
        {
            if (currentCom == null) return;

            var type = currentCom.GetType();
            var cell = this.GetComponent<RecursiveCell>();

            //FIELDS  
            var members = new List<MemberInfo>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); //Debug.LogAssertion(type.Name + "公共字段数量：" + fields.Length);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            members.AddRange(fields);
            members.AddRange(properties);

            BindMembers(members.ToArray());
        }


        private void BindMembers(MemberInfo[] members)
        {
            if (currentCom == null) return;
            if (members == null) return;

            var type = currentCom.GetType();
            var cell = this.GetComponent<RecursiveCell>();

            cell.SetChildCount(members.Length);
            for (int j = 0; j < members.Length; ++j)
            {
                var memberCell = cell.ChildsContainer.GetChild(j).GetComponent<RecursiveCell>();

                //Is Obsolate  
                if (members[j].GetCustomAttribute(typeof(System.ObsoleteAttribute)) != null)
                {
                    memberCell.CellThis.Find("TextMemberName").GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Normal;
                    memberCell.CellThis.Find("TextMemberName").GetComponent<TMPro.TextMeshProUGUI>().text = "(obsolate)" + members[j].Name;
                    memberCell.CellThis.Find("TextValue").GetComponent<TMPro.TextMeshProUGUI>().text = "";
                    continue;
                }


                if(members[j] is FieldInfo)
                {
                    memberCell.CellThis.Find("TextMemberName").GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Normal;
                    memberCell.CellThis.Find("TextMemberName").GetComponent<TMPro.TextMeshProUGUI>().text = members[j].Name;

                    object value = (members[j] as FieldInfo).GetValue(currentCom);
                    string valueDisp = "";
                    if (value == null)
                    {
                        value = "(null)";
                    }
                    else
                    {
                        var typeOfValue = value.GetType();

                        if (typeOfValue != null)
                        {
                            if (typeOfValue.IsValueType)
                            {
                                valueDisp = value.ToString();
                            }
                            else if (value is IList)
                            {
                                valueDisp = "(list)";
                            }
                            else
                            {
                                valueDisp = "(object)";
                            }
                        }
                        else
                        {
                            valueDisp = "(unknown)";
                        }
                    }
                    memberCell.CellThis.Find("TextValue").GetComponent<TMPro.TextMeshProUGUI>().text = valueDisp;
                }
                else if(members[j] is PropertyInfo)
                {
                    memberCell.CellThis.Find("TextMemberName").GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Italic | TMPro.FontStyles.Bold;
                    memberCell.CellThis.Find("TextMemberName").GetComponent<TMPro.TextMeshProUGUI>().text = members[j].Name;

                    var propertyInfo = members[j] as PropertyInfo;
                    
                    if (propertyInfo.CanRead)
                    {
                        object value = (members[j] as PropertyInfo).GetValue(currentCom);
                        string valueDisp = "";
                        if (value == null)
                        {
                            value = "(null)";
                        }
                        else
                        {
                            var typeOfValue = value.GetType();

                            if (typeOfValue != null)
                            {
                                if (typeOfValue.IsValueType)
                                {
                                    valueDisp = value.ToString();
                                }
                                else if (value is IList)
                                {
                                    valueDisp = "(list)";
                                }
                                else
                                {
                                    valueDisp = "(object)";
                                }
                            }
                            else
                            {
                                valueDisp = "(unknown)";
                            }
                        }

                        memberCell.CellThis.Find("TextValue").GetComponent<TMPro.TextMeshProUGUI>().text = valueDisp;
                    }
                    if (propertyInfo.CanWrite)
                    {
                    }
                }
                else
                {
                    memberCell.CellThis.Find("TextValue").GetComponent<TMPro.TextMeshProUGUI>().text = "";
                }
            }
        }
    }
}
