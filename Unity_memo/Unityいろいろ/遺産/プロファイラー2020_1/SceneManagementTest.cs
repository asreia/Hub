/*
https://madnesslabo.net/utage/?page_id=11109
https://www.urablog.xyz/entry/2018/02/11/164734
Static 変数
●sceneCount	現在ロードされているシーンの総数。
●sceneCountInBuildSettings	ビルド設定のシーン数。

Static 関数
●CreateScene	指定された名前で実行時に空の新しいシーンを作成します。
●SetActiveScene	シーンをアクティブに設定します。
●GetActiveScene	現在アクティブなシーンを取得します。
●GetSceneAt	SceneManagerのロードされたシーンのリストのインデックスでシーンを取得します。
    //ランタイム時、ロードされているSceneのリストのみ取得
●GetSceneByBuildIndex	ビルド設定で"ビルドに含まれるシーン"のリストのインデックスのみ
    //エディタ上のランタイム時ではビルド設定のチェックボックスにチェックしてなくても取得する..実機では違う?
●GetSceneByName	ロードされたシーンを検索して、指定された名前のシーンを探します。
●GetSceneByPath	ロードされたすべてのシーンで、指定されたアセットパスを持つシーンを検索します。
●LoadScene	ビルド設定の名前またはインデックスでシーンをロードします。
●LoadSceneAsync	シーンをバックグラウンドで非同期にロードします。
●MoveGameObjectToScene	GameObjectを現在のシーンから新しいシーンに移動します。
●UnloadSceneAsync	指定されたシーンに関連付けられているすべてのゲームオブジェクトを破棄し、SceneManagerからシーンを削除します。
●MergeScenes	これにより、ソースシーンがdestinationSceneにマージされます。

LoadSceneParamitersとは
SceneとはGameObjectの集合をメモリ上にLoadする単位?
あるSceneをLoadしようとするとなんの制約もなく無条件でLoadできる
Singleは単にLoadされている全てのSceneをUnLoadし目的のSceneをLoadする
Additiveは単に目的のSceneがLoadされるだけ
SceneはGameObjectへの参照を持っているだけ?

イベント
●activeSceneChanged	このイベントを購読して、アクティブなシーンが変更されたときに通知を受け取ります。
●sceneLoaded	これにデリゲートを追加して、シーンがロードされたときに通知を受け取ります。
●sceneUnloaded	これにデリゲートを追加して、シーンがアンロードされたときに通知を受け取ります。
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor;

public class SceneManagementTest{
    static Scene scene0;
    [MenuItem("SceneManagement/CreateScene")]
    public static void CreateScene(){
        scene0 = SceneManager.CreateScene("CreateScene");
    }
    [MenuItem("SceneManagement/ActiveScene/GetActiveScene")]
    public static void GetActiveScene(){
        Debug.Log($"GetActiveScene() :{SceneManager.GetActiveScene().name}");
    }
    [MenuItem("SceneManagement/ActiveScene/SetActiveScene")]
    public static void SetActiveScene(){
        Debug.Log($"SetActiveScene() :{SceneManager.SetActiveScene(scene0)}");
    }
    [MenuItem("SceneManagement/GetScene/GetSceneAt")]
    public static void GetSceneAt(){
        Debug.Log("GetSceneAt================");
        Debug.Log("sceneCount: " + SceneManager.sceneCount);
        for(int index = 0; index < SceneManager.sceneCount; index++){
            Debug.Log($"GetSceneAt(index): {SceneManager.GetSceneAt(index).name}, isLoaded: {SceneManager.GetSceneAt(index).isLoaded}");
        }
        Debug.Log("End_GetSceneAt================");

    }
    [MenuItem("SceneManagement/GetScene/GetSceneByBuildIndex")]
    public static void GetSceneByBuildIndex(){
        Debug.Log("GetSceneByBuildIndex================");
        Debug.Log("sceneCountInBuildSettings: " + SceneManager.sceneCountInBuildSettings);
        for(int index = 0; index < SceneManager.sceneCountInBuildSettings; index++){
            Debug.Log($"GetSceneByBuildIndex(index): {SceneManager.GetSceneByBuildIndex(index).name}, isLoaded: {SceneManager.GetSceneByBuildIndex(index).isLoaded}");
        }
        Debug.Log("End_GetSceneByBuildIndex================");
    }
    [MenuItem("SceneManagement/GetScene/GetSceneByName")]
    public static void GetSceneByName(){
        Scene scene = SceneManager.GetSceneByName("FindScene");
        Debug.Log($"GetSceneByName(\"FindScene\"): {scene.name}");
    }
    [MenuItem("SceneManagement/GetScene/GetSceneByPath")]
    public static void GetSceneByPath(){
        Scene scene = SceneManager.GetSceneByPath("Assets/Scenes/FindScene.unity");
        Debug.Log($"GetSceneByPath(\"Assets/Scenes/FindScene.unity\"): {scene.name}");
    }
    [MenuItem("SceneManagement/UnloadSceneAsync")]
    public static void UnloadSceneAsync(){
        AsyncOperation ao = SceneManager.UnloadSceneAsync("MultiSceneEditing", UnloadSceneOptions.None);
        Debug.Log($"UnloadSceneAsync: {ao.ToString()}");
    }
    [MenuItem("SceneManagement/LoadScene/LoadScene")]
    public static void LoadScene(){
        SceneManager.LoadScene("MultiSceneEditing", LoadSceneMode.Additive);
        Debug.Log($"LoadScene(\"MultiSceneEditing\", .Additive): void");
    }
    [MenuItem("SceneManagement/LoadScene/LoadSceneAsync")]
    public static void LoadSceneAsync(){
        SceneManager.LoadSceneAsync("MultiSceneEditing", LoadSceneMode.Additive);
        Debug.Log($"LoadSceneAsync(\"MultiSceneEditing\", .Additive): void");
    }
    [MenuItem("SceneManagement/MoveGameObjectToScene")]
    public static void MoveGameObjectToScene(){
        SceneManager.MoveGameObjectToScene
            (GameObject.Find("Hako"), SceneManager.GetSceneByName("SampleScene"));
        Debug.Log($"MoveGameObjectToScene: void");
    }
    [MenuItem("SceneManagement/MergeScenes")]
    public static void MergeScenes(){
        SceneManager.MergeScenes
            (SceneManager.GetSceneByName("MultiSceneEditing"), SceneManager.GetSceneByName("FindScene"));
        Debug.Log($"MergeScenes(ByName(\"MultiSceneEditing\"), ByName(\"FindScene\"): void ");
    }

    [MenuItem("SceneManagement/イベント登録")]
    public static void イベント登録(){
        SceneManager.activeSceneChanged -= ActiveSceneChanged_CallBack;
        SceneManager.activeSceneChanged += ActiveSceneChanged_CallBack;
        SceneManager.sceneLoaded -= sceneLoaded_CallBack;
        SceneManager.sceneLoaded += sceneLoaded_CallBack;
        SceneManager.sceneUnloaded -= sceneUnloaded_CallBack;
        SceneManager.sceneUnloaded += sceneUnloaded_CallBack;
        Debug.Log($"イベント登録しました");
    }
    [MenuItem("SceneManagement/イベント解除")]
    public static void イベント解除(){
        SceneManager.activeSceneChanged -= ActiveSceneChanged_CallBack;
        SceneManager.sceneLoaded -= sceneLoaded_CallBack;
        SceneManager.sceneUnloaded -= sceneUnloaded_CallBack;
        Debug.Log($"イベント解除しました");
    }
    public static void ActiveSceneChanged_CallBack(Scene beforeScene, Scene afterScene){
        Debug.Log("activeSceneChanged==============");
        Debug.Log($"beforeScene: {beforeScene.name}, afterScene: {afterScene.name}");
    }
    public static void sceneLoaded_CallBack(Scene loadScene, LoadSceneMode mode){
        Debug.Log("sceneLoaded==============");
        Debug.Log($"loadScene: {loadScene.name}, mode: {mode.ToString()}");
    }
    public static void sceneUnloaded_CallBack(Scene unloadScene){
        Debug.Log("sceneUnloaded==============");
        Debug.Log($"{nameof(unloadScene)}: {unloadScene.name}");
    }
}
