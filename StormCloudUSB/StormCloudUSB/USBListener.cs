using iMobileDevice;
using iMobileDevice.iDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudUSB
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
                        try
                        {
                            MessagingCenter.Send<MainPage, string>((MainPage)Application.Current.MainPage, "iOS_USB_CONNECT", "DISCONNECT");
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
                    MessagingCenter.Send<MainPage, string>((MainPage)Application.Current.MainPage, "iOS_USB_CONNECT", "CONNECT");
                }
                catch (Exception e)
                {

                }
                var error = _iDeviceApi.idevice_connect(deviceHandle, 5050, out iDeviceConnectionHandle connection);

                if (error != iDeviceError.Success) return;
                try
                {
                    MessagingCenter.Send<MainPage, string>((MainPage)Application.Current.MainPage, "iOS_USB_CONNECT", "SOCKET_CONNECT");
                }
                catch (Exception e)
                {

                }
                ReceiveDataFromDevice(connection);
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed USB Device Connect");
            }
            
        }

        private void ReceiveDataFromDevice(iDeviceConnectionHandle connection)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    uint receivedBytes = 0;
                    _iDeviceApi.idevice_connection_receive(connection, _inboxBuffer, (uint)_inboxBuffer.Length,
                        ref receivedBytes);
                    if (receivedBytes <= 0) continue;



                    var res = Encoding.ASCII.GetString(_inboxBuffer);
                    Console.WriteLine(res);

                    // Message back

                    MessagingCenter.Send<MainPage, string>((MainPage)Application.Current.MainPage, "iOS_USB_RES", res);
                }
            });
        }

    }
}
