
●←検索
- EditorWindowのAsset化できるか(シリアライザによるフィールド保存できるか)
    CreateAssetでエラーでる。閉じる(x)とUnityEngine.ObjectがDestroyし、assetファイルが消える。ググったが情報なし。別でScriptableObjectを持つのが普通?https://qiita.com/r-ngtm/items/173918642cc91d816af1
    C:\Users\asreia\プロファイラー2020_1\Assets\ObjectFamily\ObjectTest\EditorWindowTest\EditorWindowAsset.cs
- MonoBehaviour,ScriptableObject(UnityEngin.Objectを継承したもの?)はファイル名とクラス名が一致しないとだめ
- MonoBehaviourはnewすると警告がでるがビルドは通る。多分、普通のクラス扱い?  
- ScriptableObjectはフィールドとコンストラクタでnewできない。newよりScriptableObject.CreateInstance<~>()を推奨する(classの様に実行時、動的生成可能?)
    ScriptableObjectはシリアライズされた情報を保存できるがClassはできない(Classは[SerializeField], [Serializable]をつけてUnityObjectのScripまたはMonoBに保存してもらう?)
    シリアライザはリロード後に素早くUnityObject間を繋ぎ合わせる?NonSerializedな部分はリロード消える?
- UnityEngine.ObjectはSceneかProjectかどちらにも属していないかになるたぶん
- GUIDToAssetPath //↑の逆射(つまりUnityEditor上で交換可能(AssetPathとGUIDは同型))
- 
- PrefabはランタイムではSceneにベイクされてランタイムではPrefabというのは存在しない?
    Prefabの階層のルートの位置と回転はPrefabに保存されない
    パラメータ変更時|が青くなっている部分はまだPrefabに保存されていなくSceneに保存されている
    文字が青い所はPrefabに保存されている
    Prefabの子はGameObjectの順序変更と削除、Componentの順序変更ができない(追跡しているからと言っているが本当にそうなのか?)
