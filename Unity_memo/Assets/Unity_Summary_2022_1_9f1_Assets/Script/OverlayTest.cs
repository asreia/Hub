using UnityEditor; 
using UnityEditor.Overlays;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Overlayのサンプレ")] 

public class MyOverlay : Overlay 
{
    public override VisualElement CreatePanelContent()
    { 

    var root = new VisualElement();

    root.style.height = 100;

    return root;
    }
}