using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ContextualMenuDemo : EditorWindow
{
    [MenuItem("Window/ContextualMenuDemo")]
    public static void ShowExample()
    {
        ContextualMenuDemo wnd = GetWindow<ContextualMenuDemo>();
        wnd.titleContent = new GUIContent("ContextualMenuDemo");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        VisualElement label = new Label("Right click me!");
        root.Add(label);

        AddANewContextMenu(label);
        InsertIntoAnExistingMenu(label);

        VisualElement second = new Label("Click me also!");
        root.Add(second);

        AddANewContextMenu(second);
        InsertIntoAnExistingMenu(second);

        // メニューを消去して、デフォルトの動作を上書きする。
        ReplaceContextMenu(second);
    }

    void AddANewContextMenu(VisualElement element)
    {
        // マニピュレータは右クリックを処理し、ターゲット要素にContextualMenuPopulateEventを送信します。
        // コンストラクタ(new ContextualMenuManipulator(~))に渡されたコールバック引数は、自動的にターゲット要素に登録されます。
        element.AddManipulator(new ContextualMenuManipulator((evt) =>
        {
            evt.menu.AppendAction("First menu item", (x) => Debug.Log("First!!!!"), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Second menu item", (x) => Debug.Log("Second!!!!"), DropdownMenuAction.AlwaysEnabled);
        }));
    }

    void InsertIntoAnExistingMenu(VisualElement element)
    {
        //多分コンテキストメニュー表示時に呼ばれる
        element.RegisterCallback<ContextualMenuPopulateEvent>((evt) =>
        {
            evt.menu.AppendSeparator(); //-------が追加される
            evt.menu.AppendAction("Another action", (x) => Debug.Log("Another Action!!!!"), DropdownMenuAction.AlwaysEnabled);
        });
    }

    void ReplaceContextMenu(VisualElement element)
    {
        element.RegisterCallback<ContextualMenuPopulateEvent>((evt) =>
        {
            // evt.menu.ClearItems();
            Debug.Log("ClearItems()コメントアウト");
            evt.menu.AppendAction("The only action", (x) => Debug.Log("The only action!"), DropdownMenuAction.AlwaysEnabled);
        });
    }

}