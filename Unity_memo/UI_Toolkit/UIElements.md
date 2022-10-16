# UIElementsリファレンス

## VisualElement、Focusable、CallbackEventHandler (コンテナ)

### 概要

- VisualElementは他の全てのElementの**基礎となる機能を持つ**Elementで、**他の全てのElement**は**このElementから継承**されます
- VisualElementは、IEventHandler -> **CallbackEventHandler** -> **Focusable** -> **VisualElement** という継承構造を持ちます
  - class VisualElement: このクラス内に以下の複数の機能を持ちます
    - Hierarchy **hierarchy**: SceneのHierarchyの様に、Elementの**木構造**に取得,追加,削除などの**操作**を行うメソッド郡があります
    - VisualElementStyleSheetSet **styleSheets**: そのElementに適用されている**StyleSheetのコレクション**の様なもの?取得,追加,削除ができるようです
    - IStyle **style**: USSの**Styleの設定項目**が列挙されていて、取得,設定ができる
    - styleの **.Class操作**: **.Classの**追加,削除,包含などの**操作**がVisualElementクラス内に散りばめられている
  - class **Focusable**: Focus可能か?、**FocusするFocusを外す**、tabIndexを設定することが可能
  - class **CallbackEventHandler**: UIElementsのイベントシステムであり、イベント受信時の**CallBuck登録,解除**、**DefaultActionの実装**、などがある

### メソッド

#### VisualElement

##### 基本操作

- `VisualElement()`
  - コード上に**VisualElementを生成**する

- `string name { get; set; }`
  - UXML上のnameと同じ

- `void SetEnabled(bool value);`
  - VisualElement の**有効状態を変更**します。無効化されたVisualElementはほとんどの**イベントを受信しません**。
    - (多分GameObjectやComponentのEnableの様に有効,無効を切り替えると思われる)

- `VisualTreeAsset visualTreeAssetSource { get; }`
  - 生成された要素(this)が**VisualTreeAsset**から**クローン**されたものである場合、アセットリファレンス **(VisualTreeAsset?)を格納**する。
    - (多分、`VisualTreeAsset.CloneTree(VisualElement ve)`のveに**VisualTreeAsset**がセットされる(クローン元のVisualTreeAssetを保持しておく?))

##### Hierarchy操作

- `void RemoveFromHierarchy();`
  - このElementを親階層から削除する。

- `bool Contains(VisualElement child);`
  - このElement(this)が子Element(child)の**祖先**である場合に true を、そうでない場合に false を返す。

- `Hierarchy hierarchy { get; }`
  - このElementの物理階層へのアクセス(VisualElementの**木構造の操作**ができるみたい)

- **Hierarchy**
  - **親を取得**
    - `VisualElement parent { get; }`
      - 階層構造における**この(this)Element**の物理的な**親**。

  - **子の操作**
    - **取得**
      - `VisualElement this[int key] { get; }`
        - (恐らくthisがコンテナでその**子Elementのインデクサ**)

      - `IEnumerable<VisualElement> Children();`                                  ==試す==
        - コンテナに含まれるElementを返します。
          - (多分、**子Elementのイテレータを取得**する)

      - `int childCount { get; }`
        - この(this)Elementのコンテナ内の**子Elementの数**。

      - `VisualElement ElementAt(int index);`
        - **index番目の子Elementを取得**する。

      - `int IndexOf(VisualElement element);`
        - 引数のElementをHierarchy内(thisの直下(子)?)の**indexを取得**します。

    - **追加**
      - `void Add(VisualElement child);`
        - この(this)Elementのコンテナに**Elementを追加**する。

      - `void Insert(int index, VisualElement child);`
        - この(this)Elementのコンテナに**Elementを挿入**する。

    - **削除**
      - `void Clear();`
        - この(this)Elementの**コンテナからすべての子Elementを削除**する。

      - `void Remove(VisualElement child);`
        - 引数で指定された**子Element**を階層から**削除**する

      - `void RemoveAt(int index);`
        - 引数の**index番目の子Element**をこの(this)Elementのコンテナから**削除**する。

  - **比較**
    - `bool Equals(Hierarchy other);`
      - Hierarchy型の**Equals比較**(HierarchyのElementの木構造が同じかチェックする?)

    - `bool operator ==(Hierarchy x, Hierarchy y);`
      - ==比較(==なので参照が同じかチェック?)

    - `bool operator !=(Hierarchy x, Hierarchy y);`
      - !=比較

  - `override int GetHashCode();`
    - Hierarchyの**ハッシュ化**

