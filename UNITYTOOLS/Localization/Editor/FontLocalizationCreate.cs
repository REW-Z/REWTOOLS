using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace REWTOOLS
{
    public class FontLocalizationCreate : MonoBehaviour
    {
        [MenuItem("本地化/创建字体本地化信息")]
        public static void CreateFontLocalizationObj()
        {
            var fontConfig = ScriptableObject.CreateInstance<FontLocalization>();

            var languageNames = System.Enum.GetNames(typeof(LocalizationSystem.Language));
            fontConfig.langNames = languageNames;
            fontConfig.fonts = new Font[languageNames.Length];
            fontConfig.fontAssets = new TMPro.TMP_FontAsset[languageNames.Length];

            AssetDatabase.CreateAsset(fontConfig, "Assets/Resources/FontLocalization.asset");
        }
    }

}
