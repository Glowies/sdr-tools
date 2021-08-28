using System;
using System.Diagnostics;
using System.IO;
using CommandLine;

namespace noaaaptwrapper
{
    class MainClass
    {
        public class Options
        {
            [Option('f', "false-color", Required = false, HelpText = "Apply false color to the decoded images.")]
            public bool FalseColor { get; set; }

            [Option('o', "overlay", Required = false, HelpText = "Apply the map overlay onto the decoded images.")]
            public bool Overlay { get; set; }
        }

        private const string _lineSeperator = "= = = = = = = = = = = = = = = =\n";
        private const string _inputDir = "/root/input";
        private const string _workingDir = "/root";
        private const string _falseColorFlags = "-F --contrast telemetry";
        private const string _overlayFlags = "-m yes";

        public static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(MainWithOptions);

            return 0;
        }

        private static void MainWithOptions(Options o)
        {
            // Create output directory if it doesn't exist
            var outputDir = Path.Combine(_inputDir, "noaaAptOut");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Set optional flags
            var flags = "";
            if (o.FalseColor)
                flags += " " + _falseColorFlags;
            if (o.Overlay)
                flags += " " + _overlayFlags;

            // Loop through wav files and decode them
            string[] files = Directory.GetFiles(_inputDir, "*.wav", SearchOption.TopDirectoryOnly);

            foreach(var path in files)
            {
                Console.Write($"{_lineSeperator}Processing {Path.GetFileName(path)}\n{_lineSeperator}");

                var filename = Path.GetFileNameWithoutExtension(path);
                var satName = FindSatName(filename);
                var outputFile = Path.Combine(outputDir, $"{filename}.png");

                var args = $"-o {outputFile} --sat {satName} --rotate auto {flags} {path}";
                var bashOut = RunProgram("noaa-apt", args);

                Console.WriteLine(bashOut);
            }

        }

        private static string FindSatName(string filename)
        {
            string[] possibleSats = new[] { "noaa_15", "noaa_18", "noaa_19" };

            foreach(var sat in possibleSats)
            {
                if (filename.Contains(sat))
                    return sat;
            }

            return "noaa_19";
        }

        private static string RunProgram(string exeName, string args)
        {
            Process process = new Process();

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.FileName = exeName;
            processStartInfo.WorkingDirectory = _workingDir;
            processStartInfo.Arguments = args;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;

            process.StartInfo = processStartInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;

        }
    }
}
