using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SampleWindow : EditorWindow
{
   [MenuItem("Window/UI Toolkit/SampleWindow")]
   public static void ShowExample()
   {
       SampleWindow wnd = GetWindow<SampleWindow>();
       wnd.titleContent = new GUIContent("SampleWindow");
   }

   public void CreateGUI()
   {
       VisualElement label = new Label("Hello World! This is a UI Toolkit Label.");
       rootVisualElement.Add(label);

       label.tooltip = "And this is a tooltip"; //多分、TooltipEventコールバックを自動登録される

       // コールバックの登録をコメントアウトすると、ラベルに表示されるツールチップは "And this is a tooltip" (これはツールチップです) です。
　//コールバックの登録を維持する場合、ラベル (およびrootVisualElement の他の子) に表示されるツールチップは    
     //"Tooltip set by parent!" (親が設定したツールチップ) です。   
   rootVisualElement.RegisterCallback<TooltipEvent>(evt =>
       {
           evt.tooltip = "Tooltip set by parent!";
           evt.rect = (evt.target as VisualElement).worldBound;
        //    evt.rect = new Rect(0f, 21f,321f, 15f);
           Debug.Log($"(evt.target as VisualElement).worldBound: {(evt.target as VisualElement).worldBound}");
           evt.StopPropagation(); //コメントアウトすると"And this..が表示される。targetのTooltipEventコールバック実行でTooltipEvent.tooltipに"And this..が設定される?
       }, TrickleDown.TrickleDown); // Pass the TrickleDown.TrickleDown parameter to intercept the event before it reaches the label.
   }
}
