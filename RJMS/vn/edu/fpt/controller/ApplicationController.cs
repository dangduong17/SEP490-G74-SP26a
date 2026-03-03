using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly IJobApplicationService _jobApplicationService;

        public ApplicationController(IJobApplicationService jobApplicationService)
        {
            _jobApplicationService = jobApplicationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid? userId)
        {
            var applications = await _jobApplicationService.GetApplicationsAsync(
                userId ?? Guid.Empty
            );
            return View("Index", applications);
        }
    }
}