##### owner(ルート?)へのStyleSheets操作

- `VisualElementStyleSheetSet styleSheets { get; }`
  - この(this)Elementに付属する**StyleSheetsを操作**する**VisualElementStyleSheetSetを返します**。
    - (VisualElement(owner(ルート?))へのStyleSheetの追加削除操作ができるみたい。多分**StyleSheetはコードで生成不可**、USSファイルからのみロードして生成可能)

##### **.Class操作**

- **取得**
  - `IEnumerable<string> GetClasses();`
    - この(this)Elementに対応する **.Classのイテレータを取得**します。

  - `bool ClassListContains(string cls);`
    - この(this)Elementの.Class一覧から引数の **.Classを検索**します。
      - (多分、存在するならtrue)

- **追加,削除**
  - `void AddToClassList(string className);`
    - (this)Elementの.Classリストから引数の **.Classを追加**します。

  - `void RemoveFromClassList(string className);`
    - (this)Elementの.Classリストから引数の **.Classを削除**します。

  - `void EnableInClassList(string className, bool enable);`
    - 第一引数の.Classを第二引数の**boolで有効無効を設定**する(.Classを追加または削除)

  - `void ToggleInClassList(string className);`
    - (this)Elementに引数の.Classが**追加されているなら削除、削除なら追加**する

  - `void ClearClassList();`
    - この(this)Elementの.Classリストから**全ての.Classを削除**します。

##### この(this)ElementのStyleへの参照(Styleの操作ができる)

- `IStyle style { get; }`

##### その他

- `string tooltip { get; set; }`
  - (多分カーソルをHoverすると説明が出てくるやつ)

- `bool visible { get; set; }`
  - このElementをレンダリングするか否かを示す。
    - (Styleのdisplayと違う?)

- `void MarkDirtyRepaint();`
  - 次のフレームでVisualElementの再描画をトリガーします。

#### CallbackEventHandler

- `bool HasBubbleUpHandlers()`                                  ==試す==
  - この(this)Elementにイベント伝搬 BubbleUp フェーズのイベントハンドラ がアタッチされている場合、true を返す。
    - (BubbleUpのCallbackが登録されているならtrue?)

- `bool HasTrickleDownHandlers()`
  - (this)ElementがTrickleDownフェーズのイベントハンドラを持つ場合、Trueを返します。
    - (TrickleDownのCallbackが登録されているならtrue?)

- `void RegisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()`
  - この(this)Elementに**TEventType**のイベントを**受信**した時に実行する**callbackを登録**する。**useTrickleDown**でBubbleUp(NoTrickleDown)かTrickleDownか選べる

- `void RegisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TUserArgsType userArgs, TrickleDown useTrickleDown = TrickleDown NoTrickleDown) where TEventType : EventBase<TEventType>, new()`
  - 通常のイベント受信時のコールバック**登録** + ユーザー定義引数(**TUserArgsType**)付き

- `UnregisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()`
  - この(this)Elementに**TEventType**のイベントを**受信**した時に実行する**callbackを登録解除**する。**useTrickleDown**でBubbleUp(NoTrickleDown)かTrickleDownか選べる

- `void UnregisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()`
  - 通常のイベント受信時のコールバック**登録解除** + ユーザー定義引数(**TUserArgsType**)付き

- `virtual void ExecuteDefaultAction(EventBase evt)`
  - イベントターゲットに登録されたコールバックが実行された後にロジックを実行します。ただし、イベントがデフォルトの動作を防止するようにマークされている場合はこの限りではありません。EventBase{T}.PreventDefault.
    - (**イベントディスパッチ動作の終了直前**(最後のBubbleUpの直後)で実行される?(オーバーライドする必要があり、evtを**evt.eventTypeId**で調べて今なんのイベントか知る必要がある))

- `virtual void ExecuteDefaultActionAtTarget(EventBase evt)`                                  ==試す==
  - イベントターゲットに登録されたコールバックが実行された後にロジックを実行します。ただし、そのイベントがデフォルトの動作を防止するようにマークされている場合を除きます。EventBase{T}.PreventDefault.
    - (**イベントディスパッチ動作のtargetの直後**で実行される?(オーバーライドする必要があり、evtを**evt.eventTypeId**で調べて今なんのイベントか知る必要がある))

