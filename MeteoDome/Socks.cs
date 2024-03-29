﻿#region

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

#endregion

namespace MeteoDome
{
    public class Socks
    {
        public static int Port;
        private readonly Thread _loop;
        private readonly TcpListener _server;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private TcpClient _tcpClient;

        public Socks()
        {
            _server = TcpListener.Create(Port);
            _loop = new Thread(MainManager)
            {
                IsBackground = true
            };
        }

        public void StartListening()
        {
            _server.Start();
            Logger.AddLogEntry(@"Сервер запущен. Ожидание подключений...");
            _loop.Start();
        }

        private async void MainManager()
        {
            while (true)
            {
                if (_server is null)
                {
                    Logger.AddLogEntry("Socks error: _server is null");
                    break;
                }

                _tcpClient = await _server.AcceptTcpClientAsync();
                Logger.AddLogEntry($"Входящее подключение: {_tcpClient.Client.RemoteEndPoint}");
                _streamReader = new StreamReader(_tcpClient.GetStream());
                _streamWriter = new StreamWriter(_tcpClient.GetStream());
                _streamWriter.AutoFlush = true;

                while (true)
                {
                    try
                    {
                        if (_tcpClient.Connected)
                        {
                            var get = await _streamReader.ReadLineAsync();
                            switch (get)
                            {
                                case "full":
                                    await _streamWriter.WriteLineAsync(WeatherDataCollector.GetStringSockMessage());
                                    break;
                                case "ping":
                                    await _streamWriter.WriteLineAsync("pong");
                                    break;
                                case "sky":
                                {
                                    // отправляем ответ
                                    await _streamWriter.WriteLineAsync(WeatherDataCollector.SkyTemp.ToString("00.00"));
                                    break;
                                }
                                case "sky std":
                                {
                                    await _streamWriter.WriteLineAsync(
                                        WeatherDataCollector.SkyTempStd.ToString("00.00"));
                                    break;
                                }
                                case "ext":
                                {
                                    await _streamWriter.WriteLineAsync(
                                        WeatherDataCollector.Extinction.ToString("00.00"));
                                    break;
                                }
                                case "ext std":
                                {
                                    await _streamWriter.WriteLineAsync(
                                        WeatherDataCollector.ExtinctionStd.ToString("00.00"));
                                    break;
                                }
                                case "see":
                                {
                                    await _streamWriter.WriteLineAsync(WeatherDataCollector.Seeing.ToString("00.00"));
                                    break;
                                }
                                case "see ext":
                                {
                                    await _streamWriter.WriteLineAsync(
                                        WeatherDataCollector.SeeingExtinction.ToString("00.00"));
                                    break;
                                }
                                case "wind":
                                {
                                    await _streamWriter.WriteLineAsync(WeatherDataCollector.Wind.ToString("00.00"));
                                    break;
                                }
                                case "sun":
                                {
                                    await _streamWriter.WriteLineAsync(WeatherDataCollector.SunZd.ToString("00.00"));
                                    break;
                                }
                                case "obs":
                                {
                                    await _streamWriter.WriteLineAsync(WeatherDataCollector.IsObsRunning.ToString());
                                    break;
                                }
                                case "flat":
                                {
                                    await _streamWriter.WriteLineAsync(WeatherDataCollector.IsFlat.ToString());
                                    break;
                                }
                                default:
                                {
                                    Logger.AddLogEntry($"Recieved unknown command: {get}");
                                    await _streamWriter.WriteLineAsync("unknw");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.AddLogEntry("Socks error");
                        Logger.AddLogEntry(e.Message);
                        Disconnect();
                    }
                }
            }
        }

        private void Disconnect()
        {
            _tcpClient.Dispose();
            Logger.AddLogEntry("Соединение разорвано");
        }

        // public void StopListening()
        // {
        //     Disconnect();
        //     _server.Stop();
        // }
    }
}