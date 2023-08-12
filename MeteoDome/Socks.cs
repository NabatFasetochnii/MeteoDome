#region

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

#endregion

namespace MeteoDome
{
    public class Socks
    {
        private const int Port = 8085;
        private readonly bool _isFlat;
        private readonly bool _isObsRunning;
        private readonly Logger _logger;
        private readonly Thread _loop;
        private readonly double[] _seeing;
        private readonly TcpListener _server;
        private readonly double[] _skyIr;
        private readonly double[] _skyVis;
        private readonly double _sunZd;
        private readonly double _wind;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private TcpClient _tcpClient;

        public Socks(Logger logger, ref double[] seeing, ref double[] skyIr, ref double[] skyVis,
            ref double sunZd, ref double wind, ref bool isFlat, ref bool isObsRunning)
        {
            _logger = logger;
            _server = TcpListener.Create(Port);
            _loop = new Thread(MainManager)
            {
                IsBackground = true
            };
            _seeing = seeing;
            _skyIr = skyIr;
            _skyVis = skyVis;
            _sunZd = sunZd;
            _wind = wind;
            _isFlat = isFlat;
            _isObsRunning = isObsRunning;
        }

        public void StartListening()
        {
            _server.Start();
            _logger.AddLogEntry(@"Сервер запущен. Ожидание подключений...");
            _loop.Start();
        }

        private async void MainManager()
        {
            while (true)
            {
                try
                {
                    _tcpClient = await _server.AcceptTcpClientAsync();
                    _logger.AddLogEntry($"Входящее подключение: {_tcpClient.Client.RemoteEndPoint}");
                    _streamReader = new StreamReader(_tcpClient.GetStream());
                    _streamWriter = new StreamWriter(_tcpClient.GetStream());
                    if (_tcpClient.Connected)
                    {
                        var get = await _streamReader.ReadLineAsync();
                        // if (get == "stop")
                        // {
                        //     Disconnect();
                        //     continue;
                        // }
                        switch (get)
                        {
                            case "sky":
                            {
                                // отправляем ответ
                                await _streamWriter.WriteLineAsync(_skyIr[0].ToString("00.0"));
                                break;
                            }
                            case "sky std":
                            {
                                await _streamWriter.WriteLineAsync(_skyIr[1].ToString("00.0"));
                                break;
                            }
                            case "extinction":
                            {
                                await _streamWriter.WriteLineAsync(_skyVis[0].ToString("00.0"));
                                break;
                            }
                            case "extinction std":
                            {
                                await _streamWriter.WriteLineAsync(_skyVis[1].ToString("00.0"));
                                break;
                            }
                            case "seeing":
                            {
                                await _streamWriter.WriteLineAsync(_seeing[0].ToString("00.0"));
                                break;
                            }
                            case "seeing_extinction":
                            {
                                await _streamWriter.WriteLineAsync(_seeing[1].ToString("00.0"));
                                break;
                            }
                            case "wind":
                            {
                                await _streamWriter.WriteLineAsync(_wind.ToString("00.0"));
                                break;
                            }
                            case "sun":
                            {
                                await _streamWriter.WriteLineAsync(_sunZd.ToString("00.0"));
                                break;
                            }
                            case "obs":
                            {
                                await _streamWriter.WriteLineAsync(_isObsRunning.ToString());
                                break;
                            }
                            case "flat":
                            {
                                await _streamWriter.WriteLineAsync(_isFlat.ToString());
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.AddLogEntry("Socks error");
                    _logger.AddLogEntry(e.Message);
                    Disconnect();
                }
            }
        }

        private void Disconnect()
        {
            _streamReader.Close();
            _streamWriter.Close();
            _tcpClient.Dispose();
            _logger.AddLogEntry("Соединение разорвано");
        }

        public void StopListening()
        {
            Disconnect();
            _server.Stop();
        }
    }
}