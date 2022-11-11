using UnityEngine;

//https://docs.unity3d.com/ja/2022.2/Manual/UIE-HowTo-CreateCustomInspector.html#finalCode
public class Car : MonoBehaviour
{
  public string m_Make = "Toyota";
  public int m_YearBuilt = 1980;
  public Color m_Color = Color.black;
  public Tire[] m_Tires = new Tire[4];
}