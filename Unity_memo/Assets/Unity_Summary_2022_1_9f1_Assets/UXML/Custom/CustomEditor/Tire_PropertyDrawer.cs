using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(Tire))]
public class Tire_PropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();

        //PopupWindowはUI Builderには無い
        UnityEngine.UIElements.PopupWindow popup = new UnityEngine.UIElements.PopupWindow();//名前空間を省くとUnityEditor.PopupWindowと曖昧とでる
        popup.text = "Tire Details";
        //PropertyFieldはUI Builderにある//propertyはTire
        popup.Add(new PropertyField(property.FindPropertyRelative("m_AirPressure"), "Air Pressure (psi)"));
        popup.Add(new PropertyField(property.FindPropertyRelative("m_ProfileDepth"), "Profile Depth(mm)"));
        // popup.Add(new PropertyField(property));//SerializedPropertyをバインドしてるとも言える
        PropertyField pf = new PropertyField();pf.BindProperty(property);
        popup.Add(pf);
        container.Add(popup);
        
        return container; //返されるVisualElementからBind(SerializedObject)が自動的に呼ばれる
    }
}