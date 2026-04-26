using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class JobCategoryServiceTests
    {
        private FindingJobsDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<FindingJobsDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new FindingJobsDbContext(options);
        }

        [Fact]
        [Trait("CodeModule", "Category")]
        [Trait("Method", "GetCategoriesAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task GetCategoriesAsync_ReturnsPagedData()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.JobCategories.Add(
                new JobCategory
                {
                    Id = 1,
                    Name = "IT",
                    Level = 1,
                    Slug = "it",
                }
            );
            await context.SaveChangesAsync();

            var service = new JobCategoryService(context);

            // Act
            var result = await service.GetCategoriesAsync(null, null);

            // Assert
            Assert.Single(result.Categories);
            Assert.Equal("IT", result.Categories.First().Name);
        }

        [Fact]
        [Trait("CodeModule", "Category")]
        [Trait("Method", "CreateCategoryAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "A")]
        public async Task CreateCategoryAsync_NewName_ReturnsSuccess()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new JobCategoryService(context);
            var model = new CreateJobCategoryModel { Name = "Marketing", Level = 1 };

            // Act
            var result = await service.CreateCategoryAsync(model);

            // Assert
            Assert.True(result.Succeeded);
            Assert.True(context.JobCategories.Any(c => c.Name == "Marketing"));
        }

        [Fact]
        [Trait("CodeModule", "Category")]
        [Trait("Method", "CreateCategoryAsync")]
        [Trait("UTCID", "UTCID02")]
        [Trait("Type", "B")]
        public async Task CreateCategoryAsync_DuplicateName_ReturnsFailure()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.JobCategories.Add(
                new JobCategory
                {
                    Id = 1,
                    Name = "IT",
                    Level = 1,
                    Slug = "it",
                }
            );
            await context.SaveChangesAsync();

            var service = new JobCategoryService(context);
            var model = new CreateJobCategoryModel { Name = "IT", Level = 1 };

            // Act
            var result = await service.CreateCategoryAsync(model);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Tên danh mục đã tồn tại.", result.Errors.First().Message);
        }

        [Fact]
        [Trait("CodeModule", "Category")]
        [Trait("Method", "UpdateCategoryAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task UpdateCategoryAsync_SelfParent_ReturnsFailure()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.JobCategories.Add(
                new JobCategory
                {
                    Id = 1,
                    Name = "IT",
                    Level = 1,
                    Slug = "it",
                }
            );
            await context.SaveChangesAsync();

            var service = new JobCategoryService(context);
            var model = new UpdateJobCategoryModel
            {
                Id = 1,
                Name = "IT Updated",
                ParentId = 1,
                Level = 1,
            };

            // Act
            var result = await service.UpdateCategoryAsync(model);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không thể chọn danh mục cha là chính nó.", result.Errors.First().Message);
        }

        [Fact]
        [Trait("CodeModule", "Category")]
        [Trait("Method", "DeleteCategoryAsync")]
        [Trait("UTCID", "UTCID01")]
        [Trait("Type", "B")]
        public async Task DeleteCategoryAsync_WithChildren_ReturnsFailure()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var parent = new JobCategory
            {
                Id = 1,
                Name = "IT",
                Level = 1,
                Slug = "it",
            };
            var child = new JobCategory
            {
                Id = 2,
                Name = "Dev",
                ParentId = 1,
                Level = 2,
                Slug = "dev",
            };
            context.JobCategories.AddRange(parent, child);
            await context.SaveChangesAsync();

            var service = new JobCategoryService(context);

            // Act
            var result = await service.DeleteCategoryAsync(1);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(
                "Không thể xóa danh mục đang có danh mục con.",
                result.Errors.First().Message
            );
        }
    }
}
