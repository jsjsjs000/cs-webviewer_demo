﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using static SmartHomeTool.SmartHomeLibrary.Commands;

namespace InteligentnyDomRelay.SmartHomeLibrary
{
	[Table("devices")]
	public class Devices
	{
		[Required, Key]
		public uint Address { get; set; }
		public CentralUnitDeviceItem.LineNumber LineNumber { get; set; }
		public DeviceVersion.HardwareType1Enum HardwareType1 { get; set; }
		public DeviceVersion.HardwareType2Enum HardwareType2 { get; set; }
		public byte HardwareSegmentsCount { get; set; }
		public byte HardwareVersion { get; set; }
		//public DbSet<Devices> ParentItem { get; set; }
		public bool Active { get; set; }
	}

	[Table("devices_cu")]
	internal class DevicesCu
	{
		public uint Address { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime LastUpdated { get; set; }
		public bool Error { get; set; }
		public DateTime? ErrorFrom { get; set; }
		public uint Uptime { get; set; }
		public float Vin { get; set; }
	}

	[Table("devices_relays")]
	internal class DevicesRelays
	{
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime LastUpdated { get; set; }
		public bool Relay { get; set; }
		public bool Error { get; set; }
		public DateTime? ErrorFrom { get; set; }
		public uint Uptime { get; set; }
		public float Vin { get; set; }
	}

	[Table("devices_temperatures")]
	internal class DevicesTemperatures
	{
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime LastUpdated { get; set; }
		public float Temperature { get; set; }
		public bool Error { get; set; }
		public DateTime? ErrorFrom { get; set; }
		public uint Uptime { get; set; }
		public float Vin { get; set; }
	}

	[Table("devices_heating")]
	internal class DevicesHeating
	{
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime LastUpdated { get; set; }
		public byte Mode { get; set; }
		public float SettingTemperature { get; set; }
		public bool Relay { get; set; }
	}

	internal class HistoryRelays
	{
		public int Id { get; set; }
		public DateTime Dt { get; set; }
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public bool Relay { get; set; }
		public bool Error { get; set; }
		public float Vin { get; set; }
	}

	internal class HistoryTemperatures
	{
		public int Id { get; set; }
		public DateTime Dt { get; set; }
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public float Temperature { get; set; }
		public bool Error { get; set; }
		public float Vin { get; set; }
	}

	internal class HistoryHeating
	{
		public int Id { get; set; }
		public DateTime Dt { get; set; }
		public uint Address { get; set; }
		public byte Segment { get; set; }
		public byte Mode { get; set; }
		public float SettingTemperature { get; set; }
		public bool Relay { get; set; }
	}

	public abstract class VisualComponent
	{
		public string Name = string.Empty;
		public List<VisualComponent> VisualSubItems = new();
	}

	public class GroupVisualComponent : VisualComponent { }

	public class HeatingVisualComponent : VisualComponent
	{
		public Devices DeviceItem;
		public byte DeviceSegment;
		public List<HeatingVisualComponentSubItem> SubItems = new();
		public HeatingVisualComponentControl Control = new();
	}

	public class HeatingVisualComponentSubItem
	{
		public string Name = string.Empty;
		public Devices DeviceItem;
		public byte DeviceSegment;
	}

	public class HeatingVisualComponentControl
	{
		public enum Mode { Off, Auto, Manual };
		public Mode HeatingMode;
		public TimeSpan DayFrom = new(8, 0, 0);
		public TimeSpan NightFrom = new(22, 0, 0);
		public float ManualTemperature = 21;
		public float DayTemperature = 21;
		public float NightTemperature = 18;

		public static string ModeToString(Mode mode) => mode switch
		{
			Mode.Off => "Wyłączone",
			Mode.Auto => "Auto",
			Mode.Manual => "Manualne",
			_ => "",
		};
	}

	public class ModelCacheKeyFactory : IModelCacheKeyFactory
	{
		private static string getKey() => DateTime.Now.ToString("yyyy-MM");

		public object Create(DbContext context)
		{
			return new
			{
				Type = context.GetType(),
				Schema = getKey(),
			};
		}
	}

	internal class DatabaseContext : DbContext
	{
		//public DatabaseContext() { }

		//public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

