using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Open this in the Editor via the menu Window > UI ToolKit > Mouse Event Test Window
public class MouseEventTestWindow : EditorWindow
{
    [MenuItem("Window/UI Toolkit/Mouse Event Test Window")]
    public static void ShowExample()
    {
        MouseEventTestWindow wnd = GetWindow<MouseEventTestWindow>();
        wnd.titleContent = new GUIContent("Mouse Event Test Window");
    }
    public void CreateGUI()
    {   Button temp = default;
        // Add a few buttons
        for (int i = 0; i < 3; i++)
        {
            Button newElement = new Button { name = $"Button {i}", text = $"Button {i}" };
            newElement.style.flexGrow = 1;
            rootVisualElement.Add(newElement);
            temp = newElement;
        }
        // Register mouse event callbacks
        rootVisualElement.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);//TrickleDownでないとうまくいかない
        rootVisualElement.RegisterCallback<MouseEnterEvent>(OnMouseEnter, TrickleDown.TrickleDown);//rootVisualContainerとButtonの2つのイベントが発信され受け取れる
        // temp.RegisterCallback<MouseEnterEvent>(OnMouseEnter, TrickleDown.TrickleDown);          //temp(Button)に届くのはButtonのイベントのみ
        rootVisualElement.RegisterCallback<MouseLeaveEvent>(OnMouseLeave, TrickleDown.TrickleDown);
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        bool leftMouseButtonPressed = 0 != (evt.pressedButtons & (1 << (int)MouseButton.LeftMouse)); // 1 << 0 は、1を左に0ビットシフトなので1のまま
        bool rightMouseButtonPressed = 0 != (evt.pressedButtons & (1 << (int)MouseButton.RightMouse));
        bool middleMouseButtonPressed = 0 != (evt.pressedButtons & (1 << (int)MouseButton.MiddleMouse));
        Debug.Log($"Mouse Down event. Triggered by {(MouseButton)evt.button}.((VisualElement)evt.target).name: {((VisualElement)evt.target).name}");
        Debug.Log($"Pressed buttons: Left button: {leftMouseButtonPressed} Right button: {rightMouseButtonPressed} Middle button: {middleMouseButtonPressed}");
    }

    private void OnMouseEnter(MouseEnterEvent evt)
    {
        VisualElement targetElement = (VisualElement)evt.target;
        Debug.Log($"Mouse is now over element '{targetElement.name}'");
        // evt.StopImmediatePropagation();
    }
    private void OnMouseLeave(MouseLeaveEvent evt)
    {
        VisualElement targetElement = (VisualElement)evt.target;
        Debug.Log($"Leave'{targetElement.name}'");      //ButtonとButtonの間に切れ目がある(Leave'rootVisualContainer3')//シュッとやるとスルーする
        // evt.StopImmediatePropagation();
    }
}