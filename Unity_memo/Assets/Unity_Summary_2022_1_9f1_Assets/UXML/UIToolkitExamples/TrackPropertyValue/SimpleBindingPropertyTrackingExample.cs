using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UIToolkitExamples
{
    public class SimpleBindingPropertyTrackingExample : EditorWindow
    {
        TextField m_ObjectNameBinding;
        SerializedProperty property1;

        [MenuItem("Window/UIToolkitExamples/Simple Binding Property Tracking Example")]
        public static void ShowDefaultWindow()
        {
            var wnd = GetWindow<SimpleBindingPropertyTrackingExample>();
            wnd.titleContent = new GUIContent("Simple Binding Property Tracking");
        }
            
        public void CreateGUI()
        {
            m_ObjectNameBinding = new TextField("Object Name Binding");
            rootVisualElement.Add(m_ObjectNameBinding);
            OnSelectionChange();
        }
        //EditorWindow.OnSelectionChange():選択が変更されるたびに呼び出されます。
        public void OnSelectionChange()
        {
            GameObject selectedObject = Selection.activeObject as GameObject;
            if (selectedObject != null)
            {
                // Create the SerializedObject from the current selection
                SerializedObject so = new SerializedObject(selectedObject);

                property1 = new SerializedObject(selectedObject).FindProperty("m_Name");
                property1.stringValue = "Fixed";
                // Note: the "name" property of a GameObject is actually named "m_Name" in serialization.
                SerializedProperty property = so.FindProperty("m_Name");
                // SerializedProperty property2 = property.Copy();property2.stringValue="Fixed";

                //=>true SerializedPropertyを変えると元のSerializedObjectも変わる。つまり参照先が同じ?
                // property.stringValue = "fixed";Debug.Log($"so==property: {so.FindProperty("m_Name").stringValue == property.stringValue}");
                    
                // Ensure to use Unbind() before tracking a new property
                m_ObjectNameBinding.Unbind();
                //GUI操作毎にSerializedPropertyが関数実行時と同じであるかチェック(バグかも知れない)し違うならコールバックを実行する
                    //propertyが変更されるとCheckNameを実行する//Unbind()するとpropertyの追跡が外れる?みたい
                    //BindProperty()と同様にpropertyでBindをしている?BindPropertyは同期を取るが、TrackPropertyValueは変更時コールバック呼ぶという違いなだけ?
                m_ObjectNameBinding.TrackPropertyValue(property, CheckName); 
                // Bind the property to the field directly
                m_ObjectNameBinding.BindProperty(property);//これは、valueを変えるもの?
                    // m_ObjectNameBinding.value = property.stringValue; //←をバインディング更新時、実行している?
                // m_ObjectNameBinding.bindingPath = "aaa";
                // m_ObjectNameBinding.Bind(so);//これは、valueを変えるもの?
                // m_ObjectNameBinding.bindingPath = "m_Name";
                // so.FindProperty("m_Name").stringValue = "aaa";

                CheckName(property);
            }
            else
            {
                // Unbind any binding from the field
                m_ObjectNameBinding.Unbind();
                m_ObjectNameBinding.value = "null";
            }
        }

        void CheckName(SerializedProperty property)
        {
            Debug.Log($"m_ObjectNameBinding.value: {m_ObjectNameBinding.value}");
            if (property.stringValue == "GameObject")
            {
                Debug.Log("GameObject");
                m_ObjectNameBinding.style.backgroundColor = Color.red * 0.5f;
            }
            else
            {
                m_ObjectNameBinding.style.backgroundColor = StyleKeyword.Null;
            }
        }
    }
}