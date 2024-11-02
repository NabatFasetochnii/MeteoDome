using System;
using System.Globalization;
using Npgsql;

namespace MeteoDome
{
    public static class MeteoDb
    {
        public static float Latitude;
        public static float Longitude;
        public static short MaxWind;
        public static short MinWind;
        public static short MinSkyTemp;
        public static short MaxSkyTemp;
        public static short MaxSkyStd;
        public static short MinSkyStd;
        public static float MinExtinction;
        public static float MinExtinctionStd;
        public static float MaxExtinction;
        public static float MaxExtinctionStd;
        public static short SunZdDomeOpen;
        public static short SunZdFlat;
        public static short SunZdNight;

        public static string MeteoServer;
        public static string Port;
        public static string UserId;
        public static string Password;
        public static string Database;
        
        ////globals
        private static string _servicesConnString;

        public static void SetServices()
        {
            _servicesConnString =
                $"Server={MeteoServer};Port={Port};User Id={UserId};Password={Password}; Database={Database};";
        }

        private static double Ut2Jd(DateTime ut)
        {
            var zeroJd = DateTime.ParseExact("1858-11-17 00:00:00.0", "yyyy-MM-dd HH:mm:ss.f",
                CultureInfo.InvariantCulture);
            var jd = ut - zeroJd;
            jd = jd.Add(TimeSpan.FromDays(2400000.5));
            return jd.TotalDays;
        }

        private static double Jd2Lst(double jd, double longitude)
        {
            const double jd2000 = 2451545.0;
            //double JD = UT2JD(UT);
            var t0 = jd - jd2000;
            var T = t0 / 36525;
            //Compute GST in seconds.
            var theta = 280.46061837 + 360.98564736629 * t0 + T * T * 0.000387933 - T * T * T / 38710000.0;
            //Compute LST in hours
            var lstH = (theta + longitude) / 15.0;
            lstH -= 24 * Math.Floor(lstH / 24);
            //TimeSpan LST = TimeSpan.FromHours(LST_H);
            return lstH;
        }

        private static double[] Get_Sun(double jd, double lsTh, double latitude)
        {
            var sun = new double[2];
            var n = jd - 2451545.0; //get number of days after 1J2000
            var l = 280.460 + 0.9856474 * n; //get mean longitude of the Sun (degrees)
            var g = 357.528 + 0.9856003 * n; //get mean anomaly of the Sun

            //put L and g in the range from 0 to 360
            if (l > 360) l -= Math.Floor(l / 360) * 360.0;
            if (l < 0) l = l - Math.Floor(l / 360) * 360.0 + 360;
            if (g > 360) g -= Math.Floor(g / 360) * 360.0;
            if (g < 0) g = g - Math.Floor(g / 360) * 360.0 + 360;

            var lambda = l + 1.915 * Math.Sin(g / (180.0 / Math.PI)) + 0.02 * Math.Sin(2 * g / (180.0 / Math.PI));

            //calculation of equatorial coordinates
            var e = 23.439 - n * 0.0000004; //obliquity of the ecliptic (degrees)
            var ra = Math.Atan2(Math.Cos(e / (180.0 / Math.PI)) * Math.Sin(lambda / (180.0 / Math.PI)),
                Math.Cos(lambda / (180.0 / Math.PI)));
            var dec = Math.Asin(Math.Sin(e / (180.0 / Math.PI)) * Math.Sin(lambda / (180.0 / Math.PI)));


            //calculation of hour angle of sun
            var ha = lsTh - ra * (180.0 / Math.PI) / 15.0; //(hours)
            //а зачем нам передавать сюда LSTh, если мы можем функцию вызвать?
            //Или она уже была посчитана и проще её же и передать?
            if (ha < 0) ha += 24.0;

            //calculation of sun Zenith distance (ZD)
            var cosZd = Math.Sin(latitude / (180.0 / Math.PI)) * Math.Sin(dec) +
                        Math.Cos(latitude / (180.0 / Math.PI)) * Math.Cos(dec) *
                        Math.Cos(ha * 15.0 / (180.0 / Math.PI));
            var zd = Math.Acos(cosZd);

            //calculation of sun Azimuth (AZ)
            var cosAz = (Math.Sin(dec) - Math.Cos(zd) * Math.Sin(latitude / (180.0 / Math.PI))) /
                        (Math.Sin(zd) * Math.Cos(latitude / (180.0 / Math.PI)));
            var az = Math.Acos(cosAz);
            var aZd = az * (180.0 / Math.PI); //radians to degrees
            if (ha < 12) aZd = 360 - aZd;

            sun[0] = aZd;
            sun[1] = zd * (180.0 / Math.PI);
            return sun;
        }

