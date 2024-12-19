# Material (UnityObject継承)

主に、保持している`shader`、`.ctor(shader)`、**Local❰Instance❱\[LocalShaderKeyword**、**ShaderProperty]**、Tags{..}操作、**Materialバリアント**

## .ctor

- .ctor(⟪Shader shader¦Material src⟫)
  - ⟪｡`shader`から**Material**を**生成**｡¦｡`src`の**ShaderPropertyをコピー**しMaterialのシャローコピーもする?｡(つまりUnityObjectのClone)⟫

## Instance変数

- `Shader` **shader**: >`Material`で使用している`Shader`
- `int passCount`: >Materialの**Pass数**(ReadOnly) (多分`shader.passCount`と同じ)
- `LocalKeyword[]` **enabledKeywords**: 現在**有効**の**LocalKeywordの配列**を取得する(ReadOnly?)
- `int renderQueue`: `SubShader{Tags{"Queue" = ココ}}`のココの値の**取得と設定**(C#上)ができる
- `bool enableInstancing`: >この`Material`に対して**GPUインスタンス**が有効かどうかを**取得および設定**します。(多分ShaderKeywordの設定とRendererを集めてInstancingコールする?)
- **Materialバリアント**
  - `bool isVariant`
    - >このMaterialが**Materialバリアント**の場合は`true`を返す。(`parent != null`?)
  - `Material parent`
    - >このMaterialの`parent`。(`parent`が`null`でないMaterialは、**Materialバリアント**と呼ばれます)
    - Materialバリアントは、`parent`から**全てのShaderPropertyを継承**し、`ShaderProperty`ごとに**Override**できます。
- mainのShaderProperty
  - `Color color`: >MaterialのMainカラー。(`ShaderPropertyName`が`_Color`または`[MainColor]`属性が付くと**Mainカラー**となる)
  - `Texture mainTexture`: `_MainTex` or `[MainTexture]`を`mainTexture`とする
  - `Vector2 mainTextureScale`: `～_ST`? (`material.⟪Set¦Get⟫TextureScale(..)`を呼ぶのと同じ)
  - `Vector2 mainTextureOffset`: `～_ST`? (`material.⟪Set¦Get⟫TextureOffset(..)`を呼ぶのと同じ)
- GI
  - `bool doubleSidedGI`: 有効にすると、**GIを計算するときジオメトリの両面を考慮**するが、**裏面はライトをベイクしない**。他のオブジェクトが恩恵を受ける?
  - `MaterialGlobalIlluminationFlags globalIlluminationFlags`: >Materialがライトマップとライトプローブをどのように操作(ベイク?)するかを定義します。(教科書4P53)

## Instance関数

- **LocalShaderKeyword**
  - **SetKeyword**`(ref LocalKeyword keyword, bool value)`
    - `value`によりこの`Material内`の`LocalShaderKeyword❰keyword❱`を有効無効(`#define`有り無し)の設定をする。(内部では⟪Enable¦Disable⟫Keyword(..))
  - `bool` **IsKeywordEnabled** `(⟪ref LocalKeyword keyword¦string keyword⟫)`
    - >`LocalShaderKeyword❰keyword❱`の**有効無効**を**チェック**する
  - `⟪Enable¦Disable⟫Keyword(⟪ref LocalKeyword keyword¦string keyword⟫)`: keywordを⟪有効¦無効⟫する。(多分使わない)
- **ShaderProperty**
  - **Has,Set,Get**
    - `bool` **Has**`⟪｡Float¦Integer¦Vector¦Color¦Matrix¦Texture¦＠❰Constant❱Buffer¦Property｡⟫`
        `(⟪int nameID¦string name⟫)`
      - この`Material`に各`｢Type｣`の`nameID`の`ShaderProperty`がある場合、`true`を返す。(`Property`は`｢Type｣`指定なし)
    - **Set**`⟪Float＠❰Array❱¦Integer¦Vector＠❰Array❱¦Color＠❰Array❱¦Matrix＠❰Array❱¦Texture＠⟪Scale¦Offset⟫¦＠❰Constant❱Buffer⟫` (大体`Shader`と同じ)
        `(⟪int nameID¦string name⟫, ｢Type｣ value, ..)`
      - >この`Material`に適用される**Local**な**ShaderProperty**を**設定**します
    - `｢Type｣` **Get**`⟪Float＠❰Array❱¦Integer¦Vector＠❰Array❱¦Color＠❰Array❱¦Matrix＠❰Array❱¦Texture＠⟪Scale¦Offset⟫¦＠❰Constant❱Buffer⟫`
        `(⟪int nameID¦string name⟫)`
      - >`Set～(⟪int nameID¦string name⟫, ｢Type｣ value, ..)`を使用して**以前に設定された**この`Material`の**LocalShaderProperty**を取得します。
  - ShaderProperty名の取得
    - `string[] GetPropertyNames(MaterialPropertyType type)`: `type`に一致する、`Properties{..}`の`｢PropertyName｣`の**配列**を返す?
    - `int[] GetTexturePropertyNameIDs()`: >このMaterialで公開される**全て**の`Texture`の`ShaderProperty`の`nameID[]`。
- **Tags{..}操作**
  - `string GetTag(string tagKey, bool searchFallbacks ＠❰, string defaultValue❱)` (Shaderにも `ShaderTagId`検索系 がある)
    - `tagKey`にマッチする最初の`tagValue`を`SubShader{Tags{..}}`と`Pass{Tags{..}}`から検索する?
      (`searchFallbacks`は全ての`SubShader{..}`と`FallBack`を検索。`defaultValue`は見つからない場合の`文字列`)
  - **SetOverrideTag**`(string tagKey, string tagValue)`: `tags{..}`内の`tagKey`を`tagValue`で更新する?
- **Materialバリアント** (Editor-Only)
  - **Ancestor(祖先)** (`Ancestor`は`parent`とは限らない)
    - `bool IsChildOf(Material ancestor)`: 指定された`ancestor`(アンセスター)がこの`Material`の**祖先**である場合は`true`を返す
    - `ApplyPropertyOverride(Material destAncestor, ⟪int nameID¦string name⟫, bool recUndo)`:
      - >Materialバリアントに関連付けられたOverrideを`destAncestor`に適用します。(Undo有無あり)
        - `destAncestor`は祖先の`Material`であり、そこへ この`Material`の`ShaderProperty❰nameID❱`をOverrideし、**Override先**を**祖先に移す**。と思われる
  - **Override**
    - `bool IsPropertyOverriden(⟪int nameID¦string name⟫)`: この`Material`によって`ShaderProperty`が**Override**されている場合は`true`
    - `Revert＠❰All❱PropertyOverride＠❰s❱(＠⟪int nameID¦string name⟫)`: この`Material`の＠❰全ての❱`ShaderProperty`の`Override`を**削除**する
  - **Lock** (Lockは**子孫**による**Override**が**適用**されない(解除すると適用される))
    - `bool IsPropertyLocked＠❰ByAncestor❱(⟪int nameID¦string name⟫)`: この`Material`の＠❰祖先❱によって`ShaderProperty`が**Lock**されている場合は`true`
    - `SetPropertyLock(⟪int nameID¦string name⟫, bool value)`: この`Material`の`ShaderProperty`の**Lock**状態を`value`で設定する
- **Material要素のコピーと補間**
  - `Copy＠❰Matching❱PropertiesFromMaterial(Material mat)`
    - この`Material`と⟪**同一Shader**を持つMaterial¦matの**積集合**⟫の要素をこの`Material`に**コピー**する?。コピーする項目は、`ShaderKeyword`, `ShaderProperty`, `Materialのフィールド` ?
  - `Lerp(Material start, Material end, float t)`: `ShaderProperty`の`Color`と`float`を`start`から`end`を`t`によって補間するらしい
- **Passの扱い**
  - `bool SetPass(int passIndex)`: `passIndex`の`Pass`の`Vertex,Fragment Shader`を**GPUにセット**する?。それを`GL.⟪Begin¦End⟫`や`Graphics.DrawMeshNow(..)`で描画する
  - **Passの有効無効**
    - `bool GetShaderPassEnabled(string ｢ShaderPassName｣)`: ↓`SetShaderPassEnabled(..)`で`｢ShaderPassName｣`の**Pass**を**無効化**されている場合のみ`false`を返す
    - `SetShaderPassEnabled(string ｢ShaderPassName｣, bool enabled)`: `｢ShaderPassName｣`の**Pass**を`enabled`で**有効無効**する。(無効は、定義してない?のと同じになるらしい)
  - **｢ShaderPassName｣**
    - `int FindPass(string ｢ShaderPassName｣)`: `｢ShaderPassName｣`から`｢ShaderPassIndex｣`を返します。(存在しない場合は、-1)
    - `string GetPassName(int ｢ShaderPassIndex｣)`: ↑の逆射。`｢ShaderPassIndex｣`から`｢ShaderPassName｣`を返します。(存在しない場合は、空文字列)
- `int ComputeCRC()`
  - >この`Material`から**CRCハッシュ値**を計算する
