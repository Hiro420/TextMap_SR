using Newtonsoft.Json;
using TextMap_SR;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Please enter a valid location of a Textmap_en.bytes");
        }
        FileParser parser = new FileParser();
        Dictionary<long, Textmap> entries = parser.ParseTextmapFile(args[0]);
        string outputFilePath = ChangeExtensionToJson(args[0]);
        using (StreamWriter outputFile = new StreamWriter(outputFilePath))
        {
            outputFile.Write(DataToJson(entries));
        }
    }

    private static string ChangeExtensionToJson(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath)!;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        return Path.Combine(directory!, fileNameWithoutExtension + ".json");
    }

    private static string DataToJson<T>(T data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        return JsonConvert.SerializeObject(data, settings);
    }
}
