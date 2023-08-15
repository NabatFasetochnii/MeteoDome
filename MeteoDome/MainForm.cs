﻿using System;
using System.Collections;
using System.Drawing;
using System.IO;
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
        private const int WeatherForCloseDome = -16;
        private const string BadString = "Weather is too bad to open dome";
        private const string GoodString = "Weather is good to open dome";
        private const double Tolerance = 1e-6;
        private static readonly string Path = Directory.GetCurrentDirectory();

        private static readonly Timer MeteoTimer = new Timer(); //clock timer and status check timer

        ////globals for dome
        private readonly DomeSerialDevice _domeSerialDevice = new DomeSerialDevice();

        private readonly Logger _logger;
        // private static readonly Timer DomeTimer = new Timer(); //clock timer and status check timer

        ////globals for database
        private readonly MeteoDb _meteo;
        private short _checkWeatherForDome;
        private short _counter;

        // private int _counter;
        private BitArray _dome = new BitArray(8);
        private bool _isDomeCanOpen;
        private bool _isFirst = true;
        private bool _isFlat;
        private bool _isObsCanRun;
        private bool _isObsRunning;
        private bool _isShutterNorthOpen;
        private bool _isShutterSouthOpen;
        private int _isWeatherGood = -1;
        private double[] _seeing = {-1, -1};
        private double[] _skyIr = {-1, -1};
        private double[] _skyVis = {-1, -1};
        private double _sunZd = -1;
        private double _wind = -1;
        // private bool _work = true;

        private readonly Socks _socks;

        public MainForm()
        {
            InitializeComponent();

            button_Dome_Run.Enabled = !checkBox_AutoDome.Checked;
            
            
            

            _logger = new Logger(logBox);
            _domeSerialDevice.Logger = _logger;
            _meteo = new MeteoDb(_logger);

            if (!Read_Cfg())
                if (MessageBox.Show(@"Can't read config", @"OK", MessageBoxButtons.OK) == DialogResult.OK)
                    Environment.Exit(1);

            if (!_domeSerialDevice.Init())
                if (MessageBox.Show(@"Can't open Dome serial port", @"OK", MessageBoxButtons.OK) == DialogResult.OK)
                    Environment.Exit(1);
            //create timer for main loop
            MeteoTimer.Elapsed += TimerGetClock;
            MeteoTimer.Interval = 1000;
            MeteoTimer.Start();

            GetMeteo();
            _domeSerialDevice.UpDate();

            _socks = new Socks(_logger, ref _seeing, ref _skyIr, ref _skyVis, 
                ref _sunZd, ref _wind, ref _isFlat, ref _isObsRunning);
            _socks.StartListening();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // _work = false;
            _domeSerialDevice.Dispose();
            // _socks.StopListening();
            MeteoTimer.Close();
            timerSet.Stop();
        }

        //one second timer for status and clock
        private void TimerGetClock(object sender, ElapsedEventArgs e)
        {
            // var getMeteoThread = new Thread(GetMeteo);
            // getMeteoThread.Start();
            if (_counter == 60)
            {
                GetMeteo();
                _counter = 0;
            }
            _counter++;
            _domeSerialDevice.UpDate();
            if (_isFirst)
            {
                Thread.Sleep(2000);
                _isFirst = false;
            }

            CheckWeather();
            if (checkBox_AutoDome.Checked) Autopilot();
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
            if ((_skyVis[0] > 0) & (_skyVis[1] >= 0))
                _isObsCanRun = MeteoDb.Get_Weather_Obs(_isObsCanRun, _skyVis[0], _skyVis[1]);
            if ((_skyIr[0] < -1) & (_skyIr[1] >= 0) & (-1 < _wind) & (_wind < 100))
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

            if (_isWeatherGood == -1)
            {
                void Action()
                {
                    weather_label.Text = BadString;
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                weather_label.ForeColor = Color.Red;
            }
            else
            {
                void Action2()
                {
                    weather_label.Text = GoodString;
                }

                if (InvokeRequired)
                    Invoke((Action) Action2);
                else
                    Action2();
                weather_label.ForeColor = Color.Green;
            }

            var temp = _isObsCanRun & _isShutterNorthOpen & _isShutterSouthOpen;

            if (_isWeatherGood == 0)
            {
                label_Obs_cond.ForeColor = Color.DarkOrange;
                label_Obs_cond.Text = @"Obs conditions: Can make flat frame";
            }
            else if (temp)
            {
                label_Obs_cond.ForeColor = Color.Green;
                label_Obs_cond.Text = @"Obs conditions: Can observe";
            }
            else
            {
                label_Obs_cond.ForeColor = Color.Red;
                label_Obs_cond.Text = @"Obs conditions: Can't observe";
            }
        }

        private void TimerSetTick(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = $@"UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
            SetMeteo();
            SetDome();
        }

        private void SetDome()
        {
            var power = _domeSerialDevice.Power;
            _dome = _domeSerialDevice.Dome;
            var buttons = _domeSerialDevice.Buttons;
            var timeoutNorth = _domeSerialDevice.TimeoutNorth;
            var timeoutSouth = _domeSerialDevice.TimeoutSouth;
            var initflag = Convert.ToBoolean(_domeSerialDevice.Initflag);

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

            var nMotorDown = !(_dome[0] | _dome[1]);
            if (nMotorDown)
            {
                void Action()
                {
                    label_Motor_North.Text = @"Motor north: stopped";
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
                label_Motor_North.ForeColor = Color.Red;
                Action();
            }

            var sMotorDown = !(_dome[2] | _dome[3]);
            if (sMotorDown)
            {
                void Action()
                {
                    label_Motor_South.Text = @"Motor south: stopped";
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
            else if (nMotorDown)
            {
                label_Shutter_North.Invoke((MethodInvoker) delegate
                {
                    label_Shutter_North.Text = @"Shutter north: half open";
                });
                label_Shutter_North.ForeColor = Color.DarkOrange;
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
            else if (sMotorDown)
            {
                label_Shutter_South.Invoke((MethodInvoker) delegate
                {
                    label_Shutter_South.Text = @"Shutter south: half open";
                });
                label_Shutter_South.ForeColor = Color.DarkOrange;
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

        private void CheckWeather()
        {
            if (_isDomeCanOpen & (_sunZd > 94))
            {
                _checkWeatherForDome = 1;
                if (_isObsCanRun)
                {
                    if ((_skyIr[0] > WeatherForCloseDome) | (_sunZd < ZdFlat))
                    {
                        // cloudy or too bright
                        _isWeatherGood = -1;
                        _checkWeatherForDome = -1;
                        // stop_obs();
                        return;
                    }

                    //clear
                    if (_sunZd < ZdNight)
                    {
                        _checkWeatherForDome = 2;
                        _isWeatherGood = 0;
                        // dusk
                        // if (!_isFlat)
                        // {
                        //     //obs flat
                        //     _logger.AddLogEntry("Flat can start");
                        //     _isWeatherGood = 0;
                        //     _isFlat = true;
                        //     return;
                        // }
                        return;
                    }

                    _checkWeatherForDome = 3;
                    // // night
                    // _isFlat = false;
                    // if (_isObsRunning) return;
                    // _logger.AddLogEntry("Observation can start");
                    // _isObsRunning = true;
                    _isWeatherGood = 1;
                }
            }

            // stop_obs();
            _checkWeatherForDome = 0;
        }

        private void Autopilot()
        {
            if (_checkWeatherForDome > 0)
            {
                open_dome();
                switch (_checkWeatherForDome)
                {
                    case 2 when !_isFlat:
                        //TODO FLAT
                        //obs flat
                        _logger.AddLogEntry("Flat can start");
                        _isFlat = true;
                        return;
                    case 3:
                    {
                        // night
                        _isFlat = false;
                        if (_isObsRunning) return;
                        _logger.AddLogEntry("Observation can start");
                        _isObsRunning = true;
                        return;
                    }
                }
            }
            else
            {
                stop_obs();
            }
        }

        private void stop_obs()
        {
            _isFlat = false;
            close_dome();
            if (!_isObsRunning) return;
            _logger.AddLogEntry("Observation stop");
            _isObsRunning = false;
            // Park(); TODO
        }

        private void open_dome()
        {
            if ((!_dome[1] & !_dome[5]) | (!_dome[3] & !_dome[7]))
            {
                open_south();
                open_north();
            }
        }

        private void close_dome()
        {
            if ((!_dome[0] & !_dome[4]) | (!_dome[2] & !_dome[6]))
            {
                close_north();
                close_south();
            }
        }

        private void open_north()
        {
            if (_domeSerialDevice.Power[5])
            {
                _logger.AddLogEntry("Opening north");
                _domeSerialDevice.AddTask("1rno");
            }
            else
            {
                _logger.AddLogEntry("North motor power off");
            }
        }

        private void close_north()
        {
            if (_domeSerialDevice.Power[5])
            {
                _logger.AddLogEntry("Closing north");
                _domeSerialDevice.AddTask("1rnc");
            }
            else
            {
                _logger.AddLogEntry("North motor power off");
            }
        }

        private void open_south()
        {
            if (_domeSerialDevice.Power[6])
            {
                _domeSerialDevice.AddTask("1rso");
                _logger.AddLogEntry("Opening south");
            }
            else
            {
                _logger.AddLogEntry("South motor power off");
            }
        }

        private void close_south()
        {
            if (_domeSerialDevice.Power[6])
            {
                _domeSerialDevice.AddTask("1rsc");
                _logger.AddLogEntry("Closing south");
            }
            else
            {
                _logger.AddLogEntry("South motor power off");
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
                                _domeSerialDevice.ComId = substrings[1];
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

        private void button_Dome_Run_Click(object sender, EventArgs e)
        {
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
                        _domeSerialDevice.AddTask("1rns");
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
                    _domeSerialDevice.AddTask("1rss");
                    _logger.AddLogEntry("Stop south");
                    break;
            }
        }

        private void checkBox_AutoDome_CheckedChanged(object sender, EventArgs e)
        {
            button_Dome_Run.Enabled = !checkBox_AutoDome.Checked;
        }

        private void numericUpDown_timeout_north_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char) Keys.Return) return;
            e.Handled = true;
            _logger.AddLogEntry("North timeout change to " + numericUpDown_timeout_north.Value);
            _domeSerialDevice.AddTask("1stn=" + numericUpDown_timeout_north.Value);
        }

        private void numericUpDown_timeout_south_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char) Keys.Return) return;
            e.Handled = true;
            _logger.AddLogEntry("South timeout change to " + numericUpDown_timeout_south.Value);
            _domeSerialDevice.AddTask("1sts=" + numericUpDown_timeout_south.Value);
        }

        private void saveLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _logger.SaveLogs();
        }
    }
}