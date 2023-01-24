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

        public class USBPacket
        {
            public List<Match> matches;
            public List<Photo> photos;
            public List<string> b64Photos;
        }
        public static void HandleUSBPushData(string data)
		{
            var unpacked = Newtonsoft.Json.JsonConvert.DeserializeObject<USBPacket>(data);

            APIService.SendMatches(unpacked.matches);
		}
	}
}

