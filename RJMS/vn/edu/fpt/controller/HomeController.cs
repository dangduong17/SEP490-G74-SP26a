using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Model;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Controllers;

public class HomeController : Controller
{
    private readonly FindingJobsDbContext _context;

    public HomeController(FindingJobsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;

        var latestJobs = await _context.Jobs
            .AsNoTracking()
            .Include(j => j.Company)
                .ThenInclude(c => c.CompanyLocations)
                    .ThenInclude(cl => cl.Location)
            .Where(j => j.Status == "Active"
                && (!j.ExpiryDate.HasValue || j.ExpiryDate >= now)
                && (!j.PublishDate.HasValue || j.PublishDate <= now))
            .OrderByDescending(j => j.PublishDate ?? j.CreatedAt)
            .Take(6)
            .Select(j => new HomeJobCardDTO
            {
                Id = j.Id,
                Title = j.Title,
                CompanyName = j.Company.Name,
                CompanyLogo = j.Company.Logo,
                LocationName = j.Company.CompanyLocations
                    .Where(cl => cl.Location != null)
                    .OrderByDescending(cl => cl.IsPrimary)
                    .Select(cl => cl.Location!.CityName)
                    .FirstOrDefault(),
                MinSalary = j.MinSalary,
                MaxSalary = j.MaxSalary,
                JobType = j.JobType,
                CreatedAt = j.CreatedAt,
                PublishDate = j.PublishDate
            })
            .ToListAsync();

        var topCompanies = await _context.Companies
            .AsNoTracking()
            .Select(c => new HomeCompanyCardDTO
            {
                Id = c.Id,
                Name = c.Name,
                Logo = c.Logo,
                ActiveJobCount = c.Jobs.Count(j => j.Status == "Active"
                    && (!j.ExpiryDate.HasValue || j.ExpiryDate >= now)
                    && (!j.PublishDate.HasValue || j.PublishDate <= now))
            })
            .Where(c => c.ActiveJobCount > 0)
            .OrderByDescending(c => c.ActiveJobCount)
            .Take(8)
            .ToListAsync();

        var model = new HomeIndexViewModel
        {
            RecruiterCount = await _context.Recruiters.AsNoTracking().CountAsync(),
            CompanyCount = await _context.Companies.AsNoTracking().CountAsync(),
            ActiveJobCount = await _context.Jobs.AsNoTracking()
                .CountAsync(j => j.Status == "Active"
                    && (!j.ExpiryDate.HasValue || j.ExpiryDate >= now)
                    && (!j.PublishDate.HasValue || j.PublishDate <= now)),
            ApplicationCount = await _context.Applications.AsNoTracking().CountAsync(),
            LatestJobs = latestJobs,
            TopCompanies = topCompanies
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
