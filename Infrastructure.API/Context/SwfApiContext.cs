using Core.Domain;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Infrastructure.API
{
    public class SwfApiContext : DbContext
    {
        public static bool IsDevelopment { get; set; } = false;

        public SwfApiContext(DbContextOptions<SwfApiContext> options)
            : base(options) { }

        public DbSet<Diagnose> Diagnoses { get; set; }
        public DbSet<TreatmentType> TreatmentTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (IsDevelopment)
            {
                int id = 0;
                List<Diagnose> csvDiagnoses = ReadCsv("Infrastructure.API\\Vektis\\Vektis_Diagnoses.csv",
                    (c) => new Diagnose()
                    {
                        Id = ++id,
                        Code = c.GetField<string>("Code"),
                        BodyLocalization = c.GetField<string>("lichaamslocalisatie"),
                        Pathology = c.GetField<string>("pathologie") == "?" ? "Unknown" : c.GetField<string>("pathologie")
                    }).ToList();

                id = 0;
                List<TreatmentType> csvTreatmentTypes = ReadCsv("Infrastructure.API\\Vektis\\Vektis_TreatmentTypes.csv",
                    (c) => new TreatmentType()
                    {
                        Id = ++id,
                        Code = c.GetField<string>("Waarde").ToString(),
                        Description = c.GetField<string>("Omschrijving"),
                        IsExplanationMandatory = c.GetField<string>("Toelichting verplicht") == "Ja"
                    }).ToList();

                builder.Entity<Diagnose>().HasData(csvDiagnoses);
                builder.Entity<TreatmentType>().HasData(csvTreatmentTypes);
            }
            base.OnModelCreating(builder);
        }

        private static IEnumerable<T> ReadCsv<T>(string path, Func<CsvReader, T> mapData)
        {
            using (StreamReader reader = new StreamReader(Path.Combine(Environment.CurrentDirectory.Replace("Web_App", "").Replace("Portal.API", ""), path)))
            {
                CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                    yield return mapData(csv);
            }
        }
    }
}