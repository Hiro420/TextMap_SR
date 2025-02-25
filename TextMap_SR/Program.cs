using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TextMap_SR;

public class Program
{
    private static bool _doParseHash64;

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Please enter a valid location of a Textmap_en.bytes");
        }
        _doParseHash64 = ShouldIncludeHash64(args[0]);
        FileParser parser = new FileParser(_doParseHash64);
        Dictionary<long, Textmap> entries = parser.ParseTextmapFile(args[0]);
        string outputFilePath = ChangeExtensionToJson(args[0]);
        using (StreamWriter outputFile = new StreamWriter(outputFilePath))
        {
            outputFile.Write(DataToJson(entries));
        }
    }

    public static bool ShouldIncludeHash64(string bytesPath)
    {
        // 875924975 + 16453723977790693387 hashes for "Chinese" entry
        ReadOnlySpan<byte> pattern = new byte[]
        {
            0xDE, 0xB7, 0xAC, 0xC3, 0x06, 0x8B, 0xC0, 0xAA,
            0x84, 0x84, 0xD9, 0xD7, 0xAB, 0xE4, 0x01
        };

        ReadOnlySpan<byte> bytes = File.ReadAllBytes(bytesPath);
        return bytes.IndexOf(pattern) >= 0;
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
            ContractResolver = _doParseHash64 ? null : new IgnoreHash64Resolver()
        };

        return JsonConvert.SerializeObject(data, settings);
    }

    public class IgnoreHash64Resolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            if (type == typeof(TextID))
            {
                properties = properties.Where(p => p.PropertyName != "Hash64").ToList();
            }

            return properties;
        }
    }
}
