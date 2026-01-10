using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.ShaderKeywordFilter;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using System.Linq;

// ShaderKeywordTestShaderの"_SWITCH_0"キーワードをストリップするプリプロセッサ
public class ShaderKeywordTestShaderKeywordStripper : IPreprocessShaders
{
    // [RemoveIf(true, keywordNames: "_SWITCH_0")]
    // bool switch_0 = false; // "_SWITCH_0"キーワードをストリップするためにfalseにする 
    public int callbackOrder => 0;

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (shader.name == "Custom/ShaderKeywordTestShader" || shader.name == "Custom/ShaderKeywordTestShader_Asset")
        {
            // 現在有効なキーワードセット（material.enabledKeywords）を取得
            // ※ここではエディタ上のMaterialから取得する例
            var materials = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.shader == shader);
            //最強のFindだけど、isPersistent以外のUnityObjectはAllWeakDestroyで消えてる?

            for (int i = data.Count - 1; i >= 0; --i)
            {
                var variantKeywords = new HashSet<string>(data[i].shaderKeywordSet.GetShaderKeywords().Select(k => k.name));

                // 各マテリアルのenabledKeywords + 現在有効なグローバルキーワードを合成して判定
                var shaderGlobalKeywordNames = new HashSet<string>(
                    shader.keywordSpace.keywords
                        .Select(k => k.name)
                );

                var enabledGlobalKeywordNames = new HashSet<string>(
                    Shader.enabledGlobalKeywords.Select(k => k.name)
                );

                // シェーダーで有効なグローバルキーワードのみ抽出
                var enabledShaderGlobalKeywords = shaderGlobalKeywordNames.Intersect(enabledGlobalKeywordNames).ToHashSet();

                var matchedMaterials = materials
                    .Where(m =>
                    {
                        var matKeywords = new HashSet<string>(m.enabledKeywords.Select(k => k.name));
                        matKeywords.UnionWith(enabledShaderGlobalKeywords);
                        return matKeywords.SetEquals(variantKeywords);
                    })
                    .Select(m => m.name)
                    .ToList();
                //Material**アセット**が無い場合はマッチチェックが出来ない
                //あと、GlobalKeyword**のみ**からshader_featureのKeywordを有効にする時のマッチ失敗はチェックできない
                string mark = matchedMaterials.Count > 0 ? $" <== ★MATCH [{string.Join(", ", matchedMaterials)}]" : "";

                Debug.Log($"OnProcessShader: {shader.name} [Stage:{snippet.shaderType}] {string.Join(",", variantKeywords)}{mark}");

                foreach (var keyword in data[i].shaderKeywordSet.GetShaderKeywords())
                {
                    if (keyword.name == "_STRIP")
                    {
                        Debug.Log($"{keyword.name}を{shader.name}から削除");
                        data.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
