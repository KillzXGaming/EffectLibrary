using BfresLibrary;
using EffectLibrary.EFT2;
using Newtonsoft.Json;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffectLibrary.Tools
{
    public class PtclFileDumper
    {
        public static void DumpAll(PtclFile ptcl, string folder)
        {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string ptcl_base = Path.Combine(folder, "Base.ptcl");
            ptcl.Save(ptcl_base);

            HeaderInfo header = new HeaderInfo() { Header = ptcl.Header, Name = ptcl.Name, };
            File.WriteAllText(Path.Combine(folder, "PtclHeader.txt"), JsonConvert.SerializeObject(header, Formatting.Indented));

            foreach (var emitterSet in ptcl.EmitterList.EmitterSets)
            {
                DumpEmitterSet(emitterSet, folder);
            }

            EmitterSetInfo info = new EmitterSetInfo();
            foreach (var emitterSet in ptcl.EmitterList.EmitterSets)
                info.Order.Add(emitterSet.Name);

            File.WriteAllText(Path.Combine(folder, "EmitterSetInfo.txt"), JsonConvert.SerializeObject(info, Formatting.Indented));
        }

        public static void DumpEmitterSet(EmitterSet emitterSet, string folder)
        {
            string dir = Path.Combine(folder, emitterSet.Name);

            foreach (var emitter in emitterSet.Emitters)
                DumpEmitter(emitter, dir);

            EmitterSetInfo info = new EmitterSetInfo();
            foreach (var emitter in emitterSet.Emitters)
                info.Order.Add(emitter.Name);

            File.WriteAllText(Path.Combine(dir, "EmitterOrder.txt"), JsonConvert.SerializeObject(info, Formatting.Indented));
        }

        public static void DumpEmitter(Emitter emitter, string folder)
        {
            string dir = Path.Combine(folder, emitter.Name);

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            //Dump textures, shaders and models
            var bntx = emitter.PtclHeader.Textures.BntxFile;
            var bnsh = emitter.PtclHeader.Shaders.BnshFile;
            var bfres = emitter.PtclHeader.Primitives.ResFile;

            var model = emitter.GetModelBinary();
            var model_volume = emitter.GetVolumeModelBinary();
            var model_extra = emitter.GetModelExtraBinary();

            var shader = emitter.GetShaderBinary();
            var shader_compute = emitter.GetComputeShaderBinary();
            var shader_user1 = emitter.GetUser1ShaderBinary();
            var shader_user2 = emitter.GetUser2ShaderBinary();

            if (shader != null) shader.Export(Path.Combine(dir, $"Shader.bnsh"));
            if (shader_compute != null) shader_compute.Export(Path.Combine(dir, $"ComputeShader.bnsh"));
            if (shader_user1 != null) shader_user1.Export(Path.Combine(dir, $"UserShader1.bnsh"));
            if (shader_user2 != null) shader_user2.Export(Path.Combine(dir, $"UserShader2.bnsh"));

            void DumpModel(string filePath)
            {
                ResFile resFile = new ResFile()
                {
                    IsPlatformSwitch = true,
                    VersionMajor = 0, VersionMajor2 = 5,
                    VersionMinor = 0, VersionMinor2 = 3,
                    Alignment = 0xC,
                    Name = model.Name,
                    ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian,
                };
                resFile.Models.Add(model.Name, model);
                resFile.Save(filePath);
            }

            if (model != null)
                DumpModel(Path.Combine(dir, $"{emitter.Data.ParticleData.PrimitiveID}.bfres"));
            if (model_volume != null)
                DumpModel(Path.Combine(dir, $"{emitter.Data.ShapeInfo.PrimitiveIndex}.bfres"));
            if (model_extra != null)
                DumpModel(Path.Combine(dir, $"{emitter.Data.ParticleData.PrimitiveExID}.bfres"));

            void DumpTexures(string filePath, Texture tex)
            {
                var bntx = new BntxFile();
                bntx.Target = new char[] { 'N', 'X', ' ', ' ' };
                bntx.Name = tex.Name;
                bntx.Alignment = 0xC;
                bntx.TargetAddressSize = 0x40;
                bntx.VersionMajor = 0;
                bntx.VersionMajor2 = 4;
                bntx.VersionMinor = 0;
                bntx.VersionMinor2 = 0;
                bntx.Textures = new List<Texture>();
                bntx.TextureDict = new Syroot.NintenTools.NSW.Bntx.ResDict();
                bntx.RelocationTable = new RelocationTable();
                bntx.Flag = 0;

                bntx.TextureDict = new Syroot.NintenTools.NSW.Bntx.ResDict();
                bntx.Textures = new List<Texture>();

                bntx.Textures.Add(tex);
                bntx.TextureDict.Add(tex.Name);
                bntx.Save(filePath);
            }

            int idx = 0;
            foreach (var sampler in emitter.Data.GetSamplers())
            {
                var texture = emitter.GetTextureBinary(sampler);
                if (texture == null)
                    continue;

                DumpTexures(Path.Combine(dir, $"{sampler.TextureID}.bntx"), texture);
                idx++;
            }

            File.WriteAllBytes(Path.Combine(dir, "EmitterData.bin"), emitter.BinaryData);

            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>()
                {
                    new Newtonsoft.Json.Converters.StringEnumConverter(),
                },
            };
            string json = JsonConvert.SerializeObject(emitter.Data, Formatting.Indented, jsonsettings);

            File.WriteAllText(Path.Combine(dir, "EmitterData.json"), json);

            foreach (var sub in emitter.SubSections)
                sub.Export(Path.Combine(dir, $"{sub.Header.Magic}.bin"));

            foreach (var child in emitter.Children)
                DumpEmitter(child, dir);
        }

        public class HeaderInfo
        {
            public BinaryHeader Header;
            public string Name;
        }

        public class EmitterSetInfo
        {
            public List<string> Order = new List<string>();
        }
    }
}
