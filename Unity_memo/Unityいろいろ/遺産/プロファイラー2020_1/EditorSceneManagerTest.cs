/*
メモ: シーンにはロード状態かアンロード状態とアクティブ状態がある
    ロード状態はメモリに読まれていて実行と参照可能な状態
    アクティブ状態はInstanciteなどのUnityAPIが操作する対象とするScene (その他レンダリングとnavMeshに影響があるらしい)

Static 関数
    操作系
        作成
            NewScene(NewSceneSetup, NewSceneMode)	新しくシーンを作成します
                //Sceneをプロジェクトに保存しないでNewSceneMode.Additiveするとエラー
                //NewSceneMode.: Single, Additive 
            NewPreviewScene	新しいプレビューシーンを作成します。プレビューシーンに追加されたオブジェクトは、そのシーンでのみレンダリングされます。
        開く
            OpenScene(string(パス), OpenSceneMode)	エディターでシーンを開きます。
                //OpenSceneMode.: Single, Additive, AdditiveWithoutLoading
        閉じる
            CloseScene(Scene, bool(removeScene))	シーンを閉じます。 
                //removeScene フラグが true であれば閉じたシーンは EditorSceneManager から削除されます。(unLoadの様になる)
            ClosePreviewScene	NewPreviewSceneによって作成されたプレビューシーンを閉じます。
        移動
            MoveSceneAfter	現在ヒエラルキーウィンドウで開いているシーンの順番を入れ替えられます。 ソースシーンは移動先シーンの後に移動します。
            MoveSceneBefore	現在ヒエラルキーウィンドウで開いているシーンの順番を入れ替えられます。 ソースシーンは移動先シーンの前に移動します。
        修正済マーク
            MarkAllScenesDirty	読み込まれているすべてのシーンを修正済みとして印を付ける。
            MarkSceneDirty	                          シーンを修正済みとして印を付ける
        セーブ
            SaveScene(Scene scene, string path, bool saveAsCopy) シーンを保存します。
                //pathを省略すると保存先ダイアログが出る
                //saveAsCopyをtrueにすると多分、Sceneを変更せずDirtyをクリアしない
            SaveScenes(Scene[] scenes)	シーンのリストを保存します。
            SaveOpenScenes()	開いている(ヒエラルキーに表示)シーンをすべて保存します。
            SaveModifiedScenesIfUserWantsTo(Scene[] scenes)	SaveScenesのダイアログ版
            SaveCurrentModifiedScenesIfUserWantsTo() SaveOpenScenesのダイアログ版
            EnsureUntitledSceneHasBeenSaved	Untitledなシーンが現在のシーンマネージャーの設定に存在する場合、保存ダイアログを表示します。
                //Untitled(projectに保存されていない?)のみ保存される
        プレイモード時ロード
            LoadSceneInPlayMode	     ビルド設定シーンリストに含めないでシーンをプレイモード時にロードできる
            LoadSceneAsyncInPlayMode ビルド設定シーンリストに含めないでシーンをプレイモード時にロードできる(非同期(ロード完了はScene.isLoadを監視?))
    基本的なシーン情報
        [Static変数] loadedSceneCount	読み込まれているシーン数
        [Static変数] playModeStartScene	再生モードを開始すると、このSceneAssetが読み込まれます。
        [Static変数] previewSceneCount	アクティブなプレビューシーンの現在の量。 
        SceneSetUp
            GetSceneManagerSetup	SceneManager の現在の SceneSetUp を返します。
            RestoreSceneManagerSetup	SceneManager の SceneSetup を復元します。
    チェック系
        IsPreviewScene	シーンはプレビューシーンですか？
        IsPreviewSceneObject	このオブジェクトはプレビューシーンの一部ですか？
    CrossSceneReferences
        [Static変数]PreventCrossSceneReferences	他のシーンを指す参照 (cross-Scene references) がエディターで可能かどうかを制御します。
            //falseにするとCrossSceneReferencesが有効になり"エディタ上で"他のシーンのオブジェクトを参照できる
            //デフォルトで無効なのは再生時に他のシーンがロードされていないため
        DetectCrossSceneReferences	シーン内のクロスシーン参照を検出します。
            //プロパティで他のオブジェクトを参照しているとtrue
    CullingMask
        //DefaultSceneCullingMask(定数)がsceneのSceneCullingMaskのデフォルト値を決定し
        //GameObjectのsceneCullingMaskはsceneのSceneCullingMaskをgetしているだけ
        //sceneのSceneCullingMaskをDefaultSceneCullingMask(定数)に設定するとGameObjectが表示される
        //結局CullingMask良くわからない
        [Static変数] DefaultSceneCullingMask	すべてのカメラによって描画されるシーンカリングマスク。すべてのシーンは、デフォルトでこのカリングマスクで始まります。
        CalculateAvailableSceneCullingMask	すべてのシーンを調べて、すべてのシーンカリングマスクの組み合わせで未使用の最小ビットを見つけます。
        取得と設定
            GetSceneCullingMask	指定されたシーンに設定されているカリングマスクを返します。
            SetSceneCullingMask	このシーンのカリングマスクをこの値に設定します。カメラは、カリングマスクに同じビットが設定されているシーン内のオブジェクトのみをレンダリングします。
イベント
    activeSceneChangedInEditMode(Scene beforeScene, Scene afterScene)
        //アクティブシーンが変更された時
    newSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        //新しいシーンが作成された後
    sceneOpened(Scene scene, OpenSceneMode mode)
    sceneOpening(string path, OpenSceneMode mode)
        //シーンを開く前と後
    sceneClosed(Scene scene)
    sceneClosing(Scene scene, bool removingScene)
        //シーンを閉じる前と後 (removingSceneはunLoadはfalseで削除はtrueのようだ)
    sceneSaved(Scene scene)
    sceneSaving(Scene scene, string path)
        //シーンを保存する前と後
    sceneDirtied(Scene scene)
        //シーンを汚した後
NewSceneMode.Additive系フラグ
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;


public class EditorSceneManagerTest{
    public static void activeSceneChangedInEditMode_caller(Scene beforeScene, Scene afterScene){
        Debug.Log("activeSceneChangedInEditMode====================");
        Debug.Log($"beforeScene: {beforeScene.name}, afterScene: {afterScene.name}");
    }
    public static void newSceneCreated_caller(Scene scene, NewSceneSetup setup, NewSceneMode mode){
        Debug.Log("newSceneCreated====================");
        Debug.Log($"scene: {scene.name}, setup: {setup.ToString()}, mode: {mode.ToString()}");
    }
    public static void sceneOpened_caller(Scene scene, OpenSceneMode mode){
        Debug.Log("sceneOpened====================");
        Debug.Log($"scene: {scene.name}, mode: {mode.ToString()} ");
    }
    public static void sceneOpening_caller(string path, OpenSceneMode mode){
        Debug.Log("sceneOpening====================");
        Debug.Log($"path: {path}, mode: {mode.ToString()}");
    }
    public static void sceneClosed_caller(Scene scene){
        Debug.Log("sceneClosed====================");
        Debug.Log($"scene: {scene.name}");
    }
    public static void sceneClosing_caller(Scene scene, bool removingScene){
        Debug.Log("sceneClosing====================");
        Debug.Log($"scene: {scene.name}, removingScene: {removingScene}");
    }
    public static void sceneSaved_caller(Scene scene){
        Debug.Log("sceneSaved====================");
        Debug.Log($"scene: {scene.name}");
    }
    public static void sceneSaving_caller(Scene scene, string path){
        Debug.Log("sceneSaving====================");
        Debug.Log($"scene: {scene.name}, path: {path}");
    }
    public static void sceneDirtied_caller(Scene scene){
        Debug.Log("sceneDirtied====================");
        Debug.Log($"scene: {scene.name}");
    }

    [MenuItem("EditorSceneManagerTest/イベント登録")]
    public static void イベント登録(){
        EditorSceneManager.activeSceneChangedInEditMode -= activeSceneChangedInEditMode_caller;
        EditorSceneManager.activeSceneChangedInEditMode += activeSceneChangedInEditMode_caller;
        EditorSceneManager.newSceneCreated -= newSceneCreated_caller;
        EditorSceneManager.newSceneCreated += newSceneCreated_caller;
        EditorSceneManager.sceneOpened -= sceneOpened_caller;
        EditorSceneManager.sceneOpened += sceneOpened_caller;
        EditorSceneManager.sceneOpening -= sceneOpening_caller;
        EditorSceneManager.sceneOpening += sceneOpening_caller;
        EditorSceneManager.sceneClosed -= sceneClosed_caller;
        EditorSceneManager.sceneClosed += sceneClosed_caller;
        EditorSceneManager.sceneClosing -= sceneClosing_caller;
        EditorSceneManager.sceneClosing += sceneClosing_caller;
        EditorSceneManager.sceneSaved -= sceneSaved_caller;
        EditorSceneManager.sceneSaved += sceneSaved_caller;
        EditorSceneManager.sceneSaving -= sceneSaving_caller;
        EditorSceneManager.sceneSaving += sceneSaving_caller;
        EditorSceneManager.sceneDirtied -= sceneDirtied_caller;
        EditorSceneManager.sceneDirtied += sceneDirtied_caller;
    }
    [MenuItem("EditorSceneManagerTest/イベント解除")]
    public static void イベント解除(){
        EditorSceneManager.activeSceneChangedInEditMode -= activeSceneChangedInEditMode_caller;
        EditorSceneManager.newSceneCreated -= newSceneCreated_caller;
        EditorSceneManager.sceneOpened -= sceneOpened_caller;
        EditorSceneManager.sceneOpening -= sceneOpening_caller;
        EditorSceneManager.sceneClosed -= sceneClosed_caller;
        EditorSceneManager.sceneClosing -= sceneClosing_caller;
        EditorSceneManager.sceneSaved -= sceneSaved_caller;
        EditorSceneManager.sceneSaving -= sceneSaving_caller;
        EditorSceneManager.sceneDirtied -= sceneDirtied_caller;
    }
    static Scene scene0, scene1;
    [MenuItem("EditorSceneManagerTest/操作系/作成/NewScene0")]
    public static void NewScene0(){
        scene0 = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
        scene0.name = "scene0";
        Debug.Log($"NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive): {scene0}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/作成/NewScene1")]
    public static void NewScene1(){
        scene1 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        scene1.name = "scene1";
        Debug.Log($"NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive): {scene1}");
    }
    static Scene previewScene;
    [MenuItem("EditorSceneManagerTest/操作系/作成/NewPreviewScene")]
    public static void NewPreviewScene(){
        previewScene = EditorSceneManager.NewPreviewScene();
        Debug.Log($"NewPreviewScene(): {previewScene}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/開く/OpenScene")]
    public static void OpenScene(){
        Scene OpenScene = EditorSceneManager.OpenScene("Assets/EditorSceneManager/Scenes/LoadMe.unity", OpenSceneMode.AdditiveWithoutLoading);
        Debug.Log($"OpenSceneOpenScene(\"~/LoadMe.unity\", OpenSceneMode.AdditiveWithoutLoading): {OpenScene}");
    }

    [MenuItem("EditorSceneManagerTest/操作系/閉じる/CloseScene0")]
    public static void CloseScene0(){
        
        bool b = EditorSceneManager.CloseScene(scene0, true);
        Debug.Log($"CloseScene(scene0, true(removeScene): {b}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/閉じる/CloseScene1")]
    public static void CloseScene1(){
        bool b = EditorSceneManager.CloseScene(scene0, false);
        Debug.Log($"CloseScene(scene0, false(removeScene)): {b}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/閉じる/ClosePreviewScene")]
    public static void ClosePreviewScene(){
        bool b = EditorSceneManager.ClosePreviewScene(previewScene);
        Debug.Log($"ClosePreviewScene: {b}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/移動/MoveSceneAfter")]
    public static void MoveSceneAfter(){
        EditorSceneManager.MoveSceneAfter(scene1, scene0);
        Debug.Log($"MoveSceneAfter: void");
    }
    [MenuItem("EditorSceneManagerTest/操作系/移動/MoveSceneBefore")]
    public static void MoveSceneBefore(){
        EditorSceneManager.MoveSceneBefore(scene1, scene0);
        Debug.Log($"MoveSceneBefore: void");
    }
    [MenuItem("EditorSceneManagerTest/操作系/修正済みマーク/MarkAllScenesDirty")]
    public static void MarkAllScenesDirty(){
        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"MarkAllScenesDirty: void");
    }
    [MenuItem("EditorSceneManagerTest/操作系/修正済みマーク/MarkSceneDirty")]
    public static void MarkSceneDirty(){
        EditorSceneManager.MarkSceneDirty(scene0);
        Debug.Log($"MarkSceneDirty: void");
    }
    [MenuItem("EditorSceneManagerTest/操作系/セーブ/SaveScene")]
    public static void SaveScene(){
        bool b = EditorSceneManager.SaveScene(scene0, "Assets/EditorSceneManager/Scenes/Scene0.unity", false);
        Debug.Log($"SaveScene: {b}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/セーブ/SaveScenes")]
    public static void SaveScenes(){
        bool b = EditorSceneManager.SaveScenes(new Scene[]{scene0, scene1});
        Debug.Log($"SaveScenes: {b}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/セーブ/SaveOpenScenes")]
    public static void SaveOpenScenes(){
        bool b = EditorSceneManager.SaveOpenScenes();
        Debug.Log($"SaveOpenScenes: {b}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/セーブ/SaveModifiedScenesIfUserWantsTo")]
    public static void SaveModifiedScenesIfUserWantsTo(){
        bool b = EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new Scene[]{scene0, scene1});
        Debug.Log($"SaveModifiedScenesIfUserWantsTo: {b}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/セーブ/SaveCurrentModifiedScenesIfUserWantsTo")]
    public static void SaveCurrentModifiedScenesIfUserWantsTo(){
        bool b = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log($"SaveCurrentModifiedScenesIfUserWantsTo: {b}"); 
    }
    [MenuItem("EditorSceneManagerTest/操作系/セーブ/PreventUntitledSceneHasBeenSaved")]
    public static void EnsureUntitledSceneHasBeenSaved(){
        bool b = EditorSceneManager.EnsureUntitledSceneHasBeenSaved("EnsureUntitledSceneHasBeenSaved");
        Debug.Log($"EnsureUntitledSceneHasBeenSaved: {b}"); 
    }
    [MenuItem("EditorSceneManagerTest/操作系/プレイモード時ロード/LoadSceneInPlayMode")]
    public static void LoadSceneInPlayMode(){
        Scene scene = EditorSceneManager.LoadSceneInPlayMode("Assets/EditorSceneManager/Scenes/Scene1.unity", new LoadSceneParameters(LoadSceneMode.Additive));
        Debug.Log($"LoadSceneInPlayMode: {scene}");
    }
    [MenuItem("EditorSceneManagerTest/操作系/プレイモード時ロード/LoadSceneAsyncInPlayMode")]
    public static void LoadSceneAsyncInPlayMode(){
        AsyncOperation ao = EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/EditorSceneManager/Scenes/Scene1.unity", new LoadSceneParameters(LoadSceneMode.Additive));
        Debug.Log($"LoadSceneAsyncInPlayMode: {ao}");
    }
    static SceneSetup[] sss;
    [MenuItem("EditorSceneManagerTest/基本的なシーン情報/EditorSceneInfo")]
    public static void EditorSceneInfo(){
        Debug.Log($"loadedSceneCount: {EditorSceneManager.loadedSceneCount}, playModeStartScene: {EditorSceneManager.playModeStartScene}, PreviewSceneCount: {EditorSceneManager.previewSceneCount}");
        sss = EditorSceneManager.GetSceneManagerSetup();
        Debug.Log("==SceneSetup==");
        foreach(SceneSetup ss in sss){
            Debug.Log($"path: {ss.path}");
            Debug.Log($"isActive: {ss.isActive},isLoaded :{ss.isLoaded}, isSubScene: {ss.isSubScene}");
        }
        Debug.Log("==(End)SceneSetup==");
    }
    [MenuItem("EditorSceneManagerTest/基本的なシーン情報/RestoreSceneManagerSetup")]
    public static void RestoreSceneManagerSetup(){
        EditorSceneManager.RestoreSceneManagerSetup(sss);
        Debug.Log($"RestoreSceneManagerSetup: void");
    }
    [MenuItem("EditorSceneManagerTest/CullingMask/CullingMaskInfo")]
    public static void CullingMaskInfo(){
        Debug.Log($"DefaultSceneCullingMask: {EditorSceneManager.DefaultSceneCullingMask}, CalculateAvailableSceneCullingMask: {EditorSceneManager.CalculateAvailableSceneCullingMask()}");
        Debug.Log($"GetSceneCullingMask(scene1): {EditorSceneManager.GetSceneCullingMask(scene1)}");
    }

    [MenuItem("EditorSceneManagerTest/CullingMask/SetSceneCullingMask")]
    public static void SetSceneCullingMask(){
        SceneManager.SetActiveScene(scene1);
        GameObject go0 = new GameObject("CullingMaskTest0");
        Debug.Log(go0.sceneCullingMask);
        EditorSceneManager.SetSceneCullingMask(scene1, 0b101010111);
        Debug.Log($"SetSceneCullingMask(scene1, 0b101010111): void");
        Debug.Log(go0.sceneCullingMask);
        Debug.Log(new GameObject("CullingMaskTest1").sceneCullingMask);

    }
    [MenuItem("EditorSceneManagerTest/CullingMask/Default_SetSceneCullingMask")]
    public static void Default_SetSceneCullingMask(){
        EditorSceneManager.SetSceneCullingMask(scene1,EditorSceneManager.DefaultSceneCullingMask);
        Debug.Log($"SetSceneCullingMask(scene1,EditorSceneManager.DefaultSceneCullingMask): void");
    }
    [MenuItem("EditorSceneManagerTest/チェック系/IsPreviewScene")]
    public static void IsPreviewScene(){
        bool b = EditorSceneManager.IsPreviewScene(scene0);
        Debug.Log($"IsPreviewScene(scene0): {b}");

        Scene previewScene = EditorSceneManager.NewPreviewScene();
        Debug.Log($"IsPreviewScene(NewPreviewScene()): {EditorSceneManager.IsPreviewScene(previewScene)}");

        SceneManager.SetActiveScene(scene0);
        GameObject go = new GameObject();
        Debug.Log($"IsPreviewSceneObject(go): {EditorSceneManager.IsPreviewSceneObject(go)}");

        // SceneManager.SetActiveScene(previewScene);
            //プレビューシーンをアクティブに設定することはできません
        // GameObject psgo = new GameObject();
        // Debug.Log($"IsPreviewSceneObject(psgo): {EditorSceneManager.IsPreviewSceneObject(psgo)}");
    }

    [MenuItem("EditorSceneManagerTest/CrossSceneReferences")]
    public static void CrossSceneReferences(){
        EditorSceneManager.preventCrossSceneReferences = false;
        Debug.Log($"preventCrossSceneReferences: {EditorSceneManager.preventCrossSceneReferences}");
        Debug.Log($"DetectCrossSceneReferences(scene0): {EditorSceneManager.DetectCrossSceneReferences(scene0)}");
    }

    // [MenuItem("EditorSceneManagerTest/CullingMask/Default")]
    // public static void Default(){
    //     EditorSceneManager.Default
    //     Debug.Log($"Default: {}, ")
    // }


}