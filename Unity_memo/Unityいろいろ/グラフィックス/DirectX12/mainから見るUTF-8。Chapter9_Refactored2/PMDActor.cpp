#include "PMDActor.h"
#include"PMDRenderer.h"
#include"Dx12Wrapper.h"
#include<d3dx12.h>
using namespace Microsoft::WRL;
using namespace std;
using namespace DirectX;

namespace {
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
	///ファイル名から拡張子を取得する
	///@param path 対象のパス文字列
	///@return 拡張子
	string
		GetExtension(const std::string& path) {
		int idx = path.rfind('.');
		return path.substr(idx + 1, path.length() - idx - 1);
	}
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
}

void* 
PMDActor::Transform::operator new(size_t size) {
	return _aligned_malloc(size, 16);
}

//『主に PMDのファイルパスから、ビュー{_vbView, _ibView} ディスクリプタヒープ{_transformHeap[_transform.world], _materialHeap[CBVx1,SRVx4]} と _materials[].indicesNum を生成
PMDActor::PMDActor(const char* filepath,PMDRenderer& renderer):
	_renderer(renderer),
	_dx12(renderer._dx12),
	_angle(0.0f)
{
	//『ファイルをロードし、ビュー,構造体,Resourceに抽出
	LoadPMDFile(filepath); //『PMDのファイルパス[path]から、ビュー(_vbView, _ibView), _materials[](indicesNum, 他CBuffer関連), [_texture|_sph|_spa|_toon]Resource を生成
	//『モデル変換行列のディスクリプタヒープ生成
	_transform.world = XMMatrixIdentity(); //『↓のCreateTransformView()で使う。描画ループで更新するかも
	CreateTransformView(); //『Transform[4x4マトリックス]のResource[_transformBuff]生成 => _transformをマップ => ディスクリプタヒープ[_transformHeap]を生成して_transformBuffを入れる
	//『マテリアルの構造体(MaterialForHlsl)から、(CBVx1,SRVx4)xマテリアル数 のディスクリプタープを作る
	CreateMaterialData();  //『全てのMaterialを入れるResource[_materialBuff]を確保 => 全てのMaterial[_materials[].material(MaterialForHlsl型)]をマップする
	CreateMaterialAndTextureView(); //『ディスクリプタヒープ[_materialHeap]に、生成済みのID3D12Resource(定数[CBV(_materialBuffの1要素)]1つ、テクスチャ[SRV]4つ)を詰めていく
}


PMDActor::~PMDActor()
{
}


