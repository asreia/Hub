using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class Feature : ScriptableRendererFeature //`RenderPipelineAsset<T>`相当
{
    class Item : ContextItem
    {
        public TextureHandle textureHandle;
        public override void Reset() //プールされているので、毎フレーム`Reset()`する
        {
            textureHandle = TextureHandle.nullHandle;
        }
    } 
    Pass pass;
    class Pass : ScriptableRenderPass //`RenderPipeline`相当
    {
        public Pass() //.ctor()
        {
            requiresIntermediateTexture = true; //サンプル集ではここで設定していた
        }
        public void Setup() //ユーザー定義
        {
            ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color);
        }
        class PassData{}
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            Item item = frameData.GetOrCreate<Item>(); //`ContextItem`を取得

            using(var builder = renderGraph.AddRasterRenderPass<PassData>("PassName", out var passData))
            {
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>{});    
            }
        }
    }

    public override void Create() //`renderer`に`sRF`をセット時のみ呼ぶ
    {
        pass = new Pass(){renderPassEvent = RenderPassEvent.AfterRenderingOpaques + 1}; //.ctor()
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) //毎フレーム呼ぶ
    {
        pass.Setup();
        renderer.EnqueuePass(pass); //`renderer`に`pass`を追加
    }
    public bool active;
    void OnValidate()
    {
        SetActive(active); //`SRF`有効化設定
    }
}
