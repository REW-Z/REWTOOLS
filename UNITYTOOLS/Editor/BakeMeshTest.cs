using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BakeMeshTest
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    [MenuItem("Utils/Mesh/BakeSMR")]
    public static void BakeSelection()
    {
        var smr = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();

        if(smr != null)
        {
            Mesh mesh = new Mesh();
            smr.BakeMesh(mesh);

            UnityEditor.AssetDatabase.CreateAsset(mesh, "Assets/baked mesh(test)");
        }
    }
}