//『PMDのファイルパス[path]から、ビュー(_vbView, _ibView), _materials[](indicesNum, 他CBuffer関連), [_texture|_sph|_spa|_toon]Resource を生成
HRESULT PMDActor::LoadPMDFile(const char* path) {
	//PMDヘッダ構造体
	struct PMDHeader {
		float version; //例：00 00 80 3F == 1.00
		char model_name[20];//モデル名
		char comment[256];//モデルコメント
	};
	char signature[3];
	PMDHeader pmdheader = {};

	string strModelPath = path;

	auto fp = fopen(strModelPath.c_str(), "rb"); //『引数の char* path から FILE* fp 取得
	if (fp == nullptr) {
		//エラー処理
		assert(0);
		return ERROR_FILE_NOT_FOUND;
	}
	//『↓の2つは読んでいるが使っていない
	fread(signature, sizeof(signature), 1, fp); //『"PMD"
	fread(&pmdheader, sizeof(pmdheader), 1, fp); //『バージョン, モデル名, モデルコメント

	//バーテックスバッファ『ファイルからデータ読み込み => Resource生成 => マップ => ビュー作成===============================
	unsigned int vertNum;//頂点数
	fread(&vertNum, sizeof(vertNum), 1, fp); //『頂点数
	constexpr unsigned int pmdvertex_size = 38;//頂点1つあたりのサイズ
	std::vector<unsigned char> vertices(vertNum*pmdvertex_size);//バッファ確保 //『頂点数 x 頂点1つあたりのサイズ
	fread(vertices.data(), vertices.size(), 1, fp); //『頂点バッファ読み込み (.size()は配列の要素数)

	//『Resource生成 //UPLOAD(確保は可能) 
	auto result = _dx12.Device()->CreateCommittedResource(
		&CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD), //『UPLOAD
		D3D12_HEAP_FLAG_NONE,
		&CD3DX12_RESOURCE_DESC::Buffer(vertices.size()),  //『頂点バッファのサイズ
		D3D12_RESOURCE_STATE_GENERIC_READ,
		nullptr,
		IID_PPV_ARGS(_vb.ReleaseAndGetAddressOf())); //『ID3D12Resource _vb にResource生成

	//『マップ
	unsigned char* vertMap = nullptr;
	result = _vb->Map(0, nullptr, (void**)&vertMap);
	std::copy(vertices.begin(), vertices.end(), vertMap); //『頂点バッファをID3D12Resource _vbへマップ
	_vb->Unmap(0, nullptr);

	//『ビュー作成 //バーテックスバッファビュー //『struct D3D12_VERTEX_BUFFER_VIEW _vbView
	_vbView.BufferLocation = _vb->GetGPUVirtualAddress();//バッファの仮想アドレス
	_vbView.SizeInBytes = vertices.size();//全バイト数
	_vbView.StrideInBytes = pmdvertex_size;//1頂点あたりのバイト数

	//インデックスバッファ『ファイルからデータ読み込み => Resource生成 => マップ => ビュー作成===============================
	unsigned int indicesNum;//インデックス数
	fread(&indicesNum, sizeof(indicesNum), 1, fp); //『ファイルからインデックス数を読み込み
	std::vector<unsigned short> indices(indicesNum);
	fread(indices.data(), indices.size() * sizeof(indices[0]), 1, fp); //『インデックスバッファ読み込み


	//設定は、バッファのサイズ以外頂点バッファの設定を使いまわしてOKだと思います。
	//『Resource生成
	result = _dx12.Device()->CreateCommittedResource(
		&CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD), //『UPLOAD
		D3D12_HEAP_FLAG_NONE,
		&CD3DX12_RESOURCE_DESC::Buffer(indices.size() * sizeof(indices[0])), //『インデックスバッファのサイズ
		D3D12_RESOURCE_STATE_GENERIC_READ,
		nullptr,
		IID_PPV_ARGS(_ib.ReleaseAndGetAddressOf())); //『ID3D12Resource _ibにResource生成

	//『マップ //作ったバッファにインデックスデータをコピー
	unsigned short* mappedIdx = nullptr;
	_ib->Map(0, nullptr, (void**)&mappedIdx);
	std::copy(indices.begin(), indices.end(), mappedIdx); //『インデックスバッファをID3D12Resource _ibへマップ
	_ib->Unmap(0, nullptr);

	//『ビュー作成 //インデックスバッファビューを作成 //『struct D3D12_INDEX_BUFFER_VIEW _ibView
	_ibView.BufferLocation = _ib->GetGPUVirtualAddress();//バッファの仮想アドレス
	_ibView.SizeInBytes = indices.size() * sizeof(indices[0]);//全バイト数
	_ibView.Format = DXGI_FORMAT_R16_UINT;//フォーマット

	//マテリアル(CBufferとテクスチャx4)『ファイルからデータ読み込み => Resource生成==================================================================
#pragma pack(1)//ここから1バイトパッキング…アライメントは発生しない
	//PMDマテリアル構造体 //『ファイル[fp]から読み込み専用の構造体
	struct PMDMaterial {
		XMFLOAT3 diffuse; //ディフューズ色
		float alpha; // ディフューズα
		float specularity;//スペキュラの強さ(乗算値)
		XMFLOAT3 specular; //スペキュラ色
		XMFLOAT3 ambient; //アンビエント色
		unsigned char toonIdx; //トゥーン番号(後述)						  //『このtoonIdxのテクスチャをロードしResourceを生成する (Resourceは複数のビューから参照される)
		unsigned char edgeFlg;//マテリアル毎の輪郭線フラグ
		//2バイトのパディングが発生！！
		unsigned int indicesNum; //このマテリアルが割り当たるインデックス数 //『この単位でドローコールされる
		char texFilePath[20]; //テクスチャファイル名(プラスアルファ…後述)   //『このファイル名でテクスチャをロードしResourceを生成する
	};//70バイトのはず…でもパディングが発生するため72バイト
