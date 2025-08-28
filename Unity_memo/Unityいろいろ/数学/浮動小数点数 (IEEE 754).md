# 浮動小数点数 (IEEE 754)

主に、**比較**, **誤差/桁落ち**, **非正規数/アンダーフロー**(`±0`潰れ), **オーバーフロー**(`±INFINITY`), `NaN` に気をつける

[半精度浮動小数点数](http://ja.wikipedia.org/wiki/%E5%8D%8A%E7%B2%BE%E5%BA%A6%E6%B5%AE%E5%8B%95%E5%B0%8F%E6%95%B0%E7%82%B9%E6%95%B0)
[Unity浮動小数点数](https://www.youtube.com/watch?v=u7C7KdY8-bc)
[GPT-5によるFP教育](https://chatgpt.com/c/68a9dc47-8594-8332-948c-507975e3970c)
[逆Z_FPバッファとHDR](逆Z_FPバッファ_HDR.png)

コード中の`2^n`は`pow(2.0, n)`とする。

## `half型`の数値表現

```c
sign = ｢符号｣; //1bit
exp = ｢指数部｣; //5bit
frac = ｢仮数部｣; //10bit
expMaxValue = (1 << bitSizeof(exp)) - 1; //2^5-1 = 0x1f
bias = expMaxValue >> 1; //0x0f
fracValue = frac / (1 << bitSizeof(frac)); //0.frac
fracMSB = 1 << (bitSizeof(frac) - 1); //0x0200

value =
    exp == 0x00 ?
        frac == 0x00 ?
            sign == 0x00 ? 0.0 : -0.0 //+0 : -0
        ://frac != 0x00
            (-1)^sign * 2^(0x01 - bias) * (0 + fracValue) //exp == 0x00 (`exp`を`0x01`とする代わりに仮数部を`fracValueのみにする)(非正規仮数(極小の値)(FTZ/DAZで0に潰れる可能性がある))
    :exp < expMaxValue ?
        (-1)^sign * 2^(exp - bias) * (1 + fracValue) //exp != 0x00 (正規化数)
    ://exp == expMaxValue
        frac == 0x00 ?
            sign == 0x00 ? +INFINITY : -INFINITY // 1.0/0.0 : -1.0/0.0 //+∞ : -∞ //C#: 浮動小数点型.PositiveInfinity : 浮動小数点型.NegativeInfinity
        :frac & fracMSB != 0 ?
            NaN // 0.0/0.0 //qNaN //C#: 浮動小数点型.NaN
        :
            ｢sNaN｣ //リテラル表現は無い
;
```

## 値の伝播

`±0` => ⟪`±0`¦`有限値`¦`±INFINITY`¦`NaN`⟫
`有限値` => ⟪`±0`¦`有限値`¦`±INFINITY`¦`NaN`⟫
`±INFINITY` => ⟪`±INFINITY`¦`NaN`⟫
`NaN` => ❰`NaN`❱

つまり、`±INFINITY`と`NaN`には気をつける

## 比較

```c
//half: sign:1bit, exp:5bit, frac:10bit (約10進数で3桁)
half Half_ULP(half x) //ULP(Units in the Last Place)
{
    int fracEpsilonExp = 10; //`frac:10bit`
    int exp = (x & (0b11111 << 10)) >> 10; //`x`から`exp:5bit`抽出
    if(exp == 0b00000){exp = 0b00001}; //非正規化数のときの指数
    int bias = 0b01111;
    int expApplyBias = exp - bias;
    half xValueRelativeEpsilon = (half)2^(expApplyBias - fracEpsilonExp);
    return xValueRelativeEpsilon;
}

☆ epsilon = 2^-10 //2^-`frac:10bit` (`1.0`付近のULP) (`1.0`付近の最小の差(epsilon)) (機械イプシロン)

//binary16(half)の数字まとめ
  //・`機械イプシロン`(`1.0`付近の最小差)：ε(1) = 2^-10 ≈ 9.77 × 10^-4
  //・`最小正規化数`(`exp=1`の値)：2^(1-15) = 2^-14 ≈ 6.10 × 10^-5
  //・`最小非正規化数`(`0.0`付近の最小差)：2^(1-15)-10 = 2^-24 ≈ 5.96 × 10^-8 (`0`に丸められる可能性がある)

//.NET (C#): `float.Epsilon`は「最小サブノーマル」で違う意味。なので、`MathF.BitIncrement(1.0f) - 1.0f`で正しい`epsilon`(機械イプシロン)を作れる

//Unityの比較(bool Mathf.Approximately(float a, float b)) //Approximately(アプロクシメイトリ): 約
namespace System
{
    public readonly struct Single/*(float)*/ : ～
    {
        public const Single Epsilon = 1E-45F; //`float.Epsilon`は、`最小非正規化数`
    }
}
public struct MathfInternal
{
    public static volatile float FloatMinNormal = 1.1754944E-38f; //`最小正規化数`
    public static volatile float FloatMinDenormal = float.Epsilon;
    public static bool IsFlushToZeroEnabled = FloatMinDenormal == 0f; //環境でFTZ/DAZが有効なら`true`になる (`0f`に丸められる)
}
public static readonly float Epsilon = (MathfInternal.IsFlushToZeroEnabled ? MathfInternal.FloatMinNormal : MathfInternal.FloatMinDenormal);
public static bool Approximately(float a, float b)
{
    //1E-06fに含まれるk: k ≈ 1e-6 / 2^-23 ≈ 8.388608 (`2^-23`はfloat型の1.0のepsilon)
    //相対式は ULP 換算だと k · m ULP 許容(m は仮数で 1〜2) → Unity の 1e-6 は実質 約 8.4〜16.8 ULP を許す設定になっています。
    return Abs(b - a) < Max(1E-06f * Max(Abs(a), Abs(b)), Epsilon * 8f);
}

//Halfの演算(+)の丸め有無を判定。(floatで真値を作る)
static bool AddExactHalf(Half a, Half b) //Exact(エグザクト): ちょうど
{
    float exact = (float)a + (float)b;
    Half  r     = (Half)exact;
    return (float)r == exact;
}
```
- ☆**浮動小数点数のイメージ**
  - 2つの浮動小数点数値`a`,`b` があり、それらの**あらゆる演算**を計算したときに、演算の**真の数学的結果**が、**その型の表現可能な値**と**ぴったり一致**する場合には、丸めは発生せず**誤差は生じない**。
    - (浮動小数点演算器は、**演算結果**を**その型の表現可能な値**に**丸めて返す**仕組みです)
  - `log10(1/epsilon)`=`log10(1/9.77e-4『halfの機械イプシロン』)`=`約3桁`は、その**型**の**有効桁数(仮数部桁数)**を表す。(浮動小数点数はそれを**左右に動かしている**だけ)
    - **非正規化数**(サブノーマル領域)(`exp=0`のとき) はこの関係が崩れて、**さらに精度が落ちる**。(`0`へ潰れていく)
  - expの**浮動レンジ桁数**は、約`±log10(2^(2^(expビット数 - 1) - 1))`
    - 浮動小数点数は`1.0`**付近で使う**のが、`exp`的に**中央**であり、扱いやすい
  - ☆**最小正規化数**～**最大正規化数**は、二進数の**桁が増える**毎に`1/2`倍に**精度が悪く**なり、
    **最小正規化数**から桁が`n`コ増えたとき、**最小正規化数**に比べて`1/2^n`倍に**絶対精度**が**悪く**なる(`2^n ∝ ULP(2^n)`)。
      そして、**非正規化数**は**桁が減る**毎に`0`に潰れて**相対精度**が**悪く**なる。
    - **現実世界**の浮動小数点数は、**仮数部**が**無限bitサイズ**なので、**精度が落ちない**
    - [0～1に対するULP加算数](ULP加算数.png)
  - `FTZ/DAZ`とは、**非正規化数**を**切り捨てる**
    - `FTZ` (Flush-To-Zero)
      計算の結果が**非正規化数**になると`0`に丸める動作
    - `DAZ` (Denormals-Are-Zero)
      計算の入力に**非正規化数**が来たら`0`とみなして計算する動作
    - どちらも「`0`に近いの**極小の数(非正規化数)**」を**切り捨てるモード**
      理由はシンプルで、**非正規化数**は**ハード的に遅くなる**(スローパス)ことが多いから
    - **GPU**(`HLSL/GLSL/CUDA`など)
      多くの `GPU/シェーダ`は標準で**非正規化数**を`0`に**潰す**(`FTZ`相当)ことが多い
  - **UNorm** (例：`R8_UNORM`)
    - 格納：`i: 0..255` の整数 (byte型)
    - 読み：`f = i / 255` (**等間隔**)
    - 書き：`i = round(saturate(f) * 255)`
    - 例：`i=128` → `f≈0.50196` (※**0.5 は表せない**)
    - `SNORM` は `−1..+1` に**線形スケール** (負側が僅かに非対称)

- **比較**
  - **ULPベース比較**をやるなら：
    - `abs(a-b) < k * ULP(max(abs(a),abs(b)))` のように、**k（何ULPまで許すか⟪1～4⟫）**を調整するのが定番。(`ULP(.)`が**信用できる環境**ならば、`max(.,epsilon_abs)`は要らない)
  - **グラフィックス/ゲーム用途**なら、まずは
    - `abs(a-b) < epsilon` の 固定`epsilon`から始めて問題出たら *ULP*または以下 に切替える、が現実的 (`epsilon`: *機械イプシロン*)
    - `abs(a-b) < k * epsilon * max(abs(a),abs(b))`『`k = ⟪1～4⟫` (`0.0`付近で`0`へ潰れる可能性がある)
    - ☆`abs(a-b) < max(k * epsilon * max(abs(a),abs(b)), epsilon)`『`max(., epsilon)`の`epsilon`は、`0.0`付近で`0`への潰れ回避 (`k=4`で良いでしょう)
      - 普通の数値(`a`,`b`)は、仮数部:`[1.0～1.1111..]`の`[1～2) ULP`にしかならないので`k * [1～2) ULP`で増やす。(GPT-5:「そこまで誤差に厳しくしない」用途なら、これで十分手堅いです。)
        `half value`が、指数部(バイアス適用済み指数:`1`), 仮数部(`1.1111..`)のとき、
        `value`のULPは、`2^(-10+1)`=`2^-9`
        `1.0`のULPは、`epsilon` = `2^-10`
        `epsilon * value` を考えると、
          `= (2^-10) * (2^1 * 1.1111..)`
          `= (2^-10 * 2^1) * 1.1111..`
          `= (2^-9) * 1.1111..`
        となり、`value`は、`1.1111..ULP`(2進数表記)にしかならない。(10進数で約`2ULP`)
    - `abs(a-b) < max(k * epsilon * max(abs(a),abs(b)), epsilon_abs)`『`epsilon_abs`は、`0.0`付近で`0`への潰れ回避 (*最小 サブノーマル*～*最小 正規化数*)

- `ULP`とは
  - ある浮動小数点数`x`に対して、**「x とその次に表現できる浮動小数点数との差」**を`1 ULP`と呼びます。
    - 言い換えると、**「その数の仮数部の最下位ビット（LSB）が表す重み」**が `ULP`。
  - 例（binary32: `float`, 仮数部`23ビット`＋隠れ1 = 有効桁24）：
    - `1.0`付近では `ULP` = `2^-23` ≈ `1.19e-7`
    - `1e8`付近では指数が大きいので `ULP` ≈ `2^-23 × 2^27` = `16`
      - → つまり`1e8`付近では「`±16` が最小の刻み」
  - なぜ`1 ULP`では**足りない**のか？
    - 演算ごとに丸めが入る
      - IEEE754では「演算結果は毎回丸められる」ため、
        - A→B→C と複数演算を経ると、差が **1ULP以上に累積**する。
    - 異なる演算順序・実装差
      - GPUとCPU、最適化有無で丸め順序が変わると、数ULPずれることがある。
      - 特に並列和（リダクション）やSIMD演算。
    - 異符号ゼロやサブノーマル
      - +0.0 と -0.0 はビットパターンでは異なるが、数学的には等しい。
      - flush-to-zero の環境だと「1ULPの違い」で丸め落ちる場合もある。

## 気をつけること集

### 丸め誤差

- 浮動小数点は有限ビットで**実数を近似**しているため「ぴったり表せない数」が多いです。
  例: `0.1 + 0.2 == 0.3` → `false`（実際には `0.30000000000000004` になる）。

- 気をつけ方
  - 等値比較を直接しない。
  - 誤差を許す比較（`fabs(a-b) < epsilon`）を使う。

### 桁落ち (catastrophic cancellation)

- ほぼ等しい大きな数を引き算すると、有効桁が失われる。
  例: `(1e20 + 1) - 1e20` → `0` （`1`が消える）。

- 気をつけ方
  - 計算順序を工夫する（数値解析のアルゴリズムで定番のテクニック）。

### 表現範囲の限界

- すごく大きい数 → **オーバーフロー** で `±INFINITY`
- すごく小さい数 → **アンダーフロー** で `±0` になったり、非正規数（精度が落ちる状態）になる。

- 気をつけ方
  - 計算前にスケーリングする。
  - 非正規数を避けるためのフラグ（例: flush-to-zero）を理解しておく。

### 非結合性 (non-associativity)

実数なら `(a+b)+c == a+(b+c)` が成り立つけど、**浮動小数点**では**誤差**で違ってくる。
例:
```c
double a=1e16, b=-1e16, c=1.0;
(a+b)+c = 1.0
a+(b+c) = 0.0
```

- 気をつけ方
  - 並列計算やリダクション（総和計算）では結果が順序依存になることを理解しておく。

### プラットフォーム依存

- CPU/GPU、コンパイラ最適化、FPU の丸めモードによって微妙に結果が変わる。
- 特に GPU 並列演算では「順序が保証されない」ため和の結果が揺れることがある。

### 特殊値との相互作用

- すでに話したとおり、±0, ±∞, NaN は「伝播ルール」がある。
- 特殊値を使うことでアルゴリズムが壊れないかチェックが必要。

### まとめ

- **誤差の存在**を前提に設計する（**丸め誤差**、**桁落ち** に注意）
- **範囲外の挙動**を知っておく（**オーバーフロー**:`±INFINITY`、**アンダーフロー**:`±0`）
- **順序依存**を意識する（特に並列処理・リダクション）
- **特殊値**の扱いを理解する（`±INFINITY`、`NaN` に注意）
