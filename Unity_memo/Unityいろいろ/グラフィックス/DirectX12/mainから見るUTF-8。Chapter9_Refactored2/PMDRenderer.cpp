#include "PMDRenderer.h"
#include<d3dx12.h>
#include<cassert>
#include<d3dcompiler.h>
#include"Dx12Wrapper.h"
#include<string>
#include<algorithm>

using namespace std;

namespace {
	void PrintErrorBlob(ID3DBlob* blob) {
		assert(blob);
		string err;
		err.resize(blob->GetBufferSize());
		copy_n((char*)blob->GetBufferPointer(),err.size(),err.begin());
	}
}

//『主に RootSignature[rootParams[3], samplerDescs[2]], PipelineState, Resource[_white|_black|_grad]Tex
PMDRenderer::PMDRenderer(Dx12Wrapper& dx12):_dx12(dx12)
{
	assert(SUCCEEDED(CreateRootSignature())); //『_rootSignature生成 (3, rootParams, 2, samplerDescs)
	assert(SUCCEEDED(CreateGraphicsPipelineForPMD())); //『ID3D12PipelineState _pipeline生成(シェーダー,頂点レイアウト)
	_whiteTex = CreateWhiteTexture(); //『ID3D12Resourceを生成し、書き込み
	_blackTex = CreateBlackTexture(); //『ID3D12Resourceを生成し、書き込み
	_gradTex = CreateGrayGradationTexture(); //『ID3D12Resourceを生成し、書き込み
}


PMDRenderer::~PMDRenderer()
{
}


void 
PMDRenderer::Update() {

}
void 
PMDRenderer::Draw() {

}

ID3D12Resource* 
PMDRenderer::CreateDefaultTexture(size_t width, size_t height) {
	auto resDesc = CD3DX12_RESOURCE_DESC::Tex2D(DXGI_FORMAT_R8G8B8A8_UNORM, width, height);
	auto texHeapProp = CD3DX12_HEAP_PROPERTIES(D3D12_CPU_PAGE_PROPERTY_WRITE_BACK, D3D12_MEMORY_POOL_L0);
	ID3D12Resource* buff = nullptr;
	auto result = _dx12.Device()->CreateCommittedResource(
		&texHeapProp,
		D3D12_HEAP_FLAG_NONE,//特に指定なし
		&resDesc,
		D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE,
		nullptr,
		IID_PPV_ARGS(&buff)
	);
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return nullptr;
	}
	return buff;
}

ID3D12Resource* 
PMDRenderer::CreateWhiteTexture() {
	
	ID3D12Resource* whiteBuff = CreateDefaultTexture(4,4); //『ID3D12Resource生成
	
	std::vector<unsigned char> data(4 * 4 * 4); //『unsigned char * 4(rgba) * 4(w) * 4(h)
	std::fill(data.begin(), data.end(), 0xff); //『全0xffは白

	auto result = whiteBuff->WriteToSubresource(0, nullptr, data.data()/* *pSrcData */, 4 * 4/*SrcRowPitch(4(rgba) * 4(w))*/, data.size()/*SrcDepthPitch(1枚)*/);
	assert(SUCCEEDED(result));
	return whiteBuff;
}
ID3D12Resource*	
PMDRenderer::CreateBlackTexture() {

	ID3D12Resource* blackBuff = CreateDefaultTexture(4, 4);
	std::vector<unsigned char> data(4 * 4 * 4);
	std::fill(data.begin(), data.end(), 0x00); //全0x00は黒

	auto result = blackBuff->WriteToSubresource(0, nullptr, data.data(), 4 * 4, data.size());
	assert(SUCCEEDED(result));
	return blackBuff;
}
ID3D12Resource*	
PMDRenderer::CreateGrayGradationTexture() {
	ID3D12Resource* gradBuff = CreateDefaultTexture(4, 256);
	//上が白くて下が黒いテクスチャデータを作成
	std::vector<unsigned int> data(4 * 256); //『unsigned int? * 4(rgba) * 1(w) * 256(h) ?
	auto it = data.begin();
	unsigned int c = 0xff;
	for (; it != data.end(); it += 4) {
		auto col = (c << 0xff) | (c << 16) | (c << 8) | c;
		std::fill(it, it + 4, col); //『4(rgba) * 1(w)づつ、c[col]を--cしつつdataに書き込み
		--c;
	}

	auto result = gradBuff->WriteToSubresource(0, nullptr, data.data(), 4 * sizeof(unsigned int)/*SrcRowPitch(4(rgba) * uint)*/, sizeof(unsigned int)*data.size());
	assert(SUCCEEDED(result));
	return gradBuff;
}

bool 
PMDRenderer::CheckShaderCompileResult(HRESULT result, ID3DBlob* error) {
	if (FAILED(result)) {
		if (result == HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND)) {
			::OutputDebugStringA("ファイルが見当たりません");
		}
		else {
			std::string errstr;
			errstr.resize(error->GetBufferSize());
			std::copy_n((char*)error->GetBufferPointer(), error->GetBufferSize(), errstr.begin());
			errstr += "\n";
			OutputDebugStringA(errstr.c_str());
		}
		return false;
	}
	else {
		return true;
	}
}

