﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebApp.Entities;

#nullable disable

namespace WebApp.Migrations
{
    [DbContext(typeof(NpgsqlContext))]
    [Migration("20230308090929_update-lastlogintype")]
    partial class updatelastlogintype
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebApp.Entities.User", b =>
                {
                    b.Property<string>("OpenId")
                        .HasColumnType("text");

                    b.Property<int?>("Gender")
                        .HasColumnType("integer");

                    b.Property<string>("HeadImg")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LastLoginTime")
                        .HasColumnType("timestamp");

                    b.Property<string>("NickName")
                        .HasColumnType("text");

                    b.HasKey("OpenId");

                    b.ToTable("t_users", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}