- `abstract void SendEvent(EventBase e)`                                  ==試す==
  - イベントハンドラへイベントを送信する。

#### Focusable

- `bool focusable { get; set; }`
  - (this)Elementが**Focus可能**であればtrue
    - (設定もできる?)

- `virtual bool canGrabFocus { get; }`
  - (this)Elementに**Focusを当てることができる場合は true** を返す。

- `int tabIndex { get; set; }`
  - FocusリングのFocusタビリティをソートするために使用される整数。0以上でなければならない。
  - [Focusリング](..\画像\Focusリング.png)

- `virtual void Blur();`
  - (this)Elementに**Focusを外す**ように指示する。

- `virtual void Focus();`
  - この(this)Elementに**Focusを当てる**ようにする。

## イベント(evt)

### 概要

- UI Toolkitのイベントシステムは、**GUI操作,UIの内部状態の変化** などを**検知**し、
  UXMLの**root**からEventBase.**target**へ**イベント(evt)** を**必要とするノード**へ**ディスパッチ(送信)** して行きます
  - **必要とするノード**とは、UXMLのノードに**登録**されている`RegisterCallback<TEventType>`の`TEventType`が`typeof(TEventType) == evt.GetType`であり
    かつ [**enum TrickleDown**が合っている または **target**] の時、そのノード(Element)は**イベント(evt)を受信**します。(Callbackに**evtを引数に取る**)
- イベント(evt)の継承構造は、**EventBase** -> **EventBase<T>** -> ..各イベント(evt)のクラス列.. となっている
- >UI Toolkit は、オペレーティングシステムやスクリプトからのイベントをリッスンし、EventDispatcher でこれらのイベントをElementにディスパッチします。
- >UI Toolkit のイベントは、HTML イベントに似ています。

### メソッド

#### EventBase

- `long timestamp { get; }`
  - イベントが作成された時刻。

- `bool dispatch { get; }`
  - >イベントがビジュアル要素にディスパッチされているかどうかを示します。
    >イベントは、ディスパッチされている間は再ディスパッチすることができません。
    >イベントを再帰的にディスパッチする必要がある場合は、イベントのコピーを使用することを推奨します。
    - (イベントが**伝搬中ならばtrue?**)

- `IEventHandler target { get; set; }`
  - このイベントを受信したターゲットのビジュアル要素です。currentTargetとは異なり、このターゲットはイベントが伝搬経路上の他の要素に送信される際に変更されません。
    - **目標のIEventHandler**(各Elementにキャスト可能)

- `virtual IEventHandler currentTarget { get; internal set; }`
  - >イベントの現在のターゲットです。イベントハンドラが現在実行されている伝搬経路のVisualElementです。
    - **現在のIEventHandler**(各Elementにキャスト可能)

- `PropagationPhase propagationPhase { get; }`
  - 現在のプロパゲーションフェーズです。(enum)
    - (`PropagationPhase`(実行順): `None`, `TrickleDown`, `AtTarget`, `DefaultActionAtTarget`, `BubbleUp`, `DefaultAction`)
      - `None`以外は`dispatch`がtrue?

- `bool isDefaultPrevented { get; }`
  - このイベントに対してデフォルトアクションを実行しない場合はtrueを返す。
    - (`PreventDefault()`が呼ばれていたら**true**?)

- `bool isImmediatePropagationStopped { get; }`
  - このイベントに対して `StopImmediatePropagation()`が呼び出されたか否かを示す。

- `bool isPropagationStopped { get; }`
  - このイベントに対して `StopPropagation()`が呼び出されたかどうか。

- `bool tricklesDown { get; protected set; }`
  - TrickleDown フェーズにおいて、このイベントがイベント伝搬路に送信されるかどうかを返す。
    - (このイベントの種類(型)が`TrickleDown`で受信されるか? (イベントの種類によってはtargetのみ送られるイベントもある))

- `bool bubbles { get; protected set; }`
  - BubbleUp フェーズにおいて、このイベントがイベント伝搬路に送信されるかどうかを返す。
    - (このイベントの種類(型)が`BubbleUp`で受信されるか? (イベントの種類によってはtargetのみ送られるイベントもある))

- `bool pooled { get; set; }`
  - >イベントがイベントのプールから割り当てられたかどうか。

