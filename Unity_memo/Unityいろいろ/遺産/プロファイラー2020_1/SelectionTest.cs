/*
https://kan-kikuchi.hatenablog.com/entry/Selection
Static 変数
    UnityEngine.Object取得と設定
        Object activeObject{get;set;}
        Object[] objects{get;set;}
    InstanceID取得と設定
        int activeInstanceID{get;set;}
        int[] instanceIDs{get;set;}
    GUIDs取得と設定
        AssetGUIDs{get;}
    GameObject取得と設定
        GameObject activeGameObject{get;set}
        GameObject[] gameObjects{get;}
    Transform取得と設定
        Transform activeTransform{get;}
        Transform[] transforms{get;}
    アクティブ変更時のコールバック
        selectionChanged
    謎
        activeContext   SetActiveObjectWithContextを介して設定された、現在のコンテキストオブジェクトを返します。
Static 関数
    Contains(int instanceID)	選択中のオブジェクトに指定のオブジェクト情報(instanceID)を含んでいるか確認します
    typeやmodeでフィルタして取得
        GetFiltered<type>(SelectionMode mode)	   取得するtypeやmodeでフィルターをかけ、オブジェクトを取得します
        GetTransforms(SelectionMode mode)          //多分、GetFiltered<Transform> == GetTransforms
        SelectionMode
            SelectionMode.Unfiltered   //選択されたすべて
            SelectionMode.TopLevel     //一番上のもの
            SelectionMode.Deep         //選択したものと、子もすべて(子を選択していなくても)
            SelectionMode.ExcludePrefab//プレハブを除外したもの
            SelectionMode.Editable     //変更されていないオブジェクトを除外したもの
            SelectionMode.Assets       //アセットとしてのProjectウィンドウに存在するオブジェクトのみ
            SelectionMode.DeepAssets   //選択したものにフォルダーがあればその中身も全て(フォルダーの中身を選択していなくても)
            //複数指定する時は | で区切る
            SelectionMode.TopLevel | SelectionMode.ExcludePrefab
    謎
        SetActiveObjectWithContext	コンテキストを持つオブジェクトを選択します。
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SelectionTest{
    static Object obj;
    static Object[] objs;
    static int cnt = 0;
    [MenuItem("SelectionTest/Show_obj")]
    public static void Show_obj(){
        Debug.Log("obj.name: " + obj.name);
    }
    public static void activeが変更されました(){
        Debug.Log("activeが変更されました" + cnt++);
    }
    [MenuItem("SelectionTest/activeが変更されました解除")]
    public static void activeが変更されました解除(){
        Selection.selectionChanged -= activeが変更されました;
    }
    [MenuItem("SelectionTest/GetStatic変数")]
    public static void GetStatic変数(){
        Selection.selectionChanged -= activeが変更されました;
        Selection.selectionChanged += activeが変更されました;
        Debug.Log("GetStatic変数===============");
        Debug.Log($"activeContext: {Selection.activeContext?.name}");

        string objectsStr = "objects: ";
        for(int i = 0; i < Selection.objects.Length; i++){
            objectsStr += $"[{i}]: {Selection.objects[i]?.name}, ";
        }
        obj = Selection.activeObject;
        objs = Selection.objects;
        Debug.Log($"activeObject: {Selection.activeObject?.name}, {objectsStr}");

        string instanceIDStr = "instanceID: ";
        for(int i = 0; i < Selection.instanceIDs.Length; i++){
            instanceIDStr += $"[{i}]: {Selection.instanceIDs[i]}, ";
        }
        Debug.Log($"activeInstanceID: {Selection.activeInstanceID}, {instanceIDStr}");

        string gameObjectsStr = "gameObjects: ";
        for(int i = 0; i < Selection.gameObjects.Length; i++){
            gameObjectsStr += $"[{i}]: {Selection.gameObjects[i].name}, ";
        }
        Debug.Log($"activeGameObject: {Selection.activeGameObject?.name}, {gameObjectsStr}");

        string transformsStr = "transforms: ";
        for(int i = 0; i < Selection.transforms.Length; i++){
            transformsStr += $"[{i}]: {Selection.transforms[i].name}, ";
        }
        Debug.Log($"activeTransform: {Selection.activeTransform?.name}, {transformsStr}");

        string AssetGUIDsStr = "AssetGUIDs: ";
        for(int i = 0; i < Selection.assetGUIDs.Length; i++){
            AssetGUIDsStr += $"[{i}]: {Selection.assetGUIDs[i]}, ";
        }
        Debug.Log($"{AssetGUIDsStr}");
    }
    [MenuItem("SelectionTest/SetStatic変数/SetActiveGameObject")]
    public static void SetActiveGameObject(){
        Debug.Log("SetActiveGameObject===============");
        if(obj == null){
            Debug.Log("nullです。");
        }else if(obj is GameObject go){
            Selection.activeGameObject = go;
        }else{
            Debug.Log("GameObjectではありません。");
        }

    }
    [MenuItem("SelectionTest/SetStatic変数/SetActiveObject")]
    public static void SetActiveObject(){
        Debug.Log("SetActiveObject===============");
        if(obj == null){
            Debug.Log("nullです。");
        }else if(obj is Object o){
            Selection.activeObject = o;
        }
    }
    [MenuItem("SelectionTest/SetStatic変数/SetObjects")]
    public static void SetObjects(){
        Debug.Log("SetObjects===============");
        if(objs.Length == 0){
            Debug.Log("長さ0です。");
        }else if(objs is Object[] os){
            Selection.objects = os;
        }
    }
    [MenuItem("SelectionTest/SetStatic変数/SetActiveInstanceID")]
    public static void SetActiveInstanceID(){
        Debug.Log("SetActiveInstanceID===============");
        if(obj == null){
            Debug.Log("nullです。");
        }else if(obj is Object o){
            if(o.GetInstanceID() == 0) Debug.Log("0です。");
            Selection.activeInstanceID = o.GetInstanceID(); 
        }
    }
    [MenuItem("SelectionTest/SetStatic変数/SetInstanceIDs")]
    public static void SetInstanceIDs(){
        Debug.Log("SetInstanceIDs===============");
        if(objs.Length == 0){
            Debug.Log("長さ0です。");
        }else if(objs is Object[] os){
            int[] insIDs = new int[os.Length];
            for(int i = 0; i < os.Length; i++) insIDs[i] = os[i].GetInstanceID();
            Selection.instanceIDs = insIDs;
        }
    }
    [MenuItem("SelectionTest/SetActiveObjectWithContext")]
    static void SetActiveObjectWithContext(){
        Selection.SetActiveObjectWithContext(obj,obj);
        Debug.Log($"SetActiveObjectWithContext: void");
    }
    [MenuItem("SelectionTest/Contains")]
    static void Contains(){
        Debug.Log($"Contains(obj.GetInstanceID(): {Selection.Contains(obj.GetInstanceID())}");
    }
    [MenuItem("SelectionTest/GetFiltered")]
    static void GetFiltered(){
        Collider[] cs = Selection.GetFiltered<Collider>(SelectionMode.Deep);
        string names = "";
        for(int i = 0; i < cs.Length; i++){
            names += $"[{i}]: {cs[i].name}";
        }
        Debug.Log($"GetFiltered<Collider>(SelectionMode.Deep): {names}");
    }
    [MenuItem("SelectionTest/GetTransforms")]
    static void GetTransforms(){
        Transform[] ts = Selection.GetTransforms(SelectionMode.Deep);
        string names = "";
        for(int i = 0; i < ts.Length; i++){
            names += $"[{i}]: {ts[i].name}";
        }
        Debug.Log($"GetTransforms(SelectionMode.Deep): {names}");
    }
}
