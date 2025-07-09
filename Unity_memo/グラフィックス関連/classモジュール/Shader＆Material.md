# Shader＆Material

## 関連モジュール

大体、.ctor(..), string name

- struct **GlobalKeyword**: このGlobalなKeywordを使用して、複数の`Shader`に跨るShaderKeywordを**有効,無効,確認**が(外部APIで)できる
  - `.ctor(string name)`: >**既存**の`GlobalKeyword`を**作成**して返します(内部リストに登録されていない場合はエラー)
  - ○`static GlobalKeyword Create(string name)`: **新規**または**既存**の`GlobalKeyword`を**作成**して返します
  - ○`name`: ShaderKeyword(#defineする文字列) (ReadOnly)
- struct **LocalKeyword**: **一つのShader**のShaderKeywordを**有効,無効,確認**が(外部APIで)できる
  - ○`.ctor(Shader shader, string name)`: 指定されたShaderの**LocalKeyword**❰name❱を**作成**し返す(存在しない場合はエラー＆isValid==false)
  - `bool isDynamic`:  `#pragma dynamic_branch＠❰_local❱`でShaderKeywordが定義されているか(ReadOnly)
  - ○`bool isOverridable`: trueの場合、**GlobalKeyword.Create**(string name)でGlobalKeywordが作られている?。(ReadOnly)
    - GlobalKeywordは**有効(enable)のみ上書き可能**?((isOverridable &&) GlobalKeyword || LocalKeyword)
  - `bool isValid`: このLocalShaderKeywordが**有効なObj**かを表す(ReadOnly) (LocalKeyword.ctor(shader,name)やShader.keywordSpace.FindKeyword(name)などで見つからないとfalseになる)
  - ○`string name`: ShaderKeywordの文字列(ReadOnly)
  - `ShaderKeywordType type` (ReadOnly)
    - `None`: タイプなし
    - `BuiltinDefault`: ランタイムに組み込み(Unity?)
    - `UserDefined`: ユーザー定義
    - `Plugin`: シェーダーコンパイラプラグイン?
- struct **ShaderTagId**: 基本的にnameを保持しているだけ(string nameを型付けしただけ(内部でstringをuint❰ID❱に変換しているかも))
  - `Tags{..}`は、`C#`からアクセスする`Shader`の**フィールドメンバ**のようなもの?。(`Tags{"変数" = "初期値"}`)
  - ○`.ctor(string name)`: >指定された`name`を表す`ShaderTagId`を取得または作成します。
  - `static ShaderTagId none`: >`name`を参照しない`ShaderTagId`を記述します。
  - ○`string name`: >`ShaderTagId`によって参照されるタグの`name`を取得します。

## ShaderKeyword

- データ: `＠❰enabled❱＠❰Global❱Keywords＠❰Space❱`
  - Global:`GlobalKeyword[] Shader.＠❰enabled❱GlobalKeywords`
  - Local: `struct LocalKeywordSpace shader.keywordSpace`
  - Local: `LocalKeyword[] material.enabledKeywords`
- 関数:
  - `｡｡｡｡｡⟪Shader¦material⟫.SetKeyword｡｡｡｡｡｡｡(ref ⟪Global¦Local⟫Keyword keyword, bool value)`
  - `bool ⟪Shader¦material⟫.IsKeywordEnabled(ref ⟪Global¦Local⟫Keyword keyword)`

## ShaderProperty

✖＄nameID＝⟪int nameID¦string name⟫
- PropertyToID
  - `int Shader.PropertyToID(string shaderPropertyName)`
- Has
  - `bool ｡｡｡｡｡｡｡｡｡｡｡｡material.Has～｡｡｡｡｡｡｡｡｡｡(∫nameID∫)`
- Get
  - `｢Type｣ ⟪Shader¦material⟫.Get＠❰Global❱～(∫nameID∫)`
- Set
  - `｡｡｡｡｡｡｡｡⟪Shader¦material⟫.Set＠❰Global❱～(∫nameID∫, ｢Type｣ value, ..)`

## Shader情報

- **Shader**
  - `Shader Shader.Find(string ｢ShaderName｣)`
  - `Material.ctor(⟪Shader shader¦Material src⟫)`
  - `Shader material.shader`
- **Tags{..}**
  - `ShaderTagId shader.Find❰Pass❱｡｡｡｡｡TagValue(＠❰int subshaderIndex,❱ int passIndex, ｡｡｡｡｡ShaderTagId tagName)`: Pass{Tags{..}}
  - `ShaderTagId shader.Find❰Subshader❱TagValue(｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡｡int subshaderIndex, ShaderTagId tagName)`: Subshader{Tags{..}}
  - `material.SetOverrideTag(string tagKey, string tagValue)`: `tags{..}`内の`tagKey`を`tagValue`で更新する?
- SubShader/PassのCount
  - `｡｡｡｡｡｡｡｡｡｡｡｡shader.subshaderCount`
  - `⟪material¦shader⟫.passCount`
  - `int shader.GetPassCountInSubshader(int subshaderIndex)`
  - PassIndex
    - `int ｡｡｡material.FindPass｡｡｡(string ｢ShaderPassName｣)`
    - `string material.GetPassName(int ｡｡｡｢ShaderPassIndex｣)`
- **その他**
  - `string Shader.globalRenderPipeline`
  - `int ⟪material¦shader⟫.renderQueue`
  - `bool material.enableInstancing`
  - LOD
    - `int Shader.globalMaximumLOD`
    - `int shader.maximumLOD`

## Materialバリアント

`class_Material.md/- **Materialバリアント**` を参照

## Properties{..}

`class_Shader.md/Properties{..}` を参照

## その他

- `shader.isSupported`
- `bool material.SetPass(int passIndex)`
- `Shader.WarmupAllShaders()`
- `int material.ComputeCRC()`
- GI
  - `bool material.doubleSidedGI`
  - `MaterialGlobalIlluminationFlags material.globalIlluminationFlags`

## 考察(ShaderとMaterial)===============================================================

### ShaderKeywordの有効,無効,確認

keywordSpace(Local)の中にGlobalKeywordも含む?(isOverridable)
  ○(isOverridable &&) **GlobalKeyword || LocalKeyword**

### ShaderPropertyの設定,取得

ShaderPropertyは、Properties{..}含まれていればLocalProperty?。Set/Get(..)は、Local,Global独立 or 優先Local > Global or 上書き ?

- スクリプトからのプロパティ操作について (ナレッジ 32:44) (DOTS InstanceShader でも同じ?)
  - SRP Batcher対応マテリアルに対してMaterial.SetColorなどでスクリプトからシェーダーのCBUFFER外のプロパティを操作している場合、予期せぬ挙動になる場合がある
  - スクリプトから操作するプロパティはProperties{..}に宣言し、CBUFFERに含めるのが無難
  - Inspectorから隠したい場合は[HideInInspector]アトリビュートを使う
