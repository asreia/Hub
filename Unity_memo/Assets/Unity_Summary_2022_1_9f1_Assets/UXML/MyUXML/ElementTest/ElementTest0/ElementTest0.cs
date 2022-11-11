using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ElementTest0 : EditorWindow {

    [MenuItem("UI Toolkit/ElementTest0")]
    private static void ShowWindow()
    {
        var window = GetWindow<ElementTest0>();
        window.titleContent = new GUIContent("ElementTest0");
    }

    enum EnumFieldValue{龍,角,散};
    public Texture2D m_property;// = Texture2D.grayTexture; //Texture2D.grayTextureエラー:UnityException: get_grayTexture は ScriptableObject のコンストラクタ（またはインスタンスフィールドの**イニシャライザ**）から呼び出すことはできません。ScriptableObject 'ElementTest0'から呼び出されました。 
    void CreateGUI()
    {
        {//BaseField<TValueType>
            {//ObjectFieldは、UnityObjectのTypeを指定し、UnityObjectをウインドウでプロジェクトまたはシーンから参照できるようにする
                ObjectField objectField = new(label:"label");
                rootVisualElement.Add(objectField);

                objectField.objectType = typeof(Texture2D); //UnityObjectのTypeを指定
                // Debug.Log($"objectField.allowSceneObjects: {objectField.allowSceneObjects}");//=>true
                // objectField.allowSceneObjects = false; //true(デフォルト)にすると、⦿を押してUnityObjectを参照するとき[シーン]のUnityObjectも選べるようになる

                objectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>((evt)=>{
                    Debug.Log($"ChangeEvent<UnityEngine.Object>.newValue: {evt.newValue}");});
            }

            {//EnumFieldは、System.Enumを指定し、ドロップダウンメニューから選択できるようにする
                //System.Enumの初期値を与えるだけで型が設定される。(EnumFieldValue.龍.GetType()していると思われる)
                EnumField enumField = new(label:"label", defaultValue:EnumFieldValue.龍); 
                rootVisualElement.Add(enumField);

                enumField.RegisterCallback<ChangeEvent<EnumFieldValue/*System.Enum*/>>((evt)=>{
                    Debug.Log($"ChangeEvent<System.Enum>.newValue: {evt.newValue}, enumField.text: {enumField.text}");});//EnumField.textはnewValueをToString()しただけと思われる
            }

            {//MinMaxSliderは、Vector2を指定し、範囲付きスライダーで入力できるようにする
                MinMaxSlider minMaxSlider = new(label:"label", minValue:2000, maxValue:6000, minLimit:-100, maxLimit:9999.9f);
                rootVisualElement.Add(minMaxSlider);

                minMaxSlider.RegisterCallback<ChangeEvent<Vector2>>((evt)=>{//minMaxSlider.rangeは、highLimit - lowLimit
                    Debug.Log($"ChangeEvent<Vector2>.newValue: {evt.newValue}, minMaxSlider.value: {minMaxSlider.value},\nminMaxSlider.minValue: {minMaxSlider.minValue}, minMaxSlider.maxValue: {minMaxSlider.maxValue},\nminMaxSlider.lowLimit: {minMaxSlider.lowLimit}, minMaxSlider.highLimit: {minMaxSlider.highLimit},\nminMaxSlider.range: {minMaxSlider.range}");});
            }

            {//ColorFieldは、Colorのウインドウを表示しColorを設定できるようにする
                ColorField colorField = new(label:"label");
                rootVisualElement.Add(colorField);

                // Debug.Log($"colorField.showEyeDropper: {colorField.showEyeDropper}");//=>true
                // colorField.showEyeDropper = false; //スポイトの有無(デフォルト有り)
                // Debug.Log($"colorField.showAlpha: {colorField.showAlpha}");//=>true
                // colorField.showAlpha = false; //アルファの有無(デフォルト有り)
                // Debug.Log($"colorField.hdr: {colorField.hdr}");//=>false
                // colorField.hdr = true; //HDRかどうか(デフォルトfalse)

                colorField.RegisterCallback<ChangeEvent<Color>>((evt)=>{Debug.Log($"ChangeEvent<Color>.newValue: {evt.newValue}");});
            }

            {//RadioButtonGroupは、
                RadioButtonGroup radioButtonGroup = new(label:"label"
                , radioButtonChoices:new List<string>(){"ボ","卿"}
                );
                //水平方向右側に追加された(ウインドウを横に伸ばさないと隠れて見えない)
                radioButtonGroup.Add(new RadioButton("追"));radioButtonGroup.Add(new RadioButton("加"));
                rootVisualElement.Add(radioButtonGroup);

                // foreach(var e in radioButtonGroup.choices) Debug.Log($"{e}");//=>ボ\n卿 //choices:グループ内で利用可能な選択肢のリストです。

                radioButtonGroup.RegisterCallback<ChangeEvent<int>>((evt)=>{Debug.Log($"ChangeEvent<int>.newValue: {evt.newValue}");});
            }
        }
        {//TextValueField<TValueType>
            {//FloatFieldは、テキストでfloat値を入力するようにする
                FloatField floatField = new(label:"label", maxLength:4 /*デフォルト:-1*/);//maxLengthは文字数
                rootVisualElement.Add(floatField);
                
                floatField.RegisterCallback<ChangeEvent<float>>((evt)=>{        //formatStringが何故か常に"g7"
                    Debug.Log($"floatField.value: {floatField.value}, floatField.formatString: {floatField.formatString}");
                });
            }
        }
        {//TextInputBaseField<TValueType>
            {//TextFieldは、テキストで文字列を入力するようにする
                //maxLength:最大入力文字数, multiline:改行して複数行をかけるか
                //isPasswordField = true かつ maskChar:'*'にして文字を打つとTextFieldが"****"となり入力文字を隠す
                TextField textField = new(label:"label", maxLength:60, multiline:true, isPasswordField:false, maskChar:'*');
                rootVisualElement.Add(textField);

                // Debug.Log($"textField.cursorColor: {textField.cursorColor}");//getOnly//=>RGBA(0.706, 0.706, 0.706, 1.000)
                // Debug.Log($"textField.selectionColor: {textField.selectionColor}");//getOnly//=>RGBA(0.239, 0.502, 0.875, 0.650)
                {//select
                textField.selectAllOnMouseUp = true; //>初めてマウスを上げたときに要素のコンテンツを選択するかどうかを制御します。(上げた時?なにも変わらなかった)
                textField.selectAllOnFocus = false; //多分、フォーカスを受けた時に、全テキストを選択状態にするか?だと思うけどfalseでも常にその状態..
                textField.doubleClickSelectsWord = true; //テキストをダブルクリックしてマウスポインターの下の単語(複数行は選択しない)を選択するか
                textField.tripleClickSelectsLine = true;//テキストをトリプルクリックしてマウスポインターの下の全ての行を選択するか?だと思うがtrueにしてもできなかった
                textField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
                }
                textField.isDelayed = true; //一文字ずつ更新ではなくて、Enterキーまたはフォーカスを失うとvalueを更新するか
                textField.isReadOnly = false; //trueにすると、カーソルが現れず入力不能となる(たぶんvalueを操作して見るだけ?)
                {//保留
                // textField.SelectRange(cursorIndex:2, selectionIndex:4); //cursorIndex～selectionIndexまでを選択(これは試した)
                    // textField.cursorIndex //>これは、提示されたテキスト内のカーソル インデックスです。
                    // textField.selectIndex //>これは、提示されたテキストの選択インデックスです。
                // textField.cursorPosition //>これは、TextInputBaseField_1 内のカーソルの位置(Vector2)です。
                // textField.textEdition //>このフィールドの TextElement ITextEdition を取得します
                // textField.textSelection //>このフィールドの TextElement ITextSelection を取得します
                // textField.MeasureTextSize(～) //>font、font-size、word-wrap などの要素スタイル値に基づいて、テキスト文字列を**表示するために必要なサイズを計算(Vector2)**します。
                }
                rootVisualElement.Add(new Button(()=>
                {
                    textField.SelectAll(); //全てのテキストを選択
                }){text = "SelectAll()"});
                rootVisualElement.Add(new Button(()=>
                {
                    textField.SelectNone(); //全てのテキストを選択解除
                }){text = "SelectNone()"});
                rootVisualElement.Add(new Button(()=>
                {
                    textField.SelectRange(cursorIndex:2, selectionIndex:4); //cursorIndex～selectionIndexまでを選択
                }){text = "SelectRange(2,4)"});
                EnumField scrollerVisibilityEnum = new (ScrollerVisibility.Auto); 
                textField.SetVerticalScrollerVisibility((ScrollerVisibility)scrollerVisibilityEnum.value); //複数行書く時、スロールバーを表示するか
                scrollerVisibilityEnum.style.maxHeight = 100;                                               //←↑スクロールバーのバーが出ねぇ..(maxHeight制限したのにはみ出す)
                // rootVisualElement.style.maxHeight = 500;
                rootVisualElement.Add(scrollerVisibilityEnum);
                scrollerVisibilityEnum.RegisterCallback<ChangeEvent<System.Enum>>((evt)=>
                {
                    Debug.Log($"SetVerticalScrollerVisibility(evt.newValue): {textField.SetVerticalScrollerVisibility((ScrollerVisibility)evt.newValue)}");
                });

                textField.RegisterCallback<ChangeEvent<string>>((evt)=>{
                    Debug.Log($"textField.value: {textField.value}, textField.text: {textField.text}");
                });
            }
        }
        {//BaseBoolField
            {//Toggle
                Toggle toggle = new(label:"label");
                rootVisualElement.Add(toggle);
                
                toggle.text = ">BaseBoolFieldの後に表示されるオプションのテキスト";//トグルボタンの右側に表示された

                toggle.RegisterCallback<ChangeEvent<bool>>((evt)=>
                {
                    Debug.Log($"evt.newValue: {evt.newValue}");
                });
            }
        }
        {//BaseCompositeField<TValueType, TField, TFieldValue> //←これに設定できることは何もない(TField(FloatFieldとか)のインスタンスも公開されていなので編集不可)
            {//Vector3Field
                Vector3Field vector3Field = new(label:"label"); //labelを設定できるだけ、各軸のX,Y,Zの名前はとかは変えられない
                rootVisualElement.Add(vector3Field);

                vector3Field.RegisterCallback<ChangeEvent<Vector3>>((evt)=>
                {
                    Debug.Log($"evt.newValue: {evt.newValue}");
                });
            }
        }
        {//TextElement
            {//Label
                // Label label = new(text:"text");
                Label label = new(text:"text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_text_");
                rootVisualElement.Add(label);

                label.focusable = false; //↓falseでも選択できる..
                label.isSelectable = true; //要素の内容が選択可能であるかどうかを制御する。選択可能なTextElementは**フォーカス可能**であることが必要であることに注意。

                // Debug.Log($"label.enableRichText: {label.enableRichText}");//=>True
                // label.enableRichText = false;  //>false の場合、リッチテキストタグは解析されない。(よく分からん)

                label.tooltip = "tooltip";
                label.style.width = 100; //制限してもElidedしない
                //テキストがエリッドされている場合はtrueを、そうでない場合はfalseを返す。(Elidedとはtextが表示しきれないほど長い場合省略する事らしい)
                // Debug.Log($"label.isElided: {label.isElided}");//=>False 
                // Debug.Log($"label.displayTooltipWhenElided: {label.displayTooltipWhenElided}");//=>True
                //>true の場合、省略されたテキストの完全なバージョンがツールヒントに表示されます。また、以前にツールヒントが提供されていた場合は**上書き**されます。
                // label.displayTooltipWhenElided = false;

                // label.selection //ITextSelection//TextFieldを参照(↑にある)
            }
            {//Button //Callback呼ぶだけ。あと、MouseManipulator(まだ良く知らない)
                //このclickEventは、UI Elementのイベントシステムとは別で動くと思われる
                Button button = new(clickEvent:()=>{Debug.Log("clickEvent(多分clicked)");}){text = "text"};
                rootVisualElement.Add(button);

                //Clickable clickable { get; set; } //>このButtonに対応するクリック可能なMouseManipulatorです。
            }
            {//RepeatButtonは、長押ししている間、Callbackをdelay,intervalを元にRepeatする
                //delay,intervalはミリ秒単位。delayは>アクションが初めて実行されるまでの初期遅延時間。とあるが、押した瞬間に一回呼ばれその後delay秒間遅延する(謎)
                    //intervalは、Callbackを呼ぶ間隔時間。
                    //あとButtonとは違い、Labelと同じ外観で表示されて分かりづらい
                RepeatButton repeatButton = new(clickEvent:()=>{Debug.Log("clickEvent");},delay:2000, interval:1000){text = "repeatButton"};
                rootVisualElement.Add(repeatButton);
            }
            {//PopupWindowは、VisualElementに似ていて枠付きのコンテナ内にElementを追加でき、textを上に表示できる。だけ?(IStyle.borderなどで良いのでは?)
                UnityEngine.UIElements.PopupWindow popupWindow = new(){text = "PopupWindow_text"};
                UnityEngine.UIElements.PopupWindow popupWindow1 = new(){text = "PopupWindow1_text"};
                popupWindow.Add(new Label("popupWindow.Add(new Label(\"\"))"));
                popupWindow.Add(new Label("popupWindow.Add(new Label(\"\"))"));
                popupWindow.Add(popupWindow1);
                popupWindow.Add(new Label("popupWindow.Add(new Label(\"\"))"));
                popupWindow1.Add(new Label("popupWindow1.Add(new Label(\"\"))"));
                popupWindow1.Add(new Label("popupWindow1.Add(new Label(\"\"))"));
                rootVisualElement.Add(popupWindow);

                //contentContainer
                // popupWindow.contentContainer.Query<Label>().ForEach((label)=>{Debug.Log($"label.text: {label.text}");});//ForEachは子孫も取得する
            }
        }
        {//BindableElement
            {//Foldoutは、Elementの階層構造を作り、Elementを折りたたみ可能になる。しかしFoldoutのネストにインデントが付かない(謎)
                Foldout foldout_0 = new(){text = "foldout_0"};
                Foldout foldout_1 = new(){text = "foldout_1"};
                rootVisualElement.Add(new Button(()=>
                {
                    foldout_0.value = !foldout_0.value;
                    Debug.Log($"foldout_0.value: {foldout_0.value}");
                }){text = "foldout_0.value = !foldout_0.value"});
                rootVisualElement.Add(new Button(()=>
                {
                    foldout_1.value = !foldout_1.value;
                    Debug.Log($"foldout_1.value: {foldout_1.value}");
                }){text = "foldout_1.value = !foldout_1.value"});
                rootVisualElement.Add(foldout_0);

                foldout_0.Add(new Label("foldout.Add(new Label(\"\")"));
                foldout_0.Add(new Label("foldout.Add(new Label(\"\")"));
                foldout_0.Add(foldout_1);   //インデントが付かない
                foldout_1.Add(new Label("foldout_1.Add(new Label(\"\"))"));
                foldout_1.Add(new Label("foldout_1.Add(new Label(\"\"))"));
                foldout_0.Add(new Label("foldout.Add(new Label(\"\")"));
            }
        }
        {//VisualElement
            {//PropertyFieldは、SerializedPropertyを表現する。実態はバインディングシステムでBindPropertyを設定するだけで表現される。BindableElementの万能版
            //>コンストラクトに SerializedProperty を指定すると、bindingPath が設定されるだけです。その後、PropertyField で Bind() を呼び出す必要があります。
            
                // PropertyField propertyField = new(new SerializedObject(this).FindProperty("m_property"));
                // rootVisualElement.Add(propertyField);

                SerializedObject so0 = new(this);
                // PropertyField  propertyField0 = new(so0.FindProperty("m_property")); 
                //↕↕↕↕↕↕恐らく同等(bindingPathを設定しているだけ)↕↕↕↕↕↕↕↕↕↕
                PropertyField  propertyField0 = new(); propertyField0.bindingPath = so0.FindProperty("m_property").propertyPath;
                Debug.Log($"propertyField0.bindingPath: {propertyField0.bindingPath}");
                propertyField0.Bind(so0); //SerializedObjectをバインド
                rootVisualElement.Add(propertyField0);
                //↓↓↓↓↓↓むしろBindPropertyでbindingPath設定 & Bind() をすれば良い
                PropertyField propertyField1 = new();
                propertyField1.BindProperty(new SerializedObject(this).FindProperty("m_property"));
                rootVisualElement.Add(propertyField1);

                SerializedObject so1 = new SerializedObject(Texture2D.grayTexture);
                //label:を指定しないと"m_property"を使用しそして勝手に翻訳される
                PropertyField propertyField2 = new(so1.FindProperty("m_Name")/*so.GetIterator()*/ /*, label:"label"*/);
                propertyField2.Bind(so1);
                rootVisualElement.Add(propertyField2);

                ScrObj scrObj = ScriptableObject.CreateInstance<ScrObj>();
                SerializedObject so2 = new SerializedObject(scrObj);
                SerializedProperty propertyField3 = so2.FindProperty("fieldData");
                PropertyField pf = new(property:propertyField3, label:"label");
                rootVisualElement.Add(pf);
                // rootVisualElement.Bind(so1);
                pf.Bind(so2);

                {//InspectorElementは、SerializedObjectやUnityObjectを描画してしまう
                    rootVisualElement.Add(new InspectorElement(so1));
                    rootVisualElement.Add(new InspectorElement(so2));
                    rootVisualElement.Add(new InspectorElement(Texture2D.grayTexture));
                    //↕↕↕↕↕↕同じにならない(笑)
                    UnityEngine.Object unityObject = Texture2D.grayTexture;
                    SerializedObject serializedObject = new(unityObject);
                    VisualElement ve = new();
                    // int count = 0;
                    //Invalid iteration - (最初の要素でNext(true)を呼ばないと、最初の要素にたどり着けません）。(よく分からん)
                    // foreach(SerializedProperty sp in serializedObject.GetIterator()) //↑エラーが出るのでコメントアウト
                    // {
                    //     ve.Add(new PropertyField(sp));
                    //     if(!(count < 5)) break;
                    //     count++;
                    // } 
                    ve.Bind(serializedObject);
                    rootVisualElement.Add(ve);
                }
            }
        }
    }
}
