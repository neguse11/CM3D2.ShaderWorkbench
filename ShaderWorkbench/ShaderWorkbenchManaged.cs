using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.ShaderWorkbench.Managed
{
    namespace Callbacks
    {
        namespace ImportCM
        {
            public static class ReadMaterial
            {
                public delegate void Callback(BinaryReader r, TBodySkin bodyskin, Material existmat, Material retMat);
                public static Callbacks<Callback> Callbacks = new Callbacks<Callback>();

                public static void Invoke(BinaryReader r, TBodySkin bodyskin, Material existmat, Material retMat)
                {
                    try
                    {
                        foreach (Callback callback in Callbacks.Values)
                        {
                            callback(r, bodyskin, existmat, retMat);
                        }
                    }
                    catch (Exception ex)
                    {
                        DetailedException.Show(ex);
						throw;
                    }
                }
            }
        }

        public class Callbacks<T> : SortedDictionary<string, T> { }
    }
}
