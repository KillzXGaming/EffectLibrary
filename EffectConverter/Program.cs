using EffectLibrary;
using EffectLibrary.Tools;
using System.IO;

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
            string ext = Path.GetExtension(path);
            string name = Path.GetFileNameWithoutExtension(path);

            if (ext == ".eff") //namco effect
            {
                NamcoEffectFile namcoEffect = new NamcoEffectFile(path);
                PtclFileDumper.DumpAll(namcoEffect.PtclFile, name);
                //Dump namco effect header info which includes emitter link data
                namcoEffect.Export(Path.Combine(name, $"NamcoFile.json"));
            }
            else //assume it is a ptcl file
            {
                PtclFile ptcl = new PtclFile(path);
                PtclFileDumper.DumpAll(ptcl, name);
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

                namcoEffect.Save($"{name}.NEW.ptcl");
            }
            else
                newPtcl.Save($"{name}.NEW.ptcl");
        }
    }
}