# UIElementsリファレンス

## VisualElement (コンテナ)

### 概要

- VisualElementは他の全てのElementの**基礎となる機能を持つ**Elementで、**他の全てのElement**は**このElementから継承**されます
- VisualElementは、IEventHandler -> **CallbackEventHandler** -> **Focusable** -> **VisualElement** という継承構造を持ちます
  - class VisualElement: このクラス内に複数の機能を持ちます
    - Hierarchy **hierarchy**: SceneのHierarchyの様に、Elementの**木構造**に追加,削除,取得などの**操作**を行うメソッド郡があります
    - VisualElementStyleSheetSet **styleSheets**: 
    - IStyle **style**: 
    - styleの **.Class操作**: 
  - class Focusable: 
  - class CallbackEventHandler: 

### メソッド

VisualElement();
VisualElement this[int key] { get; }
この要素をレンダリングするか否かを示す。
bool visible { get; set; }
多分、UXMLのnameと同じ
string name { get; set; }
この要素の物理階層へのアクセス
    (VisualElementの木構造の操作ができるみたい)
**Hierarchy** hierarchy { get; }
string tooltip { get; set; }
この要素に付属するスタイルシートを操作する VisualElementStyleSheet を返します。
    (VisualElement(owner(ルート?))へのStyleSheetの追加削除操作ができるみたい。多分StyleSheetはコードで生成不可、USSファイルからのみロードして生成可能)
**VisualElementStyleSheetSet** styleSheets { get; }
この要素のスタイルオブジェクトへの参照。
    (このVisualElementへのスタイル操作)
**IStyle style** { get; }
生成された要素が VisualTreeAsset からクローンされたものである場合、アセットリファレンスを格納する。
VisualTreeAsset visualTreeAssetSource { get; }
リストに追加するクラスの名前。
void AddToClassList(string className);
この要素のクラス一覧からクラスを検索します。
bool ClassListContains(string cls);
この要素のクラスリストからすべてのクラスを削除します。AddToClassList
void ClearClassList();
この要素が子要素の祖先である場合に true を、そうでない場合に false を返す。
    (直下のみではないからHierarchyには含まれていない?)
bool Contains(VisualElement child);
指定された名前のクラスを有効または無効にします。
    bool enableがtureなら追加、falseなら削除
void EnableInClassList(string className, bool enable);
この要素に対応するクラスを取得します。
IEnumerable<string> GetClasses();
次のフレームでVisualElementの再描画をトリガーします。
void MarkDirtyRepaint();
要素のクラスリストからクラスを削除します。
void RemoveFromClassList(string className);
この要素を親階層から削除する。
void RemoveFromHierarchy();
VisualElement の有効状態を変更します。
    無効化されたVisualElementはほとんどのイベントを受信しません。
void SetEnabled(bool value);
クラス一覧から指定されたクラス名を追加するかどうかを切り替えます。
    実行毎に有効無効がトグルする
void ToggleInClassList(string className);

Hierarchy

VisualElement this[int key] { get; }
階層構造におけるこの要素の物理的な親。
VisualElement parent { get; }
このオブジェクトの contentContainer 内の子要素の数。
int childCount { get; }
この要素のcontentContainerに要素を追加する。
void Add(VisualElement child);
contentContainerに含まれる要素を返します。
IEnumerable<VisualElement> Children();
この要素の contentContainer からすべての子要素を削除する。
void Clear();
位置の子要素を取得する。
VisualElement ElementAt(int index);
bool Equals(Hierarchy other);
override int GetHashCode();
指定された VisualElement の Hierarchy 内のインデックスを取得します。
int IndexOf(VisualElement element);
この要素の contentContainer に要素を挿入する。
void Insert(int index, VisualElement child);
この子を階層から削除する
 void Remove(VisualElement child);
この位置にある子要素をこの要素の contentContainer から削除する。
void RemoveAt(int index);
bool operator ==(Hierarchy x, Hierarchy y);
bool operator !=(Hierarchy x, Hierarchy y);
