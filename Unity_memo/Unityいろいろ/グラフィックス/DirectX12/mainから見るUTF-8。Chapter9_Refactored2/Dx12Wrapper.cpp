#include "Dx12Wrapper.h"
#include<cassert>
#include<d3dx12.h>
#include"Application.h"

#pragma comment(lib,"DirectXTex.lib")
#pragma comment(lib,"d3d12.lib")
#pragma comment(lib,"dxgi.lib")
#pragma comment(lib,"d3dcompiler.lib")

using namespace Microsoft::WRL;
using namespace std;
using namespace DirectX;

namespace {
	///モデルのパスとテクスチャのパスから合成パスを得る
	///@param modelPath アプリケーションから見たpmdモデルのパス
	///@param texPath PMDモデルから見たテクスチャのパス
	///@return アプリケーションから見たテクスチャのパス
	std::string GetTexturePathFromModelAndTexPath(const std::string& modelPath, const char* texPath) {
		//ファイルのフォルダ区切りは\と/の二種類が使用される可能性があり
		//ともかく末尾の\か/を得られればいいので、双方のrfindをとり比較する
		//int型に代入しているのは見つからなかった場合はrfindがepos(-1→0xffffffff)を返すため
		int pathIndex1 = modelPath.rfind('/');
		int pathIndex2 = modelPath.rfind('\\');
		auto pathIndex = max(pathIndex1, pathIndex2);
		auto folderPath = modelPath.substr(0, pathIndex + 1);
		return folderPath + texPath;
	}

	///ファイル名から拡張子を取得する
	///@param path 対象のパス文字列
	///@return 拡張子
	string
		GetExtension(const std::string& path) {
		int idx = path.rfind('.');
		return path.substr(idx + 1, path.length() - idx - 1);
	}

	///ファイル名から拡張子を取得する(ワイド文字版)
	///@param path 対象のパス文字列
	///@return 拡張子
	wstring
		GetExtension(const std::wstring& path) {
		int idx = path.rfind(L'.');
		return path.substr(idx + 1, path.length() - idx - 1);
	}

	///テクスチャのパスをセパレータ文字で分離する
	///@param path 対象のパス文字列
	///@param splitter 区切り文字
	///@return 分離前後の文字列ペア
	pair<string, string>
		SplitFileName(const std::string& path, const char splitter = '*') {
		int idx = path.find(splitter);
		pair<string, string> ret;
		ret.first = path.substr(0, idx);
		ret.second = path.substr(idx + 1, path.length() - idx - 1);
		return ret;
	}

	///string(マルチバイト文字列)からwstring(ワイド文字列)を得る
	///@param str マルチバイト文字列
	///@return 変換されたワイド文字列
	std::wstring
		GetWideStringFromString(const std::string& str) {
		//呼び出し1回目(文字列数を得る)
		auto num1 = MultiByteToWideChar(CP_ACP,
			MB_PRECOMPOSED | MB_ERR_INVALID_CHARS,
			str.c_str(), -1, nullptr, 0);

		std::wstring wstr;//stringのwchar_t版
		wstr.resize(num1);//得られた文字列数でリサイズ

		//呼び出し2回目(確保済みのwstrに変換文字列をコピー)
		auto num2 = MultiByteToWideChar(CP_ACP,
			MB_PRECOMPOSED | MB_ERR_INVALID_CHARS,
			str.c_str(), -1, &wstr[0], num1);

		assert(num1 == num2);//一応チェック
		return wstr;
	}
	///デバッグレイヤーを有効にする
	void EnableDebugLayer() {
		ComPtr<ID3D12Debug> debugLayer = nullptr;
		auto result = D3D12GetDebugInterface(IID_PPV_ARGS(&debugLayer));
		debugLayer->EnableDebugLayer();
	}
}

