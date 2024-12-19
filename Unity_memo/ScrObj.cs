using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Threading;

// Component
    // GameObjectにアタッチする
        // Componentから参照される様にする事もできる
    // UnityObjectなのでシリアライズできる
        //シリアライズされたデータはSceneまたはPrefabに保存される
    // MonoBehaviour
        // C#Scriptで定義する
        // 沢山のコールバックが呼ばれる
// Asset
    // GameObjectにアタッチしない
        // Componentから参照される様にする
    // UnityObjectなのでシリアライズできる
        // シリアライズされたデータはAssetファイル(もしくは.meta)に保存される
    // ScriptableObject
        // C#Scriptで定義する
        // Awake, OnEnable, OnDisable, OnDestory の4つのコールバックが呼ばれる

//C:\python学習メモ\vscode-works_study\git_test\UnityMemo.md からコピペ
    // "Scene→○": Sceneの先の何か?    [C#(C++)]    ->:C#参照    Destroy([C#(C++)]) → [C#]    __:何もなし
    // ==Destroy([(Game)]);===========================//参照関係も自身も丸ごと消えるのでGameObjectへの参照は気にしなくてもいい?
    // "Scene→○" -> [(Game)] <-(C++)-> [(Comp)] -> [(Asset)]
    // Destroy([(Game)]);
    // "Scene→○"                       [Comp] -> [(Asset)]    //[Socket] OnDestroyされていたので多分そう
    // [Comp] -> [(Asset)]    //staticGameObject = null;をしなくてもUnloadUnusedAssetsの対象になってしまったので多分そう
    // C#GC
    // (Asset)
    // UnloadUnusedAssets
    // __
    // ==Destroy([(Comp)]);===========================
    // "Scene→○" -> [(Game)] <-(C++)-> [(Comp)] -> [(Asset)]
    // Destroy([(Comp)]);
    // "Scene→○" -> [(Game)]           [Comp] -> [(Asset)]
    // [Comp] -> [(Asset)]
    // C#GC
    // (Asset)
    // UnloadUnusedAssets
    // __
    // ==Destroy([(Asset)]);===========================
    // "Scene→○" -> [(Game)] <-(C++)-> [(Comp)] -> [(Asset0)] -> [(Asset1)]    //"-> [(Asset1)]"の部分は予想
    // Destroy([(Asset0)]);
    // "Scene→○" -> [(Game)] <-(C++)-> [(Comp)] -> [Asset0] -> [(Asset1)]   //Inspectorで消失 //[(ScrObj0)]を[Destroy]するとC++側が破棄されるだけでC#側は残る

//シリアライザ、ドメインリロード(C#再起動)、UnityObject、NativeObject(C++)、SerializeObject、C#Object、Scene、GameObject、Prefab、Component、Asset、
//UnloadUnusedAssets、Hideflags、cctor、ctor、Awake、OnEnable、Start、Update、OnDisable、OnDestroy、OnValidate、OnBeforeSerialize、OnAfterDeserialize
//C#Obj,NativeObj,Asset
//関心がある要素を抜き出し、それのテスト方法を考え、一気にテストしてしまう。
//シリアライズ:   フィールドデータをファイルに保存
//デシリアライズ: ファイルからフィールドデータを復元

//AddComponentで追加してもDirtyは付かずSaveSceneすると保存される
//YamlファイルにGUIDがあってもファイル名がGUIDのファイルがあるとは限らない。GUID -> パス変換しアクセスしている?(多分、AssetDataBaseV2)
//UnityEngine.Objectの変更は常に内部キャッシュに同期せずUnityEngine.Objectだけ変更する。そのためSaveAssetをしても内部キャッシュは変わってないため変わらない?
    //Inspector上では変更されたように見える(Updateの中でUnityObjectから内部キャッシュにSerialize(同期)しSerializeObject経由で表示される?)
//VSCodeもUnityもSaveするとファイルが更新される。
//SaveAssetsとCreateAsset(生成という意味で)以外はファイルの更新を行わない?
    //●UnityEditor上で変更してからSaveAssetsしないでImportAssetすると 一貫性のない結果を生成した となる?
//●既に存在しているファイルにCreateAssetしても既に存在しているファイルへの変更となり、削除されていない?
//●UnityEditor上でシリアライズデータを変更してSaveAssetsしないでImportAssetしても、まだ更新されてないファイルのシリアライズデータで 更新されないで かつ 警告もでない。
//SaveAssetsはSerializedObjectではなくUnityEngine.Objectからシリアライズしアセットに保存する?
//●ApplyModifiedPropertiesはUnityObjectまで更新し、UpdateはUnityObjectから更新される?(もしくはUnityObjectとSerializedObjectが常に同期している)
    //内部キャッシュは存在するらしいが、必要性はあるのか..
//●GameObjectと(ある一つの)Componentのアタッチの対応関係が必ず一対一(存在するComponentは必ず、あるGameObjectにアタッチされている)
    //そのためGameObjectがDestoryされればアタッチされているComponentもDestoryされるがC#側は参照が残れば残る
    //●Sceneがアンロードしてもその中の全てのGameObjectがDestroyされる為、ComponentもDestoryされる
        //Sceneと(ある一つの)GameObjectの対応関係も一対一。
            //●つまり、Scene -> GameObject(親子階層) -> Component の連なりは一意に定まる。
            //そして、どこかの階層でDestroyされればそこから下の階層は再帰的にDestroyされる(Assetまでは消えない)
//●UnityEditorのPlayMode終了してもstaticが初期化されない。ドメインをリロードしていない?
//●ドメインのリロードは、構造の下からシリアライズしDestroyされて行き、構造の上からデシリアライズしEnableされて行く?(ランダムだった)
//ScriptableObjectのHideFlagsが.Noneでアセットに保存しないとScene Memoryとなり、それをComponentに参照させてPlayModeを開始しても、Noneにならず復元される(Scene(.unity)に保存されていた)
    //HideFlagsがDontSaveでもアセットに保存しているとNot SavedだがNoneにならない(Scene(.unity)にアセット(.asset)へのパス(GUID)が書き込まれていた)
    //●.assetに保存しないで参照させると.unityに保存されそこから.assetに保存すると.unityから.assetに保存した部分が消え、代わりに.assetへのGUIDパスに変わる
        //●なんらかのファイルにシリアライズして保存されていれば復元される?
        //●HideFlagsのUnloadUnusedAssetは関係ない
    //●●Sceneから辿れるすべてのシリアライズ可能なSystem.Objectは復元される
//ScriptableObjectを.assetに保存したものはメモリのAssetsに入り何処にも参照されてない時(プロジェクトブラウザで.assetがあるフォルダを開きマウスポインタが触れてない時も含む)に、
    //UnloadUnusedAssetsを呼び出すとUnloadされる。その後、↑のマウスポインタが触れただけでもLoadする。
//メモリの種類(優先度の高い順)
    //Not Saved:    HideFlags.DontUnloadUnusedAsset
    //Assets:       アセットとして保存されている。
    //Scene Memory: 上記の何方でも無い。(Sceneにあるものもココ)
//AssetDatabase.ImportAssets, LoadAtPass と Resourceses.Load

//UnityObject(C#側)と内部キャッシュとSerializeObjectとmeta,prefab,assetファイルの関係
    //                                         [UnityObject(C#側)]
    //
    //                                Deserialize ↑  Serialize ↓    (ISerializationCallbackReceiver)
    //  _________________                        __________________
    // |                 | <-SaveAsset(Dirty)-- |                  |
    // |meat,prefab,asset| --LoadAssetAtPath--> |内部キャッシュ(C++)|
    // |_________________| --ImportAsset------> |__________________|
    //                   (AssetDataBaseV2)
    //                     ApplyModifiedProperties ↑    Update ↓    (SerializedObject)
    //
    //                                          [SerializeObject]
    //
    //LoadAssetAtPathとImportAssetはC#側C++側を含めた普通のUnityObjectを生成する。

