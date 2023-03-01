using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WipelotTask.RabbitMQProducer;

namespace WipelotTask.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private byte[] _buffer = new byte[1024];
        private List<Socket> _sockets = new List<Socket>();
        private IInitial _initial;
        //Server için soket oluşturur.
        private Socket _serverSocket = new
            Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public ServersController(IInitial initial)
        {
            _initial = initial;
        }

        [HttpGet("activateserver")]
        public IActionResult ActivateServer()
        {
            SetupServer();
            return Ok();
        }

        private void SetupServer()
        {
            //Portumuza endpoint noktası tanımlıyoruz.
            _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 7000));
            //Bağlanma isteklerini dinler.
            _serverSocket.Listen(5);
            //Bağlanma isteği gönderen portları bağlar.
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        //Bağlanma isteklerini kabul eder.
        private void AcceptCallback(IAsyncResult AR)
        {
            //bağlantı isteği kabul edilen soketleri _sockets adlı listeye kaydediyoruz.
            Socket socket = _serverSocket.EndAccept(AR);
            _sockets.Add(socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        //Soketlerin gönderdiği mesajları alıp _initial.Publish() ile RabbitMQ'ya gönderir.
        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                Socket socket = (Socket)AR.AsyncState;
                int received = socket.EndReceive(AR);
                //byte array olarak gelen veri
                byte[] dataBuf = new byte[received];
                Array.Copy(_buffer, dataBuf, received);
                //gelen veriyi string'e dönüştürüyoruz
                string text = Encoding.ASCII.GetString(dataBuf);
                //verilerin uygun olup olmadığını kontrol ediyoruz.
                var checkedData = CheckData(text);
                if (checkedData)
                {
                    byte[] data = Encoding.ASCII.GetBytes(text);
                    //gelen verileri rabbitMQ'ya gönderiyoruz.
                    _initial.Publish(data);
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                }
            }
            catch (Exception)
            {
            }
        }

        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        private bool CheckData(string text)
        {
            var datas = text.Split('/').ToList();
            if (text.Split('/').Length == 3 &&
                    text.Split('/')[0].StartsWith("Unique Number:") &&
                    text.Split('/')[1].StartsWith("Time Stamp:") &&
                    text.Split('/')[2].StartsWith("Random Value:") &&
                    Regex.Match(text.Split('/')[0], @"\d+").Value != "" &&
                    Regex.Match(text.Split('/')[1], @"\d+").Value != "" &&
                    Regex.Match(text.Split('/')[2], @"\d+").Value != "")
            {
                return true;
            }
            return false;
        }
    }
}