# Screen (継承なし) (UnityEngine名前空間)

多分、スワップチェイン(`BRTT.CameraTarget`)のリソース設定
主に、**解像度**, **MSAA**, `GetDisplayLayout(List<DisplayInfo> displayLayout)`, `FullScreenMode`, `float brightness`
      \[`SetResolution(..)`, `SetMSAASamples(..)`](`BRTT.⟪CameraTarget¦Depth⟫`を変えれる?)

`UnityEngine.Device名前空間`にもクラス名もメンバも全く同じ`class Screen`があるが、それは**エディタ上でのプラットフォーム固有**のScreen情報であり、**Device Simulator**と一緒に使用される
**ビルド**では`UnityEngine.Device`と`UnityEngine`の`class Screen`は**全く同じように動作**する

## Static変数

- **解像度**
  - `int ⟪width¦height⟫`: >ピクセル単位(**解像度**)のスクリーンの⟪幅¦高さ⟫ (Read-Only) (`BRTT.CameraTarget`(多分、**スワップチェイン**にBlitされる`内部RenderTexture`))
  - `struct Resolution currentResolution`: >現在の画面の解像度 (Read-Only) (実際の**ディスプレイ実物**の解像度?(**物理デバイス**))
    - `int ⟪width¦height⟫`: >`物理デバイス`?の解像度
    - `RefreshRate refreshRateRatio` >`物理デバイス`?のリフレッシュレート(Hz)
  - `float dpi`: >`物理デバイス`?の現在の解像度 (Read-Only) (Dots per inch: **1インチ当たりのドット数**)
  - `Resolution[] resolutions`: >`物理デバイス`がサポートしている全ての`FullScreenMode.ExclusiveFullScreen`時の**解像度**を返します。(Read-Only)
    - `SetResolution(..)`で設定する用
- **MSAA**
  - `int msaaSamples`: >要求されたスクリーンバッファのMSAAサンプルカウントを取得する。 (`BRTT.CameraTarget`,`BRTT.Depth`)
    - >`SetMSAASamples(..)`によって最後に要求されたMSAAサンプルカウントを取得する。

- メインウインドウ?がある物理デバイス情報
  - `struct DisplayInfo mainWindowDisplayInfo`: >メイン・アプリケーション・ウィンドウ?が表示されている**物理デバイス**?に関連するディスプレイ情報。
    - `int ⟪width¦height⟫`: `物理デバイス`?の解像度
    - `string name`: ディスプレイ名
    - `RefreshRate refreshRate`: リフレッシュレート(Hz)
    - `RectInt workArea`: >左上隅を基準としたディスプレイの作業領域の四角形を指定します。たとえば、macOS の Dock または Windows の**タスクバー**で覆われた領域は**除外**されます。
  - `Vector2Int mainWindowPosition`: >`物理デバイス`?の左上隅に対する**メインウィンドウ**の左上隅の位置。(`DisplayInfo`空間?)
- フルスクリーン
  - `bool fullScreen`: >アプリケーションのフルスクリーンモードを有効にする。(**フルスクリーンになる**)
  - `enum FullScreenMode fullScreenMode`: フルスクリーンの**表示モード** (`ProjectSettings/Player/FullScreenMode`がある)
    - `ExclusiveFullScreen`: 排他モード? (`物理デバイス`がサポート)
    - `FullScreenWindow`: フルスクリーンウィンドウ
    - `MaximizedWindow`: 最大化されたウィンドウ
    - `Windowed`: ウィンドウ付き
- **明るさ**、スリープ
  - `float brightness`: >現在の画面の明るさを示す。(多分、`物理デバイス`の明るさ)
  - `int sleepTimeout`: >省電力設定。**モバイル**のスクリーンが暗くなるまでのタイムアウトを設定します
- 表示,非表示 領域
  - `Rect safeArea`: >画面の安全領域をピクセル単位で返します。(Read-Only) (iPhoneなどのカメラとかが無い領域。**UIを安全に配置できるエリア**)
  - `Rect[] cutouts`: >コンテンツを表示するために機能していない画面領域のリストを返す。(Read-Only) (`DisplayInfo.⟪width¦height⟫`内の表示が見えない部分?(`safeArea`の逆?))
- モバイル画面回転
  - `ScreenOrientation orientation`: >画面の論理的な方向を指定します。(**モバイルの画面**を傾けた時に回転する**向き**だと思われる)
  - `bool autorotateTo⟪『ScreenOrientation』Portrait＠❰UpsideDown❱¦Landscape⟪Left¦Right⟫⟫`:
    - `orientation == ScreenOrientation.AutoRotation❰画面を傾けた時に回転❱`のとき許可される回転状態(`ScreenOrientation`)の設定?

## Static関数

- `SetResolution(int width, int height, FullScreenMode fullscreenMode, RefreshRate preferredRefreshRate)`:
  - 主に**解像度**(`Screen.⟪width¦height⟫`)の**切り替え**だが、`Screen.⟪fullScreen¦fullScreenMode⟫`と`Screen.mainWindowDisplayInfo?.refreshRate`も切り替えれる
- `SetMSAASamples(int numSamples)`: >Unity **SwapChain**のMSAAサンプル数を切り替えます。(>0はQuality設定の値を使用)
  - `SwapChain`という記述があるので`Screen` == `BRTT.CameraTarget`,`BRTT.Depth` == `SwapChain`
- `GetDisplayLayout(List<DisplayInfo> displayLayout)`: >**接続されているディスプレイ**の`DisplayInfo`情報を取得します。
- `AsyncOperation MoveMainWindowTo(ref DisplayInfo display, Vector2Int position)`:
  - >指定された`display`の左上隅を基準として、指定された`position`にメインウィンドウを**移動**する。(`mainWindowPosition`の移動?)