#pragma pack()//1バイトパッキング解除

	//『読み込んだマテリアル数から、Resource生成用のMaterials構造体とテクスチャResourceの配列のリサイズ
	unsigned int materialNum;
	fread(&materialNum, sizeof(materialNum), 1, fp); //『マテリアル数を読み込み
	_materials.resize(materialNum);	// struct Material {unsigned int indicesNum; MaterialForHlsl material; AdditionalMaterial additional;};
	_textureResources.resize(materialNum); //←↓テクスチャResourceの配列
	_sphResources.resize(materialNum);			//マテリアル数を元に各配列をリサイズ
	_spaResources.resize(materialNum);
	_toonResources.resize(materialNum);

	//『fpから読み込み専用の構造体[PMDMaterial]へマテリアルデータ読み込み
	std::vector<PMDMaterial> pmdMaterials(materialNum);
	fread(pmdMaterials.data(), pmdMaterials.size() * sizeof(PMDMaterial), 1, fp); //『全てのマテリアルを読み込み
	//『↑をResource生成用のMaterials構造体へコピー
	for (int i = 0; i < pmdMaterials.size(); ++i) {
		_materials[i].indicesNum = pmdMaterials[i].indicesNum;
		_materials[i].material.diffuse = pmdMaterials[i].diffuse;
		_materials[i].material.alpha = pmdMaterials[i].alpha;
		_materials[i].material.specular = pmdMaterials[i].specular;
		_materials[i].material.specularity = pmdMaterials[i].specularity;
		_materials[i].material.ambient = pmdMaterials[i].ambient;
		_materials[i].additional.toonIdx = pmdMaterials[i].toonIdx;
	}

	//『toonIdx,texFilePathから、テクスチャx4のResource生成==================================================================================================================
	for (int i = 0; i < pmdMaterials.size(); ++i) {

		//『_toonResources のテクスチャResourceをtoonIdxのパスから取得する //トゥーンリソースの読み込み
		char toonFilePath[32];
		sprintf(toonFilePath, "toon/toon%02d.bmp", pmdMaterials[i].toonIdx + 1);
		_toonResources[i] = _dx12.GetTextureByPath(toonFilePath); //toonIdxのパスから テクスチャResourceを生成 または 既にロードしたResourceテーブルから返す

		//『_textureResources, _sphResources, _spaResources のテクスチャResourceをテクスチャファイルパス[texFilePath]から取得する。
			//無かったらnulptrが入る(nulptrはディスクリプタヒープ挿入時、適当なテクスチャResourceが入る)
		if (strlen(pmdMaterials[i].texFilePath) == 0) {
			_textureResources[i] = nullptr;
			continue;
		}
		string texFileName = pmdMaterials[i].texFilePath;
		string sphFileName = "";
		string spaFileName = "";
		if (count(texFileName.begin(), texFileName.end(), '*') > 0) {//スプリッタがある
			auto namepair = SplitFileName(texFileName);
			if (GetExtension(namepair.first) == "sph") {
				texFileName = namepair.second;
				sphFileName = namepair.first;
			}
			else if (GetExtension(namepair.first) == "spa") {
				texFileName = namepair.second;
				spaFileName = namepair.first;
			}
			else {
				texFileName = namepair.first;
				if (GetExtension(namepair.second) == "sph") {
					sphFileName = namepair.second;
				}
				else if (GetExtension(namepair.second) == "spa") {
					spaFileName = namepair.second;
				}
			}
		}
		else {
			if (GetExtension(pmdMaterials[i].texFilePath) == "sph") {
				sphFileName = pmdMaterials[i].texFilePath;
				texFileName = "";
			}
			else if (GetExtension(pmdMaterials[i].texFilePath) == "spa") {
				spaFileName = pmdMaterials[i].texFilePath;
				texFileName = "";
			}
			else {
				texFileName = pmdMaterials[i].texFilePath;
			}
		}
		//モデルとテクスチャパスからアプリケーションからのテクスチャパスを得る
		if (texFileName != "") {
			auto texFilePath = GetTexturePathFromModelAndTexPath(strModelPath, texFileName.c_str());
			_textureResources[i] = _dx12.GetTextureByPath(texFilePath.c_str());
		}
		if (sphFileName != "") {
			auto sphFilePath = GetTexturePathFromModelAndTexPath(strModelPath, sphFileName.c_str());
			_sphResources[i] = _dx12.GetTextureByPath(sphFilePath.c_str());
		}
		if (spaFileName != "") {
			auto spaFilePath = GetTexturePathFromModelAndTexPath(strModelPath, spaFileName.c_str());
			_spaResources[i] = _dx12.GetTextureByPath(spaFilePath.c_str());
		}
	}
	fclose(fp);

}

