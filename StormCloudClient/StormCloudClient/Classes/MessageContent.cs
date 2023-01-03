using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormCloudClient.Classes
{
    public enum MessageType
    {
        USB_RECV, 
        USB_SEND
    }
    public class MessageContent
    {
        public MessageType Type;
        public string Data;
    }
}
