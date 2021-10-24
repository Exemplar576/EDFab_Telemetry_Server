using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace EDFab_Telemetry_Server
{
    class User
    {
        public static PacketData Parse(PacketData packet)
        {
            switch (packet.Type)
            {
                case "Request":
                    return Request(packet);
                case "Sensor":
                    return RequestSensor(packet);
                default:
                    packet.Error = "Invalid Request";
                    return packet;
            }
        }
        private static PacketData Request(PacketData packet)
        {
            int offset = int.Parse(packet.Data["Offset"]);
            SqliteDb ctx = new SqliteDb();
            List<string> sensors = ctx.userInfo.Include(i => i.Perms)
                .Where(i => i.Email.Equals(packet.Data["Email"]))
                .FirstOrDefault().Perms.Select(i => i.SID).ToList();
            packet.Data.Remove("Email");
            packet.Data.Remove("Offset");
            packet.Data = (sensors[0].Equals("*") ? ctx.sensorInfo : ctx.sensorInfo.Where(s => sensors.Contains(s.SID)))
                            .ToDictionary(k => k.Name, v => v.SID);
            packet.Data.Skip(offset).Take(packet.Data.Count < 10 ? packet.Data.Count : 10);
            return packet;
        }
        private static PacketData RequestSensor(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            List<string> values = ctx.sensorInfo.Include(i => i.Values)
                .Where(i => i.SID.Equals(packet.Data["ID"]))
                .FirstOrDefault().Values
                .Select(e => $"[{e.DateTime}] {e.Value}")
                .ToList();
            values.Reverse();
            int offset = int.Parse(packet.Data["Offset"]);
            packet.Data = values.Count > 0 ? values
                .GetRange(offset, values.Count - offset < 10 ? values.Count - offset : 10)
                .ToDictionary(k => k, v => v) : new Dictionary<string, string>();
            return packet;
        }
    }
    class Admin
    {
        public static PacketData Parse(PacketData packet)
        {
            switch (packet.Type)
            {
                case "RequestUsers":
                    return RequestUsers(packet);
                case "RequestSensors":
                    return RequestSensors(packet);
                case "RequestMotors":
                    return RequestMotors(packet);
                case "UserData":
                    return RequestUserData(packet);
                case "UpdateUser":
                    return Update_User(packet);
                case "AddSensor":
                    return AddSensor(packet);
                case "RemoveSensor":
                    return RemoveSensor(packet);
                case "RenameSensor":
                    return RenameSensor(packet);
                case "SensorPerms":
                    return Sensor_perms(packet);
                default:
                    packet.Error = "Invalid Request";
                    return packet;
            }
        }
        private static PacketData RequestUsers(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            int offset = int.Parse(packet.Data["Offset"]);
            packet.Data.Remove("Offset");
            packet.Data = ctx.userInfo
                .ToList()
                .GetRange(offset, ctx.userInfo.Count() - offset < 10 ? ctx.userInfo.Count() - offset : 10)
                .ToDictionary(k => k.Email, v => v.Email);
            return packet;
        }
        private static PacketData RequestUserData(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            UserInfo data = ctx.userInfo.Include(i => i.Perms)
                .Where(i => i.Email.Equals(packet.Data["Email"]))
                .FirstOrDefault();
            if (data == null)
            {
                packet.Error = "Email not in database";
            }
            else
            {
                string perms = "";
                packet.Data.Add("Priv", data.Priv);
                packet.Data.Add("Status", data.Hash.StartsWith("DISABLED") ? "Inactive" : "Active");
                foreach (UserPerms i in data.Perms)
                {
                    perms += $"{i.SID}, ";
                }
                packet.Data.Add("Perms", perms);
            }
            return packet;
        }
        private static PacketData Update_User(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            UserInfo data = ctx.userInfo
                .Where(i => i.Email.Equals(packet.Data["Email"]))
                .FirstOrDefault();
            if (data == null)
            {
                packet.Error = "Invalid User";
            }
            else
            {
                switch (packet.Data["Status"])
                {
                    case "Delete":
                        ctx.userInfo.Remove(data);
                        break;
                    case "Disable":
                        if (!data.Hash.StartsWith("DISABLED") && !data.Token.StartsWith("DISABLED"))
                        {
                            data.Hash = data.Hash.Insert(0, "DISABLED");
                            data.Token = data.Token.Insert(0, "DISABLED");
                        }
                        break;
                    case "Enable":
                        if (data.Hash.StartsWith("DISABLED") && data.Token.StartsWith("DISABLED"))
                        {
                            data.Hash = data.Hash.Substring(8);
                            data.Token = data.Token.Substring(8);
                        }
                        break;
                }
                ctx.SaveChanges();
            }
            return packet;
        }
        private static PacketData Sensor_perms(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            UserInfo data = ctx.userInfo.Include(i => i.Perms)
                .Where(i => i.Email.Equals(packet.Data["Email"]))
                .FirstOrDefault();
            if (data == null)
            {
                packet.Error = "User does not exist";
            }
            else
            {
                packet.Data.Remove("Email");
                List<UserPerms> sensors = packet.Data
                    .Where(e => bool.Parse(e.Value))
                    .Select(f => new UserPerms { SID = f.Key })
                    .ToList();
                data.Perms.Clear();
                data.Perms = sensors.Count > 0 ? sensors : new List<UserPerms>() { new UserPerms { SID = "none" } };
                ctx.SaveChanges();
            }
            return packet;
        }
        private static PacketData RequestSensors(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            int offset = int.Parse(packet.Data["Offset"]);
            packet.Data.Remove("Offset");
            packet.Data = ctx.sensorInfo
                .ToList()
                .GetRange(offset, ctx.sensorInfo.Count() - offset < 10 ? ctx.sensorInfo.Count() - offset : 10)
                .ToDictionary(k => k.Name, v => v.SID);
            return packet;
        }
        private static PacketData RequestMotors(PacketData packet)
        {
            packet.Error = "Not yet implemented";
            return packet;
        }
        private static PacketData AddSensor(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            SensorInfo data = ctx.sensorInfo
                .Where(i => i.Name.Equals(packet.Data["Name"]) || i.SID.Equals(packet.Data["ID"]))
                .FirstOrDefault();
            if (data != null)
            {
                packet.Error = "Sensor Name or ID already exists";
            }
            else
            {
                ctx.sensorInfo.Add(new SensorInfo
                {
                    Name = packet.Data["Name"],
                    SID = packet.Data["ID"],
                    IP = IPAddress.Any.ToString(),
                });
                ctx.SaveChanges();
            }
            return packet;
        }
        private static PacketData RemoveSensor(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            var data = ctx.sensorInfo.Include(i => i.Values)
                .Where(i => i.SID.Equals(packet.Data["ID"]))
                .FirstOrDefault();
            if (data != null)
            {
                data.Values.Clear();
                ctx.sensorInfo.Remove(data);
                ctx.SaveChanges();
                return packet;
            }
            packet.Error = "Invalid Sensor";
            return packet;
        }
        private static PacketData RenameSensor(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            SensorInfo data = ctx.sensorInfo
                .Where(i => i.Name.Equals(packet.Data["ID"]))
                .FirstOrDefault();
            if (data == null || ctx.sensorInfo.Any(i => i.Name.Equals(packet.Data["Name"])))
            {
                packet.Error = "Sensor name already in use.";
            }
            else
            {
                data.Name = packet.Data["Name"];
                ctx.SaveChanges();
            }
            return packet;
        }
    }
}
