using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

public static class ElementTreeLogClass
{    public static void ElementTreeLog(IPanel p)
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
    public static VisualElement Addd(this VisualElement ve, params VisualElement[] ves)
    {
        foreach(var eve in ves) ve.Add(eve);
        return ve;
    }
}

class Log
{
    ScrollView box_Scroll;
    VisualElement box;
    Label[] Lines;
    int logIndex = 0;
    int LineCount;

    // static readonly StyleColor defaultStyleColor = new StyleColor(StyleKeyword.Null);//>'_styleColor' の既定のパラメー
    public Log(int logLineCount = 50)
    {
        box_Scroll = new ScrollView(scrollViewMode:ScrollViewMode.VerticalAndHorizontal){};
        box = new VisualElement();
        box.style.flexDirection = FlexDirection.ColumnReverse;
        box_Scroll.Add(box);    //box_Scroll.hierarchy.Clear();してその後Add(～)しても何故か何も表示されなくなるので、VisualElementを噛ませると解消される
        this.LineCount = logLineCount; Lines = new Label[logLineCount];
        for(int i = 0; i < logLineCount; i++) Lines[i] = new Label(){isSelectable = true};
    }
    public VisualElement LogElement() => box_Scroll; 

    public void Write(string _message){Write(_message, new StyleColor(StyleKeyword.Null));}

    //>'_styleColor' の既定のパラメーター値は、コンパイル時の定数である必要があります。(メモリのプログラム領域(const)に書き込まれる?) 
    public void Write(string _message, StyleColor _styleColor /*= defaultStyleColor*/)
    {
        Lines[logIndex].text = _message;
        Lines[logIndex].style.color = _styleColor;
        box.hierarchy.Clear();
        for(int i = 0; i < LineCount; i++)
        {
            int index = ((logIndex + i) % LineCount); 
            box.Add(Lines[index]);
        }
        logIndex--;
        if(logIndex <= -1) logIndex = LineCount-1;

        DelaySetScrollOffset();
        async void DelaySetScrollOffset()
        {
            await Task.Delay(10);
            box_Scroll.scrollOffset = new Vector2(box_Scroll.scrollOffset.x, float.MaxValue);
        }
    }
}