		public DbSet<Devices> Devices => Set<Devices>();
		public DbSet<DevicesCu> DevicesCu => Set<DevicesCu>();
		public DbSet<DevicesRelays> DevicesRelays => Set<DevicesRelays>();
		public DbSet<DevicesTemperatures> DevicesTemperatures => Set<DevicesTemperatures>();
		public DbSet<DevicesHeating> DevicesHeatings => Set<DevicesHeating>();
		public DbSet<HistoryRelays> HistoryRelays => Set<HistoryRelays>();
		public DbSet<HistoryTemperatures> HistoryTemperatures => Set<HistoryTemperatures>();
		public DbSet<HistoryHeating> HistoryHeating => Set<HistoryHeating>();

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
				/// Change model after month change
				/// https://stackoverflow.com/questions/39499470/dynamically-changing-schema-in-entity-framework-core
			optionsBuilder.ReplaceService<IModelCacheKeyFactory, ModelCacheKeyFactory>();
			optionsBuilder.UseMySQL("server=localhost;database=inteligentny_dom;user=inteligentny_dom;password=3ABItuPEzani");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
				/// EF Core Enum - https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=data-annotations
			modelBuilder.Entity<Devices>()
					.Property(e => e.LineNumber)
					.HasConversion(
							v => v.ToString(),
							v => (CentralUnitDeviceItem.LineNumber)Enum.Parse(typeof(CentralUnitDeviceItem.LineNumber), v));
			modelBuilder.Entity<Devices>()
					.Property(e => e.HardwareType1)
					.HasConversion(
							v => v.ToString(),
							v => (DeviceVersion.HardwareType1Enum)Enum.Parse(typeof(DeviceVersion.HardwareType1Enum), v));
			modelBuilder.Entity<Devices>()
					.Property(e => e.HardwareType2)
					.HasConversion(
							v => v.ToString(),
							v => (DeviceVersion.HardwareType2Enum)Enum.Parse(typeof(DeviceVersion.HardwareType2Enum), v));
			DateTime now = DateTime.Now;
			modelBuilder.Entity<HistoryRelays>().ToTable($"history_relays_{now.Year}_{now.Month:d2}");
			modelBuilder.Entity<HistoryTemperatures>().ToTable($"history_temperatures_{now.Year}_{now.Month:d2}");
			modelBuilder.Entity<HistoryHeating>().ToTable($"history_heating_{now.Year}_{now.Month:d2}");
			modelBuilder.Entity<DevicesCu>().HasKey(dt => dt.Address);
			modelBuilder.Entity<DevicesRelays>().HasKey(dt => new { dt.Address, dt.Segment });
			modelBuilder.Entity<DevicesTemperatures>().HasKey(dt => new { dt.Address, dt.Segment });
			modelBuilder.Entity<DevicesHeating>().HasKey(dt => new { dt.Address, dt.Segment });
		}
	}
}

/*
CREATE TABLE IF NOT EXISTS `history_relays_2022_10` (
	`Id` INT(11) NOT NULL AUTO_INCREMENT,
	`Dt` DATETIME NOT NULL,
	`Address` INT(10) UNSIGNED NOT NULL,
	`Segment` TINYINT(3) UNSIGNED NOT NULL,
	`Relay` TINYINT(3) UNSIGNED NOT NULL,
	`Error` TINYINT(3) UNSIGNED NOT NULL,
	`Vin` FLOAT NOT NULL,
	PRIMARY KEY (`Id`) USING BTREE,
	INDEX `Dt` (`Dt`) USING BTREE,
	INDEX `Address` (`Address`) USING BTREE
)
COLLATE='utf8_general_ci' ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `history_temperatures_2022_10` (
	`Id` INT(11) NOT NULL AUTO_INCREMENT,
	`Dt` DATETIME NOT NULL,
	`Address` INT(10) UNSIGNED NOT NULL,
	`Segment` TINYINT(3) UNSIGNED NOT NULL,
	`Temperature` FLOAT UNSIGNED NOT NULL,
	`Error` TINYINT(3) UNSIGNED NOT NULL,
	`Vin` FLOAT NOT NULL,
	PRIMARY KEY (`Id`) USING BTREE,
	INDEX `Dt` (`Dt`) USING BTREE,
	INDEX `Address` (`Address`) USING BTREE
)
COLLATE='utf8_general_ci' ENGINE=InnoDB;
*/
