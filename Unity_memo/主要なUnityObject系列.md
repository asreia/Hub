# 主要なUnityObject系列

## Object (UnityEngine.Object, UnityObject)

### 概要

![UnityObject操作](\画像\UnityObject操作.drawio.png)
![UnityObject_Destroy](\画像\UnityObject_Destroy.drawio.png)
![ドメインリロード](画像\ドメインリロード.drawio.png)

- Objectは、**System.Objectではなく**、**それを継承**した**UnityEngin.Object**である。UnityEngin.Objectは名前が長いので個人的に**UnityObject**と呼ぶことにする
- UnityObjectは**Sceneかそれ以外のUnityのシステムの中**に生成され、そのインスタンスの**対**として**C++側のオブジェクト**が生成される
- そのC++側のオブジェクト(以後**C++Object**)とその**UnityObject(instance)**の間の通信は、
  UnityObjectのフィールドの**m_CachedPtr**: UnityObject(instance) -> C++Object
  C++Objectのフィールドの  **GCHandle**   : C++Object -> UnityObject(instance)
  で通信している。と思われる
- **C++Object**は、UnityObject以外にも**Assetファイル**への通信(AssetDatabase)と、**SerializeObject**への通信をしている
- C++Objectには、**Assetデータ**、**シリアライズデータ**、**InstanceID**、などがある
- UnityObjectは全て、**Inspectorに表示可能**である(多分)
- 注意
  - nullなのにnullじゃない問題
  UnityObjectを引数にDestroy＠❰Immediate❱を呼び**Destroyが実行**されると即座に**C++Objectが破棄**されますが、
  UnityObjectであるC#側に**C#の参照関係**が残っているとC#の**GCの対象にならない**為そのUnityObjectを**破棄してくれません**そして、
  UnityObjectで**オーバーライドされた等価演算子(!=,==)**で、`UnityObject(instance) == null`は、**true**が返ってしまいます(参照関係がないと見える)
  そのUnityObjectが大きいオブジェクト(テクスチャなど)を参照していた場合は**C#の参照関係**が残っているので**破棄されません**

### メソッド

- `internal static Object FindObjectFromInstanceID(int instanceID)`: なんか発見した。`internal`だから反社が必要

## Component

## Behaviour
