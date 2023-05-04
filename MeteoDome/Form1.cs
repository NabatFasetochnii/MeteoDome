using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace MeteoDome
{
    public partial class MainForm : Form
    {
        private const int ZdNight = 102;
        private const int ZdFlat = 96;
        private const int ClearAlert = -18;
        private const int Clear = -20;
        private const int WeatherForCloseDome = -16;
        private const string BadString = "Weather is bad";
        private const string GoodString = "Weather is good";
        private const double Tolerance = 1e-8;
        private static readonly string Path = Directory.GetCurrentDirectory();

        private static readonly Timer MeteoTimer = new Timer(); //clock timer and status check timer

        private readonly Logger _logger;
        // private static readonly Timer DomeTimer = new Timer(); //clock timer and status check timer

        ////globals for database
        private readonly MeteoDb _meteo;

        ////globals for dome
        private readonly SerialDevices _serialDevices = new SerialDevices();

        // private int _counter;
        private bool _br;
        private bool _isDomeCanOpen;
        private bool _isFlat;
        private bool _isObsCanRun;
        private bool _isObsRunning;
        private bool _isShutterNorthOpen;
        private bool _isShutterSouthOpen;
        private BitArray _dome = new BitArray(8);
        private int _isWeatherGood = -1;
        private double[] _seeing = {-1, -1};
        private double[] _skyIr = {-1, -1};
        private double[] _skyVis = {-1, -1};
        private double _sunZd = -1;
        private double _wind = -1;
        private bool _work = true;
        private readonly Thread _socketThread;

        public MainForm()
        {
            InitializeComponent();

            _logger = new Logger(logBox);
            _serialDevices.Logger = _logger;
            _meteo = new MeteoDb(_logger);

            if (!Read_Cfg())
                if (MessageBox.Show(@"Can't read config", @"OK", MessageBoxButtons.OK) == DialogResult.OK)
                    Environment.Exit(1);

            if (!_serialDevices.Init())
                if (MessageBox.Show(@"Can't open Dome serial port", @"OK", MessageBoxButtons.OK) == DialogResult.OK)
                    Environment.Exit(1);
            //create timer for main loop
            MeteoTimer.Elapsed += OnTimedEvent_Clock;
            MeteoTimer.Interval = 1000;
            MeteoTimer.Start();

            _socketThread = new Thread(socket_manager);
            _socketThread.Start();
        }
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _work = false;
            _serialDevices.Dispose();
            _socketThread.Abort();
            timer1.Stop();
        }

        //one second timer for status and clock
        private void OnTimedEvent_Clock(object sender, ElapsedEventArgs e)
        {
            // if(!getDomeThread.IsAlive)
            //     getDomeThread.Start();
            var getMeteoThread = new Thread(GetMeteo);
            getMeteoThread.Start();
            _serialDevices.UpDate();
        }

        private void GetMeteo()
        {
            _skyIr = _meteo.Get_Sky_IR(); // return [sky_tmp, sky_tmp_std], [-1,-1] - disconnected, [0,-1] - old data
            _skyVis = _meteo
                .Get_Sky_VIS(); // return [extinction, extinction_std], [-1,-1] - disconnected, [0,-1] - old data
            _seeing = _meteo
                .Get_Seeing(); // return [seeing, seeing_extinction], [-1,-1] - disconnected, [0,-1] - old data
            _wind = _meteo.Get_Wind(); // return wind, -1 - disconnected, 100 - old data
            _sunZd = _meteo.Sun_ZD(); //return Sun zenith distance (degree)
            if ((_skyVis[0] > 0) & (_skyVis[1] > 0))
                _isObsCanRun = MeteoDb.Get_Weather_Obs(_isObsCanRun, _skyVis[0], _skyVis[1]);
            if ((_skyIr[0] < -1) & (_skyIr[1] > 0) & (-1 < _wind) & (_wind < 100))
                _isDomeCanOpen =
                    MeteoDb.Get_Weather_Dome(_isShutterNorthOpen & _isShutterSouthOpen, _skyIr[0], _skyIr[1], _wind);
        }

        private void SetMeteo()
        {
            if (Math.Abs(_skyIr[0] - -1) < Tolerance)
            {
                void Action()
                {
                    label_SkyTemp.Text = @"Sky temperature (deg): disconnected";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else if ((_skyIr[0] == 0) & (Math.Abs(_skyIr[1] - -1) < Tolerance))
            {
                void Action()
                {
                    label_SkyTemp.Text = @"Sky temperature (deg): old data";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else
            {
                void Action()
                {
                    label_SkyTemp.Text = @"Sky temperature (deg): " + _skyIr[0].ToString("00.0");
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }

            void Act1()
            {
                label_SkyTempSTD.Text = @"Sky temperature STD (deg): " + _skyIr[1].ToString("00.00");
            }

            if (InvokeRequired)
                Invoke((Action) Act1);
            else
                Act1();

            if (Math.Abs(_skyVis[0] - -1) < Tolerance)
            {
                void Action()
                {
                    label_Allsky_ext.Text = @"AllSky extinction (mag): disconnected";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else if ((_skyVis[0] == 0) & (Math.Abs(_skyVis[1] - -1) < Tolerance))
            {
                void Action()
                {
                    label_Allsky_ext.Text = @"AllSky extinction (mag): old data";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else
            {
                void Action()
                {
                    label_Allsky_ext.Text = @"AllSky extinction (mag): " + _skyVis[0].ToString("00.0");
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }

            void Act2()
            {
                label_Allsky_ext_STD.Text = @"AllSky extinction STD (mag): " + _skyVis[1].ToString("00.00");
            }

            if (InvokeRequired)
                Invoke((Action) Act2);
            else
                Act2();
            if (Math.Abs(_seeing[0] - -1) < Tolerance)
            {
                void Action()
                {
                    label_Seeing_ext.Text = @"Seeing extinction (mag): disconnected";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else if ((_seeing[0] == 0) & (Math.Abs(_skyVis[1] - -1) < Tolerance))
            {
                void Action()
                {
                    label_Seeing_ext.Text = @"Seeing extinction (mag): old data";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else
            {
                void Action()
                {
                    label_Seeing_ext.Text = @"Seeing extinction (mag): " + _seeing[1].ToString("00.0");
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }

            void Act3()
            {
                label_Seeing.Text = @"Seeing (arcsec): " + _seeing[0].ToString("00.0");
            }

            if (InvokeRequired)
                Invoke((Action) Act3);
            else
                Act3();
            switch (_wind)
            {
                case -1:

                    void Action()
                    {
                        label_Wind.Text = @"Wind (m/s): disconnected";
                    }

                    if (InvokeRequired)
                        Invoke((Action) Action);
                    else
                        Action();
                    break;
                case 100:

                    void Action2()
                    {
                        label_Wind.Text = @"Wind (m/s): old data";
                    }

                    if (InvokeRequired)
                        Invoke((Action) Action2);
                    else
                        Action2();
                    break;
                default:
                    void Action3()
                    {
                        label_Wind.Text = @"Wind (m/s): " + _wind.ToString("00.0");
                    }

                    if (InvokeRequired)
                        Invoke((Action) Action3);
                    else
                        Action3();
                    break;
            }


            void Action4()
            {
                label_Sun.Text = @"Sun zenith distance (deg): " + _sunZd.ToString("00.0");
            }

            if (InvokeRequired)
                Invoke((Action) Action4);
            else
                Action4();
            UpdateWeatherLabel();


            if (checkBox_AutoDome.Checked) Autopilot();

            void Action5()
            {
                last_data_update_label.Text =
                    "Last data update time:\n" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (InvokeRequired)
                Invoke((Action) Action5);
            else
                Action5();
        }

        private void UpdateWeatherLabel()
        {
            switch (_isWeatherGood)
            {
                case -1:

                    void Action()
                    {
                        weather_label.Text = BadString;
                    }

                    if (InvokeRequired)
                        Invoke((Action) Action);
                    else
                        Action();
                    weather_label.ForeColor = Color.Red;
                    break;
                case 0:

                    void Action1()
                    {
                        weather_label.Text = @"Weather is good for flat frame";
                    }

                    if (InvokeRequired)
                        Invoke((Action) Action1);
                    else
                        Action1();
                    weather_label.ForeColor = Color.DarkOrange;
                    break;
                case 1:

                    void Action2()
                    {
                        weather_label.Text = GoodString;
                    }

                    if (InvokeRequired)
                        Invoke((Action) Action2);
                    else
                        Action2();
                    weather_label.ForeColor = Color.Green;
                    break;
            }

            label_Obs_cond.ForeColor =
                _isObsCanRun & _isShutterNorthOpen & _isShutterSouthOpen ? Color.Green : Color.Red;
            label_Obs_cond.Text = _isObsCanRun & _isShutterNorthOpen & _isShutterSouthOpen
                ? "Observation conditions: Run"
                : "Observation conditions: Stop";
        }

        private void Autopilot()
        {
            if (_isDomeCanOpen & _sunZd > 92)
            {
                open_dome();
                if (_isObsCanRun)
                {
                    weather_choice();
                }
            }
            else
            {
                stop_obs();
            }

            UpdateWeatherLabel();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //label_Time.Invoke(new Action(() => label_Time.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")));
            label_Time.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            SetMeteo();
            SetDome();
        }

        private void SetDome()
        {
            var power = _serialDevices.Power;
            _dome = _serialDevices.Dome;
            var buttons = _serialDevices.Buttons;
            var timeoutNorth = _serialDevices.TimeoutNorth;
            var timeoutSouth = _serialDevices.TimeoutSouth;
            var initflag = Convert.ToBoolean(_serialDevices.Initflag);

            if (power[5] & power[6])
            {
                void Action()
                {
                    label_Dome_Power.Text = @"Power: on";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Dome_Power.ForeColor = Color.Green;
                // msg = "Оба мотора запитаны";
            }
            else if (power[5])
            {
                void Action()
                {
                    label_Dome_Power.Text = @"Power: only north";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Dome_Power.ForeColor = Color.DarkOrange;
                // msg = "Запитан только северный мотор";
            }
            else if (power[6])
            {
                void Action()
                {
                    label_Dome_Power.Text = @"Power: only south";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Dome_Power.ForeColor = Color.DarkOrange;
                // msg = "Запитан только южный мотор";
            }
            else
            {
                void Action()
                {
                    label_Dome_Power.Text = @"Power: off";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Dome_Power.ForeColor = Color.Red;
                // msg = "Оба мотора без питания";
            }

            if (_dome[0])
            {
                void Action()
                {
                    label_Motor_North.Text = @"Motor north: closing";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Motor_North.ForeColor = Color.Green;
                // msg = "Северный мотор закрывает крышу";
            }
            else if (_dome[1])
            {
                void Action()
                {
                    label_Motor_North.Text = @"Motor north: opening";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Motor_North.ForeColor = Color.Green;
                // msg = "Северный мотор открывает крышу";
            }

            if (_dome[2])
            {
                void Action()
                {
                    label_Motor_South.Text = @"Motor south: closing";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Motor_South.ForeColor = Color.Green;
                // msg = "Южный мотор закрывает крышу";
            }
            else if (_dome[3])
            {
                void Action()
                {
                    label_Motor_South.Text = @"Motor south: opening";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Motor_South.ForeColor = Color.Green;
                // msg = "Южный мотор открывает крышу";
            }

            if (!(_dome[0] | _dome[1]))
            {
                void Action()
                {
                    label_Motor_North.Text = @"Motor north: run down";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Motor_North.ForeColor = Color.Red;
                Action();
            }

            if (!(_dome[2] | _dome[3]))
            {
                void Action()
                {
                    label_Motor_South.Text = @"Motor south: run down";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Motor_South.ForeColor = Color.Red;

                Action();
            }

            if (_dome[5])
            {
                label_Shutter_North.Invoke((MethodInvoker) delegate
                {
                    label_Shutter_North.Text = @"Shutter north: opened";
                });

                label_Shutter_North.ForeColor = Color.Green;
            }
            else if (_dome[4])
            {
                label_Shutter_North.Invoke((MethodInvoker) delegate
                {
                    label_Shutter_North.Text = @"Shutter north: closed";
                });
                label_Shutter_North.ForeColor = Color.Red;
            }
            else
            {
                label_Shutter_North.Invoke((MethodInvoker) delegate
                {
                    label_Shutter_North.Text = @"Shutter north: running";
                });
                label_Shutter_North.ForeColor = Color.DarkOrange;
            }

            _isShutterNorthOpen = _dome[5];

            if (_dome[7])
            {
                label_Shutter_South.Invoke((MethodInvoker) delegate
                {
                    label_Shutter_South.Text = @"Shutter south: opened";
                });
                label_Shutter_South.ForeColor = Color.Green;
            }
            else if (_dome[6])
            {
                label_Shutter_South.Invoke((MethodInvoker) delegate
                {
                    label_Shutter_South.Text = @"Shutter south: closed";
                });
                label_Shutter_South.ForeColor = Color.Red;
            }
            else
            {
                label_Shutter_South.Invoke((MethodInvoker) delegate
                {
                    label_Shutter_South.Text = @"Shutter south: running";
                });
                label_Shutter_South.ForeColor = Color.DarkOrange;
            }

            _isShutterSouthOpen = _dome[7];

            if (!buttons[4] & !buttons[5])
            {
                void Action()
                {
                    label_butt_north.Text = @"Button north: not pressed";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else if (buttons[4])
            {
                void Action()
                {
                    label_butt_north.Text = @"Button north: closing";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else
            {
                void Action()
                {
                    label_butt_north.Text = @"Button north: opening";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }

            if (!buttons[6] & !buttons[7])
            {
                void Action()
                {
                    label_butt_south.Text = @"Button south: not pressed";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else if (buttons[6])
            {
                void Action()
                {
                    label_butt_south.Text = @"Button south: closing";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else
            {
                void Action()
                {
                    label_butt_south.Text = @"Button south: opening";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }

            Action act = () => label_timeout_north.Text = @"Timeout north (s): " + timeoutNorth;
            if (InvokeRequired)
                Invoke(act);
            else
                act();
            act = () => label_timeout_south.Text = @"Timeout south (s): " + timeoutSouth;
            if (InvokeRequired)
                Invoke(act);
            else
                act();
            checkBox_initflag.Checked = initflag;
        }

        private void weather_choice()
        {
            if (_skyIr[0] > WeatherForCloseDome)
            {
                // cloudy
                _isWeatherGood = -1;
                stop_obs();
                return;
            }

            // if (_skyIr[0] > ClearAlert)
            // {
            //     // погода плохая, но не слишком
            //     sun_choice();
            //     return;
            // }
            //
            // // almost cloudless 
            // if (_skyIr[0] > Clear) sun_choice();

            //clear
            sun_choice();
        }

        private void sun_choice()
        {
            if (_sunZd < ZdFlat)
            {
                // sunny
                stop_obs();
                _isWeatherGood = -1;
                return;
            }

            if (_sunZd < ZdNight)
            {
                // dusk
                _isWeatherGood = 0;
                start_flat();
                return;
            }

            // night
            start_obs();
            _isWeatherGood = 1;
        }

        private void start_obs()
        {
            _isFlat = false;
            if (_isObsRunning) return;
            _logger.AddLogEntry("Observation start");
            // open_dome();
            _isObsRunning = true;
        }

        private void start_flat()
        {
            //TODO FLAT
            //obs flat
            _isFlat = true;
            // open_dome();
        }

        private void stop_obs()
        {
            _isFlat = false;
            close_dome();
            if (!_isObsRunning) return;
            _logger.AddLogEntry("Observation stop");
            _isObsRunning = false;
            // Park();
        }

        // private void Park()
        // {
        //     
        //     _isParking = true;
        //     _logger.AddLogEntry("Parking");
        // }

        private void open_dome()
        {
            if ((!_dome[1] & !_dome[5]) | (!_dome[3] & !_dome[7]))
            {
                open_south();
                open_north();
                update_dome();
            }
        }

        private void close_dome()
        {
            if ((!_dome[0] & !_dome[4]) | (!_dome[2] & !_dome[6]))
            {
                close_north();
                close_south();
                update_dome();
            }
        }

        //TODO opening/closing
        private void open_north()
        {
            // if (_isShutterNorthOpen) return;


            if (_serialDevices.Power[5])
            {
                label_Shutter_North.Text = @"Shutter north: running";
                _logger.AddLogEntry("Opening north");
                _serialDevices.AddTask("1rno");
                // _serialDevices.Write2Serial("1rno");
                // _isShutterNorthOpen = true;
                label_Shutter_North.Text = @"Shutter north: opened";
            }
            else
            {
                _logger.AddLogEntry("Power is down");
            }
        }

        private void close_north()
        {
            // if (!_isShutterNorthOpen) return;
            if (_serialDevices.Power[5])
            {
                label_Shutter_North.Text = @"Shutter north: running";
                _logger.AddLogEntry("Closing north");
                _serialDevices.AddTask("1rnc");
                // _serialDevices.Write2Serial("1rnc");
                // _isShutterNorthOpen = false;
                label_Shutter_North.Text = @"Shutter north: closed";
            }
            else
            {
                _logger.AddLogEntry("Power is down");
            }
        }

        private void open_south()
        {
            // if (_isShutterSouthOpen) return;
            if (_serialDevices.Power[6])
            {
                label_Shutter_South.Text = @"Shutter south: running";
                _serialDevices.AddTask("1rso");
                // _serialDevices.Write2Serial("1rso");
                _logger.AddLogEntry("Opening south");
                // _isShutterSouthOpen = true;
                // label_Shutter_South.Text = @"Shutter south: opened";
            }
            else
            {
                _logger.AddLogEntry("Power is down");
            }
        }

        private void close_south()
        {
            // if (!_isShutterSouthOpen) return;
            if (_serialDevices.Power[6])
            {
                label_Shutter_South.Text = @"Shutter south: running";
                _serialDevices.AddTask("1rsc");
                // _serialDevices.Write2Serial("1rsc");
                _logger.AddLogEntry("Closing south");
                // _isShutterSouthOpen = false;
                // label_Shutter_South.Text = @"Shutter south: closed";
            }
            else
            {
                _logger.AddLogEntry("Power is down");
            }
        }

        private bool Read_Cfg()
        {
            string line;
            string[] substrings;

            try
            {
                using (var reader = File.OpenText(Path + "\\Robophot.cfg"))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        substrings = line.Split(' ', '\t');
                        switch (substrings[0])
                        {
                            case "Dome_ComID":
                                _serialDevices.ComId = substrings[1];
                                break;
                            //case "Delay":
                            //    Delay = Convert.ToInt32(substrings[1]);
                            //    break;
                            //case "Server":
                            //    Server = substrings[1];
                            //    break;
                            //case "Dbase":
                            //    Database = substrings[1];
                            //    break;
                            //case "Table":
                            //    Table = substrings[1];
                            //    break;
                            //case "Login":
                            //    Login = substrings[1];
                            //    break;
                            //case "Pword":
                            //    Password = substrings[1];
                            //    break;
                            //case "Mirror":
                            //    Sensors[Convert.ToInt32(substrings[1])] = "Tmir";
                            //    break;
                            //case "Cell":
                            //    Sensors[Convert.ToInt32(substrings[1])] = "Tcell";
                            //    break;
                            //case "Air":
                            //    Sensors[Convert.ToInt32(substrings[1])] = "Tair";
                            //    break;
                        }
                    }
                }

                return true;
            }
            catch //(Exception ex)
            {
                return false;
            }
        }

        // private void park_button_Click(object sender, EventArgs e)
        // {
        //     Park();
        // }

        private void update_data_button_Click(object sender, EventArgs e)
        {
            var updateThreadMeteo = new Thread(update_meteo);
            updateThreadMeteo.Start();
            var updateThreadDome = new Thread(update_dome);
            updateThreadDome.Start();
        }

        private void update_meteo()
        {
            GetMeteo();
            SetMeteo();
        }

        private void update_dome()
        {
            // GetDome();
            _serialDevices.UpDate();
            SetDome();
        }

        private void button_Dome_Run_Click(object sender, EventArgs e)
        {
            // TODO DOME CLICK
            if (checkBoxNorth.Checked)
                switch (comboBox_Dome.Text)
                {
                    case "Open":
                    {
                        open_north();
                        break;
                    }
                    case "Close":
                    {
                        close_north();
                        break;
                    }
                    case "Stop":
                        // _serialDevices.Write2Serial("1rns");
                        _serialDevices.AddTask("1rns");
                        _logger.AddLogEntry("Stop north");
                        break;
                }

            if (!checkBoxSouth.Checked) return;
            switch (comboBox_Dome.Text)
            {
                case "Open":
                {
                    open_south();
                    break;
                }
                case "Close":
                {
                    close_south();
                    break;
                }
                case "Stop":
                    // _serialDevices.Write2Serial("1rss");
                    _serialDevices.AddTask("1rss");
                    _logger.AddLogEntry("Stop south");
                    break;
            }
        }

        private void checkBox_AutoDome_CheckedChanged(object sender, EventArgs e)
        {
            button_Dome_Run.Enabled = !checkBox_AutoDome.Checked;
        }

        private void socket_manager()
        {
            const int port = 8085; // порт для приема входящих запросов
            // получаем адреса для запуска сокета
            var ipPoint = new IPEndPoint(IPAddress.Loopback, port);
            // создаем сокет
            // var listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var listenSocket = new TcpListener(ipPoint)
            {
                ExclusiveAddressUse = true
            };

            while (true)
                try
                {
                    if (!_work) break;
                    listenSocket.Start();
                    _logger.AddLogEntry(@"Сервер запущен. Ожидание подключений...");
                    var tcpClient = listenSocket.AcceptTcpClient();
                    _logger.AddLogEntry($"Установленно соединение: {tcpClient.Client.RemoteEndPoint}");

                    var stream = tcpClient.GetStream();
                    // создаем StreamReader для чтения данных
                    var streamReader = new StreamReader(stream);
                    // создаем StreamWriter для отправки данных
                    var streamWriter = new StreamWriter(stream);
                    streamWriter.AutoFlush = true;
                    _br = false;

                    // получаем сообщение
                    while (tcpClient.Client.Connected)
                    {
                        if (!_work) break;
                        var get = "";
                        if (stream.CanRead) get = streamReader.ReadLine();

                        if (get == "stop" || _br)
                        {
                            _logger.AddLogEntry("Соединение разорвано");
                            listenSocket.Stop();
                            break;
                        }

                        switch (get)
                        {
                            case "sky":
                            {
                                // отправляем ответ
                                streamWriter.WriteLine(_skyIr[0].ToString("00.0"));
                                break;
                            }
                            case "sky std":
                            {
                                streamWriter.WriteLine(_skyIr[1].ToString("00.0"));
                                break;
                            }
                            case "extinction":
                            {
                                streamWriter.WriteLine(_skyVis[0].ToString("00.0"));
                                break;
                            }
                            case "extinction std":
                            {
                                streamWriter.WriteLine(_skyVis[1].ToString("00.0"));
                                break;
                            }
                            case "seeing":
                            {
                                streamWriter.WriteLine(_seeing[0].ToString("00.0"));
                                break;
                            }
                            case "seeing_extinction":
                            {
                                streamWriter.WriteLine(_seeing[1].ToString("00.0"));
                                break;
                            }
                            case "wind":
                            {
                                streamWriter.WriteLine(_wind.ToString("00.0"));
                                break;
                            }
                            case "sun":
                            {
                                streamWriter.WriteLine(_sunZd.ToString("00.0"));
                                break;
                            }
                            case "obs":
                            {
                                streamWriter.WriteLine(_isObsRunning.ToString());
                                break;
                            }
                            case "flat":
                            {
                                streamWriter.WriteLine(_isFlat.ToString());
                                break;
                            }
                        }
                    }
                }
                catch
                {
                    listenSocket.Stop();
                    break;
                }
        }

        private void numericUpDown_timeout_north_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char) Keys.Return) return;
            e.Handled = true;
            _logger.AddLogEntry("North timeout change to " + numericUpDown_timeout_north.Value);
            // _serialDevices.Write2Serial("1stn=" + numericUpDown_timeout_north.Value);
            _serialDevices.AddTask("1stn=" + numericUpDown_timeout_north.Value);
        }

        private void numericUpDown_timeout_south_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char) Keys.Return) return;
            e.Handled = true;
            _logger.AddLogEntry("South timeout change to " + numericUpDown_timeout_south.Value);
            // _serialDevices.Write2Serial("1sts=" + numericUpDown_timeout_south.Value);
            _serialDevices.AddTask("1sts=" + numericUpDown_timeout_south.Value);
        }

        private void button_disconnect_Click(object sender, EventArgs e)
        {
            _br = true;
        }
    }
}