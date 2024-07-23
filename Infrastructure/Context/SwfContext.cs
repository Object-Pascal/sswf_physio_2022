using Core.Domain;
using Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Infrastructure
{
    public class SwfContext : DbContext
    {
        private SwfSeeder _seeder;
        public SwfContext(DbContextOptions<SwfContext> options)
            : base(options) 
        {
            _seeder = new SwfSeeder();
        }

        public DbSet<Therapist> Therapists { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientFile> PatientFiles { get; set; }
        public DbSet<Remark> Remarks { get; set; }
        public DbSet<Treatment> Treatments { get; set; }
        public DbSet<TreatmentPlan> TreatmentPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            List<Therapist> therapists = _seeder.SeedTherapists();
            List<Patient> patients = _seeder.SeedPatients();
            List<PatientFile> patientFiles = _seeder.SeedPatientFiles();

            builder.Entity<Therapist>().HasData(therapists);
            builder.Entity<Patient>().HasData(patients);
            builder.Entity<PatientFile>().HasData(patientFiles);

            builder.Entity<Patient>()
                .HasOne(x => x.PatientFile).WithOne(x => x.Patient).HasForeignKey<Patient>(x => x.PatientFileId).HasConstraintName("FK_PatientFileId_PatientFile").OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(x => x.Patient).WithMany(x => x.Appointments).HasForeignKey(x => x.PatientId).HasConstraintName("FK_PatientId_Appointment").OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Appointment>()
                .HasOne(x => x.Therapist).WithMany(x => x.Appointments).HasForeignKey(x => x.TherapistId).HasConstraintName("FK_TherapistId_Appointment").OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Remark>()
                .HasOne(x => x.RemarkMadeBy).WithMany(x => x.Remarks).HasForeignKey(x => x.RemarkMadeById).HasConstraintName("FK_RemarkMadeById_Therapist").OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Remark>()
                .HasOne(x => x.PatientFile).WithMany(x => x.Remarks).HasForeignKey(x => x.PatientFileId).HasConstraintName("FK_RMK_PatientFileId_PatientFile").OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PatientFile>()
                .HasOne(x => x.IntakeBy).WithMany(x => x.IntakeByPatientFiles).HasForeignKey(x => x.IntakeById).HasConstraintName("FK_IntakeBy_Therapist").OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PatientFile>()
                .HasOne(x => x.UnderSupervisionBy).WithMany(x => x.UnderSuperVisionByPatientFiles).HasForeignKey(x => x.UnderSupervisionById).HasConstraintName("FK_UnderSupervisionById_Therapist").OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PatientFile>()
                .HasOne(x => x.HeadOfTreatment).WithMany(x => x.HeadOfTreatmentPatientFiles).HasForeignKey(x => x.HeadOfTreatmentId).HasConstraintName("FK_HeadOfTreatmentId_Therapist").OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PatientFile>()
                .HasOne(x => x.TreatmentPlan).WithOne(x => x.PatientFile).HasForeignKey<PatientFile>(x => x.TreatmentPlanId).HasConstraintName("FK_TreatmentPlanId_TreatmentPlan").OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TreatmentPlan>()
                .HasOne(x => x.UnderTreatmentBy).WithMany(x => x.TreatmentPlans).HasForeignKey(x => x.UnderTreatmentById).HasConstraintName("FK_UnderTreatmentById_TreatmentPlan").OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Treatment>()
                .HasOne(x => x.TreatmentPlan).WithMany(x => x.Treatments).HasForeignKey(x => x.TreatmentPlanId).HasConstraintName("FK_TreatmentPlanId_Treatment").OnDelete(DeleteBehavior.Restrict);
            base.OnModelCreating(builder);
        }
    }
}