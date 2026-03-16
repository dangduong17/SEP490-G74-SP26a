using RJMS.vn.edu.fpt.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IJobCategoryService
    {
        Task<JobCategoryListViewModel> GetCategoriesAsync(string? keyword, int? level, int page = 1, int pageSize = 10);
        Task<List<JobCategoryViewModel>> GetPossibleParentsAsync(int forLevel);
        Task<ServiceResult> CreateCategoryAsync(CreateJobCategoryModel model);
        Task<JobCategoryViewModel?> GetCategoryByIdAsync(int id);
        Task<ServiceResult> UpdateCategoryAsync(UpdateJobCategoryModel model);
        Task<ServiceResult> DeleteCategoryAsync(int id);
    }
}