//『主に _dev, cmd系, _swapchain, _rtvHeaps, _dsvHeap, _sceneDescHeap, _fence
Dx12Wrapper::Dx12Wrapper(HWND hwnd){
#ifdef _DEBUG
	//デバッグレイヤーをオンに
	EnableDebugLayer();
#endif

	auto& app=Application::Instance(); //『シングルトンからApplicationインスタンス取得
	_winSize = app.GetWindowSize(); //『window_width, window_heightを取得

	//DirectX12関連初期化
	if (FAILED(InitializeDXGIDevice())) { //『IDXGIFactory4 _dxgiFactory, ID3D12Device _dev の生成
		assert(0);
		return;
	}
	if (FAILED(InitializeCommand())) { //『ID3D12CommandAllocator _cmdAllocator, ID3D12GraphicsCommandList _cmdList, ID3D12CommandQueue _cmdQueue の生成
		assert(0);
		return;
	}
	if (FAILED(CreateSwapChain(hwnd))) { //『IDXGISwapChain4 _swapchain の生成
		assert(0);
		return;
	}
	if (FAILED(CreateFinalRenderTargets())) { //『RTVディスクリプタヒープ[_rtvHeaps]を生成し、スワップチェインのResourceを入れる。それと、_viewport, _scissorrectを設定
		assert(0);
		return;
	}

	if (FAILED(CreateSceneView())) { //『Resource[_sceneConstBuff]を生成し、そこにview,proj,eyeをMapする。そしてCBVディスクリプタヒープ[_sceneDescHeap]を生成し入れる
		assert(0);
		return;
	}

	//テクスチャローダー関連初期化
	CreateTextureLoaderTable(); //『拡張子を入れるとそれ用のCPUローダー[LoadFromWICFile(pathを入れるとmetaとimgを取得する)]を返す



	//深度バッファ作成 //『典型的な、Resource生成からビューを作りディスクリプタヒープに入れる例
	if (FAILED(CreateDepthStencilView())) { //『Resource [_depthBuffer]を生成しDescriptorHeap [_dsvHeap]を生成し入れる
		assert(0);
		return ;
	}

	//フェンスの作成
	if (FAILED(_dev->CreateFence(_fenceVal, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(_fence.ReleaseAndGetAddressOf())))) { //『ID3D12Fence _fenceを生成
		assert(0);
		return ;
	}
	
}

//深度バッファ作成 //『典型的な、Resource生成からビューを作りディスクリプタヒープに入れる例
HRESULT Dx12Wrapper::CreateDepthStencilView() {
	DXGI_SWAP_CHAIN_DESC1 desc = {};
	auto result = _swapchain->GetDesc1(&desc);
	//深度バッファ作成
	//深度バッファの仕様
	//auto depthResDesc = CD3DX12_RESOURCE_DESC::Tex2D(DXGI_FORMAT_D32_FLOAT,
	//	desc.Width, desc.Height,
	//	1, 0, 1, 0,
	//	D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL);

	//『Resourceに CBV,SRV,UAV,RTV,DSV,SAMPLER の指定は無い(区別がない)
	D3D12_RESOURCE_DESC resdesc = {};
	resdesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
	resdesc.DepthOrArraySize = 1;	//配列サイズ
	resdesc.Width = desc.Width;
	resdesc.Height = desc.Height;
	resdesc.Format = DXGI_FORMAT_D32_FLOAT; //『D32_FLOAT
	resdesc.SampleDesc.Count = 1;
	resdesc.SampleDesc.Quality = 0;
	resdesc.Flags = D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;
	resdesc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
	resdesc.MipLevels = 1;	//MipMapレベル数
	resdesc.Alignment = 0;	//サブリソース間のアラインメント?(https://alphakaz.hatenablog.com/entry/2022/10/05/001816)
	//デプス用ヒーププロパティ
	auto depthHeapProp = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_DEFAULT); //『DEFAULT
	//『クリアバリュー(初期化値)
	CD3DX12_CLEAR_VALUE depthClearValue(DXGI_FORMAT_D32_FLOAT, 1.0f, 0);
	result = _dev->CreateCommittedResource(
		&depthHeapProp,
		D3D12_HEAP_FLAG_NONE,
		&resdesc,
		D3D12_RESOURCE_STATE_DEPTH_WRITE, //デプス書き込みに使用 //『バリアで変わる部分(これは変えないけど)
		&depthClearValue, //『初期化値。多分マップなどで初期化されない場合は必要
		IID_PPV_ARGS(_depthBuffer.ReleaseAndGetAddressOf())); //『ID3D12Resource _depthBuffer 生成
	if (FAILED(result)) {
		//エラー処理
		return result;
	}

	//深度のためのデスクリプタヒープ作成
	D3D12_DESCRIPTOR_HEAP_DESC dsvHeapDesc = {};//深度に使うよという事がわかればいい
	dsvHeapDesc.NumDescriptors = 1;//深度ビュー1つのみ
	dsvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_DSV;//デプスステンシルビュー[DSV]として使う
	dsvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
	result = _dev->CreateDescriptorHeap(&dsvHeapDesc, IID_PPV_ARGS(_dsvHeap.ReleaseAndGetAddressOf())); //『ID3D12DescriptorHeap　_dsvHeap 生成

	//深度ビュー作成
	D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
	dsvDesc.Format = DXGI_FORMAT_D32_FLOAT;//デプス値に32bit使用
	dsvDesc.ViewDimension = D3D12_DSV_DIMENSION_TEXTURE2D;//2Dテクスチャ
	dsvDesc.Flags = D3D12_DSV_FLAG_NONE;//フラグは特になし
	_dev->CreateDepthStencilView(_depthBuffer.Get(), &dsvDesc, _dsvHeap->GetCPUDescriptorHandleForHeapStart()); //『Resourceからビューを生成しディスクリプタヒープに入れる
}


