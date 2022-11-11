using System.Collections.Generic;
using UnityEngine.UIElements;

public class CustomElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<CustomElement, UxmlTraits>
    {
    }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        private UxmlStringAttributeDescription _stringAttribute = new UxmlStringAttributeDescription
        {
            name = "TargetName",
            defaultValue = "--",
        };

        private UxmlIntAttributeDescription _intAttribute = new UxmlIntAttributeDescription
        {
            name = "Age",
            defaultValue = 0,
        };

        private UxmlFloatAttributeDescription _floatAttribute = new UxmlFloatAttributeDescription
        {
            name = "Weight",
            defaultValue = 0f,
        };

        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);

            if (!(ve is CustomElement element)) return;
           
            element.TargetName = _stringAttribute.GetValueFromBag(bag, cc);
            element.Age = _intAttribute.GetValueFromBag(bag, cc);
            element.Weight = _floatAttribute.GetValueFromBag(bag, cc);
        }
    }

    public string TargetName { get; set; }
    public int Age { get; set; }
    public float Weight { get; set; }
}