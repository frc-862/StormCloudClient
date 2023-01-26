using System;
namespace StormCloudUSBService
{
	public class DataManager
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
            public string DeviceID;
        }
        public class Photo
        {
            public DateTime Taken;
            public string Path;
            public UploadStatus Status;
            public int Team;
            public List<int> Matches;
            public string Identifier;
            public string Type;
            public string DeviceID;
            public bool JustTaken;
        }
        public class PhotoData
        {
            public string Name;
            public string Data;
        }

        public class USBPacket
        {
            public List<Match> matches;
            public List<Photo> photos;
            public List<PhotoData> b64Photos;
        }

        public static List<Match> localMatches;
        public static List<Photo> localPhotos;
        public static List<PhotoData> localPhotoData;
        
        public static void HandleUSBPushData(string data)
		{
            var unpacked = Newtonsoft.Json.JsonConvert.DeserializeObject<USBPacket>(data);

            foreach(Match m in unpacked.matches)
            {
                var countExists = localMatches.Count(x => x.Identifier == m.Identifier);
                if (countExists == 0)
                    localMatches.Add(m);
            }
            foreach (Photo p in unpacked.photos)
            {
                var countExists = localPhotos.Count(x => x.Identifier == p.Identifier);
                if (countExists == 0)
                    localPhotos.Add(p);
            }
            foreach(PhotoData pd in unpacked.b64Photos)
            {
                SaveToStreamFile(pd.Data, pd.Name);
                localPhotoData.Add(pd);
            }


            SaveAllData();

            APIService.SendMatches(unpacked.matches);
		}

        public static void SaveAllData()
        {
            var matchString = Newtonsoft.Json.JsonConvert.SerializeObject(localMatches);
            SaveToFile(matchString, "MATCHES");

            var photoString = Newtonsoft.Json.JsonConvert.SerializeObject(localPhotos);
            SaveToFile(matchString, "PHOTOS");
        }

        public static void Initialize()
        {


            var matchString = ReadFromFile("MATCHES");
            if(matchString == "")
            {
                localMatches = new List<Match>();
            }
            else
            {
                localMatches = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Match>>(matchString);
            }

            var photoString = ReadFromFile("PHOTOS");
            if (photoString == "")
            {
                localPhotos = new List<Photo>();
            }
            else
            {
                localPhotos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Photo>>(photoString);
            }

        }

        public static async void SaveToStreamFile(string contents, string name)
        {
            var saveToFile = Convert.FromBase64String(contents);
            await System.IO.File.WriteAllBytesAsync(name, saveToFile);
        }

        static async void SaveToFile(string contents, string name)
        {

            await File.WriteAllLinesAsync(name + ".json", new List<string>() { contents });
        }

        static string ReadFromFile(string name)
        {
            try
            {
                return File.ReadAllText(name + ".json");

            }
            catch(Exception e)
            {
                return "";
            }
            
        }
	}
}

