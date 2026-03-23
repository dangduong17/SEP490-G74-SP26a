using RJMS.vn.edu.fpt.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface ICVRepository
    {
        Task<Candidate?> GetCandidateByUserIdAsync(int userId);

        // CV CRUD
        Task<List<Cv>> GetCvsByCandidateIdAsync(int candidateId);
        Task<Cv?> GetCvByIdAsync(int cvId);
        Task<Cv> CreateCvAsync(Cv cv);
        Task UpdateCvAsync(Cv cv);
        Task DeleteCvAsync(Cv cv);

        // CvData (builder JSON)
        Task<CvData?> GetCvDataByCvIdAsync(int cvId);
        Task<CvData> CreateCvDataAsync(CvData data);
        Task UpdateCvDataAsync(CvData data);

        // Templates
        Task<List<CvTemplate>> GetActiveTemplatesAsync();
        Task<List<CvTemplate>> GetAllTemplatesAsync();
        Task<CvTemplate?> GetTemplateByIdAsync(int id);
        Task<CvTemplate> CreateTemplateAsync(CvTemplate template);
        Task UpdateTemplateAsync(CvTemplate template);
        Task DeleteTemplateAsync(CvTemplate template);

        // Template Categories
        Task<List<TemplateCategory>> GetAllCategoriesAsync();
        Task<TemplateCategory?> GetCategoryByIdAsync(int id);
        Task<TemplateCategory> CreateCategoryAsync(TemplateCategory category);
        Task UpdateCategoryAsync(TemplateCategory category);
        Task DeleteCategoryAsync(TemplateCategory category);
    }
}