//PlayMode(ドメインリロード と Sceneロード)
    //Editorの起動時
        //恐らく色々ロード後、UnloadUnusedAssetsが呼ばれている。
    //PlayMode開始時 (ドメインリロード -> SceneLoad)
        //現在開いている全ての未保存なSceneを含めてBackupSceneにシリアライズ -> 全UnityEngine.Objectをシリアライズ -> ドメインリロード(static剥げる) -> 全UnityEngine.Objectをデシリアライズ
        // -> isPresistentなUnityEngine.Objectを残し、そうでない大体のUnityEngine.ObjectをDestory(DontUnloadUnusedAsset関係なし) -> BackupSceneをLoad
    //PlayMode,EditorMode両方の時のSceneLoad(Single)
        //全Sceneの GameObject -> Component はUnLond(Destroy)され ->
        //ルートにStatic,DontUnloadUnusedAssetがない[isPresistentでない または [isPresistentで かつ Editor機能に捕まっていない]] UnityObject(GameObject,Componet以外)は
        //UnloadUnusedAssets(多分)でDestroyされる -> 次のSceneがLoadされ GameObject -> Component -> Asset がLoadされる。
    //PlayMode終了時
        //isPresistentを含めた大体のUnityEngine.ObjectをDestory -> BackupSceneを再Load (AssetはProjectFolderに触れただけでLoad(Import)される)
        //↑(isPresistentでかつStaticまたはDontUnloadUnusedAssetは消えない)
    
    //追記: BackupSceneはSceneが未保存な場合のみ発生するらしい
        //(現在のシーンをバックアップ: これは、Sceneが修正された場合にのみ起こります。Play Mode が終了したときに、Unity が Scenes を Play Mode の開始前の状態に戻すことができます。)
    //isPresistentなAssetはPlayMode中の変更はPlayModeが終了しても保持される。そうでないSceneに保存されているObject(ScrObjなど)は再Loadで元にもどる。
    //[Asset(isPresistent)とScene]に属していないObject はSceneのPlayMode開始時にDestoryされる。(Static(剥げる), DontUnloadUnusedAsset 関係なし)
        //Sceneをクリーン(実機と同じ)な状態にするのならisPresistentも消えるべきだが何故かどうやっても消えない。
    //ドメインリロードは現在存在している全てのUnityEngine.Objectを全て復元するがStaticは剥げる
    //"PlayMode開始時"のSceneロードはAssetに保存されていないUnityEngine.ObjectをDestoryしてクリーンにしてからLoadされる。
        //ドメインリロードはStaticを剥がし、SceneロードはリークオブジェクトをDestoryする。

//isPresistant (このオブジェクトは永続的ですか？アセットは永続的、シーンに保存されているオブジェクトは永続的、動的に作成されたオブジェクトは永続的ではありません。)
    //Assetとしてファイルに保存されている。
    //InstanceIDが正の数だとisPresitantが必ずtrueで、Unityのエディターが起動してから(UnityAPIで?)動的に作られAsset化していない。多分。(←をしたら負の数のisPresistantができた)
        //(UnityEngine.Object.GetInstanceID()の値です。私の見解では、正のIDは永続的なオブジェクトを示し、負のIDはランタイム中に作成されたオブジェクトを示します。)

//Asset(isPresistent)のロード、アンロードの挙動
    //Projectフォルダで現在表示されているAssetは マウスカーソルを乗せる または PlayMode開始時 または Editor起動時 にロード(ImportAsset?)される。(まだUnloadUnusedAssetsで消える)
        // -> そこからそのAssetのInspectorを表示するとEditor機能(UnityEngine.Object[],HashSet.Slot<UnityEngine.Object>)に捕まりUnloadUnusedAssetsでアンロード出来なくなる。
            //CreateAssetでも捕まる。 //スクリプトリロードでEditor機能(↑はUnityObjectではない)が消えるのでまたUnloadUnusedAssetsでアンロード出来る。
    //Assetの(明示的)DestoryはDestoryImmediate(obj,true)のみ可能。

//(Static)SceneVisibilityManager -> List<GameObject>↓
    //(C#)GameObject[]↓
    //(C#)GameObject      (C#)MonoB  -> (C#)ScrObj
    //   (GCH)↑ ↓         (GCH)↑ ↓      (GCH)↑ ↓
    //(C++)GameObject <-> (C++)MonoB -> (C++)ScrObj    //GameObjectとMonoB(Component)はC++通信のみ
        //他のMonoBやScrObjじゃなくComponentやAssetでも同じ構造だった。(Transformも)
            //しかし、Inspectorに表示しないとMonoBでないComponentはC#とGCHandle側を作らなかった。C#API操作として必要とされるまで生成しない?
            //C#側はC++側へのAPIとしてしか機能していない多分。なにもC++側を操作しようとしなければC++側のみで完結する?
            //https://youtu.be/NawYyrB_5No?list=PLtjAIRnny3h7KDDpkrsEEnILtEQLwOHiC&t=1191
            //[UnityC++Core] <-(C++)-> [UnityEngine.Object(C++)] <-(m_CachedPtr)(GCHandle)-> [UnityEngine.Object(C#)] <-> [C#Logic] <-> [UnityEngine.Object(C#)] <- .. -> [UnityC++Core]
            //                                     ↓                                                     ↓
            //               <-(C++)-> [UnityEngine.Object(C++)] <-(m_CachedPtr)(GCHandle)-> [UnityEngine.Object(C#)] <-> [C#Logic] <-> [UnityEngine.Object(C#)] <- .. ->
            //UnityEngine.Objectは、UnityのC++エンジン と ユーザーのC#コード を繋ぐ単なるインターフェース。多分。
            //IL2CPPは、[UnityEngine.Object(C#)] <-> [C#Logic] の部分もC++に変えてしまう。

//GCHandle
    //C++側が、GCHandleが参照しているSystem.ObjectがC#のGCの対象になったか観察していてGCの対象になったらUnloadUnusedAssetでUnloadできる?
        //GCHandle.IsAllocated: GCHandleType.Weakハンドルを使用する際にこのプロパティを使用して、GCHandleがまだ利用可能かどうかを判断します。
    //GCHandleType.Noneにする事により、C#のオブジェクトを参照しC#のGCをされない様にすることができる。これをC++側がもつ事によりDontUnloadUnusedAsset相当な事ができる?
    //[C#Obj] -> [C#Obj] -> .. -> [C#Obj] 
    //   ↑                          ↑
    //[C++Obj(.None)]           [C++Obj(.Weak)]   //C++ObjがあるC#Objは必ずUnityEngine.Object。
    //.NoneはDontUnloadUnusedAsset相当でStaticと同様のルートになり、WeakはUnloadUnusedAsset実行時にGCHandle.IsAllocatedがfalseだった場合に自身のデストラクタ呼んで解放(Unload)する?
        //なお、どちらも明示的Destoryにより即時、C++Objを完全に消す(解放)ことができる。

//ScrObj1 -> ClsObjNonSeri -> ClsObj と、繋げたがClsObjNonSeriでSerializableの繋がりを切ったため、ClsObjのSerializableは無効になりシリアライズしなかった。
    //シリアライズを効かせたいなら Serializable -> Serializable -> Serializable -> .. と繋げないとだめ
    //[System.Serializable]を付けてもHeapExplorerでは付けていない通常のオブジェクトと変わらなかった。

//●Hideflags
    //https://qiita.com/Shairo/items/df8077452d632e788bc1

    //.unityのm_ObjectHideFlags: ~ にシリアライズされている。
    //GameObjectにHideFlagsを設定するとアタッチされた全てのComponentも同じHideFlagsに設定される(UnityObjectは、Assetとして保存していなくてSceneに保存されるScriptableObjectであっても設定されない)
        //GameObjectの子は、同様に再帰的にHideFagsを設定しない。(そのGameObjectのみ設定される。)
    //GameObjectにコンポーネントを追加すると、その時のGameObjectのhideFlagの値が自動的にセットされる

    //DontUnloadUnusedAsset: UnloadUnusedAssetsでUnloadされたくないならコレ (Not Saved)
        //GameObject と Component にDontUnloadUnusedAssetを設定しても意味がない。Componentは常にGameObjectにアタッチされそしてGameObject自体がSceneのオブジェクトでルートだから
        //UnityObjectは、参照が無くなってもUnloadUnusedAssetでは消えないが、Destoryでは消える。
        //Assetは、元々UnloadUnusedAssetでは消えない? //いや、消えた(多分Editor関係に捕まってると消えないだけ)
    //DontSaveInEditor: .unityのSceneファイルに保存されたくないならコレ (Prefabにも保存されなくなるらしい)
        //GameObjectに設定するとSceneLoad時にリークする。.unityのSceneファイルに保存されない。
            //(LoadSceneMode.SingleによるUnloadではリークするが、まだSceneに属している時にUnloadSceneAsyncすると消える。)
        //Componentは、.unityのSceneファイルに保存されない。
        //Componentに参照されるUnityObjectは、.unityのSceneファイルに保存されない。
        //Componentに参照されるAssetは、SceneのデータではないのでAssetファイルには保存される。
        //UnityObjectは、意味ないし、リークもしない。(UnloadUnusedAssetで消せる)
    //DontSave: (明示的)Destory以外にUnloadされたくないならコレ (Not Saved)
        //GameObjectは、恐らくDontSaveInEditorと変わらず、SceneLoad時にリークする。.unityのSceneファイルに保存されない。
        //Componentも、恐らくDontSaveInEditorと変わらず、.unityのSceneファイルに保存されない。(常にGameObject(ルート)にアタッチされるのでDontUnloadUnusedAssetが意味がない)
        //UnityObjectは、(明示的)Destory以外消えなくなる。 
        //Assetも、(明示的)Destory以外消えなくなる。 
    //DontSaveInBuildは試してないが過去の記憶によると、Buildの.unityのSceneファイルに保存されない。だけだと思う。(Debug要員)
    //HideInHierarchy: Hierarchyに表示したくないならコレ
        //GameObjectは、Hierarchyに表示されなくなる
        //Componentは、意味無し
        //Assetは、ProjectFolderのアイコンがデフォルトアセットのようになりクリックしてもInspectorに表示されない。
    //HideInInspector: Inspectorに表示したくないならコレ
        //GameObjectは、GameObjectのInspectorは非表示になり(Tagとか)、アタッチされる全てのComponentにも設定されるため全てのComponentのInspectorも非表示になる。
        //Componentは、そのComponentは非表示になる。
        //Assetは、そのAssetは非表示になる。
    //NotEditable: Editor上で設定されたくないならコレ
        //GameObjectは、Hierarchy上で灰色の文字になり、Inspector上ではGameObjectと全てのComponentも灰色になり、情報は見えるが編集不可になる。
        //Componentは、そのComponentは灰色になり、情報は見えるが編集不可になる。
        //Assetは、そのAssetは灰色になり、情報は見えるが編集不可になる。

