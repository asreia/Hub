using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

//いろいろグシャグシャしてきてボツになったやつ

// textField_3_0_1.isDelayed = true; //true に設定すると、ユーザーが Enter キーを押すか、テキスト フィールドがフォーカスを失うまで、value プロパティは更新されません。
                                                //https://docs.unity3d.com/ScriptReference/UIElements.TextInputBaseField_1.html

class BindTest : EditorWindow
{
    [MenuItem("UI Toolkit/BindTest")]
    static void GetWindow()
    {
        EditorWindow wnd = GetWindow<BindTest>("EventTest"); //↓を書かなくてもいい
        // wnd.titleContent = new GUIContent("EventTest");
        Debug.Log("CreateGUI()の後に来る"); //恐らくGetWindowの初期化時にCreateGUIが呼ばれている
    }
    public void CreateGUI() //IMGUIは確かOnGUI、UIElementsはCreateGUI
    {

        Debug.Log("CreateGUI()");

        VisualElement vE_0 = new(){name = "n_vE_0"};
        VisualElement vE_1 = new(){name = "n_vE_1"};
        vE_0.Add(vE_1);
        VisualElement vE_2_0 = new(){name = "n_vE_2_0"};
        vE_1.Add(vE_2_0);
        Toggle toggle_3_0_0 = new("toggle_3_0_0"){name = "n_toggle_3_0_0"};
        vE_2_0.Add(toggle_3_0_0);
        TextField textField_3_0_1 = new("textField_3_0_1"){name = "n_textField_3_0_1"};
        vE_2_0.Add(textField_3_0_1);
        Toggle toggle_3_0_2 = new("toggle_3_0_2"){name = "n_toggle_3_0_2"};
        vE_2_0.Add(toggle_3_0_2);
        VisualElement vE_2_1 = new(){name = "n_vE_2_1"};
        vE_1.Add(vE_2_1);
        IntegerField integerField_3_1_0 = new("integerField_3_1_0"){name = "n_integerField_3_1_0"};
        vE_2_1.Add(integerField_3_1_0);
        Toggle toggle_3_1_1 = new("toggle_3_1_1"){name = "n_toggle_3_1_1"};
        vE_2_1.Add(toggle_3_1_1);
        Toggle toggle_3_1_2 = new("toggle_3_1_2"){name = "n_toggle_3_1_2"};
        vE_2_1.Add(toggle_3_1_2);

        ScriptableObject bindData_0 = ScriptableObject.CreateInstance<BindData_0>();
        ScriptableObject bindData_1 = ScriptableObject.CreateInstance<BindData_1>();
        ScriptableObject bindData_2 = ScriptableObject.CreateInstance<BindData_2>();
        AssetDatabase.CreateAsset(bindData_0, "Assets/UXML/MyUXML/BindTest/BindData_0.asset");
        AssetDatabase.CreateAsset(bindData_1, "Assets/UXML/MyUXML/BindTest/BindData_1.asset");
        AssetDatabase.CreateAsset(bindData_2, "Assets/UXML/MyUXML/BindTest/BindData_2.asset");
        SerializedObject so_bindData_0 = new(bindData_0);
        SerializedObject so_bindData_1 = new(bindData_1);
        SerializedObject so_bindData_2 = new(bindData_2);

        Button Bind = new(()=>{Debug.Log("vE_2_0.bind();"); vE_2_0.Bind(so_bindData_0);}){text = "Bind vE_2_0"}; //ok
        rootVisualElement.Add(Bind);
        Button unBind2 = new(()=>{Debug.Log("TrackSerializedObjectValue Unbind();"); vE_0.Unbind();}){text = "TrackSerializedObjectValue Unbind();"}; //ok 
        rootVisualElement.Add(unBind2);
        Button unBind = new(()=>{Debug.Log("vE_2_0.Unbind();"); vE_2_0.Unbind();}){text = "UnBind vE_2_0"}; //ok
        rootVisualElement.Add(unBind);
        Button unBind3 = new(()=>{Debug.Log("vE_1.Unbind();"); vE_1.Unbind();}){text = "UnBind vE_1"};//////挙動不安定//❰ネスト¦複数❱ Bind はしない方がいい
        rootVisualElement.Add(unBind3);
        Button unBind0 = new(()=>{Debug.Log("TrackPropertyValue Unbind();"); toggle_3_0_0.Unbind();}){text = "UnBind TrackPropertyValue"}; //toggle_3_0_0.Unbind()ok
        rootVisualElement.Add(unBind0);
        Button unBind1 = new(()=>{Debug.Log("FindProperty Unbind();"); integerField_3_1_0.Unbind();}){text = "UnBind FindProperty"}; //ok
        rootVisualElement.Add(unBind1);
        Button binding = new(()=>{Debug.Log($"(integerField_3_1_0.binding as VisualElement).name: {(integerField_3_1_0.binding as VisualElement).name}");}){text = "3_1_0.binding"}; //=> ぬるり 
        rootVisualElement.Add(binding);


        // vE_2_0.Bind(so_bindData); //本来、bindingPathを設定した後、BindしないとBindされないはずだけどBindされる

        /*vE_1*/vE_0.TrackSerializedObjectValue(so_bindData_0, (so)=>{//soが変化しらたCallbackを呼ぶ。Elementに紐づける意味は無さそう..
            Debug.Log($"vE_0.TrackSerializedObjectValue: {so}=============================");
        }); 

        /*vE_0*/toggle_3_0_0.TrackPropertyValue(so_bindData_0.FindProperty("bindData_0_bool"), (sp)=>{ //spが初期値から?変化したらCallbackを呼ぶ。Elementに紐づける意味は無さそう..
            Debug.Log($"toggle_3_0_0.TrackPropertyValue: {sp.boolValue}=============================");    //Unbindすると解除される
            Debug.Log($"toggle_3_0_0.bindingPath: {toggle_3_0_0.bindingPath}=");//=>"bindData_0_bool" => 1
        }); 
            Debug.Log($"toggle_3_0_0.bindingPath: {toggle_3_0_0.bindingPath}=");//=>"bindData_0_bool" 

        /**/toggle_3_0_0.bindingPath = /*"bindData_0_bool"*/"bindData_1_bool";//vE_1.Bind(so_bindData_1);からのBindも効く
                Debug.Log($"toggle_3_0_0.bindingPath: {toggle_3_0_0.bindingPath}=aaaaaaaaaaaaa");
        /**/textField_3_0_1.bindingPath = "bindData_0_string";
        // /**/textField_3_0_1.BindProperty(so_bindData_0.FindProperty("bindData_0_string"));
        textField_3_0_1.isDelayed = true; //true に設定すると、ユーザーが Enter キーを押すか、テキスト フィールドがフォーカスを失うまで、value プロパティは更新されません。
                                                //https://docs.unity3d.com/ScriptReference/UIElements.TextInputBaseField_1.html


        toggle_3_0_2.bindingPath = "bindData_2_bool";

        /**/SerializedProperty fp = so_bindData_0.FindProperty("bindData_0_int"); integerField_3_1_0.isDelayed = true;
            //BindProperty(SerializedProperty)は多分、直接このElementにSerializedObjectをBindしbindingPathも設定する
            integerField_3_1_0.BindProperty(fp); 
                Debug.Log($"integerField_3_1_0.bindingPath: {integerField_3_1_0.bindingPath}");//=>bindData_0_int
                //↕多分等価
            // /**/integerField_3_1_0.Bind(fp.serializedObject); integerField_3_1_0.bindingPath = fp.propertyPath;

        /**/toggle_3_1_1.bindingPath = "bindData_0_bool"; //vE_2_0.Bind(so_bindData);の場合、子孫では無いのでBindされない(反応しない)
        /**/toggle_3_1_2.bindingPath = "bindData_1_bool"; //vE_1.Bind(so_bindData_1);である祖先からのBindなのでBindされる

        vE_1.Bind(so_bindData_1);
        vE_1.Bind(so_bindData_2); //一つのElementに2コ以上のBindをすると挙動がおかしくなる
        //Bindをネストしたり1Elementに複数BindしたりするとUnbindがおかしくなる===================================================================================================
        vE_2_0.Bind(so_bindData_0);

        //bindingはBindしたSerializedObjectがある場所だと思ったけどnullだった
        Debug.Log($"(toggle_3_0_0.binding as VisualElement).name: {(toggle_3_0_0.binding as VisualElement)?.name}"); 

        rootVisualElement.Add(vE_0);

        ElementTreeLog(rootVisualElement.panel);

        vE_0.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Log(evt);
            }
            , TrickleDown.TrickleDown //TrickleDown,BubbleUp 期待通りの挙動
            );
        vE_1.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Log(evt);
            }
            // , TrickleDown.TrickleDown
            );
        vE_2_0.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Log(evt);
            }
            , TrickleDown.TrickleDown
            );
        toggle_3_0_0.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Log(evt);
            }
            // , TrickleDown.TrickleDown
            );
        textField_3_0_1.RegisterCallback<ChangeEvent<string>>((evt)=>
            {
                Log(evt);
            }
            , TrickleDown.TrickleDown
            );
        vE_2_1.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Log(evt);
            }
            // , TrickleDown.TrickleDown
            );
        integerField_3_1_0.RegisterCallback<ChangeEvent<int>>((evt)=>
            {
                Log(evt);
            }
            , TrickleDown.TrickleDown
            );
        toggle_3_1_1.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Log(evt);
            }
            , TrickleDown.TrickleDown
            );
        toggle_3_1_2.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Log(evt);
            }
            , TrickleDown.TrickleDown
            );

        static void Log(EventBase evt)
        {
            VisualElement e_target = evt.target as VisualElement;
            VisualElement e_currentTarget = evt.currentTarget as VisualElement;
            Debug.Log("ChangeEvent====================");
            Debug.Log($"evt.currentTarget: {e_currentTarget.name}");
            Debug.Log($"evt.target: {e_target.name}");
            Debug.Log($"propagationPhase: {evt.propagationPhase}");
        }

        static void ElementTreeLog(IPanel p)
        {
            ElementTreeLogRecursion(p.visualTree, "");
            static void ElementTreeLogRecursion(VisualElement ve, string indent)
            {
                Debug.Log($"{indent}n:{ve.name}");
                indent += "  ";

                foreach(VisualElement e in ve.hierarchy.Children())
                {
                    ElementTreeLogRecursion(e, indent);
                }
            }
        }
    }
}