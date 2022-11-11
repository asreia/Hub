using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ChangeEventTestWindow : EditorWindow
{
    // private Toggle m_MyToggle;

    [MenuItem("Window/UI Toolkit/ChangeEventTestWindow")]
    public static void ShowExample()
    {
        ChangeEventTestWindow wnd = GetWindow<ChangeEventTestWindow>();
        wnd.titleContent = new GUIContent("Change Event Test Window");
    }

    public void CreateGUI()
    {
        // Create a toggle
        Toggle MyToggle = new Toggle("Test Toggle") { name = "My Toggle" };
        rootVisualElement.Add(MyToggle);
        // Register a callback on the toggle
        MyToggle.RegisterValueChangedCallback(OnTestToggleChanged);

        rootVisualElement.Add(new Toggle("Test Toggle1") { name = "My Toggle1" });

        Toggle MyToggle1 = new Toggle("Toggle RegisterCallback") { name = "Toggle_RegisterCallback" };
        rootVisualElement.Add(MyToggle1);
        //UnregisterCallbackは即時だが、RegisterCallbackは次回で有効?
        //TargetのVisualElementのイベントはその親も同じイベントを拾う
        MyToggle1.RegisterCallback<ChangeEvent<bool>>((evt) => 
            {
                if(evt.newValue)
                {
                    rootVisualElement.RegisterCallback<ChangeEvent<bool>>(ToggleRegFunc);
                    // rootVisualElement.RegisterCallback<ChangeEvent<bool>>(OnBoolChangedEvent);
                }
                else
                {
                    rootVisualElement.UnregisterCallback<ChangeEvent<bool>>(ToggleRegFunc);
                    // rootVisualElement.UnregisterCallback<ChangeEvent<bool>>(OnBoolChangedEvent);//バフルとトリクルで別のコールバックになる
                    // rootVisualElement.UnregisterCallback<ChangeEvent<bool>>(OnTestToggleChanged); //MyToggleのコールバックなので関係ない
                }
            }
        );
        void ToggleRegFunc(ChangeEvent<bool> evt){Debug.Log($"evt.target: {evt.target}");}


        // Register a callback on the parent
        rootVisualElement.RegisterCallback<ChangeEvent<bool>>(OnBoolChangedEvent, TrickleDown.TrickleDown);//トリクルダウン//バフルとトリクルで別のコールバックになる
    }

    private void OnBoolChangedEvent(ChangeEvent<bool> evt)
    {
        Debug.Log($"Toggle changed. Old value: {evt.previousValue}, new value: {evt.newValue}");
    }

    private void OnTestToggleChanged(ChangeEvent<bool> evt)
    {
        Debug.Log($"A bool value changed. Old value: {evt.previousValue}, new value: {evt.newValue}");
        // rootVisualElement.RegisterCallback<ChangeEvent<bool>>(OnBoolChangedEvent);

    }
}