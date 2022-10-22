/*
Static 変数
    基本的な情報系(ほぼReadOnly)
        ID
            buildGUID	                        このビルドのGUIDを返します（読み取り専用）。
            cloudProjectId	                    クラウドプロジェクトの ID。これは必ずユニークな ID です（読み取り専用）
        Name
            companyName	                        アプリケーションを提供している会社名（読み取り専用）
            InstallerName	                    アプリケーションをインストールしたストア、または、パッケージ名を返します (読み込み専用)。
            productName	                        アプリケーションのプロダクト名（製品名）（読み取り専用）
        version
            version	                            アプリケーションのバージョン番号を返します（読み取り専用）
            unityVersion	                    Unity のバージョンを返す。
        言語
            systemLanguage	                    言語はユーザーのオペレーティングシステムで実行されているものです。
        Path
            dataPath	                        ターゲットデバイス上のゲームデータフォルダーへのパスが含まれます（読み取り専用）。
            persistentDataPath	                永続的なデータディレクトリのパスを返します（読み取り専用）
            StreamingAssetsPath	                StreamingAssetsフォルダーへのパス（読み取り専用）。
            temporaryCachePath	                一時的なデータ、キャッシュのディレクトリパスを返します（読み取り専用）
            consoleLogPath	                    コンソールログファイルへのパスを返します。現在のプラットフォームがログファイルをサポートしていない場合は、空の文字列を返します。
            abstractURL	                        ドキュメントのURL。WebGLの場合、これはWebURLです。Android、iOS、またはユニバーサルWindowsプラットフォーム（UWP）の場合、これはディープリンクURLです。（読み取り専用）
        拡張子?
            identifier	                        実行時にアプリケーション識別子を返します。Appleプラットフォームでは、これはinfo.plistファイルに保存されている「bundleIdentifier」であり、Androidでは、AndroidManifest.xmlの「パッケージ」です。
        インストールモード?
            installMode	                        アプリケーションのインストールモードを返します（読み取り専用）
        実行環境
            platform	                        ゲームが何のプラットフォームで実行されているかを返します（読み取り専用）
            internetReachability	            インターネットのアクセス状態を返します。
            sandboxType	                        サンドボックス内での実行モードを返します（読み取り専用）
        チェック系
            isBatchMode	                        コマンドラインから-batchmodeフラグを指定してUnityを起動するとtrueを返します（読み取り専用）。
            isConsolePlatform	                現在のランタイムプラットフォームがコンソールであるかどうか
            isEditor	                        エディター上でプレイしているかを確認します（読み取り専用）
            isFocused	                        プレーヤーが現在フォーカスを持っているかどうか。読み取り専用。
            isMobilePlatform	                現在のランタイムプラットフォームがモバイルであるかどうか
            isPlaying	                        ビルドされた任意の種類のプレーヤーで呼び出された場合、または再生モード（読み取り専用）のエディターで呼び出された場合にtrueを返します。
    フレームレート
        ●targetFrameRate	                    指定されたフレームレートでレンダリングを試みるようにゲームに指示します。
    バックグラウンド実行
        runInBackground	                    ゲーム画面がバックグラウンドでも実行されるべきかを設定します。
        backgroundLoadingPriority           バックグラウンドのスレッドの優先順位を設定します。
    改造されてないか? //ビルド後にアプリケーションが変更されたかどうかをチェックするため、著作権侵害対策チェックとして使用できます。
        genuine	                            アプリケーションが構成された後、なんらかの変更があった場合に false を返します
        genuineCheckAvailable	            アプリケーションの整合性が確認できる場合は true を返します。

Static 関数
    操作系
        OpenURL	                            アプリの現在のプラットフォームと環境の権限と制限に従って、指定されたURLを開きます。これは、URLの性質に応じてさまざまな方法で処理され、ランタイムプラットフォームに応じてさまざまなセキュリティ制限が適用されます。
        ●Unload	                            Unity Playerによって取得されたリリースメモリをアンロードし、現在のプロセスを存続させます。
        Quit	                            アプリケーションを終了します。
    BuildTags
        GetBuildTags	                    このビルドで使用されている機能タグの配列を返します。
        SetBuildTags	                    このビルドの機能タグの配列を設定します。
    StackTraceLogType
        GetStackTraceLogType	            スタックトレースのロギングのオプションを取得します。デフォルト値は StackTraceLogType.ScriptOnly です。
        SetStackTraceLogType	            スタックトレースのロギングのオプションを設定します。デフォルト値は StackTraceLogType.ScriptOnly です。
    チェック系
        IsPlaying	                        指定されたオブジェクトが、任意の種類のビルドされたプレーヤーまたはプレイモードのいずれかでプレイワールドの一部である場合、trueを返します。
        CanStreamedLevelBeLoaded	        ストリーム用のレベルが読み込めるかどうか
        HasProLicense	                    Pro ライセンスでアクティベートされているかどうか
        HasUserAuthorization	            Web Player で Web カメラやマイクの許可を行っているかを確認します。
    iOSリクエスト
        RequestAdvertisingIdentifierAsync	iOS、Android、Windows ストア用の Advertising ID を求めます。
        RequestUserAuthorization	        iOSでウェブカメラまたはマイクを使用するための認証をリクエストします。

イベント
    ログを受信
        logMessageReceived	                ログメッセージが発行されたときに受信するためのイベントハンドラー
        logMessageReceivedThreaded	        ログメッセージが発行されたときに受信するためのイベントハンドラー
        [デリゲート]LogCallback	             ログとして保存されるものをモニタリングするために Application.logMessageReceived や Application.logMessageReceivedThreaded で使用されるデリゲートです。
    メモリ不足
        lowMemory	                        このイベントは、アプリがフォアグラウンドで実行されているときにiOSまたはAndroidデバイスがメモリ不足を通知したときに発生します。これに応じて、重要でないアセット（テクスチャやオーディオクリップなど）をメモリから解放して、アプリが終了するのを防ぐことができます。このようなアセットの小さいバージョンをロードすることもできます。さらに、アプリが終了した場合のデータ損失を回避するために、一時データを永続ストレージにシリアル化する必要があります。このイベントは、さまざまなプラットフォームでの次のコールバックに対応します。-iOS：[UIApplicationDelegate applicationDidReceiveMemoryWarning] -Android：onLowMemory（）およびonTrimMemory（level == TRIM_MEMORY_RUNNING_CRITICAL）コールバックの処理例を次に示します。
        [デリゲート]LowMemoryCallback	     これは、モバイルデバイスがメモリ不足を通知したときのデリゲート機能です。
    終了時
        quitting	                        Unityは、プレーヤーアプリケーションが終了しているときにこのイベントを発生させます。
        wantsToQuit	                        Unityは、プレーヤーアプリケーションが終了したいときにこのイベントを発生させます。
    VRデバイス
        onBeforeRender	                    VRデバイスの「JustBeforeRender」入力更新の登録に使用されるデリゲートメソッド。
    フォーカスが獲得または喪失?
        focusChanged	                    フォーカスが獲得または喪失したイベントの登録に使用するデリゲートを定義します。
    アプリケーションがディープリンクURLを使用してアクティブ化?
        deepLinkActivated	                このイベントは、Android、iOS、またはユニバーサルWindowsプラットフォーム（UWP）で実行されているアプリケーションがディープリンクURLを使用してアクティブ化されたときに発生します。

デリゲート
    AdvertisingIdentifierCallback	    Advertising ID をフェッチするためのデリゲートメソッド
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ApplicationTest{
    [MenuItem("ApplicationTest/Unload")]
    static void Unload(){
        Application.Unload();
    }
    [MenuItem("ApplicationTest/targetFrameRate")]
    static void targetFrameRate(){
        Application.targetFrameRate = 0xFFFF;
    }
}
