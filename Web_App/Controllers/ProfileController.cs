using Core.Domain;
using Core.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Web_App.Models;
using Web_App.Session;

namespace Web_App.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITherapistRepository _therapistRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProfileController(ILogger<LoginController> logger, UserManager<IdentityUser> userManager, ITherapistRepository therapistRepository, IPatientRepository patientRepository, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _userManager = userManager;
            _therapistRepository = therapistRepository;
            _patientRepository = patientRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public async Task<IActionResult> Index()
        {
            ProfileModel profileModel = new ProfileModel();
            profileModel.Patient = new Patient();
            profileModel.Therapist = new Therapist();

            ApplicationUser applicationUser = (await _userManager.GetUserAsync(this.User)) as ApplicationUser;
            if (HttpContext.Session.GetBoolean("IsAdmin") || HttpContext.Session.GetBoolean("IsTherapist") || HttpContext.Session.GetBoolean("IsStudent"))
            {             
                Therapist therapistResult = _therapistRepository.Get(applicationUser.DataId);
                if (therapistResult != null)
                {
                    profileModel.Therapist = therapistResult;
                    profileModel.DisplayName = therapistResult.Name;
                }
                else
                {
                    InfoModel infoModel = new InfoModel();
                    infoModel.Title = "Retrieve Profile - Error";
                    infoModel.Description = $"An error occurred while trying to retrieve the profile of <b>{this.User.Identity.Name}</b>";
                    infoModel.Description += "\n, please contact the server admins to resolve this issue.";

                    infoModel.TableEntries.Add("Therapist", "null");

                    infoModel.ReturnAction = "Index";
                    infoModel.ReturnController = "Profile";

                    HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                    return RedirectToAction("Index", "Info", infoModel);
                }
            } 
            else
            {
                if (HttpContext.Session.GetBoolean("IsPatient"))
                {
                    Patient patientResult = _patientRepository.Get(applicationUser.DataId);
                    if (patientResult != null)
                    {
                        profileModel.Patient = patientResult;
                        profileModel.DisplayName = patientResult.Name;
                    }
                    else
                    {
                        InfoModel infoModel = new InfoModel();
                        infoModel.Title = "Retrieve Profile - Error";
                        infoModel.Description = $"An error occurred while trying to retrieve the profile of <b>{this.User.Identity.Name}</b>";
                        infoModel.Description += "\n, please contact the server admins to resolve this issue.";

                        infoModel.TableEntries.Add("Patient", "null");

                        infoModel.ReturnAction = "Index";
                        infoModel.ReturnController = "Profile";

                        HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
                        return RedirectToAction("Index", "Info", infoModel);
                    }
                }
            }
            return View(profileModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile file)
        {
            InfoModel infoModel = new InfoModel();
            infoModel.Title = "Update Profile Picture - Error";
            infoModel.Description = $"An error occurred while trying update the profile picture of <b>{this.User.Identity.Name}</b>";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Profile";

            string base64 = UploadedFileToBase64(file);
            if (!string.IsNullOrEmpty(base64))
            {
                ApplicationUser applicationUser = (await _userManager.GetUserAsync(this.User)) as ApplicationUser;
                if (HttpContext.Session.GetBoolean("IsAdmin") || HttpContext.Session.GetBoolean("IsTherapist") || HttpContext.Session.GetBoolean("IsStudent"))
                {
                    Therapist therapistResult = _therapistRepository.Get(applicationUser.DataId);
                    if (therapistResult != null)
                    {
                        (bool, Exception) updateProfilePictureResult = _therapistRepository.UpdateProfilePicture(therapistResult.Id, base64);
                        if (updateProfilePictureResult.Item1)
                        {
                            HttpContext.Session.SetObject("ProfilePictureBase64", base64);

                            infoModel.Title = "Update Profile Picture - Success";
                            infoModel.Description = $"Successfully updated your <b>profile picture</b>.";

                            infoModel.TableEntries.Add("File Name", file.FileName);
                        }
                        else
                            infoModel.TableEntries.Add(updateProfilePictureResult.Item2.GetType().Name, updateProfilePictureResult.Item2.Message);
                    }
                    else
                        infoModel.TableEntries.Add("Therapist", "null");
                }
                else
                {
                    if (HttpContext.Session.GetBoolean("IsPatient"))
                    {
                        Patient patientResult = _patientRepository.Get(applicationUser.DataId);
                        if (patientResult != null)
                        {
                            (bool, Exception) updateProfilePictureResult = _patientRepository.UpdateProfilePicture(patientResult.Id, base64);
                            if (updateProfilePictureResult.Item1)
                            {
                                HttpContext.Session.SetObject("ProfilePictureBase64", base64);

                                infoModel.Title = "Update Profile Picture - Success";
                                infoModel.Description = $"Successfully updated your <b>profile picture</b>.";

                                infoModel.TableEntries.Add("File Name", file.FileName);
                            }
                            else
                                infoModel.TableEntries.Add(updateProfilePictureResult.Item2.GetType().Name, updateProfilePictureResult.Item2.Message);
                        }
                        else
                            infoModel.TableEntries.Add("Patient", "null");
                    }
                }
            }
            else
                return RedirectToAction("Index", "Profile");

            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }

        private string UploadedFileToBase64(IFormFile file)
        {
            if (file != null)
            {
                byte[] bytes;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
                return Convert.ToBase64String(bytes);
            }
            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Therapist,Student,Patient")]
        public async Task<IActionResult> UpdateAvailability(DateTime? from, DateTime? to)
        {
            string fromValue = from.HasValue ? from.Value.ToShortDateString() : "Null";
            string toValue = to.HasValue ? to.Value.ToShortDateString() : "Null";

            InfoModel infoModel = new InfoModel();
            infoModel.Title = "Update Availability - Error";
            infoModel.Description = $"An error occurred while trying to update your <b>availability</b> to <b>from: {fromValue}</b>, <b>to: {toValue}</b>";
            infoModel.Description += "\n, please contact the server admins to resolve this issue.";
            infoModel.ReturnAction = "Index";
            infoModel.ReturnController = "Profile";

            ApplicationUser applicationUser = (await _userManager.GetUserAsync(this.User)) as ApplicationUser;
            Therapist therapistResult = _therapistRepository.Get(applicationUser.DataId);
            if (therapistResult != null)
            {
                (bool, Exception) availabilityResult = _therapistRepository.UpdateAvailability(therapistResult.Id, from, to);
                if (availabilityResult.Item1)
                {
                    infoModel.Title = "Update Availability - Success";
                    infoModel.Description = $"Successfully updated your <b>availability</b>.";

                    infoModel.TableEntries.Add("Available From: ", fromValue);
                    infoModel.TableEntries.Add("Available To: ", toValue);
                }
                else
                    infoModel.TableEntries.Add(availabilityResult.Item2.GetType().Name, availabilityResult.Item2.Message);
            }
            else
                infoModel.TableEntries.Add("Therapist", "null");

            HttpContext.Session.SetObject("InfoEntries", infoModel.TableEntries);
            return RedirectToAction("Index", "Info", infoModel);
        }
    }
}