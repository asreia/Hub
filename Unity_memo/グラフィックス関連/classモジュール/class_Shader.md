# Shader (UnityObject継承)

主に、`Find(..)`、**Global❰static❱**\[ **ShaderKeyword**、**ShaderProperty]**、**Local**\[**keywordSpace**]、**Tags**、Properties{..}

## **static変数**

Globalに関する項目が多い

- ○`GlobalKeyword[]` `＠❰enabled❱`**GlobalKeywords**: 現在＠❰有効❱のGlobalKeywordの配列を取得する(ReadOnly?)(＠❰enabled❱との差分を取れば**Disabel**も分かりそう)
- `int globalMaximumLOD`: >すべてのシェーダーのシェーダーLOD(Shaderの`SubShader{..}`に付いている**LODの上限値**(上から、この数値以下のSubShaderが選択される))
- `string globalRenderPipeline`: ShaderのSubShaderに付いているTags{"RenderPipeline" = ｢Name｣}の｢Name｣またはØな値にマッチし、上からマッチしたSubShaderを選択する?
  - (つまり、globalRenderPipeline != ｢Name｣以外はマッチ?)
  - `RenderPipelineAsset<TRenderPipeline>.renderPipelineShaderTag => typeof(TRenderPipeline).Name`で設定される (こう設定されるから、設定されている場合は、gRP == ｢Name｣では?)
- `int maximumChunksOverride`: メモリにロードする圧縮Shaderバリアントチャンクの最大数。
  - **正の値**:チャンクの最大数。**0**:全てのチャンク。**負の値**:PlayerSettings.GetDefaultShaderChunkCountに従う(プレイヤー->デフォルトチャンク数?)

## **static関数**

Globalに関する項目が多い

- ○`Shader` **Find**`(string ｢ShaderName｣)`:
  - `｢ShaderName｣`を検索しその`Shader`を取得する(見つからない場合はnull)
  - **Scene**からMaterialを通して**参照されない**Shaderは**ビルドに含まれない**ので、それ以外も検索したい場合は、**Resourcesフォルダ**にShaderまたはMaterialを入れる
- `WarmupAllShaders()`: >現在メモリにロードされた**全てのシェーダー**の**全てのバリアント**を**プリウォーム**する。
  - `shaderVariantCollection.WarmUp()`: ↑のプリウォームが重たい場合はこちらで調整してプリウォームできる