//●Instantiate (UnityObjectをC++側とC#側の両方をcloneする。HideFlagsとisPresistentはリセット)
    //コンストラクタが呼ばれる。UnityEngine.ObjectをC++側も含め複製する。
    //GameObjectの場合、そのGameObjectにアタッチされたComponentとそのGameObjectの子も同様に再帰的に複製する。Assetまでは複製しない。
        //Destoryした場合も、そのGameObjectにアタッチされたComponentとそのGameObjectの子も同様に再帰的に破棄する。Assetまでは破棄しない。
    //InstantiateしたUnityEngine.Object.nameに"(Clone)"が足される。
    //activeとenabledはオリジナルと同じになる。
    //HideFlagsは、オリジナルがどんな値でも.Noneにされる。
    //isPresistentは、オリジナルがtrueでもfalseになる(Assetごと複製しない)
    //InstanceIDは、もちろんオリジナルと異なる。(Cloneは今の所は負の数しか見てない)
    //C++側のSizeは、オリジナルと違う場合がたまにある。
    //ComponentをInstantiateするとGameObjectごとInstantiateする。
    //Texture2DをInstantiateしたら、読み取り不可能なテクスチャをインスタンス化できない というエラーが出たがC++側にオリジナルと同じSize2.7MBが複製されていた。
        //Assetは全てInstantiate出来るわけではない?

//ScriptableObjectのコールバック
    //Disabelが呼ばれアンロードして終わった時にProjectWindowでScriptableObjectが表示されていた場合は-> Deserialize -> Awake -> Enableも呼ばれる
        //ImportAsset,LoadAssetAtPathでもそう
    //isPresistent HideFlag .None
    //Unity起動
        //なし(ProjectWindowで表示されるとロードされる(Deserialize -> Awake -> Enable))
    //CreateInstance
        //Awake -> Enable
    //CreateAsset
        //Serialize
    //スクリプトリロード, PlayMode開始
        //Desabel -> Serialize -> Deserialize -> Enable 
    //UnloadUnusedAssets(isPresistent,NonIsPresistent), PlayMode終了, EditorMode,PlayModeのSceneロード(Single)
        //Disabel 
    //明示的Destroy(isPresistent,NonIsPresistent),NonIsPresistentの時の[SceneロードとPlayMode終了]
        //Disabel -> Destroy
    //ロード, Componetなどの参照を辿って先にロード(S_S_S), ProjectWindow
        //Deserialize -> [Awake -> Enable]

    //S_S_S
        //生成順に負の数でマイナス方向にInstanceIDが振られる(-1394, -1398, -1402)
        //スクリプトリロード時のコールバックは、ランダムに呼ばれる様に見え、以降その順で固定され呼ばれる(OnDisableだけ順序が違う)

//MonoBehaviourのコールバック(https://tsubakit1.hateblo.jp/entry/2017/02/05/003714)(https://yotiky.hatenablog.com/entry/2018/11/22/%E8%A4%87%E6%95%B0%E3%81%AEComponent%E3%81%AE%E3%82%A4%E3%83%99%E3%83%B3%E3%83%88%E9%96%A2%E6%95%B0%E3%81%AE%E5%AE%9F%E8%A1%8C%E9%A0%86)
    //NonExecuteAllwaysの時、Awake, Enable, Disabel, DestroyはPlayMode時のみ呼ばれる
        //生成順に負の数でマイナス方向にInstanceIDが振られる
        //PlayMode時に動的に生成(attach)しない場合はInstanceIDの昇順で呼ばれる。(負の数(動的に生成) から 正の数(isPresistentでファイルからロード))
        //PlayMode時に動的に生成(attach)する場合は生成順(InstanceIDの降順)で呼ばれる。
            //InstanceIDは実行順を一意に決めるものではない?(SEOもあるし)
            //GameObject単位で呼ばれていない。Go0Comp0 -> Go1Comp0 -> Go0Comp1 の様にバラバラになる事がある。
        //[Awake -> Enable] と Disabel はGameループ(Playerループ?)では無いのでAddComponent,Instantiate,enabled,Destroyなどの呼び出し時にすぐに呼ばれる。
            //(enabledはプロパティなのでその中で呼んでいると思われる)
        //Destroyは、Destroy()でGameループの最後に呼ばれる。DestroyImmediate()は呼び出し時にすぐに呼ばれる。
    
    //GameObjectのactive ON時
        //Awake (Componentのオブジェクトが生成(AddComponet,Instantiate,PlayMode開始時など)されてから1回だけ呼ばれる)
    //Componentのenabled ON時 (EnableとDisableはenabledのトグルでトグルする)
        //Enable -> Start (Startは、Componentのオブジェクトが生成(AddComponet,Instantiate,PlayMode開始時など)されてから1回だけ呼ばれる)
    //Componentのenabled OFF時
        //Disable
    //明示的Destroy, PlayMode終了時
        //Destroy (多分、ComponentのDestroy(C++側の削除)時に呼ばれるが、Awakeが呼ばれていない(アタッチしているGameObjectが一度もactiveになっていない)と呼ばれない)
    
    //[Awake -> Enable](Component毎、オブジェクト生成直後) -> Start(Gameループ) -> [Disable -> Destroy](GameObjet毎)

//●ScriptExecuteOrder
    //設定方法
        //ProjectSettings/Script_Execute_Order と .metaのexecutionOrder: は互いに同期している。
            //Script_Execute_Orderで0を設定すると一覧から消える。
        //MonoBehaviourの[DefaultExecuteOrder(int order)]でも設定できるが↑の2つが0または未設定の時有効になる(Script_Execute_Order == executionOrder: > [DefaultExecuteOrder])
            //0の時、有効になるのでScript_Execute_Orderの一覧にない(0のはず)のに0ではないと言うバグの様な状況になる。
    //実行順
        //[Awake -> Enable](Component毎) -> Start -> Updateなどその他コールバック の実行順序を指定できる。
        //[Disable -> Destroy](GameObjet毎) の部分は変化しない。 
    //"コンポーネントやオブジェクトを動的に追加した場合は、生成順になる"と書いてあったがそうにはならなかった。(https://tsubakit1.hateblo.jp/entry/2017/02/05/003714)
        //(Attach_SEO0_to_MonoCube()で動的に追加して試した) //やり方が正確に違うから違うのか、修正されたのか(修正されたとしたら挙動が変わって下位互換性が無くなる気がする)

//●Findまとめ //TransForm以外のstatic関数はSceneをまたいで取得可能(そもそもどのSceneで呼んだか区別できないかな)
    //Object.FindObjectOfType<T>(): 型でUnityEngine.Objectを検索、複数形あり
        //isPresistent, DontSave, 非アクティブ 取得不可
    //Resources.FindObjectsOfTypeAll<T>(): 型でUnityEngine.Objectを検索、複数形のみ
        //何でも取得可能(最強)
    //GameObject.Find(string): nameでGameObjectを検索、複数形なし
        //非アクティブ取得不可、"go0/go1/go2"とすることで子のオブジェクトを取得できる。
    //GameObject.FindGameObjectWithTag(string): 設定したtagでGameObjectを検索、複数形あり
        //非アクティブ取得不可
    //gameObject.transform.Find(string): 自身(transform)の子のGameObjectを検索。結果もtransform、複数形なし
        //非アクティブ取得可能、"go0/go1/go2"とすることで子のオブジェクトを取得できる。
    //gameObject.scene.GetRootGameObjects()
        //sceneに属しているGameObjectをGameObject[]で返す。

//ExecuteEvents.Execute https://tsubakit1.hateblo.jp/entry/2015/04/13/010645 https://www.hanachiru-blog.com/entry/2019/06/23/205101 (Receive.cs, ExecuteEvents_Execute.cs)
    //SendMessage()に変わる新しい?(Unity4.6)メッセージシステム
    //受信側
        //IEventSystemHandlerを継承したinterfaceに受信するメソッドを定義し、それを継承したComponentを定義する。
    //送信側
        //ExecuteEvents.Execute<↑のinterface>(terget: ←を持っているgameObject, Action<↑のinterface>: functor)
    //BaseEventData eventDataは知らない

