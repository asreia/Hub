# Markdownの使い方メモ

**(\\)(---, \<br>), (#, -, 1., >), (*, \*\*, \`, ~~), (```rb, |:--:|, <>, \[`![]()`]())**  
前後に複数の改行があっても詰められる  
Format: Shift + Alt + F  
"markdown.preview.breaks": true //改行コードで<br>が付くようにした(スペース改行が必要な場合は"\n"を"  \n"に置換する)

markdownlint: 水平線の上に文字を書くと見出しと見なされ"見出しスタイルが統一されていない"(MD003)と出る
---

## 言語表現

使うときはコピペして書き換えればいいと思う  

- ブロック系  
   ＄見出し＝❰⟦1～⟧❰#❱ ｢見出し｣❱  
   ＄水平線＝❰---❱  
   ＄リスト＝❰⟦～⟧∫LIndent∫- ｢リスト名｣  ❱  
   ＄番号付きリスト＝❰⟦～⟧∫LIndent∫⟪1～⟫. ｢番号付きリスト名｣  ❱  
   ＄引用＝❰⟦1～⟧❰>❱｢引用｣  ❱  
   ＄テーブル＝❰|｡｡⟦|┃＄tableCols＝｢テーブルの列数｣┃⟧｢列の説明｣｡｡|∫LReturn∫｡｡｡｡|｡｡⟦|┃○∫tableCols∫┃⟧❰｡＠❰:❱⟦1～⟧❰-❱＠❰:❱｡❱｡｡|∫LReturn∫｡｡｡｡⏎  
            　⟦∫LReturn∫┃～⟧❰｡｡｡|｡｡⟦|┃○∫tableCols∫｣┃⟧｢テーブルの要素｣｡｡|｡｡｡❱❱ (\<br>を入れると要素内で改行できる)  
   ＄プログラム＝❰```｢言語(右下の言語モードの選択から"()"内にある文字列)＠❰:｢タイトル｣❱｣∫LReturn∫｡｡｢プログラムのコード｣∫LReturn∫｡｡```❱  

- 文字列系  
   ＄改行＝⟪∫LToken∫  ¦\<br>⟫  
   ＄強調＝⟪*｢斜体｣* ¦ **｢太字｣** ¦`｢インライン｣`¦ ~~｢打ち消し｣~~ ¦<font color="｢色｣">｢色付け｣</font>⟫  
   ＄エスケープシーケンス＝❰\｢MarkDownのメタ文字｣❱  

- リンク系  
   ＄URLリンク＝⟪<｢URL｣>｡¦｡[｢URLの名前｣]｡(｡｢URL｣｡)｡⟫  
   ＄画像貼り付け＝❰![｡｢URLの名前｣｡](｡｡｢画像のURL｣＠❰ "｢ポップアップ｣"❱｡｡)❱  
   ＄画像付きURLリンク＝❰[｡｡∫画像貼り付け∫｡｡]｡(｡｢URL｣｡)｡❱  
   ＄アンカーリンク＝❰[｢リンク名｣](#＄id＝｢リンクID｣)｡｡∫LAny∫｡｡<a id="○∫id∫"></a>❱  
   ＄注釈＝❰｢文｣[^＄注釈番号＝⟪1～⟫]｡｡∫LAny∫｡｡[^○∫注釈番号∫]: ｢注釈内容｣❱  

## リンク

<https://www.asobou.co.jp/blog/bussiness/markdown>  

[チートシート](https://qiita.com/kamorits/items/6f342da395ad57468ae3#fnref1)  

[Wiki:Markdown](https://ja.wikipedia.org/wiki/Markdown):WikiのMarkdownのページ  

[![画像だよ](https://cldup.com/dTxpPi9lDf.thumb.png "ポップアップ")](https://nodesource.com/products/nsolid)  
[![ローカル画像だよ](Elaina.png "ポップアップ")](https://nodesource.com/products/nsolid)

## アンカーリンク  

[ページ内リンク(アンカーリンク)](#anchor)  
<a id="anchor"></a>
ここに飛ぶ  

---

## エスケープシーケンス("\")

例： \`インライン表示されなくなる`  
\<br>  

---

## 強調(Ctrl + D & (*,**,`))

- 斜体: *shatai*
- 太字: **futoji**
- インライン: `inline`
- 打ち消し: ~~打ち消し~~
- 色付け: <font color="Red">テキスト</font>   <font color = #3E9A13>テキスト</font>

---

## 注釈(vscodeだと注釈内容がうまくでない↓, 注釈内容は最後の行に入る)

テキスト1[^1]  
テキスト2[^2]  

[^1]: 注釈内容1  
[^2]: 注釈内容2  

---

## 水平線

---

---

## 改行

1行目__（←**半角スペース２つ**）  
2行目  
<br>
<br>
3行目  

---

## リスト

- フシギダネ
  - フシギソウ
    - フシシシシ
      - タピオカパン
- フシギバナ

---

## 番号付きリスト

1. テキスト  
   1. テキスト  
   2. テキスト
2. テキスト  

---

## 引用

>テキスト  
テキスト  
テキスト
>> 二重テキスト  
二重テキスト  

抜ける  

---

## テーブル([:--]左揃え, [:--:]中央揃え, [--:]右揃え)

| A列 |  B列  | C列 |  D列 | E列 |
| --- | :---: | --- | ---: | --- |
| あ  |  い   | う  |   え | お  |
| か  |  き   | く  |   け | こ  |
| さ  |  し   | す  |   せ | そ  |

---

## プログラムや数式を記述する場合

＠❰:｢タイトル｣❱付けれなかった

```rb
num = 0
while num < 2 do
   print("num = ", num)
end
print("End")
```  

>プレビュー画面が(VScode)version 1.58からKaTeX対応を果たしたので数式が書ける！らしい

[Tex記法](https://qiita.com/shepabashi/items/27b7284d1f0007af533b)
[KaTex記法](https://www.suzu6.net/posts/102-katex-math-functions/)
"\$"で囲む
$\left( \sum_{k=1}^n a_k b_k \right)^{\!\!2} \leq\left( \sum_{k=1}^n a_k^2 \right) \left( \sum_{k=1}^n b_k^2 \right)$
$_{124} ABCD _{3333}EFG_{abc}$
$_{23}$ //\_{..}_{..}は不可

コードブロックはvscodeだとうまくいかなかった

```math
left( sum_{k=1}^n a_k b_k right)^{!2} leq
left( sum_{k=1}^n a_k^2 right) left( sum_{k=1}^n b_k^2 right)
```

---
