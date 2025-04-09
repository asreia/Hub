import numpy as np

# 元のデータ（8bit 整数値）
original_data = np.array([15, 35, 80, 100], dtype=np.uint8)

# ステップ1: 0-255 の範囲を 0.0-1.0 に正規化
normalized_data = original_data / 255.0

# ステップ2: sRGB → リニア変換（RGBのみ変換、Alphaはそのまま）
def srgb_to_linear(c):
    if c <= 0.04045:
        return c / 12.92
    else:
        return ((c + 0.055) / 1.055) ** 2.4

linear_rgb = np.array([srgb_to_linear(c) for c in normalized_data[:3]])

# アルファ値はそのままリニア
linear_alpha = normalized_data[3]

# ステップ3: float4 から half4 への変換（近似的に16bit精度に落とす）
half_precision_rgb = np.round(linear_rgb, 5)  # 16bit float の近似精度
half_precision_alpha = np.round(linear_alpha, 5)  # 16bit float の近似精度

# 結果
linear_result = np.append(linear_rgb, linear_alpha)
half4_result = np.append(half_precision_rgb, half_precision_alpha)

# 表示
import pandas as pd
df = pd.DataFrame({
    "ステップ": ["元の値 (8bit)", "正規化 (0-1)", "sRGB→リニア", "float4 → half4"],
    "R": [original_data[0], normalized_data[0], linear_result[0], half4_result[0]],
    "G": [original_data[1], normalized_data[1], linear_result[1], half4_result[1]],
    "B": [original_data[2], normalized_data[2], linear_result[2], half4_result[2]],
    "A": [original_data[3], normalized_data[3], linear_result[3], half4_result[3]]
})

import ace_tools as tools
tools.display_dataframe_to_user(name="sRGB to Linear Conversion Trace", dataframe=df)
