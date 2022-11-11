using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class ListViewTest : EditorWindow {

    [MenuItem("UI Toolkit/ListViewTest")]
    private static void ShowWindow()
    {
        var window = GetWindow<ListViewTest>();
        window.titleContent = new GUIContent("ListViewTest");
    }

    private void CreateGUI()
    {
        // Dictionary<string, int> dict = new(){{"りんご",100}, {"みかん",200}, {"おぎや",300}, {"みるく_",400}}; //DictionaryはIListじゃなかった..
            // Debug.Log($"dict[\"りんご\"]: {dict["りんご"]}");

        // ListView listView = new(new List<(string, int)>{("りんご",100), ("みかん",200), ("おぎや",300), ("みるく_",400)});
        ListView listView = new();
        int count = 0; int Num()=>count++;
        listView.itemsSource = new List<(string, int)>{("りんご",100), ("みかん",200), ("おぎや",300), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400)/*};//*/, ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400), ("みるく_"+ Num(),400)};

        listView.makeItem = ()=>{Debug.Log("makeItem============");return new Label();}; //表示範囲分は作られる
        listView.bindItem = (label, index)=>{(label as Label).text = index + ": " + (((string name, int))listView.itemsSource[index]).name;
        Debug.Log($"bindItem: {(label as Label).text}");//ListViewの表示内に入ると呼ばれる
        };
        listView.unbindItem = (label, index)=>{Debug.Log($"unbindItem: {(label as Label).text}");}; //ListViewの表示外になると呼ばれる
        listView.destroyItem = (label)=>{Debug.Log($"destroyItem: {(label as Label).text}");}; //表示されるElementが破棄される時、呼ばれる。(Rebuild()で完全に作り直される時、呼ばれた)

        bool removeFlag = true;

        listView.onSelectionChange += (items)=>
            {
            // var enumerator = items.GetEnumerator();
            // enumerator.MoveNext();
            // var e = ((string, int en))enumerator.Current;
            //↓↓↓↓↓↓↓↓↓↓↓↓↓↓
            // if(items != null){
            if(removeFlag){
                // var e = ((string, int en))items.First();
                // Debug.Log($"{e.en}円");
                //↓↓↓↓↓↓↓↓↓↓↓↓↓↓
                foreach(var e in items)Debug.Log($"{(((string,int en))e).en}円");
            }else removeFlag = true;
            };
        listView.viewDataKey = "aiueo";//設定しても選択は保存しない
        listView.headerTitle = "じどうはんざいき"; //ListViewの上側にタイトルが付く
        listView.showFoldoutHeader = true; //trueにするとListViewを折り畳める
        listView.showBoundCollectionSize = true; //trueにするとheaderTitleの右横にIListの要素が表示される
        listView.showAddRemoveFooter = true; //このプロパティは、リストビューにフッタが追加されるかどうかを制御する。([+ -]のフッタが付いた)
        listView.reorderable = true; //trueにすると、リオーダブル(順番入れ替え)が出来るようになる(itemsSourceも並び替わっている)
        listView.reorderMode = ListViewReorderMode.Animated;//AnimatedにするとインスペクターのReorderableListのようにアニメーションする！
        listView.selectionType = SelectionType.Multiple; //None:選択不能, Single:一つのみ, Multiple:複数選択可能(Controlで選択するとonSelectionChangeが最初の選択で実行してしまう訂正:First()で先頭だけ取っているので最初の選択から最後まで入っている)
        Debug.Log($"listView.showAlternatingRowBackgrounds: {listView.showAlternatingRowBackgrounds}");//=>None
        //>コレクションビューの行の背景色を交互に表示するためのオプションです。(たぶん、良くある、背景のグレーの濃いと薄いを交互させるヤツだと思うが、全てのenumを試しても何も変わらなかった)
        listView.showAlternatingRowBackgrounds = AlternatingRowBackground.All; //Noneがデフォルト。良く分からない
        listView.fixedItemHeight = 100f;
        Debug.Log($"listView.virtualizationMethod: {listView.virtualizationMethod}");//=>FixedHeightがデフォルト
        //DynamicHeightにすると普通にElementの大きさのままFlexDirection.Columnしたように整列される(fixedItemHeightの値は無視される)(負荷は重いらいし)
        listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        Debug.Log($"listView.horizontalScrollingEnabled: {listView.horizontalScrollingEnabled}");//=>Falseがデフォルト
        listView.horizontalScrollingEnabled = true; //trueにすると必要な時に水平方向のスクロールバーがでる
        listView.showBorder = true; //trueにするとListViewのList部分の枠にボーダーが付く

        listView.itemsSourceChanged += ()=>{Debug.Log("itemsSourceChanged=============");}; //分からん
        listView.onItemsChosen += (objs)=>{foreach(var e in objs)Debug.Log($"onItemsChosen: {(((string s, int))e).s}");}; //分からん
        listView.itemIndexChanged += (n0,n1)=>{Debug.Log($"itemIndexChanged: {n0}, {n1}");};//reorderableのドラッグアンドドロップで順番を入れ替えた時、n0に入れ替え前のindex、n1に入れ替え後のindex、を引数にしたCallbackを呼ぶ
        listView.onSelectedIndicesChange += (objs)=>{foreach(var e in objs)Debug.Log($"onSelectedIndicesChange: {e}");};//onSelectionChangeのindex版、選択順に入っている

        listView.itemsRemoved += (EnumIndex)=> //[+ -]のフッタの[-]の押下時とBaseListViewController.RemoveItem(❰int¦List<int>❱ ind❰ex¦ices❱)しか実行されていない
        {
            // Debug.Log($"itemsRemoved: {EnumIndex.First()}");
            //↓↓↓↓↓↓↓↓↓↓↓↓↓↓
            foreach(var e in EnumIndex) Debug.Log($"itemsRemoved: {e}");
            removeFlag = false;
        };
        listView.itemsAdded += (EnumIndex)=> //一度も実行されていない(フッタの[+]を押すとエラー。[+]を押した時の挙動の設定も不明)
        {
            Debug.Log($"itemsAdded: {EnumIndex.First()}");
        };
        //listViewは他にもドラッグアンドドロップとかあって奥が深そうなのでココまで。。

        rootVisualElement.Add(new Button(()=>
        {
            listView.Rebuild();
        }){text = "Rebuild()"});
        rootVisualElement.Add(new Button(()=>
        {
            listView.itemsSource.Add(("============================とめる============================", 4/*4.2f*/));//4.2fは型エラ
            // listView.Rebuild();
            listView.RefreshItems();
            {//ボツ
            // Debug.Log($"listView.itemsSource.RemoveAt(3): {listView.itemsSource[3]}");
            // listView.itemsSource.RemoveAt(3);//直接けしたらListViewにすぐに反映されずダメそう
            // removeFlag = false;listView.RemoveFromSelection(3);
            }
        }){text = "itemsSource.Add((\"とめる\",4))"});
        rootVisualElement.Add(new Button(()=>
        {
            listView.itemsSource.RemoveAt(0);
            listView.RefreshItems();
        }){text = "itemsSource.RemoveAt(0)"});

        rootVisualElement.Add(new Button(()=>
        {
            foreach(var e in listView.itemsSource) Debug.Log($"{e}");
        }){text = "Show_itemsSource"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log($"listView.contentContainer == null: {listView.contentContainer == null}");//=>True..
            listView
            // .contentContainer
            .Query<VisualElement>()
            .ForEach((ve)=>
            {
                Debug.Log($"label.text: {ve.name}");
            });
        }){text = "contentContainer.Query<Label>().ForEach((label)=>"});
        rootVisualElement.Add(new Button(()=>{//ボタンを押したら選択状態が剥げて空になるかなっと思ったけど、青が選択状態じゃなく薄い灰色が選択状態でありボタンを押しても選択状態が剥がれていなかったため、ボタンを連続で押しても選択状態を維持し、選択状態への参照をすることができた
            Debug.Log($"selectedItem: {(((string s, int))listView.selectedItem).s}");
            Debug.Log($"selectedIndex: {listView.selectedIndex}");
            foreach(var e in listView.selectedItems)Debug.Log($"selectedItems: {(((string s, int))e).s}");
            foreach(var e in listView.selectedIndices)Debug.Log($"selectedIndices: {e}");
        }){text = "selected～"});

        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("AddToSelection(4)");
            listView.AddToSelection(4); //ok期待通り
        }){text = "AddToSelection(4)"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("ClearSelection()");
            listView.ClearSelection(); //ok期待通り
        }){text = "ClearSelection()"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("RemoveFromSelection(5)");
            listView.RemoveFromSelection(5); //ok期待通り
        }){text = "RemoveFromSelection(5)"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("SetSelection(new[]{2,3,5,4})");
            listView.SetSelection(new[]{2,3,5,4}); //ok期待通り
        }){text = "SetSelection(new[]{2,3,5,4})"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("SetSelection(6)");
            listView.SetSelection(6); //ok期待通り
        }){text = "SetSelection(6)"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("ScrollToItem(50)");
            listView.ScrollToItem(50); //ok期待通り
        }){text = "ScrollToItem(50)"});

        rootVisualElement.Add(new Label("BaseListViewController"));
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("AddItems(4)");
            listView.viewController.AddItems(4); //itemsSourceの数を4以下にしてから実行してもエラーよく分からん
        }){text = "AddItems(4)"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("RemoveItem(5)");
            listView.viewController.RemoveItem(5); //ok期待通り //itemsRemovedのCallbackも実行される
        }){text = "RemoveItem(5)"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("RemoveItems(new List<int>(){4,3,5,7})");
            listView.viewController.RemoveItems(new List<int>(){4,3,5,7}); //ok期待通り //itemsRemovedのCallbackも実行される
        }){text = "RemoveItems(new List<int>(){4,3,5,7})"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log("Move(2,4)");
            listView.viewController.Move(2,4); //ok期待通り
        }){text = "Move(2,4)"});
        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log($"NeedsDragHandle(6):{listView.viewController.NeedsDragHandle(6)}"); //今のセッティングではtrue
        }){text = "NeedsDragHandle(6)"});

        rootVisualElement.Add(listView);
        rootVisualElement.Add(new Label("======================================"));
        // rootVisualElement.Add(new TreeView()); //なんも出ない余白は空いた
    }
}
