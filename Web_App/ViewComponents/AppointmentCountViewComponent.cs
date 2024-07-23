using Core.Domain;
using Core.DomainServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_App.Session;

namespace Web_App.ViewComponents
{
    [ViewComponent(Name = "AppointmentCount")]
    public class AppointmentCountViewComponent : ViewComponent
    {
        private readonly IAppointmentRepository _appointmentRepository;
        public AppointmentCountViewComponent(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;
            if (HttpContext.Session.Keys.Contains("UserDataId"))
            {
                int currId = HttpContext.Session.GetObject<int>("UserDataId");
                IEnumerable<Appointment> appointments = _appointmentRepository.GetByTherapistId(currId);
                IEnumerable<Appointment> todaysAppointments = appointments.Select(x =>
                {
                    if (x.AppointmentDateTime.HasValue)
                        if (x.AppointmentDateTime.Value.Year == DateTime.Now.Year && x.AppointmentDateTime.Value.DayOfYear == DateTime.Now.DayOfYear)
                            return x;

                    return null;
                }).Where(x => x != null);
                count = await Task.Run(() => todaysAppointments.Count());
            }
            return View(count);
        }
    }
}