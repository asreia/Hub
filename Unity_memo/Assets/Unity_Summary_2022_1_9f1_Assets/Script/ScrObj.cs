using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrObj : ScriptableObject, ISerializationCallbackReceiver
{
    public string fieldData; //参照型のプロパティは適当な参照型のインスタンスで初期化される?
    public bool enableCallBuckLog = true;
    void Awake()
    {
        //CreateInstanceやAssetロード時呼ばれる//初期化
        if(enableCallBuckLog) Debug.Log($"==Awake({this.name})==");
    }
    void OnEnable()
    {
        //Awakeの直後と、ドメインリロード後に呼ばれる
        if(enableCallBuckLog) Debug.Log($"==OnEnable({this.name})==");
    }
    void OnDisable()
    {
        //OnDestroyの直前と、ドメインリロード前に呼ばれる (DeleteAssetで呼ばれない事を確認(呼ばれない事もある))
        if(enableCallBuckLog) Debug.Log($"==OnDisable({this.name})==");
    }
    void OnDestroy()
    {
        //明示的Destroy時は恐らく必ず呼ぶ。UnloadUnusedAssetsも呼ばない。PlayMode終了時のAllWeakDestroyで呼ばれない事もある(DeleteAssetも呼ばない)
        if(enableCallBuckLog) Debug.Log($"==OnDestroy({this.name})=="); 
    }
    private static float prevTime = 0f;
    public void OnBeforeSerialize() //インターフェースの実装なのでpublicが要る
    {
        // float elapsedTime = Time.realtimeSinceStartup - prevTime;
        // if(elapsedTime > 10.0f){prevTime = Time.realtimeSinceStartup; Debug.Log("OnBeforeSerialize()");}
    }
    public void OnAfterDeserialize()
    {
        Debug.Log("OnAfterDeserialize()");
    }
}

//テクスチャなどの一般的なAssetも試す