Dx12Wrapper::~Dx12Wrapper()
{
}


ComPtr<ID3D12Resource>
Dx12Wrapper::GetTextureByPath(const char* texpath) {
	auto it = _textureTable.find(texpath);
	if (it != _textureTable.end()) {
		//テーブルに内にあったらロードするのではなくマップ内の
		//リソースを返す
		return _textureTable[texpath];
	}
	else {
		return ComPtr<ID3D12Resource>(CreateTextureFromFile(texpath));
	}

}

//テクスチャローダテーブルの作成
void 
Dx12Wrapper::CreateTextureLoaderTable() {
	_loadLambdaTable["sph"] = _loadLambdaTable["spa"] = _loadLambdaTable["bmp"] = _loadLambdaTable["png"] = _loadLambdaTable["jpg"] = [](const wstring& path, TexMetadata* meta, ScratchImage& img)->HRESULT {
		return LoadFromWICFile(path.c_str(), 0, meta, img);
	};

	_loadLambdaTable["tga"] = [](const wstring& path, TexMetadata* meta, ScratchImage& img)->HRESULT {
		return LoadFromTGAFile(path.c_str(), meta, img);
	};

	_loadLambdaTable["dds"] = [](const wstring& path, TexMetadata* meta, ScratchImage& img)->HRESULT {
		return LoadFromDDSFile(path.c_str(), 0, meta, img);
	};
}
//テクスチャ名からテクスチャバッファ作成、中身をコピー
ID3D12Resource* 
Dx12Wrapper::CreateTextureFromFile(const char* texpath) {
	string texPath = texpath;
	//テクスチャのロード
	TexMetadata metadata = {};
	ScratchImage scratchImg = {};
	auto wtexpath = GetWideStringFromString(texPath);//テクスチャのファイルパス
	auto ext = GetExtension(texPath);//拡張子を取得
	auto result = _loadLambdaTable[ext](wtexpath,
		&metadata,
		scratchImg);
	if (FAILED(result)) {
		return nullptr;
	}
	auto img = scratchImg.GetImage(0, 0, 0);//生データ抽出

	//WriteToSubresourceで転送する用のヒープ設定
	auto texHeapProp = CD3DX12_HEAP_PROPERTIES(D3D12_CPU_PAGE_PROPERTY_WRITE_BACK, D3D12_MEMORY_POOL_L0);
	auto resDesc = CD3DX12_RESOURCE_DESC::Tex2D(metadata.format, metadata.width, metadata.height, metadata.arraySize, metadata.mipLevels);

	ID3D12Resource* texbuff = nullptr;
	result = _dev->CreateCommittedResource(
		&texHeapProp,
		D3D12_HEAP_FLAG_NONE,//特に指定なし
		&resDesc,
		D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE,
		nullptr,
		IID_PPV_ARGS(&texbuff)
	);

	if (FAILED(result)) {
		return nullptr;
	}
	result = texbuff->WriteToSubresource(0,
		nullptr,//全領域へコピー
		img->pixels,//元データアドレス
		img->rowPitch,//1ラインサイズ
		img->slicePitch//全サイズ
	);
	if (FAILED(result)) {
		return nullptr;
	}

	return texbuff;
}

