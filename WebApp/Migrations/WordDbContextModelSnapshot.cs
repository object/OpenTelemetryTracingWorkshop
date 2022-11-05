﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApp;

#nullable disable

namespace WebApp.Migrations
{
    [DbContext(typeof(WordDbContext))]
    partial class WordDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("WebApp.Words", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Word")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Words", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Word = "quick"
                        },
                        new
                        {
                            Id = 2,
                            Word = "brown"
                        },
                        new
                        {
                            Id = 3,
                            Word = "fox"
                        },
                        new
                        {
                            Id = 4,
                            Word = "jumped"
                        },
                        new
                        {
                            Id = 5,
                            Word = "over"
                        },
                        new
                        {
                            Id = 6,
                            Word = "the"
                        },
                        new
                        {
                            Id = 7,
                            Word = "lazy"
                        },
                        new
                        {
                            Id = 8,
                            Word = "dog"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}