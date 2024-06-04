# Script関係

![Objectライフサイクル](\画像\Objectライフサイクル.drawio.png)
[Unity スクリプト実行順の謎](https://ameblo.jp/sugawara-monolizm/entry-11889665729.html)

## 補足

- private void Update() の様なコールバックは**Unityが**privateでも**関数ポインタ**として登録し**呼び出し**ている(多分Cecilがやってる)
- 定義可能なモノのリスト
  - .unity (Scene)
  - Prefab (GameObject)
  - MonoBehaviour (Component)
  - ScriptableObject (Asset)

## MonoBehaviour

### 概要

MonoBehaviourは、必ずGameObjectにアタッチして**Componentとして振る舞い**、UnityObjectとして生成されます
**ゲームループ**の特定のタイミングで呼び出される**コールバックが多く**定義されており、主にランタイム時に**Component間で情報をやり取り**しゲームを制御します

- **注意事項**
  - ファイル名とクラス名が**同じでない**とGameObjectに**アタッチ時エラー**になる

### コールバック

### メソッド

## ScriptableObject

### 概要

ScriptableObjectは、**ScriptableなObject**であり**Assetとして振る舞い**UnityObjectして生成し**一つのモジュールシステム**として運用されます
- [CreateAssetMenu(menuName = "SelfMadeSRP_Asset/SRP0", fileName = "SRP0_Asset_File")] (とりあえずコピペメモ)

- **注意事項**
  - ファイル名とクラス名が**同じでない**とCreateAsset時警告がでる
  No script asset for ScrObj. Check that the definition is in a file of the same name and that it compiles properly.

### コールバック

### メソッド

- **静的**メソッド
  - `CreateInstance<T>()`
  **ScriptableObject**を継承したT型のインスタンスを**生成**する
  **new T()**では生成されても**Unityに管理されない**(コールバックが呼ばれない)
