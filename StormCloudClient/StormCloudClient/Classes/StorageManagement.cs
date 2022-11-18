using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudClient.Classes
{
    public class Schema
    {
        public string Name;
        public string Data;
    }
    public class StorageManagement
    {

        public static List<Schema> allSchemas;


        static string savePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static void Initialize()
        {
            // initialize global data variables
            allSchemas = new List<Schema>();

            // try to get schema data if exists; if does, set schemas variable to List deserialization
            if (File.Exists(_GetPath("schemas.json")))
            {
                var contents = File.ReadAllText(_GetPath("schemas.json"));
                try
                {
                    allSchemas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schema>>(contents);
                }catch(Exception e)
                {
                    // file is somehow corrupt; delete all so no further error is created
                    File.Delete(_GetPath("schemas.json"));
                }
            }
        }
        
        public static void AddData_Schema(string Name, string Contents)
        {
            var alreadyExists = allSchemas.Exists(s => s.Name == Name);
            if (alreadyExists)
            {
                allSchemas.Find(s => s.Name == Name).Data = Contents;
            }
            else
            {
                allSchemas.Add(new Schema() { Name = Name, Data = Contents });
            }


            
            _SaveData_Schema();
        }
        public static void RemoveData_Schema(string Name)
        {
            allSchemas.Remove(allSchemas.Find(s => s.Name == Name));
            _SaveData_Schema();
        }
        static void _SaveData_Schema()
        {
            var finalContents = Newtonsoft.Json.JsonConvert.SerializeObject(allSchemas);
            File.WriteAllText(_GetPath("schemas.json"), finalContents);
        }


        static string _GetPath(string fileName)
        {
            return Path.Combine(savePath, fileName);
        }
    }
}
