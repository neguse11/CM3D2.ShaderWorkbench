using Mono.Cecil;
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using System.Diagnostics;

[assembly: AssemblyTitle("CM3D2.ShaderWorkbench.Patcher")]
[assembly: AssemblyVersion("0.1.0.0")]

namespace CM3D2.ShaderWorkbench.Patcher
{
    public class ShaderWorkbenchPatcher : ReiPatcher.Patch.PatchBase
    {
        string PatchTag { get { return Name + "_PATCHED"; } }

        public override bool CanPatch(ReiPatcher.Patch.PatcherArguments args)
        {
            return args.Assembly.Name.Name == "Assembly-CSharp" && !base.GetPatchedAttributes(args.Assembly).Any(a => a.Info == PatchTag);
        }

        public override void PrePatch()
        {
            ReiPatcher.RPConfig.RequestAssembly("Assembly-CSharp.dll");
        }

        public override void Patch(ReiPatcher.Patch.PatcherArguments args)
        {
            try
            {
                AssemblyDefinition ta = args.Assembly;
                AssemblyDefinition da = PatcherHelper.GetAssemblyDefinition(args, "CM3D2.ShaderWorkbench.Managed.dll");

                PatcherHelper.SetHook(
                    PatcherHelper.HookType.PostCallRet,
                    ta, "ImportCM.ReadMaterial",
                    da, "CM3D2.ShaderWorkbench.Managed.Callbacks.ImportCM.ReadMaterial.Invoke");

				SetPatchedAttribute(args.Assembly, PatchTag);
            }
            catch (Exception e)
            {
                DetailedException.Show(e);
                throw;
            }
        }
    }
}
