using Newtonsoft.Json.Linq;
using System.Collections;
using System.Formats.Nrbf;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Runtime.Serialization.Formatters.Binary;
public class Program
{

    public static void ParseClassRecord(ClassRecord classRecord)
    {
        foreach (string name in classRecord.MemberNames)
        {
            object? val = classRecord.GetRawValue(name);
            //Console.WriteLine($"\t {name,-20} | Type: {classRecord.RecordType} ({classRecord.TypeName})");
        }
    }
    public static void Parse(ClassRecord classRecord, StreamWriter file, int depth)
    {
        string s = "";
        for (int i = 0; i < depth; i++)
        {
            s += "\t";
        }
        s += $"Class Name: {classRecord.TypeName}";
        //file.WriteLine(s);
        //file.WriteLine("--- Field Names Found ---");
        
        foreach (string memberName in classRecord.MemberNames)
        {
            
            object? value = classRecord.GetRawValue(memberName);
            if (value is SerializationRecord record)
            {
                // It's a complex type (Class, Array, etc.)
                s = "";
                for (int i = 0; i < depth; i++)
                { 
                    s += "\t";
                }
                s += $"Field: {memberName,-20} | Type: {record.RecordType}";
                //Console.WriteLine(s);
                file.WriteLine(s);
                if (record is ClassRecord crecord)
                {
                    Parse(crecord, file, depth + 1);
                }

                if (record is ArrayRecord arrayRecord)
                {
                    if (arrayRecord.RecordType == SerializationRecordType.ArraySinglePrimitive)
                    {
                        int count = arrayRecord.Lengths[0];
                        // Console.WriteLine(count);
                        try
                        {
                            int c = 0;
                            var primArray = arrayRecord.GetArray(typeof(int[]));
                            foreach (int i in primArray)
                            {
                                s = "";
                                for (int j = 0; j < depth; j++)
                                {
                                    s += "\t";
                                }
                                s += c.ToString();
                                file.WriteLine(s);
                                s = "";
                                for (int j = 0; j < depth + 1; j++)
                                {
                                    s += "\t";
                                }
                                s += i;
                                //Console.WriteLine(s);
                                file.WriteLine(s);
                                c++;
                            }
                        }
                        catch
                        {
                            //Console.WriteLine("Not int");
                        }
                        try
                        {
                            var primArray = arrayRecord.GetArray(typeof(float[]));
                            foreach (float i in primArray)
                            {
                                int c = 0;
                                s = "";
                                for (int j = 0; j < depth; j++)
                                {
                                    s += "\t";
                                }
                                s += c.ToString();
                                file.WriteLine(s);
                                s = "";
                                for (int j = 0; j < depth + 1; j++)
                                {
                                    s += "\t";
                                }
                                s += i;
                                //Console.WriteLine(s);
                                file.WriteLine(s);
                                c++;
                            }
                        }
                        catch
                        {
                           // Console.WriteLine("not single");
                        }
                    }
                    if (arrayRecord.RecordType == SerializationRecordType.BinaryArray)
                    {
                        int count = arrayRecord.Lengths[0];
                        //Console.WriteLine(count);

                        var t = arrayRecord.TypeName.FullName;
                        if (arrayRecord is SZArrayRecord<SerializationRecord> szArray)
                        {

                            // Use a standard for loop. 
                            // This bypasses the iterable/GetArray type-checking errors.
                            for (int i = 0; i < szArray.Length; i++)
                            {
                                s = "";
                                // Access each element individually as a record
                                SerializationRecord r = szArray.GetArray()[i];
                                for (int j = 0; j < depth; j++)
                                {
                                    s += "\t";
                                }
                                s += i.ToString();
                                file.WriteLine(s);
                                if (r is ClassRecord c)
                                {
                                    Parse(c, file, depth + 1);
                                    //Console.WriteLine($"[{i}]: {classRecord.TypeName.FullName}");

                                    //// Extract values by name from the proxy object
                                    //foreach (string m in c.MemberNames)
                                    //{
                                    //    object v = c.GetRawValue(m);
                                    //    Console.WriteLine($"  {m}: {v}");
                                    //}
                                }
                                else if (r is not null) 
                                {
                                    file.WriteLine("not classrecord.");
                                }
                            }
                        }
                        else
                        {
                            file.WriteLine("not SZ");
                        }
                    }
                }
            }
            else if (value is not null)
            {
                s = "";
                for (int i = 0; i < depth; i++)
                {
                    s += "\t";
                }
                s += $"Field: {memberName,-20} | Type: {value.GetType().FullName} | Value: {value}";
                // It's a direct primitive (int, float, string, etc.)
                //Console.WriteLine($"Field: {memberName,-20} | Type: {value.GetType().FullName} | Value: {value}");
                file.WriteLine(s);
            }
        }
    }
    