//『IDXGIFactory4 _dxgiFactory, ID3D12Device _dev の生成
HRESULT Dx12Wrapper::InitializeDXGIDevice() {
	UINT flagsDXGI = 0;
	flagsDXGI |= DXGI_CREATE_FACTORY_DEBUG;
	auto result = CreateDXGIFactory2(flagsDXGI, IID_PPV_ARGS(_dxgiFactory.ReleaseAndGetAddressOf())); //『IDXGIFactory生成
	//DirectX12まわり初期化
	//フィーチャレベル列挙
	D3D_FEATURE_LEVEL levels[] = {
		D3D_FEATURE_LEVEL_12_1,
		D3D_FEATURE_LEVEL_12_0,
		D3D_FEATURE_LEVEL_11_1,
		D3D_FEATURE_LEVEL_11_0,
	};
	if (FAILED(result)) {
		return result;
	}
	std::vector <IDXGIAdapter*> adapters;
	IDXGIAdapter* tmpAdapter = nullptr;
	for (int i = 0; _dxgiFactory->EnumAdapters(i, &tmpAdapter) != DXGI_ERROR_NOT_FOUND; ++i) {
		adapters.push_back(tmpAdapter);
	}
	for (auto adpt : adapters) {
		DXGI_ADAPTER_DESC adesc = {};
		adpt->GetDesc(&adesc);
		std::wstring strDesc = adesc.Description;
		if (strDesc.find(L"NVIDIA") != std::string::npos) {
			tmpAdapter = adpt;
			break;
		}
	}
	result = S_FALSE;
	//Direct3Dデバイスの初期化=======================================================================================
	D3D_FEATURE_LEVEL featureLevel;
	for (auto l : levels) {
		if (SUCCEEDED(D3D12CreateDevice(tmpAdapter, l, IID_PPV_ARGS(_dev.ReleaseAndGetAddressOf())))) {
				//『グラボ(tmpAdapter)に合った機能レベル(l)のデバイス(_dev)が生成される
			featureLevel = l;
			result = S_OK;
			break;
		}
	}
	return result;
}

///スワップチェイン生成関数
HRESULT
Dx12Wrapper::CreateSwapChain(const HWND& hwnd) {
	RECT rc = {};
	::GetWindowRect(hwnd, &rc);

	//『スワップチェインDesc (D3D12_RESOURCE_DESCに似ている)
	DXGI_SWAP_CHAIN_DESC1 swapchainDesc = {};
	swapchainDesc.Width = _winSize.cx;/*●*/
	swapchainDesc.Height = _winSize.cy;/*●*/
	swapchainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;/*●*/
	swapchainDesc.Stereo = false;
	swapchainDesc.SampleDesc.Count = 1;
	swapchainDesc.SampleDesc.Quality = 0;
	swapchainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	swapchainDesc.BufferCount = 2;/*●*/
	swapchainDesc.Scaling = DXGI_SCALING_STRETCH;
	swapchainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
	swapchainDesc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;
	swapchainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;
	//『スワップチェイン生成 (Resourceの作り方に似ている)
	auto result= _dxgiFactory->CreateSwapChainForHwnd(
		_cmdQueue.Get(), //『>DeviceなのになぜCommandQueue？ DXGIインターフェイスがDirectX 11とDirectX 12で共通のものだからだと推測されます。(P166)(本来は_devを渡すべき?)
		hwnd, /*●*///『Windows処理のCreateWindow(..)の戻り値
		&swapchainDesc,
		nullptr,
		nullptr,
		(IDXGISwapChain1**)_swapchain.ReleaseAndGetAddressOf());
	assert(SUCCEEDED(result));
	return result;
}

