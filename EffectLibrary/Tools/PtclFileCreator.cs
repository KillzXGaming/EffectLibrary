using BfresLibrary;
using ShaderLibrary;
using Newtonsoft.Json;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EffectLibrary.Tools
{
    public class PtclFileCreator
    {
        public static PtclFile FromFolder(PtclFile ptcl, string folder)
        {
            string header_info = Path.Combine(folder, "PtclHeader.txt");
            if (File.Exists(header_info))
            {
                var info = JsonConvert.DeserializeObject<PtclFileDumper.HeaderInfo>(File.ReadAllText(header_info));
                ptcl.Header = info.Header;
                ptcl.Name = info.Name;
            }

            ptcl.EmitterList.EmitterSets.Clear();
            ptcl.Shaders.BnshFile.Variations.Clear();
            ptcl.Textures.BntxFile.Textures.Clear();
            ptcl.Textures.BntxFile.TextureDict.Clear();
            ptcl.Textures.TexDescTable.Descriptors.Clear();
            ptcl.Primitives.ResFile?.Models.Clear();
            ptcl.Primitives.PrimDescTable.Descriptors.Clear();
            ptcl.Shaders.ComputeShader.BnshFile.Variations.Clear();

            //each entry is an emitter set
            foreach (var dir in Directory.GetDirectories(folder))
            {
                EmitterSet emitterSet = new EmitterSet();
                emitterSet.Name = new DirectoryInfo(dir).Name;  
                ptcl.EmitterList.EmitterSets.Add(emitterSet);

                foreach (var d in Directory.GetDirectories(dir))
                    emitterSet.Emitters.Add(LoadEmitter(ptcl, emitterSet, d));

                emitterSet.Emitters = emitterSet.Emitters.OrderBy(x => x.Data.Order).ToList();
            }

            {
                string path = Path.Combine(folder, "EmitterSetInfo.txt");
                if (File.Exists(path))
                {
                    var info = JsonConvert.DeserializeObject<PtclFileDumper.EmitterSetInfo>(File.ReadAllText(path));
                    ptcl.EmitterList.EmitterSets = ptcl.EmitterList.EmitterSets.OrderBy(
                        x => info.Order.IndexOf(x.Name)).ToList();
                }
            }

            return ptcl;
        }

        private static Emitter LoadEmitter(PtclFile ptcl, EmitterSet emitterSet, string dir)
        {
            Emitter emitter = new Emitter(emitterSet);
            emitter.Name = new DirectoryInfo(dir).Name;
            emitter.Data = JsonConvert.DeserializeObject<EmitterData>(File.ReadAllText(Path.Combine(dir, "EmitterData.json")));

            foreach (var f in Directory.GetFiles(dir))
            {
                if (!f.EndsWith(".bin"))
                    continue;

                if (f.Contains("EmitterData.bin"))
                    emitter.BinaryData = File.ReadAllBytes(Path.Combine(dir, "EmitterData.bin"));
                else
                {
                    string magic = Path.GetFileNameWithoutExtension(f);
                    emitter.SubSections.Add(new EmitterSubSection(magic)
                    {
                        Data = File.ReadAllBytes(f),
                    });
                }
            }
            foreach (var sub in emitter.SubSections)
                File.WriteAllBytes(Path.Combine(dir, $"{sub.Header.Magic}.bin"), sub.Data);

            foreach (var f in Directory.GetFiles(dir))
            {
                if (!f.EndsWith(".bntx"))
                    continue;

                ulong id = ulong.Parse(Path.GetFileNameWithoutExtension(f));

                BntxFile bntx = new BntxFile(f);

                ptcl.Textures.AddTexture(id, bntx.Textures[0]);
            }

            foreach (var f in Directory.GetFiles(dir))
            {
                if (!f.EndsWith(".bfres"))
                    continue;

                ulong id = ulong.Parse(Path.GetFileNameWithoutExtension(f));

                ResFile resFile = new ResFile(f);

                ptcl.Primitives.AddPrimitive(id, resFile.Models[0]);
            }

            emitter.Data.ShaderReferences.ShaderIndex = ptcl.Shaders.BnshFile.Variations.Count;

            string shader_path         = Path.Combine(dir, "Shader.bnsh");
            string user_shader_path    = Path.Combine(dir, "UserShader.bnsh");
            string compute_shader_path = Path.Combine(dir, "ComputeShader.bnsh");

            if (File.Exists(shader_path))
            {
                var shader = new BnshFile(shader_path);
                ptcl.Shaders.BnshFile.Variations.Add(shader.Variations[0]);
            }
            else
                throw new Exception($"No shader present for emitter!");
            
            if (File.Exists(user_shader_path))
            {
                emitter.Data.ShaderReferences.UserShaderIndex1 = ptcl.Shaders.BnshFile.Variations.Count;

                var shader = new BnshFile(user_shader_path);
                ptcl.Shaders.BnshFile.Variations.Add(shader.Variations[0]);
            }

            if (File.Exists(compute_shader_path))
            {
                emitter.Data.ShaderReferences.ComputeShaderIndex = ptcl.Shaders.ComputeShader.BnshFile.Variations.Count;

                var shader = new BnshFile(compute_shader_path);
                ptcl.Shaders.ComputeShader.BnshFile.Variations.Add(shader.Variations[0]);
            }

            //children
            foreach (var d in Directory.GetDirectories(dir))
                emitter.Children.Add(LoadEmitter(ptcl, emitterSet, d));

            emitter.Children = emitter.Children.OrderBy(x => x.Data.Order).ToList();

            return emitter;
        }
    }
}
