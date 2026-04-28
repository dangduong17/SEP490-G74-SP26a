using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RJMS.Vn.Edu.Fpt.Model;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers;

public class HomeController : Controller
{
    private readonly FindingJobsDbContext _context;
    private readonly IWebSliderService _sliderService;

    public HomeController(FindingJobsDbContext context, IWebSliderService sliderService)
    {
        _context = context;
        _sliderService = sliderService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;

        var activeJobQuery = _context.Jobs
            .AsNoTracking()
            .Where(j => j.Status == "Active"
                && !j.IsBanned
                && (!j.ExpiryDate.HasValue || j.ExpiryDate >= now)
                && (!j.PublishDate.HasValue || j.PublishDate <= now));

        var latestJobs = await _context.Jobs
            .AsNoTracking()
            .Include(j => j.Company)
                .ThenInclude(c => c.CompanyLocations)
                    .ThenInclude(cl => cl.Location)
            .Where(j => j.Status == "Active"
                && !j.IsBanned
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
                    && !j.IsBanned
                    && (!j.ExpiryDate.HasValue || j.ExpiryDate >= now)
                    && (!j.PublishDate.HasValue || j.PublishDate <= now))
            })
            .Where(c => c.ActiveJobCount > 0)
            .OrderByDescending(c => c.ActiveJobCount)
            .Take(8)
            .ToListAsync();

        var sliders = await _sliderService.GetActiveForDisplayAsync();

        var allCategories = await _context.JobCategories
            .AsNoTracking()
            .OrderBy(c => c.Level).ThenBy(c => c.Name)
            .ToListAsync();

        var jobCountMap = await activeJobQuery
            .Where(j => j.JobCategoryId != null)
            .GroupBy(j => j.JobCategoryId!.Value)
            .Select(g => new { CatId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CatId, x => x.Count);

        var flatCategories = allCategories
            .Select(c => new JobFilterCategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,
                Level = c.Level,
                JobCount = jobCountMap.GetValueOrDefault(c.Id, 0)
            })
            .ToList();

        var categoryGroups = BuildCategoryGroups(flatCategories);

        var locations = await _context.JobRecruiters
            .AsNoTracking()
            .Include(jr => jr.CompanyLocation)
            .ThenInclude(cl => cl.Location)
            .Where(jr => jr.Job.Status == "Active"
                && !jr.Job.IsBanned
                && (!jr.Job.ExpiryDate.HasValue || jr.Job.ExpiryDate >= now)
                && (!jr.Job.PublishDate.HasValue || jr.Job.PublishDate <= now))
            .GroupBy(jr => jr.CompanyLocation.Location.Id)
            .Select(g => new JobFilterLocationDTO
            {
                Id = g.Key,
                Name = g.First().CompanyLocation.Location.CityName,
                JobCount = g.Select(jr => jr.JobId).Distinct().Count()
            })
            .OrderBy(l => l.Name)
            .ToListAsync();

        var featuredCategories = flatCategories
            .Where(c => c.JobCount > 0 && c.Level <= 2)
            .OrderByDescending(c => c.JobCount)
            .ThenBy(c => c.Name)
            .Take(8)
            .ToList();

        if (featuredCategories.Count == 0)
        {
            featuredCategories = flatCategories
                .Where(c => c.JobCount > 0)
                .OrderByDescending(c => c.JobCount)
                .ThenBy(c => c.Name)
                .Take(8)
                .ToList();
        }

        var model = new HomeIndexViewModel
        {
            RecruiterCount = await _context.Recruiters.AsNoTracking().CountAsync(),
            CompanyCount = await _context.Companies.AsNoTracking().CountAsync(),
            ActiveJobCount = await activeJobQuery.CountAsync(),
            ApplicationCount = await _context.Applications.AsNoTracking().CountAsync(),
            LatestJobs = latestJobs,
            TopCompanies = topCompanies,
            Sliders = sliders,
            Categories = flatCategories,
            CategoryGroups = categoryGroups,
            Locations = locations,
            FeaturedCategories = featuredCategories,
            SuggestedKeywords = featuredCategories.Take(5).Select(c => c.Name).ToList()
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

    private static List<JobFilterCategoryDTO> BuildCategoryGroups(List<JobFilterCategoryDTO> flat)
    {
        var lookup = flat.ToDictionary(c => c.Id, c => new JobFilterCategoryDTO
        {
            Id = c.Id,
            Name = c.Name,
            ParentId = c.ParentId,
            Level = c.Level,
            JobCount = c.JobCount
        });

        var roots = new List<JobFilterCategoryDTO>();

        foreach (var cat in flat)
        {
            if (cat.ParentId == null || !lookup.ContainsKey(cat.ParentId.Value))
                roots.Add(lookup[cat.Id]);
            else
                lookup[cat.ParentId.Value].Children.Add(lookup[cat.Id]);
        }

        void BubbleUp(JobFilterCategoryDTO node)
        {
            foreach (var child in node.Children)
            {
                BubbleUp(child);
            }
            if (node.Children.Any())
            {
                node.JobCount += node.Children.Sum(c => c.JobCount);
            }
        }

        foreach (var r in roots)
        {
            BubbleUp(r);
        }

        void Prune(JobFilterCategoryDTO node)
        {
            node.Children = node.Children.Where(c => c.JobCount > 0).ToList();
            foreach (var child in node.Children)
            {
                Prune(child);
            }
        }

        foreach (var r in roots)
        {
            Prune(r);
        }

        return roots.Where(r => r.JobCount > 0).ToList();
    }
}
