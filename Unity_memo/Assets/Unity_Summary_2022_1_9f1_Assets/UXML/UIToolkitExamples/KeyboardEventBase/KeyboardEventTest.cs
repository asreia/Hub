using UnityEngine;
using UnityEngine.UIElements;

// 有効な UIDocument を使用して KeyboardEventTest をゲームオブジェクトに加えます。
// ユーザーがキーを押すと、キーボードイベントのプロパティがコンソールに出力されます。
[RequireComponent(typeof(UIDocument))]
public class KeyboardEventTest : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Add(new Label("Press any key to see the keyDown properties"));
        root.Add(new TextField());
        root.Q<TextField>().Focus();
        root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        root.RegisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
    }
    void OnKeyDown(KeyDownEvent ev)
    {
        Debug.Log("KeyDown_keyCode:" + ev.keyCode);
        Debug.Log("KeyDown_character:" + ev.character);
        Debug.Log("KeyDown_modifiers:" + ev.modifiers);
    }

    void OnKeyUp(KeyUpEvent ev)
    {
        Debug.Log("KeyUp_keyCode:" + ev.keyCode);
        Debug.Log("KeyUp_character:" + ev.character);
        Debug.Log("KeyUp_modifiers:" + ev.modifiers);
    }
}