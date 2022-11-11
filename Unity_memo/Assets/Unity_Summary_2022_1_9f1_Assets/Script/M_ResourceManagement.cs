using S = System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// [ExecuteAlways]
public class M_ResourceManagement : MonoBehaviour
{
    public string _m_Name = "ScrObj_0"; //多分Objectのm_nameと被った
    public ScrObj scrObj;
    public static string s_Name;
    public string setFieldData;
    public static string s_setFieldData;
    public bool enableCallBuckLog = true;
    public bool instantiateFlag = false;
    void OnValidate()
    {
        s_Name = this._m_Name;
        foreach(ScrObj so in ResourceManagement.List_ScrObj) if(so != null && so.name == this._m_Name) scrObj = so;
        s_setFieldData = this.setFieldData;
    }
    void Awake()
    {
        if(enableCallBuckLog) Debug.Log("==Awake=="); //GameObjectがActive:ON時、呼ばれる。
    }
    void OnEnable()
    {
        if(enableCallBuckLog) Debug.Log("==OnEnable=="); //GameObjectがActive:ONかつ(∧)Componentがenabled:ONになる時、呼ばれる
    }
    void Start() 
    {
        if(enableCallBuckLog) Debug.Log("==Start=="); //Awakeと同じで生成中に1回しか呼ばれず(OnEnable,OnDisableのトグルで呼ばれない)Updateの直前に呼ばれる。
                                                        //Instantiate時でも呼ばれる
    }
    float prevTime = -4.0f;
    void Update()
    {
        if(instantiateFlag)
        {
            instantiateFlag = false;
            Object.Instantiate(gameObject);
        }
        if(enableCallBuckLog)
        {
            if(Time.realtimeSinceStartup - prevTime > 4.0f)
            {
                prevTime = Time.realtimeSinceStartup;
                if(enableCallBuckLog) Debug.Log("==Update=="); //毎フレーム呼ばれる
            }
        }
    }
    void OnDisable()
    {
        if(enableCallBuckLog) Debug.Log("==OnDisable=="); //GameObjectがActive:OFFまたは(∨)Componentがenabled:OFFになる時、呼ばれる
    }
    void OnDestroy()
    {
        if(enableCallBuckLog) Debug.Log("==OnDestroy=="); //Awakeが呼ばれていてかつ、Componentが破棄される時、呼ばれる
    }
}

public class ResourceManagement
{
    public static List<ScrObj> List_ScrObj = new List<ScrObj>(); 

    [MenuItem("ScrObj/List_ScrObj_Clear")]
    static void List_ScrObj_Clear()
    {
        Debug.Log("List_ScrObj_Clear");
        List_ScrObj.Clear();
    }