//パイプライン初期化 //『ID3D12PipelineState _pipeline生成(シェーダー,頂点レイアウト)
HRESULT PMDRenderer::CreateGraphicsPipelineForPMD() {
	ComPtr<ID3DBlob> vsBlob = nullptr;
	ComPtr<ID3DBlob> psBlob = nullptr;
	ComPtr<ID3DBlob> errorBlob = nullptr;
	//『シェーダーコンパイル================================================================================
	//『ファイル名から、vertexシェーダー["BasicVS"]をコンパイルし、ID3DBlob vsBlobに格納
	auto result = D3DCompileFromFile(
		L"BasicShader.hlsl", //『ファイル名
		nullptr, //『SHADER_MACRO (#define,シェーダーキーワード)
		nullptr, //『Include (#include,インクルードファイル)
		"BasicVS", //『エントリポイント
		"vs_5_0", //『どのシェーダーを割り当てるか（vs、psなど）
		D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION, //『コンパイルオプション
		0, //『>エフェクトコンパイルオプション（0を推奨）
		&vsBlob, //『コンパイルBlob出力
		&errorBlob //『エラー出力
	);
	if (!CheckShaderCompileResult(result,errorBlob.Get())){
		assert(0);
		return result;
	}
	//『ファイル名から、pixelシェーダー["BasicPS"]をコンパイルし、ID3DBlob psBlobに格納
	result = D3DCompileFromFile(
		L"BasicShader.hlsl", //『ファイル名
		nullptr, //『SHADER_MACRO (#define,シェーダーキーワード)
		nullptr, //『Include (#include,インクルードファイル)
		"BasicPS", //『エントリポイント
		"ps_5_0", //『どのシェーダーを割り当てるか（vs、psなど）
		D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION, //『コンパイルオプション
		0, //『>エフェクトコンパイルオプション（0を推奨）
		&psBlob, //『コンパイルBlob出力
		&errorBlob //『エラー出力
	);
	if (!CheckShaderCompileResult(result, errorBlob.Get())) {
		assert(0);
		return result;
	}
	//『頂点レイアウト=======================================================================================
	D3D12_INPUT_ELEMENT_DESC inputLayout[] = {
		{ "POSITION",0,DXGI_FORMAT_R32G32B32_FLOAT,0/*スロット*/,D3D12_APPEND_ALIGNED_ELEMENT/*0*/,D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,0/*インスタンシングだっけ?*/ },
		{ "NORMAL",0,DXGI_FORMAT_R32G32B32_FLOAT,0,D3D12_APPEND_ALIGNED_ELEMENT/*↑R32G32B32=(32/8)*3=12byte*/,D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,0 },
		{ "TEXCOORD",0,DXGI_FORMAT_R32G32_FLOAT,0,D3D12_APPEND_ALIGNED_ELEMENT/*↑↑12+↑12=24byte*/,D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,0 },
		//{ "BONE_NO",0,DXGI_FORMAT_R16G16_UINT,0,D3D12_APPEND_ALIGNED_ELEMENT,D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,0 },
		//{ "WEIGHT",0,DXGI_FORMAT_R8_UINT,0,D3D12_APPEND_ALIGNED_ELEMENT,D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,0 },
		//{ "EDGE_FLG",0,DXGI_FORMAT_R8_UINT,0,D3D12_APPEND_ALIGNED_ELEMENT,D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,0 },
	};
		//UINT SemanticIndex: { "TEXCOORD",0,..},{ "TEXCOORD",1,..},..,{ "TEXCOORD",n,..}となると、シェーダー側のセマンティクスは、:TEXCOORD0,:TEXCOORD1,..,:TEXCOORDn となる?
		//D3D12_APPEND_ALIGNED_ELEMENT: UINT AlignedByteOffsetは、「そのデータの場所」を示します。例えば座標データのあとに法線データが来るとして、そのときの法線データの場所は32ビット（4バイト）のfloat 3つ分のあとなので、12（バイト）と指定します。しかし、いちいちデータサイズを計算するのも面倒なので、次から次にデータが並んでいる場合は、連続していることを表す定数、D3D12_APPEND_ALIGNED_ELEMENTを指定するとよいでしょう。

	//『パイプラインステート(ルートシグネチャ, シェーダー(VS,PS), SampleMask?, アルファブレンディング, RasterizerState, DepthStencilState, 頂点レイアウト, Strip, Primitiveタイプ, MSAA)
	D3D12_GRAPHICS_PIPELINE_STATE_DESC gpipeline = {};
	gpipeline.pRootSignature = _rootSignature.Get(); //『ルートシグネチャ

	gpipeline.VS = CD3DX12_SHADER_BYTECODE(vsBlob.Get()); //『vertexシェーダー
	gpipeline.PS = CD3DX12_SHADER_BYTECODE(psBlob.Get()); //『pixelシェーダー

	gpipeline.SampleMask = D3D12_DEFAULT_SAMPLE_MASK;//中身は0xffffffff


	gpipeline.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT); //『アルファブレンディング

	gpipeline.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
	gpipeline.RasterizerState.CullMode = D3D12_CULL_MODE_NONE;//カリングしない

	gpipeline.DepthStencilState.DepthEnable = true;//深度バッファを使うぞ
	gpipeline.DepthStencilState.DepthWriteMask = D3D12_DEPTH_WRITE_MASK_ALL;//全て書き込み
	gpipeline.DepthStencilState.DepthFunc = D3D12_COMPARISON_FUNC_LESS;//小さい方を採用
	gpipeline.DSVFormat = DXGI_FORMAT_D32_FLOAT;
	gpipeline.DepthStencilState.StencilEnable = false;

	gpipeline.InputLayout.pInputElementDescs = inputLayout;//レイアウト先頭アドレス
	gpipeline.InputLayout.NumElements = _countof(inputLayout);//レイアウト配列数

	gpipeline.IBStripCutValue = D3D12_INDEX_BUFFER_STRIP_CUT_VALUE_DISABLED;//ストリップ時のカットなし
	gpipeline.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;//三角形で構成

	gpipeline.NumRenderTargets = 1;//今は１つのみ //『2以上はMRT?
	gpipeline.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;//0～1に正規化されたRGBA

	gpipeline.SampleDesc.Count = 1;//サンプリングは1ピクセルにつき１
	gpipeline.SampleDesc.Quality = 0;//クオリティは最低

	result = _dx12.Device()->CreateGraphicsPipelineState(&gpipeline, IID_PPV_ARGS(_pipeline.ReleaseAndGetAddressOf())); //『ID3D12PipelineState _pipeline生成
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
	}
	return result;

}
//ルートシグネチャ初期化
HRESULT 
PMDRenderer::CreateRootSignature() { //『_rootSignature生成 (3, rootParams, 2, samplerDescs)
	//レンジ
	CD3DX12_DESCRIPTOR_RANGE  descTblRanges[4] = {};//テクスチャと定数の２つ
	descTblRanges[0].Init(D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 0);//定数[b0](ビュープロジェクション用)
	descTblRanges[1].Init(D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 1);//定数[b1](ワールド、ボーン用)
	descTblRanges[2].Init(D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 2);//定数[b2](マテリアル用)
	descTblRanges[3].Init(D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 4/*レンジ*/, 0/*[t0～3]*/);//テクスチャ４つ(基本とsphとspaとトゥーン)

	//ルートパラメータ
	CD3DX12_ROOT_PARAMETER rootParams[3] = {};
	rootParams[0].InitAsDescriptorTable(1, &descTblRanges[0]);//ビュープロジェクション変換 ([b0])
	rootParams[1].InitAsDescriptorTable(1, &descTblRanges[1]);//ワールド・ボーン変換 ([b1])
	rootParams[2].InitAsDescriptorTable(2, &descTblRanges[2]/*～[3]*/);//マテリアル周り ([b2],[t0],[t1],[t2],[t3] (ディスクリプタヒープとすり合わされる))

	CD3DX12_STATIC_SAMPLER_DESC samplerDescs[2] = {};
	samplerDescs[0].Init(0);
	samplerDescs[1].Init(1, D3D12_FILTER_ANISOTROPIC, D3D12_TEXTURE_ADDRESS_MODE_CLAMP, D3D12_TEXTURE_ADDRESS_MODE_CLAMP); //『トゥーン用

	CD3DX12_ROOT_SIGNATURE_DESC rootSignatureDesc = {};
	rootSignatureDesc.Init(3, rootParams, 2, samplerDescs, D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT); //『3, rootParams, 2, samplerDescs, INPUT_LAYOUT

	ComPtr<ID3DBlob> rootSigBlob = nullptr;
	ComPtr<ID3DBlob> errorBlob = nullptr;
	auto result = D3D12SerializeRootSignature(&rootSignatureDesc, D3D_ROOT_SIGNATURE_VERSION_1, &rootSigBlob, &errorBlob); //『rootSignatureDesc を rootSigBlob へ シリアライズ
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return result;
	}
	//ID3D12RootSignature _rootSignatureを生成 (ROOT_SIGNATURE_DESC => ID3DBlob => RootSignature)
	result = _dx12.Device()->CreateRootSignature(0, rootSigBlob->GetBufferPointer(), rootSigBlob->GetBufferSize(), IID_PPV_ARGS(_rootSignature.ReleaseAndGetAddressOf()));
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return result;
	}
	return result;
}

ID3D12PipelineState* 
PMDRenderer::GetPipelineState() {
	return _pipeline.Get();
}

ID3D12RootSignature* 
PMDRenderer::GetRootSignature() {
	return _rootSignature.Get();
}