using iMobileDevice;
using iMobileDevice.iDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StormCloudUSBService
{
    public class USBListener
    {


        public iDeviceApi _iDeviceApi = new iDeviceApi(LibiMobileDevice.Instance);
        public iDeviceEventCallBack _eventCallback;
        public byte[] _inboxBuffer = new byte[1024];

        public USBListener()
        {

        }

        public void BeginListening()
        {
            _iDeviceApi.idevice_event_subscribe(EventCallback(), new IntPtr());
        }

        private iDeviceEventCallBack EventCallback()
        {
            return (ref iDeviceEvent devEvent, IntPtr data) =>
            {
                switch (devEvent.@event)
                {
                    case iDeviceEventType.DeviceAdd:
                        Connect(devEvent.udidString);

                        

                        break;
                    case iDeviceEventType.DeviceRemove:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Disconnected...");
                        try
                        {
                            //MessagingCenter.Send<MainPage, string>((MainPage)Application.Current.MainPage, "iOS_USB_CONNECT", "DISCONNECT");
                        }
                        catch (Exception e)
                        {

                        }
                        break;
                    default:
                        return;
                }
            };
        }
        private void Connect(string newUdid)
        {
            try
            {
                _iDeviceApi.idevice_new(out iDeviceHandle deviceHandle, newUdid).ThrowOnError();
                try
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Connected Device");
                    //MessagingCenter.Send<MainPage, string>((MainPage)Application.Current.MainPage, "iOS_USB_CONNECT", "CONNECT");
                }
                catch (Exception e)
                {

                }
                var error = _iDeviceApi.idevice_connect(deviceHandle, 5050, out iDeviceConnectionHandle connection);

                if (error != iDeviceError.Success) return;
                try
                {
                    //MessagingCenter.Send<MainPage, string>((MainPage)Application.Current.MainPage, "iOS_USB_CONNECT", "SOCKET_CONNECT");
                }
                catch (Exception e)
                {

                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Receiving Data from Device");
                ReceiveDataFromDevice(connection);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed USB Device Connect");
            }

        }

        private void ReceiveDataFromDevice(iDeviceConnectionHandle connection)
        {
            Task.Run(() =>
            {
                var currentString = "";
                while (true)
                {
                    uint receivedBytes = 0;
                    _iDeviceApi.idevice_connection_receive(connection, _inboxBuffer, (uint)_inboxBuffer.Length,
                        ref receivedBytes);
                    if (receivedBytes <= 0) continue;


                    

                    var res = Encoding.ASCII.GetString(_inboxBuffer);
                    if(res.Contains("END"))
                    {
                        currentString = currentString.Replace("\u0000", "");
                        Console.WriteLine(currentString);
                        DataManager.HandleUSBPushData(currentString);
                        currentString = "";
                    }
                    currentString += res;



                    var response = Encoding.ASCII.GetBytes("CONTINUE");
                    uint sentBytes = 0;
                    _iDeviceApi.idevice_connection_send(connection,response, (uint)response.Length, ref sentBytes);

                    // Message back

                    //MessagingCenter.Send<MainPage, string>((MainPage)Application.Current.MainPage, "iOS_USB_RES", res);
                }
            });
        }

    }
}
