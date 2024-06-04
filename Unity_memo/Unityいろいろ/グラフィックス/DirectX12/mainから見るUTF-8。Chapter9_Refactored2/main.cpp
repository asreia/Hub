#include"Application.h"

#ifndef _DEBUG
int main() {
#else
#include<Windows.h>

//『やっていることは主に、Init()とRun()だけ
int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int) {
#endif
	auto& app = Application::Instance();
	if (!app.Init()) {
		return -1;
	}
	app.Run();
	app.Terminate(); //『Windows処理❰UnregisterClass(..)❱
	return 0;
}