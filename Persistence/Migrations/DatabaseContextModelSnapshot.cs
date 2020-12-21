﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models;

namespace Models.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Models.User", b =>
                {
                    b.Property<string>("Username")
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getdate()");

                    b.Property<bool>("Disabled")
                        .HasColumnType("bit");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<byte[]>("HashedPassword")
                        .IsRequired()
                        .HasColumnType("binary(32)");

                    b.Property<long>("Phone")
                        .HasColumnType("bigint");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("char(10)");

                    b.HasKey("Username");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Models.Vehicle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("AdditionalProperties")
                        .HasColumnType("varchar(200)");

                    b.Property<int>("ChassisType")
                        .HasColumnType("int");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("char(20)");

                    b.Property<string>("Comment")
                        .HasColumnType("varchar(1000)");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Defects")
                        .HasColumnType("varchar(200)");

                    b.Property<int>("DriveWheels")
                        .HasColumnType("int");

                    b.Property<bool>("Hidden")
                        .HasColumnType("bit");

                    b.Property<string>("Images")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("KilometersDriven")
                        .HasColumnType("int");

                    b.Property<int>("ModelId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Modified")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2")
                        .HasComputedColumnSql("getdate()");

                    b.Property<int>("NumberOfDoors")
                        .HasColumnType("int");

                    b.Property<string>("OriginalPurchaseCountry")
                        .HasColumnType("varchar(50)");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.Property<int>("Seats")
                        .HasColumnType("int");

                    b.Property<int>("SteeringWheelSide")
                        .HasColumnType("int");

                    b.Property<string>("UploaderUsername")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<bool>("Used")
                        .HasColumnType("bit");

                    b.Property<string>("Vin")
                        .HasColumnType("char(17)");

                    b.Property<int>("Weight")
                        .HasColumnType("int");

                    b.Property<string>("WheelSize")
                        .IsRequired()
                        .HasColumnType("char(20)");

                    b.HasKey("Id");

                    b.HasIndex("ModelId");

                    b.HasIndex("UploaderUsername");

                    b.ToTable("Vehicles");
                });

            modelBuilder.Entity("Models.VehicleModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Brand")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("VehicleModels");
                });

            modelBuilder.Entity("UserVehicle", b =>
                {
                    b.Property<int>("LikedAdsId")
                        .HasColumnType("int");

                    b.Property<string>("LikedByUsername")
                        .HasColumnType("varchar(50)");

                    b.HasKey("LikedAdsId", "LikedByUsername");

                    b.HasIndex("LikedByUsername");

                    b.ToTable("UserLikedAds");
                });

            modelBuilder.Entity("Models.Vehicle", b =>
                {
                    b.HasOne("Models.VehicleModel", "VehicleModel")
                        .WithMany("Vehicles")
                        .HasForeignKey("ModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Models.User", "Uploader")
                        .WithMany("UploadedAds")
                        .HasForeignKey("UploaderUsername")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Models.Engine", "Engine", b1 =>
                        {
                            b1.Property<int>("VehicleId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .UseIdentityColumn();

                            b1.Property<int?>("EuroEmissionsStandard")
                                .HasColumnType("int");

                            b1.Property<int>("FuelType")
                                .HasColumnType("int");

                            b1.Property<int>("Hp")
                                .HasColumnType("int");

                            b1.Property<int>("Kw")
                                .HasColumnType("int");

                            b1.Property<int?>("NumberOfCylinders")
                                .HasColumnType("int");

                            b1.Property<int>("Type")
                                .HasColumnType("int");

                            b1.Property<float?>("Volume")
                                .HasColumnType("real");

                            b1.HasKey("VehicleId");

                            b1.ToTable("Vehicles");

                            b1.WithOwner()
                                .HasForeignKey("VehicleId");
                        });

                    b.OwnsOne("Models.Gearbox", "Gearbox", b1 =>
                        {
                            b1.Property<int>("VehicleId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .UseIdentityColumn();

                            b1.Property<int>("GearboxType")
                                .HasColumnType("int");

                            b1.Property<int>("NumberOfGears")
                                .HasColumnType("int");

                            b1.HasKey("VehicleId");

                            b1.ToTable("Vehicles");

                            b1.WithOwner()
                                .HasForeignKey("VehicleId");
                        });

                    b.OwnsOne("Models.YearMonth", "NextVehicleInspection", b1 =>
                        {
                            b1.Property<int>("VehicleId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .UseIdentityColumn();

                            b1.Property<int>("Month")
                                .HasColumnType("int");

                            b1.Property<int>("Year")
                                .HasColumnType("int");

                            b1.HasKey("VehicleId");

                            b1.ToTable("Vehicles");

                            b1.WithOwner()
                                .HasForeignKey("VehicleId");
                        });

                    b.OwnsOne("Models.YearMonth", "Purchased", b1 =>
                        {
                            b1.Property<int>("VehicleId")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int")
                                .UseIdentityColumn();

                            b1.Property<int>("Month")
                                .HasColumnType("int");

                            b1.Property<int>("Year")
                                .HasColumnType("int");

                            b1.HasKey("VehicleId");

                            b1.ToTable("Vehicles");

                            b1.WithOwner()
                                .HasForeignKey("VehicleId");
                        });

                    b.Navigation("Engine")
                        .IsRequired();

                    b.Navigation("Gearbox");

                    b.Navigation("NextVehicleInspection");

                    b.Navigation("Purchased");

                    b.Navigation("Uploader");

                    b.Navigation("VehicleModel");
                });

            modelBuilder.Entity("UserVehicle", b =>
                {
                    b.HasOne("Models.Vehicle", null)
                        .WithMany()
                        .HasForeignKey("LikedAdsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Models.User", null)
                        .WithMany()
                        .HasForeignKey("LikedByUsername")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Models.User", b =>
                {
                    b.Navigation("UploadedAds");
                });

            modelBuilder.Entity("Models.VehicleModel", b =>
                {
                    b.Navigation("Vehicles");
                });
#pragma warning restore 612, 618
        }
    }
}
