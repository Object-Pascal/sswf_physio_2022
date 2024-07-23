using Core.Domain;
using System;
using System.Collections.Generic;

namespace Core.DomainServices
{
    public interface ITherapistRepository
    {
        Therapist Get(int id);
        (Therapist, Exception) GetByName(string name);
        (Therapist, Exception) GetByEmail(string email);
        IEnumerable<Therapist> GetAll();
        bool AddTherapist(Therapist therapist);
        (bool, Exception) UpdateProfilePicture(int id, string base64);
        (bool, Exception) UpdateAvailability(int id, DateTime? from, DateTime? to);
    }
}