    static int s_index = 0;
    [MenuItem("ScriptableObject/Create")]
    static ScrObj Create_ScrObj()
    {
        ScrObj scrObj = ScriptableObject.CreateInstance<ScrObj>();
        scrObj.name = "ScrObj_" + s_index.ToString(); s_index++;
        List_ScrObj.Add(scrObj);
        Debug.Log($"Create_ScrObj({scrObj.name})");
        return scrObj;
    }
    [MenuItem("ScrObj/ShowAll")]
    static void ShowAll_ScrObj()
    {
        Debug.Log("ShowAll_ScObj");
        if(List_ScrObj.Count == 0){Debug.Log("Empty"); return;}
        foreach(ScrObj scrobj in List_ScrObj)
        {
            Debug.Log($"{scrobj.name}");
        }
    }
    [MenuItem("ScrObj/Set_FieldData")]
    static void Set_FieldData()
    {
        ScrObj scrObj = SearchScrObj();
        Debug.Log($"Set_FieldData: {M_ResourceManagement.s_setFieldData}");
        scrObj.fieldData = M_ResourceManagement.s_setFieldData;
    }
    [MenuItem("Object/Destroy")]
    static void Destroy_ScrObj()
    {
        ScrObj scrObj = SearchScrObj();
        if(scrObj == null){Debug.Log("NotFound"); return;}
        Debug.Log($"Destroy({scrObj.name})");
        Object.DestroyImmediate(scrObj);        //EditModeなのでImmediate
        List_ScrObj.Remove(scrObj);
    }
    static ScrObj SearchScrObj(){
        string name = M_ResourceManagement.s_Name;
        ScrObj target = null;
        foreach(ScrObj scrobj in List_ScrObj)
        {
            if(scrobj.name == name) target = scrobj;
        }
        return target;
    }
    //AssetDatabase=============================================================================================================
    [MenuItem("AssetDatabase/CreateAsset")]
    static void CreateAsset_ScrObj(){
        ScrObj scrObj = SearchScrObj();
        if(scrObj == null){Debug.Log("NotFound"); return;}
        Debug.Log($"CreateAsset_ScrObj({scrObj.name})");
        AssetDatabase.CreateAsset(scrObj,"Assets/" + scrObj.name + ".asset");
        // AssetDatabase.SaveAssets(); //なくても大丈夫かな?
    }
    [MenuItem("AssetDatabase/ImportAsset")]
    static void ImportAsset()
    {
        string fileName = M_ResourceManagement.s_Name;
        Debug.Log($"ImportAsset({fileName})");
        AssetDatabase.ImportAsset("Assets/" + fileName + ".asset");
    }
    [MenuItem("AssetDatabase/LoadAssetAtPath")]
    static void LoadAssetAtPath()
    {
        string fileName = M_ResourceManagement.s_Name;
        ScrObj scrObj = AssetDatabase.LoadAssetAtPath<ScrObj>("Assets/" + fileName + ".asset"); 
            //LoadAssetAtPath<型引数>の型引数にサブアセットの型を入れるとサブアセットを取得できるみたい?LoadAllAssetsAtPathでObject[]版もある
        Debug.Log($"LoadAssetAtPath({scrObj.name})");
        List_ScrObj.Add(scrObj);
    }
    [MenuItem("AssetDatabase/SaveAssets")]
    static void SaveAsset()
    {
        Debug.Log("SaveAsset");
        AssetDatabase.SaveAssets(); //=> Inspectorのプロパティが.assetに反映される(Dirtyが保存される?)
    }
    [MenuItem("AssetDatabase/Refresh")]
    static void Refresh()
    {
        Debug.Log("Refresh");
        AssetDatabase.Refresh();
    }
    [MenuItem("AssetDatabase/AddObjectToAsset")]
    static void AddObjectToAsset()
    {
        ScrObj scrObj = SearchScrObj();
        Debug.Log($"AddObjectToAsset({scrObj.name})");
        AssetDatabase.AddObjectToAsset(scrObj, "Assets/ScrObj_1.asset");
        AssetDatabase.SaveAssets(); //SaveAssets()しないとディスクに保存されない?
    }
    [MenuItem("AssetDatabase/DeleteAsset")]
    static void DeleteAsset()
    {
        ScrObj scrObj = SearchScrObj();
        Debug.Log($"DeleteAsset({scrObj.name})");
        if(AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(scrObj))) List_ScrObj.Remove(scrObj);
        foreach(ScrObj scrObj1 in List_ScrObj.ToArray())    //イテレータでも変更するとエラー。まぁToArray()でいいか
        {
            if(scrObj1 == null) List_ScrObj.Remove(scrObj1); // サブアセットのあるパスを破棄すると全てのサブアセットのC++Objectも破棄される
        }
    } 
    [MenuItem("AssetDatabase/GUIDFromAssetPath")]
    static void GUIDFromAssetPath()
    {
        GUID guid =AssetDatabase.GUIDFromAssetPath("Assets/Texture/Understood.png");
        Debug.Log($"guid: {guid} (Assets/Texture/Understood.png)");
    }
    //SerializeObject======================================================================================================================
    private static SerializedObject serializedObject;
    [MenuItem("SerializedObject/new_SerializedObject")]
    static void new_SerializedObject()
    {
        ScrObj scrObj = SearchScrObj();
        Debug.Log($"new_SerializedObject({scrObj.name})");
        serializedObject = new SerializedObject(scrObj);
    }
    [MenuItem("SerializedObject/FindProperty_get")]
    static void FindProperty_get()
    {
        string fieldData = serializedObject.FindProperty("fieldData").stringValue;
        Debug.Log($"FindProperty_get: {fieldData}");
    }
    [MenuItem("SerializedObject/FindProperty_set")]
    static void FindProperty_set()
    {
        serializedObject.FindProperty("fieldData").stringValue = M_ResourceManagement.s_setFieldData;
        Debug.Log($"FindProperty_set: {M_ResourceManagement.s_setFieldData}");
    }
    [MenuItem("SerializedObject/Update")]
    static void Update()
    {
        Debug.Log("Update()");
        serializedObject.Update();
    }
    [MenuItem("SerializedObject/ApplyModifiedProperties")]
    static void ApplyModifiedProperties()
    {
        Debug.Log("ApplyModifiedProperties()");
        serializedObject.ApplyModifiedProperties();
    }
    //EditorUtility==================================================================================================================
    [MenuItem("EditorUtility/SetDirty")]
    static void SetDirty()
    {
        ScrObj scrObj = SearchScrObj();
        Debug.Log($"SetDirty({scrObj.name})");
        EditorUtility.SetDirty(scrObj);
    }
    [MenuItem("EditorUtility/IsDirty")]
    static void IsDirty()
    {
        ScrObj scrObj = SearchScrObj();
        Debug.Log($"IsDirty({scrObj.name}): {EditorUtility.IsDirty(scrObj)}");
    }
    //SceneManagement==================================================================================================================
    [MenuItem("SceneManagement/SaveScene")]
    static void SaveScene()
    {
        UnityEngine.SceneManagement.Scene scene = 
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log($"SaveScene({scene.name})");
    }
    //Component_Test-----
    // [MenuItem("SceneManagement/_Component_Test")]
    // static void _Component_Test()
    // {
    //     M_ResourceManagement[] a_M_RM = Resources.FindObjectsOfTypeAll<M_ResourceManagement>();
    //     M_ResourceManagement M_RM; if(a_M_RM.Length == 1){M_RM = a_M_RM[0];}else{Debug.Log("1コじゃない"); return;}
    //     M_RM.setFieldData = "スクリプトでUnityObjectを変更";
    //     // EditorUtility.SetDirty(M_RM); //Dirtyが付いていなくてもSceneSaveで.unityに保存する
    //     Debug.Log($"_Component_Test({M_RM.name}), IsDirty: {EditorUtility.IsDirty(M_RM)}");
    // }
    //--------------------
    [MenuItem("SceneManagement/LoadScene_Single")]
    static void LoadScene_Single()
    {
        string sceneName = "New Scene";
        Debug.Log($"LoadScene_Single({sceneName})");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName/*％❰, UnityEngine.SceneManagement.LoadSceneMode.Single❱*/);
    }
    [MenuItem("SceneManagement/LoadScene_Additive")]
    static void LoadScene_Additive()
    {
        string sceneName = "New Scene";     //Scene名のstringをBuildIndexに対応させるメソッドはないのか?Sceneも
        Debug.Log($"LoadScene_Additive({sceneName})");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
    [MenuItem("SceneManagement/UnloadSceneAsync")]
    static void UnloadSceneAsync()
    {
        Debug.Log("UnloadSceneAsync");
        UnityEngine.SceneManagement.Scene scene =
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
    }
    //UnityEngine.ObjectにDestroyImmediateとかあるので、PlayMode外で使えない訳ではない
    //GetActiveScene()はUnityEditorだけどPlayModeでも動いた。ランタイムではdllを読まないので使えないだろう
    //↑の2つより、UnityのEditor上ではPlayMode、非PlayModeでもUnityEngin,UnityEditor区別なく使用できるがランタイムではUnityEditor.dllが読まれないので使えないだろう
    //UnityObject(UnityEngine.Object)=======================================================================================================================
    static GameObject parentCube = null;
    [MenuItem("_GameObject/transform")]
    static void transform()
    {
        if(parentCube == null) {parentCube = GameObject.CreatePrimitive(PrimitiveType.Cube); return;};

        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        tempCube.transform.parent = parentCube.transform;

        tempCube.transform.position = parentCube.transform.position;

        Vector3 position = tempCube.transform.localPosition;
        position.x += 1;
        tempCube.transform.localPosition = position;

        parentCube = tempCube;
    }
    //Resources===============================================================================================================================================
    [MenuItem("Resources/UnloadUnusedAssets")]
    static void UnloadUnusedAssets()
    {
        Debug.Log("UnloadUnusedAssets");
        Resources.UnloadUnusedAssets();
    }
    [MenuItem("Resources/Exist_ScrObj")]
    static void Exist_ScrObj()
    {
        Debug.Log("Exist_ScrObj");
        ScrObj[] scrObjs = Resources.FindObjectsOfTypeAll<ScrObj>();
        foreach(ScrObj so in scrObjs)
        {
            Debug.Log($"name: {so.name}, HideFlag: {so.hideFlags}, IsPersistent: {EditorUtility.IsPersistent(so)}");
        }
    }
    [MenuItem("Resources/Copy_AllScrObj_to_List_ScrObj")]
    static void Copy_AllScrObj_to_List_ScrObj()
    {
        Debug.Log("Copy_AllScrObj_to_List_ScrObj");
        ScrObj[] scrObjs = Resources.FindObjectsOfTypeAll<ScrObj>();
        List_ScrObj.Clear();
        foreach(ScrObj so in scrObjs)
        {
            if(!EditorUtility.IsPersistent(so)) List_ScrObj.Add(so);
        }
    }
    //HideFlag==================================================================================================================================================
    [MenuItem("HideFlag/Set_DontUnloadUnusedAsset")]
    static void Set_DontUnloadUnusedAsset()
    {
        ScrObj scrObj = SearchScrObj();
        Debug.Log($"{scrObj.name} = DontUnloadUnusedAsset");
        scrObj.hideFlags = HideFlags.DontUnloadUnusedAsset;
    }
    [MenuItem("HideFlag/Set_DontSave")]
    static void Set_DontSave()
    {
        ScrObj scrObj = SearchScrObj();
        Debug.Log($"{scrObj.name} = DontSave");
        scrObj.hideFlags = HideFlags.DontSave;
    }
    [MenuItem("HideFlag/Set_FlagTest")]
    static void Set_FlagTest()
    {
        ScrObj scrObj = SearchScrObj();
        Debug.Log($"{scrObj.name} = DontSaveInEditor");
        scrObj.hideFlags = HideFlags.DontSaveInEditor;
    }
    [MenuItem("HideFlag/Set_All_None")]
    static void Set_All_None()
    {
        Debug.Log($"Set_All_None");
        foreach(ScrObj so in List_ScrObj)
        {
            so.hideFlags = HideFlags.None;
        }
    }
    //Util=======================================================================================================================================================
    [MenuItem("Util/ScrObj_Rebuild")]
    static void ScrObj_Rebuild()
    {
        Debug.Log("ScrObj_Rebuild");
        List_ScrObj.Clear();
        s_index = 0;
        ScrObj[] scrObjs = Resources.FindObjectsOfTypeAll<ScrObj>();
        foreach(ScrObj so in scrObjs)
        {
            Object.DestroyImmediate(so,true);
        }
        AssetDatabase.SaveAssets();
        var so1 = Create_ScrObj();
        AssetDatabase.CreateAsset(so1,"Assets/" + so1.name + ".asset");
        so1 = Create_ScrObj();
        so1.hideFlags = HideFlags.DontSave;
        so1 = Create_ScrObj();
        so1.hideFlags = HideFlags.DontUnloadUnusedAsset;
        so1 = Create_ScrObj();
        so1.hideFlags = HideFlags.DontSaveInEditor;
        so1 = Create_ScrObj();
        so1.hideFlags = HideFlags.None;
    }
    [MenuItem("Util/AllDestroy")]
    static void AllDestroy()
    {
        Debug.Log("AllDestroy");
        foreach(var so in Resources.FindObjectsOfTypeAll<ScrObj>())
        {
            if(EditorUtility.IsPersistent(so))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(so)); //AssetDatabase.RemoveObjectFromAssetはサブアセットを消すみたい?//OnDisableとOnDestroyも呼ばれない
                // Debug.Log($"so == null: {so == null}"); //=>true //DeleteAsset(~)するとEditor操作と同等にAssetが完全に消え、メモリ上のUnityObjectも消える
            }
            Object.DestroyImmediate(so,true); //soがnullでもエラーでない
            List_ScrObj.Remove(so);
            s_index = 0;
        }
        AssetDatabase.SaveAssets();
    }
}