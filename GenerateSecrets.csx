#r "nuget: System.Text.Encoding.CodePages"

using System;
using System.IO;
using System.Text;
using System.Linq;


const string inputFile = "../.env";
const string outputFile = "Secrets.cs";




if (!File.Exists(inputFile))
{
    throw new FileNotFoundException($"No .env file found. Make sure you created a .env file" +
        $" with the proper client ID and client secret. To get these values check the DeviantArt" +
        $" API docs https://www.deviantart.com/developers/apps");
}


var envData = File.ReadLines(inputFile).Select(line => SplitKeyAndValue(line));
StringBuilder output = new StringBuilder("namespace ArtViewer");
output.AppendLine("{");
output.AppendLine("    public static class Secrets");
output.AppendLine("    {");

foreach (Tuple<string, string> kvPair in envData)
{
    output.AppendLine($"        public const string {kvPair.Item1} = \"{kvPair.Item2}\";");
}

output.AppendLine("    }");
output.AppendLine("}");


File.WriteAllText(outputFile, output.ToString());


Tuple<string, string> SplitKeyAndValue(string line)
{
    int indexOfEqualSign = line.IndexOf('=');

    if (indexOfEqualSign == -1)
    {
        throw new FormatException($"Formatting error in your .env file. Please make sure each item in the" +
            $" file is on its own line, in the format [key=value]. Make sure you have no leading or trailing" +
            $" whitespace.");
    }

    string key = line.Substring(0, indexOfEqualSign);
    string value = line.Substring(indexOfEqualSign + 1);

    return Tuple.Create(key, value);
}

