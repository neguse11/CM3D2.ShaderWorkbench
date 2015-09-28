# ShaderCompiler

シェーダーを自動コンパイルするユーティリティー


## コンパイル

ルートディレクトリの[config.batを設定後、download.batを実行](../README.md)し、compile.batを実行してください

コンパイルに成功すると UnityInjector\Config\ShaderWorkbench\ShaderCompiler.exe が生成されます


## 使い方

 - UnityInjector\Config\ShaderWorkbench\ShaderCompiler.exe を実行
 - 実行後、ShaderCompiler.exe は自身が存在するディレクトリを監視します
 - 同ディレクトリにあるシェーダーファイル (.shader および .hlsl) が更新されると、自動的にコンパイルを行い、Unity側で解釈可能な状態にします

