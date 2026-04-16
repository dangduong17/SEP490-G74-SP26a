using RJMS.vn.edu.fpt.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface ICVService
    {
        // My CV list
        Task<CandidateCvViewModel> GetCandidateCvsAsync(int userId);

        // Upload
        Task<(bool Success, string Message, int CvId)> UploadCvAsync(int userId, CvUploadDTO dto);

        // Builder
        Task<List<CvTemplateViewModel>> GetActiveTemplatesAsync();
        Task<(bool Success, string Message, int CvId)> CreateBuilderCvAsync(int userId, int templateId, string title);
        Task<CvEditorViewModel?> GetEditorViewModelAsync(int cvId, int userId);
        Task<(bool Success, string Message)> SaveCvDataAsync(int cvId, int userId, string jsonData, string title);
        Task<string> RenderCvHtmlAsync(int cvId);
        Task<string> RenderCvHtmlAsync(int cvId, string dataJsonOverride);

        // Delete
        Task<(bool Success, string Message)> DeleteCvAsync(int cvId, int userId);

        // Admin: Templates
        Task<List<CvTemplateViewModel>> GetAllTemplatesAsync();
        Task<CvTemplateViewModel?> GetTemplateByIdAsync(int id);
        Task<(bool Success, string Message)> CreateTemplateAsync(CvTemplateCreateDTO dto);
        Task<(bool Success, string Message)> UpdateTemplateAsync(CvTemplateEditDTO dto);
        Task<(bool Success, string Message)> ToggleTemplateActiveAsync(int id);
        Task<(bool Success, string Message)> DeleteTemplateAsync(int id);

        // Admin: Template Categories
        Task<List<TemplateCategoryViewModel>> GetAllCategoriesAsync();
        Task<TemplateCategoryViewModel?> GetCategoryByIdAsync(int id);
        Task<(bool Success, string Message)> CreateCategoryAsync(TemplateCategoryFormDTO dto);
        Task<(bool Success, string Message)> UpdateCategoryAsync(TemplateCategoryFormDTO dto);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id);
    }
}
