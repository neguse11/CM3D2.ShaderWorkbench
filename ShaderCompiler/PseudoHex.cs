using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class PseudoHex
{
    // byte[] -> string(pseudoHex)
    public static string FromBytes(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            if (i != 0 && (i % 32) == 0)
            {
                sb.Append("\n");
            }
            sb.Append((char)('a' + (char)((b >> 4) & 0x0f)));
            sb.Append((char)('a' + (char)((b >> 0) & 0x0f)));
        }
        return sb.ToString();
    }

    // string(pseudoHex) -> byte[]
    public static byte[] ToBytes(string pseudoHex)
    {
        var ms = new System.IO.MemoryStream();
        using (var bw = new System.IO.BinaryWriter(ms))
        {
            int s = -1;
            foreach (char c in pseudoHex)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                int d = c - 'a';
                if (d < 0 || d >= 16)
                {
                    break;
                }
                if (s == -1)
                {
                    s = d;
                }
                else
                {
                    bw.Write((byte)(s * 16 + d));
                    s = -1;
                }
            }
        }
        return ms.ToArray();
    }

    public static bool IsPseudoHex(string str)
    {
        foreach (char c in str)
        {
            if (char.IsWhiteSpace(c))
            {
                continue;
            }
            int d = c - 'a';
            if (d < 0 || d >= 16)
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsPseudoHexish(string str)
    {
        string dummy;
        return IsPseudoHexish(str, out dummy);
    }

    public static bool IsPseudoHexish(string str, out string profile)
    {
        return IsPseudoHex(Filter(str, out profile));
    }

    // https://msdn.microsoft.com/en-us/library/windows/desktop/bb509626.aspx
    static string[] ShaderProfiles = {
		// Shader Model 1
		"vs_1_1",

		// Shader Model 2
		"ps_2_0", "ps_2_x", "vs_2_0", "vs_2_x", "ps_4_0_level_9_0",
        "ps_4_0_level_9_1", "ps_4_0_level_9_3", "vs_4_0_level_9_0",
        "vs_4_0_level_9_1", "vs_4_0_level_9_3", "lib_4_0_level_9_1",
        "lib_4_0_level_9_3",

		// Shader Model 3
		"ps_3_0", "vs_3_0",

		// Shader Model 4
		"cs_4_0", "gs_4_0", "ps_4_0", "vs_4_0", "cs_4_1", "gs_4_1",
        "ps_4_1", "vs_4_1", "lib_4_0", "lib_4_1",

		// Shader Model 5
		"cs_5_0", "ds_5_0", "gs_5_0", "hs_5_0", "ps_5_0", "vs_5_0",
        "lib_5_0", "gs_4_0", "gs_4_1", "ps_4_0", "ps_4_1", "vs_4_0",
        "vs_4_1",
    };

    //
    public static string Filter(string pseudoHexish)
    {
        string dummy;
        return Filter(pseudoHexish, out dummy);
    }

    public static string Filter(string pseudoHexish, out string outProfile)
    {
        string s = pseudoHexish;
        outProfile = "";
        foreach (string profile in ShaderProfiles)
        {
            Match match = Regex.Match(s, profile);
            if (match.Success)
            {
                outProfile = profile;
                s = Regex.Replace(s, profile, "");
            }
        }
        s = Regex.Replace(s, "\"", "");
        s = Regex.Replace(s, @"\s", "");
        return s;
    }
}