- `void PreventDefault()`
  - >このイベントに対して、デフォルトのアクションが実行されないようにするかどうかを示す。
    (呼ぶと`ExecuteDefaultAction＠❰AtTarget❱`を実行しない?)

- `void StopImmediatePropagation()`
  - >イベントの伝搬を即座に停止します。イベントは伝搬経路上の他の要素には送られません。このメソッドは、他のイベントハンドラが現在のターゲット上で実行されるのを**防ぎます**。
    - (多分、今ディスパッチしている**イベントの伝播経路の中**で現在実行している**登録されたCallback**以降、
      **登録されたCallback**を実行しない(`ExecuteDefaultAction＠❰AtTarget❱`は呼ぶ))

- `void StopPropagation()`
  - >このイベントの伝搬を停止します。伝搬経路上の他の要素にイベントは送信されません。このメソッドは、他のイベントハンドラが現在のターゲット上で実行されるのを**防ぐことはありません**。
    - (多分、今ディスパッチしている**イベントの伝播経路の中**で現在実行している**IEventHandlerの全ての登録されたCallback**以降、
      **登録されたCallback**を実行しない(`ExecuteDefaultAction＠❰AtTarget❱`は呼ぶ))

#### EventBase\<T>

- **TypeId**
  - 以下の関数は`if(evt.eventTypeId == TooltipEvent.TypeId())`という風に使うことで、送られて来たイベントが何のイベントか分かる。
    - `if(evt is TooltipEvent te)`でもいい?

  - `override long eventTypeId { get; }`
    - 送られてきたイベントの種類 (readOnly)

  - `static long TypeId()`
    - そのクラスのイベントの種類 (static関数 (const))

- `static T GetPooled(); //引数にT evtがいるはず`
  - プールからイベントを取得するらしい
    - 使用例: `using(KeyDownEvent keyDownEvent = KeyDownEvent.GetPooled(evt)){panel.visualTree.SendEvent(keyDownEvent)}`

## [Focusイベント (実行順)](https://docs.unity3d.com/ja/2022.2/Manual/UIE-Focus-Events.html)

- `relatedTarget`は`target`の対となる、`target`の**逆のFocus状態**のElement

- `FocusInEvent`
  - ElementがFocusを**得る前**に送信されます
  - `target`: Focusを得るElement
  - `relatedTarget`: Focusを失うElement
- `FocusEvent`
  - ElementがFocusを**得た後**に送信されます
  - `target`: Focusを得たElement
  - `relatedTarget`: Focusを失ったElement
- `FocusOutEvent`
  - ElementがFocusを**失う前**に送信されます
  - `target`: Focusを失うElement
  - `relatedTarget`: Focusを得るElement
- `BlurEvent`
  - ElementがFocusを**失った後**に送信されます
  - `target`: Focusを失ったElement
  - `relatedTarget`: Focusを得たElement

## [Changeイベント](https://docs.unity3d.com/ja/2022.2/Manual/UIE-Change-Events.html)

### 概要

- `ChangeEvent<T>`は、コントロールのフィールド?の**値(value)**が**変更**された時に送信されます。
  - `INotifyValueChanged<T>`を実装する全ての**コントロールに対して存在**する
  - `SetValueWithoutNotify()`で`ChangeEvent<T>`を**発生させずにvalueを変更**することが可能
  - **バインディングシステム**のリンク処理でも`ChangeEvent<T>`が**内部的に使用**されているらしい (UI -> シリアライズObj?のみ?)

### メソッド

- `ChangeEvent<T>`
  - `target`: 状態(value)の変更が発生するElement
  - `T previousValue`: 変更される前の値
  - `T newValue`: 変更された後の値

## [Clickイベント](https://docs.unity3d.com/ja/2022.2/Manual/UIE-Click-Events.html)

### 概要

- **ボタン以外の視覚的Element**の**クリック検出**をするために使用することができます。
  - 例えば、`Toggle`コントロールの実装では、`ClickEvent`を使用して、チェックマークを表示または非表示にし、コントロールの**値(value)を変更**します。
- `ClickEvent`の基底クラスは、`PointerEventBase`です。

### メソッド

- `ClickEvent`
  - `target`: **クリックが発生**したときの**マウス**またはポインティングデバイスの**下にあるElement**

## [Mouseイベント](https://docs.unity3d.com/ja/2022.2/Manual/UIE-Mouse-Events.html)

- マウスイベントの前には、必ず対応する`PointerEvent`があります

