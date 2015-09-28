このディレクトリに SharpDX の DLL を配置してください

配置のためのコマンドの例

```
cd SharpDX
powershell -Command "(New-Object Net.WebClient).DownloadFile('http://sharpdx.org/upload/SharpDX-SDK-2.6.3.exe', 'SharpDX-SDK-2.6.3.exe')"
7z x -y SharpDX-SDK-2.6.3.exe Bin\DirectX11-net20\SharpDX.dll Bin\DirectX11-net20\SharpDX.D3DCompiler.dll
```
