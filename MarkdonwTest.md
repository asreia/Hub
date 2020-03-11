# Markdownの使い方メモ

**(\\)(---, \<br>), (#, -, 1., >), (*, \*\*, \`, ~~), (```rb, |:--:|, <>, \[`![]()`]())**  

## リンク

<https://www.asobou.co.jp/blog/bussiness/markdown>

[チートシート](https://qiita.com/kamorits/items/6f342da395ad57468ae3#fnref1)

[Wiki:Markdown](https://ja.wikipedia.org/wiki/Markdown):WikiのMarkdownのページ  

[![画像だよ](https://cldup.com/dTxpPi9lDf.thumb.png "ポップアップ")](https://nodesource.com/products/nsolid)  

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
2. テキスト  
   1. テキスト  
3. テキスト  

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

| A列 | B列 | C列 |D列|E列|
|-----|:---:|-----|--:|---|
| あ  | い  | う  |え |お |
| か  | き  | く  |け |こ |
| さ  | し  | す  |せ |そ |

---

## プログラムを記述する場合

```rb
num = 0
while num < 2 do
   print("num = ", num)
end
print("End")
```  

---
