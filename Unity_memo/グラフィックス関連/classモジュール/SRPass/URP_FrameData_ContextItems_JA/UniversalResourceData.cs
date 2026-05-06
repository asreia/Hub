using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Textureリソースに関連する設定を保持するクラスです。
    /// </summary>
    public class UniversalResourceData : UniversalResourceDataBase
    {
        /// <summary>
        /// アクティブなColor Target ID。
        /// </summary>
        // [設定元] CreateIntermediateCameraColorAttachment で Camera に設定し、SwitchActiveTexturesToBackbuffer で BackBuffer に切り替え。
        internal ActiveID activeColorID { get; set; }

        /// <summary>
        /// 現在のアクティブなColor Target Textureを返します。PassのRender関数内ではなく、RenderGraph Passの記録時に参照してください。
        /// </summary>
        /// <value>フロント/バックバッファ間のアクティブなColor Textureを返します。</value>
        // [設定元] 代入ではなく activeColorID に応じて cameraColor または backBufferColor を返す。
        public TextureHandle activeColorTexture
        {
            get
            {
                if (!CheckAndWarnAboutAccessibility())
                    return TextureHandle.nullHandle;

                switch (activeColorID)
                {
                    case ActiveID.Camera:
                        return cameraColor;
                    case ActiveID.BackBuffer:
                        return backBufferColor;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// アクティブなColor TextureとDepth TextureをBackbufferへ切り替えます。切り替え後、isActiveTargetBackBuffer は true を返します。
        /// その後、activeColorTexture と activeDepthTexture はBackbufferを返します。これはRender Passの後で呼び出す必要があります
        /// cameraColor をBackbufferへBlit/CopyするPassがRenderGraphに記録された後です。以降のPassは
        /// アクティブなColorおよびDepthとしてBackbufferを自動的に使用します。このメソッドが前もって呼び出された場合、URPはFinal Blit Passを追加しません
        /// そのRender Passより前に呼び出された場合です。
        /// </summary>
        public void SwitchActiveTexturesToBackbuffer()
        {
            activeColorID = UniversalResourceData.ActiveID.BackBuffer;
            activeDepthID = UniversalResourceData.ActiveID.BackBuffer;
        }

        /// <summary>
        /// アクティブなDepth Target ID。
        /// </summary>
        // [設定元] CreateIntermediateCameraDepthAttachment で Camera に設定し、SwitchActiveTexturesToBackbuffer で BackBuffer に切り替え。
        internal ActiveID activeDepthID { get; set; }

        /// <summary>
        /// 現在のアクティブなDepth Target Textureを返します。PassのRender関数内ではなく、RenderGraph Passの記録時に参照してください。
        /// </summary>
        /// <value>TextureHandle</value>
        // [設定元] 代入ではなく activeDepthID に応じて cameraDepth または backBufferDepth を返す。
        public TextureHandle activeDepthTexture
        {
            get
            {
                if (!CheckAndWarnAboutAccessibility())
                    return TextureHandle.nullHandle;

                switch (activeDepthID)
                {
                    case ActiveID.Camera:
                        return cameraDepth;
                    case ActiveID.BackBuffer:
                        return backBufferDepth;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 現在のアクティブターゲットがBackbufferの場合は true。PassのRender関数内ではなく、RenderGraph Passの記録時に参照してください。
        /// </summary>
        /// <value>現在Backbufferが使用中の場合は true、それ以外の場合は false を返します。</value>
        // [設定元] 代入ではなく activeColorID または activeDepthID が BackBuffer かどうかから算出。
        public bool isActiveTargetBackBuffer
        {
            get
            {
                if (!isAccessible)
                {
                    Debug.LogError("現在のフレームセットアップ外から frameData にアクセスしようとしています。");
                    return false;
                }

                return activeColorID == UniversalResourceData.ActiveID.BackBuffer;
            }
        }


        /// <summary>
        /// 画面へ直接レンダリングするために使用されるBackbuffer Color。フレーム設定によっては、すべてのPassが書き込めます。
        /// </summary>
        // [設定元] UniversalRenderer.ImportBackBuffers で Camera の back buffer color を import して設定。
        public TextureHandle backBufferColor
        {
            get => CheckAndGetTextureHandle(ref _backBufferColor);
            internal set => CheckAndSetTextureHandle(ref _backBufferColor, value);
        }
        private TextureHandle _backBufferColor;


        /// <summary>
        /// 画面へ直接レンダリングするために使用されるBackbuffer Depth。フレーム設定によっては、すべてのPassが書き込めます。
        /// </summary>
        // [設定元] UniversalRenderer.ImportBackBuffers で Camera の back buffer depth を import して設定。
        public TextureHandle backBufferDepth
        {
            get => CheckAndGetTextureHandle(ref _backBufferDepth);
            internal set => CheckAndSetTextureHandle(ref _backBufferDepth, value);
        }
        private TextureHandle _backBufferDepth;

        // 中間カメラターゲット

        /// <summary>
        /// メインのオフスクリーンCamera Color Target。フレーム設定によっては、すべてのPassが書き込めます。
        /// MSAAが有効な場合、複数サンプルを保持できます。
        /// </summary>
        // [設定元] UniversalRenderer.CreateIntermediateCameraColorAttachment で作成または import して設定。
        public TextureHandle cameraColor
        {
            get => CheckAndGetTextureHandle(ref _cameraColor);
            set => CheckAndSetTextureHandle(ref _cameraColor, value);
        }
        private TextureHandle _cameraColor;

        /// <summary>
        /// メインのオフスクリーンCamera Depth Target。フレーム設定によっては、すべてのPassが書き込めます。
        /// MSAAが有効な場合、複数サンプルを保持できます。
        /// </summary>
        // [設定元] UniversalRenderer.CreateIntermediateCameraDepthAttachment で作成または import して設定。
        public TextureHandle cameraDepth
        {
            get => CheckAndGetTextureHandle(ref _cameraDepth);
            set => CheckAndSetTextureHandle(ref _cameraDepth, value);
        }
        private TextureHandle _cameraDepth;

        // 影

        /// <summary>
        /// メインのShadow Map。
        /// </summary>
        // [設定元] UniversalRenderer.OnBeforeRendering/OnOffscreenDepthTextureRendering が MainLightShadowCasterPass.Render の戻り値を設定。
        public TextureHandle mainShadowsTexture
        {
            get => CheckAndGetTextureHandle(ref _mainShadowsTexture);
            set => CheckAndSetTextureHandle(ref _mainShadowsTexture, value);
        }
        private TextureHandle _mainShadowsTexture;

        /// <summary>
        /// 追加ライト用のShadow Map。
        /// </summary>
        // [設定元] UniversalRenderer.OnBeforeRendering/OnOffscreenDepthTextureRendering が AdditionalLightsShadowCasterPass.Render の戻り値を設定。
        public TextureHandle additionalShadowsTexture
        {
            get => CheckAndGetTextureHandle(ref _additionalShadowsTexture);
            set => CheckAndSetTextureHandle(ref _additionalShadowsTexture, value);
        }
        private TextureHandle _additionalShadowsTexture;

        // GBufferターゲット

        /// <summary>
        /// GBuffer。GBuffer Passによって書き込まれます。
        /// </summary>
        // [設定元] UniversalRenderer.OnMainRendering で DeferredLights.CreateGbufferResourcesRenderGraph の結果を GbufferTextureHandles から設定。
        public TextureHandle[] gBuffer
        {
            get => CheckAndGetTextureHandle(ref _gBuffer);
            set => CheckAndSetTextureHandle(ref _gBuffer, value);
        }
        private TextureHandle[] _gBuffer = new TextureHandle[RenderGraphUtils.GBufferSize];

        // カメラの不透明/深度/法線

        /// <summary>
        /// Camera Opaque Texture。CopyColor Passが実行された場合、CameraColorのコピーを含みます。
        /// </summary>
        // [設定元] UniversalRenderer.OnMainRendering で CopyColorPass.Render の出力を設定。
        public TextureHandle cameraOpaqueTexture
        {
            get => CheckAndGetTextureHandle(ref _cameraOpaqueTexture);
            internal set => CheckAndSetTextureHandle(ref _cameraOpaqueTexture, value);
        }
        private TextureHandle _cameraOpaqueTexture;

        /// <summary>
        /// Camera Depth Texture。CopyDepthまたはDepth Prepassが実行された場合、シーン深度を含みます。
        /// </summary>
        // [設定元] UniversalRenderer.CreateCameraDepthCopyTexture で作成し、CopyDepth/DepthPrepass 系パスが書き込む。
        public TextureHandle cameraDepthTexture
        {
            get => CheckAndGetTextureHandle(ref _cameraDepthTexture);
            internal set => CheckAndSetTextureHandle(ref _cameraDepthTexture, value);
        }
        private TextureHandle _cameraDepthTexture;

        /// <summary>
        /// Camera Normals Texture。DepthNormals Prepassが実行された場合、シーン法線を含みます。
        /// </summary>
        // [設定元] UniversalRenderer.CreateCameraNormalsTexture で作成し、DepthNormalsPrepass/GBuffer 系パスが書き込む。
        public TextureHandle cameraNormalsTexture
        {
            get => CheckAndGetTextureHandle(ref _cameraNormalsTexture);
            internal set => CheckAndSetTextureHandle(ref _cameraNormalsTexture, value);
        }
        private TextureHandle _cameraNormalsTexture;

        // モーションベクトル

        /// <summary>
        /// Motion Vector Color。Motion Vector Passによって書き込まれます。
        /// </summary>
        // [設定元] UniversalRenderer.CreateMotionVectorTextures で Motion Vector color texture を作成して設定。
        public TextureHandle motionVectorColor
        {
            get => CheckAndGetTextureHandle(ref _motionVectorColor);
            set => CheckAndSetTextureHandle(ref _motionVectorColor, value);
        }
        private TextureHandle _motionVectorColor;

        /// <summary>
        /// Motion Vector Depth。Motion Vector Passによって書き込まれます。
        /// </summary>
        // [設定元] UniversalRenderer.CreateMotionVectorTextures で Motion Vector depth texture を作成して設定。
        public TextureHandle motionVectorDepth
        {
            get => CheckAndGetTextureHandle(ref _motionVectorDepth);
            set => CheckAndSetTextureHandle(ref _motionVectorDepth, value);
        }
        private TextureHandle _motionVectorDepth;

        // PostFX

        /// <summary>
        /// Internal Color LUT。InternalLUT Passによって書き込まれます。
        /// </summary>
        // [設定元] ColorGradingLutPass.Render で生成した LUT texture を設定。
        public TextureHandle internalColorLut
        {
            get => CheckAndGetTextureHandle(ref _internalColorLut);
            set => CheckAndSetTextureHandle(ref _internalColorLut, value);
        }
        private TextureHandle _internalColorLut;

        /// <summary>
        /// Bloom。非常に明るいハイライトの輝きです。Bloom Passによって書き込まれ、Uber Passで加算合成されます。
        /// Bloom固有のアルファ情報は含まず、高度な合成ではPremultiplied Alpha Textureと見なせます。
        /// </summary>
        // [設定元] BloomPostProcessPass.Render で選択された Bloom 実装の出力を設定。
        internal TextureHandle bloom
        {
            get => CheckAndGetTextureHandle(ref _bloom);
            set => CheckAndSetTextureHandle(ref _bloom, value);
        }
        private TextureHandle _bloom;

        /// <summary>
        /// After Post Process Colorは廃止されています。
        /// </summary>
        [Obsolete("AfterPostProcessColor は実装されたことがありません。代わりに cameraColor を使用してください。", false)]
        // [設定元] Obsolete/未実装のため通常は設定されず、Reset で nullHandle に戻る。
        public TextureHandle afterPostProcessColor
        {
            get => CheckAndGetTextureHandle(ref _afterPostProcessColor);
            internal set => CheckAndSetTextureHandle(ref _afterPostProcessColor, value);
        }
        private TextureHandle _afterPostProcessColor;

        /// <summary>
        /// Overlay UI Texture。オフスクリーンレンダリング時にDrawScreenSpaceUI PassがこのTextureへ書き込みます。
        /// </summary>
        // [設定元] UniversalRenderer.CreateOffscreenUITexture で Overlay UI 用 texture を import して設定。
        public TextureHandle overlayUITexture
        {
            get => CheckAndGetTextureHandle(ref _overlayUITexture);
            internal set => CheckAndSetTextureHandle(ref _overlayUITexture, value);
        }
        private TextureHandle _overlayUITexture;

        // レンダリングレイヤー

        /// <summary>
        /// Rendering Layers Texture。設定に応じてDrawOpaques PassまたはDepthNormals Prepassによって書き込まれます。
        /// </summary>
        // [設定元] UniversalRenderer.CreateRenderingLayersTexture で作成し、DepthNormals/GBuffer/Rendering Layers 系パスが書き込む。
        public TextureHandle renderingLayersTexture
        {
            get => CheckAndGetTextureHandle(ref _renderingLayersTexture);
            internal set => CheckAndSetTextureHandle(ref _renderingLayersTexture, value);
        }
        private TextureHandle _renderingLayersTexture;

        // デカール

        /// <summary>
        /// DBuffer。Decals Passによって書き込まれます。
        /// </summary>
        // [設定元] DBufferRenderPass.Render で Decal 用 DBuffer 配列を設定。
        public TextureHandle[] dBuffer
        {
            get => CheckAndGetTextureHandle(ref _dBuffer);
            set => CheckAndSetTextureHandle(ref _dBuffer, value);
        }
        private TextureHandle[] _dBuffer = new TextureHandle[RenderGraphUtils.DBufferSize];

        /// <summary>
        /// DBufferDepth。Decals Passによって書き込まれます。
        /// </summary>
        // [設定元] DBufferDepthCopyPass.Render で DBuffer 用 depth copy を作成して設定。
        public TextureHandle dBufferDepth
        {
            get => CheckAndGetTextureHandle(ref _dBufferDepth);
            set => CheckAndSetTextureHandle(ref _dBufferDepth, value);
        }
        private TextureHandle _dBufferDepth;

        /// <summary>
        /// Screen Space Ambient Occlusion Texture。SSAO Passによって書き込まれます。
        /// </summary>
        // [設定元] ScreenSpaceAmbientOcclusionPass.Render で SSAO の最終 texture を設定。
        public TextureHandle ssaoTexture
        {
            get => CheckAndGetTextureHandle(ref _ssaoTexture);
            internal set => CheckAndSetTextureHandle(ref _ssaoTexture, value);
        }
        private TextureHandle _ssaoTexture;

        /// <summary>
        /// Screen Space Irradiance Texture。
        /// </summary>
        // [設定元] SurfaceCacheGIRendererFeature などの条件付き RendererFeature が Screen Space Irradiance texture を設定。
        internal TextureHandle irradianceTexture
        {
            get => CheckAndGetTextureHandle(ref _irradianceTexture);
            set => CheckAndSetTextureHandle(ref _irradianceTexture, value);
        }
        private TextureHandle _irradianceTexture;

        /// <summary>
        /// STP Upscalerによって書き込まれるSTPデバッグ可視化Texture。
        /// </summary>
        // [設定元] StpUtils.ExecuteDebugViewPass で STP Debug View texture を設定。
        internal TextureHandle stpDebugView
        {
            get => CheckAndGetTextureHandle(ref _stpDebugView);
            set => CheckAndSetTextureHandle(ref _stpDebugView, value);
        }
        private TextureHandle _stpDebugView;

        //Camera Stackingのため、特定の永続的なTarget TextureをDestinationとして設定する必要がある場合があります。
        //その場合、Output/DestinationとしてRG管理TextureをDestination用に作成することはできません。
        //Camera Stackingがなければ、Backbufferが唯一の他の永続Destinationになります。
        //このDestinationの使用は、現在UberポストプロセスPassに限定されています。
        // [設定元] UniversalRenderer.OnAfterRendering で Camera Stack/PostProcess 用の最終 Blit/Resolve 先として import して設定。
        internal TextureHandle destinationCameraColor
        {
            get => CheckAndGetTextureHandle(ref _destinationCameraColor);
            set => CheckAndSetTextureHandle(ref _destinationCameraColor, value);
        }
        private TextureHandle _destinationCameraColor;

        /// <inheritdoc />
        public override void Reset()
        {
            _backBufferColor = TextureHandle.nullHandle;
            _backBufferDepth = TextureHandle.nullHandle;
            _cameraColor = TextureHandle.nullHandle;
            _cameraDepth = TextureHandle.nullHandle;
            _mainShadowsTexture = TextureHandle.nullHandle;
            _additionalShadowsTexture = TextureHandle.nullHandle;
            _cameraOpaqueTexture = TextureHandle.nullHandle;
            _cameraDepthTexture = TextureHandle.nullHandle;
            _cameraNormalsTexture = TextureHandle.nullHandle;
            _motionVectorColor = TextureHandle.nullHandle;
            _motionVectorDepth = TextureHandle.nullHandle;
            _internalColorLut = TextureHandle.nullHandle;
            _bloom = TextureHandle.nullHandle;
            _afterPostProcessColor = TextureHandle.nullHandle;
            _overlayUITexture = TextureHandle.nullHandle;
            _renderingLayersTexture = TextureHandle.nullHandle;
            _dBufferDepth = TextureHandle.nullHandle;
            _ssaoTexture = TextureHandle.nullHandle;
            _irradianceTexture = TextureHandle.nullHandle;
            _stpDebugView = TextureHandle.nullHandle;
            _destinationCameraColor = TextureHandle.nullHandle;

            for (int i = 0; i < _gBuffer.Length; i++)
                _gBuffer[i] = TextureHandle.nullHandle;

            for (int i = 0; i < _dBuffer.Length; i++)
                _dBuffer[i] = TextureHandle.nullHandle;
        }
    }
}
