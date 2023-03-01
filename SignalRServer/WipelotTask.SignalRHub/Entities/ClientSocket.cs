namespace WipelotTask.SignalRHub.Entities
{
    public class ClientSocket
    {
        public ClientSocket(int uniqueNumber, int timeStamp, int randomValue)
        {
            UniqueNumber = uniqueNumber;
            TimeStamp = timeStamp;
            RandomValue = randomValue;
        }

        public int UniqueNumber { get; set; }
        public int TimeStamp { get; set; }
        public int RandomValue { get; set; }
    }
}
