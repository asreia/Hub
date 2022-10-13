# AssetDatabase `[UnityEditor]`

## 概要

- AssetDatabaseは、**C++Object**の**Assetデータ**と**Serializeデータ**を**ディスクに保存**するために、**Asset**として保存します
- 主に**Scene内**のオブジェクト(**メモリ側**)と**Projectフォルダ**(**ディスク側**)の間で**Assetのセーブとロードを管理**します
- Assetは2つのファイルで構成され、Assetデータは**Assetデータファイル(各拡張子)**に保存、Serializeデータは**Serializeデータファイル(.meta)**に保存される
- SceneファイルやScriptableObjectなど**Unity固有**の生成ファイルは**Assetデータファイル側**にも**Serializeデータ**が**保存**されることもある
- AssetDatabaseの**スクリプトによる操作以外**にEditorのProjectフォルダのフォルダを開くなどで**Assetがロード**される事もあります

## メソッド

- `CreateAsset(UnityObject, ＄Path＝❰"パス" + "ファイル名" + "拡張子"❱)`
  - 概要の説明の通り、**C++Object**の**Assetデータ**と**Serializeデータ**を**ディスクに保存**します
    保存するために引数には**ファイル名 と 拡張子**も必要です

- `UnityObject LoadAssetAtPath<｢出力型｣>(∫Path∫)`
  - Assetファイルから**Assetデータ**と**Serializeデータ**を**C++Object**へ**ロード**します
    SubAssetの名前でロードはできない`nullが返る`とでる。SubAssetの型で指定すると取得できる?

- `SaveAssets()`
  - **SceneのComponent以外**の、(Componentは**SaveScene**で**Sceneファイル**(.unity)に**保存**される為)
    **Dirty**が付いた全てのC++Objectの**未保存**な**Serializeデータ**を**Serializeデータファイル**(もしくはAssetデータファイル側)に**保存**します
    主にスクリプトでC#側の**UnityObjectのみ更新**した場合**Dirtyが付かず**`SaveAsset()`しても保存されない場合があります。(SetDirty(~)する必要がある)
  - **Dirty(Class:EditorUtility)**
    - 概要
      - Dirtyは、C++Objectの**Serializeデータ**が更新されて、まだ`SaveAssets()`を実行して**保存されてない**事を示す**フラグ**(bool)である
      - フラグが**True**だと**未保存**の状態でそこに`SaveAssets()`が実行されると**False**に変わります
      - 注意
        - UnityObjectの**Serialize可能な変数のみ**を**変更**した場合、Dirtyが**Trueになりません**
        ので、この状態で`SaveAssets()`を実行してもSerializeデータファイルに**保存されません**
        この場合**SetDirty()**を呼ぶことでDirtyを**True**にし`SaveAssets()`を呼び出すと**保存されます**
    - メソッド
      - `SetDirty(UnityObject)`
      UnityObjectの**DirtyフラグをTrue**にします
      - `bool IsDirty(UnityObject)`
      UnityObjectの**Dirtyフラグ**の**値(bool)を返します**

- `ImportAsset(∫Path∫)`
  - **ディスクからメモリ側にロード**しますがScene内などユーザースクリプト側の**変数に代入されず**、Unityのシステムの中のみ管理されます
    Assetファイルを編集して**再読み込み**するなどに使います
    SubAssetの名前でロードはできない`does not exist`とでる

- `DeleteAsset(∫Path∫)`
  - 注意: **C++Object側も破棄**されるためC#の参照を外す事
  - **ディスクから**∫Path∫のAssetファイルを**削除**します。その時**C++Object側も破棄**されます
    **SubAssetのあるパス**を破棄すると**全て**のSubAssetのC++Objectも**破棄**される

