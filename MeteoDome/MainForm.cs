﻿using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;
using ASCOM.Com.DriverAccess;
using ASCOM.Common;

namespace MeteoDome
{
    public partial class MainForm : Form
    {
        private const string BadString = "Weather is too bad to open dome";
        private const string GoodString = "Weather is good to open dome";
        private const double Tolerance = 1e-6;
        private static readonly string Path = Directory.GetCurrentDirectory();
        private readonly Logger _logger;
        private static readonly Timer MeteoTimer = new Timer(); //clock timer and status check timer

        ////globals for mount
        private readonly Telescope _mount;
        private const string TelescopeId = "SiTechDll Telescope"; //TODO park

        ////globals for dome
        private readonly DomeSerialDevice _domeSerialDevice = new DomeSerialDevice();
        private const short DomeTimeoutUpdateAlarm = 3; // in min //TODO .cfg

        ////globals for database
        private readonly MeteoDb _meteo;
        private short _checkWeatherForDome = -1;
        private short _counter;
        private BitArray _dome = new BitArray(8);
        private bool _isDomeCanOpen;
        private bool _isFirst = true;
        private bool _isObsCanRun;
        private bool _isShutterNorthOpen;
        private bool _isShutterSouthOpen;

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

            // if (!_domeSerialDevice.Init())
            //     if (MessageBox.Show(@"Can't open Dome serial port", @"OK", MessageBoxButtons.OK) == DialogResult.OK)
            //         Environment.Exit(1);

            if (!_domeSerialDevice.Init())
            {
                MessageBox.Show(@"Can't open Dome serial port", @"OK", MessageBoxButtons.OK);
            }
            else
            {
                groupBox_Dome.Text = $@"Dome (COMPORT {_domeSerialDevice.ComId})";
            }

            //create timer for main loop
            MeteoTimer.Elapsed += TimerGetClock;
            MeteoTimer.Interval = 1000;
            MeteoTimer.Start();
            _domeSerialDevice.UpDate();

            _mount = new Telescope(TelescopeId);
            _mount.Connected = true;

            var socks = new Socks(_logger);
            socks.StartListening();
            timerSet.Enabled = true;
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
            _domeSerialDevice.UpDate();
            if (_counter == 60 || _counter == 0)
            {
                GetMeteo();
                _counter = 0;

                if (_isFirst)
                {
                    Thread.Sleep(2000);
                    _isFirst = false;
                }

                CheckWeather();
                if (checkBox_AutoDome.Checked) Autopilot();
            }
                
            _counter++;
            if ((DateTime.UtcNow - _domeSerialDevice.domeUpdateDateTime).TotalMinutes > DomeTimeoutUpdateAlarm)
            {
                groupBox_Dome.Invoke((MethodInvoker) delegate
                {
                    groupBox_Dome.Text = $@"Dome (OLD DATA)";
                });

            }
            else
            {
                groupBox_Dome.Invoke((MethodInvoker) delegate
                {
                    groupBox_Dome.Text = $@"Dome (COMPORT {_domeSerialDevice.ComId})";
                });
            }
        }


        private void GetMeteo()
        {
            var skyIr = _meteo.Get_Sky_IR(); // return [sky_tmp, sky_tmp_std], [-1,-1] - disconnected, [0,-1] - old data
            var skyVis = _meteo
                .Get_Sky_VIS(); // return [extinction, extinction_std], [-1,-1] - disconnected, [0,-1] - old data
            var seeing = _meteo
                .Get_Seeing(); // return [seeing, seeing_extinction], [-1,-1] - disconnected, [0,-1] - old data
    
            WeatherDataCollector.SkyTemp = skyIr[0];
            WeatherDataCollector.SkyTempStd = skyIr[1];
            WeatherDataCollector.Extinction = skyVis[0];
            WeatherDataCollector.ExtinctionStd = skyVis[1];
            WeatherDataCollector.Seeing = seeing[0];
            WeatherDataCollector.SeeingExtinction = seeing[1];
            WeatherDataCollector.Wind = _meteo.Get_Wind(); // return wind, -1 - disconnected, 100 - old data
            WeatherDataCollector.SunZd = MeteoDb.Sun_ZD(); //return Sun zenith distance (degree)
        }