//『モデル変換行列。オブジェクト回転?
//『Transform[4x4マトリックス]のResource[_transformBuff]生成 => _transformをマップ => ディスクリプタヒープ[_transformHeap]を生成して_transformBuffを入れる
HRESULT PMDActor::CreateTransformView() { //『Transform* _mappedTransform は描画ループで更新される
	//GPUバッファ作成
	auto buffSize = sizeof(Transform); //『XMMATRIX型をTransform型でラップしているがSIMD演算の16バイトアライメントに対応するため
	buffSize = (buffSize + 0xff)&~0xff; //『CBufferは256バイトアライメント
	auto result = _dx12.Device()->CreateCommittedResource(
		&CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD),
		D3D12_HEAP_FLAG_NONE,
		&CD3DX12_RESOURCE_DESC::Buffer(buffSize),
		D3D12_RESOURCE_STATE_GENERIC_READ,
		nullptr,
		IID_PPV_ARGS(_transformBuff.ReleaseAndGetAddressOf()) //『ID3D12Resource _transformBuff を生成
	);
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return result;
	}

	//マップとコピー //『Map(..)したままUnmap(..)はしていない
	result = _transformBuff->Map(0, nullptr, (void**)&_mappedTransform); //『Transform* _mappedTransformにbuffSizeが割り当てられる?
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return result;
	}
	//『Transform* _mappedTransform //『Unmap(..)していないのでTransform* _mappedTransformを操作するとマップ先も変更される
	*_mappedTransform = _transform; //『_transformは、PMDActor(..) の _transform.world = XMMatrixIdentity()/*単位ベクトル*/; で設定している

	//『ディスクリプタヒープ生成
	D3D12_DESCRIPTOR_HEAP_DESC transformDescHeapDesc = {};
	transformDescHeapDesc.NumDescriptors = 1;//とりあえずワールド[_transform.world]ひとつ
	transformDescHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
	transformDescHeapDesc.NodeMask = 0;
	transformDescHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;//デスクリプタヒープ種別
	result = _dx12.Device()->CreateDescriptorHeap(&transformDescHeapDesc, IID_PPV_ARGS(_transformHeap.ReleaseAndGetAddressOf()));//生成
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return result;
	}

	//『ビュー(ディスクリプタ)を生成し、ディスクリプタヒープに入れる
	D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc = {};
	cbvDesc.BufferLocation = _transformBuff->GetGPUVirtualAddress();
	cbvDesc.SizeInBytes = buffSize;
	_dx12.Device()->CreateConstantBufferView(&cbvDesc, _transformHeap->GetCPUDescriptorHandleForHeapStart());

	return S_OK;
}

//『全てのMaterialを入れるResource[_materialBuff]を確保 => 全てのMaterial[_materials[].material]をマップする
HRESULT PMDActor::CreateMaterialData() {
	//マテリアルバッファを作成 //『全てのMaterialを入れるResource[_materialBuff]を確保
	auto materialBuffSize = sizeof(MaterialForHlsl);
	materialBuffSize = (materialBuffSize + 0xff)&~0xff;
	auto result = _dx12.Device()->CreateCommittedResource(
		&CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD),
		D3D12_HEAP_FLAG_NONE,
		&CD3DX12_RESOURCE_DESC::Buffer(materialBuffSize*_materials.size()),//勿体ないけど仕方ないですね //『一つのResourceの領域にMaterialの数だけ確保する(ビューによってスライスする)
		D3D12_RESOURCE_STATE_GENERIC_READ,
		nullptr,
		IID_PPV_ARGS(_materialBuff.ReleaseAndGetAddressOf())
	);
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return result;
	}

	//マップマテリアルにコピー //『全てのMaterial[_materials[].material]をマップする
	char* mapMaterial = nullptr;
	result = _materialBuff->Map(0, nullptr, (void**)&mapMaterial);
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return result;
	}
	for (auto& m : _materials) { //『全てのMaterialを一つのResourceに入れる
		*((MaterialForHlsl*)mapMaterial) = m.material;//データコピー
		mapMaterial += materialBuffSize;//次のアライメント位置まで進める
	}
	_materialBuff->Unmap(0, nullptr);

	return S_OK;

}

