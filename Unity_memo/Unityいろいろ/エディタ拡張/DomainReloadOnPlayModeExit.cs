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

public static class GraphicsBufferExt
{
    static uint[] copyCounter = new uint[1];
    static GraphicsBuffer copyCounterBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, 1, sizeof(uint));

    public static uint GetCounterValue(this GraphicsBuffer buffer)
    {
        if(buffer.target != GraphicsBuffer.Target.Append) throw new ArgumentException($"引数 {nameof(buffer)} が {GraphicsBuffer.Target.Append} ではありません");
        GraphicsBuffer.CopyCount(buffer, copyCounterBuffer, 0);
        copyCounterBuffer.GetData(copyCounter);
        return copyCounter[0];
    }
}