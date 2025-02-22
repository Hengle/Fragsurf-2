using SourceUtils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fragsurf.BSP
{
	public partial class BspToUnity
	{

		private Dictionary<string, VmtInfo> _materialCache = new Dictionary<string, VmtInfo>();
		private Dictionary<string, Texture> _textureCache = new Dictionary<string, Texture>();

		public VmtInfo ApplyMaterial(MeshRenderer mr, string path)
		{
			var pathLower = path.ToLower();

			if (pathLower.Contains("tools") && !pathLower.Contains("skybox"))
			{
				mr.enabled = false;
			}

			if (_materialCache.ContainsKey(pathLower))
            {
				mr.material = _materialCache[pathLower].GeneratedMaterial;
				return _materialCache[pathLower];
            }

			var vmtInfo = GetVmtInfo(path);
			if(vmtInfo == null)
            {
				Debug.Log("VMT not found: " + path);
				mr.material = Options.MissingMaterial;
				return null;
            }

			vmtInfo.GeneratedMaterial = CreateMaterial(vmtInfo);
			mr.material = vmtInfo.GeneratedMaterial;
			_materialCache.Add(pathLower, vmtInfo);

			return vmtInfo;
		}

		private VmtInfo GetVmtInfo(string vmtPath)
        {
			if (!_resourceLoader.ContainsFile(vmtPath))
			{
				return null;
			}

			using var fs = _resourceLoader.OpenFile(vmtPath);
			var vmt = ValveMaterialFile.FromStream(fs);
			var result = new VmtInfo(vmtPath, vmt);
			return result;
		}

		private Texture LoadVmtBaseTexture(string vmtPath)
		{
			var baseTex = GetVmtInfo(vmtPath)?.BaseTex;
			if(baseTex == null)
            {
				return null;
            }
			return LoadTexture(baseTex);
		}

		private Material CreateMaterial(VmtInfo vmtInfo)
		{
			if(vmtInfo == null)
            {
				return Options.MissingMaterial;
            }

            if (vmtInfo.CompileSky > 0 && Options.SkyMaterial)
            {
				return Options.SkyMaterial;
            }

            if (!string.IsNullOrEmpty(vmtInfo.Include))
            {
				return CreateMaterial(GetVmtInfo(vmtInfo.Include));
            }

			if(vmtInfo.SurfaceProp == "water")
            {
				return Options.WaterMaterial;
            }

			var mat = GameObject.Instantiate<Material>(GetMaterialForVmt(vmtInfo));
			mat.mainTexture = LoadTexture(vmtInfo.BaseTex);
			mat.SetFloat("_Smoothness", 0);
			mat.SetFloat("_Metallic", 0f);
			mat.name = vmtInfo.VmtName;

			if (!string.IsNullOrEmpty(vmtInfo.BumpMap))
			{
				//result.SetTexture("_BumpMap", LoadTexture(vmtInfo.BumpMap));
			}

			if (vmtInfo.Transition)
			{
				//result.SetTexture("_MainTex2", LoadTexture(vmtInfo.BaseTex2));
			}

			return mat;
		}

		private Material GetMaterialForVmt(VmtInfo vmt)
        {
			if(vmt == null)
            {
				return Options.MissingMaterial;
            }
            if (vmt.CompileWater == 1)
            {
				return Options.WaterMaterial;
            }
			if (vmt.VmtShader.Equals("Cable", StringComparison.OrdinalIgnoreCase)
				|| vmt.VmtShader.Equals("Sprite", StringComparison.OrdinalIgnoreCase))
			{
				return Options.Unlit;
			}
			if (vmt.VmtShader.Equals("VertexLitGeneric", StringComparison.OrdinalIgnoreCase) 
				|| !vmt.Lightmapped)
            {
				return vmt.Translucent ? Options.VertexLitGenericTransparent : Options.VertexLitGeneric;
            }
			if (vmt.Translucent || vmt.AlphaTest)
			{
				return Options.LightmappedGenericTransparent;
			}
			return Options.LightmappedGeneric;
		}

		private Texture LoadTexture(string textureName)
		{
			var resourcePath = GetTexturePath(textureName);
			resourcePath = resourcePath.Replace('\\', '/');

			if (!_resourceLoader.ContainsFile(resourcePath))
			{
				return null;
			}

			var textureNameLower = textureName.ToLower();
			if (_textureCache.ContainsKey(textureNameLower))
			{
				return _textureCache[textureNameLower];
			}

			using (var fs = _resourceLoader.OpenFile(resourcePath))
			{
				try
				{
					var tex = VtfProvider.GetImage(fs);
					_textureCache.Add(textureNameLower, tex);
					return tex;
				}
				catch (System.Exception e) { Debug.LogError(e); }
			}

			return null;
		}

		private string GetTexturePath(string textureName)
		{
			var path = "materials/" + textureName;
			if (!Path.HasExtension(path))
			{
				return "materials/" + textureName + ".vtf";
			}
			return path;
		}

	}

	public class VmtInfo
	{
		public Material GeneratedMaterial;
		public string VmtName;
		public string VmtShader;
		public bool Lightmapped;
		public bool AlphaTest;
		public bool Translucent;
		public bool NoCull;
		public bool Transition;
		public string BaseTex;
		public string BaseTex2;
		public string BumpMap;
		public string SurfaceProp;
		public string Include;
		public float Alpha;
		public float RefractAmount;
		public int CompileSky;
		public int CompileWater;
		public int CompileLadder;
		public int CompileTrigger;
		public int CompileClip;
		public int VertexColor;
		public SourceUtils.Color32 Color;
		public SourceUtils.Color32 RefractTint;

		public VmtInfo(string vmtName, ValveMaterialFile vmt)
		{
			VmtName = vmtName;
			Parse(vmt);
		}

		private void Parse(ValveMaterialFile vmt)
		{
			// idk why this is enumerable
			var e = vmt.Shaders.GetEnumerator();
			while (e.MoveNext())
			{
				VmtShader = e.Current.ToLower();
				Lightmapped = VmtShader == "lightmappedgeneric" || VmtShader == "worldvertextransition";
				Transition = VmtShader == "worldvertextransition";
				AlphaTest = vmt[VmtShader].ContainsKey("$alphatest") ? vmt[VmtShader]["$alphatest"] : false;
				Translucent = vmt[VmtShader].ContainsKey("$translucent") ? vmt[VmtShader]["$translucent"] : false;
				NoCull = vmt[VmtShader].ContainsKey("$nocull") ? vmt[VmtShader]["$nocull"] : false;
				BaseTex = vmt[VmtShader].ContainsKey("$basetexture") ? vmt[VmtShader]["$basetexture"] : string.Empty;
				BaseTex2 = vmt[VmtShader].ContainsKey("$basetexture2") ? vmt[VmtShader]["$basetexture2"] : string.Empty;
				BumpMap = vmt[VmtShader].ContainsKey("bumpmap") ? vmt[VmtShader]["bumpmap"] : string.Empty;
				Color = vmt[VmtShader].ContainsKey("$color") ? vmt[VmtShader]["$color"] : new SourceUtils.Color32(255, 255, 255, 255);
				RefractTint = vmt[VmtShader].ContainsKey("$refracttint") ? vmt[VmtShader]["$refracttint"] : new SourceUtils.Color32(255, 255, 255, 255);
				SurfaceProp = vmt[VmtShader].ContainsKey("$surfaceprop") ? vmt[VmtShader]["$surfaceprop"] : "grass";
				Include = vmt[VmtShader].ContainsKey("include") ? vmt[VmtShader]["include"] : string.Empty;
				Alpha = vmt[VmtShader].ContainsKey("$alpha") ? vmt[VmtShader]["$alpha"] : 1.0f;
				RefractAmount = vmt[VmtShader].ContainsKey("$refractamount") ? vmt[VmtShader]["$refractamount"] : 0.2f;
				CompileSky = vmt[VmtShader].ContainsKey("%compilesky") ? vmt[VmtShader]["%compilesky"] : 0;
				CompileWater = vmt[VmtShader].ContainsKey("%compilewater") ? vmt[VmtShader]["%compilewater"] : 0;
				CompileTrigger = vmt[VmtShader].ContainsKey("%compiletrigger") ? vmt[VmtShader]["%compiletrigger"] : 0;
				CompileClip = vmt[VmtShader].ContainsKey("%compileclip") ? vmt[VmtShader]["%compileclip"] : 0;
				VertexColor = vmt[VmtShader].ContainsKey("$vertexcolor") ? vmt[VmtShader]["$vertexcolor"] : 0;
				break;
			}
		}
	}
}