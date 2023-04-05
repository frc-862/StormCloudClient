using StormCloudClient.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudClient
{

    public class USBPacket
    {
        public List<Match> matches;
        public List<Photo> photos;
        public List<PhotoData> b64Photos;
    }
    public class USBService
    {
        public static AsyncCallback cb = async (ac) =>
        {
            var connectionAttempt = (Socket)ac.AsyncState;
            var connectedSocket = connectionAttempt.EndAccept(ac);

            // need to send latest matches and photos

            USBPacket transfer = new USBPacket();
            transfer.matches = StorageManagement.allMatches.Where(m => m.Status != UploadStatus.SUCCEEDED).ToList();
            transfer.photos = StorageManagement.allPhotos.Where(p => p.Status != UploadStatus.SUCCEEDED).ToList();

            transfer.b64Photos = new List<PhotoData>();
            //foreach(Photo p in transfer.photos)
            //{
            //    var n = new PhotoData() { Name = p.Path };
            //    n.Data = StorageManagement.GetPhotoB64(p.Path);
            //    transfer.b64Photos.Add(n);
            //}

            var sendString = Newtonsoft.Json.JsonConvert.SerializeObject(transfer);
            connectedSocket.SendBufferSize = 1024;
            connectedSocket.ReceiveBufferSize = 1024;



            var totalSent = 0;
            var result = new byte[1024];

            connectedSocket.Receive(result);

            while (sendString != "")
            {

                if (sendString.Length > 300)
                {
                    totalSent += connectedSocket.Send(Encoding.ASCII.GetBytes(sendString.Substring(0, 300)));
                    sendString = sendString.Substring(300);
                }
                else
                {
                    totalSent += connectedSocket.Send(Encoding.ASCII.GetBytes(sendString.PadRight(300, ' ')));
                    sendString = "";
                }

                connectedSocket.Receive(result);
                
            }

            await Task.Delay(200);
            //connectedSocket.Receive(result);
            connectedSocket.Send(Encoding.ASCII.GetBytes("END".PadRight(300, ' ')));

            var response = "";
            var gotString = "";
            while(true)
            {
                byte[] byteResult = new byte[1024];
                connectedSocket.Receive(byteResult);
                var responseGet = Encoding.ASCII.GetString(byteResult).Replace("\0", "");
                if(responseGet.StartsWith("ok, and?"))
                {
                    connectedSocket.Send(Encoding.ASCII.GetBytes("ok, and?".PadRight(300, ' ')));
                }
                else if(!responseGet.StartsWith("END"))
                {
                    gotString += responseGet;
                    connectedSocket.Send(Encoding.ASCII.GetBytes("ok, and?".PadRight(300, ' ')));
                }
                else
                {
                    break;
                }
                
            }


            try{
                if(gotString != ""){
                    var responsePacket = Newtonsoft.Json.JsonConvert.DeserializeObject(gotString);

                    var responseDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(gotString);

                    if (responseDict.ContainsKey("analysises"))
                        HandleAnalysis(responseDict["analysises"]);

                    if(responseDict.ContainsKey("matches"))
                        HandleMatches(responseDict["matches"]);

                    if(responseDict.ContainsKey("rankings"))
                        HandleRankings(responseDict["rankings"]);
                    
                    if(responseDict.ContainsKey("documents"))
                        HandleDocuments(responseDict["documents"]);

                    if (responseDict.ContainsKey("teams"))
                        HandleTeams(responseDict["teams"]);

                    foreach (var match in transfer.matches)
                    {
                        match.Status = UploadStatus.SUCCEEDED;
                    }

                }
            }catch(Exception rex){
                Console.WriteLine(rex.Message);
            }
            

            

            // send message back to app

            MessageContent messageSend = new MessageContent()
            {
                Type = MessageType.USB_SEND,
                Data = "Sent " + totalSent.ToString() + " Bytes"
            };

            MessagingCenter.Send<NavigationPage, MessageContent>((NavigationPage)Application.Current.MainPage, "USB", messageSend);

            socket.BeginAccept(cb, socket);


        };
        public static Socket socket;
        public void StartService()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 5050));
            socket.Listen(100);
            socket.BeginAccept(cb, socket);
        }

        public static void HandleSchemas(dynamic schemas)
        {
            foreach(var schema in schemas)
            {
                StorageManagement.AddData_Schema((string)schema["Name"], Newtonsoft.Json.JsonConvert.SerializeObject(schema), (dynamic)schema["Settings"]);
            }
            StorageManagement._SaveData_Schema();
        }

        public static void HandleAnalysis(dynamic analysises){
            Console.WriteLine("Got Analysis");

            StorageManagement.downloadCache.analysisSets = analysises;
            if(StorageManagement.dataDeterminer.analysisSets == null)
            {
                StorageManagement.dataDeterminer.analysisSets = new TimeSet()
                {
                    usb = DateTime.Now
                };
            }
            else
            {
                StorageManagement.dataDeterminer.analysisSets.usb = DateTime.Now;
            }

            StorageManagement._SaveData_Download();
            
        }

        public static void HandleMatches(dynamic matches){
            Console.WriteLine("Got Matches");

            StorageManagement.downloadCache.matches = matches;
            if (StorageManagement.dataDeterminer.matches == null)
            {
                StorageManagement.dataDeterminer.matches = new TimeSet()
                {
                    usb = DateTime.Now
                };
            }
            else
            {
                StorageManagement.dataDeterminer.matches.usb = DateTime.Now;
            }

            StorageManagement._SaveData_Download();
        }

        public static void HandleRankings(dynamic teams){
            Console.WriteLine("Got Teams");

            StorageManagement.downloadCache.rankings = teams;
            if (StorageManagement.dataDeterminer.rankings == null)
            {
                StorageManagement.dataDeterminer.rankings = new TimeSet()
                {
                    usb = DateTime.Now
                };
            }
            else
            {
                StorageManagement.dataDeterminer.rankings.usb = DateTime.Now;
            }

            StorageManagement._SaveData_Download();
        }

        public static void HandleDocuments(dynamic documents){
            Console.WriteLine("Got Documents");

            StorageManagement.downloadCache.documents = documents;
            if (StorageManagement.dataDeterminer.documents == null)
            {
                StorageManagement.dataDeterminer.documents = new TimeSet()
                {
                    usb = DateTime.Now
                };
            }
            else
            {
                StorageManagement.dataDeterminer.documents.usb = DateTime.Now;
            }

            StorageManagement._SaveData_Download();
        }

        public static void HandleTeams(dynamic teams)
        {
            Console.WriteLine("Got Teams");

            List<dynamic> teamHolder = new List<dynamic>();
            foreach(var team in teams)
            {
                teamHolder.Add(team);
            }

            List<dynamic> teamsSorted = Sort(teamHolder, "teamNumber");



            StorageManagement.downloadCache.teams = Newtonsoft.Json.JsonConvert.DeserializeObject(Newtonsoft.Json.JsonConvert.SerializeObject(teamsSorted));
            if (StorageManagement.dataDeterminer.teams == null)
            {
                StorageManagement.dataDeterminer.teams = new TimeSet()
                {
                    usb = DateTime.Now
                };
            }
            else
            {
                StorageManagement.dataDeterminer.teams.usb = DateTime.Now;
            }

            StorageManagement._SaveData_Download();
        }

        



        public static dynamic GetMatch(int matchNumber)
        {
            try
            {
                dynamic matchToReturn = null;
                foreach (var match in StorageManagement.downloadCache.matches)
                {
                    if((string)match.matchNumber == matchNumber.ToString())
                    {
                        matchToReturn = match;
                        break;
                    }
                }
                return matchToReturn;
            }catch(Exception e)
            {
                return null;
            }
            
        }

        public static dynamic GetTeam(int teamNumber)
        {
            try
            {
                dynamic teamToReturn = null;
                foreach(var team in StorageManagement.downloadCache.teams)
                {
                    if((string)team.teamNumber == teamNumber.ToString())
                    {
                        teamToReturn = team;
                        break;
                    }
                }
                return teamToReturn;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static dynamic GetAnalysis(int teamNumber)
        {
            try
            {
                Dictionary<string, dynamic> analysisData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(StorageManagement.downloadCache.analysisSets);
                dynamic analysisToReturn = null;
                foreach(var team in analysisData.Keys)
                {
                    if(team == teamNumber.ToString())
                    {
                        analysisToReturn = analysisData[team];
                        break;
                    }
                }
                return analysisToReturn;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static List<dynamic> Sort(List<dynamic> input, string property)
        {
            return input.OrderBy(p => p.GetType().GetProperty(property).GetValue(p, null)).ToList();
        }
    }

}
