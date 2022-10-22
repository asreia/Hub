/*
メモ
    全て試してないがエディターの状態によるコールバックとチェック系があり
    それとエディターの操作系がある感じ
Static変数
    UnityEditorの実行ファイルとデータの場所
        applicationContentsPath	//=> 2020.1.7f1/Editor/Unity.exe
        applicationPath	//=> 2020.1.7f1/Editor/Data

    デリゲート
        delayCall //MonoBehaviourで言うLateUpdate?で、すべてのインスペクターが更新された後に1度だけ呼び出され廃棄されるデリゲート
        とupdate  //MonoBehaviourで言うUpdateで200fspで描画系処理の前に実行。
            //http://49.233.81.186/callbacks.html#h13-12 の図 //http://49.233.81.186/callbacks.html#h13-11
        modifierKeysChanged	キーボードの修飾キーが更新されたときに呼び出されるデリゲート
            //http://49.233.81.186/callbacks.html#h13-10
        searchChanged	ウィンドウの検索ボックスの内容が変更されるごとに呼び出されるコールバック
        contextualPropertyMenu //Inspectorでプロパティを右クリックのメニュー
            //https://baba-s.hatenablog.com/entry/2017/12/04/090000#Inspector-%E3%81%A7%E3%83%97%E3%83%AD%E3%83%91%E3%83%86%E3%82%A3%E3%81%8C%E5%8F%B3%E3%82%AF%E3%83%AA%E3%83%83%E3%82%AF%E3%81%95%E3%82%8C%E3%81%9F%E6%99%82
        OnGUI
            hierarchyWindowItemOnGUI //OnGUI イベントごとに HierarchyWindow に表示されているリストごとに呼び出されるデリゲート
            とprojectWindowItemOnGUI //OnGUI イベントごとに projectWindow   に表示されているリストごとに呼び出されるデリゲート
                //http://49.233.81.186/callbacks.html#h13-8
    チェック系
        isUpdating	エディタが現在AssetDatabaseを更新している場合はTrue。

        デリゲート playModeStateChanged //エディタの再生モードの状態が変化するたびに発生するイベント。
            isPaused //エディターが一時停止中か
            isPlaying //エディターが再生モードか
            isPlayingOrWillChangePlaymode //エディターが現在再生モードか、再生モードに切り替えようとしているかどうか
                //説明と便利なクラス: https://kan-kikuchi.hatenablog.com/entry/PlaymodeStateObserver
        isCompiling	現在スクリプトがコンパイルされているかどうか（読み取り専用）
        isRemoteConnected	エディターが Unity Remote 4 と接続しているかどうか
        isTemporaryProject	現在のプロジェクトが一時プロジェクトとして作成された場合はtrueを返します。
            static関数 SetTemporaryProjectKeepPath	一時プロジェクトが閉じられたときに、Unityが現在の一時プロジェクトを保存するパスを設定します。
    時間
        timeSinceStartup	エディターが起動してからの時間 エディターの時間計測に使える
            //Time.timeとTime.realtimeSinceStartupはランタイム?
            //https://zaki0929.github.io/page42.html

Static 関数
    操作系
        OpenProject	    //他の Unity プロジェクトを開きます
        ExecuteMenuItem	//パスを指定して MenuItem を実行します
        Beep	        //PC システムのビープ音を再生します。
        Exit	        //UnityEditor を終了させます
        プレイモード
            EnterPlaymode	//ゲーム再生(EditorApplication.isPlayingをtrueに設定するのと同じ)
            Step	        //ゲームステップ実行(StepPlaymodeではない謎)
            ExitPlaymode	//ゲーム終了
    Repaint(再描画)
        RepaintHierarchyWindow	//HierarchyWindow の再描画を行うために使用することができます。
        RepaintProjectWindow	//ProjectWindow の再描画を行うために使用することができます。
        RepaintAnimationWindow  //AnimationWindow の再描画を行うために使用することができます?。
    コンパイルロック
        LockReloadAssemblies	//Assembly の再コンパルを行わないようにロックします。
        UnlockReloadAssemblies	//EditorApplication.LockReloadAssemblies でロックしたアセンブリロードを解除します
    DirtyHierarchyWindowSorting	//ヒエラルキーのオブジェクトソートが、最新状態ではないとしてダーティーフラグを立てます。
        //ヒエラルキーウインドウがソートされる?良く分からない
    QueuePlayerLoopUpdate	通常、シーンが変更されると、エディターでプレーヤーループの更新が発生します。このメソッドを使用すると、シーンが変更されているかどうかに関係なく、プレーヤーループの更新をキューに入れることができます。
        //良く分からないがうまく画面が更新しない時に呼び出すと良い?
        //https://baba-s.hatenablog.com/entry/2019/05/27/000000

イベント
    hierarchyChanged	    //Hierarchyのオブジェクトまたはオブジェクトのグループが変更されたときに発生するイベント。
    projectChanged	        //プロジェクトの状態が変化するたびに発生するイベント。
    playModeStateChanged	//エディタの再生モードの状態が変化するたびに発生するイベント。
    pauseStateChanged	    //エディタの一時停止状態が変化するたびに発生するイベント。
    wantsToQuit	            //Unityは、エディターアプリケーションが終了したいときにこのイベントを発生させます。
    quitting	            //Unityは、エディターアプリケーションが終了するときにこのイベントを発生させます。
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class EditorApplicationTest
{
    public static int eatInt = 423;
    [MenuItem("EditorApplication/Test0")]
    public static void EditorApplicationTest0(){
        // Debug.Log(NormalClass.normalInt);
        Debug.Log($"applicationPath: {EditorApplication.applicationPath}");
        Debug.Log($"applicationContentsPath: {EditorApplication.applicationContentsPath}");

        EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
        EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
            //一個だけデリゲートを登録できる
        EditorApplication.delayCall -= DelayCall;
        EditorApplication.delayCall += DelayCall;
    }
    [MenuItem("EditorApplication/BasicOperation/OpenProject")]
    static void OpenProject(){
        EditorApplication.OpenProject("C:/Users/asreia/戻ってきた");
        Debug.Log("OpenProject");
    }
    [MenuItem("EditorApplication/BasicOperation/ExecuteMenuItem")]
    static void ExecuteMenuItem(){
        EditorApplication.ExecuteMenuItem("EditorApplication/RepaintHierarchyAndProjectAndAnimationWindow");
        Debug.Log("ExecuteMenuItem");
    }
    [MenuItem("EditorApplication/BasicOperation/Beep")]
    static void Beep(){
        EditorApplication.Beep();
        Debug.Log("Beep");
    }
    [MenuItem("EditorApplication/BasicOperation/Exit")]
    static void Exit(){
        EditorApplication.Exit(-1);
        Debug.Log("Exit");
    }
    [MenuItem("EditorApplication/PlayMode/EnterPlaymode")]
    static void EnterPlaymode(){
        EditorApplication.EnterPlaymode();
        Debug.Log("EnterPlaymode");
    }
    [MenuItem("EditorApplication/PlayMode/Step")]
    static void Step(){
        EditorApplication.Step();
        Debug.Log("Step");
    }
    [MenuItem("EditorApplication/PlayMode/ExitPlaymode")]
    static void ExitPlaymode(){
        EditorApplication.ExitPlaymode();
        Debug.Log("ExitPlaymode");
    }
    static bool toggle = false;
    [MenuItem("EditorApplication/ToggleCompileLock")]
    public static void ToggleCompileLock(){
        toggle = !toggle;
        if(toggle){
            EditorApplication.LockReloadAssemblies();
            Debug.Log("LockReloadAssemblies");
        }
        else{
            EditorApplication.UnlockReloadAssemblies();
            Debug.Log("UnlockReloadAssemblies");

        }
    }
    [MenuItem("EditorApplication/RepaintHierarchyAndProjectAndAnimationWindow")]
    private/*privateでも呼ばれるのはCecilか?*/ static void RepaintHierarchyAndProjectAndAnimationWindow(){
        EditorApplication.RepaintHierarchyWindow();
        EditorApplication.RepaintProjectWindow();
        EditorApplication.RepaintAnimationWindow();
        Debug.Log("Repainted");
    }
    [MenuItem("EditorApplication/Checks")]
    public static void Checks(){
        Debug.Log($"isUpdating: #{EditorApplication.isUpdating}#");
        Debug.Log($"isPlaying: #{EditorApplication.isPlaying}# isPaused: #{EditorApplication.isPaused}# isPlayingOrWillChangePlaymode: #{EditorApplication.isPlayingOrWillChangePlaymode}#");
        Debug.Log($"isCompiling: #{EditorApplication.isCompiling}# isRemoteConnected: #{EditorApplication.isRemoteConnected}# isTemporaryProject: #{EditorApplication.isTemporaryProject}#");
    }
    [MenuItem("EditorApplication/isUpdating after Refresh")]
    public static void IsUpdatingAfterRefresh(){
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log($"isUpdating: #{EditorApplication.isUpdating}#");//=> false
    }

    static void DelayCall(){
        Debug.Log("DelayCall");
    }
    static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property){
        Debug.Log($"contextualPropertyMenu_displayName: {property.displayName}");
        Debug.Log($"contextualPropertyMenu_name: {property.name}");
    }
}
