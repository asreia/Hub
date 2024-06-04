using UnityEngine;
using System.Collections;

// Attach this script to a Camera
public class OnRender : MonoBehaviour {
    Mesh CubeMesh;
    Material TestShader;
    Transform CubeTransform;
    void Awake(){
        TestShader = new Material(Shader.Find ("Unlit/TestShader"));
        CubeMesh = GameObject.Find("Cube").GetComponent<MeshFilter>().mesh; //Debug.Log($"CubeMesh == null: {CubeMesh == null}");
        CubeTransform = GameObject.Find("Cube").GetComponent<Transform>();
    }
    public void OnPostRender() {
        Debug.Log("OnPostRender");
        // set first shader pass of the material
        TestShader.SetPass(0);
        // draw mesh at the origin
        Graphics.DrawMeshNow(CubeMesh, CubeTransform.position, CubeTransform.rotation);
    }
}