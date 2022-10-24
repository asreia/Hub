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

- `void SetEnabled(bool value);`                                  ==試す=ok=
  - VisualElement の**有効状態を変更**します。無効化されたVisualElementはほとんどの**イベントを受信しません**。
    - falseにすると、(this)Element以下の**子孫**がNotEditableの様に**灰色になり操作不能**になる

- `VisualTreeAsset visualTreeAssetSource { get; }`                                  ==試す==
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
    - `VisualElement parent { get; }`                                  ==試す=ok=
      - 階層構造における**この(this)Element**の物理的な**親**。

  - **子の操作**
    - **取得**
      - `VisualElement this[int key] { get; }`
        - (恐らくthisがコンテナでその**子Elementのインデクサ**)

      - `IEnumerable<VisualElement> Children();`
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

- `VisualElementStyleSheetSet styleSheets { get; }`                                  ==試す==
  - この(this)Elementに付属する**StyleSheetsを操作**する**VisualElementStyleSheetSetを返します**。
    - (VisualElement(owner(ルート?))へのStyleSheetの追加削除操作ができるみたい。多分**StyleSheetはコードで生成不可**、USSファイルからのみロードして生成可能)

- **メモ**
  - IStyleの1要素毎の**セレクターの適用条件**(たぶん、100*2 + 10*0 + 1*4 + 2(階層) = 206)
    - セレクターの単一要素の優先度 
      - **❰1❱セット(10000), ❰2❱名前(#name)(1000), ❰3❱クラス(.class)(100), ❰3❱状態(:state)(100), ❰4❱型(Type)(10), ❰5❱デフォルト(0), ❰5❱ユニバーサルセレクタ(*)(0)**
      - **子がデフォルト**の場合その**親**のデフォルト以外**の設定**は**子をオーバーライド**する
      - セレクターの複数要素の優先度 (And(❰word❱❰word❱), 子(❰word❱ > ❰word❱), 子孫(❰word❱ ❰word❱), or(❰word❱,❰word❱), ユニバーサルセレクタ(*))
        - orは、`スタイルセレクタには、❰*_-.#>,❱文字、数字のみ使用できます。`と出て`,`は使えるはずなのに使えないので`or`をすることが**できない**
    - Element階層間でのセレクター適用優先度(子:セレクタ適用,親:直接適用 とか)

    - Element階層間でのUSSロード(override,優先順序)//まだ

##### **.class操作**(.classは単なるタグ。USSはセレクタで類別しているのであって、.classはセレクタの要素の一種。.classはUSSの適用以外にQueryの検索にも使う)

- **取得**
  - `IEnumerable<string> GetClasses();`                                  ==試す==
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

- `bool visible { get; set; }`                                  ==試す=ok=
  - この(this)Elementをレンダリングするか否かを示す。
    - `(this)Element.style.visibility.value`が`Hidden`になる(子孫には影響受けないが**親がHiddenだと子孫も非表示**になる)

- `void MarkDirtyRepaint();`
  - 次のフレームでVisualElementの再描画をトリガーします。

#### CallbackEventHandler

- `bool HasBubbleUpHandlers()`                                  ==試す=ok=
  - この(this)Elementにイベント伝搬 BubbleUp フェーズのイベントハンドラ がアタッチされている場合、true を返す。
    - **BubbleUp**の**Callbackが登録**されているなら**true**(恐らく、何のイベントか,いくつ登録されるか、は関係ない)

- `bool HasTrickleDownHandlers()`
  - (this)ElementがTrickleDownフェーズのイベントハンドラを持つ場合、Trueを返します。
    - **TrickleDown**の**Callbackが登録**されているなら**true**(恐らく、何のイベントか,いくつ登録されるか、は関係ない)

- `void RegisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()`
  - この(this)Elementに**TEventType**のイベントを**受信**した時に実行する**callbackを登録**する。**useTrickleDown**でBubbleUp(NoTrickleDown)かTrickleDownか選べる

- `void RegisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TUserArgsType userArgs, TrickleDown useTrickleDown = TrickleDown NoTrickleDown) where TEventType : EventBase<TEventType>, new()`
  - 通常のイベント受信時のコールバック**登録** + ユーザー定義引数(**TUserArgsType**)付き
                                  ==試す=ok=
- `UnregisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()`
  - この(this)Elementに**TEventType**のイベントを**受信**した時に実行する**callbackを登録解除**する。**useTrickleDown**でBubbleUp(NoTrickleDown)かTrickleDownか選べる

- `void UnregisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()`
  - 通常のイベント受信時のコールバック**登録解除** + ユーザー定義引数(**TUserArgsType**)付き

- `protected virtual void ExecuteDefaultAction(EventBase evt)`                                  ==試す=ok=
  - >イベントターゲットに登録されたコールバックが実行された後にロジックを実行します。ただし、イベントがデフォルトの動作を防止するようにマークされている場合はこの限りではありません。EventBase{T}.PreventDefault.
    - (**イベントディスパッチ動作の終了直前**(最後のBubbleUpの直後)で実行される(オーバーライドする必要があり、evtを**evt.eventTypeId**で調べて今なんのイベントか知る必要がある))
    - 追記:現在実行中の**ディスパッチ動作の終了の直前**に呼ばれる (何のイベントでも受信する)(↑`if(!(evt is ChangeEvent<bool> ce)) return;`でもイケる)
      - ↑コントロールしか試していない

- `protected virtual void ExecuteDefaultActionAtTarget(EventBase evt)`                                  ==試す=ok=
  - >イベントターゲットに登録されたコールバックが実行された後にロジックを実行します。ただし、そのイベントがデフォルトの動作を防止するようにマークされている場合を除きます。EventBase{T}.PreventDefault.
    - (**イベントディスパッチ動作のtargetの直後**で実行される(オーバーライドする必要があり、evtを**evt.eventTypeId**で調べて今なんのイベントか知る必要がある))
    - 追記:**ディスパッチ動作のtarget段階**であり現在受信しているイベントの**全てのRegisterCallback<~>(~)を呼んだ後**に呼ばれる(何のイベントでも受信する)(↑`if(!(evt is ChangeEvent<bool> ce)) return;`でもイケる)
      - ↑コントロールしか試していない

- `abstract void SendEvent(EventBase e)`                                  ==試す=ok=
  - イベントハンドラ(**IEventHandler?**)へ**イベントを送信**する。
  - 以下の三種類の方法で試したが**無反応**だった
    - `vE_0.SendEvent(m_CacheEvt);`
    - [SynthesizeAndSendKeyDownEvent(vE_0.panel, KeyCode.A);](https://docs.unity3d.com/ja/2022.2/Manual/UIE-Events-Synthesizing.html)
    - `vE_0.panel.visualTree.SendEvent(new KeyDownEvent(){target = vE_0});`

#### Focusable

- `bool focusable { get; set; }`                                  ==試す=ok=
  - (this)Elementが**Focus可能**であればtrue
  - **true**に設定すると**Focusでき**、**false**に設定すると**Focus不能**になる

- `virtual bool canGrabFocus { get; }`                                  ==試す=ok=
  - >(this)Elementに**Focusを当てることができる場合は true** を返す。
  - **Focusできる**場合は**true**。focusableのgetとの違いは多分**もともとのElement**が**Focus可能か?**と言うのが論理積される?

- `int tabIndex { get; set; }`                                  ==試す=ok=
  - >FocusリングのFocusタビリティをソートするために使用される整数。0以上でなければならない。
    - Focusの**優先順位(0が高い)**。**負の数**だとTabキーで**Focusされない**。tabIndexが**同列の場合**、多分**Focusリングアルゴリズムに従う**(デフォルトは0)
  - [Focusリング](..\画像\Focusリング.png)

- `virtual void Blur();`                                  ==試す=ok=
  - (this)Elementに**Focusを外す**ように指示する。

- `virtual void Focus();`                                  ==試す=ok=
  - この(this)Elementに**Focusを当てる**ようにする。

## SerializedObjectデータバインディング(BindableElement(IBindable) と BindingExtensions)

### 概要

- `INotifyValueChanged<TValueType>.value`と、`SerializedProperty`をリンクし**同期**させます

### メンバ

- **BindableElement**
  - `IBinding binding { get; set; }`
    - >更新されるバインディング・オブジェクト。(とりあえず翻訳)
    - >//bindingはBindしたSerializedObjectがある場所だと思ったけどnullだった
      - 追記(試してない): `IBinding`(`IBindable`と見間違えた)は、`SerializedProperty` => `value`への`＠❰Pre❱Update`関数があり実行すると`value`を**更新**すると思われる
        `Release()`は`Unbind()`の様なものと思われる
  - `string bindingPath { get; set; }`
    - >バインドする対象プロパティのパス
    - `bindingPath`が設定されているElementよりも**祖先**で実行し、引数に取る`SerializedObject`と、その中の`SerializedProperty`の**名前**が設定されている**子孫**の`bindingPath`を
      対応させ、`bindingPath`が設定されているElementの`value`と、その`SerializedProperty`で**バインディング**し同期します
- **BindingExtensions**
  - `void Bind(SerializedObject obj);`
    - `bindingPath`が設定されているElementよりも**祖先**で実行し、引数に取る`SerializedObject`と、その中の`SerializedProperty`の**名前**が設定されている**子孫**の`bindingPath`を
      対応させ、`bindingPath`が設定されているElementの`value`と、その`SerializedProperty`で**バインディング**し同期します
    - この`Bind(~)`関数は`bindingPath`が**設定し終わった後**に実行し**バインドする**と公式マニュアルに書いてあったが、試したところ**設定前でも機能した**。が、一応マニュアルに従った方が良い
    - `CustomEditor`,`PropertyDrawer`の場合は`return`する**Element**に自動的で`Bind(~)`が呼び出されるようなので**呼ぶ必要はない**(多分、逆に呼ぶと階層間で**二重にバインド**し**挙動が怪しく**なるかも?)
      - `Unbind()`は**閉じた時に呼ばれる**
  - `void BindProperty(SerializedProperty property);`
    - `SerializedProperty`を直接設定することで、このElementの`value`がその`SerializedProperty`と**バインディング**し同期します
  - `void TrackPropertyValue(SerializedProperty property, Action<SerializedProperty> callback = null);`
    - 引数に渡された`SerializedProperty`を監視し、変更があった時、第二引数の`callback`を呼び出します(`SerializedProperty`と`callback`の**バインディング**と言える?)
    - >//追跡(Track)先が、追跡先の初期値に変化するとCallbackを呼ばない(謎) //TrackPropertyValue,Unbindを繰り返したら、直った(謎)
      >//追跡(Track)先が、追跡先の初期値では無い状態で、so_bindData_0の任意のPropertyが変化するとCallbackを実行してしまう(仕様?)
  - `void TrackSerializedObjectValue(SerializedObject obj, Action<SerializedObject> callback = null);`
    - 引数に渡された`SerializedObject`を監視し、(任意の`SerializedProperty`の)変更があった時、第二引数の`callback`を呼び出します(`SerializedObject`と`callback`の**バインディング**と言える?)
  - `void Unbind();`
    - `Unbind()`は↑の4つの**バインディング状態**を**解除**します(つまり、↑の**呼び出し前の状態**と同じになる)

## BaseField\<TValueType>

### 概要

- `BaseField\<TValueType>`は、UIに**表示する時**、左側に`Label`、右側に`INotifyValueChanged<TValueType>`を表現するUIを表示する
- クラスの定義: `abstract class BaseField<TValueType> : BindableElement, INotifyValueChanged<TValueType>, IMixedValueSupport`
- `abstract`なので継承して実装される必要がある
- クラスの型引数に`TValueType`を取り`INotifyValueChanged<TValueType>`を実装している
  - `INotifyValueChanged<TValueType>`は`ChangeEvent<T>`を**発生**させる
- メンバに`Label labelElement { get; }`を持つ

### メンバ

- **Label**
  - `Label labelElement { get; }`
    - UIの左側に表示される`Label`

  - `string label { get; set; }`
    - 恐らく`labelElement.text`と同じ

- **INotifyValueChanged\<TValueType>**
  - `virtual TValueType value { get; set; }`
    - setすると`ChangeEvent<TValueType>`が**発生する**

  - `virtual void SetValueWithoutNotify(TValueType newValue)`
    - `ChangeEvent<~>`を**発行しない**でvalueを更新する

## イベント(evt)

### 概要

- UI Toolkitのイベントシステムは、**GUI操作,UIの内部状態の変化** などを**検知**し、
  UXMLの**root**からEventBase.**target**へ**イベント(evt)** を**必要とするノード**へ**ディスパッチ(送信)** して行きます
  - **必要とするノード**とは、UXMLのノードに**登録**されている`RegisterCallback<TEventType>`の`TEventType`が`typeof(TEventType) == evt.GetType`であり
    かつ [**enum TrickleDown**が合っている または **target**] の時、そのノード(Element)は**イベント(evt)を受信**します。(Callbackに**evtを引数に取る**)
  - Callbackを**登録する条件**は、**どのElementに登録するか**, **どのイベントを受信するか**, **どのタイミングで受信するか(enum TrickleDown)**、によって決まります。
- イベント(evt)の継承構造は、**EventBase** -> **EventBase\<T>** -> ..各イベント(evt)のクラス列.. となっている
- >UI Toolkit は、オペレーティングシステムやスクリプトからのイベントをリッスンし、EventDispatcher でこれらのイベントをElementにディスパッチします。
- >UI Toolkit のイベントは、HTML イベントに似ています。

### メソッド

#### EventBase

- **target**
  - `IEventHandler target { get; set; }`
    - このイベントを受信したターゲットのビジュアル要素です。currentTargetとは異なり、このターゲットはイベントが伝搬経路上の他の要素に送信される際に変更されません。
      - **目標のIEventHandler**(各Elementにキャスト可能)

  - `virtual IEventHandler currentTarget { get; internal set; }`
    - >イベントの現在のターゲットです。イベントハンドラが現在実行されている伝搬経路のVisualElementです。
      - **現在のIEventHandler**(各Elementにキャスト可能)

- `Stop＠❰Immediate❱Propagation()`と`PreventDefault()` と**その確認**
  - **Propagation**
    - `void StopPropagation()`                                  ==試す=ok=
      - >このイベントの伝搬を停止します。伝搬経路上の他の要素にイベントは送信されません。このメソッドは、他のイベントハンドラが現在のターゲット上で実行されるのを**防ぐことはありません**。
        - 現在ディスパッチしている**イベントの伝播経路のIEventHandler**で**そのIEventHandlerの全ての登録されたCallback(イベントの種類とタイミングは同じ)**を**全て実行し**、それ以降の
          **登録されたCallback**を実行しない(`ExecuteDefaultAction＠❰AtTarget❱`は呼ぶ)
        - `evt.StopPropagation()`を呼び出すと即時に`evt.isPropagationStopped`がtrueになる

    - `bool isPropagationStopped { get; }`                                  ==試す=ok=
      - >このイベントに対して `StopPropagation()`が呼び出されたかどうか。
      - `evt.StopPropagation()`を呼び出すと即時に`evt.isPropagationStopped`が**true**になる

  - **ImmediatePropagation**
    - `void StopImmediatePropagation()`                                  ==試す=ok=
      - >イベントの伝搬を即座に停止します。イベントは伝搬経路上の他の要素には送られません。このメソッドは、他のイベントハンドラが現在のターゲット上で実行されるのを**防ぎます**。
        - 現在ディスパッチしている**イベントの伝播経路のIEventHandler**で**現在実行している登録されたCallback**を**実行し**、それ以降の
          **登録されたCallback**を実行しない(`ExecuteDefaultAction＠❰AtTarget❱`は呼ぶ)

    - `bool isImmediatePropagationStopped { get; }`                                  ==試す=ok=
      - >このイベントに対して `StopImmediatePropagation()`が呼び出されたか否かを示す。
      - `evt.StopImmediatePropagation()`を呼び出すと即時に`evt.isImmediatePropagationStopped`を**true**にする

  - **DefaultPrevented**
    - `void PreventDefault()`                                  ==試す=ok=
      - >このイベントに対して、デフォルトのアクションが実行されないようにするかどうかを示す。
      - `evt.PreventDefault()`を実行すると即時に`evt.isDefaultPrevented`が`true`になり、`ExecuteDefaultAction＠❰AtTarget❱`を呼ばなくなる
        - (そのイベントが**キャンセル可能な場合**)(`ExecuteDefaultActionAtTarget`をキャンセルする場合、**TrickleDownフェーズ**で`evt.PreventDefault()`を実行する必要がある)

    - `bool isDefaultPrevented { get; }`                                  ==試す=ok=
      - このイベントに対してデフォルトアクションを実行しない場合はtrueを返す。
      - `evt.PreventDefault()`を実行すると即時に`evt.isDefaultPrevented`が`true`になり、`ExecuteDefaultAction＠❰AtTarget❱`を呼ばなくなる
        - (そのイベントが**キャンセル可能な場合**)(`ExecuteDefaultActionAtTarget`をキャンセルする場合、**TrickleDownフェーズ**で`evt.PreventDefault()`を実行する必要がある)

- 現在の**evtの状態**
  - `PropagationPhase propagationPhase { get; }`
    - 現在のプロパゲーションフェーズです。(enum)
      - (`PropagationPhase`(実行順): `None`, `TrickleDown`, `AtTarget`, `DefaultActionAtTarget`, `BubbleUp`, `DefaultAction`)
        - (多分、`propagationPhase.None` <=> `dispatch:False`?)

  - `bool dispatch { get; }`                                  ==試す=ok=
    - >イベントがElementにディスパッチされているかどうかを示します。
      >イベントは、ディスパッチされている間は再ディスパッチすることができません。
      >イベントを再帰的にディスパッチする必要がある場合は、イベントのコピーを使用することを推奨します。
      - **現在伝搬中であるか**チェックし**伝搬中ならtrue**になる。(多分、`propagationPhase.None` <=> `dispatch:False`?)

- そのフェーズで**受信するか?**[リンク先の表で、"BlurEvent"は"バブルアップ (上昇) 伝播"では受信しない為"bubbles"は"false"になる](https://docs.unity3d.com/ja/2022.2/Manual/UIE-Focus-Events.html)
  - `bool tricklesDown { get; protected set; }`                                  ==試す=ok=
    - >TrickleDown フェーズにおいて、このイベントがイベント伝搬路に送信されるかどうかを返す。
      - evtの**イベントの型**がもともと**TrickleDown**で**受信**する場合は**true**

  - `bool bubbles { get; protected set; }`                                  ==試す=ok=
    - BubbleUp フェーズにおいて、このイベントがイベント伝搬路に送信されるかどうかを返す。
      - evtの**イベントの型**がもともと**BubbleUp**で**受信**する場合は**true**

- `bool pooled { get; set; }`
  - >イベントがイベントのプールから割り当てられたかどうか。

- `long timestamp { get; }`
  - イベントが作成された時刻。

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
  - 試したが、コントロールの**奥のElement**をFocusしてイベントを呼んでいるせい?か`name`がうまく出なかった

- `FocusInEvent`
  - ElementがFocusを**得る前**に送信されます
  - `target`: Focusを得るElement
  - `relatedTarget`: Focusを失うElement
- `FocusEvent`                                  ==試す=ok=
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

### イベント

- `ChangeEvent<T>`
  - `target`: 状態(value)の変更が発生するElement
  - `T previousValue`: 変更される前の値
  - `T newValue`: 変更された後の値

## [Clickイベント](https://docs.unity3d.com/ja/2022.2/Manual/UIE-Click-Events.html)

### 概要

- **ボタン以外の視覚的Element**の**クリック検出**をするために使用することができます。
  - 例えば、`Toggle`コントロールの実装では、`ClickEvent`を使用して、チェックマークを表示または非表示にし、コントロールの**値(value)を変更**します。
- `ClickEvent`の基底クラスは、`PointerEventBase`です。

### イベント

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

## [IPanelイベント](https://docs.unity3d.com/ja/2022.2/Manual/UIE-Panel-Events.html)

### IPanelとは

- `IPanel`は、UXML(**Element**)の木構造(**ツリー**)の**ルートに存在**していて、`IPanel.visualTree`は**Elementのツリー**の**ルートのElement**を参照している(セットされていれば)
- Elementのツリーの**各ノード**の**element.panel**は、ルートに存在している**同じIPanelを参照**している
- **あるelement**のツリーに**別のelement**のツリーを `あるelement.Add(別のelement)`した場合、
  **別のelement.panel**は、**あるelement.panel**に更新される(**ルートのIPanelが更新**される)
- `IPanel`には`EventDispatcher`などUIとして機能させる機能がある
- >パネルは、UI 階層の可視インスタンスを表します。パネルは、ビジュアルツリーの階層の中で、要素の動作イベントのディスパッチを処理します。
  >階層のルートのビジュアル要素への参照を維持します。ランタイム UIでは、UGUI の Canvas に相当します。
- >VisualElement :: IPanel panel { get; } 概要: この VisualElement が貼り付けられているパネル。

### 概要

>IPanelイベントは、**IPanelとの関係が変化**するときにElementで**発生**します。
>IPanelイベントは、IPanelの変更(Add,Removeなど)の発生時に**直接影響を受けるElement**と**その子孫**にのみ送信されます。(**親Element**はイベントを**受信しない**)

### イベント

- **イベント**
  - `AttachToPanelEvent`
    - >Element(またはその親) がパネルに接続(`Add`)された直後に送信されます。
  - `DetachFromPanelEvent`
    - >Element(またはその親) がパネルから離される(`Remove`)直前に送信されます。
- **プロパティ**
- `originPanel`
  - >`DetachFromPanelEvent`特有のデータが含まれています。パネル変更時にElementが切り離される(`Remove`)ソースパネルが示されています。
    - `DetachFromPanelEvent`のみ有効なプロパティ (`AttachToPanelEvent`で参照したら`.visualTree`が`null`だった)
- `destinationPanel`
  - >`AttachToPanelEvent`特有のデータが含まれています。データは、Elementが現在接続(`Add`)しているパネルを示します。
    - `AttachToPanelEvent`のみ有効なプロパティ (`DetachFromPanelEvent`で参照したら`.visualTree`が`null`だった)

## UQuery

- 戻り値: `❰VisualElement¦T¦UQueryBuilder<❰VisualElement¦T❱>❱`
  本体: `❰Q＠❰<T>❱¦Query＠❰<T>❱❱` 引数:`(this VisualElement e, string name = null, params string[] classes)`
  型制約: `＠❰where T : VisualElement❱`
  - UQueryで検索する**条件**
    - Elementの**型**:`<T>`
    - Elementの**name**:`name`
    - Elementに追加している **.Class**:`classes`
  - 恐らく `element.Q<T>(~)` <=> `element.Query<T>(~)＠❰.Build()❱.First()`
    - >`Q<T>`は`Query<T>.Build().First()` の短縮形である便利なオーバーロードです。
  - `UQueryBuilder<T>`
    - >ルートビジュアルエレメント上で実行される**選択ルール群を構築**するユーティリティオブジェクトです。
      - (さらに絞り込むまたは、子をForEachで処理する用の中間Query?)
