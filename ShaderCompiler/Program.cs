// SharpDX.dll, SharpDX.D3DCompiler.dll を配置してからコンパイル、実行すること
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ShaderCompiler
{
    class Program
    {
        static string targetPath = ".";
        static string BasePath = ".";

        static HashSet<string> targetExtensions = new HashSet<string> {
            ".hlsl", ".shader"
        };

        static Dictionary<string, DateTime> lastWriteTimes = new Dictionary<string, DateTime>();

        // HLSLファイル名でルックアップすると、そのファイルを含むshaderファイルを得る
        static Dictionary<string, HashSet<string>> ownerFilesDict = new Dictionary<string, HashSet<string>>();

        static void Main(string[] args)
        {
            BasePath = Path.GetFullPath(".");
            System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher();
            Action<object, System.IO.FileSystemEventArgs> eventHandler = (obj, e) =>
            {
                Refresh();
            };
            watcher.Path = targetPath;
            watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            watcher.Filter = "*.*";
            watcher.Created += new System.IO.FileSystemEventHandler(eventHandler);
            watcher.Deleted += new System.IO.FileSystemEventHandler(eventHandler);
            watcher.Changed += new System.IO.FileSystemEventHandler(eventHandler);
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press ESC to Exit");
            Refresh();
            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
            }
        }

        static void Refresh()
        {
            try
            {
                Refresh2();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("\nEXCEPTION : {0}\n", ex.Message);
                foreach (System.Diagnostics.StackFrame f in (new System.Diagnostics.StackTrace(ex, true)).GetFrames())
                {
                    sb.AppendFormat(
                        "{0}({1}.{2}) : {3}.{4}\n",
                        f.GetFileName(), f.GetFileLineNumber(), f.GetFileColumnNumber(),
                        f.GetMethod().DeclaringType, f.GetMethod()
                    );
                }
                Console.WriteLine("{0}", sb.ToString());
            }
        }

        static void Refresh2()
        {
            HashSet<string> targetShadres = new HashSet<string>();

            walkDir(targetPath, (filename) =>
            {
                filename = Path.GetFullPath(filename);

                if (!targetExtensions.Contains(Path.GetExtension(filename)))
                {
                    return;
                }
                DateTime t = File.GetLastWriteTime(filename);
                DateTime ot = DateTime.MinValue;
                lastWriteTimes.TryGetValue(filename, out ot);
                lastWriteTimes[filename] = t;
                if (t <= ot)
                {
                    return;
                }

                if (filename.EndsWith(".shader"))
                {
                    targetShadres.Add(filename);
                }
                else if (filename.EndsWith(".hlsl"))
                {
                    // shaderFiles : .shader files which contain HLSL (filename)
                    HashSet<string> shaderFiles;
                    if (ownerFilesDict.TryGetValue(filename, out shaderFiles))
                    {
                        targetShadres.UnionWith(shaderFiles);
                    }
                }
            });

            foreach (string shaderFilename in targetShadres)
            {
                CompileShader(shaderFilename, shaderFilename + ".out", BasePath);
            }
        }

        static void CompileShader(string inpFileName, string outFileName, string basePath)
        {
            Console.WriteLine(
                "{0} : Compile({1})",
                DateTime.Now.ToShortTimeString(),
                Path.GetFileName(inpFileName));
            string inputFile = File.ReadAllText(inpFileName);

            var shader = ShaderFile.CompileShader(inputFile, (compilerSettings) =>
            {
                string result = "";
                var input = new D3DCompiler.Input();
                input.FileName = Path.Combine(basePath, compilerSettings.SourceFileName);
                input.EntryPoint = compilerSettings.EntryPoint;
                input.Profile = compilerSettings.Profile;
                input.BasePath = basePath;
                input.SpecialIncludeHandler = (includeFileName) =>
                {
                    if (includeFileName == "__shader_predefined__")
                    {
                        return compilerSettings.SubProgramPredefinedHeader;
                    }
                    return null;
                };

                HashSet<string> shaderFiles;
                if (ownerFilesDict.TryGetValue(input.FileName, out shaderFiles))
                {
                    shaderFiles.Add(inpFileName);
                }
                else
                {
                    shaderFiles = new HashSet<string>();
                    shaderFiles.Add(inpFileName);
                    ownerFilesDict[input.FileName] = shaderFiles;
                }


                D3DCompiler.Result compilerResult = D3DCompiler.Compile(input);
                byte[] shaderBytes = compilerResult.Bytecode;

                if (shaderBytes == null)
                {
                    Console.WriteLine("ERROR\n{0}", compilerResult.Message);
                    Console.WriteLine("__shader_predefined__\n{0}", compilerSettings.SubProgramPredefinedHeader);
                }

                if (shaderBytes != null)
                {
                    result += string.Format("\n/*\n{0}\n*/\n", compilerSettings.SubProgramPredefinedHeader);
                    result += string.Format(
                        "\"{0}\n{1}\"",
                        compilerSettings.Profile,
                        PseudoHex.FromBytes(shaderBytes));
                }
                return result;
            });

            string output;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(memoryStream, Encoding.UTF8) { NewLine = "\n" };
                shader.Dump(sw, 0);
                sw.Flush();

                memoryStream.Position = 0;
                output = (new StreamReader(memoryStream)).ReadToEnd();
            }

            File.WriteAllText(outFileName, output);
        }

        static void walkDir(string path, Action<string> func)
        {
            var dirs = Directory.GetDirectories(path);
            foreach (string d in dirs)
            {
                walkDir(d, func);
            }
            foreach (string f in Directory.GetFiles(path))
            {
                func(f);
            }
        }
    }
}
