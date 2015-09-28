using System;
using System.Diagnostics;
using System.IO;
using System.Text;

internal static class DetailedException
{
    public static void Show(Exception exception)
    {
        Show(exception, Console.Out);
    }

    public static void Show(Exception exception, TextWriter textWriter)
    {
        textWriter.WriteLine("{0}", MakeString(exception));
    }

    public static string MakeString(Exception exception)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0}\n", exception.Message);
        foreach (StackFrame f in (new StackTrace(exception, true)).GetFrames())
        {
            sb.AppendFormat("{0}({1}.{2}) : {3}\n", f.GetFileName(), f.GetFileLineNumber(), f.GetFileColumnNumber(), f.GetMethod());
        }
        return sb.ToString();
    }
}
