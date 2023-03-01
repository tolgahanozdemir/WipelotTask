using System.Net.Sockets;

namespace WipelotTask.Client.Entities
{
    public class ClientSocket
    {
        public int UniqueNumber { get; set; }
        public int TimeStamp { get; set; }
        public int RandomValue { get; set; }
        public Socket Socket { get; set; }
    }
}