//『ID3D12CommandAllocator _cmdAllocator, ID3D12GraphicsCommandList _cmdList, ID3D12CommandQueue _cmdQueue の生成
HRESULT Dx12Wrapper::InitializeCommand() {
	//『コマンドアロケータ,コマンドリストを生成====================================================================================================================
	//『コマンドアロケータ❰_cmdAllocator❱生成
	auto result = _dev->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(_cmdAllocator.ReleaseAndGetAddressOf()));
	if (FAILED(result)) {
		assert(0);
		return result;
	}
	//『コマンドアロケータを引数にコマンドリスト❰_cmdList❱を生成
	result = _dev->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, /*●*/_cmdAllocator.Get(), nullptr, IID_PPV_ARGS(_cmdList.ReleaseAndGetAddressOf()));
	if (FAILED(result)) {
		assert(0);
		return result;
	}

	//『コマンドキュー生成=======================================================================================================================================
	D3D12_COMMAND_QUEUE_DESC cmdQueueDesc = {};
	cmdQueueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;//タイムアウトなし
	cmdQueueDesc.NodeMask = 0;
	cmdQueueDesc.Priority = D3D12_COMMAND_QUEUE_PRIORITY_NORMAL;//プライオリティ特に指定なし
	cmdQueueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;//ここはコマンドリストと合わせてください
	result = _dev->CreateCommandQueue(&cmdQueueDesc, IID_PPV_ARGS(_cmdQueue.ReleaseAndGetAddressOf()));//コマンドキュー生成
	assert(SUCCEEDED(result));
	return result;
}

//『ほぼカメラ//ビュープロジェクション用ビューの生成
HRESULT Dx12Wrapper::CreateSceneView(){
	DXGI_SWAP_CHAIN_DESC1 desc = {};
	auto result = _swapchain->GetDesc1(&desc);

	//定数バッファ作成
	result = _dev->CreateCommittedResource(
		&CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD),/*●*/
		D3D12_HEAP_FLAG_NONE,
		&CD3DX12_RESOURCE_DESC::Buffer((sizeof(SceneData) + 0xff)&~0xff), //『SceneData.view,proj,eye。 [+ 0xff)&~0xff]は256バイトアライメントにしている
		D3D12_RESOURCE_STATE_GENERIC_READ,
		nullptr,
		IID_PPV_ARGS(_sceneConstBuff.ReleaseAndGetAddressOf()) //『ID3D12Resource生成
	);

	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return result;
	}

