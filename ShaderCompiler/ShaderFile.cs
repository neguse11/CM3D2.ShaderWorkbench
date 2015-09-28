// todo あまりにひどいので、書き直すこと

// http://aras-p.info/texts/files/201301%20Shader%20Pipeline%20in%20Unity.pdf
// https://gist.github.com/aras-p/4b198ffbd627454284d6
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SimpleLexer;

class ShaderFile
{
    public delegate string CompilerFunc(CompilerSettings compilerSettings);

    public class CompilerSettings
    {
        public CompilerFunc CompilerFunc;
        public string ShaderName;
        public Dictionary<string, string> SubShaderTags;
        public string PassName;
        public Dictionary<string, string> PassTags;
        public string ProgramName;
        public string SubProgramName;
        public HashSet<string> SubProgramKeywords;

        public string SubProgramPredefinedHeader;

        public string Profile;
        public string EntryPoint;
        public string SourceFileName;
    }

    public static class TokenType
    {
        public static int None = 0;
        public static int WhiteSpace = 1;
        public static int Equal = 2;
        public static int Comma = 3;
        public static int OpenParen = 4;
        public static int CloseParen = 5;
        public static int OpenBrace = 6;
        public static int CloseBrace = 7;
        public static int OpenBracket = 8;
        public static int CloseBracket = 9;
        public static int Identifier = 10;
        public static int Number = 11;
        public static int InlineComment = 12;
        public static int Literal = Token.LiteralType;
    }

    public abstract class Element
    {
        public Element Parent = null;
        public List<Element> Children = new List<Element>();

        public string GetTypeName()
        {
            return this.GetType().Name;
        }

        public bool TryParse<T>(Predicate<Token> predicate, TokenQueue it) where T : Element, new()
        {
            if (!predicate(it.Current))
            {
                return false;
            }
            T t = new T();
            t.Parent = this;
            Children.Add(t);
            t.Parse(it);
            return true;
        }

        public abstract void Parse(TokenQueue it);

        public virtual void CompileShader(CompilerSettings compilerSettings)
        {
        }

        public abstract void Dump(StreamWriter sw, int level);

        public static bool ParseBlock(TokenQueue it, Action error, Func<TokenQueue, bool> func)
        {
            Get(it, TokenType.OpenBrace);
            bool done = false;
            while (it.MoveNext())
            {
                Token t = it.Current;
                if (t.Is(TokenType.CloseBrace))
                {
                    done = true;
                    break;
                }
                if (func(it))
                {
                    continue;
                }
                throw new Exception(string.Format("Illegal Command : Line({0}), Token='{1}'", t.Position.Line, t.Value));
            }
            if (!done)
            {
                error();
            }
            return done;
        }

        public void DumpChildren(StreamWriter sw, int level)
        {
            foreach (Element e in Children)
            {
                e.Dump(sw, level + 1);
            }
        }

        public static void Indent(StreamWriter sw, int level)
        {
            sw.Write("".PadLeft(level * 4));
        }

        public static void Write(StreamWriter sw, int level, string format, params object[] args)
        {
            Indent(sw, level);
            Write(sw, format, args);
        }

        public static void Write(StreamWriter sw, string format, params object[] args)
        {
            sw.WriteLine(format, args);
        }

        public static void Write_(StreamWriter sw, int level, string format, params object[] args)
        {
            Indent(sw, level);
            Write_(sw, format, args);
        }

        public static void Write_(StreamWriter sw, string format, params object[] args)
        {
            sw.Write(format, args);
        }
    }

    public class Shader : Element
    {
        public string Name;

        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Shader");
        }

        public override void Parse(TokenQueue tq)
        {
            Get(tq, TokenType.Literal, out Name);
            Name = TrimQuotes(Name);
            ParseBlock(
                tq,
                () => { throw new Exception("Unexpected EOF (Shader)"); },
                (TokenQueue it) =>
                {
                    return TryParse<SubShader>(SubShader.IsMatch, it) ||
                        TryParse<Properties>(Properties.IsMatch, it) ||
                        TryParse<Fallback>(Fallback.IsMatch, it);
                }
            );
        }