    public static void Main()
    {
        //#pragma warning disable SYSLIB0011
        //var bf = new BinaryFormatter();
        //using FileStream fs = File.OpenRead("../../../../slot0.save");
        //bf.Deserialize(fs);
        List<string> members = new List<string>();

        using FileStream fs = File.OpenRead("../../../../slot0.save");
        using StreamWriter wText = new StreamWriter("File.txt");
        SerializationRecord rootRecord = NrbfDecoder.Decode(fs);

        if (rootRecord is ClassRecord classRecord)
        {
            //foreach (string memberName in classRecord.MemberNames)
            //{

            //    object? value = classRecord.GetRawValue(memberName);
            //    if (value is not SerializationRecord record)
            //    {
            //        Console.WriteLine(memberName);
            //    }
            //}
            Parse(classRecord, wText, 0);
            //Console.WriteLine($"Class Name: {classRecord.TypeName}");
            //Console.WriteLine("--- Field Names Found ---");

            //foreach (string memberName in classRecord.MemberNames)
            //{
            //    object? value = classRecord.GetRawValue(memberName);


            //    if (value is SerializationRecord record)
            //    {
            //        // It's a complex type (Class, Array, etc.)
            //        Console.WriteLine($"Field: {memberName,-20} | Type: {record.RecordType} ({record.TypeName})");
            //        if (record is ClassRecord crecord)
            //        {
            //            ParseClassRecord(crecord);
            //        }

            //        if (record is ArrayRecord arrayRecord)
            //        {
            //            if (arrayRecord.RecordType == SerializationRecordType.ArraySinglePrimitive)
            //            {
            //                int count = arrayRecord.Lengths[0];
            //                Console.WriteLine(count);
            //                try
            //                {
            //                    var primArray = arrayRecord.GetArray(typeof(int[]));
            //                    foreach (int i in primArray)
            //                    {
            //                        Console.WriteLine(i);
            //                    }
            //                }
            //                catch
            //                {
            //                    Console.WriteLine("Not int");
            //                }
            //                try
            //                {
            //                    var primArray = arrayRecord.GetArray(typeof(Single[]));
            //                    foreach (Single i in primArray)
            //                    {
            //                        Console.WriteLine(i);
            //                    }
            //                } catch
            //                {
            //                    Console.WriteLine("not single");
            //                }
            //            }
            //            if (arrayRecord.RecordType == SerializationRecordType.BinaryArray)
            //            {
            //                int count = arrayRecord.Lengths[0];
            //                Console.WriteLine(count);

            //                var t = arrayRecord.TypeName.FullName;
            //                if (arrayRecord is SZArrayRecord<SerializationRecord> szArray)
            //                {
            //                    // Use a standard for loop. 
            //                    // This bypasses the iterable/GetArray type-checking errors.
            //                    for (int i = 0; i < szArray.Length; i++)
            //                    {
            //                        // Access each element individually as a record
            //                        SerializationRecord r = szArray.GetArray()[i];

            //                        if (r is ClassRecord c)
            //                        {
            //                            Console.WriteLine($"Mission Record [{i}]: {classRecord.TypeName.FullName}");

            //                            // Extract values by name from the proxy object
            //                            foreach (string m in c.MemberNames)
            //                            {
            //                                object v = c.GetRawValue(m);
            //                                Console.WriteLine($"  {m}: {v}");
            //                            }
            //                        }
            //                    }
            //                }


            //            }
            //        }
            //    }
            //    else if (value is not null)
            //    {
            //        // It's a direct primitive (int, float, string, etc.)
            //        Console.WriteLine($"Field: {memberName,-20} | Type: {value.GetType().FullName} | Value: {value}");
            //    }
            //}
        }
    }
}