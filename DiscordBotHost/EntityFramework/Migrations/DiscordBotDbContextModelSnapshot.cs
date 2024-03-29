﻿// <auto-generated />
using System;
using DiscordBotHost.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DiscordBotHost.EntityFramework.Migrations
{
    [DbContext(typeof(DiscordBotDbContext))]
    partial class DiscordBotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DiscordBotHost.EntityFramework.User", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("DiscordUserId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("FirebaseId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("LinksChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DiscordBotHost.Features.Auditions.Opportunity", b =>
                {
                    b.Property<int>("OpportunityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("OpportunityId"));

                    b.Property<DateTimeOffset?>("AuditionDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("AuditionEndDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Company")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("JobName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShowName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Summary")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("OpportunityId");

                    b.ToTable("Opportunities");
                });

            modelBuilder.Entity("DiscordBotHost.Features.ContentMonitor.ContentInspection", b =>
                {
                    b.Property<int>("ContentInspectionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ContentInspectionId"));

                    b.Property<string>("ContentSnapshotUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<double>("DifferenceThreshold")
                        .HasColumnType("float");

                    b.Property<double>("DifferenceValue")
                        .HasColumnType("float");

                    b.Property<string>("Differences")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Error")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MonitorContentRequestId")
                        .HasColumnType("int");

                    b.HasKey("ContentInspectionId");

                    b.HasIndex("MonitorContentRequestId");

                    b.ToTable("ContentInspections");
                });

            modelBuilder.Entity("DiscordBotHost.Features.ContentMonitor.MonitorContentRequest", b =>
                {
                    b.Property<int>("MonitorContentRequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MonitorContentRequestId"));

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<double>("DifferenceThreshold")
                        .HasColumnType("float");

                    b.Property<decimal>("DiscordUserId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<long>("Interval")
                        .HasColumnType("bigint");

                    b.Property<int>("Repeat")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("RunUntil")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Selector")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MonitorContentRequestId");

                    b.ToTable("MonitorContentRequests");
                });

            modelBuilder.Entity("DiscordBotHost.Features.ContentMonitor.ContentInspection", b =>
                {
                    b.HasOne("DiscordBotHost.Features.ContentMonitor.MonitorContentRequest", "MonitorContentRequest")
                        .WithMany("ContentInspections")
                        .HasForeignKey("MonitorContentRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MonitorContentRequest");
                });

            modelBuilder.Entity("DiscordBotHost.Features.ContentMonitor.MonitorContentRequest", b =>
                {
                    b.Navigation("ContentInspections");
                });
#pragma warning restore 612, 618
        }
    }
}
