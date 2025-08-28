# Frame Debugger

- *Frame Debugger*は**GameView**の描画の**Action系cmd**が変わった(有無or順序変更)ときに描画更新する。(*cmd*の引数の変化では更新されない)
  - **更新**したい場合は、*Frame Debugger*の**Disabel→Enable**して更新する。(それか、**ダミー**の**Action系cmd**を仕込む)
- *Frame Debugger*の**Enable**中は、`RenderPipeline.Render(..)`が**時間連続的に呼ばれる**みたい。(`Time.deltaTime`は**Editor Mode**でも機能している)
