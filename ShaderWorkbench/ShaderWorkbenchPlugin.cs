/*
  http://forum.unity3d.com/threads/runtime-shader-compilation.87085/
  http://forum.unity3d.com/threads/reverse-engineering-unityshadercompiler-code-inside.289109/
  http://forum.kerbalspaceprogram.com/threads/109269-Custom-Shader-Loader-2-0-release
  http://kylehalladay.com/blog/tutorial/bestof/2014/01/12/Runtime-Shader-Compilation-Unity.html
 */
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.ShaderWorkbench.Plugin
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("Shader Workbench"),
    PluginVersion("0.1.0.0")]

    public class ShaderWorkbenchPlugin : PluginBase
    {
		static string ShaderBasePath;
		static readonly string ShaderExtension = ".shader.out";
		string ConfigReloadKey;

		static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
		System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher();
		float lastUpdateTime = 0f;
		float lastChanged = 0f;
		bool dirty = false;

		void Awake()
		{
			DontDestroyOnLoad(this);

			ConfigReloadKey = Preferences["Config"]["Reload"].Value ?? "f12";
			ShaderBasePath = Path.Combine(DataPath, "ShaderWorkbench");

			Action<object, System.IO.FileSystemEventArgs> onChanged = (source, e) =>
			{
				if(e.Name.EndsWith(ShaderExtension)) {
					lastChanged = lastUpdateTime;
					dirty = true;
				}
			};

			watcher.Path = ShaderBasePath;
			watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
			watcher.Filter = "*.*";
			watcher.Created += new System.IO.FileSystemEventHandler(onChanged);
			watcher.Deleted += new System.IO.FileSystemEventHandler(onChanged);
			watcher.Changed += new System.IO.FileSystemEventHandler(onChanged);
			watcher.EnableRaisingEvents = true;

			CM3D2.ShaderWorkbench.Managed.Callbacks.ImportCM.ReadMaterial.Callbacks[Name] = ImportCMReadMaterialCallback;
		}

        void Update()
        {
			lastUpdateTime = Time.realtimeSinceStartup;

			bool forceReload = false;
			if(dirty && Time.realtimeSinceStartup > lastChanged + 0.5f) {
				forceReload = true;
			}
			if (Input.GetKeyDown(ConfigReloadKey))
			{
				forceReload = true;
			}

			if(forceReload) {
				dirty = false;
				Reload();
			}
        }

		static void ImportCMReadMaterialCallback(BinaryReader r, TBodySkin bodyskin, Material existmat, Material retMat)
		{
			Material material = retMat;

			string shaderName = material.shader.name;
			if(! shaders.ContainsKey(shaderName)) {
				string shaderPath = GetShaderPath(shaderName);
				if(System.IO.File.Exists(shaderPath)) {
					string shaderCode = GetShaderCode(shaderPath);
					Material m = new Material(shaderCode);
					if(m.shader != null) {
						DontDestroyOnLoad(m.shader);
						shaders[shaderName] = m.shader;
					}
					Destroy(m);
				}
			}
			Shader newShader;
			if(shaders.TryGetValue(shaderName, out newShader)) {
				material.shader = newShader;
			}
		}

		static void ReplaceShader(Material material) {
			string shaderName = material.shader.name;
			if(! shaders.ContainsKey(shaderName)) {
				string shaderPath = GetShaderPath(shaderName);
				if(System.IO.File.Exists(shaderPath)) {
					string shaderCode = GetShaderCode(shaderPath);
					Material m = new Material(shaderCode);
					if(m.shader != null) {
						DontDestroyOnLoad(m.shader);
						shaders[shaderName] = m.shader;
					}
					Destroy(m);
				}
			}

			Shader newShader;
			if(shaders.TryGetValue(shaderName, out newShader)) {
				material.shader = newShader;
			}
		}

		void Reload()
		{
#if DEBUG
			Console.WriteLine("ShaderWorkbench : Reload");
#endif
			shaders.Clear();
			Material[] materials = UnityEngine.Object.FindObjectsOfType<Material>();
			foreach(Material material in materials) {
				try {
					string shaderName = material.shader.name;
					if(! shaders.ContainsKey(shaderName)) {
						string shaderPath = GetShaderPath(shaderName);
						if(System.IO.File.Exists(shaderPath)) {
							string shaderCode = GetShaderCode(shaderPath);
							Material m = new Material(shaderCode);
							if(m.shader != null) {
								shaders[shaderName] = m.shader;
							}
							Destroy(m);
						}
					}
				} catch(Exception ex) {
					DetailedException.Show(ex);
				}
			}

			// pass2
			foreach(Material material in materials) {
				string shaderName = material.shader.name;
				Shader newShader;
				if(shaders.TryGetValue(shaderName, out newShader)) {
					material.shader = newShader;
				}
			}
		}

		static string GetShaderPath(string shaderName) {
			return Path.Combine(ShaderBasePath, shaderName.Replace("/", "_") + ShaderExtension);
		}

		static string GetShaderCode(string shaderPath) {
			return System.IO.File.ReadAllText(shaderPath);
		}
	}
}
