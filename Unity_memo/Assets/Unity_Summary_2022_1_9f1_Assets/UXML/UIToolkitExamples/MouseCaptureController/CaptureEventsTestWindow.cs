using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CaptureEventsTestWindow : EditorWindow
{
    [MenuItem("Window/UI Toolkit/CaptureEventsTestWindow")]
    public static void ShowExample()
    {
        var wnd = GetWindow<CaptureEventsTestWindow>();
        wnd.titleContent = new GUIContent("Capture Events Test Window");
    }

    private bool m_IsCapturing = false;

    public void CreateGUI()
    {
        // クリックするとコンソールにメッセージを出力するラベルをいくつか加えます。
        for (int i = 0; i < 4; i++)
        {
            Label clickableLabel = new Label($"Label {i} - Click Me!");
            clickableLabel.RegisterCallback<MouseDownEvent>((evt) => { Debug.Log($"Clicked on label '{(evt.target as Label).text}'"); });
            rootVisualElement.Add(clickableLabel);
        }

        // ポインターを捉えるラベルを加えます。
        Label capturingLabel = new Label("Click here to capture mouse");
        capturingLabel.RegisterCallback<MouseDownEvent>((evt) =>
        {
            if (!m_IsCapturing)
            {
                capturingLabel.text = "Click here to release mouse";
                MouseCaptureController.CaptureMouse(capturingLabel);
                m_IsCapturing = true;
            }
            else
            {
                capturingLabel.text = "Click here to capture mouse";
                MouseCaptureController.ReleaseMouse(capturingLabel);
                m_IsCapturing = false;
            }
        });
        rootVisualElement.Add(capturingLabel);

        // マウスがキャプチャ/離されるときにメッセージを出力するコールバックを登録します。
        rootVisualElement.RegisterCallback<MouseCaptureEvent>((evt) =>
        {
            Debug.Log("Mouse captured");
        });
        rootVisualElement.RegisterCallback<MouseCaptureOutEvent>((evt) =>
        {
            Debug.Log("Mouse captured released");
        });
    }
}