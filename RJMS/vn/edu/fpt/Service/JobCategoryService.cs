using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class JobCategoryService : IJobCategoryService
    {
        private readonly FindingJobsDbContext _context;

        public JobCategoryService(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<JobCategoryListViewModel> GetCategoriesAsync(string? keyword, int? level, int page = 1, int pageSize = 10)
        {
            var query = _context.JobCategories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.Name.Contains(keyword) || (c.Description != null && c.Description.Contains(keyword)));
            }

            if (level.HasValue && level.Value > 0)
            {
                query = query.Where(c => c.Level == level.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderBy(c => c.Level).ThenBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new JobCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ParentId = c.ParentId,
                    Level = c.Level,
                    Slug = c.Slug,
                    CreatedAt = c.CreatedAt,
                    ParentName = c.Parent != null ? c.Parent.Name : null,
                    ChildrenCount = c.Children.Count
                })
                .ToListAsync();

            return new JobCategoryListViewModel
            {
                Categories = items,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                Keyword = keyword,
                FilterLevel = level
            };
        }

        public async Task<List<JobCategoryViewModel>> GetPossibleParentsAsync(int forLevel)
        {
            if (forLevel <= 1) return new List<JobCategoryViewModel>();

            var targetParentLevel = forLevel - 1;
            return await _context.JobCategories
                .Where(c => c.Level == targetParentLevel)
                .Select(c => new JobCategoryViewModel { Id = c.Id, Name = c.Name })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ServiceResult> CreateCategoryAsync(CreateJobCategoryModel model)
        {
            var exists = await _context.JobCategories.AnyAsync(c => c.Name == model.Name);
            if (exists) return ServiceResult.Failed(new ServiceError { Message = "Tên danh mục đã tồn tại." });

            var entity = new JobCategory
            {
                Name = model.Name,
                Description = model.Description,
                ParentId = model.ParentId,
                Level = model.Level,
                Slug = model.Slug ?? CreateSlug(model.Name),
                CreatedAt = System.DateTime.Now
            };

            _context.JobCategories.Add(entity);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<JobCategoryViewModel?> GetCategoryByIdAsync(int id)
        {
            var entity = await _context.JobCategories.FindAsync(id);
            if (entity == null) return null;

            return new JobCategoryViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ParentId = entity.ParentId,
                Level = entity.Level,
                Slug = entity.Slug
            };
        }

        public async Task<ServiceResult> UpdateCategoryAsync(UpdateJobCategoryModel model)
        {
            var entity = await _context.JobCategories.FindAsync(model.Id);
            if (entity == null) return ServiceResult.Failed(new ServiceError { Message = "Không tìm thấy danh mục." });

            var exists = await _context.JobCategories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id);
            if (exists) return ServiceResult.Failed(new ServiceError { Message = "Tên danh mục đã tồn tại." });

            // Prevent circular logic simply by preventing a category modifying to its own child
            if (model.ParentId.HasValue && model.ParentId.Value == model.Id)
            {
                return ServiceResult.Failed(new ServiceError { Message = "Không thể chọn danh mục cha là chính nó." });
            }

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.ParentId = model.ParentId;
            entity.Level = model.Level;
            entity.Slug = model.Slug ?? CreateSlug(model.Name);

            _context.JobCategories.Update(entity);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteCategoryAsync(int id)
        {
            var entity = await _context.JobCategories.Include(c => c.Children).Include(c => c.Jobs).FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return ServiceResult.Failed(new ServiceError { Message = "Không tìm thấy danh mục." });

            if (entity.Children.Any())
                return ServiceResult.Failed(new ServiceError { Message = "Không thể xóa danh mục đang có danh mục con." });

            if (entity.Jobs.Any())
                return ServiceResult.Failed(new ServiceError { Message = "Không thể xóa danh mục đang có công việc liên kết." });

            _context.JobCategories.Remove(entity);
            await _context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        private string CreateSlug(string name)
        {
            return name.ToLower().Replace(" ", "-").Replace("đ", "d");
            // Real slug generation is more complex, but this is a stub.
        }
    }
}