        //return Sun zenith distance (degree)
        public static double Sun_ZD()
        {
            var jd = Ut2Jd(DateTime.UtcNow);
            var lsTh = Jd2Lst(jd, Longitude);
            var sun = Get_Sun(jd, lsTh, Latitude);

            return sun[1];
        }

        //return [-1,-1] not connected, [0,-1] old data, [extinction, extinction_std] good data
        public static double[] Get_Sky_VIS()
        {
            double dT = 0;
            var result = new double[] {-1, -1};

            using (var conn = new NpgsqlConnection(_servicesConnString))
            {
                try
                {
                    conn.Open();
                    result[0] = 0; //connected
                    var qwery = "SELECT EXTRACT(EPOCH FROM (now() - MAX(timestamp))) FROM cloud_cam;"; 
                    var command = new NpgsqlCommand(qwery, conn);
                    var dr = command.ExecuteReader();
                    while (dr.Read()) dT = dr.GetDouble(0);
                    dr.Close();

                    if (dT < 180)
                    {
                        result[1] = 0; //time ok
                        qwery =
                            "SELECT AVG(mean_ext), STDDEV_SAMP(mean_ext) FROM cloud_cam WHERE timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            result[0] = dr.GetDouble(0);
                            result[1] = dr.GetDouble(1);
                        }

                        dr.Close();

                        qwery = "SELECT AVG(mean_ext), STDDEV_SAMP(mean_ext) FROM cloud_cam WHERE (mean_ext) >= " +
                                (result[0] - 2 * result[1]) + " AND (mean_ext) <= " + (result[0] + 2 * result[1]) +
                                " AND timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            result[0] = Convert.ToDouble(dr[0]);
                            result[1] = Convert.ToDouble(dr[1]);
                        }

                        dr.Close();
                    }
                    conn.Close();
                }
                catch (Exception e)
                {
                    Logger.AddLogEntry(e.Message);
                    Logger.AddLogEntry("MeteoDB: ERROR SKY VIS");
                }
            }