- **ShaderKeyword**
  - ○**SetKeyword**`(ref GlobalKeyword keyword, bool value)`:
    - valueによってkeywordを有効無効(#define有り無し)を設定している。(内部では⟪Enable¦Disable⟫Keyword(..)を使っているらしい)
  - ○`bool` **IsKeywordEnabled**`(ref GlobalKeyword keyword)`:
    - >keywordの**有効無効**を**チェック**する。(Keywordが`keywordSpace`(`enabledGlobalKeywords`じゃなくて?)に無い場合、**falseを返す**)
  - `⟪Enable¦Disable⟫Keyword(⟪ref GlobalKeyword keyword¦string keyword⟫)`: keywordを⟪有効¦無効⟫する。(string版は内部でGlobalKeywordを作成し使用する) (`SetKeyword(..)`で十分)
- **ShaderProperty**
  - ○`int` **PropertyToID**`(string shaderPropertyName)`:
    - `shaderPropertyName`から**nameID**を**取得**します。(static関数なので**全てのShader**で**一意**の`nameID`。つまり**Global的**)
  - ○**SetGlobal**`⟪Float＠❰Array❱¦Integer¦Vector＠❰Array❱¦Color¦Matrix＠❰Array❱¦Texture¦＠❰Constant❱Buffer¦RayTracingAccelerationStructure⟫`
        `(⟪int nameID¦string name⟫, ｢Type｣ value, ..)`
    - >全てのShaderに適用される**Global**な**ShaderProperty**を**設定**します
  - ○`｢Type｣` **GetGlobal**`⟪Float＠❰Array❱¦Integer¦Vector＠❰Array❱¦Color¦Matrix＠❰Array❱¦Texture⟫`
        `(⟪int nameID¦string name⟫)`
    - >`SetGlobal～(⟪int nameID¦string name⟫, ｢Type｣ value, ..)`を使用して**以前に設定された**全ての`Shader`の**GlobalShaderProperty**を取得します。

## **Instance変数**

Localに関する項目が多い

- ○`struct LocalKeywordSpace` **keywordSpace**: このShaderのLocalKeywordSpace。`Fallback`や`UsePass`内の**ShaderKeyword**も含む。(enable版は`material.enabledKeywords`)
  - 変数
    - `uint keywordCount:` Keywordの数
    - `string[] keywordNames`: Keywordの文字列の配列(LocalKeyword.nameと同じと思われる)
    - ○`LocalKeyword[] keywords`: LocalKeywordの配列
  - 関数
    - `LocalKeyword FindKeyword(string name)`: nameのLocalKeywordを探す。見つからない場合は無効(.isValid==false)なLocalKeywordを返す。
- ○`int passCount`: >アクティブなSubShaderの**Pass数**を返します。
- ○`int subshaderCount`: >このシェーダーの**SubShader数**を返します。
- ○`int renderQueue`: >シェーダーのレンダーキュー(ReadOnly) (多分、SubShader{Tags{**"Queue"** = }})
- `int maximumLOD`: `globalMaximumLOD`のLocal版
- `isSupported`: >このShaderが**グラフィックスカード上で実行できるか**どうか(ReadOnly)(Shaderにfallbackが設定されている場合はtrueらしい)

## **Instance関数**

Localに関する項目が多い

- `int GetPassCountInSubshader(int subshaderIndex)`: >指定されたSubShaderの**Pass数**を返します。
- ○**ShaderTagId検索系** (Materialにも `GetTag(..)` がある)
  - `ShaderTagId FindSubshaderTagValue(int subshaderIndex, ShaderTagId tagName)`:
    - `subshaderIndex`の`tagName`を**キー**に、その**バリュー**の**ShaderTagIdを返す**?(SubshaderのTags{..})
  - `ShaderTagId FindPassTagValue(＠❰int subshaderIndex,❱ int passIndex, ShaderTagId tagName)`:
    - ⟪アクティブなSubShader¦`subshaderIndex`⟫の`passIndex`の`tagName`を**キー**に、その**バリュー**の**ShaderTagIdを返す**?(PassのTags{..})
- `Properties{..}`
  - `int GetPropertyCount()`: >このShaderの`Properties{..}`内のプロパティの数を返します。
  - `propertyIndex`系
    - `int FindPropertyIndex(string propertyName)`:
      - `Properties{｢PropertyName｣(..) = ..}`の`｢PropertyName｣`を引数に渡すとその`propertyIndex`❰int❱が返ってくる?
        その`propertyIndex`は↓,↓↓の関数らに渡すと様々な情報を得ることが出来る (`｢PropertyName｣`が見つからない場合は-1が返る)
    - `⟪string[]¦string¦ShaderPropertyFlags¦string¦int¦Vector2¦string¦TextureDimension¦ShaderPropertyType⟫`
          **GetProperty**`⟪Attributes¦Description¦Flags¦Name¦NameId¦RangeLimits¦TextureDefaultName¦TextureDimension¦ShaderPropertyType⟫(int propertyIndex)`:
      - >`Properties{..}`の指定された`propertyIndex`の
          ⟪`attributes¦description string`¦`ShaderPropertyFlags`¦`name`¦`nameId`¦`min and max limits`¦`default Texture name`¦`TextureDimension`¦`ShaderPropertyType`⟫
          を返します。
    - `float GetPropertyDefault⟪Float¦Int¦Vector⟫Value(int propertyIndex)`:
      - >`Properties{..}`の指定された`propertyIndex`のデフォルト(`｢PropertyName｣(..) = ココ?`)の⟪`float`¦`int`¦`Vector4`⟫値を返します。
- よく分からんやつ
  - `bool FindTextureStack (intpropertyIndex、out string stackName、out int layerIndex )`: >テクスチャが属するテクスチャスタックの名前を検索します。
  - `Shader GetDependency (string name)`: >依存関係shaderを返します。shader ソース ファイルには、依存関係shaderが "DependencyName" = "ShaderName" 形式でリストされます。
