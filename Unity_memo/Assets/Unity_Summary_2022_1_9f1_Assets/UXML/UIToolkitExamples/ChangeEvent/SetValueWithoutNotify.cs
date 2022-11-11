using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SetValueWithoutNotify : EditorWindow
{
    private Toggle m_MyToggle;

    [MenuItem("Window/UI Toolkit/SetValueWithoutNotify")]
    public static void ShowExample()
    {
        GetWindow<SetValueWithoutNotify>().titleContent = new GUIContent("Change Event Test Window");
    }

    public void CreateGUI()
    {
        // Create a toggle and register callback 
        m_MyToggle = new Toggle("Test Toggle") { name = "My Toggle" };
        m_MyToggle.RegisterValueChangedCallback((evt) => { Debug.Log("Change Event received"); });
        rootVisualElement.Add(m_MyToggle);

        // Create button to toggle the toggle's value
        Button button01 = new Button() { text = "Toggle" };
        //RegisterCallback<~>(~)は同じ関数(とトリクル)は一つまで
        button01.RegisterCallback<MouseUpEvent>((evt) =>    //clickedよりRegisterCallbackの方が早い//clickedはButton独自の独立した機能?
            {
                Debug.Log("RegisterCallback");
            }
        );
        button01.clicked += () => 
        {
            Debug.Log("clicked"); //↑のRegisterCallbackより先にclickedが呼ばれる
            m_MyToggle.value = !m_MyToggle.value;
        };
        rootVisualElement.Add(button01);

        // Create button to toggle the toggle's value without triggering a ChangeEvent
        Button button02 = new Button() { text = "Toggle without notification" };
        button02.clicked += ClickedFunc;
        // button02.clicked += ClickedFunc; //マルチキャストデリゲートは同じ関数でも入れた分だけ実行する
        // button02.clicked += ClickedFunc;
        // button02.clicked += ClickedFunc;
        // button02.clicked -= ClickedFunc;
        void ClickedFunc()
        {
            Debug.Log("ClickedFunc");
            m_MyToggle.SetValueWithoutNotify(!m_MyToggle.value);
        }
        rootVisualElement.Add(button02);
    }
}