using EffectLibrary;
using EffectLibrary.Tools;
using System.IO;
using System.Text;

namespace EffectConverter
{
    class Program
    {
        public static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                //process particle file
                if (File.Exists(arg))
                    DumpParticleFile(arg);
                else if (Directory.Exists(arg))
                    CreateParticleFile(arg);
            }
        }

        static void DumpParticleFile(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);

            string magic = GetMagic(path);

            if (magic == "EFFN") //namco effect
            {
                NamcoEffectFile namcoEffect = new NamcoEffectFile(path);
                PtclFileDumper.DumpAll(namcoEffect.PtclFile, name);
                //Dump namco effect header info which includes emitter link data
                namcoEffect.Export(Path.Combine(name, $"NamcoFile.json"));
            }
            else if (magic == "VFXB") //ptcl file
            {
                PtclFile ptcl = new PtclFile(path);
                PtclFileDumper.DumpAll(ptcl, name);
            }
            else
            {
                throw new Exception($"Unknown file {path} given! Expected EFFN or VFXB magic, but got {magic}.");
            }
        }

        static string GetMagic(string path)
        {
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                return Encoding.ASCII.GetString(reader.ReadBytes(4));
            }
        }

        static void CreateParticleFile(string folder)
        {
            string name = Path.GetFileNameWithoutExtension(folder);

            //base ptcl to edit
            string ptcl_base = Path.Combine(folder, "Base.ptcl");
            if (!File.Exists(ptcl_base))
                return;

            PtclFile ptcl = new PtclFile(ptcl_base);
            var newPtcl = PtclFileCreator.FromFolder(ptcl, folder);

            string namco_header = Path.Combine(folder, "NamcoFile.json");
            if (File.Exists(namco_header))
            {
                NamcoEffectFile namcoEffect = new NamcoEffectFile(newPtcl);
                namcoEffect.Import(namco_header);

                namcoEffect.Save($"{name}_NEW.eff");
            }
            else
                newPtcl.Save($"{name}_NEW.ptcl");
        }
    }
}