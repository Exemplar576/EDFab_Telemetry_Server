using System.Collections.Generic;
using System.Linq;

namespace EDFab_Telemetry_Server
{
    class Logins
    {
        public static PacketData Parse(PacketData packet)
        {
            switch (packet.Type)
            {
                case "Token":
                    return Token(packet);
                case "Login":
                    return Login(packet);
                case "Create":
                    return Create(packet);
                case "Verify":
                    return Verify(packet);
                case "Forogt":
                    return Forgot(packet);
                default:
                    packet.Error = "Invalid Request";
                    return packet;
            }
        }
        private static PacketData Token(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            UserInfo data = ctx.userInfo
                .Where(i => i.Email.Equals(packet.Data["Email"]) && i.Token.Equals(packet.Data["Token"]))
                .FirstOrDefault();
            if (data == null)
            {
                packet.Error = "Invalid Token";
            }
            return packet;
        }
        private static PacketData Login(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            UserInfo data = ctx.userInfo
                .Where(i => i.Email.Equals(packet.Data["Email"]))
                .FirstOrDefault();
            if (data != null && Encryption.Compare_Hash(data.Hash, packet.Data["Password"]))
            {
                packet.Data.Remove("Password");
                packet.Data.Add("Priv", data.Priv);
                packet.Data.Add("Token", data.Token);
            }
            else
            {
                packet.Error = "An incorrect password was entered.";
            }
            return packet;
        }
        private static PacketData Create(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            UserInfo data = ctx.userInfo
                .Where(i => i.Email.Equals(packet.Data["Email"]))
                .FirstOrDefault();
            if (data == null)
            {
                packet.Error = "This email already exists";
            }
            else
            {
                Email.Send(packet.Data["Email"], "EDFab Verify Email", $"Your code is: {packet.Data["Code"]}");
            }
            return packet;
        }
        private static PacketData Verify(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            ctx.userInfo.Add(new UserInfo
            {
                Email = packet.Data["Email"],
                Priv = "User",
                Token = "",
                Hash = Encryption.Hash(packet.Data["Password"]),
                Perms = new List<UserPerms>() 
                { 
                    new UserPerms() { SID = packet.Data["Email"].EndsWith("@edfab.co.uk") ? "*" : "none" } 
                }
            });
            ctx.SaveChanges();
            return packet;
        }
        private static PacketData Forgot(PacketData packet)
        {
            SqliteDb ctx = new SqliteDb();
            UserInfo data = ctx.userInfo
                .Where(i => i.Email.Equals(packet.Data["Email"]))
                .FirstOrDefault();
            if (data == null)
            {
                packet.Error = "Unable to change password.";
            }
            else
            {
                data.Hash = Encryption.Hash(packet.Data["Password"]);
                ctx.SaveChanges();
            }
            return packet;
        }
    }
}
