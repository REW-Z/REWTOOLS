using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MyConfigs/ConfigTest", fileName = "Config")]
public class ConfigTest : ScriptableObject
{
    public string config1;
    public string config2;
    public string config3;


    //Inspecter中修改会自动设置脏标记。代码修改时自行调用EditorUtility.SetDirty();
}