- **イベント**
  - `MouseDownEvent`: マウスボタンを**押す**ときに送信されます
  - `MouseUpEvent`: マウスボタンを**離す**ときに送信されます
  - `MouseMoveEvent`: マウスを**動かす**と送信されます
  - `WheelEvent`: **マウスホイールをアクティブ**にすると送信されます
  - `MouseEnterWindowEvent`: マウスが**ウィンドウ**に**入る**ときに送信されます
  - `MouseLeaveWindowEvent`: マウスが**ウィンドウ**を**離れる**ときに送信されます
  - `MouseEnterEvent`: マウスが**Element**またはその**子孫**に**入る**ときに送信されます
  - `MouseLeaveEvent`: マウスが**Element**またはその**子孫**から**離れる**ときに送信されます
  - `MouseOverEvent`: マウスが**Element**に**入る**ときに送信されます
  - `MouseOutEvent`: マウスが**Element**から**離れる**ときに送信されます
- **プロパティ**
  - `target`: **マウスキャプチャ**を受け取るElement。それ以外の場合は、**カーソルの下**の**一番上**の**選択可能**なElement
  - `button`: **押されたマウスボタン**を**識別するint**を返します (LMB:0, RMB:1, MMB:2)
  - `pressedButtons`: 現在**押されているマウスボタンの組み合わせ**を**識別するint**を返します (LMB:1, RMB:2, MMB:4)
    - `0 != (evt.pressedButtons & (1 << (int)MouseButton.LeftMouse))` // a << n <=> a * (2^n)
  - `modifiers`: キーボードイベント中に押された**修飾キー**を返します。修飾キーの例としては、 Shift、Ctrl、Alt キーなどがあります
  - `mousePosition`: パネル内の**マウスの位置** (スクリーン座標系とも呼ばれます) を返します
  - `localMousePosition`: `target`となるElementに対し**ローカル座標**を返します
  - `mouseDelta`: **前**のマウスイベント時のポインターと、**現在**のマウスイベント時のポインターの**位置の差**

## IPanelイベント

子を持てるVisualElemntは実際にはIPanelがそのVisualElemntを持っていて
実際にはIPanelが子への参照を持っている?(EventDispatcherをフィールドに持っている)
VisualElemnt :: IPanel panel { get; } 概要: この VisualElement が貼り付けられているパネル。
パネルイベントは、パネルとの関係が変化するときにビジュアル要素で発生します。例えば、ビジュアル要素をパネルに加えるとき (AttachToPanelEvent)、またはパネルから削除するとき (DetachFromPanelEvent) などです。
パネルイベントは、パネルの変更の発生時に直接影響を受ける階層内のビジュアル要素とその子孫にのみ送信されます。(つまり、変更したパネル以下全てにイベント送信)
AttachToPanelEvent	要素 (またはその親) がパネルに接続された直後に送信されます。
DetachFromPanelEvent	要素 (またはその親) がパネルから離される直前に送信されます。
originPanel: originPanel には、DetachFromPanelEvent 特有のデータが含まれています。(そのイベントのみ有効なプロパティ?)パネル変更時にビジュアル要素が切り離されるソースパネルが示されています。
destinationPanel: destinationPanel には、AttachFromPanelEvent 特有のデータが含まれています。データは、ビジュアル要素が現在接続しているパネルを示します。

## UQuery

概要: Query&lt;T&gt;.Build().First() の短縮形である便利なオーバーロードです。
e: セレクタが適用されるルート VisualElement。
name: 指定された場合、この名前を持つ要素を選択します。
className: 指定された場合、指定されたクラス（Typeと混同しないように）を持つ要素を選択します。
//型かnameか.class[]か
❰VisualElement¦T¦UQueryBuilder<❰VisualElement¦T❱>❱ ❰Q＠❰T❱¦Query＠❰T❱❱(this VisualElement e, string name = null, params string[] classes)＠❰where T : VisualElement❱;
❰VisualElement¦T¦UQueryBuilder<❰VisualElement¦T❱>❱ ❰Q＠❰T❱¦Query＠❰T❱❱(this VisualElement e, string name = null, ❰params string[] classes¦string className = null❱)＠❰where T : VisualElement❱;
ルートビジュアルエレメント上で実行される選択ルール群を構築するユーティリティオブジェクトです。
    (さらに絞り込むまたは、子をForEachで処理する用の中間Query?)
UQueryBuilder<T>
