using System;
using System.Globalization;
using Npgsql;

namespace MeteoDome
{
    public class Meteo_DB
    {
        private static readonly double Latitude = 57.036537;
        private static readonly double Longitude = 59.545735;

        ////globals
        public string services_conn_string =
            "Server=192.168.240.26;Port=5432;User Id=services;Password=services; Database=services;";

        private static double UT2JD(DateTime UT)
        {
            var zero_jd = DateTime.ParseExact("1858-11-17 00:00:00.0", "yyyy-MM-dd HH:mm:ss.f",
                CultureInfo.InvariantCulture);
            var JD = UT - zero_jd;
            JD = JD.Add(TimeSpan.FromDays(2400000.5));
            return JD.TotalDays;
        }

        private static double JD2LST(double JD, double Longitude)
        {
            var JD2000 = 2451545.0;
            //double JD = UT2JD(UT);
            var T0 = JD - JD2000;
            var T = T0 / 36525;
            //Compute GST in seconds.
            var theta = 280.46061837 + 360.98564736629 * T0 + T * T * 0.000387933 - T * T * T / 38710000.0;
            //Compute LST in hours
            var LST_H = (theta + Longitude) / 15.0;
            LST_H = LST_H - 24 * Math.Floor(LST_H / 24);
            //TimeSpan LST = TimeSpan.FromHours(LST_H);
            return LST_H;
        }

        private static double[] Get_Sun(double JD, double LSTh, double Latitude)
        {
            var Sun = new double[2];
            var N = JD - 2451545.0; //get number of days after 1J2000
            var L = 280.460 + 0.9856474 * N; //get mean longitude of the Sun (degrees)
            var g = 357.528 + 0.9856003 * N; //get mean anomaly of the Sun

            //put L and g in the range from 0 to 360
            if (L > 360) L = L - Math.Floor(L / 360) * 360.0;
            if (L < 0) L = L - Math.Floor(L / 360) * 360.0 + 360;
            if (g > 360) g = g - Math.Floor(g / 360) * 360.0;
            if (g < 0) g = g - Math.Floor(g / 360) * 360.0 + 360;

            var Lambda = L + 1.915 * Math.Sin(g / (180.0 / Math.PI)) + 0.02 * Math.Sin(2 * g / (180.0 / Math.PI));

            //calculation of equatorial coordinates
            var e = 23.439 - N * 0.0000004; //obliquity of the ecliptic (degrees)
            var Ra = Math.Atan2(Math.Cos(e / (180.0 / Math.PI)) * Math.Sin(Lambda / (180.0 / Math.PI)),
                Math.Cos(Lambda / (180.0 / Math.PI)));
            var Dec = Math.Asin(Math.Sin(e / (180.0 / Math.PI)) * Math.Sin(Lambda / (180.0 / Math.PI)));


            //calculation of hour angle of sun
            var HA = LSTh - Ra * (180.0 / Math.PI) / 15.0; //(hours)
            //а зачем нам передавать сюда LSTh, если мы можем функцию вызвать?
            //Или она уже была посчитана и проще её же и передать?
            if (HA < 0) HA = HA + 24.0;

            //calculation of sun Zenith distance (ZD)
            var cosZD = Math.Sin(Latitude / (180.0 / Math.PI)) * Math.Sin(Dec) +
                        Math.Cos(Latitude / (180.0 / Math.PI)) * Math.Cos(Dec) *
                        Math.Cos(HA * 15.0 / (180.0 / Math.PI));
            var ZD = Math.Acos(cosZD);

            //calculation of sun Azimuth (AZ)
            var cosAZ = (Math.Sin(Dec) - Math.Cos(ZD) * Math.Sin(Latitude / (180.0 / Math.PI))) /
                        (Math.Sin(ZD) * Math.Cos(Latitude / (180.0 / Math.PI)));
            var AZ = Math.Acos(cosAZ);
            var AZd = AZ * (180.0 / Math.PI); //radians to degrees
            if (HA < 12) AZd = 360 - AZd;

            Sun[0] = AZd;
            Sun[1] = ZD * (180.0 / Math.PI);
            return Sun;
        }

        //return Sun zenith distance (degree)
        public double Sun_ZD()
        {
            var JD = UT2JD(DateTime.Now.ToUniversalTime());
            var LSTh = JD2LST(JD, Longitude);
            var Sun = Get_Sun(JD, LSTh, Latitude);

            return Sun[1];
        }

