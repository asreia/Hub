using S = System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EW_ResourceManagement : EditorWindow
{
    [MenuItem("UI Toolkit/EW_ResourceManagement")]
    static void GetWindow()
    {
        EditorWindow wnd = GetWindow<EW_ResourceManagement>("EW_ResourceManagement");
    }

    void CreateGUI()
    {
        VisualTreeAsset list_ScrObj_ListView_Asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/MyUXML/EW_ResourceManagement/EW_RM_List_ScrObj_ListView.uxml");
        VisualElement list_ScrObj_ListViewElement = list_ScrObj_ListView_Asset.CloneTree();

        Log log = new();
        rootVisualElement.Addd
        (
            new TwoPaneSplitView(fixedPaneIndex:0, fixedPaneStartDimension:500f, orientation: TwoPaneSplitViewOrientation.Horizontal).Addd
            (
                new TwoPaneSplitView(fixedPaneIndex:0, fixedPaneStartDimension:450f, orientation: TwoPaneSplitViewOrientation.Vertical).Addd
                (
                    new ScrollView(scrollViewMode:ScrollViewMode.VerticalAndHorizontal).Addd
                    (
                        new UnityEngine.UIElements.PopupWindow(){text = "SerializedObjectView"}.Addd
                        (
                            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/MyUXML/EW_ResourceManagement/EW_RM_SO.uxml").CloneTree()
                        ),
                        new UnityEngine.UIElements.PopupWindow(){text = "AssetDatabase"}.Addd
                        (
                            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/MyUXML/EW_ResourceManagement/EW_RM_AD.uxml").CloneTree()
                        ),

                        new UnityEngine.UIElements.PopupWindow(){text = "EditorUtility"}.Addd
                        (
                            new VisualElement(){name = "RootEditorUtility"}.Addd
                            (
                                new Button(){name = "SetDirty", text = "SetDirty"},
                                new Button(){name = "IsDirty", text = "IsDirty"}
                            )
                        ),
                        new UnityEngine.UIElements.PopupWindow(){text = "SceneManagement"}.Addd
                        (
                            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UXML/MyUXML/EW_ResourceManagement/EW_RM_SM.uxml").CloneTree()
                        )
                    ),
                    log.LogElement()
                ),
                new TwoPaneSplitView(fixedPaneIndex:1, fixedPaneStartDimension:350f, orientation: TwoPaneSplitViewOrientation.Vertical).Addd
                (
                    new VisualElement().Addd
                    (
                        new ListView(){name = "Exist_ScrObj_ListView"}, //flexGrow = 1f;
                        new VisualElement(){name = "ResourceButtons"}.Addd //VisualElementを噛ませると何故かListViewに潰される(height入れても効果なし)
                        (
                            new Button(){name = "Exist_ScrObj", text = "Exist ScrObj"}, //height = 20f; //ListViewが上にあるとButtonが潰れるのでheightを入れてる
                            new Button(){name = "Copy_AllScrObj_to_ListView", text = "Copy AllScrObj to ListView(itemsSource)"} //height = 20f;//↑と同じ理由
                            // new Button(()=>{ ShowAll_ScrObj();}){text = " ShowAll_ScrObj()"},
                        ),
                        new Button(){name = "UnloadUnusedAssets",text = "UnloadUnusedAssets"}
                    ),
                    //=======================================================================================================================
                    new VisualElement().Addd
                    (
                        new VisualElement(){name = "List_ScrObj_ListView_OperateButtons1"}.Addd
                        (
                            new TextField(){name = "LoadAssetAtPath_TextField", label = "LoadAssetAtPath"},
                            new Button(){name = "LoadAssetAtPath_Button", text = "Load"}
                        ),
                        new VisualElement(){name = "List_ScrObj_ListView_OperateButtons"}.Addd
                        (
                            new Button(){name = "Create_ScrObj", text = "Create_ScrObj"},
                            new Button(){name = "Clear_ScrObjList", text = "Clear ScrObjList"}
                        ),
                        new ListView(){name = "List_ScrObj_ListView"} //flexGrow = 1f;
                        // list_ScrObj_ListViewElement
                    )
                )
            )
        );

        ListView list_ScrObj_ListView = rootVisualElement.Q<ListView>(name:"List_ScrObj_ListView");
        list_ScrObj_ListView.style.flexGrow = 1f;
        list_ScrObj_ListView.fixedItemHeight = 50;
        list_ScrObj_ListView.itemsSource = new List<SerializedObject>();
        //リオーダブル対応をしないといけないと思ったが、bindで呼んだ(element, index)に対して必ず同じ(element, index)でunbindを呼んでくれる、かつ、
            //bind時にunbindの時のCallbackを登録しているためitemsSourceのList入れ替え操作後でも、操作前の(element, index)のCallbackが呼ばれるため、問題が起こらかった
            //(削除操作後にunbindを呼んだ場合、既に存在しないitemsSource[index]を参照しnullポインタ例外が出ると思う)
        list_ScrObj_ListView.reorderable = true; list_ScrObj_ListView.reorderMode = ListViewReorderMode.Animated;
        // list_ScrObj_ListView.itemIndexChanged += (beforeIndex, afterIndex)=>{Debug.Log($"beforeIndex: {beforeIndex}, afterIndex: {afterIndex}");};

        int elementCount = 0;
        list_ScrObj_ListView.makeItem = ()=>
        {
            // return list_ScrObj_ListViewElement;  //←↓ItemのElementを生成(new)する必要があるのでVisualTreeAsset.CloneTree()を使う
            var cloneTree = list_ScrObj_ListView_Asset.CloneTree();
            cloneTree.name = $"element_{elementCount}";
            // Debug.Log($"makeItem: {cloneTree.name}");
            // cloneTree.Q<TextField>(name:"name").label = cloneTree.name;
            elementCount++;
            return cloneTree;
        };

        List<(List<S.Action>, int)> unbindCallbackListList = new();//☆ Item内のelement設定
        bool nowRebinding = false; List<(VisualElement, int)> bindingElements = new();//☆ AllUnbindItems,AllBindItems

        // static class Ext
        // {

        // }
        void AllUnbindItems(ListView _listView)//==Unbind,bind,Refreshは要素数が変化するCallbackに対して使う==//☆================================================
        {
            nowRebinding = true;
            foreach((VisualElement element, int index) e in bindingElements)
            // {
                // Debug.Log($"index: {e.index}");
                _listView.unbindItem(e.element, e.index);
            // }
            nowRebinding = false;
        }
        void AllBindItems(ListView _listView)//===//☆====================================================================
        {
            nowRebinding = true;
            foreach((VisualElement element, int index) e in bindingElements.ToArray())
            // {
                // Debug.Log($"index: {e.index}");
                if(e.index < _listView.itemsSource.Count)
                    _listView.bindItem(e.element, e.index);
                else
                    bindingElements.Remove(e);
            // }
            nowRebinding = false;
        }
        //bindUnbindItemを呼ぶのはAllUnbindItems,AllBindItems,RefreshItems,ListViewのスクロール
        list_ScrObj_ListView.bindItem = (element, index)=>
        {
            if(!nowRebinding) bindingElements.Add((element,index));//☆

            // Debug.Log($"B:: {element.name}, index: {index}");
            // foreach((VisualElement element, int index) e in bindingElements){Debug.Log($"index: {e.index}");}

            SerializedObject so = (SerializedObject)list_ScrObj_ListView.itemsSource[index];
            ScrObj scrObj = (ScrObj)so.targetObject;
            List<S.Action> unbindCallbackList = new();//☆

            {//SerializedObject_DataBinding
                // Debug.Log($"so: {so.FindProperty("m_Name") == null}");
                element.Q<TextField>(name:"name").bindingPath = so.FindProperty("m_Name").propertyPath;

                element.Q<TextField>(name:"Set_FieldData").bindingPath = "fieldData";

                VisualElement rootElement = element.Q<VisualElement>(name:"root");
                rootElement.Bind(so);
                unbindCallbackList.Add(()=>{rootElement.Unbind();});
            }

            {//HideFlags
                // element.Q<EnumField>(name:"HideFlags").bindingPath = "m_ObjectHideFlags"; //bindingPathを設定してもうまくいかなかった
                EnumField hideFlagsField = element.Q<EnumField>(name:"HideFlags");
                EventCallback<ChangeEvent<S.Enum>> HF_CB = (evt)=>
                {
                    scrObj.hideFlags = (HideFlags)evt.newValue;
                    //RefreshItem(index)はbindItemしか呼ばないため連続でCallbackが登録され、かつ登録したCallbackを呼ぶと2の累乗倍にbindItemが走る
                    list_ScrObj_ListView.RefreshItem(index);//NotEditableの反映
                    //そのため、↑のbindItemに対して↓のunbindItemを呼ぶ必要がある(バグ?)
                    list_ScrObj_ListView.unbindItem(element, index);
                };
                //bindItem HideFlags
                hideFlagsField.RegisterCallback<ChangeEvent<S.Enum>>(HF_CB);
                hideFlagsField.SetValueWithoutNotify(scrObj.hideFlags);
                //unBindItem HideFlags
                S.Action hideFlagsUnregisterCallback = ()=>{hideFlagsField.UnregisterCallback<ChangeEvent<S.Enum>>(HF_CB);};
                unbindCallbackList.Add(hideFlagsUnregisterCallback);
            }

            {//CreateAsset_ScrObj
                Button createAssetButton = element.Q<Button>("CreateAsset");
                TextField CreateAsset_Folder = rootVisualElement.Q<TextField>(name:"CreateAsset_Folder");

                S.Action createAssetCallback = ()=>
                {
                    string path = CreateAsset_Folder.value + scrObj.name + ".asset";
                    log.Write($"CreateAsset_ScrObj({scrObj.name}) path: {path}");
                    AssetDatabase.CreateAsset(scrObj, path);
                };
                
                createAssetButton.clicked += createAssetCallback;
                S.Action removeCreateAssetCallback = ()=>{createAssetButton.clicked -= createAssetCallback;};
                unbindCallbackList.Add(removeCreateAssetCallback);
            }

            {//destroyImmediate
                Button destroyImmediateButton  = element.Q<Button>("DestroyImmediate");
                S.Action destroyImmediateCallback = ()=>
                {
                    // Debug.Log("AllUnbindItems()");
                    //bindingElements(表示分のelement郡)を全アンバインド(element郡とitemsSourceを切断)=======//☆========================================
                    AllUnbindItems(list_ScrObj_ListView);
                    // Debug.Log("Destroy_ScrObj(so)");
                    // Destroy_ScrObj(so);
                    {
                        Object.DestroyImmediate((ScrObj)so.targetObject/*, true*/);        //EditModeなのでImmediate 
                        // List_ScrObj.Remove((ScrObj)so_scrObj.targetObject);
                        list_ScrObj_ListView.itemsSource.Remove(so);
                    }
                    // Debug.Log("AllBindItems()");
                    //Projectフォルダで手動でScrObjのassetファイルを削除した場合、削除時ににAll＠❰Un❱bindItems(～)を呼べないため、ここでエラーになる
                    //itemsSourceの要素数以下のbindingElementsを全バインド(element郡とitemsSourceを接続)===//☆====================================================
                        //itemsSourceの要素数の再バインド & 不要な表示elementの削除(unbindItem(～)は呼ばれない(呼んだらOutRangeExceptionを発生させる可能性がある))、
                        //もしくは、新たな表示elementの追加(bindItemは呼ばれる)
                    AllBindItems(list_ScrObj_ListView);
                    // Debug.Log("RefreshItems()");
                    list_ScrObj_ListView.RefreshItems();//☆
                };
                // element.Q<Button>("DestroyImmediate").clicked -= destroyImmediateCallback; 
                destroyImmediateButton.clicked += destroyImmediateCallback;
                S.Action RemoveDestroyImmediateCallback = ()=>{destroyImmediateButton.clicked -= destroyImmediateCallback;};
                unbindCallbackList.Add(RemoveDestroyImmediateCallback);//☆
            }

            {//callbackListへ追加
                unbindCallbackListList.Add((unbindCallbackList, index));//☆

                // string str = "B_index: ";
                // foreach((List<S.Action> b_unbindCallbackList, int b_index) e in unbindCallbackListList) str += $"{e.b_index}, ";
                // Debug.Log(str);
            }
        };
        list_ScrObj_ListView.unbindItem = (element, index)=>
        {
            if(!nowRebinding) bindingElements.Remove((element,index));//☆
            // Debug.Log($"U:: {element.name}, index: {index}");
            // foreach((VisualElement element, int index) e in bindingElements){Debug.Log($"index: {e.index}");}

            SerializedObject so = (SerializedObject)list_ScrObj_ListView.itemsSource[index];

            // element.Q<VisualElement>(name:"root").Unbind();

            {//新//☆
                foreach((List<S.Action> b_unbindCallbackList, int b_index) e in unbindCallbackListList.ToArray())
                {
                    if(e.b_index == index)
                    {
                        foreach(S.Action UnbindCallback in e.b_unbindCallbackList)
                        {
                            UnbindCallback();
                        }
                        unbindCallbackListList.Remove(e);
                        break;
                    }

                }
                // string str = "U_index: ";
                // foreach((List<S.Action> b_unbindCallbackList, int b_index) e in unbindCallbackListList) str += $"{e.b_index}, ";
                // Debug.Log(str);
            }
        };

        {//List_ScrObj_ListView_OperateButtons1
            VisualElement list_ScrObj_ListView_OperateButtons1 = rootVisualElement.Q<VisualElement>(name:"List_ScrObj_ListView_OperateButtons1");
            list_ScrObj_ListView_OperateButtons1.style.flexDirection = FlexDirection.Row;
            {//LoadAssetAtPath_TextField
                TextField loadAssetAtPath_TextField = rootVisualElement.Q<TextField>(name:"LoadAssetAtPath_TextField");
                loadAssetAtPath_TextField.style.flexGrow = 1f;
                {//LoadAssetAtPath_Button
                    Button loadAssetAtPath_Button = rootVisualElement.Q<Button>(name:"LoadAssetAtPath_Button");
                    loadAssetAtPath_Button.style.width = new Length(50);

                    loadAssetAtPath_Button.clicked += ()=>
                    {
                        AllUnbindItems(list_ScrObj_ListView);
                        list_ScrObj_ListView.itemsSource.Add(new SerializedObject(AssetDatabase.LoadAssetAtPath<ScrObj>(loadAssetAtPath_TextField.value)));
                        AllUnbindItems(list_ScrObj_ListView);
                        list_ScrObj_ListView.RefreshItems();
                    };
                }
            }
        }

        {//List_ScrObj_ListView_OperateButtons
            VisualElement list_ScrObj_ListView_OperateButtons = rootVisualElement.Q<VisualElement>(name:"List_ScrObj_ListView_OperateButtons");
            list_ScrObj_ListView_OperateButtons.style.flexDirection = FlexDirection.Row;

            {//Create ScrObj
                Button create_ScrObj = rootVisualElement.Q<Button>(name:"Create_ScrObj");
                create_ScrObj.style.flexGrow = 1f;

                int nameIndex = 0;
                create_ScrObj.clicked += ()=>       
                {
                    // Debug.Log("C_AllUnbindItems()");
                    AllUnbindItems(list_ScrObj_ListView);
                    {//Create_ScrObj
                        ScrObj scrObj = ScriptableObject.CreateInstance<ScrObj>();
                        scrObj.name = "ScrObj_" + nameIndex.ToString(); nameIndex++;
                        // List_ScrObj.Add(scrObj);
                        list_ScrObj_ListView.itemsSource.Add(new SerializedObject(scrObj));
                        log.Write($"Create_ScrObj({scrObj.name})");
                    }
                    // Debug.Log("AllBindItems()");
                    AllBindItems(list_ScrObj_ListView);
                    // Debug.Log("RefreshItems()");
                    list_ScrObj_ListView.RefreshItems();
                };
            }

            {//Clear ScrObjList
                Button clear_ScrObjList = list_ScrObj_ListView_OperateButtons.Q<Button>(name:"Clear_ScrObjList");
                // clear_ScrObjList.style.width = Length.Percent(50f);
                clear_ScrObjList.style.flexGrow = 1f;

                clear_ScrObjList.clicked += ()=>
                {
                    log.Write($"Clear_ScrObjList (itemsSource.Clear())");
                    AllUnbindItems(list_ScrObj_ListView);
                    list_ScrObj_ListView.itemsSource.Clear();
                    AllBindItems(list_ScrObj_ListView);
                    list_ScrObj_ListView.RefreshItems();
                };
            }
        }

        ElementTreeLogClass.ElementTreeLog(rootVisualElement.panel);

        //未整理メモ
        // EW_RM_UI.hierarchy.Clear(); //Clear()をコメントアウトして同じElementを複数のリーフにAddしようとすると、最後にAddしたリーフのみ表示される(多分)
        //基本的にはElementの木構造に対して、Styleの設定やCallbackを登録する。Elementの木構造からあらゆるリソース(System.Object)の参照を保持し操作する
        //ChangeEvent<EventBase>はvalueに対してUI操作か、スクリプトから反応を返す(valueがstructまたはイミュータブルなclassなら反応できる。UnityObjectはSerializedバインディング)
            //多分それ以外は、UI内で閉じた反応
        //ローカル変数をCallbackでキャプチャしまくるのでローカル変数がクラスのフィールドに昇格しローカル変数がフィールドの様な扱いになっている(Callback間で共有している)
            //()=>{～}の"=>"にカーソルを当てるとキャプチャされた変数のリストが出てくる
        //画面遷移、別ウィンドウは、Callbackの中で別の関数を呼びそこに新たなCreateGUI()の様なものを書く?(画面遷移:rootVisualElement差し替え)
            //EditorWindowはScriptableObjectなのでFindで探せる。そこからフィールド経由でElementを渡しvalueを参照することもできそう
            //(しかし、バインディングされたSerializedObjectまでは参照できない)
        //処理の構造が大きくなる場合、別ScriptableObjectに処理を投げる(もしくはScriptableObject経由で外部と通信する UI -> SO -> 外部)
        //UI操作ログ(ListViewでなくディレクション↑で作れるかも)、CreateAsset(～)の横に赤い●をDisplayでisPersistentか、Existを色付け整形(複数Label)、AllDestroyまだ

        {//Exist_ScrObj_ListView 
            ListView exist_ScrObj_ListView = rootVisualElement.Q<ListView>(name:"Exist_ScrObj_ListView");
            // exist_ScrObj_ListView.style.height = new StyleLength(Length.Percent(45f)); 
            exist_ScrObj_ListView.style.flexGrow = 1f;
            exist_ScrObj_ListView.fixedItemHeight = 18f;

            exist_ScrObj_ListView.makeItem = ()=>
            {
                return new Label();
            };

            exist_ScrObj_ListView.bindItem = (element, index)=>
            {
                Label label  = (Label)element;
                string scrObjString = (string)exist_ScrObj_ListView.itemsSource[index];
                // Debug.Log($"scrObjString: {scrObjString}");

                label.text = scrObjString;
            };

            {//Button
                VisualElement resourceButtons = rootVisualElement.Q<VisualElement>(name:"ResourceButtons");
                resourceButtons.style.flexDirection = FlexDirection.Row;
                resourceButtons.style.height = 20f; //ListViewが上にあるとButtonが潰れるのでheightを入れたが効果なかった

                {//Exist ScrObj
                    Button exist_ScrObj = resourceButtons.Q<Button>(name:"Exist_ScrObj");
                    // exist_ScrObj.style.width = Length.Percent(50f);
                    exist_ScrObj.style.flexGrow = 1f;
                    exist_ScrObj.style.height = 20f; //ListViewが上にあるとButtonが潰れるのでheightを入れてる

                    exist_ScrObj.clicked += ()=>
                    {
                        log.Write("Exist_ScrObj");
                        exist_ScrObj_ListView.itemsSource = Exist_ScrObj().Item1;
                        exist_ScrObj_ListView.RefreshItems();
                    };
                }

                {//Copy AllScrObj to ListView 
                    Button copy_AllScrObj_to_ListView = resourceButtons.Q<Button>(name:"Copy_AllScrObj_to_ListView");
                    copy_AllScrObj_to_ListView.style.flexGrow = 1f;
                    copy_AllScrObj_to_ListView.style.height = 20f; //ListViewが上にあるとButtonが潰れるのでheightを入れてる

                        copy_AllScrObj_to_ListView.clicked += ()=>
                        {
                            log.Write("Copy exist AllScrObj to ListView");
                            AllUnbindItems(list_ScrObj_ListView);
                            list_ScrObj_ListView.itemsSource = Exist_ScrObj().Item2.Select((scrObj)=>new SerializedObject(scrObj)).ToList();
                            AllBindItems(list_ScrObj_ListView);
                            list_ScrObj_ListView.RefreshItems();

                            exist_ScrObj_ListView.itemsSource = Exist_ScrObj().Item1;
                            exist_ScrObj_ListView.RefreshItems();
                        };
                }

                {//UnloadUnusedAssets 
                    Button unloadUnusedAssets = rootVisualElement.Q<Button>(name:"UnloadUnusedAssets");

                    unloadUnusedAssets.clicked += ()=>
                    {
                        log.Write($"Resources.UnloadUnusedAssets()", Color.magenta);
                        Resources.UnloadUnusedAssets(); //UnloadUnusedAssets()は即時ではないみたい

                        DelayRefreshItems();

                        async void DelayRefreshItems()
                        {
                            await Task.Delay(10);
                            exist_ScrObj_ListView.itemsSource = Exist_ScrObj().Item1;
                            exist_ScrObj_ListView.RefreshItems();
                        }
                    };
                }

                (string[], ScrObj[]) Exist_ScrObj()
                {
                    Debug.Log("Exist_ScrObj");
                    ScrObj[] scrObjs = Resources.FindObjectsOfTypeAll<ScrObj>();
                    string[] str = new string[scrObjs.Length];
                    int count = 0;
                    foreach(ScrObj so in scrObjs)
                    {
                        str[count] = $"name: {so.name}, HideFlag: {so.hideFlags}, IsPersistent: {EditorUtility.IsPersistent(so)}, fieldData: {so.fieldData}";
                        count++;
                    }
                    return (str, scrObjs);
                }
            }
        }

        {//SerializedObjectView
            Color textColor = Color.cyan;
            VisualElement rootSerializedObjectView = rootVisualElement.Q<VisualElement>(name:"RootSerializedObjectView");

            ObjectField scrObj_ObjectField = rootSerializedObjectView.Q<ObjectField>(name:"ScrObj");

            {//Get Unity Object form ListView
                Button get_UnityObject_Button = rootSerializedObjectView.Q<Button>(name:"Get_UnityObject");

                get_UnityObject_Button.clicked += ()=>
                {
                    ScrObj scrObj = (ScrObj)((SerializedObject)list_ScrObj_ListView.selectedItem).targetObject;
                    log.Write($"Get_UnityObject: {scrObj.name}", textColor);
                    scrObj_ObjectField.value = scrObj;

                    {//UnityObject_fieldData
                        TextField u_fieldData_TextField = rootSerializedObjectView.Q<TextField>(name: "U_fieldData");

                        u_fieldData_TextField.BindProperty(new SerializedObject((ScrObj)scrObj_ObjectField.value).FindProperty("fieldData"));
                    }
                };
            }

            SerializedObject serializedObject = default;
            {//new SerializedObject
                Button new_SerializedObject = rootSerializedObjectView.Q<Button>(name:"new_SerializedObject");
                Label serializedObject_Label = rootSerializedObjectView.Q<Label>(name:"SerializedObject");

                new_SerializedObject.clicked += ()=>
                {
                    ScrObj scrObj = (ScrObj)scrObj_ObjectField.value;
                    log.Write($"new SerializedObject({scrObj})", textColor);
                    serializedObject = new SerializedObject(scrObj);
                    serializedObject_Label.text = $"serializedObject.ToString(): {serializedObject.ToString()}";
                };
            }

            {//ApplyModifiedProperties
                Button applyModifiedProperties = rootSerializedObjectView.Q<Button>(name:"ApplyModifiedProperties");

                applyModifiedProperties.clicked += ()=>
                {
                    log.Write($"ApplyModifiedProperties()", textColor);
                    serializedObject.ApplyModifiedProperties();
                };
            }

            {//Update
                Button update = rootSerializedObjectView.Q<Button>(name:"Update");

                update.clicked += ()=>
                {
                    log.Write($"Update()", textColor);
                    serializedObject.Update();
                };
            }

            {//SerializedObject_fieldData
                TextField fieldData = rootSerializedObjectView.Q<TextField>(name:"S_fieldData");

                {//FindProperty_get 
                    Button findProperty_get = rootSerializedObjectView.Q<Button>(name:"FindProperty_get");

                    findProperty_get.clicked += ()=>
                    {
                        fieldData.value = serializedObject.FindProperty("fieldData").stringValue;
                        log.Write($"\"{fieldData.value}\" = FindProperty(\"fieldData\").stringValue", textColor);
                    };
                }
                {//FindProperty_set
                    Button findProperty_set = rootSerializedObjectView.Q<Button>(name:"FindProperty_set");

                    findProperty_set.clicked += ()=>
                    {
                        log.Write($"FindProperty(\"fieldData\").stringValue = \"{fieldData.value}\"", textColor);
                        serializedObject.FindProperty("fieldData").stringValue = fieldData.value;
                    };
                }
            }
        }

        {//AssetDatabase
            Color textColor = Color.red;
            VisualElement rootAssetDatabase = rootVisualElement.Q<VisualElement>(name:"RootAssetDatabase");

            {//CreateAsset_Folder
                TextField CreateAsset_Folder = rootAssetDatabase.Q<TextField>(name:"CreateAsset_Folder");
                CreateAsset_Folder.value = "Assets/ScrObj/";
            }

            {//AssetDatabase OperateElement
                TextField assetPath = rootAssetDatabase.Q<TextField>(name:"AssetPath");

                {//ImportAsset
                    Button importAsset = rootAssetDatabase.Q<Button>(name:"ImportAsset");
                    importAsset.clicked += ()=> {AssetDatabase.ImportAsset(assetPath.value); log.Write($"ImportAsset({assetPath.value})",textColor);};
                }

                {//AddObjectToAsset
                    Button addObjectToAsset = rootAssetDatabase.Q<Button>(name:"AddObjectToAsset");
                    addObjectToAsset.clicked += ()=> 
                        {
                            log.Write($"textField.value: {assetPath.value}");
                            AssetDatabase.AddObjectToAsset((ScrObj)((SerializedObject)list_ScrObj_ListView.selectedItem).targetObject, assetPath.value);
                            AssetDatabase.SaveAssets(); //SaveAssets()しないとディスクに保存されない?
                            log.Write($"AddObjectToAsset({(ScrObj)((SerializedObject)list_ScrObj_ListView.selectedItem).targetObject}, {assetPath.value})",textColor);
                        };
                }

                {//GUIDFromAssetPath
                    Button gUIDFromAssetPath = rootAssetDatabase.Q<Button>(name:"GUIDFromAssetPath");
                    gUIDFromAssetPath.clicked += ()=> {log.Write($"GUIDFromAssetPath: {AssetDatabase.GUIDFromAssetPath(assetPath.value)}",textColor);};
                }

                {//DeleteAsset //list_ScrObj_ListViewで問題起こる
                    Button deleteAsset = rootAssetDatabase.Q<Button>(name:"DeleteAsset");
                    deleteAsset.clicked += ()=>
                    {
                        log.Write($"AssetDatabase.DeleteAsset({assetPath.value})",textColor);
                        // AllUnbindItems(list_ScrObj_ListView);
                        // //これは新たにassetをロードしているので既存のScrObjではない。そして既存のScrObjを集める関数もない
                        // ScrObj scrobj = AssetDatabase.LoadAssetAtPath<ScrObj>(assetPath.value);
                        // Debug.Log($"Contains(scrobj): {list_ScrObj_ListView.itemsSource.Contains(scrobj)}");
                        // list_ScrObj_ListView.itemsSource.Remove(scrobj);
                        AssetDatabase.DeleteAsset(assetPath.value);//既存のScrObjをRemove(scrobj)できないのでAllBindItems(～)時エラー
                        // AllBindItems(list_ScrObj_ListView);
                        // list_ScrObj_ListView.RefreshItems();
                    };
                }
            }

            {//SaveAssets
                Button saveAssets = rootAssetDatabase.Q<Button>(name:"SaveAssets");
                saveAssets.clicked += ()=> {AssetDatabase.SaveAssets(); log.Write($"SaveAssets()",textColor);};
            }

            {//Refresh 
                Button refresh = rootAssetDatabase.Q<Button>(name:"Refresh");
                refresh.clicked += ()=> {AssetDatabase.Refresh(); log.Write("Refresh", textColor);};
            }
        }

        {//EditorUtility
            Color textColor = Color.yellow;
            VisualElement rootEditorUtility = rootVisualElement.Q<VisualElement>(name:"RootEditorUtility");
            rootEditorUtility.style.flexDirection = FlexDirection.Row;

            {//SetDirty
                Button setDirty = rootEditorUtility.Q<Button>(name:"SetDirty");
                setDirty.style.flexGrow = 1f;

                setDirty.clicked += ()=>
                {
                    EditorUtility.SetDirty((ScrObj)((SerializedObject)list_ScrObj_ListView.selectedItem).targetObject);
                    log.Write($"SetDirty({(ScrObj)((SerializedObject)list_ScrObj_ListView.selectedItem).targetObject})",textColor);
                };
            }

            {//IsDirty
                Button isDirty = rootEditorUtility.Q<Button>(name:"IsDirty");
                isDirty.style.flexGrow = 1f;

                isDirty.clicked += ()=>
                {
                    log.Write($"EditorUtility.IsDirty: {EditorUtility.IsDirty((ScrObj)((SerializedObject)list_ScrObj_ListView.selectedItem).targetObject)}", textColor);
                };
            }
        }

        {//RootSceneManagement
            Color textColor = Color.green;
            VisualElement rootSceneManagement = rootVisualElement.Q<VisualElement>(name:"RootSceneManagement");
            {//LoadScene_Single
                Button loadScene_Single = rootSceneManagement.Q<Button>(name:"LoadScene_Single");
                loadScene_Single.clicked += ()=>
                {
                    string sceneName = "New Scene";
                    log.Write($"LoadScene_Single({sceneName})", textColor);
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName/*％❰, UnityEngine.SceneManagement.LoadSceneMode.Single❱*/);
                };
            }
            
            {//loadScene_Additive
                Button loadScene_Additive = rootSceneManagement.Q<Button>(name:"LoadScene_Additive");
                loadScene_Additive.clicked += ()=>
                {
                    string sceneName = "New Scene";     //Scene名のstringをBuildIndexに対応させるメソッドはないのか?Sceneも
                    log.Write($"LoadScene_Additive({sceneName})", textColor);
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                };
            }
            
            {//UnloadSceneAsync
                Button unloadSceneAsync = rootSceneManagement.Q<Button>(name:"UnloadSceneAsync");
                unloadSceneAsync.clicked += ()=>
                {
                    log.Write("UnloadSceneAsync", textColor);
                    UnityEngine.SceneManagement.Scene scene =
                        UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
                };
            }
            
            {//SaveScene
                Button saveScene = rootSceneManagement.Q<Button>(name:"SaveScene");
                saveScene.clicked += ()=>
                {
                    UnityEngine.SceneManagement.Scene scene = 
                        UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                    log.Write($"SaveScene({scene.name})", textColor);
                };
            }
        }
    }
}
