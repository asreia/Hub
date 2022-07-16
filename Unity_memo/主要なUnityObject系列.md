# 主要なUnityObject系列

## Object

### 概要

Objectは、**System.Objectではなく**、**それを継承**した**UnityEngin.Object**である。UnityEngin.Objectは名前が長いので個人的に**UnityObject**と呼ぶことにする
UnityObjectは**Unityのシステムの中**に **(各部分に)シングルトン**として生成され、そのインスタンスの**対**として**C++側のオブジェクト**が生成され、
そのC++側のオブジェクト(以後C++Object)とそのUnityObject(instance)間の通信は、
UnityObject(instance) -> C++Object は、UnityObjectのフィールドに**m_CachedPtr**がありそこからC++Objectに通信して
C++Object -> UnityObject(instance) は、C++Objectのフィールドにある**GCHandle**で通信している。と思われる
C++Objectには、**シリアライズデータ**、**InstanceID**、などがある

- 注意事項
  - nullなのにnullじゃない問題
  UnityObjectを引数にDestroy＠❰Immediate❱を呼び**Destroyが実行**されると即座に**C++Objectが破棄**されますが、
  UnityObjectのC#側に**C#の参照関係**が残っているとC#はGCでそのUnityObjectを**破棄してくれません**が、
  UnityObjectで**オーバーライドされた等価演算子(!=,==)**で、`UnityObject(instance) == null`は、**True**が返ってしまいます
  そのUnityObjectが大きいオブジェクト(テクスチャなど)を参照していた場合は**C#の参照関係**が残っているので破棄されません

### メソッド

## Component

## Behaviour
