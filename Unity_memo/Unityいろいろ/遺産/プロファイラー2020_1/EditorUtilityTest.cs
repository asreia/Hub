/*
Static 変数
    scriptCompilationFailed  //ログにコンパイルエラーメッセージがある場合はTrue。
Static 関数
    オブジェクト
        ●InstanceIDToObject	インスタンス ID をオブジェクト参照へと変換します
        ●GetObjectEnabled	オブジェクトが有効かどうか（ 0 は無効、1 は有効、-1 は有効/無効状態が存在しません）
        ●SetObjectEnabled	オブジェクトの無効/有効を設定します
        ●IsPersistent	オブジェクトがディスク上に保存されているかを確認します
        ゲームオブジェクト
            CreateGameObjectWithHideFlags	HideFlags と特定のコンポーネントをアタッチしたゲームオブジェクトを作成します
    アセット
        ゲームオブジェクトとアセット
            ●CollectDeepHierarchy	GameObjectを渡すと下の階層のGameObjectまでの全てのGameObjectとそのComponentを取得する(GameObjectのソートは階層順ではないみたい)
            ●CollectDependencies	    GameObjectを渡すと下の階層のGameObjectまでの全てのGameObjectとそのComponentとその参照されているアセットを取得する(並びはぐちゃぐちゃ)
                //Assets/下でも大体同じ挙動
        ●UnloadUnusedAssetsImmediate	使用していないアセットをアンロードします
    ダーティー
        ●SetDirty	target のオブジェクトをダーティとしてマークします(シーンに含まれないオブジェクトだけに使うのが適切です)。
        ●ClearDirty	ターゲットのダーティフラグをクリアします。
        ●IsDirty	指定されたオブジェクトが最後に保存されてから変更されたかどうかを示すブール値を取得します。
        GetDirtyCount	指定されたオブジェクトのシリアル化されたプロパティが変更された回数を示す整数を返します。
    シリアライズ
        ●CopySerialized	Object のすべての設定をコピーします
        ●CopySerializedIfDifferent	差異がある場合は、Object のすべての設定を 2番目の Object にコピーします
        ●CopySerializedManagedFieldsOnly	シリアル化可能なフィールドを1つのマネージドオブジェクトから別のマネージドオブジェクトにコピーします。 object to another.
    アセンブリ
        RequestScriptReload	Unity Editorは、次のフレームでスクリプトアセンブリを非同期にリロードします。これにより、すべてのスクリプトの状態がリセットされますが、Unityは前回のコンパイル以降に変更されたコードをコンパイルしません。
            //スクリプトが再コンパイルした。
    ダイアログ
        ●DisplayPopupMenu	ポップアップメニューを表示します。
        モーダルダイアログ
            ●DisplayDialog	このメソッドは、モーダルダイアログを表示します。
            ●DisplayDialogComplex	3 つボタンのモーダルダイアログを表示します
            ●GetDialogOptOutDecision	このメソッドは、ユーザーが現在のダイアログボックスの表示を再度オプトアウトできるようにするモーダルダイアログを表示します。
            ●SetDialogOptOutDecision	このメソッドは、ユーザーが現在のダイアログボックスの表示を再度オプトアウトできるようにするモーダルダイアログを表示します。
                //上の２つは謎
        パス取得ダイアログ
            ●OpenFilePanel	"open file"ダイアログを表示し、選択されたパスを取得します
            ●OpenFilePanelWithFilters	"open file"ダイアログを表示し、選択されたパスを取得します
            ●OpenFolderPanel	"open folder"ダイアログを表示し、選択されたパスを取得します
            ●SaveFilePanel	"save file"ダイアログを表示し、選択されたパスを取得します
            ●SaveFilePanelInProject	プロジェクトのアセットフォルダーから始まる"save file"ダイアログを表示し、選択したファイルのパス名を取得します
            ●SaveFolderPanel	プロジェクトのアセットフォルダーから始まる"save folder"ダイアログを表示し、選択したファイルのパス名を取得します
        プログレスバー //うまく表示されない。スクリプトデバッグでちょっと出る
            ●ClearProgressBar	EditorUtility.DisplayProgressBar で表示されているプログレスバーを削除します
            ●DisplayCancelableProgressBar	キャンセルボタンのあるプログレスバーを表示します
            ●DisplayProgressBar	プログレスバーを表示/更新します
    ウィンドウ
        ●FocusProjectWindow	全面にプロジェクトウィンドウを表示し、フォーカスを当てます。
    カメラ
        SetCameraAnimateMaterials	エディターでマテリアルのアニメーションを許可するようにこのカメラを設定します。
        SetCameraAnimateMaterialsTime	このカメラがレンダリング時に使用するグローバル時間を設定します。
            //カメラごとにエディタ上でシェーダーを動かせる? 
    テクスチャー //エディタ上で圧縮する方法?
        CompressCubemapTexture	キューブマップテクスチャを圧縮します。
            //EditorUtility.CompressCubemapTexture(texture, TextureFormat.RGB24, TextureCompressionQuality.Normal);
        CompressTexture	テクスチャを圧縮します
            //EditorUtility.CompressTexture(texture, TextureFormat.RGB24, TextureCompressionQuality.Normal);
    レンダラー
        SetSelectedRenderState	このレンダラーのシーンビューで選択した表示モードを設定します。
            //引数にEditorSelectedRenderState.Wireframeというstateがある
        UpdateGlobalShaderProperties	レンダリング時に使用するグローバルシェーダープロパティを更新します。
            //引数は時間?
    その他
        FormatBytes	バイト数をフォーマットされた状態の文字列として取得します
            //print(EditorUtility.FormatBytes(2048));// prints "2.0 KB"
        NaturalCompare	アルファベットソートのようなもの
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class EditorUtilityTest{
    static Texture2D tex0;
    static Texture2D tex1;
    static GameObject capsule;
    static CapsuleCollider cc;
    [MenuItem("EditorUtilityTest/Test")]
    static void Test(){
        UnityEngine.Object eObj = new UnityEngine.Object();
        if(eObj == null) Debug.Log("IsNull");
        object Nobj = new object();

        tex0 = new Texture2D(128, 128);
        tex1 = new Texture2D(64, 64);
        EditorUtility.CopySerialized(tex0, tex1); 
        // EditorUtility.CopySerializedIfDifferent(tex0, tex1);
        // EditorUtility.CopySerializedManagedFieldsOnly(tex0, tex1);
        Debug.Log($"{tex1.height}, {tex1.width}");

        /* //'M' is missing the class attribute 'ExtensionOfNativeClass'!
                //という実行できるけど謎のエラーがでるのでコメントアウト
        // M m0 = ScriptableObject.CreateInstance<M>();
        // M m1 = ScriptableObject.CreateInstance<M>();
        M m0 = new M();
        M m1 = new M();
        m0.mInt = 1; m0.inner.innerInt = 11;
        EditorUtility.CopySerializedManagedFieldsOnly(m0, m1);
            //集約されていない外側がUnityEngine.Objectを継承していないが
            //シリアル化されているのか?謎
        Debug.Log($"m1.mInt: {m1.mInt}, m1.inner.innerInt: {m1.inner.innerInt}");
            //=>1, 11 となり成功するがシリアライズ経由か分からない
        // SerializedObject sm0 = new SerializedObject(m0); 
            //SerializedObjectはUnityEngine.Objectのみ生成可能
        */
        // Object.Destroy(tex0);//たしかC++側が開放されるらいい?
        // tex0 = null;//C#マネージのGC待ち


        AssetDatabase.CreateAsset(tex0, "Assets/EditorUtility/tex0.asset");
        capsule = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EditorUtility/capsule.prefab");
        cc = capsule.GetComponent<CapsuleCollider>();
    }
    static bool toggle = false;
    [MenuItem("EditorUtilityTest/EnableToggle")]
    static void EnableToggle(){
        toggle = !toggle;
        EditorUtility.SetObjectEnabled(tex0, toggle);
        EditorUtility.SetObjectEnabled(tex1, toggle);
        EditorUtility.SetObjectEnabled(capsule, toggle);
        EditorUtility.SetObjectEnabled(cc, toggle);
        Debug.Log($"[tex0] GetObjectEnabled: {EditorUtility.GetObjectEnabled(tex0)}");
        Debug.Log($"[tex1] GetObjectEnabled: {EditorUtility.GetObjectEnabled(tex1)}");
        Debug.Log($"[capsule] GetObjectEnabled: {EditorUtility.GetObjectEnabled(capsule)}");
        Debug.Log($"[cc] GetObjectEnabled: {EditorUtility.GetObjectEnabled(cc)}");
    }
    [MenuItem("EditorUtilityTest/ProgressBar")]
    static void ProgressBar(){
        EditorUtility.ClearProgressBar();
        float cnt = 0;
        void Callback(){
            EditorUtility.DisplayProgressBar("DisplayProgressBar", "info", cnt);
            cnt += 0.01f; //ブレークで止めると出る
            if(cnt > 100) EditorApplication.update -= Callback; 
        }
        EditorApplication.update += Callback;
    }
    [MenuItem("EditorUtilityTest/ModalDialogs")]
    static void ModalDialogs(){
        EditorUtility.FocusProjectWindow();
        // EditorUtility.DisplayPopupMenu(new Rect(200, 200, 200, 200), "Main/Sub/abc", new MenuCommand(tex0));
        Debug.Log(EditorUtility.DisplayDialog("DisplayDialog", "メッセージ", "おけ", "脳"));
        EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, "key", true);
        Debug.Log(EditorUtility.DisplayDialogComplex("DisplayDialogComplex", "メッセージ", "おけ", "脳", "alt?"));
        Debug.Log(EditorUtility.GetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, "key"));
        Debug.Log(EditorUtility.OpenFilePanel("OpenFilePanel", "C:/Users/asreia/プロファイラー2020_1/Assets/EditorUtility", "meta,asset"));
        Debug.Log(EditorUtility.SaveFilePanel("SaveFilePanel", "C:/Users/asreia/プロファイラー2020_1/Assets/EditorUtility", "デフォルトネーム", "meta"));
        Debug.Log(EditorUtility.SaveFilePanelInProject("SaveFilePanelInProject", "デフォルトネーム", "meta", "メッセージ?"));
        Debug.Log(EditorUtility.OpenFilePanelWithFilters("OpenFilePanelWithFilters", "C:/Users/asreia/プロファイラー2020_1/Assets", new string[]{"Image files", "png,jpg", "Script files", "cs"}));
        Debug.Log(EditorUtility.OpenFolderPanel("OpenFolderPanel", "C:/Users/asreia/プロファイラー2020_1/Assets", "デフォルトネーム"));
        Debug.Log(EditorUtility.SaveFolderPanel("SaveFolderPanel", "C:/Users/asreia/プロファイラー2020_1/Assets", "デフォルトネーム"));
        EditorUtility.DisplayProgressBar("DisplayProgressBar", "info", 50.43f);
    }
    [MenuItem("EditorUtilityTest/RequestScriptReload")]
    static void RequestScriptReload(){
        EditorUtility.RequestScriptReload();
        Debug.Log($"scriptCompilationFailed: {EditorUtility.scriptCompilationFailed}");
    }
    [MenuItem("EditorUtilityTest/UnloadUnusedAssetsImmediate")]
    static void UnloadUnusedAssetsImmediate(){
        EditorUtility.UnloadUnusedAssetsImmediate();
    }
    [MenuItem("EditorUtilityTest/CollectDependencies")]
    static void CollectDependencies(){
        Debug.Log("==collectDependencies==");
        // Object[] ao = EditorUtility.CollectDependencies(new Object[]{GameObject.Find("CubeT")});
        Object[] ao = EditorUtility.CollectDependencies(new Object[]{AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EditorUtility/Capsule.prefab")});
        // Object[] ao = EditorUtility.CollectDependencies(new Object[]{AssetDatabase.LoadAssetAtPath<Object>("Assets/EditorUtility/NewAudioMixer.mixer")});

        foreach(Object obj in ao){
            Debug.Log(obj);
        }
    }
    [MenuItem("EditorUtilityTest/CollectDeepHierarchy")]
    static void CollectDeepHierarchy(){
        Debug.Log("==CollectDeepHierarchy==");
        GameObject go = GameObject.Find("CubeT");
        // Object[] child = EditorUtility.CollectDeepHierarchy(new Object[]{go});
        Object[] child = EditorUtility.CollectDeepHierarchy(new Object[]{AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EditorUtility/Capsule.prefab")});
        // Object[] child = EditorUtility.CollectDeepHierarchy(new Object[]{AssetDatabase.LoadAssetAtPath<Object>("Assets/EditorUtility/NewAudioMixer.mixer")});
        foreach(Object obj in child){
                Debug.Log($"child: {obj}");
        }
    }
    [MenuItem("EditorUtilityTest/CreateGameObjectWithHideFlags")]
    static void CreateGameObjectWithHideFlags(){
        GameObject go = EditorUtility.CreateGameObjectWithHideFlags("C_GOwHF", HideFlags.HideInInspector, typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider));
        Debug.Log($"name: {go.name}, hideFlags: {go.hideFlags}");
        foreach(Object obj in EditorUtility.CollectDeepHierarchy(new Object[]{go})){
            Debug.Log(obj);
        }
    }
    [MenuItem("EditorUtilityTest/ObjInfo")]
    static void ObjInfo(){
        Debug.Log($"[tex0] InstanceIDToObject: {EditorUtility.InstanceIDToObject(tex0.GetInstanceID())}, GetObjectEnabled: {EditorUtility.GetObjectEnabled(tex0)}, IsPersistent: {EditorUtility.IsPersistent(tex0)}");
        Debug.Log($"[tex1] InstanceIDToObject: {EditorUtility.InstanceIDToObject(tex1.GetInstanceID())}, GetObjectEnabled: {EditorUtility.GetObjectEnabled(tex1)}, IsPersistent: {EditorUtility.IsPersistent(tex1)}");
        Debug.Log($"[capsule] InstanceIDToObject: {EditorUtility.InstanceIDToObject(capsule.GetInstanceID())}, GetObjectEnabled: {EditorUtility.GetObjectEnabled(capsule)}, IsPersistent: {EditorUtility.IsPersistent(capsule)}");
        Debug.Log($"[cc] InstanceIDToObject: {EditorUtility.InstanceIDToObject(cc.GetInstanceID())}, GetObjectEnabled: {EditorUtility.GetObjectEnabled(cc)}, IsPersistent: {EditorUtility.IsPersistent(cc)}");

    }
    [MenuItem("EditorUtilityTest/DirtyTest/ObjMod")]
    static void ObjMod(){
        tex0.anisoLevel++;
        Debug.Log($"[ObjMod]anisoLavel: {tex0.anisoLevel}, IsDirty: {EditorUtility.IsDirty(tex0)}, GetDirtyCount: {EditorUtility.GetDirtyCount(tex0)}");
    }
    [MenuItem("EditorUtilityTest/DirtyTest/SetDirty")]
    static void SetDirty(){
        EditorUtility.SetDirty(tex0);
        Debug.Log($"[SetDirty] IsDirty: {EditorUtility.IsDirty(tex0)}, GetDirtyCount: {EditorUtility.GetDirtyCount(tex0)}");
    }
    [MenuItem("EditorUtilityTest/DirtyTest/ClearDirty")]
    static void ClearDirty(){
        EditorUtility.ClearDirty(tex0);
        Debug.Log($"[ClearDirty] IsDirty: {EditorUtility.IsDirty(tex0)}, GetDirtyCount: {EditorUtility.GetDirtyCount(tex0)}");
    }
    [MenuItem("EditorUtilityTest/DirtyTest/LogOnly")]
    static void LogOnly(){
        Debug.Log($"[LogOnly] IsDirty: {EditorUtility.IsDirty(tex0)}, GetDirtyCount: {EditorUtility.GetDirtyCount(tex0)}");
    }
    [MenuItem("EditorUtilityTest/DirtyTest/SaveAssets")]
    static void SaveAssets(){
        AssetDatabase.SaveAssets();
        // AssetDatabase.Refresh();//RefreshはSaveAssetsしないみたい?
        Debug.Log($"[SaveAssets] IsDirty: {EditorUtility.IsDirty(tex0)}, GetDirtyCount: {EditorUtility.GetDirtyCount(tex0)}");
    }
}


// [System.Serializable]
// public class M /*: ScriptableObject */{
//     [System.Serializable]
//     public class Inner{
//         public int innerInt = 0;
//     }
//     public Inner inner = new Inner();
//     public int mInt = 0;
// }