        //return [-1,-1] not connected, [0,-1] old data, [extinction, extinction_std] good data
        public double[] Get_Sky_VIS()
        {
            double dT = 0;
            var Result = new double[] {-1, -1};

            using (var conn = new NpgsqlConnection(services_conn_string))
            {
                try
                {
                    conn.Open();
                    Result[0] = 0; //connected
                    var qwery = "SELECT EXTRACT(EPOCH FROM (now() - MAX(timestamp))) FROM cloud_cam;";
                    var command = new NpgsqlCommand(qwery, conn);
                    var dr = command.ExecuteReader();
                    while (dr.Read()) dT = Convert.ToDouble(dr[0]);
                    dr.Close();

                    if (dT < 180)
                    {
                        Result[1] = 0; //time ok
                        qwery =
                            "SELECT AVG(mean_ext), STDDEV_SAMP(mean_ext) FROM cloud_cam WHERE timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            Result[0] = Convert.ToDouble(dr[0]);
                            Result[1] = Convert.ToDouble(dr[1]);
                        }

                        dr.Close();

                        qwery = "SELECT AVG(mean_ext), STDDEV_SAMP(mean_ext) FROM cloud_cam WHERE (mean_ext) >= " +
                                (Result[0] - 2 * Result[1]) + " AND (mean_ext) <= " + (Result[0] + 2 * Result[1]) +
                                " AND timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            Result[0] = Convert.ToDouble(dr[0]);
                            Result[1] = Convert.ToDouble(dr[1]);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
                catch
                {
                    // ignored
                }
            }

            return Result;
        }

        //return [-1,-1] not connected, [0,-1] old data, [sky_tmp, sky_tmp_std] good data
        public double[] Get_Sky_IR()
        {
            double dT = 0;
            var Result = new double[2] {-1, -1};

            using (var conn = new NpgsqlConnection(services_conn_string))
            {
                try
                {
                    conn.Open();
                    Result[0] = 0; //connected
                    var qwery = "SELECT EXTRACT(EPOCH FROM (now() - MAX(timestamp))) FROM allsky_mlx;";
                    var command = new NpgsqlCommand(qwery, conn);
                    var dr = command.ExecuteReader();
                    while (dr.Read()) dT = Convert.ToDouble(dr[0]);
                    dr.Close();

                    if (dT < 120)
                    {
                        Result[1] = 0; //time ok
                        qwery =
                            "SELECT AVG(temp2-temp1), STDDEV_SAMP(temp2-temp1) FROM allsky_mlx " +
                            "WHERE timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            Result[0] = Convert.ToDouble(dr[0]);
                            Result[1] = Convert.ToDouble(dr[1]);
                        }

                        dr.Close();

                        qwery =
                            "SELECT AVG(temp2-temp1), STDDEV_SAMP(temp2-temp1) FROM allsky_mlx WHERE (temp2 - temp1) >= " +
                            (Result[0] - 2 * Result[1]) + " AND (temp2-temp1) <= " + (Result[0] + 2 * Result[1]) +
                            " AND timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            Result[0] = Convert.ToDouble(dr[0]);
                            Result[1] = Convert.ToDouble(dr[1]);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
                catch
                {
                    // ignored
                }
            }

            return Result;
        }

        //return: -1 - not connected, 100 - old data, wind
        public double Get_Wind()
        {
            double dT = 0;
            double Result = -1;
            double A = 0, B = 0;

            using (var conn = new NpgsqlConnection(services_conn_string))
            {
                try
                {
                    conn.Open();
                    Result = 0; //connected
                    var qwery = "SELECT EXTRACT(EPOCH FROM (now() - MAX(timestamp))) FROM boltwood;";
                    var command = new NpgsqlCommand(qwery, conn);
                    var dr = command.ExecuteReader();
                    while (dr.Read()) dT = Convert.ToDouble(dr[0]);
                    dr.Close();

                    if (dT < 120)
                    {
                        Result = 100; //time ok
                        qwery =
                            "SELECT AVG(wind), STDDEV_SAMP(wind) FROM boltwood WHERE timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read())
                        {
                            A = Convert.ToDouble(dr[0]);
                            B = Convert.ToDouble(dr[1]);
                        }

                        dr.Close();

                        qwery = "SELECT AVG(wind), STDDEV_SAMP(wind) FROM boltwood WHERE wind >= " +
                                (A - 2 * B) + " AND wind <= " + (A + 2 * B) +
                                " AND timestamp > now() - '10 minutes'::interval;";
                        command = new NpgsqlCommand(qwery, conn);
                        dr = command.ExecuteReader();
                        while (dr.Read()) Result = Convert.ToDouble(dr[0]);
                        dr.Close();
                    }

                    conn.Close();
                }
                catch
                {
                    // ignored
                }
            }

            return Result;
        }

        //return [-1,-1] not connected, [0,-1] old data, [seeing, seeing_extinction] good data
        public double[] Get_Seeing()
        {
            double dT = 0;
            double[] result = {-1, -1};

            using (var conn = new NpgsqlConnection(services_conn_string))
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
                catch
                {
                    // ignored
                }
            }

            return result;
        }

        public static bool Get_Weather_Dome(bool dome, double skyTemp, double skyStd, double wind)
        {
            switch (dome)
            {
                //if dome closed
                case false when wind > 3:
                case false when skyTemp > -18:
                case false when skyStd > 1:
                    return false;
                case false: return true;
                //if dome opened
                case true when wind > 6:
                case true when skyTemp > -14:
                case true when skyStd > 3:
                    return false;
                case true: return true;
            }
        }

        public static bool Get_Weather_Obs(bool obs, double ext, double extStd)
        {
            switch (obs)
            {
                //standby
                case false when ext > 0.3:
                case false when extStd > 0.1:
                    return false;
                case false:
                    return true;
                //observation in run
                case true when ext > 0.6:
                case true when extStd > 0.2:
                    return false;
                case true:
                    return true;
            }
        }
    }
}