// D3D12_ROOT_PARAMETER
// D3D12_ROOT_SIGNATURE_DESC
// D3D12_DESCRIPTOR_RANGE_FLAGS
// ID3D12Resource
// GetGPUDescriptorHandleForHeapStart()
// D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND

	_mappedSceneData = nullptr;//マップ先を示すポインタ[SceneData*]
	result = _sceneConstBuff->Map(0, nullptr, (void**)&_mappedSceneData);/*●*///マップ
	
	XMFLOAT3 eye(0, 15, -15);		//『マップした_mappedSceneDataにuniformデータ設定
	XMFLOAT3 target(0, 15, 0);		//		|
	XMFLOAT3 up(0, 1, 0);			//		|
	_mappedSceneData->view = XMMatrixLookAtLH(XMLoadFloat3(&eye), XMLoadFloat3(&target), XMLoadFloat3(&up));
	_mappedSceneData->proj = XMMatrixPerspectiveFovLH(XM_PIDIV4,//画角は45°
		static_cast<float>(desc.Width) / static_cast<float>(desc.Height),//アス比
		0.1f,//近い方  				//		|
		1000.0f//遠い方				//		|
	);							   //		|
	_mappedSceneData->eye = eye;   //	   __

	D3D12_DESCRIPTOR_HEAP_DESC descHeapDesc = {};
	descHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;/*●*///シェーダから見えるように
	descHeapDesc.NodeMask = 0;//マスクは0
	descHeapDesc.NumDescriptors = 1;/*●*/
	descHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;/*●*///デスクリプタヒープ種別
	result = _dev->CreateDescriptorHeap(&descHeapDesc, IID_PPV_ARGS(_sceneDescHeap.ReleaseAndGetAddressOf()));//生成

	////デスクリプタの先頭ハンドルを取得しておく
	auto heapHandle = _sceneDescHeap->GetCPUDescriptorHandleForHeapStart(); //『パイプラインにセットは_sceneDescHeap->Get"GPU"DescriptorHandleForHeapStart()
	
	D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc = {};
	cbvDesc.BufferLocation = _sceneConstBuff->GetGPUVirtualAddress();/*●*/
	cbvDesc.SizeInBytes = _sceneConstBuff->GetDesc().Width;/*●*/
	//定数バッファビューの作成
	_dev->CreateConstantBufferView(&cbvDesc, heapHandle);
	return result;

}

//『RTVディスクリプタヒープ❰_rtvHeaps❱を生成し、スワップチェインのResourceを入れる。それと、_viewport, _scissorrectを設定
HRESULT	Dx12Wrapper::CreateFinalRenderTargets() {
	//『スワップチェインから.BufferCount,.Width,Heightの情報を取得=============================================================================================================
	//『descは、スワップチェインの.Width,Heightのみ利用されている
	DXGI_SWAP_CHAIN_DESC1 desc = {};
	auto result = _swapchain->GetDesc1(&desc);
	//『swcDescは、スワップチェインの.BufferCountのみ利用されている
	DXGI_SWAP_CHAIN_DESC swcDesc = {};
	result = _swapchain->GetDesc(&swcDesc);

	//『スワップチェインのResource分だけディスクリプタヒープ❰_rtvHeaps❱を生成 と ハンドル❰handle❱取得==============================================================================
	D3D12_DESCRIPTOR_HEAP_DESC heapDesc = {};
	heapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;/*●*///レンダーターゲットビューなので当然RTV	//『draw.ioではD3D12_DESCRIPTOR_HEAP_TYPE_を小さく薄くし、[RTV|DSV|..]を大きくする?
	heapDesc.NodeMask = 0;
	heapDesc.NumDescriptors = 2;/*●*///表裏の２つ
	heapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;//特に指定なし
	result = _dev->CreateDescriptorHeap(&heapDesc, IID_PPV_ARGS(_rtvHeaps.ReleaseAndGetAddressOf())); //『_rtvHeapsにディスクリプタヒープ生成
	if (FAILED(result)) {
		SUCCEEDED(result);
		return result;
	}
	//『ディスクリプタヒープ❰_rtvHeaps❱のハンドル取得
	D3D12_CPU_DESCRIPTOR_HANDLE handle = _rtvHeaps->GetCPUDescriptorHandleForHeapStart(); //『ディスクリプタヒープの先頭アドレス取得
		
	//『スワップチェインからResourceを取得し、ディスクリプタヒープ❰_rtvHeaps❱のハンドル❰handle❱に詰める============================================================================
	//『配列(vector)を2個にリサイズするだけ
	_backBuffers.resize(swcDesc.BufferCount);
	//『レンダーターゲットビュー(RTV)のDescを設定
	D3D12_RENDER_TARGET_VIEW_DESC rtvDesc = {};
	rtvDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;/*●*/
	rtvDesc.ViewDimension = D3D12_RTV_DIMENSION_TEXTURE2D;/*●*///『width,heightなど足りない情報はResourceから取得している[
	//『スワップチェインのResource分ループし、ディスクリプタヒープ❰_rtvHeaps❱のハンドル❰handle❱に詰める
	for (int i = 0; i < swcDesc.BufferCount; ++i) {
		result = _swapchain->GetBuffer(i, IID_PPV_ARGS(&_backBuffers[i])); /*●*///『スワップチェインからバッファを_backBuffers[i]に抽出
		assert(SUCCEEDED(result));
		rtvDesc.Format = _backBuffers[i]->GetDesc().Format;
		_dev->CreateRenderTargetView(_backBuffers[i], &rtvDesc, handle); /*●*///『スワップチェインから取得したResourceを、ディスクリプタヒープ❰_rtvHeaps❱のハンドル❰handle❱に詰める
		handle.ptr += _dev->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);//『RTV分ハンドル❰handle❱の.ptrを進める
	}

	//『ビューポート,シザーレクトの設定========================================================================================================================================
	_viewport.reset(new CD3DX12_VIEWPORT(_backBuffers[0]));
	_scissorrect.reset(new CD3DX12_RECT(0, 0, desc.Width, desc.Height));

	return result;
}

