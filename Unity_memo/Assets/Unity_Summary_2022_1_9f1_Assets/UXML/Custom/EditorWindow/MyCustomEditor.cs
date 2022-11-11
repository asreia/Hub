using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//https://docs.unity3d.com/ja/2022.2/Manual/UIE-HowTo-CreateEditorWindow.html
public class MyCustomEditor : EditorWindow
{
  [SerializeField] private int m_SelectedIndex = -1;
  private VisualElement m_RightPane;

  [MenuItem("Tools/My Custom Editor")]
  public static void ShowMyEditor()
  {
    EditorWindow wnd = GetWindow<MyCustomEditor>();
    wnd.titleContent = new GUIContent("My Custom Editor");
    //ウィンドウサイズ
    wnd.minSize = new Vector2(450, 200);
    wnd.maxSize = new Vector2(1920, 720);
  }

  public void CreateGUI()
  {
    // プロジェクトに含まれる全スプライトのリストを取得する(GUID(string)を返す)
    string[] allObjectGuids = AssetDatabase.FindAssets("t:Sprite");
    var allObjects = new List<Sprite>();
    foreach (var guid in allObjectGuids)
    {
      allObjects.Add(AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)));
    }

    // TwoPaneSplitView(VisualElement)。スプリットの部分を動かせる(UI Builderには無かった)
    var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

    // EditorWindowに定義されている: VisualElement rootVisualElement { get; }
    rootVisualElement.Add(splitView);

    // TwoPaneSplitViewは常に正確に2つの子要素を必要とします。
    var leftPane = new ListView(); //UI Builderにある
    splitView.Add(leftPane);
    m_RightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal); //UI Builderにある
    splitView.Add(m_RightPane);

    // リストビューを初期化し、すべてのスプライトの名前を表示します。
    //ListViewの表示される領域に対して↓の2つが呼ばれる?
    //Func<VisualElement>
    leftPane.makeItem = () => new Label();
    //Action<VisualElement, int>
    leftPane.bindItem = (item, index) => { (item as Label).text = allObjects[index].name; };
    //これを設定しないとAction<IEnumerable<object>> onSelectionChangeのobjectが渡せない。あと長さも取ってる
    leftPane.itemsSource = allObjects;
    // leftPane.itemsSource = new List<int>{1,2,3,4};

    // ユーザーの選択に反応する。onSelectionChangeはAction<IEnumerable<object>>
    leftPane.onSelectionChange += OnSpriteSelectionChange;

    // ホットリロード(ドメインリロード)前の選択インデックスに戻す
    leftPane.selectedIndex = m_SelectedIndex;

    //選択範囲が変化したときに選択インデックスを保存する。itemsはIEnumerable<object>(多分選択されたobject)
    leftPane.onSelectionChange += (items) => { m_SelectedIndex = leftPane.selectedIndex;};
  }

  private void OnSpriteSelectionChange(IEnumerable<object> selectedItems)
  {
    // ペインに表示されている以前の内容をすべて消去する
    m_RightPane.Clear();

    // 選択されたスプライトを取得する
    var selectedSprite = selectedItems.First() as Sprite;
    if (selectedSprite == null)
      return;

    // 新規にImageコントロールを追加し、スプライトを表示する
    var spriteImage = new Image();//ImageはVisualElement
    spriteImage.scaleMode = ScaleMode.ScaleToFit;
    spriteImage.sprite = selectedSprite;

    // 右側ペインにImageコントロールを追加する
    m_RightPane.Add(spriteImage);
  }
}