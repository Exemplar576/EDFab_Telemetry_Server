using System;
using System.Linq;

namespace EDFab_Telemetry_Server
{
    class Sensors
    {
        public static void Parse(string msg, string addr)
        {
            SqliteDb ctx = new SqliteDb();
            SensorInfo data = ctx.sensorInfo
                .Where(i => msg.Substring(0, 4).Equals(i.SID))
                .FirstOrDefault();
            if (data != null)
            {
                data.IP = addr;
                data.Values.Add(new SensorValues()
                {
                    DateTime = DateTime.Now.ToString(),
                    Value = msg.Substring(4)
                });
                ctx.SaveChanges();
            }
        }
        private static void Sensor_Notification(string msg, string addr)
        {

        }
    }
}
