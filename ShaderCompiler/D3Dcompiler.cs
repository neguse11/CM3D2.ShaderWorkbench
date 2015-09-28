// SharpDX.D3DCompiler wrapper
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class D3DCompiler
{
    public static Result Compile(Input input)
    {
        Result result = new Result();
        if (input == null)
        {
            return result;
        }

        if (!string.IsNullOrEmpty(input.FileName) && string.IsNullOrEmpty(input.Code))
        {
            //Console.WriteLine("input.FileName=<{0}>", input.FileName);
            input.Code = System.IO.File.ReadAllText(input.FileName);
        }

        if (string.IsNullOrEmpty(input.Code))
        {
            return result;
        }

        if (string.IsNullOrEmpty(input.BasePath))
        {
            if (!string.IsNullOrEmpty(input.FileName))
            {
                input.BasePath = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(input.FileName));
            }
            else
            {
                input.BasePath = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath("."));
            }
        }

        SharpDX.D3DCompiler.CompilationResult sdxResult = null;
        try
        {
            if (input.Include == null)
            {
                input.Include = new Include(
                    input.BasePath,
                    input.SpecialIncludeHandler,
                    (includeFilename) =>
                    {
                        result.DependentFiles.Add(includeFilename);
                    }
                );
            }

            //Console.WriteLine("input.Code=<{0}>", input.Code);
            sdxResult = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                input.Code,
                input.EntryPoint,
                input.Profile,
                input.ShaderFlag,
                input.EffectFlag,
                null,
                input.Include,
				input.FileName
            );
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
            result.Message += sb;
        }
        if (sdxResult != null)
        {
            result.Set(sdxResult);
        }
        return result;
    }

    // todo		Input, Result を扱うようにすること
    public static string Disassemble(string pseudoHex)
    {
        pseudoHex = PseudoHex.Filter(pseudoHex);
        byte[] bytes = PseudoHex.ToBytes(pseudoHex);
        return Disassemble(bytes);
    }

    public static string Disassemble(byte[] bytes)
    {
        return Disassemble(new SharpDX.D3DCompiler.ShaderBytecode(bytes));
    }

    public static string Disassemble(SharpDX.D3DCompiler.ShaderBytecode shaderByteCode)
    {
        SharpDX.D3DCompiler.DisassemblyFlags flags = SharpDX.D3DCompiler.DisassemblyFlags.None;
        return shaderByteCode.Disassemble(flags);
    }

    public class Input
    {
        public string FileName;
        public string EntryPoint;
        public string Profile;
        public string BasePath;
        public string Code;
        public byte[] Bytes;
        public Func<string, string> SpecialIncludeHandler;
        public Include Include;
        //		public int		OptimizationLevel = int.MaxValue;
        public SharpDX.D3DCompiler.ShaderFlags ShaderFlag = SharpDX.D3DCompiler.ShaderFlags.None;
        public SharpDX.D3DCompiler.EffectFlags EffectFlag = SharpDX.D3DCompiler.EffectFlags.None;
    }

    public class Result
    {
        public byte[] Bytecode { get; set; }
        public string Message { get; set; }
        public bool Ok { get; set; }
        public HashSet<string> DependentFiles { get; set; }

        public Result()
        {
            Bytecode = null;
            Message = "";
            Ok = false;
            DependentFiles = new HashSet<string>();
        }

        public void Set(SharpDX.D3DCompiler.CompilationResult sdxResult)
        {
            if (sdxResult != null)
            {
                SharpDX.D3DCompiler.ShaderBytecode shaderByteCode = sdxResult;// as SharpDX.D3DCompiler.ShaderBytecode;
                if (shaderByteCode != null)
                {
                    Bytecode = (byte[])shaderByteCode.Data.Clone();
                }
                Message += sdxResult.Message;
                Ok = (!sdxResult.HasErrors) && (sdxResult.ResultCode == SharpDX.Result.Ok);
            }
            else
            {
                Ok = false;
            }
        }
    }

    // http://svn.darwinbots.com/Darwinbots3/Trunk/Modules/UnitTestSharp/UnitTestSharpHLSL/UnitTestSharpHLSL/TestRunner.cs
    public class Include : SharpDX.D3DCompiler.Include
    {
        string basePath;
        Func<string, string> specialIncludeHandler = null;
        Action<string> callback;

        public Include(
            string basePath,
            Func<string, string> specialIncludeHandler,
            Action<string> callback
        )
        {
            this.basePath = basePath;
            this.specialIncludeHandler = specialIncludeHandler;
            this.callback = callback;
        }

        public IDisposable Shadow { get; set; }

        public void Dispose()
        {
            Shadow.Dispose();
        }

        public void Close(Stream stream)
        {
            stream.Close();
            stream.Dispose();
        }

        public Stream Open(SharpDX.D3DCompiler.IncludeType type, string fileName, Stream parentStream)
        {
            if (specialIncludeHandler != null)
            {
                string s = specialIncludeHandler(fileName);
                if (s != null)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    StreamWriter sw = new StreamWriter(memoryStream);
                    sw.Write(s);
                    sw.Flush();
                    memoryStream.Position = 0;
                    return memoryStream;
                }
            }

            var files = Directory.GetFiles(basePath, fileName, System.IO.SearchOption.AllDirectories);
            if (files.Count() == 0)
            {
                throw new Exception(String.Format("Cannot find a file named {0}", fileName));
            }
            else if (files.Count() != 1)
            {
                throw new Exception(String.Format("Found multiple files named {0}", fileName));
            }
            string file = files[0];
            callback(file);
            return new System.IO.StreamReader(file).BaseStream;
        }
    }
}