ComPtr< ID3D12Device> 
Dx12Wrapper::Device() {
	return _dev;
}
ComPtr < ID3D12GraphicsCommandList> 
Dx12Wrapper::CommandList() {
	return _cmdList;
}

void 
Dx12Wrapper::Update() {

}

 //『バックバッファ(RTV)Resourceのバリア と RTVとDSVのレンダーターゲット設定とクリア と ビューポートとシザーの設定
void Dx12Wrapper::BeginDraw() {
	//DirectX処理
	//現在のバックバッファのインデックスを取得=================================================================================================
	auto bbIdx = _swapchain->GetCurrentBackBufferIndex();

// ID3D12GraphicsCommandList
// D3D12_CPU_DESCRIPTOR_HANDLE
// D3D12_RECT
// D3D12_ENCODE_SHADER_4_COMPONENT_MAPPING
// ID3D12GraphicsCommandList
// D3D12_QUERY_TYPE_OCCLUSION
// D3D12_COMMAND_SIGNATURE_DESC
// D3D12_DRAW_INDEXED_ARGUMENTS
// D3D12_INDIRECT_ARGUMENT_TYPE_DRAW_INDEXED
// CreateCommandSignature
// D3D12_DRAW_INDEXED_ARGUMENTS
// ID3D12CommandQueue

	//『バリア設定//『現在のバックバッファ[ID3D12Resource _backBuffers[bbIdx]]のバリアを Transition:PRESENT=>TARGET に設定======================
	_cmdList->ResourceBarrier(
		1/*NumBarriers*/,
		&CD3DX12_RESOURCE_BARRIER::Transition(
			_backBuffers[bbIdx], //『現在のバックバッファ
			D3D12_RESOURCE_STATE_PRESENT, 		//『プレゼントから、
			D3D12_RESOURCE_STATE_RENDER_TARGET  //『レンダーターゲットに遷移
		)/*pBarriers*/
	);

	//『RTVとDSVのハンドルの取得=============================================================================================================
	//『RTVのハンドルを取得 //『_rtvHeapsから、bbIdxで現在のバックバッファ(RTV)のD3D12_CPU_DESCRIPTOR_HANDLE rtvHを取得 //『ビューはCPU側にある? ResourceはGPU側?
	auto rtvH = _rtvHeaps->GetCPUDescriptorHandleForHeapStart();
	rtvH.ptr += bbIdx * _dev->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
	//『DSVのハンドルを取得 //『_dsvHeapsから、DSVのD3D12_CPU_DESCRIPTOR_HANDLE dsvHを取得
	auto dsvH = _dsvHeap->GetCPUDescriptorHandleForHeapStart();

	//『パイプラインのレンダーターゲット(RTV,DSV)の設定とクリア================================================================================
	//『RTVとDSVのハンドルをレンダーターゲットとして設定
	_cmdList->OMSetRenderTargets(1/*2以上はMRT?*/, &rtvH, false, &dsvH);
	//『DSVのクリア
	_cmdList->ClearDepthStencilView(dsvH, D3D12_CLEAR_FLAG_DEPTH, 1.0f/*Depth*/, 0/*Stencil*/, 0/*NumRects*/, nullptr/*pRects*/);
	//『RTVのクリア
	float clearColor[] = { 1.0f,0.0f,1.0f,1.0f };//白色
	_cmdList->ClearRenderTargetView(rtvH, clearColor/*ColorRGBA*/, 0/*NumRects*/, nullptr/*pRects*/);

	//ビューポート、シザー矩形のセット========================================================================================================
	_cmdList->RSSetViewports(1/*NumViewports*/, _viewport.get());
	_cmdList->RSSetScissorRects(1/*NumRects*/, _scissorrect.get());
}

