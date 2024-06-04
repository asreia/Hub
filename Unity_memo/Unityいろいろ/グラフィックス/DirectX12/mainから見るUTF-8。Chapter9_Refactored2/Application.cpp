#include "Application.h"
#include"Dx12Wrapper.h"
#include"PMDRenderer.h"
#include"PMDActor.h"

//ウィンドウ定数
const unsigned int window_width = 1280;
const unsigned int window_height = 720;

//面倒だけど書かなあかんやつ
LRESULT WindowProcedure(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
	if (msg == WM_DESTROY) {//ウィンドウが破棄されたら呼ばれます
		PostQuitMessage(0);//OSに対して「もうこのアプリは終わるんや」と伝える
		return 0;
	}
	return DefWindowProc(hwnd, msg, wparam, lparam);//規定の処理を行う
}

void 
Application::CreateGameWindow(HWND &hwnd, WNDCLASSEX &windowClass) {
	HINSTANCE hInst = GetModuleHandle(nullptr);
	//ウィンドウクラス生成＆登録
	windowClass.cbSize = sizeof(WNDCLASSEX);
	windowClass.lpfnWndProc = (WNDPROC)WindowProcedure;//コールバック関数の指定
	windowClass.lpszClassName = _T("DirectXTest");//アプリケーションクラス名(適当でいいです)
	windowClass.hInstance = GetModuleHandle(0);//ハンドルの取得
	RegisterClassEx(&windowClass);/*●*///アプリケーションクラス(こういうの作るからよろしくってOSに予告する) //『最後でapp.Terminate()=>UnregisterClass(..)が呼ばれる

	RECT wrc = { 0,0, window_width, window_height };//●ウィンドウサイズを決める
	AdjustWindowRect(&wrc, WS_OVERLAPPEDWINDOW, false);//ウィンドウのサイズはちょっと面倒なので関数を使って補正する
	//ウィンドウオブジェクトの生成
	hwnd = CreateWindow(windowClass.lpszClassName,//クラス名指定
		_T("DX12リファクタリング"),//タイトルバーの文字
		WS_OVERLAPPEDWINDOW,//タイトルバーと境界線があるウィンドウです
		CW_USEDEFAULT,//表示X座標はOSにお任せします
		CW_USEDEFAULT,//表示Y座標はOSにお任せします
		wrc.right - wrc.left,//ウィンドウ幅
		wrc.bottom - wrc.top,//ウィンドウ高
		nullptr,//親ウィンドウハンドル
		nullptr,//メニューハンドル
		windowClass.hInstance,//呼び出しアプリケーションハンドル
		nullptr);//追加パラメータ

}

SIZE
Application::GetWindowSize()const {
	SIZE ret;
	ret.cx = window_width;
	ret.cy = window_height;
	return ret;
}



void Application::Run() {
	float angle = 0.0f; //『使ってない
	unsigned int frame = 0; //『使ってない
	MSG msg = {};
	//ウィンドウ表示===============================================================================================================================
	ShowWindow(_hwnd, SW_SHOW);
	//『描画ループ=================================================================================================================================
	while (true) {
		//『Windowsの処理(どうでもいい)=============================================================================================================
		//『メッセージ処理
		if (PeekMessage(&msg, nullptr, 0, 0, PM_REMOVE)) {
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
		//もうアプリケーションが終わるって時にmessageがWM_QUITになる
		if (msg.message == WM_QUIT) {
			break; //『whileループを抜けて終了する
		}
		//『バックバッファ(RTV)Resourceのバリア と RTVとDSVのレンダーターゲット設定とクリア と ビューポートとシザーの設定=============================================
		_dx12->BeginDraw(); /*●*///全体の描画準備

		//『パイプラインステート設定、ルートシグネチャ設定、Primitiveトポロジー設定=================================================================================
		//『パイプラインステート設定//PMD用の描画パイプラインに合わせる
		_dx12->CommandList()->SetPipelineState(_pmdRenderer->GetPipelineState());
		//『ルートシグネチャ設定//ルートシグネチャもPMD用に合わせる
		_dx12->CommandList()->SetGraphicsRootSignature(_pmdRenderer->GetRootSignature());
		//『Primitiveトポロジー設定
		_dx12->CommandList()->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST); //『三角形リスト (他に三角形ストリップ(TRIANGLESTRIP)もある)

		//『カメラのCBVをパイプラインに設定=====================================================================================================================
		_dx12->SetScene();
		
		//『_pmdActor=========================================================================================================================================
		//『モデル変換行列_mappedTransform->worldを更新し、Y軸を中心に回転する
		_pmdActor->Update();
		//『VBV,IBVをパイプラインに設定。モデル変換行列(CBV)をルートパラメータに設定。マテリアルのハンドル.ptrと頂点範囲[m.indicesNum]を切り替えながらドローコール
		_pmdActor->Draw();/*●*/

		//『_dx12==============================================================================================================================================
		//『バックバッファのバリア、コマンドリスト命令のクローズ、コマンドリストをコマンドキューで実行、フェンスによる待ち、コマンド[リスト|アロケータ]のクリア
		_dx12->EndDraw();
		//『スワップチェインのフリップ
		_dx12->Swapchain()->Present(1/*SyncInterval*/, 0);
	}
}

bool 
Application::Init() {
	//『>呼び出し元のスレッドで使用する COM ライブラリを初期化し、スレッドのコンカレンシー モデルを設定し、必要に応じてスレッドの新しいアパートメントを作成します。
		//ComPtrの初期化処理?
	auto result = CoInitializeEx(0, COINIT_MULTITHREADED);
	CreateGameWindow(_hwnd, _windowClass); //『HWND hwnd の生成。このウインドウをOSに登録

	//DirectX12ラッパー生成＆初期化 //『下は上のモジュールを含む
	//『主に _dev, cmd系, _swapchain, _rtvHeaps, _dsvHeap, _sceneDescHeap, _fence
	_dx12.reset(new Dx12Wrapper(_hwnd)); //『URPのような
	//『主に RootSignature[rootParams[3], samplerDescs[2]], PipelineState, Resource[_white|_black|_grad]Tex
	_pmdRenderer.reset(new PMDRenderer(*_dx12)); //『Universal Rendererのような
	//『主に PMDのファイルパスから、ビュー{_vbView, _ibView} ディスクリプタヒープ{_transformHeap[_transform.world], _materialHeap[CBVx1,SRVx4]} と _materials[].indicesNum を生成
	_pmdActor.reset(new PMDActor("Model/初音ミク.pmd", *_pmdRenderer)); //『GameObject.Rendererのような

	return true;
}

void
Application::Terminate() {
	//もうクラス使わんから登録解除してや
	UnregisterClass(_windowClass.lpszClassName, _windowClass.hInstance);
}

Application& 
Application::Instance() {
	static Application instance; //『シングルトンのインスタンス。多分、フィールドにstaticメンバ持つのと同じ
	return instance;
}

Application::Application()
{
}


Application::~Application()
{
}