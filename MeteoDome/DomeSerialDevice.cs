using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MeteoDome
{
    public static class DomeSerialDevice
    {
        private static readonly Timer ComTimer = new Timer(); //timer for Serial port communication delay
        private static readonly Timer TasksTimer = new Timer(); //timer for Waiters
        private static readonly List<string> Waiters = new List<string>();
        public static DateTime DomeUpdateDateTime;
        
        private static readonly string[] Commands =
        {
            "1gcp",
            "1gcb",
            "1gct",
            "1gcm",
            "1gtn",
            "1gts",
            "1gin",
            "1gca"
        };

        private static readonly SerialPort SerialPort = new SerialPort();
        public static BitArray Buttons = new BitArray(8, false);

        //serial port
        public static string ComId;

        public static BitArray Dome = new BitArray(8, false);

        public static int InitFlag;
        // public Logger Logger;

        public static BitArray Power = new BitArray(8, false);

        // public string Power = "";
        // public BitArray Timeout = new BitArray(8, false);

        // public string timeout = "";
        public static int TimeoutNorth = 120;
        public static int TimeoutSouth = 120;
        private static bool _transmissionEnabled;

        public static void Dispose()
        {
            Close_Port();
            SerialPort.Dispose();
            
        }

        public static bool Init()
        {
            ComTimer.Elapsed += OnTimedEvent_Com;
            ComTimer.Interval = 1000; // ожидание ответа микроконтроллера 1000мс
            SerialPort.DataReceived += SerialPort_DataReceived;
            ComTimer.Start();
            OpenPort();
            TasksTimer.Elapsed += Looper;
            TasksTimer.Interval = 140;
            TasksTimer.Start();
            return _transmissionEnabled;
        }
    
        private static void Looper(object sender, ElapsedEventArgs e)
        {
            if (!SerialPort.IsOpen) return;
            if (!_transmissionEnabled) return;
            if (Waiters.Count == 0) return;
            Write2Serial(Waiters[0]);
            Waiters.RemoveAt(0);
        }

        private static void OpenPort()
        {
            SerialPort.PortName = "COM" + ComId;
            SerialPort.BaudRate = 9600;
            SerialPort.DataBits = 8;
            try
            {
                SerialPort.Open();
                if (!SerialPort.IsOpen) return;
                SerialPort.ReadTimeout = 500;
                SerialPort.NewLine = "\0"; // Serial commands separator
                SerialPort.ReceivedBytesThreshold = 6;
                SerialPort.DiscardInBuffer(); // чистить порт после открытия
                Logger.AddLogEntry("SerialPort opened");
                _transmissionEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry("SerialPort opening fails");
                Logger.AddLogEntry(ex.ToString());
            }
        }

        private static void Close_Port()
        {
            try
            {
                ComTimer.Stop();
                _transmissionEnabled = false;
                SerialPort.Close();
                Logger.AddLogEntry("SerialPort closed");
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry("SerialPort closing fails");
                Logger.AddLogEntry(ex.ToString());
            }
        }

        public static void UpDate()
        {
            foreach (var command in Commands) AddTask(command);
        }

        public static void AddTask(string com)
        {
            if (!Waiters.Contains(com)) Waiters.Add(com);
        }

        //send command without answer
        private static void Write2Serial(string command)
        {
            try
            {
                if (command is null) return;
                if (!SerialPort.IsOpen || !_transmissionEnabled) return;
                if (command[1] == 'r' || command[1] == 's') //if run command
                {
                    _transmissionEnabled = false;
                    SerialPort.WriteLine(command);
                    _transmissionEnabled = true;
                }

                if (command[1] != 'g') return;
                SerialPort.DiscardInBuffer(); //clear input buffer
                SerialPort.WriteLine(command); //send question
                _transmissionEnabled = false; //disable transmission of next command
                ComTimer.Start(); //start 1000 ms timer for waiting
                //if question
            }
            catch (NullReferenceException e)
            {
                // FIXME: Sometimes program tries to send empty command
                //        Bad command queue management maybe?
                Logger.AddLogEntry(e.ToString());
                // Logger.AddLogEntry($"Magic #{++_magicCounter} in Write2Serial, empty command");
            }
        }

        //timer for waiting of reply from mc
        private static void OnTimedEvent_Com(object sender, ElapsedEventArgs e)
        {
            ComTimer.Stop();
            _transmissionEnabled = true;
        }

        //serial port reader
        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ComTimer.Stop(); //stop timer

            var indata = "#";
            try
            {
                indata = SerialPort.ReadLine(); //read answer [1ap=1234]
                // Logger.AddLogEntry("get msg "+indata);
            }
            catch (Exception exception)
            {
                // ignored
                Logger.AddLogEntry(exception.Message);
            }

            var bits = new BitArray(8);
            // var bits = "";
            var t = 0;

            try
            {
                var reply = indata.Substring(1, 3); //var reply = "___";
                if (reply == "ats" || reply == "atn")
                {
                    t = Convert.ToInt32(indata.Substring(5));
                }
                else
                {
                    var value = Convert.ToByte(indata.Substring(5));
                    var gar = BitConverter.GetBytes(value);
                    bits = new BitArray(new[] {gar[0]});
                    for (var i = 0; i < bits.Count / 2; i++) // HACK Reverse order of bits variable
                    {
                        var buf = bits[i];
                        bits[i] = bits[bits.Count - i - 1];
                        bits[bits.Count - i - 1] = buf;
                    }
                }

                // var msg = "";
                switch (reply)
                {
                    case "aca": //Motors*16 + end switches
                        // запрос состояния моторов и концевиков   //  1gca;   //  1aca=[byte];    //  возвращает байт nc no sc so nc no sc so (Motors*16 + Switches)
                        Dome = bits;
                        break;
                    case "acp": //power
                        // запрос состояния питания моторов//1gcp;//1acp=[byte];//возвращает байт u  u  u  u  u  pn ps ee
                        Power = bits;
                        break;
                    case "acb":
                        // запрос состояния кнопок//	1gcb;//	1acb=[byte];//	возвращает байт u  u  u  u  nc no sc so
                        Buttons = bits;
                        break;
                    // case "act":
                    //     // запрос состояния таймаутов//1gct;//1act=[byte];//	возвращает байт u  u  u  u  nc no sc so
                    //     Timeout = bits;
                    //     break;
                    case "ats":
                        // запрос значения южного таймаута			//	1gts;	//	1ats=[int];		//	возвращает значение в секундах
                        TimeoutSouth = t;
                        break;
                    case "atn":
                        //запрос значения северного таймаута		//	1gtn;	//	1atn=[int];		//	возвращает значение в секундах
                        TimeoutNorth = t;
                        break;
                    case "ain":
                        InitFlag = int.Parse(indata.Substring(5, 1));
                        break;
                }
                DomeUpdateDateTime = DateTime.UtcNow;
            }
            catch (Exception exception)
            {
                Logger.AddLogEntry("Can't read the dome answer " + indata);
                Logger.AddLogEntry(exception.Message);
            }

            try
            {
                SerialPort.ReadExisting(); //cleaning
            }
            catch
            {
                // ignored
            }

            _transmissionEnabled = true; //enable transmission
        }
    }
}