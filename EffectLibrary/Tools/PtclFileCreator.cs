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
using EffectLibrary.EFT2;
using System.Reflection.PortableExecutable;

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

            var compute_shaders = ptcl.Shaders.ComputeShader.BnshFile.Variations.ToArray();

            ptcl.EmitterList.EmitterSets.Clear();
            ptcl.Shaders.BnshFile.Variations.Clear();
            ptcl.Textures.BntxFile.Textures.Clear();
            ptcl.Textures.BntxFile.TextureDict.Clear();
            ptcl.Textures.TexDescTable.Descriptors.Clear();
            ptcl.Primitives.ResFile?.Models.Clear();
            ptcl.Primitives.PrimDescTable.Descriptors.Clear();
            ptcl.Shaders.ComputeShader.BnshFile.Variations.Clear();
            

            var emitter_folders = Directory.GetDirectories(folder);

            //order load 
            {
                string path = Path.Combine(folder, "EmitterSetInfo.txt");
                if (File.Exists(path))
                {
                    var info = JsonConvert.DeserializeObject<PtclFileDumper.EmitterSetInfo>(File.ReadAllText(path));
                    emitter_folders = emitter_folders.OrderBy(
                        x => info.Order.IndexOf(new DirectoryInfo(x).Name)).ToArray();
                }
            }

            //each entry is an emitter set
            foreach (var dir in emitter_folders)
            {
                EmitterSet emitterSet = new EmitterSet();
                emitterSet.Name = new DirectoryInfo(dir).Name;  
                ptcl.EmitterList.EmitterSets.Add(emitterSet);

                var folders = Directory.GetDirectories(dir);

                //order load 
                {
                    string path = Path.Combine(dir, "EmitterOrder.txt");
                    if (File.Exists(path))
                    {
                        var info = JsonConvert.DeserializeObject<PtclFileDumper.EmitterSetInfo>(File.ReadAllText(path));
                        folders = folders.OrderBy(
                            x => info.Order.IndexOf(new DirectoryInfo(x).Name)).ToArray();
                    }
                }

                foreach (var d in folders)
                    emitterSet.Emitters.Add(LoadEmitter(ptcl, emitterSet, d));

                emitterSet.Emitters = emitterSet.Emitters.OrderBy(x => x.Data.Order).ToList();
            }

            //ptcl files can have a compute shader present but no references, add it anyways to be accurate
            if (compute_shaders.Length == 1 && ptcl.Shaders.ComputeShader.BnshFile.Variations.Count == 0)
            {
                ptcl.Shaders.ComputeShader.BnshFile.Variations.AddRange(compute_shaders);
            }

            return ptcl;
        }

        private static Emitter LoadEmitter(PtclFile ptcl, EmitterSet emitterSet, string dir)
        {
            Emitter emitter = new Emitter(emitterSet);
            emitter.Name = new DirectoryInfo(dir).Name;

            var jsonsettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>()
                {
                    new Newtonsoft.Json.Converters.StringEnumConverter(),
                },
            };
            emitter.Data = JsonConvert.DeserializeObject<EmitterData>(File.ReadAllText(Path.Combine(dir, "EmitterData.json")), jsonsettings);

            foreach (var f in Directory.GetFiles(dir))
            {
                if (!f.EndsWith(".bin") && !f.EndsWith(".json"))
                    continue;

                if (f.Contains("EmitterData.bin"))
                {
                    emitter.BinaryData = File.ReadAllBytes(Path.Combine(dir, "EmitterData.bin"));
                }
                else if (f.Contains("EmitterData.json"))
                {

                }
                else
                {
                    string magic = Path.GetFileNameWithoutExtension(f);

                    var sect = new EmitterSubSection(magic);
                    if (magic.StartsWith("EA")) // Emitter anim
                        sect = new EmitterAnimation(magic);

                    sect.Import(f);
                    emitter.SubSections.Add(sect);
                }
            }

            var section_order = new string[] { "FCOV",  "CSDP", "CUDP", "CADP", "EAA0", "EAA1", "EATR", }.ToList();

            emitter.SubSections = emitter.SubSections.OrderBy(x => section_order.IndexOf(x.Header.Magic)).ToList();

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

            Console.WriteLine($"{emitterSet.Name} {emitter.Name} {emitter.Data.ShaderReferences.ShaderIndex}");
             
            string shader_path          = Path.Combine(dir, "Shader.bnsh");
            string user_shader1_path    = Path.Combine(dir, "UserShader1.bnsh");
            string user_shader2_path    = Path.Combine(dir, "UserShader2.bnsh");
            string compute_shader_path  = Path.Combine(dir, "ComputeShader.bnsh");

            if (File.Exists(shader_path))
            {
                var shader = new BnshFile(shader_path);
                ptcl.Shaders.BnshFile.Variations.Add(shader.Variations[0]);
            }
            else
                throw new Exception($"No shader present for emitter!");
            
            if (File.Exists(user_shader1_path))
            {
                emitter.Data.ShaderReferences.UserShaderIndex1 = ptcl.Shaders.BnshFile.Variations.Count;

                var shader = new BnshFile(user_shader1_path);
                ptcl.Shaders.BnshFile.Variations.Add(shader.Variations[0]);
            }

            if (File.Exists(user_shader2_path))
            {
                emitter.Data.ShaderReferences.UserShaderIndex2 = ptcl.Shaders.BnshFile.Variations.Count;

                var shader = new BnshFile(user_shader2_path);
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
