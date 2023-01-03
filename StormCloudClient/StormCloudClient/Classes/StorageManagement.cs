using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudClient.Classes
{
    public enum UploadStatus
    {
        NOT_TRIED,
        FAILED,
        SUCCEEDED
    }
    public class Schema
    {
        public string Name;
        public string Data;
    }

    public class Match
    {
        public int Team;
        public int Number;
        public DateTime Created;
        public string Scouter;
        public string Schema;
        public string Color;
        public string Environment;
        public string Data;
        public UploadStatus Status;
        public string Identifier;
    }
    public class Photo
    {
        public DateTime Taken;
        public string Path;
        public UploadStatus Status;
        public int Team;
        public List<int> Matches;
        public string Identifier;
    }
    public class StorageManagement
    {

        public static List<Schema> allSchemas;
        public static List<Match> allMatches;
        public static List<Photo> allPhotos;


        static string savePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static void Initialize()
        {
            // initialize global data variables
            allSchemas = new List<Schema>();
            allMatches = new List<Match>();
            allPhotos = new List<Photo>();
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

            if (File.Exists(_GetPath("matches.json")))
            {
                var contents = File.ReadAllText(_GetPath("matches.json"));
                try
                {
                    allMatches = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Match>>(contents);
                }
                catch (Exception e)
                {
                    
                }
            }

            if (File.Exists(_GetPath("photos.json")))
            {
                var contents = File.ReadAllText(_GetPath("photos.json"));
                try
                {
                    allPhotos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Photo>>(contents);
                }
                catch (Exception e)
                {

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
        public static void _SaveData_Schema()
        {
            var finalContents = Newtonsoft.Json.JsonConvert.SerializeObject(allSchemas);
            File.WriteAllText(_GetPath("schemas.json"), finalContents);
        }


        public static void AddData_Match(int Number, int Team, string Scouter, string Color, string Schema, string Environment, string Data)
        {
            var alreadyExists = allMatches.Exists(s => s.Number == Number && s.Environment == Environment);
            if (alreadyExists)
            {
                allMatches.Find(s => s.Number == Number && s.Environment == Environment).Data = Data;
                allMatches.Find(s => s.Number == Number && s.Environment == Environment).Color = Color;
                allMatches.Find(s => s.Number == Number && s.Environment == Environment).Scouter = Scouter;
                allMatches.Find(s => s.Number == Number && s.Environment == Environment).Team = Team;
                allMatches.Find(s => s.Number == Number && s.Environment == Environment).Created = DateTime.Now;
                allMatches.Find(s => s.Number == Number && s.Environment == Environment).Status = UploadStatus.NOT_TRIED;
            }
            else
            {
                allMatches.Add(new Match() {
                    Number = Number,
                    Team = Team,
                    Scouter = Scouter,
                    Color = Color,
                    Schema = Schema,
                    Environment = Environment,
                    Data = Data,
                    Created = DateTime.Now,
                    Status = UploadStatus.NOT_TRIED,
                    Identifier = GenerateUUID()
                });
            }



            _SaveData_Match();
        }
        public static void RemoveData_Match(int Number, string Environment)
        {
            allMatches.Remove(allMatches.Find(s => s.Number == Number && s.Environment == Environment));
            _SaveData_Match();
        }
        public static void _SaveData_Match()
        {
            var finalContents = Newtonsoft.Json.JsonConvert.SerializeObject(allMatches);
            File.WriteAllText(_GetPath("matches.json"), finalContents);
        }


        public static async void AddData_Photo(FileResult photo, int Team, List<int> Matches)
        {

            var created = DateTime.Now;
            var filename = created.ToString();
            filename = filename.Replace("/", "_");
            var p = new Photo()
            {
                Taken = created,
                Status = UploadStatus.NOT_TRIED,
                Path = filename,
                Team = Team,
                Matches = Matches,
                Identifier = GenerateUUID()
            };

            var localFilePath = _GetPath(p.Path);

            using Stream sourceStream = await photo.OpenReadAsync();
            using FileStream localFileStream = File.OpenWrite(localFilePath);
            
            await sourceStream.CopyToAsync(localFileStream);

            allPhotos.Add(p);



            _SaveData_Photo();
        }

        public static async void AddData_Photo(FileResult photo)
        {

            var created = DateTime.Now;
            var filename = created.ToString();
            filename = filename.Replace("/", "_");
            var p = new Photo()
            {
                Taken = created,
                Status = UploadStatus.NOT_TRIED,
                Path = filename
            };

            var localFilePath = _GetPath(p.Path);

            using Stream sourceStream = await photo.OpenReadAsync();
            using FileStream localFileStream = File.OpenWrite(localFilePath);

            await sourceStream.CopyToAsync(localFileStream);

            allPhotos.Add(p);



            _SaveData_Photo();
        }

        public static void RemoveData_Photo(string Path)
        {
            allPhotos.Remove(allPhotos.Find(s => s.Path == Path));
            _SaveData_Photo();
        }
        public static void _SaveData_Photo()
        {
            var finalContents = Newtonsoft.Json.JsonConvert.SerializeObject(allPhotos);
            File.WriteAllText(_GetPath("photos.json"), finalContents);
        }


        static string _GetPath(string fileName)
        {
            return Path.Combine(savePath, fileName);
        }
        public static string GetPath(string fileName)
        {
            return Path.Combine(savePath, fileName);
        }

        static string GenerateUUID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
