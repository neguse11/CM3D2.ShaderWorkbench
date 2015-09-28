# CM3D2.ShaderWorkbench

CM3D2のシェーダの書き換えを行う実験

HLSLでシェーダを書き換えて遊べます


## 前提

 - 1.10 までパッチを適用する
 - ReiPatcher, UnityInjector が動作している
 - .NET Framework 3.5 がインストールされている
 - Direct3D 11 モード（通常の動作モードです）でゲーム本体が動作している
 - 以下では「C:\KISS\CM3D2_KAIZOU」に改造版があるとして説明をします


## ダウンロードと設置

 - https://github.com/neguse11/CM3D2.ShaderWorkbench/archive/master.zip をダウンロード
 - master.zip を展開し、C:\KISS\CM3D2_KAIZOU\CM3D2.ShaderWorkbench-master\config.bat.txtが存在するようにコピーする


## 準備

 - config.bat.txt を config.bat にリネームし、config.bat内の「`CM3D2_VANILLA_DIR`」および「`CM3D2_MOD_DIR`」を適宜設定してください
 - compile-patch-and-go.bat を実行することで、ダウンロード、コンパイル、パッチ適用が行われ、その後 CM3D2x64.exe と ShaderCompiler.exe が実行されます


## 実験

 - ゲームを起動し、エディット画面へ移動
 - CM3D2_KAIZOU\UnityInjector\Config\ShaderWorkbench\ に生成された `ShaderCompiler.exe` を実行
 - CM3D2_KAIZOU\UnityInjector\Config\ShaderWorkbench\Toony_Lighted_ps.hlsl を開く

ファイル末尾の
```
    return mainTex;
```
を
```
    mainTex.x = 1.0;
    return mainTex;
```
に変更し、ファイルを保存

 - コンパイルが実行され、肌の色などが赤くなることを確認する
    - うまく更新されない場合は F12 を押すことで強制的にリロードも可能です


## 消したい

clean.bat を実行してください

手動で外すには以下の操作を行ってください

 - CM3D2_KAIZOU\CM3D2x64_Data\Managed\CM3D2.ShaderWorkbench.Managed.dll を削除
 - CM3D2_KAIZOU\ReiPatcher\Patches\CM3D2.ShaderWorkbench.Patcher.dll を削除
 - CM3D2_KAIZOU\UnityInjector\CM3D2.ShaderWorkbench.Plugin.dll を削除
 - CM3D2_KAIZOU\CM3D2x64_Data\Managed\Assembly-CSharp.dll をバニラのファイルで上書き
 - 以上を実行した後で、ReiPatcherを実行
