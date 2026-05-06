using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.RenderGraphModule.Util
{
    /// <summary>
    /// RenderGraph で使用するヘルパー関数です。
    /// </summary>
    public static partial class RenderGraphUtils
    {
        static MaterialPropertyBlock s_PropertyBlock = new MaterialPropertyBlock();

        /// <summary>
        /// コピー パスの MSAA 版に必要なシェーダー機能が、現在のプラットフォームでサポートされているかを確認します。
        /// </summary>
        /// <returns>コピー パスに必要なシェーダー機能が MSAA でサポートされている場合は true、そうでない場合は false を返します。</returns>
        public static bool CanAddCopyPassMSAA()
        {
            if (!IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform())
                return false;

            return Blitter.CanCopyMSAA();
        }

        /// <summary>
        /// コピー パスの MSAA 版に必要なシェーダー機能が、現在のプラットフォームでサポートされているかを確認します。
        /// </summary>
        /// <param name="sourceDesc">コピー元となるテクスチャの説明です。</param>
        /// <returns>コピー パスに必要なシェーダー機能が MSAA でサポートされている場合は true、そうでない場合は false を返します。</returns>
        public static bool CanAddCopyPassMSAA(in TextureDesc sourceDesc)
        {
            if (!IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform())
                return false;

            return Blitter.CanCopyMSAA(sourceDesc.bindTextureMS);
        }

        /// <summary>
        /// コピー パスの MSAA 版に必要なシェーダー機能が、現在のプラットフォームでサポートされているかを確認します。
        /// </summary>
        /// <param name="bindTextureMS">コピー元となるテクスチャが MSAA テクスチャとしてバインドされるかを示します。</param>
        /// <returns>コピー パスに必要なシェーダー機能が MSAA でサポートされている場合は true、そうでない場合は false を返します。</returns>
        public static bool CanAddCopyPassMSAA(bool bindTextureMS)
        {
            if (!IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform())
                return false;

            return Blitter.CanCopyMSAA(bindTextureMS);
        }

        internal static bool IsFramebufferFetchEmulationSupportedOnCurrentPlatform()
        {
#if PLATFORM_WEBGL
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                return false;
#endif
            return true;
        }

        internal static bool IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform()
        {
            // TODO: PS4/PS5 で framebuffer fetch emulation のサポート可否をより効率的に扱う方法が用意されるまで、このユーティリティを一時的に無効化します。
            return (SystemInfo.graphicsDeviceType != GraphicsDeviceType.PlayStation4
                 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.PlayStation5 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.PlayStation5NGGC);
        }

        /// <summary>
        /// 指定されたテクスチャについて、現在のプラットフォームで framebuffer fetch がサポートされているかを判定します。
        /// これには、framebuffer fetch emulation の一般的なサポートと、
        /// マルチサンプル（MSAA）テクスチャ固有のサポートの両方の確認が含まれます。
        /// </summary>
        /// <param name="graph">このパスを追加する RenderGraph です。</param>
        /// <param name="tex">framebuffer fetch との互換性を検証するテクスチャ ハンドルです。</param>
        /// <returns>
        /// 指定されたテクスチャについて、現在のプラットフォームで framebuffer fetch がサポートされている場合は true を返します。
        /// そうでない場合は false を返します。
        /// </returns>
        public static bool IsFramebufferFetchSupportedOnCurrentPlatform(this RenderGraph graph, in TextureHandle tex)
        {
            if (!IsFramebufferFetchEmulationSupportedOnCurrentPlatform())
                return false;

            if (!IsFramebufferFetchEmulationMSAASupportedOnCurrentPlatform())
            {
                var sourceInfo = graph.GetRenderTargetInfo(tex);
                if (sourceInfo.msaaSamples > 1)
                    return sourceInfo.bindMS;
            }
            return true;
        }

        /// <summary>
        /// RenderGraph 内で、指定されたコピー元テクスチャとコピー先テクスチャの間にコピー パスを使用できるかを確認します。
        /// </summary>
        /// <param name="graph">このパスを追加する RenderGraph です。</param>
        /// <param name="source">データのコピー元となるテクスチャです。</param>
        /// <param name="destination">データのコピー先となるテクスチャです。source とは異なる必要があります。</param>
        /// <returns>指定されたコピー元テクスチャとコピー先テクスチャの間でコピー パスを使用できる場合は true、そうでない場合は false です。</returns>
        public static bool CanAddCopyPass(this RenderGraph graph, TextureHandle source, TextureHandle destination) //大体`AddCopyPass(..)`の最初のチェックと同じ
        {
            if (!source.IsValid() || !destination.IsValid())
                return false;

            if (!graph.nativeRenderPassesEnabled)
                return false;

            if (!IsFramebufferFetchEmulationSupportedOnCurrentPlatform())
                return false;

            var sourceInfo = graph.GetRenderTargetInfo(source);
            var destinationInfo = graph.GetRenderTargetInfo(destination);

            if (sourceInfo.msaaSamples != destinationInfo.msaaSamples)
                return false;

            if (sourceInfo.width != destinationInfo.width ||
                sourceInfo.height != destinationInfo.height)
                return false;

            if (sourceInfo.volumeDepth != destinationInfo.volumeDepth)
                return false;

            if (GraphicsFormatUtility.IsDepthFormat(sourceInfo.format) || GraphicsFormatUtility.IsDepthFormat(destinationInfo.format))
                return false;

            // 注: SV_SampleIndex をサポートするには shader model ps_4.1 が必要です。そのため、一部のプラットフォームではコピー パスの MSAA がサポートされません。
            //       これは、コピー シェーダーが持つシェーダー パス数を確認することで判定できます。
            //       ターゲットで MSAA パスを使用できない場合は 1、使用できる場合は 2 になります。
            //       https://docs.unity3d.com/2017.4/Documentation/Manual/SL-ShaderCompileTargets.html
            //       https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-to-get-sample-position
            if ((int)sourceInfo.msaaSamples > 1 && !CanAddCopyPassMSAA(sourceInfo.bindMS))
                return false;

            return true;
        }

        class CopyPassData
        {
            public bool isMSAA;
            public bool force2DForXR;
        }

        /// <summary>
        /// コピー元テクスチャからコピー先テクスチャへデータをコピーするパスを追加し、builder を返します。
        /// テクスチャ内のデータはピクセル単位でコピーされます。このコピー関数は 1:1 コピーのみを行い、データのスケーリングや
        /// テクスチャ フィルタリングは行えません。さらに、コピー元とコピー先のサーフェスはピクセル単位で同じサイズで、MSAA サンプル数と配列スライス数も同じである必要があります。
        /// テクスチャがマルチサンプルの場合は、個々のサンプルがコピーされます。
        ///
        /// コピーは、タイル ベース GPU で最適な性能を得るために framebuffer fetch で実装できるよう、意図的に機能が制限されています。より汎用的な
        /// 機能が必要な場合は AddBlitPass 関数を使用してください。目的の操作でコピー パスがサポートされるかを確認するには、CanAddCopyPass 関数を使用します。
        ///
        /// XR が有効な場合、両目を含む配列テクスチャは自動的にコピーされます。
        /// 
        /// </summary>
        /// <param name="graph">このパスを追加する RenderGraph です。</param>
        /// <param name="source">データのコピー元となるテクスチャです。</param>
        /// <param name="destination">データのコピー先となるテクスチャです。source とは異なる必要があります。</param>
        /// <param name="returnBuilder">追加されたコピー パスの builder インスタンスです。</param>
        /// <param name="passName">デバッグおよびエラー ログに使用する名前です。この名前は RenderGraph デバッガーに表示されます。</param>
        /// <param name="file">この関数が呼び出されたソース ファイルのファイル パスです。デバッグに使用されます。このパラメーターはコンパイラーによって自動生成されるため、ユーザーが渡す必要はありません。</param>
        /// <param name="line">この関数が呼び出されたソース ファイルの行番号です。デバッグに使用されます。このパラメーターはコンパイラーによって自動生成されるため、ユーザーが渡す必要はありません。</param>
        /// <returns>追加されたコピー パスの builder インスタンスです。</returns>
        public static IBaseRenderGraphBuilder AddCopyPass( //1枚のFBF slice:0 mip:0 color to color
            this RenderGraph graph,
            TextureHandle source,
            TextureHandle destination,
            string passName = "Copy Pass Utility",
            bool returnBuilder = false
#if !CORE_PACKAGE_DOCTOOLS
            , [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
#endif
        {
            if (!graph.nativeRenderPassesEnabled)
                throw new ArgumentException("CopyPass はネイティブのレンダーパスでのみサポートされています。ネイティブレンダーパスではないプラットフォームでは、代わりに blit 関数を使用してください。");

            var sourceInfo = graph.GetRenderTargetInfo(source);
            var destinationInfo = graph.GetRenderTargetInfo(destination);

            if (sourceInfo.msaaSamples != destinationInfo.msaaSamples)
                throw new ArgumentException("コピー元テクスチャとコピー先テクスチャの MSAA サンプル数が一致しません。");

            if (sourceInfo.width != destinationInfo.width ||
                sourceInfo.height != destinationInfo.height)
                throw new ArgumentException("コピー元テクスチャとコピー先テクスチャの寸法が一致しません。");

            if (sourceInfo.volumeDepth != destinationInfo.volumeDepth)
                throw new ArgumentException("コピー元テクスチャとコピー先テクスチャのスライス数が一致しません。");

            if (GraphicsFormatUtility.IsDepthFormat(sourceInfo.format) || GraphicsFormatUtility.IsDepthFormat(destinationInfo.format))
                throw new ArgumentException("コピー元またはコピー先テクスチャの depth フォーマットはサポートされていません。代わりに AddBlitPass を使用してください。");

            var isMSAA = (int)sourceInfo.msaaSamples > 1;

            // 注: SV_SampleIndex をサポートするには shader model ps_4.1 が必要です。そのため、一部のプラットフォームではコピー パスの MSAA がサポートされません。
            //       これは、コピー シェーダーが持つシェーダー パス数を確認することで判定できます。
            //       ターゲットで MSAA パスを使用できない場合は 1、使用できる場合は 2 になります。
            //       https://docs.unity3d.com/2017.4/Documentation/Manual/SL-ShaderCompileTargets.html
            //       https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-to-get-sample-position
            if (isMSAA && !CanAddCopyPassMSAA(sourceInfo.bindMS)) //FBFが可能 かつ MSAAのサンプル毎のコピーができるかをチェックする //￢(isMSAA→CanAddCopyPassMSAA) ⇒ ￢(￢isMSAA∨CanAddCopyPassMSAA) ⇒ isMSAA∧￢CanAddCopyPassMSAA
                throw new ArgumentException("ターゲットは AddCopyPass の MSAA をサポートしていません。代わりに blit を使用するか、非 MSAA テクスチャを使用してください。");

            var builder = graph.AddRasterRenderPass<CopyPassData>(passName, out var passData, file, line);

            try
            {
                bool isXRArrayTextureActive = TextureXR.useTexArray;
                bool isArrayTexture = sourceInfo.volumeDepth > 1;

                passData.isMSAA = isMSAA;
                passData.force2DForXR = isXRArrayTextureActive && (!isArrayTexture);

                builder.SetInputAttachment(source, 0);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
                builder.SetRenderFunc(static (CopyPassData data, RasterGraphContext context) => CopyRenderFunc(data, context));

                if (passData.force2DForXR)
                    builder.AllowGlobalStateModification(true);// キーワードを設定できるようにするため
            }
            catch
            {
                builder.Dispose();
                throw;
            }

            if (returnBuilder)
                return builder;

            builder.Dispose();
            return null;
        }

        /// <summary>
        /// コピー元テクスチャからコピー先テクスチャへデータをコピーするパスを追加します。テクスチャ内のデータはピクセル単位でコピーされます。このコピー関数は 1:1 コピーのみを行い、データのスケーリングや
        /// テクスチャ フィルタリングは行えません。さらに、コピー元とコピー先のサーフェスはピクセル単位で同じサイズで、MSAA サンプル数も同じである必要があります。テクスチャがマルチサンプルの場合は、
        /// 個々のサンプルがコピーされます。
        ///
        /// コピーは、タイル ベース GPU で最適な性能を得るために framebuffer fetch で実装できるよう、意図的に機能が制限されています。より汎用的な
        /// 機能が必要な場合は AddBlitPass 関数を使用してください。Blit は引数に基づいて、通常のレンダリングを使うか内部で copy を呼び出すかを自動的に判断します。
        /// 目的の操作でコピー パスがサポートされるかを確認するには、CanAddCopyPass 関数を使用します。
        ///
        /// コピー元/コピー先の mip と slice 引数は無視され、この関数では一度も使用されていません。そのため、それらを持たない AddCopyPass オーバーロードを呼び出す方が適切です。この関数は
        /// 既存コードとの後方互換性のために残されています。
        ///
        /// XR が有効な場合、両目を含む配列テクスチャは自動的にコピーされます。
        ///
        /// </summary>
        /// <param name="graph">このパスを追加する RenderGraph です。</param>
        /// <param name="source">データのコピー元となるテクスチャです。</param>
        /// <param name="destination">データのコピー先となるテクスチャです。source とは異なる必要があります。</param>
        /// <param name="sourceSlice">この引数は使用されたことがありません。代わりに、この引数を持たないオーバーロードを使用してください。mip や配列スライスを扱いたい場合は、blit を使うか、frame buffer fetch ベースの独自実装を書いてください。</param>
        /// <param name="destinationSlice">この引数は使用されたことがありません。代わりに、この引数を持たないオーバーロードを使用してください。mip や配列スライスを扱いたい場合は、blit を使うか、frame buffer fetch ベースの独自実装を書いてください。</param>
        /// <param name="sourceMip">この引数は使用されたことがありません。代わりに、この引数を持たないオーバーロードを使用してください。mip や配列スライスを扱いたい場合は、blit を使うか、frame buffer fetch ベースの独自実装を書いてください。</param>
        /// <param name="destinationMip">この引数は使用されたことがありません。代わりに、この引数を持たないオーバーロードを使用してください。mip や配列スライスを扱いたい場合は、blit を使うか、frame buffer fetch ベースの独自実装を書いてください。</param>
        /// <param name="passName">デバッグおよびエラー ログに使用する名前です。この名前は RenderGraph デバッガーに表示されます。</param>
        /// <param name="file">この関数が呼び出されたソース ファイルのファイル パスです。デバッグに使用されます。このパラメーターはコンパイラーによって自動生成されるため、ユーザーが渡す必要はありません。</param>
        /// <param name="line">この関数が呼び出されたソース ファイルの行番号です。デバッグに使用されます。このパラメーターはコンパイラーによって自動生成されるため、ユーザーが渡す必要はありません。</param>
        public static void AddCopyPass(
            this RenderGraph graph,
            TextureHandle source,
            TextureHandle destination,
            int sourceSlice,
            int destinationSlice = 0,
            int sourceMip = 0,
            int destinationMip = 0,
            string passName = "コピー パス ユーティリティ"
#if !CORE_PACKAGE_DOCTOOLS
            , [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0)
#endif
        {
            AddCopyPass(graph, source, destination, passName, false, file, line);
        }

        static void CopyRenderFunc(CopyPassData data, RasterGraphContext rgContext)
        {
            Blitter.CopyTexture(rgContext.cmd, data.isMSAA, data.force2DForXR);
        }

        /// <summary>
        /// XR テクスチャを自動検出しようとします。
        /// </summary>
        /// <param name="sourceDesc"></param>
        /// <param name="destDesc"></param>
        /// <param name="sourceSlice"></param>
        /// <param name="destinationSlice"></param>
        /// <param name="numSlices"></param>
        /// <param name="numMips"></param>
        internal static bool IsTextureXR(ref RenderTargetInfo destDesc, int sourceSlice, int destinationSlice, int numSlices, int numMips)
        {
            if (TextureXR.useTexArray &&
                  destDesc.volumeDepth > 1 &&
                  destDesc.volumeDepth == TextureXR.slices && //非XR環境は`TextureXR.slices == 1` => return false
                  sourceSlice == 0 &&
                  destinationSlice == 0 &&
                  numSlices == TextureXR.slices &&
                  numMips == 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// シンプルな blit で使用するフィルター モードです。
        /// </summary>
        public enum BlitFilterMode
        {
            /// <summary>
            /// blit 元のピクセルを選択するとき、最も近いピクセルにクランプします。
            /// </summary>
            ClampNearest,
            /// <summary>
            /// blit 元のピクセルを選択するとき、バイリニア フィルタリングを使用します。
            /// </summary>
            ClampBilinear
        }

        class BlitPassData
        {
            public TextureHandle source;
            public TextureHandle destination;
            public Vector2 scale;
            public Vector2 offset;
            public int sourceSlice;
            public int destinationSlice;
            public int numSlices;
            public int sourceMip;
            public int destinationMip;
            public int numMips;
            public BlitFilterMode filterMode;
            public bool isXR;
            public bool isDepth;
        }

        /// <summary>
        /// コピー元テクスチャの領域をコピー先テクスチャへ blit する RenderGraph パスを追加します。blit は、コピー元からコピー先へテクスチャ データを転送する高レベルな方法です。
        /// 転送されるデータに対して、スケーリングやテクスチャ フィルタリング、データ変換（例: R8Unorm から float）を行うことがあります。
        ///
        /// この関数は MSAA カラー テクスチャに対する特別な処理を持ちません。つまり、コピー元カラーをサンプリングすると resolved 値になります（MSAA RenderTexture をサンプリングするときの Unity の標準動作です）。
        /// また、コピー先が MSAA の場合、書き込まれるすべてのサンプルには同じ値が入ります（たとえば MSAA バッファへフルスクリーン quad を描画するときに期待される挙動です）。特別な MSAA
        /// 処理やカスタム resolve が必要な場合は、Material を受け取るオーバーロードを使用し、シェーダー内で適切な挙動を実装してください。
        ///
        /// depth テクスチャでは MSAA の扱いが異なります。コピー元テクスチャの bindTextureMS フラグが true の場合、MSAA 値は blit シェーダーで resolve され、非 MSAA の出力テクスチャへ単一の値を書き込みます。
        /// depth テクスチャをコピー元として使用する場合、コピー元テクスチャの bindTextureMS フラグを false にすることや、MSAA テクスチャをコピー先としてバインドすることはサポートされていません。
        ///
        /// この関数は通常のテクスチャと XR テクスチャ（状況によっては 2D 配列テクスチャ）を透過的に扱います。XR 配列テクスチャの場合、
        /// numSlices が -1 に設定されていると、テクスチャ内の各スライスに対して操作が繰り返されます。
        ///
        /// </summary>
        /// <param name="graph">このパスを追加する RenderGraph です。</param>
        /// <param name="source">データのコピー元となるテクスチャです。</param>
        /// <param name="destination">データのコピー先となるテクスチャです。</param>
        /// <param name="scale">コピー元テクスチャのサンプリングに使用するテクスチャ座標へ適用されるスケールです。</param>
        /// <param name="offset">コピー元テクスチャのサンプリングに使用するテクスチャ座標へ加算されるオフセットです。</param>
        /// <param name="sourceSlice">テクスチャが 3D または配列テクスチャの場合の、コピー元の最初のスライスです。通常のテクスチャでは 0 である必要があります。</param>
        /// <param name="destinationSlice">テクスチャが 3D または配列テクスチャの場合の、コピー先の最初のスライスです。通常のテクスチャでは 0 である必要があります。</param>
        /// <param name="numSlices">コピーするスライス数です。-1 を指定すると、テクスチャ末尾までのすべてのスライスをコピーします。無効なスライスをコピーする引数はエラーになります。</param>
        /// <param name="sourceMip">コピー元の最初の mipmap レベルです。mipmap を持たないテクスチャでは 0 である必要があります。mipmap を持つテクスチャでは有効なインデックスである必要があります。</param>
        /// <param name="destinationMip">コピー先の最初の mipmap レベルです。mipmap を持たないテクスチャでは 0 である必要があります。mipmap を持つテクスチャでは有効なインデックスである必要があります。</param>
        /// <param name="numMips">コピーする mipmap 数です。-1 を指定すると、すべての mipmap をコピーします。無効な mip をコピーする引数はエラーになります。</param>
        /// <param name="filterMode">コピー元からコピー先へ blit するときに使用するフィルタリングです。</param>
        /// <param name="passName">デバッグおよびエラー ログに使用する名前です。この名前は RenderGraph デバッガーに表示されます。</param>
        /// <param name="returnBuilder">blit パスの builder インスタンスを返すかどうかを示す bool 値です。</param>
        /// <param name="file">この関数が呼び出されたソース ファイルのファイル パスです。デバッグに使用されます。このパラメーターはコンパイラーによって自動生成されるため、ユーザーが渡す必要はありません。</param>
        /// <param name="line">この関数が呼び出されたソース ファイルの行番号です。デバッグに使用されます。このパラメーターはコンパイラーによって自動生成されるため、ユーザーが渡す必要はありません。</param>
        /// <returns>新しい Render Pass の設定に使用する IBaseRenderGraphBuilder の新しいインスタンスです。<paramref name="returnBuilder"/> が <c>true</c> の場合のみ返され、<paramref name="returnBuilder"/> が <c>false</c> の場合は <c>null</c> です。</returns>
        public static IBaseRenderGraphBuilder AddBlitPass(this RenderGraph graph,
            TextureHandle source,
            TextureHandle destination,
            Vector2 scale,
            Vector2 offset,
            int sourceSlice = 0,
            int destinationSlice = 0,
            int numSlices = -1,
            int sourceMip = 0,
            int destinationMip = 0,
            int numMips = 1,
            BlitFilterMode filterMode = BlitFilterMode.ClampBilinear,
            string passName = "Blit パス ユーティリティ",
            bool returnBuilder = false
#if !CORE_PACKAGE_DOCTOOLS
                , [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0)
#endif
        {
            if (!source.IsValid())
            {
                throw new ArgumentException($"BlitPass: {passName} の source は有効なテクスチャ ハンドルである必要があります。");
            }
            var sourceDesc = graph.GetTextureDesc(source);

            if (!destination.IsValid())
            {
                throw new ArgumentException($"BlitPass: {passName} の destination は有効なテクスチャ ハンドルである必要があります。");
            }
            var destinationDesc = graph.GetRenderTargetInfo(destination);

            //dimension を見ずに slices を最大サイズに含めています。これは Texture3D なら正しいです。実害は numMips = -1 かつ slices > width/height の 2DArray などで出る。
            int sourceMaxWidth = math.max(math.max(sourceDesc.width, sourceDesc.height), sourceDesc.slices);
            int sourceTotalMipChainLevels = (int)math.log2(sourceMaxWidth) + 1;

            int destinationMaxWidth = math.max(math.max(destinationDesc.width, destinationDesc.height), destinationDesc.volumeDepth);
            int destinationTotalMipChainLevels = (int)math.log2(destinationMaxWidth) + 1;

            if (numSlices == -1) numSlices = sourceDesc.slices - sourceSlice;
            if (numSlices > sourceDesc.slices - sourceSlice
                || numSlices > destinationDesc.volumeDepth - destinationSlice)
            {
                throw new ArgumentException($"BlitPass: {passName} は多すぎるスライスを blit しようとしています。このパスはスキップされます。");
            }
            if (numMips == -1) numMips = sourceTotalMipChainLevels - sourceMip;
            if (numMips > sourceTotalMipChainLevels - sourceMip
                || numMips > destinationTotalMipChainLevels - destinationMip)
            {
                throw new ArgumentException($"BlitPass: {passName} は多すぎる mip を blit しようとしています。このパスはスキップされます。");
            }

            bool sourceIsDepth = GraphicsFormatUtility.IsDepthFormat(sourceDesc.format);
            bool destinationIsDepth = GraphicsFormatUtility.IsDepthFormat(destinationDesc.format);
            if (!sourceIsDepth && destinationIsDepth)
                throw new ArgumentException($"BlitPass: {passName} はカラー テクスチャから depth テクスチャへ blit しようとしています。これは許可されていません。"); //多分カラーのほうが次元が多いから

            if (sourceIsDepth && !sourceDesc.bindTextureMS && sourceDesc.msaaSamples != MSAASamples.None) //基本的にデプスバッファはリゾルブしないから
                throw new ArgumentException($"BlitPass: {passName} の source depth RenderTexture は MSAA ですが、bindTextureMS フラグが true に設定されていません。これはサポートされておらず、許可されていません。");

            var canUseCopyPass = CanAddCopyPass(graph, source, destination)
                                 && scale == Vector2.one && offset == Vector2.zero && numSlices == 1 && numMips == 1;

            if (canUseCopyPass && !destinationIsDepth)
            {
                return AddCopyPass(graph, source, destination, passName, returnBuilder, file, line);
            }

            var builder = graph.AddUnsafePass<BlitPassData>(passName, out var passData, file, line);
            try
            {
                passData.isXR = IsTextureXR(ref destinationDesc, sourceSlice, destinationSlice, numSlices, numMips);
                passData.source = source;
                passData.destination = destination;
                passData.scale = scale;
                passData.offset = offset;
                passData.sourceSlice = sourceSlice;
                passData.destinationSlice = destinationSlice;
                passData.numSlices = numSlices;
                passData.sourceMip = sourceMip;
                passData.destinationMip = destinationMip;
                passData.numMips = numMips;
                passData.filterMode = filterMode;
                passData.isDepth = destinationIsDepth;
                builder.UseTexture(source, AccessFlags.Read);
                builder.UseTexture(destination, AccessFlags.Write);
                builder.SetRenderFunc(static (BlitPassData data, UnsafeGraphContext context) => BlitRenderFunc(data, context));
            }
            catch
            {
                builder.Dispose();
                throw;
            }

            if (returnBuilder)
                return builder;

            builder.Dispose();
            return null;
        }

        static Vector4 s_BlitScaleBias = new Vector4();
        static void BlitRenderFunc(BlitPassData data, UnsafeGraphContext context)
        {
            s_BlitScaleBias.x = data.scale.x;
            s_BlitScaleBias.y = data.scale.y;
            s_BlitScaleBias.z = data.offset.x;
            s_BlitScaleBias.w = data.offset.y;

            CommandBuffer unsafeCmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

            if (data.isDepth)
            {
                context.cmd.SetRenderTarget(data.destination, 0, CubemapFace.Unknown, -1);
                Blitter.BlitDepth(unsafeCmd, data.source, s_BlitScaleBias, 0);
            }
            else if (data.isXR) //非XR環境はココを通らない
            {
                // これが blit で XR を機能させる要点です。スライスに -1 を渡して render target を設定します。これにより、すべての（両目の）スライスがバインドされます。
                // その後、エンジンは描画を自動的に複製し、vertex shader と pixel shader（マクロ経由）が、それらの描画が正しい目に到達するようにします。
                context.cmd.SetRenderTarget(data.destination, 0, CubemapFace.Unknown, -1); //mipは`data.destinationMip`じゃないのか？あと、XRじゃなくてたまたま～SliceとnumSlicesが2だったらココに入ってしまうその結果1枚しかBlitされない
                Blitter.BlitTexture(unsafeCmd, data.source, s_BlitScaleBias, data.sourceMip, data.filterMode == BlitFilterMode.ClampBilinear);
            }
            else
            {
                for (int currSlice = 0; currSlice < data.numSlices; currSlice++)
                {
                    for (int currMip = 0; currMip < data.numMips; currMip++)
                    {
                        context.cmd.SetRenderTarget(data.destination, data.destinationMip + currMip, CubemapFace.Unknown, data.destinationSlice + currSlice);
                        Blitter.BlitTexture(unsafeCmd, data.source, s_BlitScaleBias, data.sourceMip + currMip, data.sourceSlice + currSlice, data.filterMode == BlitFilterMode.ClampBilinear);
                    }
                }
            }
        }

        /// <summary>
        /// blit に使用するジオメトリを選択する enum です。
        /// </summary>
        public enum FullScreenGeometryType
        {
            /// <summary>
            /// 2 つの三角形で構成された quad メッシュを描画します。メッシュのテクスチャ座標は 0-1 のテクスチャ空間を覆います。既存の Unity Graphics.Blit シェーダーがある場合に最も互換性が高い方式です。
            /// このジオメトリでは単純な vertex shader を使用できますが、メッシュと vertex buffer をパイプラインにバインドする必要があるため、CPU 側のレンダリング オーバーヘッドは大きくなります。
            /// </summary>
            Mesh,
            /// <summary>
            /// 単一の三角形がスケジュールされます。vertex shader は、3 つの頂点で全画面を覆うための正しい頂点データを生成する必要があります。
            /// vertex shader 内で頂点を取得するには "com.unity.render-pipelines.core\ShaderLibrary\Common.hlsl" を include し、GetFullScreenTriangleTexCoord/GetFullScreenTriangleVertexPosition を呼び出します。
            /// </summary>
            ProceduralTriangle,
            /// <summary>
            /// 2 つの三角形を構成する 4 つの頂点がスケジュールされます。vertex shader は、4 つの頂点で全画面を覆うための正しい頂点データを生成する必要があります。
            /// より直感的ではありますが、2 つの三角形が接する対角線に沿って quad occupancy が低くなるため、遅くなる可能性があります。
            /// vertex shader 内で頂点を取得するには "com.unity.render-pipelines.core\ShaderLibrary\Common.hlsl" を include し、GetQuadTexCoord/GetQuadVertexPosition を呼び出します。
            /// </summary>
            ProceduralQuad,
        }

        /// <summary>
        /// この struct は、Material を使う blit 関数のすべての引数を指定します。パラメーターが多く、その一部はまれにしか使われないため、struct にまとめることで
        /// 関数を使いやすくしています。
        ///
        /// 一般的な用途では、いずれかのコンストラクター オーバーロードを使用してください。
        ///
        /// デフォルトでは、ほとんどのコンストラクターが配列テクスチャのすべてのスライスをコピーします。これにより、追加の考慮なしに XR テクスチャを「自動的に」扱えます。
        ///
        /// struct またはコンストラクターで定義されたシェーダー プロパティは一般的な用途で使用されますが、シェーダー内で必ず使用する必要はありません。
        /// <c>MaterialPropertyBlock</c> を使用することで、カスタム値を持つシェーダー プロパティを追加できます。
        /// </summary>
        public struct BlitMaterialParameters
        {
            private static readonly int blitTextureProperty = Shader.PropertyToID("_BlitTexture");
            private static readonly int blitSliceProperty = Shader.PropertyToID("_BlitTexArraySlice");
            private static readonly int blitMipProperty = Shader.PropertyToID("_BlitMipLevel");
            private static readonly int blitScaleBias = Shader.PropertyToID("_BlitScaleBias");

            /// <summary>
            /// blit で最も一般的に使用されるパラメーターだけを設定するシンプルなコンストラクターです。その他のパラメーターは妥当なデフォルト値に設定されます。
            ///
            /// </summary>
            /// <param name="source">データのコピー元となるテクスチャです。</param>
            /// <param name="destination">データのコピー先となるテクスチャです。</param>
            /// <param name="material">blit に使用する Material です。</param>
            /// <param name="shaderPass">Material に使用するシェーダー パス インデックスです。</param>
            public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Material material, int shaderPass)
                : this(source, destination, Vector2.one, Vector2.zero, material, shaderPass) { }

            /// <summary>
            /// blit で最も一般的に使用されるパラメーターだけを設定するシンプルなコンストラクターです。その他のパラメーターは妥当なデフォルト値に設定されます。
            /// </summary>
            /// <param name="source">データのコピー元となるテクスチャです。</param>
            /// <param name="destination">データのコピー先となるテクスチャです。</param>
            /// <param name="scale">入力テクスチャをサンプリングするためのスケールです。</param>
            /// <param name="offset">入力テクスチャをサンプリングするためのオフセットです。bias とも呼ばれます。</param>
            /// <param name="material">blit に使用する Material です。</param>
            /// <param name="shaderPass">Material に使用するシェーダー パス インデックスです。</param>
            public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Vector2 scale, Vector2 offset, Material material, int shaderPass)
            {
                this.source = source;
                this.destination = destination;
                this.scale = scale;
                this.offset = offset;
                sourceSlice = -1;
                destinationSlice = 0;
                numSlices = -1;
                sourceMip = -1;
                destinationMip = 0;
                numMips = 1;
                this.material = material;
                this.shaderPass = shaderPass;
                propertyBlock = null;
                sourceTexturePropertyID = blitTextureProperty;
                sourceSlicePropertyID = blitSliceProperty;
                sourceMipPropertyID = blitMipProperty;
                scaleBiasPropertyID = blitScaleBias;
                geometry = FullScreenGeometryType.ProceduralTriangle;
            }

            /// <summary>
            /// コピー元/コピー先の mip と slice、およびそれらを操作するための material property と ID を設定するコンストラクターです。
            /// </summary>
            /// <param name="source">データのコピー元となるテクスチャです。</param>
            /// <param name="destination">データのコピー先となるテクスチャです。</param>
            /// <param name="material">blit に使用する Material です。</param>
            /// <param name="shaderPass">Material に使用するシェーダー パス インデックスです。</param>
            /// <param name="mpb">blit のレンダリングに使用する MaterialPropertyBlock です。このプロパティには、シェーダーが必要とするすべてのデータを含める必要があります。</param>
            /// <param name="destinationSlice">テクスチャが 3D または配列テクスチャの場合の、コピー先の最初のスライスです。通常のテクスチャでは 0 である必要があります。</param>
            /// <param name="destinationMip">コピー先の最初の mipmap レベルです。mipmap を持たないテクスチャでは 0 である必要があります。mipmap を持つテクスチャでは有効なインデックスである必要があります。</param>
            /// <param name="numSlices">コピーするスライス数です。-1 を指定すると、テクスチャ末尾までのすべてのスライスをコピーします。無効なスライスをコピーする引数はエラーになります。</param>
            /// <param name="numMips">コピーする mipmap 数です。-1 を指定すると、すべての mipmap をコピーします。無効な mip をコピーする引数はエラーになります。</param>
            /// <param name="sourceSlice">テクスチャが 3D または配列テクスチャの場合の、コピー元の最初のスライスです。通常のテクスチャでは 0 である必要があります。デフォルトは -1 で、コピー元スライスを無視し、コピー先スライスごとにループせず 0 に設定します。</param>
            /// <param name="sourceMip">コピー元の最初の mipmap レベルです。mipmap を持たないテクスチャでは 0 である必要があります。mipmap を持つテクスチャでは有効なインデックスである必要があります。デフォルトは -1 で、コピー元 mip を無視し、コピー先 mip ごとにループせず 0 に設定します。</param>
            /// <param name="geometry">コピー元テクスチャの blit に使用するジオメトリです。</param>
            /// <param name="sourceTexturePropertyID">
            /// コピー元テクスチャを設定するテクスチャ プロパティです。-1 の場合、デフォルトの "_BlitTexture" テクスチャ プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// propertyBlock が null の場合、テクスチャは Material に直接適用されます。
            /// </param>
            /// <param name="sourceSlicePropertyID">
            /// コピー元スライス インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitTexArraySlice" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数のスライスをレンダリングする場合（numSlices > 1）、各スライスに対して異なる sourceSlicePropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// </param>
            /// <param name="sourceMipPropertyID">
            /// コピー元 mip インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitMipLevel" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数の mip をレンダリングする場合（numMips > 1）、各スライスに対して異なる sourceMipPropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// </param>
            public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Material material, int shaderPass,
                MaterialPropertyBlock mpb,
                int destinationSlice,
                int destinationMip,
                int numSlices = -1,
                int numMips = 1,
                int sourceSlice = -1,
                int sourceMip = -1,
                FullScreenGeometryType geometry = FullScreenGeometryType.Mesh,
                int sourceTexturePropertyID = -1,
                int sourceSlicePropertyID = -1,
                int sourceMipPropertyID = -1)
                : this(source, destination, Vector2.one, Vector2.zero, material, shaderPass,
                      mpb,
                      destinationSlice, destinationMip,
                      numSlices, numMips,
                      sourceSlice, sourceMip,
                      geometry,
                      sourceTexturePropertyID, sourceSlicePropertyID, sourceMipPropertyID) { }

            /// <summary>
            /// コピー元/コピー先の mip と slice、およびそれらを操作するための material property と ID を設定するコンストラクターです。
            /// </summary>
            /// <param name="source">データのコピー元となるテクスチャです。</param>
            /// <param name="destination">データのコピー先となるテクスチャです。</param>
            /// <param name="scale">入力テクスチャをサンプリングするためのスケールです。</param>
            /// <param name="offset">入力テクスチャをサンプリングするためのオフセットです。bias とも呼ばれます。</param>
            /// <param name="material">blit に使用する Material です。</param>
            /// <param name="shaderPass">Material に使用するシェーダー パス インデックスです。</param>
            /// <param name="mpb">blit のレンダリングに使用する MaterialPropertyBlock です。このプロパティには、シェーダーが必要とするすべてのデータを含める必要があります。</param>
            /// <param name="destinationSlice">テクスチャが 3D または配列テクスチャの場合の、コピー先の最初のスライスです。通常のテクスチャでは 0 である必要があります。</param>
            /// <param name="destinationMip">コピー先の最初の mipmap レベルです。mipmap を持たないテクスチャでは 0 である必要があります。mipmap を持つテクスチャでは有効なインデックスである必要があります。</param>
            /// <param name="numSlices">コピーするスライス数です。-1 を指定すると、テクスチャ末尾までのすべてのスライスをコピーします。無効なスライスをコピーする引数はエラーになります。XR 配列テクスチャを使用している場合は、これを -1 または配列内のスライス数に設定してください。そうしないと XR テクスチャが正しくコピーされません。</param>
            /// <param name="numMips">コピーする mipmap 数です。-1 を指定すると、すべての mipmap をコピーします。無効な mip をコピーする引数はエラーになります。</param>
            /// <param name="sourceSlice">テクスチャが 3D または配列テクスチャの場合の、コピー元の最初のスライスです。通常のテクスチャでは 0 である必要があります。デフォルトは -1 で、コピー元スライスを無視し、コピー先スライスごとにループせず 0 に設定します。</param>
            /// <param name="sourceMip">コピー元の最初の mipmap レベルです。mipmap を持たないテクスチャでは 0 である必要があります。mipmap を持つテクスチャでは有効なインデックスである必要があります。デフォルトは -1 で、コピー元 mip を無視し、コピー先 mip ごとにループせず 0 に設定します。</param>
            /// <param name="geometry">コピー元テクスチャの blit に使用するジオメトリです。</param>
            /// <param name="sourceTexturePropertyID">
            /// コピー元テクスチャを設定するテクスチャ プロパティです。-1 の場合、デフォルトの "_BlitTexture" テクスチャ プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// propertyBlock が null の場合、テクスチャは Material に直接適用されます。
            /// </param>
            /// <param name="sourceSlicePropertyID">
            /// コピー元スライス インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitTexArraySlice" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数のスライスをレンダリングする場合（numSlices > 1）、各スライスに対して異なる sourceSlicePropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// </param>
            /// <param name="sourceMipPropertyID">
            /// コピー元 mip インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitMipLevel" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数の mip をレンダリングする場合（numMips > 1）、各スライスに対して異なる sourceMipPropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// </param>
            /// <param name="scaleBiasPropertyID">
            /// scale と bias（offset とも呼ばれます）を設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitScaleBias" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// </param>
            public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Vector2 scale, Vector2 offset, Material material, int shaderPass,
                MaterialPropertyBlock mpb,
                int destinationSlice,
                int destinationMip,
                int numSlices = -1,
                int numMips = 1,
                int sourceSlice = -1,
                int sourceMip = -1,
                FullScreenGeometryType geometry = FullScreenGeometryType.Mesh,
                int sourceTexturePropertyID = -1,
                int sourceSlicePropertyID = -1,
                int sourceMipPropertyID = -1,
                int scaleBiasPropertyID = -1) : this(source, destination, scale, offset, material, shaderPass)
            {
                this.propertyBlock = mpb;
                this.sourceSlice = sourceSlice;
                this.destinationSlice = destinationSlice;
                this.numSlices = numSlices;
                this.sourceMip = sourceMip;
                this.destinationMip = destinationMip;
                this.numMips = numMips;
                if (sourceTexturePropertyID != -1)
                    this.sourceTexturePropertyID = sourceTexturePropertyID;
                if (sourceSlicePropertyID != -1)
                    this.sourceSlicePropertyID = sourceSlicePropertyID;
                if (sourceMipPropertyID != -1)
                    this.sourceMipPropertyID = sourceMipPropertyID;
                if (scaleBiasPropertyID != -1)
                    this.scaleBiasPropertyID = scaleBiasPropertyID;
                this.geometry = geometry;
            }

            /// <summary>
            /// テクスチャ、Material、シェーダー パス、MaterialPropertyBlock を設定するコンストラクターです。
            /// </summary>
            /// <param name="source">データのコピー元となるテクスチャです。</param>
            /// <param name="destination">データのコピー先となるテクスチャです。</param>
            /// <param name="material">blit に使用する Material です。</param>
            /// <param name="shaderPass">Material に使用するシェーダー パス インデックスです。</param>
            /// <param name="mpb">blit のレンダリングに使用する MaterialPropertyBlock です。このプロパティには、シェーダーが必要とするすべてのデータを含める必要があります。</param>
            /// <param name="geometry">コピー元テクスチャの blit に使用するジオメトリです。</param>
            /// <param name="sourceTexturePropertyID">
            /// コピー元テクスチャを設定するテクスチャ プロパティです。-1 の場合、デフォルトの "_BlitTexture" テクスチャ プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// propertyBlock が null の場合、テクスチャは Material に直接適用されます。
            /// </param>
            /// <param name="sourceSlicePropertyID">
            /// コピー元スライス インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitTexArraySlice" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数のスライスをレンダリングする場合（numSlices > 1）、各スライスに対して異なる sourceSlicePropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// </param>
            /// <param name="sourceMipPropertyID">
            /// コピー元 mip インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitMipLevel" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数の mip をレンダリングする場合（numMips > 1）、各スライスに対して異なる sourceMipPropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// </param>
            public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Material material, int shaderPass,
                MaterialPropertyBlock mpb,
                FullScreenGeometryType geometry = FullScreenGeometryType.Mesh,
                int sourceTexturePropertyID = -1,
                int sourceSlicePropertyID = -1,
                int sourceMipPropertyID = -1)
                : this(source, destination,
                      Vector2.one, Vector2.zero,
                      material, shaderPass,
                      mpb, geometry,
                      sourceTexturePropertyID, sourceSlicePropertyID, sourceMipPropertyID) { }

            /// <summary>
            /// テクスチャ、Material、シェーダー パス、MaterialPropertyBlock を設定するコンストラクターです。
            /// </summary>
            /// <param name="source">データのコピー元となるテクスチャです。</param>
            /// <param name="destination">データのコピー先となるテクスチャです。</param>
            /// <param name="scale">入力テクスチャをサンプリングするためのスケールです。</param>
            /// <param name="offset">入力テクスチャをサンプリングするためのオフセットです。bias とも呼ばれます。</param>
            /// <param name="material">blit に使用する Material です。</param>
            /// <param name="shaderPass">Material に使用するシェーダー パス インデックスです。</param>
            /// <param name="mpb">blit のレンダリングに使用する MaterialPropertyBlock です。このプロパティには、シェーダーが必要とするすべてのデータを含める必要があります。</param>
            /// <param name="geometry">コピー元テクスチャの blit に使用するジオメトリです。</param>
            /// <param name="sourceTexturePropertyID">
            /// コピー元テクスチャを設定するテクスチャ プロパティです。-1 の場合、デフォルトの "_BlitTexture" テクスチャ プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// propertyBlock が null の場合、テクスチャは Material に直接適用されます。
            /// </param>
            /// <param name="sourceSlicePropertyID">
            /// コピー元スライス インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitSlice" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数のスライスをレンダリングする場合（numSlices > 1）、各スライスに対して異なる sourceSlicePropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// </param>
            /// <param name="sourceMipPropertyID">
            /// コピー元 mip インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitMipLevel" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数の mip をレンダリングする場合（numMips > 1）、各スライスに対して異なる sourceMipPropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// </param>
            /// <param name="scaleBiasPropertyID">
            /// scale と bias（offset とも呼ばれます）を設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitScaleBias" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// </param>
            public BlitMaterialParameters(TextureHandle source, TextureHandle destination, Vector2 scale, Vector2 offset, Material material, int shaderPass,
                MaterialPropertyBlock mpb,
                FullScreenGeometryType geometry = FullScreenGeometryType.Mesh,
                int sourceTexturePropertyID = -1,
                int sourceSlicePropertyID = -1,
                int sourceMipPropertyID = -1,
                int scaleBiasPropertyID = -1) : this(source, destination, scale, offset, material, shaderPass)
            {
                this.propertyBlock = mpb;
                if (sourceTexturePropertyID != -1)
                    this.sourceTexturePropertyID = sourceTexturePropertyID;
                if (sourceSlicePropertyID != -1)
                    this.sourceSlicePropertyID = sourceSlicePropertyID;
                if (sourceMipPropertyID != -1)
                    this.sourceMipPropertyID = sourceMipPropertyID;
                if (scaleBiasPropertyID != -1)
                    this.scaleBiasPropertyID = scaleBiasPropertyID;
                this.geometry = geometry;
            }

            /// <summary>
            /// コピー元テクスチャです。このテクスチャは、指定された MaterialPropertyBlock のプロパティに設定されます。
            /// その名前は sourceTexturePropertyID で指定されます。property block が null の場合、一時的な property block が
            /// blit 関数によって割り当てられます。
            /// </summary>
            public TextureHandle source;

            /// <summary>
            /// blit 先のテクスチャです。このテクスチャのサブリソース（mip、slice）は、コピー先引数に基づいて render attachment として設定されます。
            /// </summary>
            public TextureHandle destination;

            /// <summary>
            /// blit 操作で使用されるスケールです。
            /// </summary>
            public Vector2 scale;

            /// <summary>
            /// blit 先のオフセットです。
            /// </summary>
            public Vector2 offset;

            /// <summary>
            /// blit 元となるコピー元テクスチャの最初のスライスです。-1 の場合、コピー元スライスを無視します。この場合、sourceSlicePropertyID テクスチャ パラメーターには値を設定しません。
            /// -1 でない場合、blit される各スライスについて、sourceSlice から sourceSlice+numSlices の範囲で sourceSlicePropertyID が設定されます。
            /// </summary>
            public int sourceSlice;

            /// <summary>
            /// blit 先となるコピー先テクスチャの最初のスライスです。
            /// </summary>
            public int destinationSlice;

            /// <summary>
            /// blit するスライス数です。-1 の場合、destinationSlice からテクスチャ末尾までのすべてのスライスを blit します。無効なスライス（範囲外や 0 など）をコピーする引数はエラーになります。
            /// </summary>
            public int numSlices;

            /// <summary>
            /// blit 元となる最初のコピー元 mipmap です。-1 の場合、コピー元 mip を無視します。この場合、sourceMipPropertyID テクスチャ パラメーターには値を設定しません。
            /// -1 でない場合、blit される各 mip について、sourceMip から sourceMip+numMips の範囲で sourceMipPropertyID が設定されます。
            /// </summary>
            public int sourceMip;

            /// <summary>
            /// blit 先となる最初のコピー先 mipmap です。
            /// </summary>
            public int destinationMip;

            /// <summary>
            /// blit する mipmap 数です。-1 の場合、destinationMip からテクスチャ末尾までのすべての mipmap を blit します。無効なスライス（範囲外や 0 など）をコピーする引数はエラーになります。
            /// </summary>
            public int numMips;

            /// <summary>
            /// 使用する Material です。null にはできません。blit 関数はこの Material を一切変更しません。
            /// </summary>
            public Material material;

            /// <summary>
            /// 使用するシェーダー パス インデックスです。
            /// </summary>
            public int shaderPass;

            /// <summary>
            /// 使用する MaterialPropertyBlock です。null も指定できます。blit 関数は blit の一部として、この MaterialPropertyBlock の sourceTexturePropertyID、sourceSliceProperty、sourceMipPropertyID を変更します。
            /// BlitMaterialParameters で使用する propertyBlock の SetTexture(...) 関数を呼び出すことは避けてください。RenderGraph 使用時に未追跡のテクスチャが発生し、予期しない挙動の原因になる可能性があります。
            /// </summary>
            public MaterialPropertyBlock propertyBlock;

            /// <summary>
            /// コピー元テクスチャを設定するテクスチャ プロパティです。-1 の場合、デフォルトの "_BlitTexture" テクスチャ プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// propertyBlock が null の場合、テクスチャは Material に直接適用されます。
            /// </summary>
            public int sourceTexturePropertyID;

            /// <summary>
            /// コピー元スライス インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitTexArraySlice" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数のスライスをレンダリングする場合（numSlices > 1）、各スライスに対して異なる sourceSlicePropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// sourceSlice が -1 の場合、このプロパティには値が設定されません。
            /// </summary>
            public int sourceSlicePropertyID;

            /// <summary>
            /// コピー元 mip インデックスを設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitMipLevel" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// blit 関数で複数の mip をレンダリングする場合（numMips > 1）、各スライスに対して異なる sourceMipPropertyID 値を設定した複数のフルスクリーン quad がレンダリングされます。
            /// sourceMip が -1 の場合、このプロパティには値が設定されません。
            /// </summary>
            public int sourceMipPropertyID;

            /// <summary>
            /// コピー元からコピー先への scale と bias（offset とも呼ばれます）を設定する scalar プロパティです。-1 の場合、デフォルトの "_BlitScaleBias" プロパティが使用されます。注: 文字列のプロパティ名を ID に変換するには Shader.PropertyToID を使用します。
            /// </summary>
            public int scaleBiasPropertyID;

            /// <summary>
            /// blit 用 Material をレンダリングするときに使用するフルスクリーン ジオメトリの種類です。詳細は FullScreenGeometryType を参照してください。
            /// </summary>
            public FullScreenGeometryType geometry;
        }

        class BlitMaterialPassData
        {
            public int sourceTexturePropertyID;
            public TextureHandle source;
            public TextureHandle destination;
            public Vector2 scale;
            public Vector2 offset;
            public Material material;
            public int shaderPass;
            public MaterialPropertyBlock propertyBlock;
            public int sourceSlice;
            public int destinationSlice;
            public int numSlices;
            public int sourceMip;
            public int destinationMip;
            public int numMips;
            public FullScreenGeometryType geometry;
            public int sourceSlicePropertyID;
            public int sourceMipPropertyID;
            public int scaleBiasPropertyID;
            public bool isXR;
        }

        /// <summary>
        /// コピー元テクスチャの領域をコピー先テクスチャへ blit する RenderGraph パスを追加し、要求された場合は builder を返します。
        /// blit は、コピー元からコピー先へテクスチャ データを転送する高レベルな方法です。
        /// このオーバーロードでは、任意の Material によってデータを変換できます。
        ///
        /// numSlices が -1 に設定され、slice プロパティが正しく機能している場合、この関数は通常のテクスチャと XR テクスチャ（状況によっては 2D 配列テクスチャ）を透過的に扱います。
        ///
        /// これは単一の blit を呼び出すシンプルなパスをスケジュールするためのヘルパー関数です。複数の blit を連続して呼び出したい場合（例: slice ごと、mip ごとの blit）は、一般的には
        /// 単一のパスをスケジュールし、その中で command buffer に直接 blit をスケジュールする方が高速です。
        ///
        /// この関数は、RenderGraph の実行タイムライン上で実行されるパスをスケジュールします。渡された Material と MaterialPropertyBlock がこの挙動を正しく考慮していることを確認することが重要です。
        /// 特に、次のコードは意図した通りに動作しない可能性があります。
        /// material.SetFloat("Visibility", 0.5);
        /// renderGraph.AddBlitPass(... material ...);
        /// material.SetFloat("Visibility", 0.8);
        /// renderGraph.AddBlitPass(... material ...);
        ///
        /// この場合、graph が実行される時点では Material の "Visibility" 値が 0.8 に設定されているため、両方のパスがその float 値を使用します。このようなケースを正しく扱うには、2 つの別々の
        /// Material を使うか、2 つの別々の MaterialPropertyBlock を使います。例:
        ///
        /// propertyBlock1.SetFloat("Visibility", 0.5);
        /// renderGraph.AddBlitPass(... material, propertyBlock1, ...);
        /// propertyBlock2.SetFloat("Visibility", 0.8);
        /// renderGraph.AddBlitPass(... material, propertyBlock2, ...);
        ///
        /// この関数を使用する際の注意:
        /// - MSAA バッファの特別な処理が必要な場合は、コピー元テクスチャの bindMS フラグと、コピー先テクスチャでのサンプルごとの pixel shader 呼び出し（例: SV_SampleIndex を使用）で実装できます。
        /// - この関数で使用する MaterialPropertyBlock には、MaterialPropertyBlock.SetTexture(...) で追加されたテクスチャを含めないでください。RenderGraph 使用時に未追跡のテクスチャが発生し、意図しない挙動の原因になります。
        ///
        /// </summary>
        /// <param name="graph">このパスを追加する RenderGraph です。</param>
        /// <param name="blitParameters">レンダリングに使用するパラメーターです。</param>
		/// <param name="passName">デバッグおよびエラー ログに使用する名前です。この名前は RenderGraph デバッガーに表示されます。</param>
        /// <param name="returnBuilder">blit パスの builder インスタンスを返すかどうかを示す bool 値です。</param>
        /// <param name="file">この関数が呼び出されたソース ファイルのファイル パスです。デバッグに使用されます。このパラメーターはコンパイラーによって自動生成されるため、ユーザーが渡す必要はありません。</param>
        /// <param name="line">この関数が呼び出されたソース ファイルの行番号です。デバッグに使用されます。このパラメーターはコンパイラーによって自動生成されるため、ユーザーが渡す必要はありません。</param>
        /// <returns>新しい Render Pass の設定に使用する IBaseRenderGraphBuilder の新しいインスタンスです。<paramref name="returnBuilder"/> が <c>true</c> の場合のみ返され、<paramref name="returnBuilder"/> が <c>false</c> の場合は <c>null</c> です。</returns>
        public static IBaseRenderGraphBuilder AddBlitPass(this RenderGraph graph,
            BlitMaterialParameters blitParameters,
            string passName = "Material 付き Blit パス ユーティリティ",
            bool returnBuilder = false
#if !CORE_PACKAGE_DOCTOOLS
                , [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0)
#endif
        {
            if (!blitParameters.destination.IsValid())
            {
                throw new ArgumentException($"BlitPass: {passName} の destination は有効なテクスチャ ハンドルである必要があります。");
            }

            var destinationDesc = graph.GetRenderTargetInfo(blitParameters.destination);

            // 未指定のパラメーターをテクスチャ デスクリプターに基づいて自動的に埋めます。
            int destinationMaxWidth = math.max(math.max(destinationDesc.width, destinationDesc.height), destinationDesc.volumeDepth);
            int destinationTotalMipChainLevels = (int)math.log2(destinationMaxWidth) + 1;
            if (blitParameters.numSlices == -1)
            {
                blitParameters.numSlices = destinationDesc.volumeDepth - blitParameters.destinationSlice; //`source`が無い場合もあるから`destination`になっていると思われる
            }

            if (blitParameters.numMips == -1)
            {
                blitParameters.numMips = destinationTotalMipChainLevels - blitParameters.destinationMip;
            }

            // 利用可能な場合はコピー元に対して検証します。
            if (blitParameters.source.IsValid())
            {
                var sourceDesc = graph.GetTextureDesc(blitParameters.source);
                int sourceMaxWidth = math.max(math.max(sourceDesc.width, sourceDesc.height), sourceDesc.slices);
                int sourceTotalMipChainLevels = (int)math.log2(sourceMaxWidth) + 1;

                if (blitParameters.sourceSlice != -1 && blitParameters.numSlices > sourceDesc.slices - blitParameters.sourceSlice)
                {
                    throw new ArgumentException($"BlitPass: {passName} は多すぎるスライスを blit しようとしています。コピー元配列に十分なスライスがありません。このパスはスキップされます。");
                }

                if (blitParameters.sourceMip != -1 && blitParameters.numMips > sourceTotalMipChainLevels - blitParameters.sourceMip)
                {
                    throw new ArgumentException($"BlitPass: {passName} は多すぎる mip を blit しようとしています。コピー元テクスチャに十分な mip がありません。このパスはスキップされます。");
                }
            }

            // コピー先に対して検証します。
            if (blitParameters.numSlices > destinationDesc.volumeDepth - blitParameters.destinationSlice)
            {
                throw new ArgumentException($"BlitPass: {passName} は多すぎるスライスを blit しようとしています。コピー先配列に十分なスライスがありません。このパスはスキップされます。");
            }

            if (blitParameters.numMips > destinationTotalMipChainLevels - blitParameters.destinationMip)
            {
                throw new ArgumentException($"BlitPass: {passName} は多すぎる mip を blit しようとしています。コピー先テクスチャに十分な mip がありません。このパスはスキップされます。");
            }

            if (blitParameters.material == null)
            {
                throw new ArgumentException($"BlitPass: {passName} は null の Material を使用しようとしています。");
            }

            var builder = graph.AddUnsafePass<BlitMaterialPassData>(passName, out var passData, file, line);
            try
            {
                passData.sourceTexturePropertyID = blitParameters.sourceTexturePropertyID;
                passData.source = blitParameters.source;
                passData.destination = blitParameters.destination;
                passData.scale = blitParameters.scale;
                passData.offset = blitParameters.offset;
                passData.material = blitParameters.material;
                passData.shaderPass = blitParameters.shaderPass;
                passData.propertyBlock = blitParameters.propertyBlock;
                passData.sourceSlice = blitParameters.sourceSlice;
                passData.destinationSlice = blitParameters.destinationSlice;
                passData.numSlices = blitParameters.numSlices;
                passData.sourceMip = blitParameters.sourceMip;
                passData.destinationMip = blitParameters.destinationMip;
                passData.numMips = blitParameters.numMips;
                passData.geometry = blitParameters.geometry;
                passData.sourceSlicePropertyID = blitParameters.sourceSlicePropertyID;
                passData.sourceMipPropertyID = blitParameters.sourceMipPropertyID;
                passData.scaleBiasPropertyID = blitParameters.scaleBiasPropertyID;

                passData.isXR = IsTextureXR(ref destinationDesc, (passData.sourceSlice == -1) ? 0 : passData.sourceSlice, passData.destinationSlice, passData.numSlices, passData.numMips);
                if (blitParameters.source.IsValid())
                {
                    builder.UseTexture(blitParameters.source);
                }
                builder.UseTexture(blitParameters.destination, AccessFlags.Write);
                builder.SetRenderFunc(static (BlitMaterialPassData data, UnsafeGraphContext context) => BlitMaterialRenderFunc(data, context));
            }
            catch
            {
                builder.Dispose();
                throw;
            }

            if (returnBuilder)
                return builder;

            builder.Dispose();
            return null;
        }

        static void BlitMaterialRenderFunc(BlitMaterialPassData data, UnsafeGraphContext context)
        {
            s_BlitScaleBias.x = data.scale.x;
            s_BlitScaleBias.y = data.scale.y;
            s_BlitScaleBias.z = data.offset.x;
            s_BlitScaleBias.w = data.offset.y;

            CommandBuffer unsafeCmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd); //生cmdを取得

            if (data.propertyBlock == null) data.propertyBlock = s_PropertyBlock;

            if (data.source.IsValid())
            {
                data.propertyBlock.SetTexture(data.sourceTexturePropertyID, data.source);
            }

            data.propertyBlock.SetVector(data.scaleBiasPropertyID, s_BlitScaleBias);

            if (data.isXR)
            {
                // これが blit で XR を機能させる要点です。スライスに -1 を渡して render target を設定します。これにより、すべての（両目の）スライスがバインドされます。
                // その後、エンジンは描画を自動的に複製し、vertex shader と pixel shader（マクロ経由）が、それらの描画が正しい目に到達するようにします。

                if (data.sourceSlice != -1)
                    data.propertyBlock.SetInt(data.sourceSlicePropertyID, 0);
                if (data.sourceMip != -1)
                    data.propertyBlock.SetInt(data.sourceMipPropertyID, data.sourceMip);

                context.cmd.SetRenderTarget(data.destination, 0, CubemapFace.Unknown, -1);
                switch (data.geometry)
                {
                    case FullScreenGeometryType.Mesh:
                        Blitter.DrawQuadMesh(unsafeCmd, data.material, data.shaderPass, data.propertyBlock);
                        break;
                    case FullScreenGeometryType.ProceduralQuad:
                        Blitter.DrawQuad(unsafeCmd, data.material, data.shaderPass, data.propertyBlock);
                        break;
                    case FullScreenGeometryType.ProceduralTriangle:
                        Blitter.DrawTriangle(unsafeCmd, data.material, data.shaderPass, data.propertyBlock);
                        break;
                }
            }
            else
            {
                for (int currSlice = 0; currSlice < data.numSlices; currSlice++)
                {
                    for (int currMip = 0; currMip < data.numMips; currMip++)
                    {
                        if (data.sourceSlice != -1)
                            data.propertyBlock.SetInt(data.sourceSlicePropertyID, data.sourceSlice + currSlice);
                        if (data.sourceMip != -1)
                            data.propertyBlock.SetInt(data.sourceMipPropertyID, data.sourceMip + currMip);

                        context.cmd.SetRenderTarget(data.destination, data.destinationMip + currMip, CubemapFace.Unknown, data.destinationSlice + currSlice);
                        switch (data.geometry)
                        {
                            case FullScreenGeometryType.Mesh:
                                Blitter.DrawQuadMesh(unsafeCmd, data.material, data.shaderPass, data.propertyBlock);
                                break;
                            case FullScreenGeometryType.ProceduralQuad:
                                Blitter.DrawQuad(unsafeCmd, data.material, data.shaderPass, data.propertyBlock);
                                break;
                            case FullScreenGeometryType.ProceduralTriangle:
                                Blitter.DrawTriangle(unsafeCmd, data.material, data.shaderPass, data.propertyBlock);
                                break;
                        }
                    }
                }
            }
        }
    }
}