        public override void CompileShader(CompilerSettings compilerSettings)
        {
            compilerSettings.ShaderName = Name;
            foreach (Element e in Children)
            {
                e.CompileShader(compilerSettings);
            }
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "Shader \"{0}\" {{", Name);
            DumpChildren(sw, level);
            Write_(sw, level, "}}");
        }
    }

    public class Properties : Element
    {
        List<Token> tokens = new List<Token>();

        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Properties");
        }

        public override void Parse(TokenQueue tq)
        {
            ParseBlock(
                tq,
                () => { throw new Exception("Unexpected EOF (Properties)"); },
                (TokenQueue it) =>
                {
                    int level = 0;
                    it.MovePrev();
                    while (it.MoveNext())
                    {
                        Token t = it.Current;
                        tokens.Add(t);
                        if (t.Is(TokenType.OpenBrace))
                        {
                            ++level;
                            continue;
                        }
                        if (t.Is(TokenType.CloseBrace))
                        {
                            --level;
                            if (level < 0)
                            {
                                tokens.RemoveAt(tokens.Count - 1);
                                tq.MovePrev();
                                break;
                            }
                            continue;
                        }
                    }
                    if (level >= 0)
                    {
                        throw new Exception("Unexpected EOF (Properties)");
                    }
                    return true;
                }
            );
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write_(sw, level, "{0} {{", GetTypeName());
            bool prevb = false;
            foreach (Token t in tokens)
            {
                bool newline = false;
                if (t.Type == TokenType.Identifier && t.Value[0] == '_')
                {
                    if (prevb)
                    {
                        prevb = false;
                    }
                    else
                    {
                        newline = true;
                    }
                }
                if (t.Type == TokenType.OpenBracket)
                {
                    newline = true;
                    prevb = true;
                }

                if (newline)
                {
                    Write(sw, "");
                    Write_(sw, level + 1, "{0}", t.Value);
                }
                else
                {
                    Write_(sw, "{0}", t.Value);
                }
            }
            DumpChildren(sw, level);
            Write(sw, "");
            Write(sw, level, "}}");
        }
    }

    public class SubShader : Element
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "SubShader");
        }

        public override void Parse(TokenQueue tq)
        {
            ParseBlock(
                tq,
                () => { throw new Exception("Unexpected EOF (SubShader)"); },
                (TokenQueue it) =>
                {
                    return TryParse<LOD>(LOD.IsMatch, it) ||
                            TryParse<Tags>(Tags.IsMatch, it) ||
                            TryParse<Pass>(Pass.IsMatch, it) ||
                            TryParse<Fallback>(Fallback.IsMatch, it) ||
                            TryParse<UsePass>(UsePass.IsMatch, it);
                }
            );
        }

        public override void CompileShader(CompilerSettings compilerSettings)
        {
            //	Tags
            foreach (Element e in Children)
            {
                if (e is Tags)
                {
                    var t = (Tags)e;
                    compilerSettings.SubShaderTags = t.Attributes;
                }
            }

            //	Pass
            foreach (Element e in Children)
            {
                if (e is Pass)
                {
                    e.CompileShader(compilerSettings);
                }
            }
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "SubShader {{");
            DumpChildren(sw, level);
            Write(sw, level, "}}");
        }
    }

    public class SingleValueBase : Element
    {
        public Token[] tokens = new Token[1];

        public override void Parse(TokenQueue it)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                if (it.MoveNext())
                {
                    tokens[i] = it.Current;
                }
            }
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write_(sw, level, "{0}", GetTypeName());
            foreach (Token t in tokens)
            {
                Write_(sw, " {0}", t.Value);
            }
            Write(sw, "");
            DumpChildren(sw, level);
        }
    }

    public class DoubleValueBase : Element
    {
        public Token[] tokens = new Token[2];

        public override void Parse(TokenQueue it)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                if (it.MoveNext())
                {
                    tokens[i] = it.Current;
                }
            }
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write_(sw, level, "{0}", GetTypeName());
            foreach (Token t in tokens)
            {
                Write_(sw, " {0}", t.Value);
            }
            Write(sw, "");
            DumpChildren(sw, level);
        }
    }

    public class LOD : SingleValueBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "LOD");
        }
    }

    public class Cull : SingleValueBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Cull");
        }
    }

    public class ZWrite : SingleValueBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "ZWrite");
        }
    }

    public class ColorMask : SingleValueBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "ColorMask");
        }
    }

    public class Blend : DoubleValueBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Blend");
        }
    }

    public class AlphaTest : DoubleValueBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "AlphaTest");
        }
    }

    public class TagsBase : Element
    {
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();

        public override void Parse(TokenQueue tq)
        {
            ParseBlock(
                tq,
                () => { throw new Exception("Unexpected EOF (Tags)"); },
                (TokenQueue it) =>
                {
                    Token t = it.Current;
                    if (t.Is(TokenType.Literal))
                    {
                        string key = t.Value;
                        string value;
                        Get(it, TokenType.Equal);
                        Get(it, TokenType.Literal, out value);
                        key = TrimQuotes(key);
                        value = TrimQuotes(value);
                        Attributes[key] = value;
                        return true;
                    }
                    return false;
                }
            );
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write_(sw, level, "{0} {{", GetTypeName());
            foreach (KeyValuePair<string, string> entry in Attributes)
            {
                Write_(sw, " \"{0}\"=\"{1}\"", entry.Key, entry.Value);
            }
            Write(sw, " }}");
            DumpChildren(sw, level);
        }
    }

    public class Tags : TagsBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Tags");
        }
    }

    public class Pass : Element
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Pass");
        }

        public override void Parse(TokenQueue tq)
        {
            ParseBlock(
                tq,
                () => { throw new Exception("Unexpected EOF (Pass)"); },
                (TokenQueue it) =>
                {
                    return TryParse<Name>(Name.IsMatch, it) ||
                            TryParse<Tags>(Tags.IsMatch, it) ||
                            TryParse<Cull>(Cull.IsMatch, it) ||
                            TryParse<ZWrite>(ZWrite.IsMatch, it) ||
                            TryParse<ColorMask>(ColorMask.IsMatch, it) ||
                            TryParse<Blend>(Blend.IsMatch, it) ||
                            TryParse<AlphaTest>(AlphaTest.IsMatch, it) ||
                            TryParse<Program>(Program.IsMatch, it);
                }
            );
        }

        public override void CompileShader(CompilerSettings compilerSettings)
        {
            //	Name
            foreach (Element e in Children)
            {
                if (e is Name)
                {
                    var t = (Name)e;
                    compilerSettings.PassName = t.tokens[0].Value;
                }
            }

            //	Tags
            foreach (Element e in Children)
            {
                if (e is Tags)
                {
                    var t = (Tags)e;
                    compilerSettings.PassTags = t.Attributes;
                }
            }

            //	Program
            foreach (Element e in Children)
            {
                if (e is Program)
                {
                    e.CompileShader(compilerSettings);
                }
            }
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "Pass {{");
            DumpChildren(sw, level);
            Write(sw, level, "}}");
        }
    }

    public class Name : SingleValueBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Name");
        }
    }

    public class Fallback : SingleValueBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Fallback");
        }
    }

    public class UsePass : SingleValueBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "UsePass");
        }
    }

    public class Program : Element
    {
        public string Name;     // "vp" or "fp"

        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Program");
        }

        public override void Parse(TokenQueue tq)
        {
            Get(tq, TokenType.Literal, out Name);
            Name = TrimQuotes(Name);
            ParseBlock(
                tq,
                () => { throw new Exception("Unexpected EOF (Program)"); },
                (TokenQueue it) =>
                {
                    return TryParse<SubProgram>(SubProgram.IsMatch, it);
                }
            );
        }

        public override void CompileShader(CompilerSettings compilerSettings)
        {
            //	Name
            compilerSettings.ProgramName = Name;

            foreach (Element e in Children)
            {
                e.CompileShader(compilerSettings);
            }
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "Program \"{0}\" {{", Name);
            DumpChildren(sw, level);
            Write(sw, level, "}}");
        }
    }

    public class SubProgram : Element
    {
        public string Name;         // profile "opengl ", "d3d11 ", etc

        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "SubProgram");
        }

        public override void Parse(TokenQueue tq)
        {
            Get(tq, TokenType.Literal, out Name);
            Name = TrimQuotes(Name);
            ParseBlock(
                tq,
                () => { throw new Exception("Unexpected EOF (SubProgram)"); },
                (TokenQueue it) =>
                {
                    Token t = it.Current;
                    return
                    TryParse<ShaderAsm>(ShaderAsm.IsMatch, it) ||
                    TryParse<Keywords>(Keywords.IsMatch, it) ||
                    TryParse<Bind>(Bind.IsMatch, it) ||
                    TryParse<ConstBuffer>(ConstBuffer.IsMatch, it) ||
                    TryParse<Matrix>(Matrix.IsMatch, it) ||
                    TryParse<Vector>(Vector.IsMatch, it) ||
                    TryParse<Float>(Float.IsMatch, it) ||
                    TryParse<SetTexture>(SetTexture.IsMatch, it) ||
                    TryParse<BindCB>(BindCB.IsMatch, it) ||
                    TryParse<CompilerArgument>(CompilerArgument.IsMatch, it);
                }
            );
        }

        public override void CompileShader(CompilerSettings compilerSettings)
        {
            //	Name
            compilerSettings.SubProgramName = Name;

            //	Keywords
            foreach (Element e in Children)
            {
                if (e is Keywords)
                {
                    var t = (Keywords)e;
                    compilerSettings.SubProgramKeywords = t.Attributes;
                }
            }

            compilerSettings.SubProgramPredefinedHeader = MakeHeader();

            //	CompilerArgument
            foreach (Element e in Children)
            {
                if (e is CompilerArgument)
                {
                    var t = (CompilerArgument)e;
                    t.CompileShader(compilerSettings);
                }
            }
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "SubProgram \"{0}\" {{", Name);
            DumpChildren(sw, level);
            Write(sw, level, "}}");
        }

        public string MakeHeader()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            // ConstBuffer, Vector, Matrix, Float, BindCB
            {
                Dictionary<string, string> constBufferDefinitions = new Dictionary<string, string>();
                string currentConstBufferName = "";
                Action<string> addHeader = (str) =>
                {
                    if (!string.IsNullOrEmpty(currentConstBufferName))
                    {
                        constBufferDefinitions[currentConstBufferName] += str;
                    }
                };
                foreach (Element element in Children)
                {
                    if (element is ConstBuffer)
                    {
                        ConstBuffer constBuffer = (ConstBuffer)element;
                        currentConstBufferName = constBuffer.Name;
                        constBufferDefinitions[currentConstBufferName] = "";
                        addHeader("{\n");
                        continue;
                    }
                    if (element is Float)
                    {
                        Float v = (Float)element;
                        int vn = int.Parse(v.Number);
                        int vn16 = vn / 16;
                        int vn4 = (vn / 4) % 4;
                        char component = (char)('x' + vn4);
                        addHeader(string.Format("\tfloat {0} : packoffset(c{1}.{2});\n", v.Identifier, vn16, component));
                        continue;
                    }
                    if (element is Matrix)
                    {
                        Matrix v = (Matrix)element;
                        addHeader(string.Format("\tmatrix {0} : packoffset(c{1});\n", v.Identifier, int.Parse(v.Number) / 16));
                        continue;
                    }
                    if (element is Vector)
                    {
                        Vector v = (Vector)element;
                        addHeader(string.Format("\tfloat{0} {1} : packoffset(c{2});\n", v.GetComponentCount(), v.Identifier, int.Parse(v.Number) / 16));
                        continue;
                    }
                    if (element is BindCB)
                    {
                        BindCB v = (BindCB)element;
                        string s = constBufferDefinitions[v.Name];
                        s = string.Format(
                            "cbuffer {0} : register(b{1})\n{2}}};\n",
                            FilterVariableName(TrimQuotes(v.Name)), v.Number, s);
                        constBufferDefinitions[v.Name] = s;
                        continue;
                    }
                }
                foreach (KeyValuePair<string, string> kv in constBufferDefinitions)
                {
                    result[kv.Key] = kv.Value;
                }
            }

            // SetTexture
            foreach (Element element in Children)
            {
                if (element is SetTexture)
                {
                    SetTexture v = (SetTexture)element;
                    string s = "";
                    s += string.Format(
                        "Texture{0}\t{1} : register(t{2});\n",
                        v.TextureType,
                        v.Identifier,
                        v.Number);
                    s += string.Format(
                        "SamplerState\t{0}_Sampler : register(s{1});\n",
                        v.Identifier,
                        v.SamplerState);
                    s += string.Format(
                        "#define\t{0}_Sample(x) {0}.Sample({0}_Sampler,x)\n",
                        v.Identifier);
                    result[v.Identifier] = s;
                }
            }

            {
                string s = "";
                foreach (KeyValuePair<string, string> kv in result)
                {
                    s += kv.Value;
                }
                return s;
            }
        }
    }

    public class ShaderAsm : Element
    {
        public string Code;

        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Literal);
        }

        public override void Parse(TokenQueue tq)
        {
            tq.MovePrev();
            Get(tq, TokenType.Literal, out Code);
            Code = TrimQuotes(Code);
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "\"{0}\"", Code);
            DumpChildren(sw, level);
        }
    }

    public class Keywords : Element
    {
        public HashSet<string> Attributes = new HashSet<string>();

        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Keywords");
        }

        public override void Parse(TokenQueue tq)
        {
            ParseBlock(
                tq,
                () => { throw new Exception("Unexpected EOF (SubProgram)"); },
                (TokenQueue it) =>
                {
                    Token t = it.Current;
                    if (t.Is(TokenType.Literal))
                    {
                        string s = TrimQuotes(t.Value);
                        Attributes.Add(s);
                        return true;
                    }
                    return false;
                }
            );
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write_(sw, level, "Keywords {{");
            foreach (string entry in Attributes)
            {
                Write_(sw, " \"{0}\"", entry);
            }
            Write(sw, " }}");
            DumpChildren(sw, level);
        }
    }

    public class CompilerArgument : TagsBase
    {
        public string Result = "";

        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "__CompilerArgument__");
        }

        public override void CompileShader(CompilerSettings compilerSettings)
        {
            compilerSettings.Profile = Attributes["profile"];
            compilerSettings.EntryPoint = Attributes["entrypoint"];
            compilerSettings.SourceFileName = Attributes["source"];
            Result = compilerSettings.CompilerFunc(compilerSettings);
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "{0}", Result);
            DumpChildren(sw, level);
        }
    }

    public class Bind : Element
    {
        public string Name;
        public string Identifier;

        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Bind");
        }

        public override void Parse(TokenQueue it)
        {
            Get(it, TokenType.Literal, out Name);
            Get(it, TokenType.Identifier, out Identifier);
            Name = TrimQuotes(Name);
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "{0} \"{1}\" {2}", GetTypeName(), Name, Identifier);
            DumpChildren(sw, level);
        }
    }

    public class ConstBufferBase : Element
    {
        public string Name;
        public string Number;

        public override void Parse(TokenQueue it)
        {
            Get(it, TokenType.Literal, out Name);
            Get(it, TokenType.Number, out Number);
            Name = TrimQuotes(Name);
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "{0} \"{1}\" {2}", GetTypeName(), Name, Number);
            DumpChildren(sw, level);
        }
    }

    public class ConstBuffer : ConstBufferBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "ConstBuffer");
        }
    }

    public class BindCB : ConstBufferBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier) && t.Value == "BindCB";
        }
    }

    public class VectorBase : Element
    {
        public string Number;
        public string Identifier;
        public string ComponentCount;

        public override void Parse(TokenQueue it)
        {
            Get(it, TokenType.Number, out Number);
            Get(it, TokenType.OpenBracket);
            Get(it, TokenType.Identifier, out Identifier);
            Get(it, TokenType.CloseBracket);
            TryGet(it, TokenType.Number, out ComponentCount);
        }

        public string GetComponentCount()
        {
            if (string.IsNullOrEmpty(ComponentCount))
            {
                return "4";
            }
            else
            {
                return ComponentCount;
            }
        }

        public override void Dump(StreamWriter sw, int level)
        {
            if (string.IsNullOrEmpty(ComponentCount))
            {
                Write(sw, level, "{0} {1} [{2}]", GetTypeName(), Number, Identifier);
            }
            else
            {
                Write(sw, level, "{0} {1} [{2}] {3}", GetTypeName(), Number, Identifier, ComponentCount);
            }
            DumpChildren(sw, level);
        }
    }

    public class Vector : VectorBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Vector");
        }
    }

    public class Matrix : VectorBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Matrix");
        }
    }

    public class Float : VectorBase
    {
        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "Float");
        }
    }

    public class SetTexture : Element
    {
        public string Number;
        public string Identifier;
        public string TextureType;
        public string SamplerState;

        public static bool IsMatch(Token t)
        {
            return t.Is(TokenType.Identifier, "SetTexture");
        }

        public override void Parse(TokenQueue it)
        {
            Get(it, TokenType.Number, out Number);
            Get(it, TokenType.OpenBracket);
            Get(it, TokenType.Identifier, out Identifier);
            Get(it, TokenType.CloseBracket);
            Get(it, TokenType.Identifier, out TextureType);
            Get(it, TokenType.Number, out SamplerState);
        }

        public override void Dump(StreamWriter sw, int level)
        {
            Write(sw, level, "SetTexture {0} [{1}] {2} {3}", Number, Identifier, TextureType, SamplerState);
            DumpChildren(sw, level);
        }
    }

    public ShaderFile()
    {
    }

    public static bool Get(TokenQueue it, int type)
    {
        string dummy;
        return Get(it, type, out dummy);
    }

    public static bool Get(TokenQueue it, int type, out string value)
    {
        if (TryGet(it, type, out value))
        {
            return true;
        }
        throw new Exception(string.Format("Get : error (Line({0}))", it.Current.Position.Line));
    }

    public static bool TryGet(TokenQueue it, int type, out string value)
    {
        while (it.MoveNext())
        {
            Token t = it.Current;
            if (t.Type == (int)type)
            {
                value = t.Value;
                return true;
            }
            break;
        }
        value = null;
        it.MovePrev();
        return false;
    }

    public static string TrimQuotes(string str)
    {
        return str.Trim('\"');
    }

    public static string FilterVariableName(string str)
    {
        return str.Replace('$', '_');
    }

    public class TokenQueue
    {
        List<Token> Tokens = new List<Token>();
        int index = -1;

        public Token Current
        {
            get
            {
                if (IsValid())
                {
                    return Tokens[index];
                }
                return null;
            }
        }

        public TokenQueue(IEnumerable<Token> tokens)
        {
            foreach (Token t in tokens)
            {
                Tokens.Add(t);
            }
        }

        public bool IsValid()
        {
            return index >= 0 && index < Tokens.Count();
        }

        public bool MoveNext()
        {
            if (index >= Tokens.Count())
            {
                return false;
            }
            index += 1;
            return IsValid();
        }

        public bool MovePrev()
        {
            if (index < 0)
            {
                return false;
            }
            index -= 1;
            return IsValid();
        }
    }

    public static Shader Parse(string str)
    {
        var lexer = new Lexer();

        Action<int, string> Def = (name, pattern) =>
        {
            lexer.AddDefinition(new TokenDefinition((int)name, new Regex(pattern)));
        };

        Action<int, string> DefIgnore = (name, pattern) =>
        {
            lexer.AddDefinition(new TokenDefinition((int)name, new Regex(pattern), true));
        };

        Action<int, char> DefChar = (name, pattern) =>
        {
            lexer.AddDefinition(new TokenDefinition((int)name, pattern));
        };

        Action<int, char> DefSpaceChar = (name, pattern) =>
        {
            lexer.AddDefinition(new TokenDefinition((int)name, pattern, true));
        };

        DefSpaceChar(TokenType.WhiteSpace, '\t');
        DefSpaceChar(TokenType.WhiteSpace, ' ');
        DefSpaceChar(TokenType.WhiteSpace, '\n');
        DefSpaceChar(TokenType.WhiteSpace, '\r');
        DefChar(TokenType.Equal, '=');
        DefChar(TokenType.Comma, ',');
        DefChar(TokenType.OpenParen, '(');
        DefChar(TokenType.CloseParen, ')');
        DefChar(TokenType.OpenBrace, '{');
        DefChar(TokenType.CloseBrace, '}');
        DefChar(TokenType.OpenBracket, '[');
        DefChar(TokenType.CloseBracket, ']');

        Def(TokenType.Identifier, @"2D");                       // (number)よりも上にすること
        Def(TokenType.Identifier, @"3D");                       // (number)よりも上にすること
        Def(TokenType.Identifier, @"[_A-Za-z][_A-Za-z0-9]*");
        Def(TokenType.Number, @"[+-]?[0-9]*\.?[0-9]+");
        DefIgnore(TokenType.InlineComment, @"//.*");
        // Def(TokenType.Literal, @"""[^""]*""");
        Def(TokenType.Literal, @"");

        IEnumerable<Token> tokens = lexer.Tokenize(str);
        TokenQueue tokenQueue = new TokenQueue(tokens);

        var shader = new Shader();
        if (tokenQueue.MoveNext())
        {
            shader.Parse(tokenQueue);
        }
        return shader;
    }

    public static Shader CompileShader(string shaderSource, CompilerFunc compilerFunc)
    {
        Shader shader = Parse(shaderSource);
        var compilerSettings = new CompilerSettings();
        compilerSettings.CompilerFunc = compilerFunc;
        shader.CompileShader(compilerSettings);
        return shader;
    }
}
