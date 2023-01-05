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

    public class iOSUSBPacket
    {
        public List<Match> matches;
        public List<Photo> photos;
        public List<string> b64Photos;
    }
    public partial class USBService
    {
        public static AsyncCallback cb = (ac) =>
        {
            var connectionAttempt = (Socket)ac.AsyncState;
            var connectedSocket = connectionAttempt.EndAccept(ac);

            // need to send latest matches and photos

            iOSUSBPacket transfer = new iOSUSBPacket();
            transfer.matches = StorageManagement.allMatches.Where(m => m.Status != UploadStatus.SUCCEEDED).ToList();
            transfer.photos = StorageManagement.allPhotos.Where(p => p.Status != UploadStatus.SUCCEEDED).ToList();

            var sendString = Newtonsoft.Json.JsonConvert.SerializeObject(transfer);
            connectedSocket.SendBufferSize = 1024;
            var sent = connectedSocket.Send(Encoding.ASCII.GetBytes(sendString));
            Console.WriteLine(sent);

            foreach(var match in transfer.matches)
            {
                match.Status = UploadStatus.SUCCEEDED;
            }


            // send message back to app

            MessageContent messageSend = new MessageContent()
            {
                Type = MessageType.USB_SEND,
                Data = "Sent " + sent.ToString() + " Bytes"
            };

            MessagingCenter.Send<NavigationPage, MessageContent>((NavigationPage)Application.Current.MainPage, "USB", messageSend);

            socket.BeginAccept(cb, socket);


        };
        public static Socket socket;
        public partial void StartService()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 5050));
            socket.Listen(100);
            socket.BeginAccept(cb, socket);
        }
    }
}