//●Coroutine
    //Unityのスクリプトライフサイクル(https://docs.unity3d.com/ja/2021.1/Manual/ExecutionOrder.html)の中で
    //各、yield returnに応じたタイミングで実行を再開する。
    //MonoBehaviourクラスにStartCoroutine(IEnumerator routine)とStopCoroutine(IEnumerator coroutine)を持っていて
        //これを使ってCoroutineを操作する。
    //StartCoroutine(IEnumerator coroutine)
        //coroutineの実行を開始する。同じ引数で複数回呼ぶとUnityのサイクル内で複数回実行される。(同一のcoroutineを複数回実行する(Waitは独立でcoroutineは同一かな?))
        //StartCoroutine()は呼んだ時、その中で即時一回実行され、その後yield returnに応じたUnityのサイクル内で呼ばれる。
    //StopCoroutine(IEnumerator coroutine)
        //StartCoroutine()の引数に渡していたcoroutineの実行を一時停止する。複数回呼んでいた場合も一斉に止まる
        //再びStartCoroutine()にその引数で呼ぶ事で実行を再開する事ができる。
    //coroutineの中で複数のyield returnが色々なYieldInstruction, CustomYieldInstructionを返してもそれぞれに対応するタイミングで再開される。
    //MoveNext()しきって使い果たしたcoroutineは当然、停止し、StartCoroutine()に渡して実行しても無反応。
    //考察 (C:\Users\asreia\Reverse\Assets\CoroutineTest.cs :82 自作簡易コルーチンシステム)
        //StartCoroutine(),StopCoroutine()は同一の引数を渡して呼ぶ事でcoroutineを複数回実行したり一斉に停止することができるので、
        //内部で実行中のcoroutineを持っていて、外部からcoroutineをKeyとして実行中の同一のcoroutineを停止したり再開したり操作できると思われる。
          //追記:妄想:List<CoroutineState> CoroutineStateList; struct CoroutineState{IEnumerator routine; ＠❰Custom❱YieldInstruction waitPoint; bool enable;} //enableは⟪Start¦Stop⟫Coroutine
    //YieldInstruction, CustomYieldInstruction
        //WaitForFixedUpdate():                 物理演算実行後まで待つ、１フレーム内に複数回実行される事がある
        //WaitWhile(Func<bool> predicate):      delegateがtrueを返す間停止される。
        //WaitUntil(Func<bool> predicate):      delegateがtrueを返すと実行される。WaitWhile(()=>true) == WaitUntil(()=>false)
        //null:                                 毎フレーム実行される。
        //WaitForSeconds(float time):           指定秒間待ってから実行を再開する。
        //WaitForSecondsRealtime(float time):   スケールされていない時間で指定秒間待ってから実行を再開する。
        //WaitForEndOfFrame:                    フレームの最後まで待つ。
    // //実行順は恐らく FixedUpdate -> [Update] -> While == Until == null == Seconds == SecondsRealtime -> StartCoroutine? -> EndOfFrame

//New Prefab Workflow (Prefabファイルは GameObjectとPrefabObjectの親子階層 -> Component -> Asset をまとめたモノ)
    //https://www.youtube.com/watch?v=Ih1QlmYLeDI https://www.youtube.com/watch?v=Apr0SBjqTAY
    //https://www.youtube.com/watch?v=On8qUnuFHmo&t=2656s
    //PrefabはランタイムではSceneにベイクされランタイムにはPrefabと言う概念は無い。
    //PrefabVariant
        //PrefabVariantはあるPrefabから派生をつくりオブジェクト指向の継承を再現する。
            //(派生は基底のPrefabを常に参照し、基底のパラメータを変更すれば派生も変わる。派生のパラメータの変更は基底のパラメータをOverrideする。)
            //派生Prefab(Variant)は基底Prefab(ただのPrefab)の参照と位置回転名前のOverrideしか持っていないのでYAMLがスッキリしている。
        //NestedPrefabされたPrefabのルートもそのまま派生を生成する事ができる。
        //NestedPrefabの[overrides ▼]のApply先のPrefabVariantの項目にはそのPrefabVariantのみでは無く下から基底 -> 派生 -> 派生の派生.. が差し込まれる。
    //NestedPrefab
        //Apply先の指定方法
            //NestされたPrefabのルートのみ[overrides ▼]があり、そこからApply先を指定する。([Apply All]を押すとルートにOverrideされる)
                //一番下の項目は自分自身へのApplyで二番目からoverrideとなり上の項目ほど影響範囲が小さくなる(▲)(Variantの基底も含まれる)
            //HierarchyのPrefabの右側にある">"かPrefabのInspectorの[ Open ]より
                //そのPrefabのPrefabEditorが開きそこで編集されたパラメータ(プロパティ)はそのPrefabへのOverrideまたは自分自身へのApplyとして保存される。
        //自分自身の位置と回転と名前はその自分自身のPrefabに保存できない。(親PrefabまたはSceneへのOverrideとして保存する事はできる)
        //ある子Prefabのパラメータの変更がどの階層のPrefabにOverrideされているか分かりにくい。
            //(SceneへのOverrideは青色の"|"がパラメータの左側に付く(あと、GameObject,Componentの追加は(+)、Componentの削除は(-)が付く))
        //SceneへのOverrideがあるPrefabをあるPrefabの子PrefabにしそのPrefabにApplyした場合、子PrefabのSceneへのOverrideはそのPrefabへのOverrideにすり替わる。
        //NestedPrefabの子PrefabにはそのPrefabの削除と移動、Componetの移動が出来なくなる(Componentの削除はできる)(Prefabの移動できないと言うがTransFormの"︙"のMove to Frontなどで出来てしまう)
            //↑プレファブモードならできる(↑局所的な変更と見なされていただけだった?)
    //YAML
        //PrefabVariantもNestedPrefabもOverrideすると、派生Prefabまたは親PrefabにOverrideされたパラメータだけ追加で書き込まれる↓。
            // - target: {fileID: 2494047992306550763, guid: cba9722e1f8256d40920fe1d8ff8d528, type: 3}
            //     propertyPath: m_IsTrigger
            //     value: 0
            //     objectReference: {fileID: 0}
            //●↑の部分を削除すればOverrideを無かった事にできる。(これ以外は無かった事にできない?//自分自身のPrefabにApplyしたらOverride情報が全階層で消えた)
            //位置と回転と名前は自分自身のPrefabに保存されないので派生Prefabまたは親Prefabに既に書き込まれ既にOverrideされている。
            //２つ以上階層が離れるとguidは１つ上の基底Prefabまたは下の子Prefabしか指さず、そのguid内でfileIDを検索してもComponentが見つからないと言う、謎の情報が書き込まれるが正しく動作はしている。。

//CustomEditor 保留

//●プロジェクトフォルダ内の削除 -> UnityHubから起動
    //Librayフォルダを消すとUnity起動時にインポートが走りLibrayが再構築される(Librayをリフレッシュできる)
    //バージョン管理は、Assets,Packages,ProjectSettingsフォルダがあれば、プロジェクトを復元できバージョン管理できる。
    //Assets,ProjectSettings/ProjectVersion.txtがあれば、警告なしに起動する。
    //Assetsだけあれば、起動はするが、versionがUnity5.0だと言う警告がでる
    //Assetsも無い空のプロジェクトフォルダだと無効なパスと出て起動しない。

//.meta
    //type: 0 //Null?
    //type: 1 //Imported Asset?
    //type: 2 //Scene Object?
        //ScriptableObject
    //type: 3 //Source Asset?
        //MonoBehaviourスクリプト, Texture2D

//●HeapExplorer
    //HeapExplorerは、UnityAPIのMemorySnapshotを使って取得したデータを解析している。
    //UnityObject -> NativeObject -> GCHandle -> UnityObject -> .. 
    //Coles SnapshotしてHeapExplorerを閉じてGC.Collect()5回くらいでメモリを 12GB から 2GB に減へった。
    //●Captrueする時はColes Snapshotして、ウインドウを閉じてGC.Collect()をfor文で10回実行して500MB台にメモリ使用量を落とす
        //(追記)それより、Use Multi-Threadingを無効にして Capture -> Coles Snapshot -> Capture -> ..
        //↑↑で、何かが原因でメモリを圧迫している時は原因を閉じてGC.Collect()を連打すれば良い?
    //GCHandleが参照しているだけのUnityEngine.Objectもある(C++側はない)

//Inspector表示時にOnBeforeSerialize()が呼ばれ続ける理由 (https://www.urablog.xyz/entry/2017/08/06/104652 の最後(さいごに))
    //InspectorのPropertyに表示されている情報はシリアライズされた情報(C++側)である。
        //よってC#側のフィールドデータをシリアライズして常に最新の情報を表示する為に呼ばれ続ける。(多分Updateの時に呼ばれる)
    //InspectorのPropertyを書き換えた場合は、逆にOnAfterDeserialize()が呼ばれシリアライズ情報(C++側)からフィールドデータ(C#側)へ反映される。

//循環参照したUnityObjectでもしっかりUnloadUnusedAssetsで消える

//Sceneに属しているScriptableObjectがあってそのSceneやGameObjectやComponentを削除してもScriptableObjectが消える訳ではない。他のアセットも多分同じ(よく考えると当たり前かな)
//ScriptableObjectの.assetをVSCodeで変更して保存しUnityに戻ると変更が反映されている(他の.metaも多分同じ)

