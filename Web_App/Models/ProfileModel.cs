using Core.Domain;
using Microsoft.AspNetCore.Http;

namespace Web_App.Models
{
    public class ProfileModel
    {
        public Patient Patient { get; set; }
        public Therapist Therapist { get; set; }
        public string DisplayName { get; set; }
        public IFormFile ProfileImage { get; set; }
    }
}