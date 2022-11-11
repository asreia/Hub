using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

class SelectorTest : EditorWindow
{
    [MenuItem("UI Toolkit/SelectorTest")]
    static void GetWindow(){
        var wnd = GetWindow<SelectorTest>("SelectorTest");
    }
    void CreateGUI()
    {
        //Elementを付け過ぎると 
            //Layout update is struggling to process current layout (consider simplifying to avoid recursive layout)
            //レイアウトの更新は、現在のレイアウトを処理するために苦労しています (再帰的なレイアウトを避けるために簡素化を検討してください)
            //と出る
        VisualTreeAsset vta = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/MyUXML/SelectorTest/SelectorTest.uxml");
        TemplateContainer tc = vta.CloneTree(); //UXMLが無編集だと一つの空っぽのTemplateContainerのみ
        Debug.Log($"tc.visualTreeAssetSource == null: {tc.visualTreeAssetSource == null}");//=>true //vtaが入っていない良く分からない
        rootVisualElement.Add(tc);
        ElementTreeLogClass.ElementTreeLog(rootVisualElement.panel);

        var container_0 = tc.Q<VisualElement>(name = "Container_0");
        var container_1 = tc.Q<VisualElement>(name = "Container_1");
        var playStation = tc.Q<Label>(name = "PlayStation");

        StyleSheet uss_A = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UXML/MyUXML/SelectorTest/AddUSSTest_A.uss");
        StyleSheet uss_B = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UXML/MyUXML/SelectorTest/AddUSSTest_B.uss");
        StyleSheet uss_C = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UXML/MyUXML/SelectorTest/AddUSSTest_C.uss");

        {//container_0
            tc.Q<VisualElement>(name = "Container_0_Add").Q<Button>(name = "uss_A").clicked 
                += ()=>{AddStyleSheet(container_0, uss_A);};
            tc.Q<VisualElement>(name = "Container_0_Add").Q<Button>(name = "uss_B").clicked 
                += ()=>{AddStyleSheet(container_0, uss_B);};
            tc.Q<VisualElement>(name = "Container_0_Add").Q<Button>(name = "uss_C").clicked 
                += ()=>{AddStyleSheet(container_0, uss_C);};

            tc.Q<VisualElement>(name = "Container_0_Remove").Q<Button>(name = "uss_A").clicked
                += ()=>{RemoveStyleSheet(container_0, uss_A);};
            tc.Q<VisualElement>(name = "Container_0_Remove").Q<Button>(name = "uss_B").clicked
                += ()=>{RemoveStyleSheet(container_0, uss_B);};
            tc.Q<VisualElement>(name = "Container_0_Remove").Q<Button>(name = "uss_C").clicked
                += ()=>{RemoveStyleSheet(container_0, uss_C);};
        }

        {//container_1
            tc.Q<VisualElement>(name = "Container_1_Add").Q<Button>(name = "uss_A").clicked 
                += ()=>{AddStyleSheet(container_1, uss_A);};
            tc.Q<VisualElement>(name = "Container_1_Add").Q<Button>(name = "uss_B").clicked 
                += ()=>{AddStyleSheet(container_1, uss_B);};
            tc.Q<VisualElement>(name = "Container_1_Add").Q<Button>(name = "uss_C").clicked 
                += ()=>{AddStyleSheet(container_1, uss_C);};

            tc.Q<VisualElement>(name = "Container_1_Remove").Q<Button>(name = "uss_A").clicked
                += ()=>{RemoveStyleSheet(container_1, uss_A);};
            tc.Q<VisualElement>(name = "Container_1_Remove").Q<Button>(name = "uss_B").clicked
                += ()=>{RemoveStyleSheet(container_1, uss_B);};
            tc.Q<VisualElement>(name = "Container_1_Remove").Q<Button>(name = "uss_C").clicked
                += ()=>{RemoveStyleSheet(container_1, uss_C);};
        }

        {//playStation 
            tc.Q<VisualElement>(name = "PlayStation_Add").Q<Button>(name = "uss_A").clicked 
                += ()=>{AddStyleSheet(playStation, uss_A);};
            tc.Q<VisualElement>(name = "PlayStation_Add").Q<Button>(name = "uss_B").clicked 
                += ()=>{AddStyleSheet(playStation, uss_B);};
            tc.Q<VisualElement>(name = "PlayStation_Add").Q<Button>(name = "uss_C").clicked 
                += ()=>{AddStyleSheet(playStation, uss_C);};

            tc.Q<VisualElement>(name = "PlayStation_Remove").Q<Button>(name = "uss_A").clicked
                += ()=>{RemoveStyleSheet(playStation, uss_A);};
            tc.Q<VisualElement>(name = "PlayStation_Remove").Q<Button>(name = "uss_B").clicked
                += ()=>{RemoveStyleSheet(playStation, uss_B);};
            tc.Q<VisualElement>(name = "PlayStation_Remove").Q<Button>(name = "uss_C").clicked
                += ()=>{RemoveStyleSheet(playStation, uss_C);};
        }

        {//classA
            tc.Q<Button>(name = "A_A").clicked += ()=>
            {
                Debug.Log("AddToClassList(\"classA\")");
                playStation.AddToClassList("classA");
                GetClasses(playStation);
            };
            tc.Q<Button>(name = "A_R").clicked += ()=>
            {
                Debug.Log("RemoveFromClassList(\"classA\")");
                playStation.RemoveFromClassList("classA");
                GetClasses(playStation);
            };
        }

        {//classB
            tc.Q<Button>(name = "B_A").clicked += ()=>
            {
                Debug.Log("AddToClassList(\"classB\")");
                playStation.AddToClassList("classB");
                GetClasses(playStation);
            };
            tc.Q<Button>(name = "B_R").clicked += ()=>
            {
                Debug.Log("RemoveFromClassList(\"classB\")");
                playStation.RemoveFromClassList("classB");
                GetClasses(playStation);
            };
        }

        {//classC
            tc.Q<Button>(name = "C_A").clicked += ()=>
            {
                Debug.Log("AddToClassList(\"classC\")");
                playStation.AddToClassList("classC");
                GetClasses(playStation);
            };
            tc.Q<Button>(name = "C_R").clicked += ()=>
            {
                Debug.Log("RemoveFromClassList(\"classC\")");
                playStation.RemoveFromClassList("classC");
                GetClasses(playStation);
            };
        }

        {//style.color = 
            tc.Q<Button>(name = "Set").clicked += ()=>
            {
                Debug.Log("style.color = Color.magenta;");
                playStation.style.color = Color.magenta;
                Debug.Log($"style.color: {playStation.style.color}");
                Debug.Log($"style.color.keyword: {playStation.style.color.keyword}");//=>Undefined
            };
            tc.Q<Button>(name = "Null").clicked += ()=>
            {
                Debug.Log("style.color = StyleKeyword.Null");
                playStation.style.color = StyleKeyword.Null;
                Debug.Log($"style.color: {playStation.style.color}"); //デフォルトはNull
                // Debug.Log($"style.color.keyword: {playStation.style.color.keyword}");//=>Null
            };
        }
        // Debug.Log($"playStation.style.flexGrow.keyword: {playStation.style.flexGrow.keyword}");//=>Null //Noneを期待したがNullだった

        static void GetClasses(VisualElement ve)
        {
            string str = "";
            foreach(string s in ve.GetClasses()) str += s + ", ";
            Debug.Log($"GetClasses(): {str}");
        }

        static void AddStyleSheet(VisualElement _ve, StyleSheet _uss)
        {
            if(!_ve.styleSheets.Contains(_uss))
            {
                Debug.Log($"Add({_uss.name})");
                // Debug.Log($"styleSheets.count: {_ve.styleSheets.count}"); //=>2
                _ve.styleSheets.Add(_uss);
                Debug.Log($"styleSheets.count: {_ve.styleSheets.count}"); //=>3
            }
            else
            {
                Debug.Log($"Already Contains: {_uss.name}");
            }
        }

        static void RemoveStyleSheet(VisualElement _ve, StyleSheet _uss)
        {
            if(_ve.styleSheets.Contains(_uss))
            {
                Debug.Log($"Remove({_uss.name})");
                _ve.styleSheets.Remove(_uss);
                Debug.Log($"styleSheets.count: {_ve.styleSheets.count}");//Remove(_uss)で即時に削除されカウントが即時に減る 3=>2
            }
            else
            {
                Debug.Log($"No Contains: {_uss.name}");
            }
        }

        rootVisualElement.Add(new Button(()=> 
        {
            Debug.Log("MarkDirtyRepaint");
            rootVisualElement.MarkDirtyRepaint(); 
        }){text = "rootVisualElement MarkDirtyRepaint"}); 

        Debug.Log("container_0.styleSheets.Add(uss_A)(uss_B)(uss_C);");
            container_0.styleSheets.Add(uss_A);
            container_0.styleSheets.Add(uss_B);
            container_0.styleSheets.Add(uss_C); 

        {//ボツ
        // tc.Query<Button>(name = "Container_0_Add").Children<Button>(name = "uss_A").First().clicked += () =>

        // VisualElement Button_container_0 = new();
        // Button_container_0.style.flexDirection = FlexDirection.Row;
        // rootVisualElement.Add(Button_container_0);

        // var b_c_0_A = new Button(()=>
        // {
        //     if(container_0.styleSheets.Contains(uss_A))
        //     {
        //         Debug.Log("Remove(uss_A)");
        //         container_0.styleSheets.Remove(uss_A);
        //         Debug.Log($"styleSheets.count: {container_0.styleSheets.count}");//Remove(uss_A)で即時に削除されカウントが即時に減る 3=>2
        //     }
        //     else
        //     {
        //         Debug.Log($"No Contains: {uss_A.name}");
        //     }
        // }){text = $"container_0 remove USS: {uss_A.name}"};
        // b_c_0_A.style.flexBasis = new StyleLength(
        //     new Length(33.33f, LengthUnit.Percent)
        // );
        // Button_container_0.Add(b_c_0_A);
        }
    }
}