//アセットデータベース
    //AssetDatabaseV2 https://docs.unity3d.com/ja/2021.1/Manual/AssetDatabase.html    https://www.youtube.com/watch?v=ldpu3pzUsaM
        //AssetはImportされると、Library/Artifactsにインポート結果を保存する(Import時にソースファイルを使いやすい様に変換する)
        //LoadAssetAtPathなどで取得したAssetは変換したインポート結果のデータをC++側に持つ。
        //AssetDatabaseV2では、一つのGUIDでSwitchPlatformやインポートなどの設定により複数のインポート結果を持てる。(設定を変える事によりGUIDが指すインポート結果が切り替わる)
    //アセットデータベースの更新
        //SourceAssetDBで変更を検知し自身を更新する -> コード関連のアセットをImportしドメインリロード -> コード以外のアセットをImport
            //コード以外のアセットはファイルの拡張子にそれぞれ対応したインポーターが起動する。Unity組み込みのNative Importers を Scripted Importersで上書きする事もできる。
    //AssetDatabase によるバッチ処理
        //AssetDatabaseによるファイル操作を複数並べると一つ一つにオーバーヘッドが生じる。
            //StartAssetEditingとSotpAssetEditingで囲むとそれを抜けた後にバッチ処理しオーバーヘッドを最小限にできる
            //↑はカウント方式でStartでインクリメントしStopでデクリメントし0になった時にバッチ処理を実行する。こうする事で関数呼び出しなどによりネストした状態になっても正しく機能する。

//UnityObjectがファイルに保存されるかされないか。
    //エディター操作によるSave(Clrt + S)は全ての.unityを含めた全てのアセットを保存するが、.unityでIsDirtyでDirtyが付いていても保存されない場合がある。
        //任意のUnityObjectでEditorUtility.SetDirtyでDirtyを付ける(例外: .assetに保存されていないScene内のScriptableObject) または
            //シリアライザ経由の変更によるDirty(ApplyModifiedProperties実行(エディター操作))はSave(Clrt + S)で保存される
    //要約
        //エディター操作はSave(Clrt + S)でSceneもそれ以外も保存する。
        //プログラム操作では、
            //SceneはSaveScene(Scene)で.unityを保存する。           //HierarchyのSave Scene と UnityEditor.SceneManagement.EditorSceneManager.SaveScene(Scene)は同じ?
            //それ以外のAssetはSaveAssetsで.unity以外を保存する。    //Save Project は AssetDatabase.SaveAssetsと同じ
            //SaveAssetsはDirtyが付いていれば保存する。(SaveSceneはUnityEngine.Object経由による変更でDirtyが付いていなくても保存した)
            //UnityEngine.Object経由による変更はDirtyが付かな無いので変更を保存したい場合はSetDirtyでDirtyを付ける。