            return result;
        }

        //return [-1,-1] not connected, [0,-1] old data, [sky_tmp, sky_tmp_std] good data
        public static double[] Get_Sky_IR()
        {
            double dT = 0;
            var result = new double[] {-1, -1};

            using (var conn = new NpgsqlConnection(_servicesConnString))
            {
                try
                {
                    conn.Open();
                    result[0] = 0; //connected
                    var qwery = "SELECT EXTRACT(EPOCH FROM (now() - MAX(timestamp))) FROM allsky_mlx;";
                    var command = new NpgsqlCommand(qwery, conn);
                    var dr = command.ExecuteReader();
                    while (dr.Read()) dT = Convert.ToDouble(dr[0]);
                    dr.Close();
                    // Logger.AddLogEntry($"allsky_mlx dT = {dT}");
                    if (dT < 120)
                    {
                        result[1] = 0; //time ok
                        qwery =
                            "SELECT AVG(temp2-temp1), STDDEV_SAMP(temp2-temp1) FROM allsky_mlx " +
                            "WHERE timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            result[0] = Convert.ToDouble(dr[0]);
                            result[1] = Convert.ToDouble(dr[1]);
                        }

                        dr.Close();

                        // WARNING: Sensitive to decimal separator --- must be '.'
                        qwery =
                            "SELECT AVG(temp2-temp1), STDDEV_SAMP(temp2-temp1) FROM allsky_mlx WHERE (temp2 - temp1) >= " +
                            (result[0] - 2 * result[1]) + " AND (temp2-temp1) <= " + (result[0] + 2 * result[1]) +
                            " AND timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            result[0] = Convert.ToDouble(dr[0]);
                            result[1] = Convert.ToDouble(dr[1]);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
                catch (Exception e)
                {
                    Logger.AddLogEntry(e.Message);
                    Logger.AddLogEntry("MeteoDB: ERROR SKY IR");
                }
            }

            return result;
        }

        //return: -1 - not connected, 100 - old data, wind
        public static double Get_Wind()
        {
            double dT = 0;
            double result = -1;
            double a = 0, b = 0;

            using (var conn = new NpgsqlConnection(_servicesConnString))
            {
                try
                {
                    conn.Open();
                    result = 0; //connected
                    var qwery = "SELECT EXTRACT(EPOCH FROM (now() - MAX(timestamp))) FROM boltwood;";
                    var command = new NpgsqlCommand(qwery, conn);
                    var dr = command.ExecuteReader();
                    while (dr.Read()) dT = Convert.ToDouble(dr[0]);
                    dr.Close();
                    // Logger.AddLogEntry($"boltwood dT = {dT}");
                    if (dT < 120)
                    {
                        result = 100; //time ok
                        qwery =
                            "SELECT AVG(wind), STDDEV_SAMP(wind) FROM boltwood WHERE timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            a = Convert.ToDouble(dr[0]);
                            b = Convert.ToDouble(dr[1]);
                        }

                        dr.Close();

                        if (a < 0)
                        {
                            Logger.AddLogEntry("DB WARNING: WIND < 0");
                        }
                        else
                        {
                            // WARNING: Sensitive to decimal separator --- must be '.'
                            qwery = "SELECT AVG(wind), STDDEV_SAMP(wind) FROM boltwood WHERE (wind) >= " +
                                    (a - 2 * b) + " AND (wind) <= " + (a + 2 * b) +
                                    " AND timestamp > now() - '10 minutes'::interval;";
                            command = new NpgsqlCommand(qwery, conn);
                            dr = command.ExecuteReader();
                            while (dr.Read()) result = Convert.ToDouble(dr[0]);
                            dr.Close();
                        }
                    }

                    conn.Close();
                }
                catch (Exception e)
                {
                    Logger.AddLogEntry(e.Message);
                    Logger.AddLogEntry("MeteoDB: ERROR WIND");
                }
            }

            return result;
        }

        //return [-1,-1] not connected, [0,-1] old data, [seeing, seeing_extinction] good data
        public static double[] Get_Seeing()
        {
            double dT = 0;
            double[] result = {-1, -1};

            using (var conn = new NpgsqlConnection(_servicesConnString))
            {
                try
                {
                    conn.Open();
                    result[0] = 0; //connected
                    var qwery = "SELECT EXTRACT(EPOCH FROM (now() - MAX(timestamp))) FROM seeing;";
                    var command = new NpgsqlCommand(qwery, conn);
                    var dr = command.ExecuteReader();
                    while (dr.Read()) dT = Convert.ToDouble(dr[0]);
                    dr.Close();
                    // Logger.AddLogEntry($"seeing dT = {dT}");
                    if (dT < 120)
                    {
                        result[1] = 0; //time ok
                        qwery =
                            "SELECT AVG(seeing_z), AVG(flux) FROM seeing WHERE timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            result[0] = Math.Round(Convert.ToDouble(dr[0]), 2);
                            result[1] = Math.Round(-2.5 * Math.Log10(Convert.ToDouble(dr[1]) / 5000.0), 2);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
                catch (Exception e)
                {
                    Logger.AddLogEntry(e.Message);
                    Logger.AddLogEntry("MeteoDB: ERROR SEEING");
                }
            }

            return result;
        }

        public static bool Get_Weather_Dome(bool dome, double skyTemp, double skyStd, double wind)
        {
            switch (dome)
            {
                //if dome closed
                case false when wind > MinWind:
                case false when skyTemp > MinSkyTemp:
                case false when skyStd > MinSkyStd:
                    return false;
                case false: return true;
                //if dome opened
                case true when wind > MaxWind:
                case true when skyTemp > MaxSkyTemp:
                case true when skyStd > MaxSkyStd:
                    return false;
                case true: return true;
            }
        }

        public static bool Get_Weather_Obs(bool obs, double ext, double extStd)
        {
            switch (obs)
            {
                //standby
                case false when ext > MinExtinction:
                case false when extStd > MinExtinctionStd:
                    return false;
                case false:
                    return true;
                //observation in run
                case true when ext > MaxExtinction:
                case true when extStd > MaxExtinctionStd:
                    return false;
                case true:
                    return true;
            }
        }
    }
}