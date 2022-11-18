using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudClient.Classes
{
    public class Document
    {
        public int TeamNumber;
        public int MatchNumber;
        public string AllianceColor;
        public string ScouterName;

        public string JSONData;

        public bool Uploaded;
        public DateTime Created;
    }
}