- `AddObjectToAsset(UnityObject, ∫Path∫)`
  - 常に存在しているAssetのパスに、**SubAssetとして追加**します
    なぜか`SaveAssets()`しないとディスクに**保存されない**?みたい
    [SubAssetの取り扱いまとめ](https://light11.hatenadiary.com/entry/2018/12/30/230123)

- `Refresh()`

  - >コンテンツ内容が何かしら変更されたAssetや、Projectフォルダーに追加/削除されたAssetをインポートします。
  
    しかし、`ImportAsset(~)`とは挙動が違うみたい

  - ↓恐らくこれのこと?。これだとしたらAPIで呼ん度もこれが出ないなら更新済みで必要ないと認識されている?
    [アセットデータベースの更新](https://docs.unity3d.com/ja/2022.1/Manual/AssetDatabaseRefreshing.html)
    ![Refresh](画像\Refresh.png)

## アセットデータベースの動作

- プロジェクトのLibraryフォルダにある**SourceAssetDB**と**ArtifactDB**を合わせて**アセットデータベース**と言うらしい
  - >**SourceAssetDB**はソースアセットファイルに関する**メタ情報**(最終更新日、ファイルコンテンツのハッシュ、GUID)これに基づいて、
    Unity は**ファイルが変更されているかどうか**、その結果ファイルを再インポートするかどうかを判断します。
  - >**ArtifactDB**は各ソースアセットの**インポート結果に関する情報**(インポートの依存関係情報、メタ情報、ファイルのリスト)が含まれています。
- >Unity はアセットデータベースの更新中に以下の手順を実行します。(Refresh?)
  1. >アセットファイルに対する変更を探し、ソースアセットデータベース(`Library\SourceAssetDB`)を更新します。
  2. >.dll、.asmdef、.asmref、.rsp、.cs ファイルなどのコード関連ファイルをインポートして**コンパイル**します。
  3. >スクリプトから Refresh が呼び出されなかった場合は、**ドメインをリロード**します。
  4. >インポートされたコード関連ファイルのすべてのアセットを後処理します。
  5. >次に、 **コードに関連しないアセットをインポート**し、インポートしたアセットの残りすべてを後処理します。
  6. >その後、アセットを **ホットリロード** します。

- [AssetDatabase V2とは](https://www.youtube.com/watch?v=ldpu3pzUsaM)
  ![AssetDatabaseV2_Import](画像\AssetDatabaseV2_Import.png)

- 過去のメモ
  - AssetDatabaseV2
    AssetはImportされると、Library/**Artifactsにインポート結果を保存**する(Import時にソースファイルを**使いやすい様に変換**する)
    LoadAssetAtPathなどで取得したAssetは変換したインポート結果のデータをC++側に持つ。
    AssetDatabaseV2では、一つのGUIDでSwitchPlatformやインポートなどの設定により**複数のインポート結果を持てる**。(設定を変える事によりGUIDが指すインポート結果が切り替わる)
  - アセットデータベースの更新
    SourceAssetDBで変更を検知し自身を更新する -> コード関連のアセットをImportしドメインリロード -> コード以外のアセットをImport
    - コード以外のアセットはファイルの拡張子にそれぞれ対応したインポーターが起動する。Unity組み込みのNative Importers の後に**Scripted Importers**を処理する
      - >Scripted Importer は、すでに Unity によってネイティブに処理されているファイル拡張子を処理できません。
    - (追記)あとインポート処理前後にコールバックを呼び出す。(On**Pre**processTexture, On**Post**processTexture など)
  - AssetDatabase によるバッチ処理
    - AssetDatabaseによるファイル操作を複数並べると一つ一つにオーバーヘッドが生じる。
      **StartAssetEditingとStopAssetEditingで囲む**とそれを抜けた後にバッチ処理しオーバーヘッドを最小限にできる
      ↑はカウント方式でStartでインクリメントしStopでデクリメントし0になった時にバッチ処理を実行する。こうする事で関数呼び出しなどにより**ネストした状態になっても正しく機能する**。

## [AssetDatabase を使ったファイル操作](https://docs.unity3d.com/ja/2022.1/Manual/AssetDatabase.html)

```CSharp
public class AssetDatabaseIOExample {
    [MenuItem ("AssetDatabase/FileOperationsExample")]
    static void Example ()
    {
        string ret;
        
        // 作成
        Material material = new Material (Shader.Find("Specular"));
        AssetDatabase.CreateAsset(material, "Assets/MyMaterial.mat");
        if(AssetDatabase.Contains(material))
            Debug.Log("Material asset created");
        
        // 名前変更
        ret = AssetDatabase.RenameAsset("Assets/MyMaterial.mat", "MyMaterialNew");
        if(ret == "")
            Debug.Log("Material asset renamed to MyMaterialNew");
        else
            Debug.Log(ret);
        
        // フォルダーの作成
        ret = AssetDatabase.CreateFolder("Assets", "NewFolder");
        if(AssetDatabase.GUIDToAssetPath(ret) != "")
            Debug.Log("Folder asset created");
        else
            Debug.Log("Couldn't find the GUID for the path");
        
        // 移動
        ret = AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(material), "Assets/NewFolder/MyMaterialNew.mat");
        if(ret == "")
            Debug.Log("Material asset moved to NewFolder/MyMaterialNew.mat");
        else
            Debug.Log(ret);
        
        // コピー
        if(AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(material), "Assets/MyMaterialNew.mat"))
            Debug.Log("Material asset copied as Assets/MyMaterialNew.mat");
        else
            Debug.Log("Couldn't copy the material");
        //変更をしらせるために手動でデータベースを最新に更新する 
        AssetDatabase.Refresh();
        Material MaterialCopy = AssetDatabase.LoadAssetAtPath("Assets/MyMaterialNew.mat", typeof(Material)) as Material;
        
        // トラッシュに移動
        if(AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(MaterialCopy)))
            Debug.Log("MaterialCopy asset moved to trash");
        
        // 削除
        if(AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(material)))
            Debug.Log("Material asset deleted");
        if(AssetDatabase.DeleteAsset("Assets/NewFolder"))
            Debug.Log("NewFolder deleted");
        
        // すべての変更後に AssetDatabase を更新
        AssetDatabase.Refresh();
    }
}
```
