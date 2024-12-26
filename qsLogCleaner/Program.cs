using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace _qsLogCleaner;

class Program
{
    static void Main (string [] args)
    {
        try
        {
            string xDirectoryPathListFilePath = Path.Join (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location), "DirectoryPaths.txt");

            if (File.Exists (xDirectoryPathListFilePath) == false)
                File.WriteAllText (xDirectoryPathListFilePath, string.Empty, Encoding.UTF8);

            var xDirectoryPaths = File.ReadAllLines (xDirectoryPathListFilePath, Encoding.UTF8).
                Select (x => x.Trim()).
                Where (y => string.IsNullOrEmpty (y) == false && Path.IsPathFullyQualified (y) && Directory.Exists (y)).
                Order (StringComparer.OrdinalIgnoreCase);

            if (xDirectoryPaths.Any () == false)
            {
                Console.WriteLine ("No valid directory paths found in the file.");
                return;
            }

            Console.WriteLine ("Directory paths found in the file:");

            foreach (var xDirectoryPath in xDirectoryPaths)
                Console.WriteLine ($"    {xDirectoryPath}");

            Regex xRegex = new (@"^[0-9]{8}-[0-9]{6}$", RegexOptions.Compiled);

            var xFilePaths = xDirectoryPaths.SelectMany (x => Directory.GetFiles (x, "*.log", SearchOption.TopDirectoryOnly)).
                Where (y => xRegex.IsMatch (Path.GetFileNameWithoutExtension (y))).
                Order (StringComparer.OrdinalIgnoreCase);

            if (xFilePaths.Any () == false)
            {
                Console.WriteLine ("No log files found in the directories.");
                return;
            }

            foreach (string xFilePath in xFilePaths)
            {
                var xLinesBeforeCleaning = File.ReadAllLines (xFilePath, Encoding.UTF8);

                var xLinesAfterCleaning = xLinesBeforeCleaning.
                    Where (x =>
                    {
                        if (x.EndsWith (@"\Desktop.ini (File Updated)", StringComparison.OrdinalIgnoreCase))
                            return false;

                        return true;
                    }).
                    ToArray ();

                if (xLinesAfterCleaning.Length < xLinesBeforeCleaning.Length)
                {
                    string xBackupFilePath = Path.ChangeExtension (xFilePath, ".bak");
                    File.Move (xFilePath, xBackupFilePath);

                    File.WriteAllLines (xFilePath, xLinesAfterCleaning, Encoding.UTF8);

                    Console.WriteLine ($"Cleaned: {xFilePath}");
                }

                else
                    Console.WriteLine ($"Skipped: {xFilePath}");
            }
        }

        catch (Exception xException)
        {
            Console.WriteLine (xException.ToString ());
        }

        finally
        {
            Console.Write ("Press any key to exit: ");
            Console.ReadKey (true);
            Console.WriteLine ();
        }
    }
}
