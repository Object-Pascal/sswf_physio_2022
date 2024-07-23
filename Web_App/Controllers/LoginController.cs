using Core.Domain;
using Core.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Web_App.Models;
using Web_App.Session;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Web_App.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITherapistRepository _therapistRepository;
        private readonly IPatientRepository _patientRepository;

        public LoginController(ILogger<LoginController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ITherapistRepository therapistRepository, IPatientRepository patientRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _therapistRepository = therapistRepository;
            _patientRepository = patientRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (SessionState.IsUserConsent(HttpContext))
            {
                if (HttpContext.Session.GetBoolean("LoggedIn"))
                {
                    if (HttpContext.Session.GetBoolean("IsAdmin") || HttpContext.Session.GetBoolean("IsTherapist") || HttpContext.Session.GetBoolean("IsStudent"))
                        return RedirectToAction("Index", "Patient");
                    else if (HttpContext.Session.GetBoolean("IsPatient"))
                        return RedirectToAction("Index", "Profile");
                }
                return View();
            }
            return RedirectToAction("Privacy", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Index(LoginModel data)
        {
            if (SessionState.IsUserConsent(HttpContext))
            {
                if (ModelState.IsValid && !HttpContext.Session.GetBoolean("LoggedIn"))
                {
                    ApplicationUser identityUser = (await _userManager.FindByNameAsync(data.UserName)) as ApplicationUser;
                    if (identityUser != null)
                    {
                        SignInResult signInResult = await _signInManager.PasswordSignInAsync(identityUser, data.Password, false, false);
                        if (signInResult.Succeeded)
                        {
                            HttpContext.Session.SetObject("CurrentUser", identityUser);

                            bool isAdministrator = await _userManager.IsInRoleAsync(identityUser, Role.ADMINISTRATOR_ROLE);
                            bool isTherapist = await _userManager.IsInRoleAsync(identityUser, Role.THERAPIST_ROLE);
                            bool isStudent = await _userManager.IsInRoleAsync(identityUser, Role.STUDENTTHERAPIST_ROLE);
                            bool isPatient = await _userManager.IsInRoleAsync(identityUser, Role.PATIENT_ROLE);

                            HttpContext.Session.SetBoolean("IsAdmin", isAdministrator);
                            HttpContext.Session.SetBoolean("IsTherapist", isTherapist);
                            HttpContext.Session.SetBoolean("IsStudent", isStudent);
                            HttpContext.Session.SetBoolean("IsPatient", isPatient);

                            HttpContext.Session.SetBoolean("LoggedIn", true);
                            HttpContext.Session.SetObject("ProfilePictureBase64", "");

                            if (isAdministrator || isTherapist || isStudent)
                            {
                                Therapist therapistResult = _therapistRepository.Get(identityUser.DataId);
                                if (therapistResult != null)
                                {
                                    HttpContext.Session.SetObject("UserDataId", therapistResult.Id);
                                    HttpContext.Session.SetObject("ProfilePictureBase64", therapistResult.ProfilePictureBase64);
                                }

                                using (HttpClient http = new HttpClient())
                                {
                                    string url = $"https://sswf-api-pascal.azurewebsites.net";
                                    //string url = $"https://sswf-phrstoop-api.azurewebsites.net";
                                    //string url = $"https://localhost:44390";
                                    HttpContext.Session.SetObject("WebAPIHost", url);

                                    HttpResponseMessage webApiSignInResponse = await http.PostAsJsonAsync($"{HttpContext.Session.GetObject<string>("WebAPIHost")}/api/v1/auth/signin", new SignInRequest()
                                    {
                                        UserName = data.UserName,
                                        Password = data.Password
                                    });

                                    if (webApiSignInResponse.IsSuccessStatusCode)
                                    {
                                        string responseJson = await webApiSignInResponse.Content.ReadAsStringAsync();
                                        SignInResponse responseBody = JsonSerializer.Deserialize<SignInResponse>(responseJson);

                                        HttpContext.Session.SetBoolean("WebApiLoggedIn", true);
                                        HttpContext.Session.SetObject("WebApiToken", responseBody.token);
                                    }
                                    else
                                    {
                                        HttpContext.Session.SetBoolean("WebApiLoggedIn", false);
                                        HttpContext.Session.SetObject("WebApiToken", string.Empty);
                                    }
                                }
                                return RedirectToAction("Index", "Patient");
                            }
                            else if (isPatient)
                            {
                                Patient patientResult = _patientRepository.Get(identityUser.DataId);
                                if (patientResult != null)
                                {
                                    HttpContext.Session.SetObject("UserDataId", patientResult.Id);
                                    HttpContext.Session.SetObject("PatientNumber", patientResult.PatientNumber);
                                    HttpContext.Session.SetObject("ProfilePictureBase64", patientResult.ProfilePictureBase64);
                                }
                                return RedirectToAction("View", "Patient", new { pn = patientResult.PatientNumber, tabTo = "ed-tab" });
                            }
                            return RedirectToAction("Index");
                        }
                    }
                }
                return View(data);
            }
            return View(data);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            if (SessionState.IsUserConsent(HttpContext))
            {
                if (HttpContext.Session.GetBoolean("LoggedIn"))
                {
                    _signInManager.SignOutAsync().Wait();

                    HttpContext.Session.SetBoolean("LoggedIn", false);
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}