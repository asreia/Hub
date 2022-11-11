using UnityEngine;
using UnityEditor;

//https://kan-kikuchi.hatenablog.com/entry/playModeStateChanged
[InitializeOnLoad]
public static class DomainReloadOnPlayModeExit {
  static DomainReloadOnPlayModeExit() 
  {
    EditorApplication.playModeStateChanged += 
      (state) => 
      {
        if (state == PlayModeStateChange.EnteredEditMode) //.ExitingPlayMode //実行状態の終了開始！(停止ボタンを押した) 
        {
          Debug.Log("ドメインリロード");
          EditorUtility.RequestScriptReload(); //C#のstaticメンバを初期化する
        }
      };
  }
}