//『ディスクリプタヒープ[_materialHeap]に、生成済みのID3D12Resource(定数[CBV(_materialBuffの1要素)]1つ、テクスチャ[SRV]4つ)を詰めていく
HRESULT PMDActor::CreateMaterialAndTextureView() {
	//『ディスクリプターヒープ生成[ID3D12DescriptorHeap _materialHeap]
	D3D12_DESCRIPTOR_HEAP_DESC materialDescHeapDesc = {};
	materialDescHeapDesc.NumDescriptors = _materials.size() * 5;//マテリアル数 * 5要素(定数[CBV]1つ、テクスチャ[SRV]4つ)
	materialDescHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
	materialDescHeapDesc.NodeMask = 0;
	materialDescHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;//デスクリプタヒープ種別
	auto result = _dx12.Device()->CreateDescriptorHeap(&materialDescHeapDesc, IID_PPV_ARGS(_materialHeap.ReleaseAndGetAddressOf()));
	if (FAILED(result)) {
		assert(SUCCEEDED(result));
		return result;
	}

	//『MaterialForHlslのCBV_DESC
	auto materialBuffSize = sizeof(MaterialForHlsl);
	materialBuffSize = (materialBuffSize + 0xff)&~0xff;
	D3D12_CONSTANT_BUFFER_VIEW_DESC matCBVDesc = {};
	matCBVDesc.BufferLocation = _materialBuff->GetGPUVirtualAddress(); //『_materialBuffは、マテリアル数 * MaterialForHlsl が入った Resource
	matCBVDesc.SizeInBytes = materialBuffSize; //『1つ分のマテリアル
	
	//『テクスチャx4のSRV_DESC
	D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
	srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;//後述
	srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE2D;//2Dテクスチャ
	srvDesc.Texture2D.MipLevels = 1;//ミップマップは使用しないので1

	//『ディスクリプタヒープ[_materialHeap]のHandleとincSize
	CD3DX12_CPU_DESCRIPTOR_HANDLE matDescHeapH(_materialHeap->GetCPUDescriptorHandleForHeapStart());
	auto incSize = _dx12.Device()->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

	//『ディスクリプタヒープ[_materialHeap]に、全てのマテリアル[_materials[]と～Resources[]]の5要素(定数[CBV]1つ、テクスチャ[SRV]4つ)を詰めていく==============================
	for (int i = 0; i < _materials.size(); ++i) {
		//マテリアル固定バッファビュー
		_dx12.Device()->CreateConstantBufferView(&matCBVDesc, matDescHeapH); //『Resource[_materialBuff]から1つ分のMaterialForHlslのビューをmatDescHeapHに入れる
		matCBVDesc.BufferLocation += materialBuffSize; //『次のMaterialForHlslにポインタを移動する★
		matDescHeapH.ptr += incSize; //『ディスクリプタヒープの次の位置に移動
		if (_textureResources[i] == nullptr) { //『_whiteTex
			srvDesc.Format = _renderer._whiteTex->GetDesc().Format;
			_dx12.Device()->CreateShaderResourceView(_renderer._whiteTex.Get(), &srvDesc, matDescHeapH);
		}
		else { //『_texture
			srvDesc.Format = _textureResources[i]->GetDesc().Format;
			_dx12.Device()->CreateShaderResourceView(_textureResources[i].Get(), &srvDesc, matDescHeapH);
		}
		matDescHeapH.Offset(incSize); //『Offset?

		if (_sphResources[i] == nullptr) { //『_whiteTex
			srvDesc.Format = _renderer._whiteTex->GetDesc().Format;
			_dx12.Device()->CreateShaderResourceView(_renderer._whiteTex.Get(), &srvDesc, matDescHeapH);
		}
		else { //『_sph
			srvDesc.Format = _sphResources[i]->GetDesc().Format;
			_dx12.Device()->CreateShaderResourceView(_sphResources[i].Get(), &srvDesc, matDescHeapH);
		}
		matDescHeapH.ptr += incSize; //『ディスクリプタヒープの次の位置に移動

		if (_spaResources[i] == nullptr) { //『_blackTex
			srvDesc.Format = _renderer._blackTex->GetDesc().Format;
			_dx12.Device()->CreateShaderResourceView(_renderer._blackTex.Get(), &srvDesc, matDescHeapH);
		}
		else { //『_spa
			srvDesc.Format = _spaResources[i]->GetDesc().Format;
			_dx12.Device()->CreateShaderResourceView(_spaResources[i].Get(), &srvDesc, matDescHeapH);
		}
		matDescHeapH.ptr += incSize; //『ディスクリプタヒープの次の位置に移動


		if (_toonResources[i] == nullptr) { //『_gradTex
			srvDesc.Format = _renderer._gradTex->GetDesc().Format;
			_dx12.Device()->CreateShaderResourceView(_renderer._gradTex.Get(), &srvDesc, matDescHeapH);
		}
		else { //『_toon
			srvDesc.Format = _toonResources[i]->GetDesc().Format;
			_dx12.Device()->CreateShaderResourceView(_toonResources[i].Get(), &srvDesc, matDescHeapH);
		}
		matDescHeapH.ptr += incSize; //『ディスクリプタヒープの次の位置に移動
	}
}


