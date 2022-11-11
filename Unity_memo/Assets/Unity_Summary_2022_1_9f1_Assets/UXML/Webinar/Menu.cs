using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviour
{
public UIDocument Document;
    void Awake()
    {
        VisualElement root = Document.rootVisualElement;
        SliderInt volumeSlider = root.Q<SliderInt>("VolumeSlider");
        volumeSlider.RegisterValueChangedCallback(evt =>
        {
            Debug.Log(evt.newValue);
        });
        Button optionsButton = root.Q<Button>("Options");
        VisualElement optionsContainer = root.Q<VisualElement>("OptionsContainer");
        optionsContainer.AddToClassList("hide");
        optionsButton.clicked += () => optionsContainer.ToggleInClassList("hide");
    }
}
