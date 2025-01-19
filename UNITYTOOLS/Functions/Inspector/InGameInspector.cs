using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using REWTOOLS;


namespace SimpleInspectpr
{

    public class InGameInspector : MonoBehaviour
    {
        public RectTransform hierarchyPanel;
        public RectTransform inspectPanel;

        [HideInInspector] public GameObject activeGameObject = null;
        [HideInInspector] public  List<GameObject> result = new List<GameObject>();

        private bool searchingOn = false;

        void Start()
        {
        }

        private void Update()
        {
        }

        /// <summary>
        /// 寻找所有根节点对象  
        /// </summary>
        /// <returns></returns>
        private GameObject[] GetRootObjects()
        {
            var rootObjs = SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .Where(go => go.GetComponent<InGameInspector>() == null)
                .ToArray();

            return rootObjs;
        }
        private GameObject[] GetAllObjects()
        {
            return FindObjectsOfType<GameObject>();
        }

        /// <summary>
        /// 显示场景树结构  
        /// </summary>
        public void RefreshSceneHierachy()
        {
            var rootGos = GetRootObjects();

            var scroll = hierarchyPanel.GetComponentInChildren<ScrollRect>();
            var rootcontainer = scroll.content.GetComponent<UIGridManager>();
            rootcontainer.SetGrid(rootGos.Length);
            for (int i = 0; i < rootGos.Length; ++i)
            {
                BindGo(rootGos[i], rootcontainer.transform.GetChild(i).GetComponent<RecursiveCell>(), true);
            }
        }
        /// <summary>
        /// 显示搜索结果  
        /// </summary>
        public void RefreshSearchingResult()
        {
            var scroll = hierarchyPanel.GetComponentInChildren<ScrollRect>();
            var rootcontainer = scroll.content.GetComponent<UIGridManager>();
            rootcontainer.SetGrid(result.Count);
            for (int i = 0; i < result.Count; ++i)
            {
                BindGo(result[i], rootcontainer.transform.GetChild(i).GetComponent<RecursiveCell>(), false);
            }
        }

        /// <summary>
        /// 绑定Cell  
        /// </summary>
        private void BindGo(GameObject go, RecursiveCell cell, bool includeChilds = false)
        {
            //绑定GO
            cell.GetComponent<UIGameObject>().Bind(go);

            //子节点递归  
            if (includeChilds)
            {
                if (go.transform.childCount > 0)
                {
                    cell.SetChildCount(go.transform.childCount);
                    for (int i = 0; i < go.transform.childCount; ++i)
                    {
                        BindGo(go.transform.GetChild(i).gameObject, cell.ChildsContainer.GetChild(i).GetComponent<RecursiveCell>(), true);
                    }
                }
                else
                {
                    cell.SetChildCount(0);
                }
            }
            else
            {
                cell.SetChildCount(0);
            }
        }

        /// <summary>
        /// 查找  
        /// </summary>
        public void Search()
        {
            var inputCom = this.GetComponentInChildren<TMPro.TMP_InputField>();
            var input = inputCom.text;

            if (string.IsNullOrEmpty(input) == false)
            {
                //组件查找  
                if (input.StartsWith("t:"))
                {
                    var typename = input.Substring(2);
                    var type = UtilsReflection.GetTypeByName(typename);
                    if (type != null && type.IsSubclassOf(typeof(Component)))
                    {
                        this.result.Clear();
                        this.result.AddRange(FindObjectsOfType(type).Select(o => (o as Component).gameObject).ToList());
                        searchingOn = true;
                        RefreshSearchingResult();
                        return;
                    }
                    else
                    {
                        Debug.Log("找不到该类型组件：" + typename);
                    }
                }
                //名称查找  
                else
                {
                    var allgos = GetAllObjects();
                    result.Clear();
                    foreach (var go in allgos)
                    {
                        if (go.name.StartsWith(input))
                        {
                            result.Add(go);
                        }
                    }

                    searchingOn = true;
                    RefreshSearchingResult();
                    return;
                }
            }

            searchingOn = false;
            RefreshSceneHierachy();
        }
        
        /// <summary>
        /// 监视当前选中GameObject
        /// </summary>
        /// <param name="show"></param>
        public void Inspect(bool show)
        {
            if (show)
            {
                if (activeGameObject == null) return;

                inspectPanel.gameObject.SetActive(true);


                //GO INFO  
                inspectPanel.Find("TextName").GetComponent<TMPro.TextMeshProUGUI>().text = activeGameObject.name;
                inspectPanel.Find("TextTag").GetComponent<TMPro.TextMeshProUGUI>().text = activeGameObject.tag;
                inspectPanel.Find("TextLayer").GetComponent<TMPro.TextMeshProUGUI>().text = activeGameObject.layer.ToString();


                //GO SCENE PATH  
                List<Transform> parentChain = new List<Transform>();
                Transform current = activeGameObject.transform;
                while (current != null)
                {
                    parentChain.Add(current);
                    current = current.parent;
                }
                parentChain.Reverse();
                string path = string.Concat(parentChain.Select(go => go.name + "/"));
                inspectPanel.Find("TextScenePath").GetComponent<TMPro.TextMeshProUGUI>().text = path;


                //COMS BIND  
                var scroll = inspectPanel.GetComponentInChildren<ScrollRect>();
                var container = scroll.content.GetComponent<UIGridManager>();
                var components = activeGameObject.GetComponents<Component>();
                container.SetGrid(components.Length);

                for (int i = 0; i < components.Length; ++i)
                {
                    var com = components[i];
                    
                    var cell = container.transform.GetChild(i).GetComponent<RecursiveCell>();

                    //默认Fold状态  
                    cell.Fold();

                    cell.GetComponent<UIComponent>().Bind(com);
                }
            }
            else
            {
                inspectPanel.gameObject.SetActive(false);
            }
        }
    }

}
