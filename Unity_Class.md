## Editor  
- Attribute  
- UnityEngin.Object (18)  
  - UnityEditor.AssetImporter (8)  
  - UnityEditorInternal.ProjectSettingsBase (9)  
- AssetImporterEditor (8)  
- UnityEditor.AssetPostprocessor (7)  
- UnityEditor.Build.IOrderedCallback (13)  
- UnityEditor.GUIDrawer (2)  
  - UnityEditor.PropertyDrawer (12)  
- UnityEditor.PopupWindowContent (21)  
- UnityEditor.SceneManagement.PrefabOverride (4)  
- UnityEditor.EditorWindow (63)  
- UnityEditor.IHasCustomMenu (16)  
- (UnityEngine.)ScriptableObject (32)  
  - UnityEditor.EditorTools.EditorTool (13)  
  - UnityEditor.ProjectWindowCallback.EndNameEditAction(11)  
  - UnityEditor.Editor (96)  
    - UnityEditor.RendererEditorBase (6)  
    - UnityEditorInternal.ProjectSettingsBaseEditor (7)  
    - UnityEditor.ColliderEditorBase (2)  
- UnityEditor.IMGUI.Controls.TreeViewItem (22)  
- UnityEditor.GUIView (6)  
- BaseField (10)  
- VisualElement (8)  
- UnityEditor.AssetDatabase (0)  

## Runtime  
- UnityEngine.PropertyAttribute (12)  
- UnityEngine.Object (18)  
  - UnityEngine.GameObject (0)  
  - UnityEngine.ScriptableObject (3)  
  - UnityEngine.Texture (9)  
    - UnityEngine.RenderTexture (1)  
  - UnityEngine.ComputeShader (0)  
  - UnityEngine.ShaderVariantCollection (0)  
  - UnityEngine.Component (6)  
    - UnityEngine.Transform (1)  
    - UnityEngine.Behaviour(11)  
      - UnityEngine.MonoBehaviour (0)  
      - UnityEngine.Light (0)  
      - UnityEngine.Camera(0)  
- UnityEngine.Renderer (4)  
- UnityEngine.PlayerLoop.~が多い  

## Full
- Attribute  
- IBindingsAttribute  (インターフェース)
- UnityEngine.Renderer  
- UnityEditor.Editor  
- UnityEditor.EditorWindow  
  - UnityEditor.PopupWindow  
  - UnityEditor.ScriptableWizard  
- UnityEditor.IHasCustomMenu  
- UnityEngine.Playbles.Iplayble  
- UnityEngine.Object (沢山)  
  - UnityEngine.GameObject  
  - UnityEngine.Texture  
  - UnityEditor.AssetImporter  
    - UnityEditor.TextureImporter  
  - UnityEngine.Component (沢山)  
    - UnityEngine.Behaviour (沢山)  
      - UnityEngine.MonoBehaviour (1)  
  - UnityEngine.ScriptableObject (すごい沢山)  
    - UnityEditor.ScriptableSingleton  
    

## クラスメモ  
- SerializedObject  
- AssetDataBase  
- Object  
  - ScriptableObject  
    - EditorWindow(Fullには継承していなかった..)  
    - Editor  
      //inspectorはEditorWindowに複数のEditorを持っている?   
  - GameObject  
  - Behaviour
    - MonoBehaviour  
- Selection  
- InternalEditorUtility  
- Resources  
- HideFlags.DontSaveInEditor  
- UnityEditor.SceneManagement.~
- EditorSceneManager
- \[RuntimeInitializeOnLoadMethod()\]
- \[ExecuteAlways\]  
- typeof(Manager) typeofの型?

## ワード
- .metaファイル  
  - YAML merge  
  - GUID  
- OnEnableなど  
- Immediate Window (Package)
- ScriptExecutionOrderをセット  
- マニュアル、スクリプトリファレンス  
- ProjectSettings  
- プロファイラ
  - 処理負荷を計測したい部分にProfiler.BeginSample("string")と
      Profiler.EndSample()を仕込む  
  - CustomSampler sampler = CustomSampler.Create("処理計測")  
      sampler.Begin();  
      sampler.End();  
  - Profiler.BeginThreadProfiling("string");  
      Profiler.EndThreadProfiling();  
      new Thread(ThreadExec).Start();  
        static void ThreadExec(){~}  
  - Time.realtimeSinceStartup;とTime.time;  
  - Recorder.Get("Camera.Render");  
