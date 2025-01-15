using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SimpleInspectpr
{
    public class UIGameObject : MonoBehaviour
    {
        private GameObject currentgo = null;

        void Start()
        {
            this.transform.Find("This").GetComponent<Button>().onClick.AddListener(OnClick);
        }
        
        public void Bind(GameObject go)
        {
            this.currentgo = go;

            var cell = this.GetComponent<RecursiveCell>();

            var txt = cell.CellThis.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (go.activeInHierarchy)
                txt.color = Color.white;
            else
                txt.color = Color.gray;

            txt.text = go.name;
        }


        public void OnClick()
        {
            var inspector = GetComponentInParent<InGameInspector>();
            inspector.activeGameObject = this.currentgo;
            inspector.Inspect(true);
        }
    }
    
}
