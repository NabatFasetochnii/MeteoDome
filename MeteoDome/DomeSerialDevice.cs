using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MeteoDome
{
    public class DomeSerialDevice
    {
        private static readonly Timer ComTimer = new Timer(); //timer for Serial port communication delay

        // private long _magicCounter = 0;
        // private BufferBlock<Consumer> consumers = new BufferBlock<Consumer>();
        private static readonly List<string> Waiters = new List<string>();

        private readonly string[] _commands =
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

        private readonly SerialPort _serialPort = new SerialPort();
        private Thread _loopThreadForQuery;
        public BitArray Buttons = new BitArray(8, false);

        //serial port
        public string ComId;

        public BitArray Dome = new BitArray(8, false);

        public int InitFlag;
        public Logger Logger;

        public BitArray Power = new BitArray(8, false);

        // public string Power = "";
        // public BitArray Timeout = new BitArray(8, false);

        // public string timeout = "";
        public int TimeoutNorth = 120;
        public int TimeoutSouth = 120;
        private bool _transmissionEnabled;

        public void Dispose()
        {
            Close_Port();
            _serialPort.Dispose();
            _loopThreadForQuery.Abort();
        }

        public bool Init()
        {
            ComTimer.Elapsed += OnTimedEvent_Com;
            ComTimer.Interval = 1000; // ожидание ответа микроконтроллера 1000мс
            _serialPort.DataReceived += SerialPort_DataReceived;
            // ComTimer.Start();
            OpenPort();
            _loopThreadForQuery = new Thread(Looper);
            _loopThreadForQuery.Start();
            return _transmissionEnabled;
        }

        private void Looper()
        {
            while (true)
            {
                if (!_serialPort.IsOpen) break;
                if (!_transmissionEnabled) continue;
                if (Waiters.Count == 0) continue;
                Write2Serial(Waiters[0]);
                Waiters.RemoveAt(0);
            }
        }

        private void OpenPort()
        {
            _serialPort.PortName = "COM" + ComId;
            _serialPort.BaudRate = 9600;
            _serialPort.DataBits = 8;
            try
            {
                _serialPort.Open();
                if (!_serialPort.IsOpen) return;
                _serialPort.ReadTimeout = 500;
                _serialPort.NewLine = "\0"; // Serial commands separator
                _serialPort.ReceivedBytesThreshold = 6;
                _serialPort.DiscardInBuffer(); // чистить порт после открытия
                Logger.AddLogEntry("SerialPort opened");
                _transmissionEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry("SerialPort opening fails");
                Logger.AddLogEntry(ex.ToString());
            }
        }

        private void Close_Port()
        {
            try
            {
                ComTimer.Stop();
                _transmissionEnabled = false;
                _serialPort.Close();
                Logger.AddLogEntry("SerialPort closed");
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry("SerialPort closing fails");
                Logger.AddLogEntry(ex.ToString());
            }
        }

        public void UpDate()
        {
            foreach (var command in _commands) AddTask(command);
        }

        public static void AddTask(string com)
        {
            if (!Waiters.Contains(com)) Waiters.Add(com);
        }

        //send command without answer
        private void Write2Serial(string command)
        {
            try
            {
                if (command is null) return;
                if (!_serialPort.IsOpen || !_transmissionEnabled) return;
                if (command[1] == 'r' || command[1] == 's') //if run command
                {
                    _transmissionEnabled = false;
                    _serialPort.WriteLine(command);
                    _transmissionEnabled = true;
                }

                if (command[1] != 'g') return;
                _serialPort.DiscardInBuffer(); //clear input buffer
                _serialPort.WriteLine(command); //send question
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
        private void OnTimedEvent_Com(object sender, ElapsedEventArgs e)
        {
            ComTimer.Stop();
            _transmissionEnabled = true;
        }

        //serial port reader
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ComTimer.Stop(); //stop timer

            var indata = "#";
            try
            {
                indata = _serialPort.ReadLine(); //read answer [1ap=1234]
                // Logger.AddLogEntry("get msg "+indata);
            }
            catch
            {
                // ignored
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
                        (bits[i], bits[bits.Count - i - 1]) = (bits[bits.Count - i - 1], bits[i]);
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
            }
            catch (Exception exception)
            {
                Logger.AddLogEntry("Can't read the dome answer " + indata);
                Logger.AddLogEntry(exception.Message);
            }

            try
            {
                _serialPort.ReadExisting(); //cleaning
            }
            catch
            {
                // ignored
            }

            _transmissionEnabled = true; //enable transmission
        }
    }
}