- Script Debugging  
- Textuer2D[] textures = Resources.FindObjectsOfTypeAll<\Texture2D>();  
- new Editor(コンポーネント?)で内部でnew SerializedObjectしている?  
- unityスクリプトをシェーダー風につかう  
- unityで書くコードはコールバック  
- Time.timeScale Time.deltaTime と Time.fixedDeltaTime  
- Application.CaptureScreenshot  
- Unity Recorder  
- Unity Physics  
- VideoPlayer



- プロファイラの結果の項目  
  - //UpdateScreenManagerAndInput  
  - //GUIUtility.SetSkin()  
  - //PrepareSceneNodes  
  - Initialization EarlyUpdate FixedUpdate PreUpdate Update PreLateUpdate PostLateUpdate  
  - PlayerUpdateTime XREarlyUpdate AsyncUploadTimeSlicedUpdate DirectorSampleTime  
  - Profiler.CollectEditorStats Profiler.CollectMemoryAllocationStats
  - EarlyUpdate.GpuTimestamp ExecuteMainThreadJobs UpdatePreloading PostLateUpdate.FinishFrameRendering  
  - EditorGUIUtility.HandleControlID Application.Message GC.Alloc  
  - Application.Tick  UpdateSchedulers
   
  tickEditor.Invoke Application.UpsateScene GUIView.RepaintAll.RepaintScene
  UIElementsUtility.DoDispatch ProfilerWindow.OnGUI.repaint UIR.ImmediateRenderer

  The Standalone Profiler launches the Profiler window in a separate process from the Editor.
  This means that the performance of the Editor dose not affect profiling data, and the Profiler does not affect the performance of the Editor.
  It takes around 3-4 seconds to launch.
  スタンドアロンプロファイラーは、エディターとは別のプロセスでプロファイラーウィンドウを起動します。
  これは、エディターのパフォーマンスがプロファイリングデータに影響を与えず、プロファイラーがエディターのパフォーマンスに影響を与えないことを意味します。
  起動には約3〜4秒かかります。  

  Preview packages are in the early atage of development and not yet ready for production.
  We recommend using these only for testing purpose and to give us direct feedback
  プレビューパッケージは開発の初期段階にあり、まだ本番環境の準備ができていません。
   これらはテスト目的でのみ使用し、直接フィードバックを提供することをお勧めします

- UnityEngin  
  - ●Application  
  - ●UnityEngine.SceneManagement  
  - UnityEngine.PlayerLoop  
  - Object  
  - ScriptableObject  
  - GameObject  
  - Behaviour  
  - MonoBehaviour  
  - Component  
  - UnityEngine.Serialization  
  - PlayerPrefs  
  - Resources  
  - UnityEngine.Events Delegate? uGUIのButtonのOn Click()で使われている
  - 
  - UnityEngine.Jobs  
  - UnityEngine.Profiling  
  - UnityEngine.Rendering  
  - Transform  
  - Time  
  - Debug
  - UnityEngine.UIElements  
  - Attributes  
    - ExecuteAlways  
    - RuntimeInitializeOnLoadMethod  
    - SerializeField  
    - SerializeReference  
  - Enumerations  
    - HideFlags  

- UnityEditor  
  - Editor  
  - ●AssetDatabase  
  - AssetImporter  
  - ●EditorUtility  
  - ●EditorApplication  
  - ●UnityEditor.SceneManagement.EditorSceneManager  
  - UnityEditor.Callbacks  
  - ●SerializedObject  
  - ●SerializedProperty  
  - 
  - ●Selection
  - EditorPrefs  
  - EditorUserSettings.Set/GetConfigValue  
  - PrefabUtility
  - SessionState  
  - ObjectNames  
  - GlobalObjectId  
  - ObjectFactory  
  - Handles  
  - HandleUtility  
  - SettingsProvider(表示)とSettingsService(対話)  
  - UnityEditor.Compilation.CompilationPipeline
  - UnityEditor.UIElements  
  - Attributes
    - InitializeOnLoadAttribute  
    - InitializeOnLoadMethodAttribute
    - MenuItem  
- Other
  - Classes  
    - Serializable 