using IOTOI.Model.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOTOI.Model.Db
{
    public class Context : DbContext
    {
        public DbSet<ZigBee.ZigBeeEndDevice> ZigBeeEndDevice { get; set; }
        public DbSet<ZigBee.ZigBeeEndPoint> ZigBeeEndPoint { get; set; }

        public DbSet<ZigBee.ZigBeeInCluster> ZigBeeInCluster { get; set; }
        public DbSet<ZigBee.ZigBeeOutCluster> ZigBeeOutCluster { get; set; }
        public DbSet<ZigBee.ZigBeeInClusterAttribute> ZigBeeInClusterAttribute { get; set; }
        public DbSet<ZigBee.ZigBeeOutClusterAttribute> ZigBeeOutClusterAttribute { get; set; }

        public DbSet<ProtocolType> ProtocolType { get; set; }

        public DbSet<CCTV> CCTV { get; set; }

        //public DbSet<Space> Space { get; set; }

        public DbSet<ZWave.ZWaveNode> ZWaveNode { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=iotoiapp.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            #region ZigBeeEndDevice MacAddress Unique 선언
            modelBuilder.Entity<ZigBee.ZigBeeEndDevice>()
                .HasIndex(b => b.MacAddress)
                .IsUnique();
            #endregion

            #region ProtocolType Name Column Unique 선언
            modelBuilder.Entity<ProtocolType>()
                .HasIndex(b => b.Name)
                .IsUnique();
            #endregion

            #region Space Name Column Unique 선언
            //modelBuilder.Entity<Space>()
            //    .HasIndex(b => b.Name)
            //    .IsUnique();
            #endregion

            #region EndPoint EpNum, MacAddress Column Unique 선언
            modelBuilder.Entity<ZigBee.ZigBeeEndPoint>()
                .HasIndex(b => new { b.EpNum, b.MacAddress })
                .IsUnique();
            #endregion

            #region ZigBeeEndPoint Table ForeignKey(ZigBeeInCluster EndPointId) 선언            
            modelBuilder.Entity<ZigBee.ZigBeeEndPoint>()
                .HasMany(p => p.ZigBeeInClusters)
                .WithOne()
                .HasForeignKey(p => p.ParentId);
            #endregion

            #region ZigBeeEndPoint Table ForeignKey(ZigBeeOutCluster EndPointId) 선언            
            modelBuilder.Entity<ZigBee.ZigBeeEndPoint>()
                .HasMany(p => p.ZigBeeOutClusters)
                .WithOne()
                .HasForeignKey(p => p.ParentId);
            #endregion

            #region ZigBeeInCluster Table ForeignKey(ZigBeeInClusterAttribute ClusterId) 선언
            modelBuilder.Entity<ZigBee.ZigBeeInCluster>()
                .HasMany(p => p.ZigBeeInClusterAttributes)
                .WithOne()
                .HasForeignKey(p => p.ParentId);
            #endregion

            #region ZigBeeOutCluster Table ForeignKey(ZigBeeOutClusterAttribute ClusterId) 선언
            modelBuilder.Entity<ZigBee.ZigBeeOutCluster>()
                .HasMany(p => p.ZigBeeOutClusterAttributes)
                .WithOne()
                .HasForeignKey(p => p.ParentId);
            #endregion

            #region ZWaveNodes Id 선언
            modelBuilder.Entity<ZWave.ZWaveNode>()
                .HasIndex(b => b.Id)
                .IsUnique();
            #endregion

            // CCTVId required.
            modelBuilder.Entity<CCTV>()
                .Property(c => c.CCTVId)
                .IsRequired();

            modelBuilder.Entity<CCTV>()
                .Property(c => c.IpAddress)
                .IsRequired();

            modelBuilder.Entity<CCTV>()
                .Property(c => c.CCTVName)
                .IsRequired();
        }

    }
}