public class ScrObj_Method{
    // [MenuItem("Scriptable/CreateScrObj")]
    // public static void CreateScrObj_(){
    //     Debug.Log("CreateScrObj_2");
    //     ScrObj scrObj = ScriptableObject.CreateInstance<ScrObj>();
    //     ScrObj1 scrObj1 = ScriptableObject.CreateInstance<ScrObj1>();
    //     new SerializedObject(scrObj).ApplyModifiedProperties();
    //     AssetDatabase.CreateAsset(scrObj, "Assets/ScrObj.asset");
    //     AssetDatabase.AddObjectToAsset(scrObj1, "Assets/ScrObj.asset"); //Yaml上では差し込まれる位置はランダム?
    //     // AssetDatabase.SaveAssets(); //インポートする前に保存しないと警告がでる。
    //                                     //Importer（NativeFormatImporter）は、asset（guid：484162f22f856644bbd7ae3943c39afe） "Assets /ScrObj.asset"に対して一貫性のない結果を生成しました
    //     // AssetDatabase.ImportAsset("Assets/ScrObj.asset");
    //     AssetDatabase.Refresh(); //プロジェクト内(Assets)の全てのファイルをチェックして未インポートなAssetがあればImportAssetを実行する。
    // } 
    [MenuItem("C++Ref_Test/Create_S_C_C_S")]
    public static void Create_S_C_C_S(){
        Debug.Log("Create_S_C_C_S"); // _ (CreateAsset時) // _ (CreateAsset無し時)
        ScrObj_Ref root_ScrObj = ScriptableObject.CreateInstance<ScrObj_Ref>();
        AssetDatabase.CreateAsset(root_ScrObj, "Assets/ScrObj_Ref_0sccs.asset");
        root_ScrObj.clsObj_Ref = new ClsObj_Ref();
        root_ScrObj.clsObj_Ref.clsObj_Ref = new ClsObj_Ref();
        root_ScrObj.clsObj_Ref.clsObj_Ref.scrObj_ref = ScriptableObject.CreateInstance<ScrObj_Ref>();
        AssetDatabase.CreateAsset(root_ScrObj.clsObj_Ref.clsObj_Ref.scrObj_ref, "Assets/ScrObj_Ref_1sccs.asset");
        AssetDatabase.SaveAssets();
    }
    [MenuItem("C++Ref_Test/Create_S_S_S")]
    public static void Create_S_S_S(){
        //生成順に負の数でマイナス方向にInstanceIDが振られる(-1394, -1398, -1402)
        //スクリプトリロード時のコールバックは、ランダムに呼ばれる様に見え、以降その順で固定され呼ばれる(OnDisableだけ順序が違う)
        Debug.Log("Create_S_S_S"); // S -> S -> S (CreateAsset時) // S -> S -> S (CreateAsset無し時)
        ScrObj_Ref root_ScrObj = ScriptableObject.CreateInstance<ScrObj_Ref>(); 
        AssetDatabase.CreateAsset(root_ScrObj, "Assets/ScrObj_Ref_0ss.asset");
        ScrObj_Ref temp = ScriptableObject.CreateInstance<ScrObj_Ref>(); 
        AssetDatabase.CreateAsset(temp, "Assets/ScrObj_Ref_1ss.asset");
        root_ScrObj.scrObj_ref = temp;
        root_ScrObj.scrObj_ref.scrObj_ref = ScriptableObject.CreateInstance<ScrObj_Ref>();
        AssetDatabase.CreateAsset(root_ScrObj.scrObj_ref.scrObj_ref, "Assets/ScrObj_Ref_2ss.asset");
        EditorUtility.SetDirty(root_ScrObj); EditorUtility.SetDirty(root_ScrObj.scrObj_ref); EditorUtility.SetDirty(root_ScrObj.scrObj_ref.scrObj_ref); 
        AssetDatabase.SaveAssets();
    }
    [MenuItem("C++Ref_Test/Create_A_S_C")]
    public static void Create_A_S_C(){
        Debug.Log("Create_A_S_C"); // A -> S (CreateAsset時) // A -> S (CreateAsset無し時)
        ScrObj_Ref scrObj_Ref = ScriptableObject.CreateInstance<ScrObj_Ref>();

        AssetDatabase.CreateAsset(scrObj_Ref, "Assets/ScrObj_Ref.asset");

        attachMonoCache.scrObj_Ref = scrObj_Ref;
        attachMonoCache.scrObj_Ref.clsObj_Ref = new ClsObj_Ref();
        AssetDatabase.SaveAssets();
    }
    [MenuItem("C++Ref_Test/Create_A_S_S")]
    public static void Create_A_S_S(){
        Debug.Log("Create_A_S_S"); // A -> S -> S (CreateAsset時) // A -> S -> S (CreateAsset無し時)
        ScrObj_Ref scrObj_Ref0 = ScriptableObject.CreateInstance<ScrObj_Ref>();
        AssetDatabase.CreateAsset(scrObj_Ref0, "Assets/ScrObj_Ref0.asset");    
        ScrObj_Ref scrObj_Ref1 = ScriptableObject.CreateInstance<ScrObj_Ref>();
        AssetDatabase.CreateAsset(scrObj_Ref1, "Assets/ScrObj_Ref1.asset");
        attachMonoCache.scrObj_Ref = scrObj_Ref0;
        attachMonoCache.scrObj_Ref.scrObj_ref = scrObj_Ref1;
        AssetDatabase.SaveAssets();
    }
    [MenuItem("UnityObjectTest_1/Instantiate_monoCubeCache")]
    public static void Instantiate_monoCubeCache(){
        Debug.Log("Instantiate_monoCubeCache");
        monoCubeCache = Object.Instantiate<GameObject>(monoCubeCache);
    }
    [MenuItem("UnityObjectTest_1/Instantiate_attachMonoCache")]
    public static void Instantiate_attachMonoCache(){
        Debug.Log("Instantiate_attachMonoCache");
        attachMonoCache = Object.Instantiate<AttachMono>(attachMonoCache); //ComponentをInstantiateするとGameObjectごとInstantiateする。
    }
    [MenuItem("UnityObjectTest_1/Instantiate_BoxCollider")]
    public static void Instantiate_BoxCollider(){
        Debug.Log("Instantiate_BoxCollider");
        // Object.Instantiate<BoxCollider>(monoCubeCache.GetComponent<BoxCollider>()); //ComponentをInstantiateするとGameObjectごとInstantiateする。
        // Object.Instantiate(monoCubeCache.GetComponent<BoxCollider>());
        Object.Instantiate<Component>(monoCubeCache.GetComponent<BoxCollider>()); //<Component>でも同じ動作(UnityEngine.Objectとして処理されてる)
    }
    public static Texture2D UnityLogoCache;
    [MenuItem("UnityObjectTest_1/LoadAssetAtPath_UnityLogo")]
    public static void LoadAssetAtPath_UnityLogo(){
        Debug.Log("LoadAssetAtPath_UnityLogo");
        UnityLogoCache = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UnityLogo.png");
        UnityLogoCache.hideFlags = HideFlags.None;
    }
    [MenuItem("UnityObjectTest_1/Instantiate_UnityLogoCache")]
    public static void Instantiate_UnityLogoCache(){
        Debug.Log("Instantiate_UnityLogo");
        //読み取り不可能な「UnityLogo」テクスチャをインスタンス化することはできません。インスペクタでテクスチャを読めるようにするか、インスタンス化しないようにしてください。
            //と、出るがHeapExplorerではUnityLogo(Clone)が出来ていてC++側のSizeがオリジナルと同じ2.7MBで複製されている。 (Exist_UnityLogo_ObjectでもCount出来ている)
        Object.Instantiate<Texture2D>(UnityLogoCache);  //Assetは、なんでもかんでもInstantiateできる訳じゃない事が分かった。多分。
    }
    [MenuItem("UnityObjectTest_1/Exist_UnityLogo_Object")] 
    public static void Exist_UnityLogo_Object(){
        Debug.Log("=Exist_UnityLogo_Object=");
        Texture2D[] tex2d_Array = Resources.FindObjectsOfTypeAll<Texture2D>();
        int count = 0; Texture2D tex2d = null; bool firest = true;
        foreach(Texture2D _tex2d in tex2d_Array){
            if(_tex2d.name == "UnityLogo" || _tex2d.name == "UnityLogo(Clone)"){
                count++;
                if(firest){
                    firest = false;
                    tex2d = _tex2d;
                }
            }
        }
        UnityLogoCache = tex2d;
        Debug.Log($"Texture2D: Count: {count}\n UnityLogoCache_HideFlags: {UnityLogoCache?.hideFlags}");
    }
    [MenuItem("UnityObjectTest_1/scrObjCache_Instantiate")]
    public static void scrObjCache_Instantiate(){
        Debug.Log("scrObjCache_Instantiate");
        scrObjCache = Object.Instantiate<ScrObj>(scrObjCache);  //HideFlagsは同じにならず.Noneになる。Assetの場合はUnityObject側のみ複製されてisPresistantはFalseになる。
    }
    [MenuItem("UnityObjectTest_1/scrObjCache_SetName")]
    public static void scrObjCache_SetName(){
        Debug.Log("scrObjCache_SetName");
        scrObjCache.name = "PekoTDa";
    }
    public static bool silentFlag = false;
    [MenuItem("UnityObjectTest/Silent_OnBeforeSerialize_DebugLog")]
    public static void Silent_OnBeforeSerialize_DebugLog(){
        silentFlag = !silentFlag;
        Debug.Log($"Silent_OnBeforeSerialize_DebugLog: {silentFlag}");
    }
    [MenuItem("UnityObjectTest/Rebuild_MonoCube")]
    public static void Rebuild_MonoCube(){
        Debug.Log("==Start_Rebuild_MonoCube==");
        Destroy_All_ScrObj();
        Create_ScrObj();
        Create_ScrObj_Asset();
        ScrObj_Set_IntValue();
        SetDirty_scrObj();
        SaveAssets();
        GameObject g; while(g = GameObject.Find("MonoCube")) GameObject.DestroyImmediate(g);
        Create_MonoCube();
        AttachMono();
        AttachMono_Reference_to_ScrObj();
        SaveScene();
        Debug.Log("==End_Rebuild_MonoCube==");
    }
    [MenuItem("UnityObjectTest/Rebuild_MonoCube_SEO")]
    public static void Rebuild_MonoCube_SEO(){
        Debug.Log("==Start_Rebuild_MonoCube_SEO==");
        GameObject g; while(g = GameObject.Find("MonoCube")) GameObject.DestroyImmediate(g);
        Create_MonoCube();
        monoCubeCache.AddComponent<ScriptExecutionOrderTest_0>();
        monoCubeCache.AddComponent<ScriptExecutionOrderTest_1>();
        SaveScene();
        Debug.Log("==End_Rebuild_MonoCube_SEO==");
    }
    [MenuItem("UnityObjectTest/Rebuild_MonoCube_Nest")]
    public static void Rebuild_MonoCube_Nest(){
        Debug.Log("==Start_Rebuild_MonoCube_Nest==");
        Destroy_All_ScrObj();
        GameObject g;
        while(g = GameObject.Find("MonoCube")) GameObject.DestroyImmediate(g);
        Create_ScrObj();
        Create_ScrObj_Asset();
        ScrObj_Set_IntValue();
        SetDirty_scrObj();
        SaveAssets();
        Create_MonoCube();
            monoCubeCache.name = "MonoCube1";
        AttachMono();
        AttachMono_Reference_to_ScrObj();

        GameObject top = monoCubeCache, temp = monoCubeCache;
        Create_MonoCube();
            monoCubeCache.name = "MonoCube2";
        AttachMono();
        AttachMono_Reference_to_ScrObj();
        monoCubeCache.transform.parent = temp.transform;
        temp = monoCubeCache;
        Create_MonoCube();
            monoCubeCache.name = "MonoCube3";
        AttachMono();
        AttachMono_Reference_to_ScrObj();
        monoCubeCache.transform.parent = temp.transform;

        monoCubeCache = top;
        // monoCubeCache = temp;

        SaveScene();
        Debug.Log("==End_Rebuild_MonoCube_Nest==");
    }
    [MenuItem("SEO/Attach_SEO0_to_MonoCube")]
    public static void Attach_SEO0_to_MonoCube(){
        Debug.Log("Attach_SEO0_to_MonoCube");
        Find_MonoCube();
        monoCubeCache.AddComponent<ScriptExecutionOrderTest_0>();
        Debug.Log("Attach_SEO0_to_MonoCube_End");
    }
    [MenuItem("UnityObjectTest/Exist_Object")] 
    public static void Exist_Object(){
        Debug.Log("=Exist_Object=");
        int count = Resources.FindObjectsOfTypeAll<AttachMono>().Length;
        AttachMono am = null;
        if(count != 0) am = Resources.FindObjectsOfTypeAll<AttachMono>()[0];
        Debug.Log($"AttachMono: Count: {count}\n HideFlags: {am?.hideFlags}");

        count = Resources.FindObjectsOfTypeAll<ScrObj>().Length;
        ScrObj so = null;
        if(count != 0) so = Resources.FindObjectsOfTypeAll<ScrObj>()[0];
        Debug.Log($"ScrObj: Count: {count}\n HideFlags: {so?.hideFlags}");

        Debug.Log($"MonoCube != null: {GameObject.Find("MonoCube") != null}\n HideFlags: {GameObject.Find("MonoCube")?.hideFlags}");
    }
    [MenuItem("UnityObjectTest/Set_HideFlags")]
    public static void Set_HideFlags(){
        Debug.Log("=Set_HideFlags=");
        // GameObject.Find("MonoCube").hideFlags = HideFlags.NotEditable; 
        // Resources.FindObjectsOfTypeAll<AttachMono>()[0].hideFlags = HideFlags.NotEditable;
        // Resources.FindObjectsOfTypeAll<ScrObj>()[0].hideFlags = HideFlags.DontUnloadUnusedAsset;
        scrObjCache.hideFlags = HideFlags.DontSave;
    }
    [MenuItem("UnityObjectTest/Destory")]
    public static void Destory(){
        Debug.Log("Destory");
        Object.Destroy(GameObject.Find("MonoCube"));
        // Object.Destroy(Resources.FindObjectsOfTypeAll<AttachMono>()[0]);
        // Object.Destroy(Resources.FindObjectsOfTypeAll<ScrObj>()[0]);
    } 
    [MenuItem("UnityObjectTest/DestroyImmediate")]
    public static void DestroyImmediate(){
        Debug.Log("DestoryImmediate");
        Object.DestroyImmediate(GameObject.Find("MonoCube"));
        // Object.DestroyImmediate(Resources.FindObjectsOfTypeAll<AttachMono>()[0]);
        // Object.DestroyImmediate(Resources.FindObjectsOfTypeAll<ScrObj>()[0], true); 
    }
    //AddObjectToAsset
    [MenuItem("Scriptable/CreateUniObj")]
    public static void CreateUniObj(){
        UniObj uniObj = new UniObj();
        Debug.Log($"(object)uniObj == null: {(object)uniObj == null}");
        AssetDatabase.CreateAsset(uniObj, "Assets/UniObj.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    //UnloadUnUsedAsset call

    static AttachMono attachMonoCache;
    [MenuItem("UnityObjectTest/AttachMono")]
    public static void AttachMono(){
        Debug.Log("AttachMono");
        // GameObject monoCube = GameObject.Find("MonoCube");
        attachMonoCache = monoCubeCache.AddComponent<AttachMono>();
        // attachMonoCache.hideFlags = HideFlags.DontSave; //これをするとPlayMode開始時、消える。多分DontSaveInEditorの効果でSceneに保存されていない
    }
    [MenuItem("UnityObjectTest/Destory_AttachMonoCache")]
    public static void Destory_attachMonoCache(){
        Debug.Log("Destory_attachMonoCache");
        GameObject.DestroyImmediate(attachMonoCache);
    }
    [MenuItem("UnityObjectTest/AttachMono_Set_Int")]
    public static void AttachMono_Set_Int(){
        Debug.Log("AttachMono_Set_Int");
        var temp = attachMonoCache.IntValue;
        attachMonoCache.IntValue += 10000;
        Debug.Log($"IntValue: {temp} -> {attachMonoCache.IntValue}");
    }
    [MenuItem("UnityObjectTest/Show_AttachMonoCache")]
    public static void Show_AttachMonoCache(){
        Debug.Log($"attachMonoCache.IntValue: {attachMonoCache.IntValue}\nattachMonoCache.clsObj: {attachMonoCache.clsObj != null}\nattachMonoCache.scrObj: {attachMonoCache.scrObj != null}");
        Debug.Log($"=attachMonoCache.scrObj.IntValue=: {attachMonoCache.scrObj.IntValue}");
        Debug.Log($"attachMonoCache != null: {attachMonoCache != null}");
        Debug.Log($"gameObject.hideFlags: {attachMonoCache.gameObject.hideFlags}\nattachMonoCache.hideFlags: {attachMonoCache.hideFlags}");
    }
    [MenuItem("UnityObjectTest/Destroy_MonoCube")]
    public static void Destroy_MonoCube(){
        GameObject monoCube = GameObject.Find("MonoCube");
        GameObject.DestroyImmediate(monoCube);
    }
    [MenuItem("UnityObjectTest/SaveAssets")]
    public static void SaveAssets(){
        Debug.Log("SaveAssets");
        AssetDatabase.SaveAssets();
    }
    [MenuItem("UnityObjectTest/SaveScene")]
    public static void SaveScene(){
        Debug.Log("SaveScene");
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
    [MenuItem("SceneManagement/LoadScene_New_Scene")]
    public static void LoadScene_New_Scene(){
        Debug.Log("LoadScene_New_Scene");
        UnityEngine.SceneManagement.SceneManager.LoadScene("New Scene");
    }
   [MenuItem("SceneManagement/LoadScene_New_Scene_Additive")]
    public static void LoadScene_New_Scene_Additive(){
        Debug.Log("LoadScene_New_Scene_Additive");
        UnityEngine.SceneManagement.SceneManager.LoadScene("New Scene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
   [MenuItem("SceneManagement/UnloadSceneAsync_New_Scene")]
    public static void UnloadSceneAsync_New_Scene(){
        Debug.Log("UnloadSceneAsync_New_Scene");
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("New Scene");
    }
   [MenuItem("SceneManagement/UnloadSceneAsync_SampleScene")]
    public static void UnloadSceneAsync_SampleScene(){
        Debug.Log("UnloadSceneAsync_SampleScene");
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("SampleScene");
    }
    public static GameObject monoCubeCache;
    [MenuItem("UnityObjectTest/Create_MonoCube")]
    public static void Create_MonoCube(){
        Debug.Log("Create_MonoCube");
        monoCubeCache = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monoCubeCache.name = "MonoCube";
        // Debug.Log($"monoCubeCache.GetComponent<BoxCollider>().bounds: {monoCubeCache.GetComponent<BoxCollider>().bounds}");
        // monoCubeCache.hideFlags = HideFlags.DontSave;
        EditorUtility.SetDirty(monoCubeCache); //SetDirtyしないとSceneの変更が認識されない
        SaveScene(); //SetDirtyしなくても確実に保存する
    }
    public static ScrObj1 scrObj1Cache;
    [MenuItem("UnityObjectTest_1/Create_ScrObj1_and_CreateAsset")]
    public static void Create_ScrObj1_and_CreateAsset(){
        Debug.Log("Create_ScrObj1_and_CreateAsset");
        // ScrObj1 scrObj1Cache;
        scrObj1Cache = ScriptableObject.CreateInstance<ScrObj1>();
        AssetDatabase.CreateAsset(scrObj1Cache, "Assets/ScrObj1.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    [MenuItem("UnityObjectTest_1/Create_ScrObj1_SetHideFlags")]
    public static void Create_ScrObj1_SetHideFlags(){
        Debug.Log("Create_ScrObj1_SetHideFlags");
        scrObj1Cache.hideFlags = HideFlags.DontSave;
        // scrObj1Cache.hideFlags = HideFlags.DontUnloadUnusedAsset;
        // scrObj1Cache.hideFlags = HideFlags.DontSaveInEditor;
    } 
    
    public static int createNum = 0;
    public static ScrObj scrObjCache;
    [MenuItem("UnityObjectTest/Create_ScrObj")]
    public static void Create_ScrObj(){
        Debug.Log("Create_ScrObj");
        scrObjCache = ScriptableObject.CreateInstance<ScrObj>();
        scrObjCache.name = "ScrObj_" + createNum.ToString(); ++createNum;
    }
    [MenuItem("UnityObjectTest/Show_ScrObjCache")]
    public static void Show_ScrObjCache(){
        Debug.Log($"scrObjCache.IntValue: {scrObjCache.IntValue}");
        Debug.Log($"scrObjCache.hideFlags: {scrObjCache.hideFlags}");
    }
    [MenuItem("UnityObjectTest/ScrObj_Set_IntValue")]
    public static void ScrObj_Set_IntValue(){
        var temp = scrObjCache.IntValue;
        scrObjCache.IntValue += 1111;
        // EditorUtility.SetDirty(scrObjCache);
        Debug.Log($"IntValue: {temp} -> {scrObjCache.IntValue}");
    }
    [MenuItem("UnityObjectTest/Null_scrObjCache")]
    public static void Null_scrObjCache(){
        Debug.Log("Null_scrObjCache");
        scrObjCache = null;
    }
    [MenuItem("UnityObjectTest/Find_ScrObj")]
    public static void Find_ScrObj(){
        ScrObj[] scrObj_Array = Resources.FindObjectsOfTypeAll<ScrObj>();
        if(scrObj_Array.Length == 0){Debug.Log($"scrObj_Array.Length: {scrObj_Array.Length}");return;}
        scrObjCache = scrObj_Array[0];
        Debug.Log($"scrObj_Array.Length: {scrObj_Array.Length}\nscrObjCache = scrObj_Array[0];");
    }
    [MenuItem("UnityObjectTest/Find_MonoCube")]
    public static void Find_MonoCube(){
        monoCubeCache = GameObject.Find("MonoCube");
        Debug.Log($"monoCubeCache.name: {monoCubeCache.name}");
    }
    [MenuItem("UnityObjectTest/Find_AttachMono")]
    public static void Find_AttachMono(){
        AttachMono[] attachMono_Array = Resources.FindObjectsOfTypeAll<AttachMono>();
        if(attachMono_Array.Length == 0){Debug.Log($"attachMono_Array.Length: {attachMono_Array.Length}");return;}
        attachMonoCache = attachMono_Array[0];
        Debug.Log($"attachMono_Array.Length: {attachMono_Array.Length}\nattachMonoCache = attachMono_Array[0];");
    }
    [MenuItem("UnityObjectTest/Destroy_All_ScrObj")]
    public static void Destroy_All_ScrObj(){
        ScrObj[] scrObj_Array = Resources.FindObjectsOfTypeAll<ScrObj>();
        Debug.Log($"{scrObj_Array.Length}個のScrObjをDestroy");
        foreach(ScrObj so in scrObj_Array){
            Object.DestroyImmediate(so, true);
        }
    }
    public static SerializedObject seriObjCache;
    [MenuItem("SerializedObjectTest/Create_SeriObj")]
    public static void Create_SeriObj(){
        seriObjCache =  new SerializedObject(scrObjCache);
    }
    
    [MenuItem("SerializedObjectTest/Show_SeriObjCache_IntValue")]
    public static void Show_SeriObjCache_IntValue(){
        Debug.Log($"SeriObjCache.FindProperty(\"IntValue\").intValue: {seriObjCache.FindProperty("IntValue").intValue}");
    }
    [MenuItem("SerializedObjectTest/SeriObj_Set_IntValue")]
    public static void SeriObj_Set_IntValue(){
        Debug.Log("=Start_Update=");
        seriObjCache.Update();      //Serialize
        Debug.Log("=End_Update=");
        var temp = seriObjCache.FindProperty("IntValue").intValue;
        seriObjCache.FindProperty("IntValue").intValue += 11;
        Debug.Log("=Start_ApplyModifiedProperties=");
        seriObjCache.ApplyModifiedProperties();     //Serialize -> Deserialize -> Serialize
        Debug.Log("=End_ApplyModifiedProperties=");
        Debug.Log($"IntValue: {temp} -> {seriObjCache.FindProperty("IntValue").intValue}");
    }
    [MenuItem("UnityObjectTest/UnloadUnusedAsset")]
    public static void UnloadUnusedAssets(){
        Debug.Log("UnloadUnusedAssets");
        Resources.UnloadUnusedAssets();
    }
    [MenuItem("UnityObjectTest/Create_ScrObj_Asset")]
    public static void Create_ScrObj_Asset(){
        Debug.Log("Create_ScrObj_Asset");
        AssetDatabase.CreateAsset(scrObjCache, "Assets/ScrObj.asset");
    }
    [MenuItem("UnityObjectTest/AttachMono_Reference_to_ScrObj")]
    public static void AttachMono_Reference_to_ScrObj(){
       monoCubeCache.GetComponent<AttachMono>().scrObj = scrObjCache;
    }
    [MenuItem("UnityObjectTest/AttachMono_Reference_to_ClsObj")]
    public static void AttachMono_Reference_to_ClsObj(){
       monoCubeCache.GetComponent<AttachMono>().clsObj = new ClsObj(){IntValue = 4444};
    }
    [MenuItem("UnityObjectTest/LoadAssetAtPath_ScrObj")]
    public static void LoadAssetAtPath_ScrObj(){
        Debug.Log("LoadAssetAtPath_ScrObj");
        scrObjCache = AssetDatabase.LoadAssetAtPath<ScrObj>("Assets/ScrObj.asset");
    }
    [MenuItem("UnityObjectTest/ImportAsset_ScrObj")]
    public static void ImportAsset_ScrObj(){
        Debug.Log("ImportAsset_ScrObj");
        AssetDatabase.ImportAsset("Assets/ScrObj.asset"); //パスからUnityEngine.ObjectをLoadする。触れた時にロードするのと多分同じ
        // AssetDatabase.Refresh(); //Loadしなかった
    }
    public static ScrObj_Ref scrObj_RefCache;
    [MenuItem("UnityObjectTest_1/Circular_Reference")]
    public static void Circular_Reference(){
        Debug.Log("Circular_Reference");
        ScrObj_Ref temp = ScriptableObject.CreateInstance<ScrObj_Ref>();
        temp.scrObj_ref = ScriptableObject.CreateInstance<ScrObj_Ref>();
        temp.scrObj_ref.scrObj_ref = temp;  //循環参照したUnityObjectでもしっかりUnloadUnusedAssetsで消える
        // scrObj_RefCache = temp;
        // temp.hideFlags = HideFlags.DontUnloadUnusedAsset;
        // temp.scrObj_ref.hideFlags = HideFlags.DontUnloadUnusedAsset; //片方がStatic や DontUnloadUnusedAssetに捕まった時は消せない
    }
    [MenuItem("UnityObjectTest_1/Exist_ScrObj_Ref")] 
    public static void Exist_ScrObj_Ref(){
        Debug.Log("=Exist_ScrObj_Ref=");
        int count = Resources.FindObjectsOfTypeAll<ScrObj_Ref>().Length;
        ScrObj_Ref sr = null;
        if(count != 0) sr = Resources.FindObjectsOfTypeAll<ScrObj_Ref>()[0];
        Debug.Log($"ScrObj_Ref: Count: {count}\n HideFlags: {sr?.hideFlags}");
    }
    [MenuItem("UnityObjectTest_1/Destory_ScrObj_Ref")]
    public static void Destory_ScrObj_Ref(){
        Debug.Log($"Destory_ScrObj_Ref\nScrObj_Refの数: {Resources.FindObjectsOfTypeAll<ScrObj_Ref>().Length}");
        foreach(ScrObj_Ref so_r in Resources.FindObjectsOfTypeAll<ScrObj_Ref>()){
            Object.DestroyImmediate(so_r);
        }
    }
    [MenuItem("SerializedObjectTest/IsDirty")]
    public static void IsDirty(){
        Debug.Log($"IsDirty(monoCubeCache): {EditorUtility.IsDirty(monoCubeCache)}");
        Debug.Log($"IsDirty(attachMonoCache): {EditorUtility.IsDirty(attachMonoCache)}");
        Debug.Log($"IsDirty(scrObjCache): {EditorUtility.IsDirty(scrObjCache)}");
        // EditorUtility.IsDirty(seriObjCache);
    }
    [MenuItem("SerializedObjectTest/SetDirty_monoCube")]
    public static void SetDirty_monoCube(){
        Debug.Log("SetDirty_monoCube");
        EditorUtility.SetDirty(monoCubeCache);
    }
    [MenuItem("SerializedObjectTest/SetDirty_attachMono")]
    public static void SetDirty_attachMono(){
        Debug.Log("SetDirty_attachMono");
        EditorUtility.SetDirty(attachMonoCache);
    }
    [MenuItem("SerializedObjectTest/SetDirty_scrObj")]
    public static void SetDirty_scrObj(){
        Debug.Log("SetDirty_scrObj");
        EditorUtility.SetDirty(scrObjCache);
    }
    [MenuItem("SerializedObjectTest/SetDirty_Main_Camera")]
    public static void SetDirty_Main_Camera(){
        Debug.Log("SetDirty_Main_Camera");
        EditorUtility.SetDirty(GameObject.Find("Main Camera"));
    }
    [MenuItem("SerializedObjectTest/GCCollect")]
    public static void GCCollect(){
        Debug.Log("System.CG.Collect() x10");
        for(int i = 0; i < 10; i++)
            System.GC.Collect();
    }
    public static GCHandle gcHandleCache;
    [MenuItem("GCHandleTest/Create_GCHandle")]
    public static void Create_GCHandle(){
        Debug.Log("Create_GCHandle");
        gcHandleCache = GCHandle.Alloc(scrObjCache);
    }
    [MenuItem("GCHandleTest/scrObj_Set_Form_GCHandle")]
    public static void scrObj_Set_Form_GCHandle(){
        Debug.Log("scrObj_Set_Form_GCHandle");
        var temp = scrObjCache.IntValue;
        ((ScrObj)gcHandleCache.Target).IntValue += 10000;
        EditorUtility.SetDirty(scrObjCache);
        Debug.Log($"IntValue: {temp} -> {scrObjCache.IntValue}");
    }
    public static ClsObj clsObjCache;
    public static ClsObjNonSeri clsObjNonSeriCache;
    public static UniObj uniObjCache;
    [MenuItem("GCHandleTest/Create_ClsObj_ClsObjNonSiri_UniObj")]
    public static void Create_ClsObj_ClsObjNonSiri_UniObj(){
        Debug.Log("Create_ClsObj_ClsObjNonSiri_UniObj");
        // ClsObj clsObjCache;ClsObjNonSeri clsObjNonSeriCache;UniObj uniObjCache;  //HeapExplorerではルートに参照されていないオブジェクトは表示されない(GC.Collectで即効消される?)
        clsObjCache = new ClsObj();                 //[System.Serializable]関係なしに普通のClassオブジェクト GCHandleもなし
        clsObjNonSeriCache = new ClsObjNonSeri();   //↑と全く同じ
        uniObjCache = new UniObj(); //m_CachedPtr: null, m_InstanceID: 0, GCHandleもなし　完全にUnityObjectとして無効なオブジェクト
    }
    [MenuItem("GCHandleTest/UnloadUnusedAssets_In_Stack")]
    public static void UnloadUnusedAssets_In_Stack(){
        /*ScrObj scrObj =*/ ScriptableObject.CreateInstance<ScrObj>();
        Resources.UnloadUnusedAssets();
        Thread.Sleep(1000);
        // scrObjCache = scrObj;
        // Debug.Log($"scrObj != null: {scrObj != null}"); //=>True スタックにある場合もUnloadされないようだ?しかし、Debug.Logの後に[SO_OnDisable]が出て消えてるのでコマンドを後で実行してるだけ?
        Debug.Log($"FindObjectsOfTypeAll<ScrObj>()[0] != null: {Resources.FindObjectsOfTypeAll<ScrObj>()[0] != null}");//=>True
    }
}

public class ScrObj : ScriptableObject, ISerializationCallbackReceiver{
    public int IntValue;
    public void Awake(){
        Debug.Log("[SO_Awake]");
    }
    public void OnEnable(){
        Debug.Log("[SO_OnEnable]");
    }
    public void OnDisable(){
        Debug.Log("[SO_OnDisable]");
    }
    public void OnDestroy(){
        Debug.Log("[SO_OnDestroy]"); 
    }
    public void OnBeforeSerialize(){
        if(!ScrObj_Method.silentFlag) Debug.Log("[SO_OnBeforeSerialize]"); 
    }
    public void OnAfterDeserialize(){
        Debug.Log("[SO_OnAfterDeserialize]");
    }
}
public class ClsObj_Ref{
    public ScrObj_Ref scrObj_ref;
    public ClsObj_Ref clsObj_Ref;

    public void Awake(){
        Debug.Log("[CO_Ref_Awake]");
    }
    public void OnEnable(){
        Debug.Log("[CO_Ref_OnEnable]");
    }
    public void OnDisable(){
        Debug.Log("[CO_Ref_OnDisable]");
    }
    public void OnDestroy(){
        Debug.Log("[CO_Ref_OnDestroy]"); 
    }
}
public class UniObj : UnityEngine.Object{
    public int IntValue;
}
