using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements; //SerializedObjectバインディングはEditorのみ

class BindTest1 : EditorWindow
{
    [MenuItem("UI Toolkit/BindTest1")]
    static void GetWindow()
    {
        EditorWindow wnd = GetWindow<BindTest1>("EventTest1"); //↓を書かなくてもいい
        // wnd.titleContent = new GUIContent("EventTest");
        Debug.Log("CreateGUI()の後に来る"); //恐らくGetWindowの初期化時にCreateGUIが呼ばれている
    }
    public void CreateGUI() //IMGUIは確かOnGUI、UIElementsはCreateGUI
    {

        Debug.Log("CreateGUI()");

        VisualElement vE_0 = new(){name = "n: E_0"}; //ここはBind禁止
        rootVisualElement.Add(vE_0);
            VisualElement vE_1_0 = new(){name = "n: E_1_0 /B/UB/ S_0"}; //Bind so_bindData_0 //Bind(~), UnBind()
            vE_0.Add(vE_1_0);
                Toggle t_2_0_0 = new("T_2_0_0 /bP/ S_0_0"){name = "n: T_2_0_0 /bP/ S_0_0"}; //共有so bindingPath so_bindData_0 0
                vE_1_0.Add(t_2_0_0);
                Toggle t_2_0_1 = new("T_2_0_1 /bP/ S_0_1"){name = "n: T_2_0_1 /bP/ S_0_1"}; //共有so bindingPath so_bindData_0 1
                vE_1_0.Add(t_2_0_1);
                Toggle t_2_0_2 = new("T_2_0_2 /BPt/ S_1_0"){name = "n: T_2_0_2 /BPt/ S_1_0"}; //vE_1_0:BindProperty so_bindData_1 0 //Bind(~), UnBind()
                vE_1_0.Add(t_2_0_2);
                Toggle t_2_0_3 = new("T_2_0_3 /TPV/ S_0_2"){name = "n: T_2_0_3 /TPV/ S_0_2"}; //TrackPropertyValue so_bindData_0 2 //Bind(~), UnBind()
                vE_1_0.Add(t_2_0_3);
                Toggle t_2_0_4 = new("T_2_0_4 /TSV/ S_0"){name = "n: T_2_0_4 /TSV/ S_0"}; //TrackSerializedObjectValue so_bindData_0 _ //Bind(~), UnBind()
                vE_1_0.Add(t_2_0_4);

            Toggle t_1_1 = new("T_1_1 /bP/ S_0_3"){name = "n: T_1_1 /bP/ S_0_3"}; //bindingPathミス so_bindData_0 3
            vE_0.Add(t_1_1);
            Toggle t_1_2 = new("T_1_2 /BPt/ S_0_4"){name = "n: T_1_2 /BPt/ S_0_4"}; //vE_0:BindProperty so_bindData_0 4
            vE_0.Add(t_1_2);


        ScriptableObject bindData_0 = ScriptableObject.CreateInstance<BindData_0>();
        ScriptableObject bindData_1 = ScriptableObject.CreateInstance<BindData_1>();
        AssetDatabase.CreateAsset(bindData_0, "Assets/UXML/MyUXML/BindTest/BindData_0.asset");
        AssetDatabase.CreateAsset(bindData_1, "Assets/UXML/MyUXML/BindTest/BindData_1.asset");
        SerializedObject S_0 = new(bindData_0);
        SerializedObject S_1 = new(bindData_1);


        //Bind(~), UnBind 

        //vE_1_0 / S_0
        Button vE_1_0_Bind_S_0 = new(()=>
        {
            Debug.Log("vE_1_0.Bind(so_bindData_0);");
            vE_1_0.Bind(S_0);
        }
        ){name = "vE_1_0_Bind_S_0", text = "vE_1_0_Bind_S_0"};
        rootVisualElement.Add(vE_1_0_Bind_S_0);
        Button vE_1_0_UnBind = new(()=>
        {
            Debug.Log("vE_1_0.Unbind();");
            vE_1_0.Unbind();
        }
        ){name = "vE_1_0_Unbind", text = "vE_1_0_Unbind"};
        rootVisualElement.Add(vE_1_0_UnBind);

        //t_2_0_2 / S_1_0
        Button t_2_0_2_BPt_S_1_0 = new(()=>
        {
            Debug.Log("t_2_0_2.BindProperty(so_bindData_1.FindProperty(\nameof(BindData_0.b_1_0)));");
            t_2_0_2.BindProperty(S_1.FindProperty(nameof(BindData_1.b_1_0)));
        }
        ){name = "t_2_0_2 /BPt/ S_1_0", text = "t_2_0_2 /BPt/ S_1_0"};
        rootVisualElement.Add(t_2_0_2_BPt_S_1_0);
        Button t_2_0_2_Unbind = new(()=>
        {
            Debug.Log("t_2_0_2.Unbind();");
            t_2_0_2.Unbind();
        }
        ){name = "t_2_0_2_Unbind", text = "t_2_0_2_Unbind"};
        rootVisualElement.Add(t_2_0_2_Unbind);

        //t_2_0_3 / S_0_2
        Button t_2_0_3_TPV_S_0_2 = new(()=>
        {
            Debug.Log("t_2_0_3.TrackPropertyValue(so_bindData_0.FindProperty(\nameof(BindData_0.b_0_2)),(sp)=>{Debug.Log($\"{t_2_0_3.name} / sp.boolValue: {sp.boolValue}\");});");
            t_2_0_3.TrackPropertyValue(S_0.FindProperty(nameof(BindData_0.b_0_2)),(sp)=>{Debug.Log($"{t_2_0_3.name} / sp.boolValue: {sp.boolValue}");});
        }
        ){name = "t_2_0_3 /TPV/ S_0_2", text = "t_2_0_3 /TPV/ S_0_2"};
        rootVisualElement.Add(t_2_0_3_TPV_S_0_2);
        Button t_2_0_3_Unbind = new(()=>
        {
            Debug.Log("t_2_0_3.Unbind();");
            t_2_0_3.Unbind();
        }
        ){name = "t_2_0_3_Unbind", text = "t_2_0_3_Unbind"};
        rootVisualElement.Add(t_2_0_3_Unbind);

        //T_2_0_4 / S_0
        Button T_2_0_4_TSV_S_0 = new(()=>
        {
            Debug.Log("t_2_0_4.TrackSerializedObjectValue(so_bindData_0, (so)=>{Debug.Log($\"{t_2_0_4.name} / so.ToString(): {so.ToString()}\");});");
            t_2_0_4.TrackSerializedObjectValue(S_0, (so)=>{Debug.Log($"{t_2_0_4.name} / so.ToString(): {so.ToString()}");});
        }
        ){name = "T_2_0_4 /TSV/ S_0", text = "T_2_0_4 /TSV/ S_0"};
        rootVisualElement.Add(T_2_0_4_TSV_S_0);
        Button t_2_0_4_Unbind = new(()=>
        {
            Debug.Log("t_2_0_4.Unbind();");
            t_2_0_4.Unbind();
        }
        ){name = "t_2_0_4_Unbind", text = "t_2_0_4_Unbind"};
        rootVisualElement.Add(t_2_0_4_Unbind);

        //共有so
        t_2_0_0.bindingPath = nameof(BindData_0.b_0_0);
        t_2_0_1.bindingPath = nameof(BindData_0.b_0_1);

        //vE_1_0:BindProperty
        t_2_0_2.BindProperty(S_1.FindProperty(nameof(BindData_1.b_1_0)));

        //TrackPropertyValue
            //追跡(Track)先が、追跡先の初期値に変化するとCallbackを呼ばない(謎) //TrackPropertyValue,Unbindを繰り返したら、直った(謎)
            //追跡(Track)先が、追跡先の初期値では無い状態で、so_bindData_0の任意のPropertyが変化するとCallbackを実行してしまう(仕様?)
                //(色々な条件で試したが同じ(vE_0.TrackPropertyValue(~), TrackSerializedObjectValue無効化))
        t_2_0_3.TrackPropertyValue(S_0.FindProperty(nameof(BindData_0.b_0_2)),(sp)=>{Debug.Log($"{t_2_0_3.name} / sp.boolValue: {sp.boolValue}");});

        //TrackSerializedObjectValue
        t_2_0_4.TrackSerializedObjectValue(S_0, (so)=>{Debug.Log($"{t_2_0_4.name} / so.ToString(): {so.ToString()}");});

        //bindingPathミス
        t_1_1.bindingPath = nameof(BindData_0.b_0_3);

        //vE_0:BindProperty
        t_1_2.BindProperty(S_0.FindProperty(nameof(BindData_0.b_0_4)));

        //Bind so_bindData_0
        vE_1_0.Bind(S_0);

        ElementTreeLog(rootVisualElement.panel);

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
