# AssetDataBase

## 概要

- AssetDataBaseは、**C++Object**の**Assetデータ**と**Serializeデータ**を**ディスクに保存**するために、**Asset**と言う形式で保存します
- 主に**Scene内**のオブジェクト(**メモリ側**)と**Projectフォルダ**(**ディスク側**)の間で**Assetのセーブとロードを管理**します
- Assetは2つのファイルで構成され、Assetデータは**Assetデータファイル(各拡張子)**に保存、Serializeデータは**Serializeデータファイル(.meta)**に保存される
- SceneファイルやScriptableObjectなど**Unity固有**の生成ファイルは**Assetデータファイル側**にも**Serializeデータ**が**保存**されることもある
- AssetDataBaseの**スクリプトによる操作以外**にEditorのProjectフォルダのフォルダを開くなどで**Assetがロード**される事もあります

## メソッド

- `CreateAsset(UnityObject, ＄Path＝❰"パス" + "ファイル名" + "拡張子"❱)`
  - 概要の説明の通り、**C++Object**の**Assetデータ**と**Serializeデータ**を**ディスクに保存**します
    保存するために引数には**ファイル名 と 拡張子**も必要です

- `LoadAssetAtPath<｢出力型｣>(∫Path∫)`
  - Assetファイルから**Assetデータ**と**Serializeデータ**を**C++Object**へ**ロード**します
    SubAssetの名前でロードはできない`nullが返る`とでる

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