- ScriptableObjectアセットはシングルトンのようにも見える?
- Hierarchy(UnityC#?) -> Scene(Struct) -> GameObject(Class) -> Component(Class) -> Asset(Class)
    ↑のようにSceneのUnloadとかでC#のルートからも外れてC#のGCの対象になる?
- SceneのLoad,UnloadはGameObjectとComponent(ルート)をLoad,UnloadしてUnload.singleの時UnloadUnusedAssetsもする?
    - GameObjectやComponentへのDestroyはScene(ルート)からの参照をC#的に切れる?のでそのGameObjectやComponentが参照しているAssetやCompがそれ以外の他に参照されてなければ
        UnloadUnusedAssetsの対象に多分なる(●UnloadUnusedAssetsはC#のGCの対象になるようなObjectを[Destroy]する?)
        GameObjectを明示的[Destroy]すると中のCompも[Destroy]されるが、C#の参照が残っていればC#側のみアクセスできるし、それ以下がUnloadUnusedAssetsの対象にならない
            ●SceneのUnloadはその中の全GameObjectを明示的[Destroy]をするだけ?(中のCompも自動的に[Destroy])
- Scene構造体はGetRootGameObjectsInternal(handle, rootGameObjects);で外部からルートGameObjectを取ってくるのでC#的にルートGameObjectへの参照を持っている訳ではない
"Scene→○": Sceneの先の何か?    [C#(C++)]    ->:C#参照    Destroy([C#(C++)]) → [C#]    __:何もなし
==Destroy([(Game)]);===========================//参照関係も自身も丸ごと消えるのでGameObjectへの参照は気にしなくてもいい?
"Scene→○" -> [(Game)] <-(C++)-> [(Comp)] -> [(Asset)]
Destroy([(Game)]);
"Scene→○"                       [Comp] -> [(Asset)]    //[Socket] OnDestroyされていたので多分そう
[Comp] -> [(Asset)]    //staticGameObject = null;をしなくてもUnloadUnusedAssetsの対象になってしまったので多分そう
C#GC
(Asset)
UnloadUnusedAssets
__
==Destroy([(Comp)]);===========================
"Scene→○" -> [(Game)] <-(C++)-> [(Comp)] -> [(Asset)]
Destroy([(Comp)]);
"Scene→○" -> [(Game)]           [Comp] -> [(Asset)]
[Comp] -> [(Asset)]
C#GC
(Asset)
UnloadUnusedAssets
__
==Destroy([(Asset)]);===========================
"Scene→○" -> [(Game)] <-(C++)-> [(Comp)] -> [(Asset0)] -> [(Asset1)]    //"-> [(Asset1)]"の部分は予想
Destroy([(Asset)]);
"Scene→○" -> [(Game)] <-(C++)-> [(Comp)] -> [Asset0] -> [(Asset1)]   //Inspectorで消失 //[(ScrObj0)]を[Destroy]するとC++側が破棄されるだけでC#側は残る

//[Destroy]は消さるものは局所的に消しているという印象(C#側とそれが参照しているものは消せない)

- addされたSceneをUnloadする場合、そのSceneの要素への参照にNullを入れ、Unloadし、UnloadUnusedAssetsを呼ぶ?
- 
- ●アンロードのされ方(C++側とC#側)とプレイモード実行時(ドメイン¦シーンリロード)、シリアライズとカスタムエディター¦プロパティ、アセンブリdll、アセット、Editor_GUI_Layout(UIElement)、[属性]
- 
- M m0 = new M();
    // SerializedObject sm0 = new SerializedObject(m0); 
    //SerializedObjectはUnityEngine.Objectのみ生成可能
- UnityEngine.Objectを継承したシリアライズ可能なクラスのコンストラクタではUnityAPIを呼び出せない多分
    https://docs.unity3d.com/ja/current/Manual/script-Serialization-Errors.html
- so.ApplyModifiedPropertiesでUnityEngine.Objectと同じUndo登録している?  
-  EditorGUILayout.IntSlider、SerializedProperty版とプリミティブ版がある  
-  Undoはunityのあらゆる操作に対する差分を一つのスタックに積んでいる?  
-  アセンブリ定義ファイルはEditorフォルダより強くアセンブリ定義ファイルに従ってEditorフォルダも同じアセンブリにコンパイルされる
   -  アセンブリ定義ファイルの任意のプラットフォームは上のチェックが入っているとコード全体に、下のチエック入れた所が#if !Platform_0 && !Platform_1..(除外)されて、
      上のチェックが入っていないと下のチェック入れた所が#if Platform_0 || Platform_1..(含む)される?
      上のチェックが入っていなくても入っていても下のチェックが全て入っていない時#if(プリプロセッサ)が現れないので全て通る?
      上も下も全て入っていない状態で適用すると上が勝手に入る現象がある
      このアセンブリが実行されるPlatformは、任意のPlatformを[含み/含まず]、あるPlatformを[除外し/含め]たものです。?
- 該当するスクリプトをロードできませんはアセンブリ定義ファイルを作り直すとたまに直る
- UnityEngine.Object{unmanaged型* C++メモリへのポインタ} がある? (恐らくIntPtr   m_CachedPtr;) 
    file:///C:/Google/gitUnityCsRef2019.4.0f1/Doxygen_HTMLOut/Full/html/d0/d91/class_unity_engine_1_1_object.html#a885526bf994dc449c134f86fa20fc8f8
    https://docs.unity3d.com/ja/2018.4/ScriptReference/MemoryProfiler.PackedNativeUnityEngineObject.html
    Object.Destroyはそのptrの先を開放する? [nullなのにnullじゃない](https://qiita.com/satanabe1@github/items/e896303859be5d42c188)
- hideflagをCsRefFullでみる?
- [CustomEditor(typeof(Cus))] class CusIns : Editor{}
    属性はそのクラスのオブジェクトをtargetにそれとver serializeobject = new SerializeObject(target)している?
    このクラスはInspectorWindow?が生成し保持している??
- カスタムPropertyDrawerはPropertyField時に参照される?
- SaveAssetは内部キャッシュSerializedObjectを.metaに書き出す?

- アンロードとデストロイは同じDestroyを呼ぶ?
    sceneアンロード、スコープ外のGCからのアンロード、明示的DestroyはすべてDestroyを呼ぶ?その前に明示的いがいはOnDisableを呼ぶ?      //明示的Destroy時にScene Memoryとかから外れる
- シリアライザはエディター機能                                                                                                //Scene Memory,Assets,Not Savedは多分C++側のみ
- UnityはC++エンジンであり、C#はUnityAPI(UnityEditor, UnityEngine)を通じてC++エンジンへアクセスしている?
- UnityEngine.Objectはランタイムではヒエラルキー全体(Scene)と静的変数(とスタック)エディタではエディタ(エディタウィンドウ?Projectも?)がルートになりルートに繋がっていないObjectは
    UnityGC(またはUnloadUnusedAssets＠❰Immediate❱を呼ぶ)されて、その後Destroyが呼ばれてObjectの中のC++リソース(多分IntPtr   m_CachedPtr;の先)が完全に破棄される
    しかし、HideFlags.DontUnloadUnusedAssetが設定されたObjectはNotSavedになり破棄されないhttps://www.f-sp.com/entry/2016/09/03/140006
    UnloadUnusedAssets＠❰Immediate❱系はUnityのGC.Collect()?https://youtu.be/NawYyrB_5No?list=PLtjAIRnny3h7KDDpkrsEEnILtEQLwOHiC&t=1162
        //Scene Memoryの中のさらにそれ以外のルートに参照されていない部分がUnityGC、UnloadUnusedAssets対象?
        //●Objectのアンロードの種類は、全Sceneのアンロードによる[Destroy]WorldObjectsか、UnityGCか、UnloadUnusedAssetsか、明示的[Destroy]?
- UnloadUnusedAssets＠❰Immediate❱は[OnDestroy]が呼ばれない見たい..後は予想通り
- ●MonoB,Component,GameObjectはSceneまたはPrefabにそのままシリアライズされるが、
    ScriptableObjectは独自のアセットとしてシリアライズされる                                                                   //ScriptableObjectはSceneとは独立している
    Prefabで実行時に変更して実行が終わると戻るがその変更は多分SceneのオーバーライドなのでPrefab自体の変更ではないため戻る?          //PrefabはSceneへコピーされて扱われる
- エディタ実行時はSceneをバックアップしバックアップしたSceneを実行するため実行が終わると元のSceneに戻り変更が元に戻ってしまう?

C:\Users\asreia\プロファイラー2020_1\Assets\ObjectFamily\ObjectTest\ResourceTest.cs (Assets/EditorAssembly.asmdefを作り直す必要あるかも)
| Message/Settings  |      なし      |  Asset   |     DS      |  Asset & DS  |    Comp(#3)    |  MonoB(Soket)  |  Asset & Comp  |  DS & Comp  |  Asset & DS & Comp  |
| :---------------- | :------------: | :------: | :---------: | :----------: | :------------: | :------------: | :------------: | :---------: | :-----------------: |
|                   | (Scene Memory) | (Assets) | (Not Saved) | (Not Saved)  | (Scene Memory) | (Scene Memory) |    (Assets)    | (Not Saved) |     (Not Saved)     |
| ==Build========== |  ===========   | =======  |   ======    | ============ |    ========    |  ============  | ============== | =========== | =================== |
| All               |       -        |    -     |      -      |      -       |       -        |      ctor      |       -        |      -      |          -          |
| ==Create->Build== | ==[リロード]=  | =======  |   ======    | ============ |     ======     |  ============  | ============== | =========== | =================== |
| Constructor       |       2        |    2     |      2      |      2       |       2        |                |       2        |      2      |          2          |
| Awake             |       -        |    -     |      -      |      -       |       -        |                |       -        |      -      |          -          |
| OnEnable          |       3        |    3     |      3      |      3       |       3        |      なし      |       3        |      3      |          3          |
| OnDisable         |       1        |    1     |      1      |      1       |       1        |                |       1        |      1      |          1          |
| OnDestroy         |       -        |    -     |      -      |      -       |       -        |                |       -        |      -      |          -          |
| ==CreateInstance= |  ==[生成]===   | =======  |   ======    | ============ |    ========    |  ============  | ============== | =========== | =================== |
| Constructor       |     1(#1)      |    1     |      1      |      1       |       1        |                |       1        |      1      |          1          |
| Awake             |       2        |    2     |      2      |      2       |       2        |                |       2        |      2      |          2          |
| OnEnable          |       3        |    3     |      3      |      3       |       3        |      なし      |       3        |      3      |          3          |
| OnDisable         |       -        |    -     |             |              |                |                |                |             |                     |
| OnDestroy         |       -        |    -     |             |              |                |                |                |             |                     |
| ==StartPlayMode== | ==[リロード]+  | =======  |   ======    | ============ |     =(#4)=     |  ============  | ============== | ==CompNull= | =================== |
| Constructor       |     2(#1)      |    2     |      2      |      2       |    2,6(#2)     |      1,2       |       2        |      2      |          2          |
| Awake             |       -        |    -     |             |              |       7        |       3        |                |             |                     |
| OnEnable          |       3        |    3     |      3      |      3       |      3,8       |       4        |       3        |      3      |          3          |
| OnDisable         |      1,4       |    1     |      1      |      1       |      1,4       |       -        |       1        |      1      |          1          |
| OnDestroy         |       5        |    -     |             |              |       5        |       -        |                |             |                     |
| ==EndPlayMode==== |  ===========   | =======  |   ======    | ============ |     ======     |  ============  | ============== | =========== | =================== |
| Constructor       |       -        |    -     |      -      |      -       |     3(#2)      |       3        |       -        |      -      |          -          |
| Awake             |       -        |    -     |      -      |      -       |       4        |       -        |       -        |      -      |          -          |
| OnEnable          |       -        |    -     |      -      |      -       |       5        |       -        |       -        |      -      |          -          |
| OnDisable         |       -        |    1     |      -      |      -       |       1        |       1        |       -        |      -      |          -          |
| OnDestroy         |       -        |    -     |      -      |      -       |       2        |       2        |       -        |      -      |          -          |
(#1)Constructorが2回あるのはC#ドメインのリロード?
    1回目と2回目のIDが同じなのはC#ヒープがリロード時に消えてないから?
    //C#ヒープは多分消えてるInstanceIDはC++側にあるらしい(多分、C++への参照もデシリアライズ?)
(#2)InvalidOperationException：EnsureRunningOnMainThreadはメインスレッドからのみ呼び出すことができます
    UnityEngine.Object.GetInstanceID()で例外
(#3)CompはObjectが存在する時、どの場面でも[生成]で新しく作り直している
●(パターン)[生成]:ctor→Awake→Enable, [リロード]:Disable→ctor→Enable, [廃棄]:Disable→Destroy ([廃棄]以外はOnEnableで終わる)
(#4)[リロード]→[廃棄]→[生成]
[廃棄]→[生成]でもIDが同じ。[廃棄]はC++メモリの廃棄でC#ヒープはそのまま使っている?
    ドメインのリロードだからC#ヒープも消えてるはず..
        デシリアライズ?でフィールドを復元しているがIDまで同じ..IDもデシリアライズ?
[生成]の時のみAwakeが呼ばれる。[廃棄]の時のみOnDestroyが呼ばれる。
[Awake]は生成されるとき(ctorの代わり?)。[Enable]はそのObjectのUnity管理下のメンバ?が外部から使えるとき。[Disable]は使えなくなるとき。[Destroy]は廃棄されるとき(Disposeの代わり?) 多分..
    [Destroy]されても自分で作ったフィールドは消えていないし参照できる(Unity管理下やC++メモリが消える?) https://youtu.be/NawYyrB_5No?list=WL&t=1191
●まとめ ドメインをリロードし、オリジナルのSceneをバックアップして、
        Sceneと何処にも参照されていないUnityEngine.Object(多分Scene Memory)を全て[Destroy]する(多分DestroyWorldObjectsメソッド?)。AssetとDSは残る(AssetsとNot Saved)
        そして、バックアップしたSceneをロードする。(憶測:終了時、またDestroyWorldObjectsしてオリジナルのSceneをロードする?(これがSceneへの変更が戻る原因))
                                                (LoadSceneもほぼ↑と同じ?)(DestroyWorldObjectsは全SceneUnload→UnloadUnusedAssets?)
SerializeはEditor機能。ランタイムではシーンロード時にディスクからデシリアライズされるのみ?
https://docs.unity3d.com/2020.1/Documentation/Manual/ConfigurableEnterPlayModeDetails.html
- Enter Play Mode ScriptableObject と MonoBehaviour
  - ドメインリロード [Disable]
    - シーンバックアップ
    - シリアライズ
    - ドメインアンロード
      - GC, finalize を呼ぶ
      - スレッドを終了
      - 全てのJit情報削除
    - ドメインロード
      - アセンブリをロード(System,Unity,User(.dll?))
    - デシリアライズ [ctor] [Enable] [Disable]
  - シーンリロード MonoBehaviour?
    - シーンのオブジェクトを破棄 [Destroy]
    - バックアップシーンをロード [ctor] [Awake] [Enable]
●要約: スクリプトコンパイル -> シーンバックアップ -> ドメインリロード -> デストロイ -> バックアップシーンロード
[MonoBとSOの違い](https://www.youtube.com/watch?v=NawYyrB_5No&list=PLtjAIRnny3h7KDDpkrsEEnILtEQLwOHiC&index=1)
    MonoBehaviourやComponentはSceneやPrefabに保存する。                         //PrefabはAssetsに属す?
    ScriptableObjectはディスクに保存しないか、独自アセットとして保存する。
```C#  
public class ExampleAsset : ScriptableObject{
    [SerializeField, Range (0, 10)]
    int number;

    [MenuItem ("Example/Create ExampleAsset")]
    static void CreateExampleAsset (){
        //new を使用してインスタンス化はしてはいけません。理由は MonoBehaviour と同じで、Unity のシリアライズ機構経由でオブジェクトを作成する必要があるからです。
        var exampleAsset = CreateInstance<ExampleAsset> ();//メモリ内生成
        // 必ず .asset でなくてはいけません。他の拡張子にしてしまうと、Unity が ScriptableObject 派生のアセットとして認識しません。//ディスク書き出し
        AssetDatabase.CreateAsset (exampleAsset, "Assets/Editor/ExampleAsset.asset");
        AssetDatabase.Refresh ();
        //ロードはAssetDatabase.LoadAssetAtPath
    }
}
//or//
[CreateAssetMenu(menuName = "Example/Create ExampleAsset Instance")]
public class ExampleAsset : ScriptableObject{}
```

- AssetDatabase.LoadAssetAtPassやEditorGUIUtility.Loadでロードするときは指定のフォルダの相対パスから拡張子までいれる。  
- Unity2/属性  
   [ExecuteInEditMode](廃止) -> [ExecuteAlways]?  
  - [MenuItem("Test/proc")], [ContextMenuItem("ラベル", "関数")](変数につける)  
    [ContextMenu ("RandomNumber")](コンポーネントにつく)
    [SelectionBase]はそれが付いた一番近い親を選択する  
- EditorPrefs で保存すべきものは ウィンドウの位置・サイズ、 Unity エディターの環境設定
    メジャーバージョン(今は無い?)ごとに分けて保存されます。特に Windows はレジストリに値を保存
    EditorPrefs.SetInt("_CNT_", cnt++);
- EditorUserSettings.Set/GetConfigValue
    暗号化してプロジェクト内のみ影響します。データの保存先がLibrary/EditorUserSettings.asset  
    EditorUserSettings.SetConfigValue ("Data 1", "text");  
- SerializedObject  
    .ApplyModifiedProperties //変更を内部キャッシュに適用  
    .ApplyModifiedPropertiesWithoutUndo //Undoなし版?  
    .Update //内部キャッシュから更新  
    .hasModifiedProperties //まだ適用していない変更があるか  
    .isEditingMultipleObjects //複数編集かのうか?  
    .FindProperty //propertyを検索しSerializedPropertyを返す  
    .GetIterator //一番最初のpropertyを返す?  
    .targetObject //このSerializedObjectを生成したobject  
    .targetObjects //object配列を引数に生成した時の配列  
    - SerializedProperty  
        .depth //そのpropertyがある入れ子の深さ  
        .hasChildren //そのpropertyは入れ子の子を持つか  
        .hasMultipleDifferentValues //複数編集で複数のpropertyは全て同じではないか  
        .serializedObject //このpropertyを生成したserializedObject  
        .FindPropertyRelative //FindPropertyのproperty版  
    SerializedObjectはSerializedPropertyを持っている  
    SerializedObject は、Unity 上で扱うすべてのオブジェクトに関係しています。普段扱っているアセット  
    (マテリアルやテクスチャ、アニメーションクリップ等）も SerializedObject がなくては作成できません。(Serialize情報はC++側にある)  
    -（UnityEngine.Object）は SerializedObject に変換されて扱われています。  
    インスペクターでコンポーネントの値を編集している時も、Component のインスタンスを  
    編集しているわけではなく、SerializedObject のインスタンスを編集していることになります。  
    ScriptableObject,MaterialなどのUnityEngine.Objectから複数のSerializedObjectを生成できる。  
        複数あるがファイルと通信できるのは内部にキャッシュされた一つのSerializeObject?  
        new SerializeObject(UnityEngine.Object obj)は内部にキャッシュされた  
        objのSerializeObjectを複製する?  
        ↑↑予想、UnityEngine.ObjectのInstanceID?をキーに内部にキャッシュされたSerializedObjectを返す?
    一つのUnityEngine.Objectと複数のSerializeObjectと同期するための２つの関数がある↓  
    SerializedObject -> 内部キャッシュSerializedObject : ApplyModifiedProperties() //(Serialize -> Deserialize -> Serialize)  
    内部キャッシュSerializedObject -> SerializedObject : Updata() //(Serialize)  
        内部キャッシュからのはずだがUnityEngine.Objectから(へ)反映されている?(内部キャッシュ⇔Objectが常に同じに保たれる?) //多分↑のSerialize,Deserializeされているから  

    SerializedObject のコンストラクタで配列を渡すだけで複数の UnityEngine.Object を扱うことができます。
    ただし、引数として渡せるものは同じ型のみです。もし異なる型のオブジェクトを引数として渡した場合、キー
    マップが一致せずにエラーが発生します。
    ```C#
    //複数のリジッドボディ
    Rigidbody[] rigidbodies = /* さまざまな方法で Rigidbody コンポーネントを取得 */;
    var serializedObject = new SerializedObject(rigidbodies);
    //オブジェクトの同時編集?
    serializedObject.FindProperty ("m_UseGravity").boolValue = true;
    ```
    SerializedObjectはUnityEngine.Objectを複数もてるし、  
    UnityEngine.Objectを複数のSerializeObjectで持てる  

    InternalEditorUtility.SaveToSerializedFileAndForget で UnityEngine.Object をアセットとして保存できます。  

    (↓最初から再帰してるとserializeされない)
    class Recur{Recur recur;} class Cls{Recur recur;} のようなクラスの再帰でも構造体のように再帰的(7回)にシリアライズされnullはサポートされず多分nullの部分はnewで生成され中のフィールドはdefaultで入る?  
    あと、UnityEngine.Objectを継承していないクラスはポリモーフィズムが効かず全て基底クラスになる  
        (public Animal[] animals に Dog、Cat、Giraffe のインスタンスを加えると、シリアル化後に 3 つの Animal のインスタンスができます。)  
    
    シリアル化条件
    できる: public, [SerializeField], [Serializable]をもつカスタムクラス¦構造体, UnityEngine.Objectファミリア, プリミティブ型(int,stringとか), Enum型, Unityビルトイン型(Vector2,Rectとか), 配列, List<T>  
    できない: static, const, readonly, 抽象¦ジェネリッククラス  

     プレハブ は 1 つまたは複数の ゲームオブジェクト と コンポーネント のシリアル化されたデータです。
    
- Editor_GUI_Layout //全てUIElementに置き換わる部分?  
    OnGUI()を描画するためのメソッド群?(コマンドバッファ的なのを裏で作って最後に実行?)  
    GUIは実行時でも動く？(全然確認してない)
    EditorGUIは自動レイアウトでなく、Rectで位置と範囲をしていする?  
    EditorGUILayoutは自動レイアウトで上から順に並べてくれる?  

- CustomEditor  
     EditorUtility.SetDirty を使用します。そして Unityプロジェクトを保存（File -> Save Project や AssetDatabase.SaveAssets）したとき、ダーティーフラグの立ったオブジェクトすべてがアセットに書き込まれます。
     CustomPropertyDrawerもCustomEditorもSerializedProperty(フィールドの型)とSerializedObject(コンポーネントのクラス)を自動でその型に設定されて使われる?  
     (EditorのOnInspectorGUI(base内部DoDrawDefaultInspector)とPropertyDrawerのEditorGUILayout.PropertyFieldの二段構え)

     SceneはUnityEngine.Objectを持っているEditorもUnityEngine.Objectを持っている?




 - C# Job System
 nativeArray[i] = structVal
 Struct this[int i]{set{(ptr + i*typeof(Struct))* = value;}}かな？

 - UnsafeUtility
    https://qiita.com/mao_/items/fc9b4340b05e7e83c3ff
    https://qiita.com/pCYSl5EDgo/items/4b5a5e089eabc8f4387d
    void* Malloc(long size, int align, Allocator allocator)
    void Free(void* ptr, Allocator allocator)
    void* GetUnsafePtr(NativeArray<T> nativeArray);
    SizeOf,AlignOf,AddressOf
    bool IsBlittable<T>() where T : unmanaged //型引数がBlittable型であるかどうか判別します。
    bool IsUnmanaged<T>() where T : unmanaged //型引数がunmanagedな構造体であるかどうか判別します。
    bool IsValidAllocator(Unity.Collections.Allocator allocator) //Allocator.NoneとAllocator.Invalidはfalseを返します。
    void MemClear(void* destination, long size) //ポインタの指す先のsize分の領域を0クリアします。
    void MemCpy(void* dest, void* src, long size)
    void MemMove(void* dest, void* src, long size)//領域が重なる場合はMemMoveを使用してください。
    AsyncReadManager
    ```C#
    using Unity.Collections.LowLevel.Unsafe;
    public unsafe struct NativeObject<T> : IDisposable where T : unmanaged{
        [NativeDisableUnsafePtrRestriction] readonly T* _buffer;
        readonly Allocator _allocatorLabel;

    #if ENABLE_UNITY_COLLECTIONS_CHECKS
        [NativeSetClassTypeToNullOnSchedule] DisposeSentinel _disposeSentinel;
        AtomicSafetyHandle _safety;
    #endif

        public NativeObject(Allocator allocator, T value = default){
            this._buffer = (T*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>();, UnsafeUtility.AlignOf<T>(), allocator);
            *this._buffer = value;
            this._allocatorLabel = allocator;
            #if ENABLE_UNITY_COLLECTIONS_CHECKS
                DisposeSentinel.Create(out _safety, out _disposeSentinel, 0, allocator);
            #endif
        }

        public T Value{
            get{
            #if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(_safety);
            #endif
                return *_buffer;
            }
        }

        public void Dispose(){
        #if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref _safety, ref _disposeSentinel);
        #endif
            UnsafeUtility.Free(_buffer, _allocatorLabel);
        }
    }
    ```