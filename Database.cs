using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EDFab_Telemetry_Server
{
    class PacketData
    {
        public string Request { get; set; } = "";
        public string Type { get; set; } = "";
        public string Error { get; set; } = "";
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }

    //SQL Classes
    public class SqliteDb : DbContext
    {
        public DbSet<UserInfo> userInfo { get; set; }
        public DbSet<UserPerms> userPerms { get; set; }
        public DbSet<SensorInfo> sensorInfo { get; set; }
        public DbSet <SensorValues> sensorValues { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite("Data Source=Data/Storage.db;");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserInfo>().ToTable("UserInfo")
                .HasMany(i => i.Perms).WithOne().HasForeignKey(k => k.EmailKey);
            modelBuilder.Entity<SensorInfo>().ToTable("SensorInfo")
                .HasMany(i => i.Values).WithOne().HasForeignKey(k => k.SIDKey);
        }
    }
    public class UserInfo
    {
        [Key]
        public string Email { get; set; }
        public string Priv { get; set; }
        public string Token { get; set; }
        public string Hash { get; set; }
        public virtual ICollection<UserPerms> Perms { get; set; }
    }
    public class UserPerms
    {
        [Key]
        public string EmailKey { get; set; }
        public string SID { get; set; }
    }
    public class SensorInfo
    {
        [Key]
        public string SID { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public virtual ICollection<SensorValues> Values { get; set; }
    }
    public class SensorValues
    {
        [Key]
        public string SIDKey { get; set; }
        public string DateTime { get; set; }
        public string Value { get; set; }
    }
}