//『カメラのCBVをパイプラインに設定
void Dx12Wrapper::SetScene() {
	//『カメラのCBVをパイプラインに設定//現在のシーン(ビュープロジェクション)をセット
	ID3D12DescriptorHeap* sceneheaps[] = { _sceneDescHeap.Get() };
	_cmdList->SetDescriptorHeaps(1, sceneheaps);
	_cmdList->SetGraphicsRootDescriptorTable(0/*RootParameterIndex*/, _sceneDescHeap->GetGPUDescriptorHandleForHeapStart()); //『テーブルとヒープがすり合わせられる
}

//『バックバッファのバリア、コマンドリスト命令のクローズ、コマンドリストをコマンドキューで実行、フェンスによる待ち、コマンド[リスト|アロケータ]のクリア
void Dx12Wrapper::EndDraw() {
	//『バックバッファのバリア=====================================================================================
	//『スワップチェインから現在のバックバッファのIndexを取得
	auto bbIdx = _swapchain->GetCurrentBackBufferIndex();
	//『バリアで、現在のバックバッファResourceの状態を RENDER_TARGET=>PRESENT に Transition する
	_cmdList->ResourceBarrier(1,
		&CD3DX12_RESOURCE_BARRIER::Transition(_backBuffers[bbIdx],
			D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_PRESENT));

	//命令のクローズ=============================================================================================
	_cmdList->Close();

	//『コマンドキュー[_cmdQueue]から、コマンドリスト[_cmdlist] をキューに入れ実行====================================
	ID3D12CommandList* cmdlists[] = { _cmdList.Get() };
	_cmdQueue->ExecuteCommandLists(1/*NumCommandLists*/, cmdlists/*[型]ID3D12CommandList*/);

	//『フェンス[_fence]による、コマンドキュー[_cmdQueue]の実行の待ち================================================
		//『フェンスは任意の処理をコマンドキューの間に差し込める?(基本非同期)
	//『最後にキューに入れられたコマンドが実行完了した時に、pFenceの値がValueに変わる?
	_cmdQueue->Signal(_fence.Get()/*pFence*/, ++_fenceVal/*Value*/);
	//『フェンスの値[GetCompletedValue()]が_fenceValと同じで無い場合、イベントを作って待つ===========
	if (_fence->GetCompletedValue() < _fenceVal) {
		auto event = CreateEvent(nullptr, false, false, nullptr);
		//『多分、{中でGetCompletedValue()を実行し、_fenceValと同じかチェックし待つ、同じ(その時最後だったコマンドの実行完了?)になったらイベントを実行する} というイベントをeventに作る?
		_fence->SetEventOnCompletion(_fenceVal, event);
		//『↑で設定したeventが終わるまで待っている多分
		WaitForSingleObject(event, INFINITE);
		CloseHandle(event);
	}

	//『コマンドリストとコマンドアロケータのクリア==================================================================
		//『(なぜ2つクリアのAPIがあるのか、コマンドキューの実行時にクリアするかのフラグで制御したほうがいいのでは)
	_cmdAllocator->Reset();//キューをクリア
	_cmdList->Reset(_cmdAllocator.Get(), nullptr);//再びコマンドリストをためる準備
}

ComPtr < IDXGISwapChain4> 
Dx12Wrapper::Swapchain() {
	return _swapchain;
}