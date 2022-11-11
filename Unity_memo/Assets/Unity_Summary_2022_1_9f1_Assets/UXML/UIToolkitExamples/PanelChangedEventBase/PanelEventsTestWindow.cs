using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PanelEventsTestWindow : EditorWindow
{
    [MenuItem("Window/UI Toolkit/Panel Events Test Window")]
    public static void ShowExample()
    {
        PanelEventsTestWindow wnd = GetWindow<PanelEventsTestWindow>();
        wnd.titleContent = new GUIContent("Panel Events Test Window");
    }

    public void CreateGUI()
    {
        // パネルの名前を設定
        rootVisualElement.panel.visualTree.name = "Our Window Root Visual Element";
        rootVisualElement.name = "testRootElement";  //evt.❰destinationPanel¦originPanel❱.visualTree.nameで表示されない

        var add_Label = new Button(){text = "Add Label"};
        add_Label.clicked += () => 
            {
                var label = new Label("===");
                label.RegisterCallback<AttachToPanelEvent>(evt=>{Debug.Log($"(VisualElement)evt.target: {(VisualElement)evt.target}");});
                rootVisualElement.Add(label);
            };

        rootVisualElement.Add(add_Label);

        VisualElement ve = new (){name = "ve"}; 
        // rootVisualElement.Add(ve); ve.Add(add_Label);

        // rootVisualElement.Add(ve); //ボタンの前にできる

        // カスタムラベルの新しいインスタンスをウィンドウに加えるボタンを加えます
        rootVisualElement.Add(new Button(() =>{ 
            // rootVisualElement.Add(new CustomLabel());
            ve.Add(new CustomLabel());
        }) { text = "Add New Label" }); 

        rootVisualElement.Add(ve);

        ve.panel.visualTree.name = "Override"; //"Our Window Root Visual Element"を上書きする

        Debug.Log($"ve.panel.visualTree.Contains(rootVisualElement): {ve.panel.visualTree.Contains(rootVisualElement)}");//=>true
        Debug.Log($"ve.panel.visualTree.Equals(rootVisualElement): {ve.panel.visualTree.Equals(rootVisualElement)}");//=>false //←↑子孫に含まれるが同じではない
        Debug.Log($"ve.panel.visualTree.Equals(rootVisualElement.panel.visualTree): {ve.panel.visualTree.Equals(rootVisualElement.panel.visualTree)}");//=>true
        Debug.Log($"ve.panel == rootVisualElement.panel: {ve.panel == rootVisualElement.panel}");//=>true //←↑同じIPanelを共有している
        VisualElement ve1 = new(){name = "ve1"};
        Debug.Log($"ve1.panel.visualTree.name: {ve1.panel?.visualTree.name}");//=>null
        VisualElement ve2 = new(){name = "ve2"};
        ve1.Add(ve2);
        Debug.Log($"ve2.panel.visualTree.name: {ve2.panel?.visualTree.name}");//=>null  //←↑panelは存在するみたいだが、visualTree(パネルのルート?)がセットされない
                                                                                            //getOnlyなのでIPanel生成時に初期化される必要がある
                                                                                            //つまりディスパッチャーなど機能させるには、
                                                                                            //Element.IPanel.visualTreeがセットされている、ElementにAddする必要がある
        Debug.Log($"ve.panel == ve1.panel: {ve.panel == ve1.panel}");//=>false
        ve.Add(ve1);
        // ve1.Add(ve);
        Debug.Log($"ve.panel == ve1.panel: {ve.panel == ve1.panel}");//=>true           //←↑ve1のIPanelがveのIPanelに置き換わり、ve1がveの子Elementになった
        Debug.Log($"ve.panel.visualTree.name: {ve.panel.visualTree.name}");//=>Override
        Debug.Log($"ve.panel.visualTree.panel.visualTree.name: {ve.panel.visualTree.panel.visualTree.name}");//=>Override
        
        ElementTreeLog(ve1.panel); //n:Override => n:Dockarea => n:testRootElement => n: => n: => n:ve =>  n:ve1 => n:ve2

        static void ElementTreeLog(IPanel p)
        {
            ElementTreeLogRecursion(p.visualTree, "");
            static void ElementTreeLogRecursion(VisualElement ve, string indent)
            {
                Debug.Log($"{indent}n:{ve.name}");

                foreach(VisualElement e in ve.hierarchy.Children())
                {
                    indent += "  ";
                    ElementTreeLogRecursion(e, indent);
                }
            }
        }
    }
}

/// <summary>
/// 接続またはデタッチ時にコンソールメッセージを出力するカスタムラベルクラス
/// </summary>
public class CustomLabel : Label
{
    private static int m_InstanceCounter = 0;
    private int m_LabelNumber;

    public CustomLabel() : base()
    {
        m_LabelNumber = ++m_InstanceCounter;
        text = $"Label #{m_LabelNumber} - click me to detach";
        RegisterCallback<AttachToPanelEvent>(evt =>
        {
            Debug.Log($"I am label {m_LabelNumber} and I " +
                $"just got attached to panel '{evt.destinationPanel.visualTree.name}'");
            Debug.Log($"originPanel: {evt.originPanel?.visualTree.name}");//=>null
        });
        RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            Debug.Log($"I am label {m_LabelNumber} and I " +
                $"just got detached from panel '{evt.originPanel.visualTree.name}'");
            Debug.Log($"destinationPanel: {evt.destinationPanel?.visualTree.name}");//=>null
        });
        // この要素を階層から削除するポインターダウンコールバックを登録します
        RegisterCallback<PointerDownEvent>(evt => this.RemoveFromHierarchy()); //RemoveFromHierarchy():この要素を親階層から削除する。
    }
}