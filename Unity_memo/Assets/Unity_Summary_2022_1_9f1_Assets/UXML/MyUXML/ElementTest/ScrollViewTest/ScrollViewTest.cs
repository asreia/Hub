using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ScrollViewTest : EditorWindow
{
    [MenuItem("UI Toolkit/ScrollViewTest")]
    static void GetWindow()
    {
        EditorWindow wnd = GetWindow<ScrollViewTest>(typeof(ScrollViewTest));
    }
    public void CreateGUI()
    {
        // Debug.Log($"Toggle.ussClassName: {Toggle.ussClassName}");//=>unity-toggle
        // Debug.Log($"BaseField<Toggle>.ussClassName: {BaseField<Toggle>.ussClassName}");//=>unity-base-field

        ScrollView scrollView = new(ScrollViewMode.VerticalAndHorizontal);
        scrollView.viewDataKey = "ScrollViewTest_viewDataKey"; //スクロールの位置を保存する
        
        int labelCount = 0;
        rootVisualElement.Add(new Button(()=>
        {
            var newLabel = new Label($"====================label{labelCount}====================");
            scrollView.Add(newLabel);
            labelCount++;
            Debug.Log($"newLabel.localBound.position: {newLabel.localBound}");//何故かpositionが0。Add後すぐに設定されない
            // scrollView.scrollOffset = newLabel.localBound.position;
            Debug.Log($"scrollView.contentContainer.Query<Label>().Last() == newLabel: {scrollView.contentContainer.Query<Label>().Last() == newLabel}");//=>true
            scrollView.scrollOffset = scrollView.contentContainer[scrollView.contentContainer.childCount - 2].localBound.position;
            // scrollView.scrollOffset = new Vector2(scrollView.scrollOffset.x, float.MaxValue);
        }){text = "Add Label"});

        rootVisualElement.Add(new Button(()=>
        {
            // scrollView.Query<Label>().Last().RemoveFromHierarchy();
            scrollView.contentContainer.Query<Label>().Last().RemoveFromHierarchy();
        }){text = "Remove Label"});

        Vector2 offset = Vector2.zero;
        rootVisualElement.Add(new Button(()=>
        {
            offset = scrollView.scrollOffset;
            Debug.Log($"scrollView.scrollOffset: {offset}");
        }){text = "Log scrollOffset"});
        rootVisualElement.Add(scrollView);

        rootVisualElement.Add(new Button(()=>
        {
            Debug.Log($"scrollView.scrollOffset: {offset}");
            scrollView.scrollOffset = offset;
        }){text = "Set scrollOffset"});

        rootVisualElement.Add(scrollView);
        
        scrollView.contentContainer.RegisterCallback<ClickEvent>((evt)=>
        {
            VisualElement ve = evt.target as VisualElement;
            Debug.Log($"ve.localBound: {ve.localBound}, ve.worldBound: {ve.worldBound}, ve.contentRect: {ve.contentRect}");
            offset = ve.localBound.position;
            // ITransform transform = (evt.target as VisualElement).transform;
            // Debug.Log($"transform.position:{transform.position}");//=>(0.00, 0.00, 0.00) //常に0、ScrollView内のLabelの位置じゃない
            // Vector2 pos = transform.position;
            // pos.x += 100f;
            // transform.position = pos;
        });

        for(int i = 0; i < 100; i++)
        {
            scrollView.Add(new Label("label" + labelCount));
            labelCount++;
        }

        {//test
            Debug.Log($"scrollView.mode: {scrollView.mode}");//=>VerticalAndHorizontal
                scrollView.mode = ScrollViewMode.VerticalAndHorizontal;
            Debug.Log($"scrollView.touchScrollBehavior: {scrollView.touchScrollBehavior}");//=>Clamped
                scrollView.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped; //Unrestrictedにするとスクロールバーが末端に到達してもスクロールする
            
            //content
            Debug.Log($"scrollView.contentContainer.name: {scrollView.contentContainer.name}");//=>unity-content-container //ScrollViewのAdd先
                //恐らくscrollView.Add(new Label("label"))と同じ
                scrollView.contentContainer.Add(new Label("label_cc"));
            Debug.Log($"scrollView.contentViewport.name: {scrollView.contentViewport.name}");//=>unity-content-viewport
                //unity-content-containerの親のVisualElementにAddする
                scrollView.contentViewport.Add(new Label("scrollView.contentViewport:unity-content-viewport"));

            //PageSize(速度)
            Debug.Log($"scrollView.verticalPageSize: {scrollView.verticalPageSize}");
                // scrollView.verticalPageSize = 100f;
            Debug.Log($"scrollView.horizontalPageSize: {scrollView.horizontalPageSize}");
                scrollView.horizontalPageSize = 10f;

            //ScrollerVisibility
            Debug.Log($"scrollView.verticalScrollerVisibility: {scrollView.verticalScrollerVisibility}");
                scrollView.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            Debug.Log($"scrollView.horizontalScrollerVisibility: {scrollView.horizontalScrollerVisibility}");
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

        }
    }
}
