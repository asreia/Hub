# SceneManagement名前空間

## 概要

[SceneManagement](\画像\SceneManagement.drawio.png)
[シーングラム](画像\シーングラム.drawio.png)
[Scene間の参照](\画像\Scene間の参照.drawio.png)
[Prefabワークフロー](画像\Prefabワークフロー.drawio.png)

- SceneManagementは、主に、Scene(に**属する全てのGameObjectと全てのComponent**)の**ロード、アンロード、ディスクへの保存**を管理します
- **C#スクリプト上のScene**は、構造体であり**UnityObjectではありません**。
- SceneはそのSceneに属する **(Root)GameObjectへアクセス**するための**メソッド**を持っています(`GetRootGameObjects()`)
- Projectフォルダ **(ディスク)に保存**されるSceneファイルは、**.unity**と.metaで、C#スクリプト上のSceneはUnityObjectではないので、
SceneのC++ObjectのSerializeデータが保存される訳ではありません(そもそも無い)
Sceneファイルの **.unity**には、そのSceneに**属する全てのGameObjectと全てのComponent**の、C++Objectの**Serializeデータが保存**されます(`SaveScene()`)(あとScene単位で関係する情報もある)
- Sceneの**ロード**は、**.unity**から、そのSceneに**属する全てのGameObjectと全てのComponent**を**メモリ上にロード**(`LoadScene()`)しUnityに管理されます(PlayerLoop(ゲームループ)など)
- Sceneの**アンロード**は、Sceneに**属する全てのGameObject**に対して**Destroy()を呼び**(多分)、
GameObjectに`Destroy()`を呼んだ場合は、そのGameObjectに属している**全てのComponentに対して**も`Destroy()`が呼ばれます
そして、Scene自体も**Hierarchyから消える様な処理**をしている(IsValid,isLoadedをFalseにするとか?)と思います(`UnloadSceneAsync()`)

## メソッド

- `LoadScene(＄Scene＝⟪string sceneName¦int sceneBuildIndex⟫, ⟪LoadSceneMode.Single¦LoadSceneMode.Additive⟫)` `[UnityEngine]`
  - LoadSceneは**Sceneをロード**します(詳細は概要)
  - Sceneをスクリプトからロードするには、Editorの ファイル -> ビルド設定…(**Build Settings**が開く) -> "ビルドに含まれるシーン" にSceneファイル **(.unity)をD&Dで設定**する必要がある
  - **ロードするScene**をメソッドで**指定**するには.unityの**ファイル名**(`string sceneName`) か "ビルドに含まれるシーン"の右端に書いてある数字(`int sceneBuildIndex`)を指定します
  - **LoadSceneMode**は列挙型であり、**ロードする時の挙動**を変えます
    - **Single**は、現在ロードされている全てのSceneを**アンロードし**、指定されたSceneを**ロード**します
    - **Additive**は、現在ロードされている全てのSceneを**アンロードせず**、指定されたSceneを**追加でロード**します
      - **async/await?とusingステートメント**でusingの時LoadしDisposeの時Unloadし参照を外すとかいいかも？

- `UnloadSceneAsync(⟪∫Scene∫¦Scene scene⟫)` `[UnityEngine]`
  - UnloadSceneAsyncは、Sceneを非同期でアンロードします(詳細は概要)
  - 注意
    - Sceneをアンロードする前に、そのSceneに**属する全てのGameObjectと全てのComponent**に対する**参照を外して**ください([Unityでの複数シーンを使ったゲームの実装方法とメモリリークについて](https://madnesslabo.net/utage/?page_id=11109))

- `SaveScene(Scene scene)` `[UnityEditor]`
  - Sceneを **.unity(ディスク)に保存**します(**UnityObjectのみ**を**編集**して**Dirtyが付いていなくても保存**されます)(詳細は概要)
    - AssetDatabaseの`SaveAssets()`では**保存されません**。

- `Scene GetActiveScene()` `[UnityEditor]`
  - 現在**アクティブなSceneを返します**。アクティブなSceneはPlayer再生中にいろいろ恩恵があります(あと操作の対象とかあった気が吸う)

## Scene構造体 (非UnityObject)

- Scene ([↓スクリプトリファレンスからコピペ](https://docs.unity3d.com/ja/2019.1/ScriptReference/SceneManagement.Scene.html))
  - 変数
    - buildIndex
      - >ビルド設定でシーンのインデックスを返します。
    - isDirty
      - >シーンが変更された場合はtrueを返します。
    - isLoaded
      - >シーンがロードされている場合はtrueを返します。
    - name
      - >ゲームまたはアプリで現在アクティブなシーンの名前を返します。
    - path
      - >シーンの相対パスを返します。(例: Assets/MyScenes/MyScene.unity)
    - rootCount
      - >このシーンのルートtransformの数。
  - Public 関数
    - **GetRootGameObjects**
      - >シーン内のすべてのルートゲームオブジェクトを返します。
    - IsValid
      - >これが有効なシーンであるかどうか。たとえば、存在しないシーンを開こうとした場合、シーンが無効になる可能性があります。
      この場合、EditorSceneManager.OpenSceneから返されたシーンはIsValidに対してFalseを返します。
