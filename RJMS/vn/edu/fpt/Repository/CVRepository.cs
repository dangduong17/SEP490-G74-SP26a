using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class CVRepository : ICVRepository
    {
        private readonly FindingJobsDbContext _context;

        public CVRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<Candidate?> GetCandidateByUserIdAsync(int userId)
            => await _context.Candidates.FirstOrDefaultAsync(c => c.UserId == userId);

        // ── CV ──────────────────────────────────────────────────────
        public async Task<List<Cv>> GetCvsByCandidateIdAsync(int candidateId)
            => await _context.Cvs
                .Include(c => c.Template)
                .Where(c => c.CandidateId == candidateId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<Cv?> GetCvByIdAsync(int cvId)
            => await _context.Cvs
                .Include(c => c.Template)
                .Include(c => c.CvData)
                .FirstOrDefaultAsync(c => c.Id == cvId);

        public async Task<Cv> CreateCvAsync(Cv cv)
        {
            _context.Cvs.Add(cv);
            await _context.SaveChangesAsync();
            return cv;
        }

        public async Task UpdateCvAsync(Cv cv)
        {
            _context.Cvs.Update(cv);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCvAsync(Cv cv)
        {
            _context.Cvs.Remove(cv);
            await _context.SaveChangesAsync();
        }

        // ── CvData ───────────────────────────────────────────────────
        public async Task<CvData?> GetCvDataByCvIdAsync(int cvId)
            => await _context.CvDataSet.FirstOrDefaultAsync(d => d.CvId == cvId);

        public async Task<CvData> CreateCvDataAsync(CvData data)
        {
            _context.CvDataSet.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task UpdateCvDataAsync(CvData data)
        {
            _context.CvDataSet.Update(data);
            await _context.SaveChangesAsync();
        }

        // ── Templates ───────────────────────────────────────────────
        public async Task<List<CvTemplate>> GetActiveTemplatesAsync()
            => await _context.CvTemplates.Include(t => t.Category).Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();

        public async Task<List<CvTemplate>> GetAllTemplatesAsync()
            => await _context.CvTemplates.Include(t => t.Category).OrderBy(t => t.Name).ToListAsync();

        public async Task<CvTemplate?> GetTemplateByIdAsync(int id)
            => await _context.CvTemplates.FindAsync(id);

        public async Task<CvTemplate> CreateTemplateAsync(CvTemplate template)
        {
            _context.CvTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task UpdateTemplateAsync(CvTemplate template)
        {
            _context.CvTemplates.Update(template);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTemplateAsync(CvTemplate template)
        {
            _context.CvTemplates.Remove(template);
            await _context.SaveChangesAsync();
        }

        // ── Template Categories ──────────────────────────────────────
        public async Task<List<TemplateCategory>> GetAllCategoriesAsync()
            => await _context.TemplateCategories.Include(c => c.CvTemplates).OrderBy(c => c.Name).ToListAsync();

        public async Task<TemplateCategory?> GetCategoryByIdAsync(int id)
            => await _context.TemplateCategories.FindAsync(id);

        public async Task<TemplateCategory> CreateCategoryAsync(TemplateCategory category)
        {
            _context.TemplateCategories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateCategoryAsync(TemplateCategory category)
        {
            _context.TemplateCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(TemplateCategory category)
        {
            _context.TemplateCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
