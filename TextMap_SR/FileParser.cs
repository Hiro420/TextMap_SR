using System.Reflection;
using TexMap_SR;

namespace TextMap_SR
{
    public class FileParser
    {
        public Dictionary<long,Textmap> ParseTextmapFile(string filepath)
        {
            Dictionary<long, Textmap> result = new Dictionary<long, Textmap>();
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (EndianBinaryReader reader = new EndianBinaryReader(fs))
            {
                long entriesCount = reader.ReadSignedVarInt();
                Console.WriteLine($"Amount of entries: {entriesCount}");
                for (int i = 0; i < entriesCount; i++)
                {
                    List<bool> bitfield = reader.ReadExcelBitfield();
                    Textmap mapEntry = ReadEntry(bitfield, reader);
                    result[mapEntry.ID!.Hash] = mapEntry;
                }
            }
            return result;
        }

        internal Textmap ReadEntry(List<bool> bitfield, EndianBinaryReader reader)
        {
            Textmap textmap = new Textmap();
            int bitIndex = 0;

            foreach (var field in typeof(Textmap).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (bitIndex < bitfield.Count && bitfield[bitIndex])
                {
                    switch (field.FieldType)
                    {
                        case Type t when t == typeof(long):
                            field.SetValue(textmap, reader.ReadSignedVarInt());
                            break;

                        case Type t when t == typeof(string):
                            field.SetValue(textmap, reader.ReadString());
                            break;

                        case Type t when t == typeof(bool?):
                            field.SetValue(textmap, reader.ReadBoolean());
                            break;

                        case Type t when t == typeof(TextID):
                            TextID textID = new TextID
                            {
                                Hash = reader.ReadSignedVarInt()
                            };
                            field.SetValue(textmap, textID);
                            break;

                        default:
                            throw new InvalidOperationException($"Unsupported field type: {field.FieldType}");
                    }
                }
                bitIndex++;
            }
            return textmap;
        }
    }

    public class Textmap
    {
        public TextID ID;
        public string Text;
        public bool? HasParam;
    }

    public class TextID
    {
        public long Hash;
    }
}
