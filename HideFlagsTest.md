Rebuild_MonoCube() -> Set_HideFlags() -> Exist_Object() -> 項目の操作 -> Exist_Object()と状態確認  
Exist_Object()と状態確認: コールバック確認 -> Exist_Object() -> MonoCubeのInspector確認 -> ScrObjのInspector確認 -> Sceneファイル確認
Sceneファイル確認: SampleSceneのMonoCube -> AttachMono(fineID) -> ScrObj -> ScrObj.asset
Aw: Awake, En: Enable, Di: Disable, De: Destroy  
消えた: Exist_Object()で 0 または False だった場合  
//..: その他の現象が起きた場合のコメント  
HideFlagsでまとめる? シリアライザコールバックok .Noneも見る Sceneファイルも見る 

- .None
  - MonoCube(GameObject)  
    - HideFlagを設定  
     特になし  
    - スクリプト編集によるドメインリロード  
     SO_Di, AM_BS, SO_BS, AM_AD, SO_AD, SO_En  
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     SO_Di, AM_AD, SO_AD, SO_Aw, AO_En  
    - PlayMode開始  
     AM_BS, AM_BS, AM_BS, SO_Di, AM_BS, SO_BS, AM_AD, SO_AD, SO_En, AM_AD, AM_Aw, AM_En, AM_St  
    - PlayMode終了  
     AM_Di, AM_De, AM_AD  
    - PlayMode中にHideFlagを設定  
     特になし  
    - PlayMode中にUnloadUnusedAssets  
     特になし  
    - PlayMode中にDestroy  
     AM_Di, AM_De  //MonoCube AttachMono Destroy  
    - UnloadUnusedAssets  
     特になし  
    - DestroyImmediate  
     //MonoCube Destroy  
  - AttachMono(Component)  
    - HideFlagを設定  
     特になし  
    - スクリプト編集によるドメインリロード  
     SO_Di, AM_BS, SO_BS, SO_AD, AM_AD, SO_En
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     SO_Di, AM_AD, SO_AD, SO_Aw, AO_En  
    - PlayMode開始  
     AM_BS, AM_BS, AM_BS, SO_Di, AM_BS, SO_BS, AM_AD, SO_AD, SO_En, AM_AD, AM_Aw, AM_En, AM_St  
    - PlayMode終了  
     AM_Di, AM_De, AM_AD  
    - PlayMode中にHideFlagを設定  
     特になし  
    - PlayMode中にUnloadUnusedAssets  
     特になし  
    - PlayMode中にDestroy  
     AM_Di, AM_De  //AttachMono Destroy  
    - UnloadUnusedAssets  
     特になし  
    - DestroyImmediate  
     //AttachMono Destroy  
  - ScrObj(Asset)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - MonoCube(GameObject)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  

    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
- .DontSaveInBuild
  - MonoCube(GameObject)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - AttachMono(Component)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - ScrObj(Asset)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - MonoCube(GameObject)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  

    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
- .DontSaveInEditor
  - MonoCube(GameObject)  
    - HideFlagを設定  
     //AttachMonoもDontSaveInEditorになった  
    - スクリプト編集によるドメインリロード  
     SO_Di, SO_BS, AM_BS, AM_BS, SO_AD, AM_AD, AM_AD, SO_En  
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     AM_AD, 
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - AttachMono(Component)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - ScrObj(Asset)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - MonoCube(GameObject)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  

    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
- .DontSave  
  - MonoCube(GameObject)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - AttachMono(Component)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - ScrObj(Asset)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - MonoCube(GameObject)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  

    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
- .DontUnloadUnusedAsset  
  - MonoCube(GameObject)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - AttachMono(Component)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - ScrObj(Asset)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  
    
    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
  - MonoCube(GameObject)  
    - HideFlagを設定  
     
    - スクリプト編集によるドメインリロード  

    - 手動でシーンリロード  (プロジェクトウィンドウでNew Scene -> SampleScene切り替え)  
     
    - PlayMode開始  
     
    - PlayMode終了  
     
    - PlayMode中にHideFlagを設定  
     
    - PlayMode中にUnloadUnusedAssets  
     
    - PlayMode中にDestroy  
     
    - UnloadUnusedAssets  
     
    - DestroyImmediate  
     
