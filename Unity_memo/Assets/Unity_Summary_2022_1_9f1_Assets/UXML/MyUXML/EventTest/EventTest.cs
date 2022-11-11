using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

class EventTest : EditorWindow
{
    [MenuItem("UI Toolkit/EventTest")]
    static void GetWindow()
    {
        EditorWindow wnd = GetWindow<EventTest>("EventTest"); //↓を書かなくてもいい
        // wnd.titleContent = new GUIContent("EventTest");
        Debug.Log("CreateGUI()の後に来る"); //恐らくGetWindowの初期化時にCreateGUIが呼ばれている
    }

    EventBase m_CacheEvt;
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
        Toggle toggle_3_0_1 = new("toggle_3_0_1"){name = "n_toggle_3_0_1"};
        vE_2_0.Add(toggle_3_0_1);
        VisualElement vE_2_1 = new(){name = "n_vE_2_1"};
        vE_1.Add(vE_2_1);
        ExtToggle extToggle_3_1 = new("extToggle_3_1"){name = "n_extToggle_3_1"};
        vE_2_1.Add(extToggle_3_1);

        Button CheckCacheEvt = new(() => 
        {
            //dispatchは現在伝搬中であるかチェックすし伝搬中ならtrueになる。(多分、propagationPhase.None <=> dispatch:False?)
            Debug.Log($"m_CacheEvt.dispatch: {m_CacheEvt.dispatch}");                   //=>False 
            Debug.Log($"m_CacheEvt.propagationPhase: {m_CacheEvt.propagationPhase}");   //=>None
        }
        ){text = "CheckCacheEvt"};
        vE_0.Add(CheckCacheEvt);

        Button Focus_toggle_3_0_0 = new(() =>
        {
            // toggle_3_0_0.focusable = false; //falseにするとFocusされなくなる
            // toggle_3_0_0.tabIndex = -1; //負の数に設定してもcanGrabFocusはtrue
            Debug.Log($"toggle_3_0_0.tabIndex: {toggle_3_0_0.tabIndex}");

            //Focusできる場合はtrue。focusableのgetとの違いは多分もともとのElementがFocus可能か?と言うのが論理積される?
            Debug.Log($"toggle_3_0_0.canGrabFocus: {toggle_3_0_0.canGrabFocus}"); 
            toggle_3_0_0.Focus(); //Focusする
            // toggle_3_0_0.Blur(); //これを実行するとフォーカスされないので多分うまくフォーカス外れてる
        }
        ){text = "Focus toggle_3_0_0"};
        vE_0.Add(Focus_toggle_3_0_0);

        Button tabIndex_toggle_3_0_1 = new(() =>
        {
            Debug.Log($"toggle_3_0_1.tabIndexデフォルト値: {toggle_3_0_1.tabIndex}");
            toggle_3_0_1.tabIndex = 1;//-1; //フォーカスの優先順位。負の数だとTabキーでフォーカスされない。tabIndexが同列の場合、多分フォーカスリングアルゴリズムに従う
        }
        ){text = "tabIndex toggle_3_0_1"};
        vE_0.Add(tabIndex_toggle_3_0_1);

        Button SendEvent = new(() =>
        {
            Debug.Log("SendEvent");
            // vE_0.SendEvent(m_CacheEvt); //一回目:無反応, 2回目以降:イベントは複数回ディスパッチすることはできません。
            // SynthesizeAndSendKeyDownEvent(vE_0.panel, KeyCode.A); //無反応
            vE_0.panel.visualTree.SendEvent(new KeyDownEvent(){target = vE_0}); //無反応
        }){text = "SendEvent"};
        vE_0.Add(SendEvent);

        VisualElement receiveKeyDownEvent = new();
        receiveKeyDownEvent.RegisterCallback<KeyDownEvent>((evt)=>
        {
            Debug.Log("receiveKeyDownEvent==========================");
            Debug.Log($"(evt.target as VisualElement).name: {(evt.target as VisualElement).name}");
            Debug.Log($"evt.keyCode: {evt.keyCode}");
        });
        vE_0.Add(receiveKeyDownEvent);

        rootVisualElement.Add(vE_0);

        ElementTreeLog(rootVisualElement.panel);

