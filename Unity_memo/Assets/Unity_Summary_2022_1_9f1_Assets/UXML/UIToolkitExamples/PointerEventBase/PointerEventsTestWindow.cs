using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class PointerEventsTestWindow : EditorWindow
{
    [MenuItem("Window/UI Toolkit/Pointer Events Test Window")]
    public static void ShowExample()
    {
        PointerEventsTestWindow wnd = GetWindow<PointerEventsTestWindow>();
        wnd.titleContent = new GUIContent("Pointer Events Test Window");
    }

    public void CreateGUI()
    {
        // UXML をインポート
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/UIToolkitExamples/PointerEventBase/PointerEventsTestWindow.uxml");
        visualTree.CloneTree(rootVisualElement);

        // 赤いボックスを取得して、ポインターのイベントコールバックを登録
        VisualElement redBox = rootVisualElement.Q("Red_Box");
        redBox.RegisterCallback<PointerOutEvent>(OnPointerOutEvent, TrickleDown.TrickleDown);
        redBox.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent, TrickleDown.TrickleDown);
    }

    private void OnPointerLeaveEvent(PointerLeaveEvent evt) //ビジュアル要素とその子孫すべてから離れるとき
    {
        Debug.Log($"Pointer LEAVE Event. Target: {(evt.target as VisualElement).name}");
    }

    private void OnPointerOutEvent(PointerOutEvent evt) //ビジュアル要素から離れるとき
    {
        Debug.Log($"Pointer OUT Event. Target: {(evt.target as VisualElement).name}");
    }
}