//『モデル変換行列_mappedTransform->worldを更新し、Y軸を中心に回転する
void PMDActor::Update() {
	_angle += 0.03f;
	//『Y軸を中心に回転する//『Transform* _mappedTransform。マップ先は_transformBuff。ビューは_transformHeap
	_mappedTransform->world =  XMMatrixRotationY(_angle);
}

//『VBV,IBVをパイプラインに設定。モデル変換行列(CBV)をルートパラメータに設定。マテリアルのハンドル.ptrと頂点範囲[m.indicesNum]を切り替えながらドローコール
void PMDActor::Draw() {
	//『VBVとIBVをパイプラインに設定=====================================================================================================================
	//『Vertexバッファビュー(VBV)を設定 //『StartSlotはそのスロットからNumViews分、pViews[]が順番に入っていく]
	_dx12.CommandList()->IASetVertexBuffers(0/*StartSlot*/, 1/*NumViews*/, &_vbView/*pViews*/);
	//『Indexバッファビュー(IBV)を設定
	_dx12.CommandList()->IASetIndexBuffer(&_ibView/*pView*/);

	//『モデル変換行列のConstantバッファビュー(CBV)のディスクリプタヒープを設定==============================================================================
	ID3D12DescriptorHeap* transheaps[] = {_transformHeap.Get()};
	_dx12.CommandList()->SetDescriptorHeaps(1, transheaps); //『毎回、このAPIの存在意義がよく分からない
	_dx12.CommandList()->SetGraphicsRootDescriptorTable(1/*RootParameterIndex*/, _transformHeap->GetGPUDescriptorHandleForHeapStart());

	//『indicesNum毎のマテリアルが入った_materialHeap(マテリアル数 x (CBVx1, SRVx4))を切り替えながらドローコール(DrawIndexedInstanced(..))する================
	ID3D12DescriptorHeap* mdh[] = { _materialHeap.Get() };
	//『ディスクリプタヒープをパイプラインに設定
	_dx12.CommandList()->SetDescriptorHeaps(1, mdh);
	//『ディスクリプタヒープのGPUハンドル取得
	auto materialH = _materialHeap->GetGPUDescriptorHandleForHeapStart();
	//『indicesNumのオフセット値を定義&初期化
	unsigned int idxOffset = 0;
	//『ディスクリプタヒープのGPUハンドル[materialH]のインクリメントサイズを定義
	auto cbvsrvIncSize = _dx12.Device()->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV) * 5;
	//『ドローコールマテリアルループ=================================================
	for (auto& m : _materials) {
		//『ルートパラメータの2番にディスクリプタヒープ[materialH]をパイプラインに設定
		_dx12.CommandList()->SetGraphicsRootDescriptorTable(2/*RootParameterIndex*/, materialH/*BaseDescriptor[ディスクリプタヒープ]*/);
		//『idxOffset～m.indicesNumまでの頂点をドローコール
		_dx12.CommandList()->DrawIndexedInstanced(m.indicesNum/*IndexCountPerInstance*/, 1, idxOffset/*StartIndexLocation*/, 0, 0);
		//『ディスクリプタヒープ[materialH]の.ptr を cbvsrvIncSize[D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV * 5] でインクリメント
		materialH.ptr += cbvsrvIncSize;
		//『次の頂点範囲の下限をm.indicesNumで設定
		idxOffset += m.indicesNum;
	}

}