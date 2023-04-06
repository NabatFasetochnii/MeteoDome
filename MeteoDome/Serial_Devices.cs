using System;
using System.Collections;
using System.IO.Ports;
using System.Linq;
using System.Timers;

namespace MeteoDome
{
    public class SerialDevices
    {
        private static readonly Timer ComTimer = new Timer(); //timer for Serial port communication delay
        private readonly SerialPort _serialPort = new SerialPort();
        public BitArray buttons = new BitArray(8, false);

        //serial port
        public string ComId;
        public BitArray Dome = new BitArray(8, false);
        public int initflag;
        public Logger Logger;
        public BitArray Power = new BitArray(8, false);
        public BitArray timeout = new BitArray(8, false);
        public int timeout_north = 120;
        public int timeout_south = 120;
        public bool TransmissionEnabled;

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        SerialPort.Dispose();
        //    }
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        public bool Init()
        {
            ComTimer.Elapsed += OnTimedEvent_Com;
            ComTimer.Interval = 1000; // ожидание ответа микроконтроллера 1000мс
            _serialPort.DataReceived += SerialPort_DataReceived;
            // ComTimer.Start();
            Open_Port();
            return TransmissionEnabled;
        }

        private void Open_Port()
        {
            _serialPort.PortName = "COM" + ComId;
            _serialPort.BaudRate = 9600;
            _serialPort.DataBits = 8;
            try
            {
                _serialPort.Open();
                if (!_serialPort.IsOpen) return;
                _serialPort.ReadTimeout = 500;
                _serialPort.NewLine = "\0";
                _serialPort.ReceivedBytesThreshold = 6;
                _serialPort.DiscardInBuffer(); // чистить порт после открытия
                Logger.AddLogEntry("SerialPort opened");
                TransmissionEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry("SerialPort opening fails");
                Logger.AddLogEntry(ex.ToString());
            }
        }

        public void Close_Port()
        {
            try
            {
                ComTimer.Stop();
                TransmissionEnabled = false;
                _serialPort.Close();
                Logger.AddLogEntry("SerialPort closed");
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry("SerialPort closing fails");
                Logger.AddLogEntry(ex.ToString());
            }
        }

        //send command without answer
        public void Write2Serial(string command)
        {
            if (!_serialPort.IsOpen || !TransmissionEnabled) return;
            // Logger.AddLogEntry("send msg: " + command);
            if (command[1] == 'r' || command[1] == 's') //if run command
            {
                TransmissionEnabled = false;
                _serialPort.WriteLine(command);
                TransmissionEnabled = true;
            }

            if (command[1] == 'g') //if question
            {
                _serialPort.DiscardInBuffer(); //clear input buffer
                _serialPort.WriteLine(command); //send question
                TransmissionEnabled = false; //disable transmission of next command
                ComTimer.Start(); //start 1000 ms timer for waiting
            }
        }

        //timer for waiting of reply from mc
        private void OnTimedEvent_Com(object sender, ElapsedEventArgs e)
        {
            ComTimer.Stop();
            TransmissionEnabled = true;
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

            var reply = "___";
            byte value;
            var bits = new BitArray(8);

            try
            {
                reply = indata.Substring(1, 3);
                value = Convert.ToByte(indata.Substring(indata.IndexOf('=') + 1));
                bits = new BitArray(BitConverter.GetBytes(value).ToArray());
                // var msg = "";
                switch (reply)
                {
                    case "aca": //Motors*16 + end switches
                        // запрос состояния моторов и концевиков   //  1gca;   //  1aca=[byte];    //  возвращает байт nc no sc so nc no sc so (Motors*16 + Switches)
                        Dome = bits;
                        Logger.AddLogEntry("Dome");
                        break;
                    case "acp": //power
                        // запрос состояния питания моторов//1gcp;//1acp=[byte];//возвращает байт u  u  u  u  u  pn ps ee
                        Power = bits;

                        // if (Power[5] & Power[6])
                        // {
                        //     msg = "Оба мотора запитаны";
                        // }
                        // else if (Power[5])
                        // {
                        //     msg = "Запитан только северный мотор";
                        // }
                        // else if (Power[6])
                        // {
                        //     msg = "Запитан только южный мотор";
                        // }
                        // else
                        // {
                        //     msg = "Оба мотора без питания";
                        // }
                        // logger.AddLogEntry(msg);
                        break;
                    case "acb":
                        // запрос состояния кнопок//	1gcb;//	1acb=[byte];//	возвращает байт u  u  u  u  nc no sc so
                        buttons = bits;
                        break;
                    case "act":
                        // запрос состояния таймаутов//1gct;//1act=[byte];//	возвращает байт u  u  u  u  nc no sc so
                        timeout = bits;
                        break;
                    case "ats":
                        // запрос значения южного таймаута			//	1gts;	//	1ats=[int];		//	возвращает значение в секундах
                        timeout_south = int.Parse(indata.Substring(5, 3));
                        break;
                    case "atn":
                        //запрос значения северного таймаута		//	1gtn;	//	1atn=[int];		//	возвращает значение в секундах
                        timeout_north = int.Parse(indata.Substring(5, 3));
                        break;
                    case "ain":
                        initflag = int.Parse(indata.Substring(5, 1));
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.AddLogEntry("Can't read the dome answer " + indata);
                // Logger.AddLogEntry(exception.Message);
            }

            try
            {
                indata = _serialPort.ReadExisting(); //cleaning
            }
            catch
            {
                // ignored
            }

            TransmissionEnabled = true; //enable transmission
        }
    }
}