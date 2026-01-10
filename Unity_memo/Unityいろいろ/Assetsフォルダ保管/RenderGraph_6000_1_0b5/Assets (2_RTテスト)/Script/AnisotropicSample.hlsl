
//バイリニア(`texel`を`mip`の単位にし、`floor(t)`の近傍4点を`frac(t)`で補間する)
float4 BilinearSample(Texture2D tex, float2 texel, int mip)
{
    //`texel`を`mip`の単位にする(MipLv0:1/1倍, MipLv1:1/2倍, MipLv2:1/4倍 ..)
    float2 t = texel * exp2(-mip);

    int2 i = int2(floor(t));
    float2 f = frac(t);

    float4 c00 = tex.Load(int3(i.x + 0, i.y + 0, mip));
    float4 c10 = tex.Load(int3(i.x + 1, i.y + 0, mip));
    float4 c01 = tex.Load(int3(i.x + 0, i.y + 1, mip));
    float4 c11 = tex.Load(int3(i.x + 1, i.y + 1, mip));

    float4 cx0 = lerp(c00, c10, f.x);
    float4 cx1 = lerp(c01, c11, f.x);

    return lerp(cx0, cx1, f.y);
}

//トライリニア(`lod`から2つのMipMap(lod0,lod1)を`w`で補間する)
float4 TrilinearSample(Texture2D tex, float2 texel, float lod, uint levels)
{
    int lod0 = (int)floor(lod);
    int lod1 = min(lod0 + 1, (int)levels - 1);
    float w = saturate(lod - lod0);

    float4 c0 = BilinearSample(tex, texel, lod0);
    float4 c1 = BilinearSample(tex, texel, lod1);

    return lerp(c0, c1, w);
}

//アニソトロピック(長軸方向に`N`個並べて平均をとる)
float4 AnisotropicSample(Texture2D tex, float2 uv, uint maxAniso, SamplerState samplerState)
{
    uint W, H, levels;
    tex.GetDimensions(0, W, H, levels);
    float2 baseTexel = uv * float2(W, H);
    
    //特異ベクトル //UV勾配(ピクセル当たりのテクセル変化量)=======================
    float2 ddx_texel = ddx(uv) * float2(W, H);  float Lx = length(ddx_texel);
    float2 ddy_texel = ddy(uv) * float2(W, H);  float Ly = length(ddy_texel);
    //特異値                        //特異値分解を超簡略化して、
    float a  = max(Lx, Ly); //長軸  //`ddx_texel`,`ddy_texel`を特異ベクトル、
    float b  = min(Lx, Ly); //短軸  //`a`,`b`を特異値として扱っている。
    
    //異方性度 A = a / b ======================================================
    float A = (b > 1e-8) ? (a / b) : maxAniso;
    
    //2の冪に丸める(N)(GPUが概ねこの挙動)
    uint N = 1;
    if (A > 1.0){
        float p = exp2(ceil(log2(A)));
        N = (uint)min(p, (float)maxAniso);
    }
    
    //MipLv(lod)を算出========================================================
    float lod_b  = log2(max(b, 1e-8)); //短軸 //λ_b = log2(b)
    float lod_aN = log2(max(a, 1e-8)) - log2((float)N);//長軸
        //λ_a/N = log2(a/N) = log2(a) - log2(N)
    
    //長軸側(a)に隙間ができる場合は`lod_aN`を使用する(NがmaxAnisoで制限される時)
    float lod    = max(max(lod_b, lod_aN), 0.0);
    
    //長軸方向ベクトル(dir)====================================================
    float2 dir = (Lx >= Ly ? ddx_texel : ddy_texel);
    dir = normalize(dir); //長軸の`正規化特異ベクトル`
    
    //長軸方向(dir)に`N`個サンプルして平均する===================================
    float4 acc = 0;
    for (uint i = 0; i < N; ++i)
    {
        //-0.5〜+0.5 の中央配置(N=4 → t= -0.375, -0.125, 0.125, 0.375)
        float t = (((i + 0.5) / N) - 0.5 );
                        //↑等間隔     ↑0基準
        float2 texel = baseTexel + (t * a * dir);
            //長軸の、`正規化特異ベクトル(dir)`を`特異値(a)`でスケールし、
            //`t`を変えながら長軸方向に等間隔で↓サンプリングしている。
        acc += TrilinearSample(tex, texel, lod, levels);
        // acc += tex.Sample(samplerState, texel / float2(W, H));
    }
    return acc / N; //サンプリングの平均を取る
}
