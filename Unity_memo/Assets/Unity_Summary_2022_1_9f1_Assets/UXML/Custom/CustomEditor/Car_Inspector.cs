using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(Car))]
public class Car_Inspector : Editor //Editorの継承の場合、UnityObjectのみシリアライズ可能
{
    public VisualTreeAsset m_InspectorUXML;
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement myInspector = new VisualElement();
        // myInspector.Add(new Label("This is a custom inspector"));

        // VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/Custom/CustomEditor/Car_Inspector_UXML.uxml");
        m_InspectorUXML.CloneTree(myInspector); //引数の子としCloneされる

        //UI Builderで作ったFoldoutを取得
        VisualElement inspectorFoldout = myInspector.Q("Default_Inspector"); 
        //FillDefaultInspectorで第一引数のルートに多分serializedObject内のSerializedPropertyをnew PropertyFieldしてルートにAddしていく。thisは必要なのか
        InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this);
        //--test--
        // new Button().bindingPath = "abc";
        // TextField
        //--------
        return myInspector;
    }
}