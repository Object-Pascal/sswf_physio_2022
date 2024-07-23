using Core.Domain;
using Core.DomainServices;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class TherapistRepository : ITherapistRepository
    {
        private SwfContext _context;
        public TherapistRepository(SwfContext context)
        {
            _context = context;
        }

        public Therapist Get(int id)
        {
            return _context.Therapists.FirstOrDefault(x => x.Id == id);
        }

        public (Therapist, Exception) GetByName(string name)
        {
            try
            {
                return (_context.Therapists.First(x => x.Name == name), null);
            }
            catch (Exception e)
            {
                return (null, e);
            }
        }

        public (Therapist, Exception) GetByEmail(string email)
        {
            try
            {
                return (_context.Therapists.First(x => x.EmailAddress == email), null);
            }
            catch (Exception e)
            {
                return (null, e);
            }
        }

        public bool AddTherapist(Therapist therapist)
        {
            EntityEntry<Therapist> addedEntry = _context.Therapists.Add(therapist);
            return addedEntry == null ? false : true;
        }

        public IEnumerable<Therapist> GetAll()
        {
            return _context.Therapists.ToList();
        }

        public (bool, Exception) UpdateAvailability(int id, DateTime? from, DateTime? to)
        {
            try
            {
                Therapist therapist = _context.Therapists.First(x => x.Id == id);
                therapist.AvailableFrom = from;
                therapist.AvailableTo = to;

                _context.Therapists.Update(therapist);
                _context.SaveChanges();

                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public (bool, Exception) UpdateProfilePicture(int id, string base64)
        {
            try
            {
                Therapist therapist = _context.Therapists.First(x => x.Id == id);
                therapist.ProfilePictureBase64 = base64;

                _context.Therapists.Update(therapist);
                _context.SaveChanges();
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }
    }
}