        vE_0.RegisterCallback<ChangeEvent<bool>, string>((evt, tUserArgsType)=>
            {
                Log(evt);
                Debug.Log(tUserArgsType);
                // Debug.Log("Call evt.PreventDefault()");evt.PreventDefault(); //ExecuteDefaultAction＠❰AtTarget❱が止まらない。何も変化がない

                //期待通り、evt.StopImmediatePropagation();を呼び出すと即時にevt.isImmediatePropagationStoppedをtrueにし、
                    //現在のCallback以降のCallbackを呼ばない(ExecuteDefaultAction＠❰AtTarget❱は呼ぶ)
                // Debug.Log("evt.StopImmediatePropagation()");evt.StopImmediatePropagation(); 
                // Debug.Log($"evt.isImmediatePropagationStopped: {evt.isImmediatePropagationStopped}");

                //期待通り、Callback_1でevt.isPropagationStoppedがtrueになりvE_0のイベントハンドラのCallBackを全て実行し止まる
                // Debug.Log("evt.StopPropagation()");evt.StopPropagation();
                // Debug.Log($"evt.isPropagationStopped(vE_0): {evt.isPropagationStopped}");
            }
            , "TUserArgsType"
            , TrickleDown.TrickleDown //TrickleDown,BubbleUp 期待通りの挙動
            );
        vE_0.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Debug.Log("==vE_0 Add Callback_1==");
                Debug.Log($"evt.isPropagationStopped: {evt.isPropagationStopped}");
            }
            , TrickleDown.TrickleDown
            );
        vE_0.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Debug.Log("==vE_0 Add Callback_2==");
            }
            , TrickleDown.TrickleDown
            );
        // vE_0.RegisterCallback<FocusEvent>((evt)=>
        //     {
        //         Debug.Log("FocusEvent=======================================");
        //         Debug.Log($"(evt.target as VisualElement).name: {(evt.target as VisualElement)?.name}");
        //         Debug.Log($"(evt.relatedTarget as VisualElement).name: {(evt.relatedTarget as VisualElement)?.name}"); //Dockareaしか出ない

        //         //evtのイベントの型がもともと❰TrickleDown¦BubbleUp❱で受信する場合はtrue
        //         // Debug.Log($"==evt.tricklesDown: {evt.tricklesDown}=="); //=>True
        //         // Debug.Log($"==evt.bubbles: {evt.bubbles}==");           //=>False?
        //     }
        //     , TrickleDown.TrickleDown
        //     );
        vE_1.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Log(evt);
                m_CacheEvt = evt;
                // Debug.Log($"==evt.dispatch: {evt.dispatch}==");//=>True
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
        toggle_3_0_1.RegisterCallback<ChangeEvent<bool>>((evt)=>
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
        vE_2_1.RegisterCallback<ClickEvent>((evt)=>
            {
                Debug.Log("vE_2_1/ClickEvent=======================================");
                Debug.Log("evt.PreventDefault()");
                evt.PreventDefault();   //evt.PreventDefault()を実行すると即時にevt.isDefaultPreventedがtrueになる。(そのイベントがキャンセル可能な場合)
                                            //(ExecuteDefaultActionAtTargetをキャンセルする場合、TrickleDownフェーズでevt.PreventDefault()を実行する必要がある)
                // Debug.Log($"evt.isDefaultPrevented: {evt.isDefaultPrevented}"); //=>True (evt.PreventDefault()実行時)
            }
            , TrickleDown.TrickleDown
            );
        extToggle_3_1.RegisterCallback<ChangeEvent<bool>>((evt)=>
            {
                Log(evt);
                // Debug.Log("Call evt.PreventDefault()");evt.PreventDefault();//直前でも無反応..
                Debug.Log($"evt.isDefaultPrevented: {evt.isDefaultPrevented}");//=>False (vE_0:evt.PreventDefault()実行時)
            }
            , TrickleDown.TrickleDown
            );
        extToggle_3_1.RegisterCallback<ClickEvent>((evt)=>
            {
                Debug.Log("extToggle_3_1/ClickEvent=======================================");
                Debug.Log($"evt.isDefaultPrevented: {evt.isDefaultPrevented}"); //=>True (evt.PreventDefault()実行時)
            }
            , TrickleDown.TrickleDown
            );

        {//メソッドテスト
            //SetEnabled
            // vE_2_1.SetEnabled(false); //falseにすると、(this)Element以下の**子孫**がNotEditableの様に**灰色になり操作不能**になる

            //parent
            // Debug.Log($"vE_2_1.parent.name: {vE_2_1.parent.name}");//=>vE_2_1.parent.name: n_vE_1 //期待通り

            //visible
            // vE_2_0.visible = false; //`(this)Element.style.visibility.value`が`Hidden`になる(子孫には影響受けないが**親がHiddenだと子孫も非表示**になる)
            // Debug.Log($"vE_2_0.style.visibility.value: {vE_2_0.style.visibility.value}"); //=>Hidden 
            // Debug.Log($"toggle_3_0_0.style.visibility.value: {toggle_3_0_0.style.visibility.value}"); //=>Visible

            //HasTrickleDownHandlers, HasBubbleUpHandlers
                //❰**TrickleDown**¦**BubbleUp**❱の**Callbackが登録**されているなら**true**(恐らく、何のイベントか,いくつ登録されるか、は関係ない)
            // Debug.Log($"vE_2_0.HasTrickleDownHandlers(): {vE_2_0.HasTrickleDownHandlers()}"); //=>True
            // Debug.Log($"vE_2_1.HasTrickleDownHandlers(): {vE_2_1.HasTrickleDownHandlers()}"); //=>False
            // Debug.Log($"vE_2_0.HasBubbleUpHandlers(): {vE_2_0.HasBubbleUpHandlers()}"); //=>False
            // Debug.Log($"vE_2_1.HasBubbleUpHandlers(): {vE_2_1.HasBubbleUpHandlers()}"); //=>True
        }

        static void Log(ChangeEvent<bool> evt)
        {
            VisualElement e_target = evt.target as VisualElement;
            VisualElement e_currentTarget = evt.currentTarget as VisualElement;
            Debug.Log("ChangeEvent<bool>====================");
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
    class ExtToggle : Toggle
    {
        readonly Color m_DefaultBackgroundColor;
        readonly StyleColor m_DefaultColor;
        public ExtToggle(string s) : base(s)
        {
            m_DefaultBackgroundColor = style.backgroundColor.value; //何故かStyleColor型だと後でm_Def~をセットしても色が変わらなかった
            m_DefaultColor = labelElement.style.color;
        }
        protected override void ExecuteDefaultAction(EventBase evt) //現在実行中のディスパッチ動作の終了の直前に呼ばれる (何のイベントでも受信する)
        {
            base.ExecuteDefaultAction(evt);

            if(evt is ClickEvent) Debug.Log("ExecuteDefaultAction on ClickEvent");

            // if(evt.eventTypeId != ChangeEvent<bool>.TypeId()) return; //全てのイベントで実行される為、何のイベントを受信したかチェックする
            if(!(evt is ChangeEvent<bool> ce)) return; //単純に型チェックでもイケる

            if(ce.newValue)
            {
                style.backgroundColor = Color.green;
            }
            else
            {
                style.backgroundColor = m_DefaultBackgroundColor;
            }
            Debug.Log($"==ExecuteDefaultAction from: {(evt.currentTarget as VisualElement).name}==");
        }
        protected override void ExecuteDefaultActionAtTarget(EventBase evt) //targetであり現在受信しているイベントの全てのRegisterCallback<~>(~)を呼んだ後に呼ばれる
                                                                                //(何のイベントでも受信する)
        {
            base.ExecuteDefaultActionAtTarget(evt);

            if(evt is ClickEvent) Debug.Log("ExecuteDefaultActionAtTarget on ClickEvent");

            if(!(evt is ChangeEvent<bool> ce)) return; //単純に型チェックでもイケる

            if(ce.newValue)
            {
                labelElement.style.color = Color.red;
            }
            else
            {
                labelElement.style.color = m_DefaultColor;
            }
            Debug.Log($"==ExecuteDefaultActionAtTarget from: {(evt.target as VisualElement).name}==");
        }
    }
    void SynthesizeAndSendKeyDownEvent(IPanel panel, KeyCode code,
        char character = '\0', EventModifiers modifiers = EventModifiers.None)
    {
        // Create a UnityEngine.Event to hold initialization data.
        var evt = new Event() {
            type = EventType.KeyDown,
            keyCode = code,
            character = character,
            modifiers = modifiers
        };

        using (KeyDownEvent keyDownEvent = KeyDownEvent.GetPooled(evt))
        {
            panel.visualTree.SendEvent(keyDownEvent);
        }
    }
}