using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WipelotTask.Client.Entities;

namespace WipelotTask.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private Random _random = new Random();
        private List<ClientSocket> _clientSockets = new List<ClientSocket>();

        [HttpGet("createsockets")]
        public async Task<IActionResult> CreateSockets(int clientNumber)
        {
            //Soketleri oluşturur
            for (int i = 0; i < clientNumber; i++)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ClientSocket clientSocket = new ClientSocket
                {
                    UniqueNumber = i + 1,
                    TimeStamp = 0,
                    RandomValue = _random.Next(0, 3001),
                    Socket = socket
                };
                _clientSockets.Add(clientSocket);
            }

            //Porta bağlanması için sürekli istek atmasını sağlar.
            LoopConnect();

            //Porta veri gönderme işlemini gerçekleştirir
            SendLoop();
            return Ok(_clientSockets);
        }
        private void LoopConnect()
        {
            int attempts = 0;
            foreach (var clientSocket in _clientSockets)
            {
                //Soket bağlanana kadar döngüde kalmasını sağlar.
                while (!clientSocket.Socket.Connected)
                {
                    try
                    {
                        attempts++;
                        clientSocket.Socket.Connect(IPAddress.Loopback, 7000);
                        Thread.Sleep(500);
                    }
                    catch (SocketException)
                    {
                    }
                }
            }
        }
        private void SendLoop()
        {
            //Eklemelerin yapılabilmesi için ilk oluşturulan random değerleri bir diziye alır.
            List<int> randomValues = new List<int>();
            for (int i = 0; i < _clientSockets.Count; i++)
            {
                randomValues.Add(_clientSockets[i].RandomValue);
            }
            //Porta bağlı olduğu sürece döngünün içinde kalarak her saniye porta mesaj gönderir.
            while (_clientSockets[0].Socket.Connected)
            {

                try
                {
                    for (int i = 0; i < _clientSockets.Count; i++)
                    {
                        string dataToSend =
                            $"Unique Number:{_clientSockets[i].UniqueNumber}/" +
                            $"Time Stamp:{_clientSockets[i].TimeStamp}/" +
                            $"Random Value:{_clientSockets[i].RandomValue}";
                        byte[] buffer = Encoding.ASCII.GetBytes(dataToSend);
                        _clientSockets[i].Socket.Send(buffer);

                        byte[] receivedBuf = new byte[1024];
                        int rec = _clientSockets[i].Socket.Receive(receivedBuf);
                        byte[] data = new byte[rec];
                        Array.Copy(receivedBuf, data, rec);

                        _clientSockets[i].TimeStamp++;
                        int randomValue = randomValues[i];
                        randomValue += _random.Next(-100, 101);
                        randomValue = Math.Max(randomValue, 1);
                        randomValue = Math.Min(randomValue, 3000);
                        _clientSockets[i].RandomValue = randomValue;
                    }
                }
                catch (Exception)
                {
                }
                Thread.Sleep(1000);
            }
        }

        
    }
}