        private void SetMeteo()
        {
            if (Math.Abs(WeatherDataCollector.SkyTemp + 1) < Tolerance)
            {
                void Action()
                {
                    label_SkyTemp.Text = @"Sky temperature (deg): disconnected";
                    label_SkyTemp.ForeColor = Color.DarkBlue;
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else if ((WeatherDataCollector.SkyTemp == 0) & (Math.Abs(WeatherDataCollector.SkyTempStd + 1) < Tolerance))
            {
                void Action()
                {
                    label_SkyTemp.Text = @"Sky temperature (deg): old data";
                    label_SkyTemp.ForeColor = Color.DarkBlue;
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
                    label_SkyTemp.Text = @"Sky temperature (deg): " + WeatherDataCollector.SkyTemp.ToString("00.0");
                    if (WeatherDataCollector.SkyTemp < MeteoDb.MinSkyTemp)
                    {
                        label_SkyTemp.ForeColor = Color.Green;
                    }
                    else if (WeatherDataCollector.SkyTemp > MeteoDb.MinSkyTemp && WeatherDataCollector.SkyTemp < MeteoDb.MaxSkyTemp)
                    {
                        label_SkyTemp.ForeColor = Color.DarkOrange;
                    }
                    else
                    {
                        label_SkyTemp.ForeColor = Color.Red;
                    }
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }

            if (InvokeRequired)
                Invoke((Action) Act1);
            else
                Act1();

            if (Math.Abs(WeatherDataCollector.Extinction + 1) < Tolerance)
            {
                void Action()
                {
                    label_Allsky_ext.Text = @"AllSky extinction (mag): disconnected";
                    label_Allsky_ext.ForeColor = Color.DarkBlue;
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else if ((WeatherDataCollector.Extinction == 0) & (Math.Abs(WeatherDataCollector.ExtinctionStd + 1) < Tolerance))
            {
                void Action()
                {
                    label_Allsky_ext.Text = @"AllSky extinction (mag): old data";
                    label_Allsky_ext.ForeColor = Color.DarkBlue;
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
                    label_Allsky_ext.Text = @"AllSky extinction (mag): " + WeatherDataCollector.Extinction.ToString("00.0");
                    if (WeatherDataCollector.Extinction < MeteoDb.MinExtinction)
                    {
                        label_Allsky_ext.ForeColor = Color.Green;
                    }
                    else if (WeatherDataCollector.Extinction > MeteoDb.MinExtinction && WeatherDataCollector.Extinction < MeteoDb.MaxExtinction)
                    {
                        label_Allsky_ext.ForeColor = Color.DarkOrange;
                    }
                    else
                    {
                        label_Allsky_ext.ForeColor = Color.Red;
                    }
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }

            if (InvokeRequired)
                Invoke((Action) Act2);
            else
                Act2();
            if (Math.Abs(WeatherDataCollector.Seeing + 1) < Tolerance)
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
            else if ((WeatherDataCollector.Seeing == 0) & (Math.Abs(WeatherDataCollector.ExtinctionStd + 1) < Tolerance))
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
                    label_Seeing_ext.Text = @"Seeing extinction (mag): " + WeatherDataCollector.SeeingExtinction.ToString("00.0");
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }

            if (InvokeRequired)
                Invoke((Action) Act3);
            else
                Act3();
            switch (WeatherDataCollector.Wind)
            {
                case -1:

                    if (InvokeRequired)
                        Invoke((Action) Action);
                    else
                        Action();
                    break;

                    void Action()
                    {
                        label_Wind.Text = @"Wind (m/s): disconnected";
                        label_Wind.ForeColor = Color.DarkBlue;
                    }
                case 100:

                    if (InvokeRequired)
                        Invoke((Action) Action2);
                    else
                        Action2();
                    break;

                    void Action2()
                    {
                        label_Wind.Text = @"Wind (m/s): old data";
                        label_Wind.ForeColor = Color.DarkBlue;
                    }
                default:

                    if (InvokeRequired)
                        Invoke((Action) Action3);
                    else
                        Action3();
                    break;

                    void Action3()
                    {
                        label_Wind.Text = @"Wind (m/s): " + WeatherDataCollector.Wind.ToString("00.0");
                        if (WeatherDataCollector.Wind < MeteoDb.MinWind)
                            label_Wind.ForeColor = Color.Green;
                        else if (WeatherDataCollector.Wind > MeteoDb.MinWind && WeatherDataCollector.Wind < MeteoDb.MaxWind)
                            label_Wind.ForeColor = Color.DarkOrange;
                        else
                            label_Wind.ForeColor = Color.Red;
                    }
            }

            if (InvokeRequired)
                Invoke((Action) Action4);
            else
                Action4();

            if (_checkWeatherForDome < 1)
            {
                void Action()
                {
                    dome_weather_label.Text = BadString;
                    dome_weather_label.ForeColor = Color.Red;
                }

                if (InvokeRequired)
                    Invoke((Action) Action);
                else
                    Action();
            }
            else
            {
                void Action2()
                {
                    dome_weather_label.Text = GoodString;
                    dome_weather_label.ForeColor = Color.Green;
                }

                if (InvokeRequired)
                    Invoke((Action) Action2);
                else
                    Action2();
            }

            /////observe
            var temp = _isObsCanRun & _isShutterNorthOpen & _isShutterSouthOpen;

            if (_checkWeatherForDome == 2)
            {
                label_Obs_cond.ForeColor = Color.DarkOrange;
                label_Obs_cond.Text = @"Obs conditions: Can make flat frame";
            }
            else if (temp && _checkWeatherForDome == 3)
            {
                label_Obs_cond.ForeColor = Color.Green;
                label_Obs_cond.Text = @"Obs conditions: Can observe";
            }
            else
            {
                label_Obs_cond.ForeColor = Color.Red;
                label_Obs_cond.Text = @"Obs conditions: Can't observe";
            }

            return;

            void Act3()
            {
                label_Seeing.Text = @"Seeing (arcsec): " + WeatherDataCollector.Seeing.ToString("00.0");
            }

            void Action4()
            {
                label_Sun.Text = @"Sun zenith distance (deg): " + WeatherDataCollector.SunZd.ToString("00.0");
                if (WeatherDataCollector.SunZd > MeteoDb.SunZdNight)
                    label_Sun.ForeColor = Color.Green;
                else if (WeatherDataCollector.SunZd > MeteoDb.SunZdFlat)
                    label_Sun.ForeColor = Color.Lime;
                else if (WeatherDataCollector.SunZd > MeteoDb.SunZdDomeOpen) label_Sun.ForeColor = Color.DarkOrange;
                else label_Sun.ForeColor = Color.Red;
            }

            void Act2()
            {
                label_Allsky_ext_STD.Text = @"AllSky extinction STD (mag): " + WeatherDataCollector.ExtinctionStd.ToString("00.00");
                if (WeatherDataCollector.ExtinctionStd < MeteoDb.MinExtinctionStd)
                    label_Allsky_ext_STD.ForeColor = Color.Green;
                else if (WeatherDataCollector.ExtinctionStd > MeteoDb.MinExtinctionStd && WeatherDataCollector.ExtinctionStd < MeteoDb.MaxExtinctionStd)
                    label_Allsky_ext_STD.ForeColor = Color.DarkOrange;
                else
                    label_Allsky_ext_STD.ForeColor = Color.Red;
            }

            void Act1()
            {
                label_SkyTempSTD.Text = @"Sky temperature STD (deg): " + WeatherDataCollector.SkyTempStd.ToString("00.00");
                if (WeatherDataCollector.SkyTempStd < MeteoDb.MinSkyStd)
                    label_SkyTempSTD.ForeColor = Color.Green;
                else if (WeatherDataCollector.SkyTempStd > MeteoDb.MinSkyStd && WeatherDataCollector.SkyTempStd < MeteoDb.MaxSkyStd)
                    label_SkyTempSTD.ForeColor = Color.DarkOrange;
                else
                    label_SkyTempSTD.ForeColor = Color.Red;
            }
        }

        private void TimerSetTick(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = $@"UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
            SetDome();
            if (_counter == 1)
            {
                SetMeteo();   
            }
        }

        private void SetDome()
        {
            var power = _domeSerialDevice.Power;
            _dome = _domeSerialDevice.Dome;
            var buttons = _domeSerialDevice.Buttons;
            var timeoutNorth = _domeSerialDevice.TimeoutNorth;
            var timeoutSouth = _domeSerialDevice.TimeoutSouth;
            var initFlag = Convert.ToBoolean(_domeSerialDevice.InitFlag);

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
            checkBox_initflag.Checked = initFlag;
        }

        private void CheckWeather()
        {
            if (_counter == 0 || _counter == 60)
            {
                if ((WeatherDataCollector.Extinction > 0) & (WeatherDataCollector.ExtinctionStd >= 0))
                    _isObsCanRun = MeteoDb.Get_Weather_Obs(_isObsCanRun, WeatherDataCollector.Extinction, WeatherDataCollector.ExtinctionStd);
                if ((WeatherDataCollector.SkyTemp < -1) & (WeatherDataCollector.SkyTempStd >= 0) & (-1 < WeatherDataCollector.Wind) & (WeatherDataCollector.Wind < 100))
                    _isDomeCanOpen =
                        MeteoDb.Get_Weather_Dome(_isShutterNorthOpen & _isShutterSouthOpen,
                            WeatherDataCollector.SkyTemp, WeatherDataCollector.SkyTempStd, WeatherDataCollector.Wind);
            }

            if (_isDomeCanOpen & (WeatherDataCollector.SunZd > MeteoDb.SunZdDomeOpen))
            {
                _checkWeatherForDome = 1;
                if (_isObsCanRun)
                {
                    if (WeatherDataCollector.SunZd < MeteoDb.SunZdFlat)
                    {
                        // cloudy or too bright
                        _checkWeatherForDome = -1;
                        return;
                    }

                    //clear
                    if (WeatherDataCollector.SunZd < MeteoDb.SunZdNight)
                    {
                        _checkWeatherForDome = 2;
                        // dusk
                        return;
                    }

                    _checkWeatherForDome = 3;
                    return;
                    // // night
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
                    case 2 when !WeatherDataCollector.IsFlat:
                        //obs flat
                        _logger.AddLogEntry("Flat can start");
                        WeatherDataCollector.IsFlat = true;
                        return;
                    case 3:
                    {
                        // night
                        WeatherDataCollector.IsFlat = false;
                        if (WeatherDataCollector.IsObsRunning) return;
                        _logger.AddLogEntry("Observation can start");
                        WeatherDataCollector.IsObsRunning = true;
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
            WeatherDataCollector.IsFlat = false;
            close_dome();
            if (!WeatherDataCollector.IsObsRunning) return;
            _logger.AddLogEntry("Observation stop");
            WeatherDataCollector.IsObsRunning = false;
            // TODO
            if (_mount.Connected & _mount.CanPark)
            {
                _mount.ParkAsync();
            }
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
                DomeSerialDevice.AddTask("1rno");
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
                DomeSerialDevice.AddTask("1rnc");
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
                DomeSerialDevice.AddTask("1rso");
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
                DomeSerialDevice.AddTask("1rsc");
                _logger.AddLogEntry("Closing south");
            }
            else
            {
                _logger.AddLogEntry("South motor power off");
            }
        }

        private bool Read_Cfg()
        {
            try
            {
                using (var reader = File.OpenText(Path + "\\Robophot.cfg"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var substrings = line.Split(' ', '\t');
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

        private void Button_Dome_Run_Click(object sender, EventArgs e)
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
                        DomeSerialDevice.AddTask("1rns");
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
                    DomeSerialDevice.AddTask("1rss");
                    _logger.AddLogEntry("Stop south");
                    break;
            }
        }

        private void CheckBox_AutoDome_CheckedChanged(object sender, EventArgs e)
        {
            button_Dome_Run.Enabled = !checkBox_AutoDome.Checked;
        }

        private void NumericUpDown_timeout_north_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char) Keys.Return) return;
            e.Handled = true;
            _logger.AddLogEntry("North timeout change to " + numericUpDown_timeout_north.Value);
            DomeSerialDevice.AddTask("1stn=" + numericUpDown_timeout_north.Value);
        }

        private void NumericUpDown_timeout_south_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char) Keys.Return) return;
            e.Handled = true;
            _logger.AddLogEntry("South timeout change to " + numericUpDown_timeout_south.Value);
            DomeSerialDevice.AddTask("1sts=" + numericUpDown_timeout_south.Value);
        }

        private void toolStripMenuItemSaveLogs_Click(object sender, EventArgs e)
        {
            _logger.SaveLogs();
        }

        private void toolStripMenuItemClearLogs_Click(object sender, EventArgs e)
        {
            _logger.ClearLogs();
        }
    }
}