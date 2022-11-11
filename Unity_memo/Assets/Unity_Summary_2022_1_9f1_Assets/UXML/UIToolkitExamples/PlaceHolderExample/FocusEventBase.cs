using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlaceHolderExample : EditorWindow
{
    [MenuItem("Window/UI Toolkit/PlaceHolderExample")]
    public static void ShowExample()
    {
        PlaceHolderExample wnd = GetWindow<PlaceHolderExample>();
        wnd.titleContent = new GUIContent("PlaceHolderExample");
    }

    private bool placeHolderMode = true;
    private const string placeHolderText = "Write here";
    private StyleColor defaultStyleColor;

    public void CreateGUI()
    {
        TextField textField = new TextField();
        textField.value = placeHolderText;
        defaultStyleColor = textField.style.color;
        rootVisualElement.Add(textField);

        textField.RegisterCallback<FocusInEvent>(OnFocusInTextField);
        textField.RegisterCallback<FocusOutEvent>(OnFocusOutTextField);
    }

    private void OnFocusInTextField(FocusInEvent evt)
    {
        // テキストフィールドがフォーカスされたばかりで、ユーザーが内部にテキストを書き込んだり編集したりする可能性がある場合は、
        // プレースホルダーテキストをクリアする必要があります (アクティブな場合は)
        if (placeHolderMode)
        {
            var textField = evt.target as TextField;
            textField.value = "";
            textField.style.color = defaultStyleColor;  //TextFieldの中の文字の色だからIStyleでは無いかもしれない。TextField継承列を見ても見つからない
        }
    }

    private void OnFocusOutTextField(FocusOutEvent evt)
    {
        // ユーザーが編集を終えて要素がフォーカスを失った後にテキストフィールドが空の場合は、
        // プレースホルダーテキストをテキストフィールドに書き込みます
        var textField = evt.target as TextField;
        placeHolderMode = string.IsNullOrEmpty(textField.value);
        if (placeHolderMode)
            textField.value = placeHolderText;
            textField.style.color = new StyleColor(Color.magenta);
    }
}