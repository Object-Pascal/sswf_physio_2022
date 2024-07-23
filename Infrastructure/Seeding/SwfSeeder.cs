﻿using Core.Domain;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Seeding
{
    public class SwfSeeder
    {
        private static List<Therapist> _therapistSeeds;
        private static List<Patient> _patientSeeds;
        private static List<PatientFile> _patientFileSeeds;
        private static List<IdentityRole> _roleSeeds;
        private static List<ApplicationUser> _identityUserSeeds;
        private static List<IdentityUserRole<string>> _identityUserRoleSeeds;

        private string placeHolderAvatarBase64 = @"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";
        public List<Therapist> SeedTherapists()
        {
            if (_therapistSeeds == null)
            {
                _therapistSeeds = new List<Therapist>()
                {
                    new Therapist()
                    {
                        Id = 1,
                        ProfilePictureBase64 = placeHolderAvatarBase64,
                        Name = "Therapist01",
                        BIGNumber = "123",
                        PersonNumber = "123",
                        EmailAddress = "therapist01@mail.com",
                        IsStudent = false,
                        AvailableFrom = DateTime.Now,
                        AvailableTo = DateTime.Now.AddDays(7)
                    },
                    new Therapist()
                    {
                        Id = 2,
                        ProfilePictureBase64 = placeHolderAvatarBase64,
                        Name = "Therapist02",
                        BIGNumber = "123",
                        PersonNumber = "123",
                        EmailAddress = "therapist02@mail.com",
                        IsStudent = false,
                        AvailableFrom = DateTime.Now,
                        AvailableTo = DateTime.Now.AddDays(7)
                    },
                    new Therapist()
                    {
                        Id = 3,
                        ProfilePictureBase64 = placeHolderAvatarBase64,
                        Name = "Therapist03",
                        BIGNumber = "123",
                        PersonNumber = "123",
                        EmailAddress = "therapist03@mail.com",
                        IsStudent = false,
                        AvailableFrom = DateTime.Now,
                        AvailableTo = DateTime.Now.AddDays(7)
                    },
                    new Therapist()
                    {
                        Id = 4,
                        ProfilePictureBase64 = placeHolderAvatarBase64,
                        Name = "Student01",
                        BIGNumber = "123",
                        PersonNumber = "123",
                        EmailAddress = "student01@mail.com",
                        IsStudent = true,
                        AvailableFrom = DateTime.Now,
                        AvailableTo = DateTime.Now.AddDays(7)
                    }
                };
            }
            return _therapistSeeds;
        }

        public List<ApplicationUser> SeedIdentityUsers()
        {
            if (_identityUserSeeds == null)
            {
                if (_therapistSeeds == null)
                    SeedTherapists();
                if (_patientSeeds == null)
                    SeedPatients();

                _identityUserSeeds = new List<ApplicationUser>();

                PasswordHasher<ApplicationUser> hasher = new PasswordHasher<ApplicationUser>();
                foreach (Therapist t in _therapistSeeds)
                {
                    _identityUserSeeds.Add(new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        DataId = t.Id,
                        UserName = t.Name,
                        NormalizedUserName = t.Name.ToUpper(),
                        PasswordHash = hasher.HashPassword(null, $"Password{t.Name}")
                    });
                }

                foreach (Patient t in _patientSeeds)
                {
                    _identityUserSeeds.Add(new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        DataId = t.Id,
                        UserName = t.Name,
                        NormalizedUserName = t.Name.ToUpper(),
                        PasswordHash = hasher.HashPassword(null, $"Password{t.Name}")
                    });
                }
            }
            return _identityUserSeeds;
        }

        public List<IdentityUserRole<string>> SeedIdentityUserRoles()
        {
            if (_identityUserRoleSeeds == null)
            {
                if (_identityUserSeeds == null)
                    SeedIdentityUsers();
                if (_roleSeeds == null)
                    SeedRoles();

                _identityUserRoleSeeds = new List<IdentityUserRole<string>>();

                foreach (IdentityUser i in _identityUserSeeds)
                {
                    string roleId = Guid.Empty.ToString();
                    switch(i.UserName.Substring(0, i.UserName.Length - 2))
                    {
                        case Role.ADMINISTRATOR_ROLE:
                            roleId = _roleSeeds.First(x => x.Name == Role.ADMINISTRATOR_ROLE).Id;
                            break;
                        case Role.THERAPIST_ROLE:
                            roleId = _roleSeeds.First(x => x.Name == Role.THERAPIST_ROLE).Id;
                            break;
                        case Role.STUDENTTHERAPIST_ROLE:
                            roleId = _roleSeeds.First(x => x.Name == Role.STUDENTTHERAPIST_ROLE).Id;
                            break;
                        case Role.PATIENT_ROLE:
                            roleId = _roleSeeds.First(x => x.Name == Role.PATIENT_ROLE).Id;
                            break;
                    }

                    _identityUserRoleSeeds.Add(new IdentityUserRole<string>
                    {
                        UserId = i.Id,
                        RoleId = roleId
                    });
                }
            }
            return _identityUserRoleSeeds;
        }

        public List<Patient> SeedPatients()
        {
            if (_patientSeeds == null)
            {
                _patientSeeds = new List<Patient>()
                {
                    new Patient()
                    {
                        Id = 1,
                        Name = "Patient01",
                        ProfilePictureBase64 = placeHolderAvatarBase64,
                        IsStudent = true,
                        StudentNumber = "01",
                        Email = "patient01@gmail.com",
                        PatientNumber = Guid.NewGuid().ToString(),
                        TelephoneNumber = "0612345678",
                        DateOfBirth = new DateTime(2000, 1, 1),
                        Gender = "Male",
                        City = "City01",
                        Street = "Street01",
                        HouseNumber = "01",
                        PatientFileId = 1
                    },
                    new Patient()
                    {
                        Id = 2,
                        Name = "Patient02",
                        ProfilePictureBase64 = placeHolderAvatarBase64,
                        IsStudent = false,
                        StudentNumber = null,
                        Email = "patient02@gmail.com",
                        PatientNumber = Guid.NewGuid().ToString(),
                        TelephoneNumber = "0623456789",
                        DateOfBirth = new DateTime(2002, 10, 23),
                        Gender = "Female",
                        City = "City02",
                        Street = "Street02",
                        HouseNumber = "02",
                        PatientFileId = 2
                    },
                    new Patient()
                    {
                        Id = 3,
                        Name = "Patient03",
                        ProfilePictureBase64 = placeHolderAvatarBase64,
                        IsStudent = false,
                        StudentNumber = null,
                        Email = "patient03@gmail.com",
                        PatientNumber = Guid.NewGuid().ToString(),
                        TelephoneNumber = "0634567890",
                        DateOfBirth = new DateTime(1998, 2, 12),
                        Gender = "Female",
                        City = "City03",
                        Street = "Street03",
                        HouseNumber = "03",
                        PatientFileId = 3
                    }
                };
            }
            return _patientSeeds;
        }

        public List<PatientFile> SeedPatientFiles()
        {
            if (_patientFileSeeds == null)
            {
                _patientFileSeeds = new List<PatientFile>()
                {
                    new PatientFile()
                    {
                        Id = 1,
                        Age = 21,
                        IntakeById = 1,
                        HeadOfTreatmentId = 1,
                        DiagnoseCode = "1000",
                        RegisterDate = DateTime.Now,
                        ResignDate = DateTime.Now.AddDays(1),
                        GlobalDescription = "-",
                        ExtraDescription = null,
                        TreatmentPlan = null,
                        TreatmentPlanId = null,
                        UnderSupervisionBy = null,
                        UnderSupervisionById = null
                    },
                    new PatientFile()
                    {
                        Id = 2,
                        Age = 19,
                        IntakeById = 1,
                        HeadOfTreatmentId = 1,
                        DiagnoseCode = "1001",
                        RegisterDate = DateTime.Now,
                        ResignDate = DateTime.Now.AddDays(1),
                        GlobalDescription = "-",
                        ExtraDescription = null,
                        TreatmentPlan = null,
                        TreatmentPlanId = null,
                        UnderSupervisionBy = null,
                        UnderSupervisionById = null
                    },
                    new PatientFile()
                    {
                        Id = 3,
                        Age = 23,
                        IntakeById = 1,
                        HeadOfTreatmentId = 1,
                        DiagnoseCode = "1002",
                        RegisterDate = DateTime.Now,
                        ResignDate = DateTime.Now.AddDays(1),
                        GlobalDescription = "-",
                        ExtraDescription = null,
                        TreatmentPlan = null,
                        TreatmentPlanId = null,
                        UnderSupervisionBy = null,
                        UnderSupervisionById = null
                    }
                };
            }
            return _patientFileSeeds;
        }

        public List<IdentityRole> SeedRoles()
        {
            if (_roleSeeds == null)
            {
                _roleSeeds = new List<IdentityRole>()
                {
                    new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = Role.ADMINISTRATOR_ROLE,
                        NormalizedName = Role.ADMINISTRATOR_ROLE.ToUpper()
                    },
                    new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = Role.THERAPIST_ROLE,
                        NormalizedName = Role.THERAPIST_ROLE.ToUpper()
                    },
                    new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = Role.STUDENTTHERAPIST_ROLE,
                        NormalizedName = Role.STUDENTTHERAPIST_ROLE.ToUpper()
                    },
                    new IdentityRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = Role.PATIENT_ROLE,
                        NormalizedName = Role.PATIENT_ROLE.ToUpper()
                    },
                };
            }
            return _roleSeeds;
        }
    }
}