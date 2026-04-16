using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class CVService : ICVService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        private readonly ICVRepository _cvRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ICVRenderService _cvRenderService;

        public CVService(ICVRepository cvRepository, ICloudinaryService cloudinaryService, ICVRenderService cvRenderService)
        {
            _cvRepository = cvRepository;
            _cloudinaryService = cloudinaryService;
            _cvRenderService = cvRenderService;
        }

        // ──────────────────────────────────────────────────────────────────
        // MY CV LIST
        // ──────────────────────────────────────────────────────────────────
        public async Task<CandidateCvViewModel> GetCandidateCvsAsync(int userId)
        {
            var candidate = await _cvRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null) return new CandidateCvViewModel();

            var cvs = await _cvRepository.GetCvsByCandidateIdAsync(candidate.Id);

            return new CandidateCvViewModel
            {
                Cvs = cvs.Select(cv => new CvListItemViewModel
                {
                    Id = cv.Id,
                    Title = string.IsNullOrWhiteSpace(cv.Title) ? "CV chưa đặt tên" : cv.Title,
                    CvType = cv.CvType,
                    CreatedAt = cv.CreatedAt,
                    UpdatedAt = cv.UpdatedAt,
                    IsDefault = cv.IsDefault,
                    FileUrl = cv.FileUrl,
                    FileName = cv.FileName,
                    FileSize = cv.FileSize,
                    TemplateName = cv.Template?.Name
                }).ToList()
            };
        }

        // ──────────────────────────────────────────────────────────────────
        // UPLOAD CV
        // ──────────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message, int CvId)> UploadCvAsync(int userId, CvUploadDTO dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return (false, "Vui lòng chọn file đính kèm.", 0);

            var allowedExts = new[] { ".pdf", ".doc", ".docx" };
            var ext = System.IO.Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            if (!allowedExts.Contains(ext))
                return (false, "Chỉ hỗ trợ file .pdf, .doc, .docx.", 0);

            const long maxSize = 10 * 1024 * 1024; // 10 MB
            if (dto.File.Length > maxSize)
                return (false, "File không được vượt quá 10MB.", 0);

            var candidate = await _cvRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null) return (false, "Không tìm thấy hồ sơ ứng viên.", 0);

            // ── PRE-VALIDATE & EXTRACT PDF CONTENT (PdfPig) ──
            string extractedText = string.Empty;
            if (ext == ".pdf")
            {
                try
                {
                    using var stream = dto.File.OpenReadStream();
                    using var document = PdfDocument.Open(stream);
                    var sb = new StringBuilder();
                    foreach (var page in document.GetPages())
                    {
                        sb.AppendLine(page.Text);
                    }
                    extractedText = sb.ToString();
                }
                catch (Exception)
                {
                    return (false, "File PDF bị lỗi định dạng hoặc có mật khẩu khóa. Vui lòng tải file khác.", 0);
                }
            }

            var fileUrl = await _cloudinaryService.UploadRawAsync(dto.File, "cv-uploads");
            if (string.IsNullOrEmpty(fileUrl))
                return (false, "Upload file thất bại. Vui lòng thử lại.", 0);

            var cv = new Cv
            {
                CandidateId = candidate.Id,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? dto.File.FileName : dto.Title,
                CvType = "UPLOAD",
                FileUrl = fileUrl,
                FileName = dto.File.FileName,
                FileSize = (int)dto.File.Length,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var created = await _cvRepository.CreateCvAsync(cv);

            var cvDataModel = BuildCvDataModelFromRawText(extractedText);
            var jsonStr = JsonSerializer.Serialize(cvDataModel, JsonOptions);
            var cvData = new CvData
            {
                CvId = created.Id,
                JsonData = jsonStr,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _cvRepository.CreateCvDataAsync(cvData);
            
            return (true, "Upload CV thành công!", created.Id);
        }

        // ──────────────────────────────────────────────────────────────────
        // BUILDER: CREATE
        // ──────────────────────────────────────────────────────────────────
        public async Task<List<CvTemplateViewModel>> GetActiveTemplatesAsync()
        {
            var templates = await _cvRepository.GetActiveTemplatesAsync();
            return templates.Select(MapTemplate).ToList();
        }

        public async Task<(bool Success, string Message, int CvId)> CreateBuilderCvAsync(int userId, int templateId, string title)
        {
            var candidate = await _cvRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null) return (false, "Không tìm thấy ứng viên.", 0);

            var template = await _cvRepository.GetTemplateByIdAsync(templateId);
            if (template == null || !template.IsActive)
                return (false, "Template không tồn tại hoặc đã bị vô hiệu hoá.", 0);

            var cv = new Cv
            {
                CandidateId = candidate.Id,
                TemplateId = templateId,
                CvType = "BUILDER",
                Title = string.IsNullOrWhiteSpace(title) ? "CV mới" : title,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _cvRepository.CreateCvAsync(cv);

            var cvData = new CvData { CvId = cv.Id, JsonData = JsonSerializer.Serialize(new CvDataModel(), JsonOptions), CreatedAt = DateTime.Now };
            await _cvRepository.CreateCvDataAsync(cvData);

            return (true, "Tạo CV thành công!", cv.Id);
        }

        // ──────────────────────────────────────────────────────────────────
        // BUILDER: EDITOR
        // ──────────────────────────────────────────────────────────────────
        public async Task<CvEditorViewModel?> GetEditorViewModelAsync(int cvId, int userId)
        {
            var candidate = await _cvRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null) return null;

            var cv = await _cvRepository.GetCvByIdAsync(cvId);
            if (cv == null || cv.CandidateId != candidate.Id || cv.CvType != "BUILDER")
                return null;

            var template = cv.Template;
            if (template == null) return null;

            var cvData = cv.CvData ?? await _cvRepository.GetCvDataByCvIdAsync(cvId);

            return new CvEditorViewModel
            {
                CvId = cv.Id,
                Title = cv.Title ?? "CV chưa đặt tên",
                TemplateId = template.Id,
                HtmlContent = template.HtmlContent,
                CssContent = template.CssContent,
                ConfigJson = template.ConfigJson,
                JsonData = cvData?.JsonData ?? JsonSerializer.Serialize(new CvDataModel(), JsonOptions)
            };
        }

        public async Task<(bool Success, string Message)> SaveCvDataAsync(int cvId, int userId, string jsonData, string title)
        {
            var candidate = await _cvRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null) return (false, "Không tìm thấy ứng viên.");

            var cv = await _cvRepository.GetCvByIdAsync(cvId);
            if (cv == null || cv.CandidateId != candidate.Id)
                return (false, "Không tìm thấy CV hoặc bạn không có quyền.");

            // Update title
            cv.Title = string.IsNullOrWhiteSpace(title) ? cv.Title : title;
            cv.UpdatedAt = DateTime.Now;
            await _cvRepository.UpdateCvAsync(cv);

            var normalized = NormalizeCvDataJson(jsonData);
            var serialized = JsonSerializer.Serialize(normalized, JsonOptions);

            // Update or create CvData
            var cvData = await _cvRepository.GetCvDataByCvIdAsync(cvId);
            if (cvData == null)
            {
                await _cvRepository.CreateCvDataAsync(new CvData { CvId = cvId, JsonData = serialized, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now });
            }
            else
            {
                cvData.JsonData = serialized;
                cvData.UpdatedAt = DateTime.Now;
                await _cvRepository.UpdateCvDataAsync(cvData);
            }

            return (true, "Lưu CV thành công!");
        }

        // ──────────────────────────────────────────────────────────────────
        // RENDER HTML (replace placeholders)
        // ──────────────────────────────────────────────────────────────────
        public async Task<string> RenderCvHtmlAsync(int cvId)
        {
            return await RenderCvHtmlAsync(cvId, string.Empty);
        }

        public async Task<string> RenderCvHtmlAsync(int cvId, string dataJsonOverride)
        {
            var cv = await _cvRepository.GetCvByIdAsync(cvId);
            if (cv?.Template == null) return "<p>Không tìm thấy template.</p>";

            var dataJson = string.IsNullOrWhiteSpace(dataJsonOverride)
                ? cv.CvData?.JsonData ?? "{}"
                : dataJsonOverride;
            var configJson = cv.Template.ConfigJson ?? "{}";

            return _cvRenderService.Render(configJson, dataJson);
        }

        // ──────────────────────────────────────────────────────────────────
        // DELETE
        // ──────────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> DeleteCvAsync(int cvId, int userId)
        {
            var candidate = await _cvRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null) return (false, "Không tìm thấy ứng viên.");

            var cv = await _cvRepository.GetCvByIdAsync(cvId);
            if (cv == null || cv.CandidateId != candidate.Id)
                return (false, "Không tìm thấy CV hoặc bạn không có quyền.");

            await _cvRepository.DeleteCvAsync(cv);
            return (true, "Đã xoá CV.");
        }

        // ──────────────────────────────────────────────────────────────────
        // ADMIN – TEMPLATES
        // ──────────────────────────────────────────────────────────────────
        public async Task<List<CvTemplateViewModel>> GetAllTemplatesAsync()
        {
            var templates = await _cvRepository.GetAllTemplatesAsync();
            return templates.Select(MapTemplate).ToList();
        }

        public async Task<CvTemplateViewModel?> GetTemplateByIdAsync(int id)
        {
            var t = await _cvRepository.GetTemplateByIdAsync(id);
            return t == null ? null : MapTemplate(t);
        }

        public async Task<(bool Success, string Message)> CreateTemplateAsync(CvTemplateCreateDTO dto)
        {
            string? thumbnailUrl = null;
            if (dto.ThumbnailFile != null)
                thumbnailUrl = await _cloudinaryService.UploadImageAsync(dto.ThumbnailFile, "cv-templates");

            var template = new CvTemplate
            {
                Name = dto.Name,
                ThumbnailUrl = thumbnailUrl,
                HtmlContent = dto.HtmlContent,
                CssContent = dto.CssContent,
                ConfigJson = dto.ConfigJson,
                CategoryId = dto.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            await _cvRepository.CreateTemplateAsync(template);
            return (true, "Tạo template thành công!");
        }

        public async Task<(bool Success, string Message)> UpdateTemplateAsync(CvTemplateEditDTO dto)
        {
            var template = await _cvRepository.GetTemplateByIdAsync(dto.Id);
            if (template == null) return (false, "Template không tồn tại.");

            if (dto.ThumbnailFile != null)
                template.ThumbnailUrl = await _cloudinaryService.UploadImageAsync(dto.ThumbnailFile, "cv-templates");
            else
                template.ThumbnailUrl = dto.ExistingThumbnailUrl;

            template.Name = dto.Name;
            template.HtmlContent = dto.HtmlContent;
            template.CssContent = dto.CssContent;
            template.ConfigJson = dto.ConfigJson;
            template.CategoryId = dto.CategoryId;
            template.IsActive = dto.IsActive;

            await _cvRepository.UpdateTemplateAsync(template);
            return (true, "Cập nhật template thành công!");
        }

        public async Task<(bool Success, string Message)> ToggleTemplateActiveAsync(int id)
        {
            var template = await _cvRepository.GetTemplateByIdAsync(id);
            if (template == null) return (false, "Template không tồn tại.");

            template.IsActive = !template.IsActive;
            await _cvRepository.UpdateTemplateAsync(template);
            return (true, template.IsActive ? "Template đã được kích hoạt." : "Template đã bị vô hiệu hoá.");
        }

        public async Task<(bool Success, string Message)> DeleteTemplateAsync(int id)
        {
            var template = await _cvRepository.GetTemplateByIdAsync(id);
            if (template == null) return (false, "Template không tồn tại.");

            await _cvRepository.DeleteTemplateAsync(template);
            return (true, "Xóa template thành công.");
        }

        // ──────────────────────────────────────────────────────────────────
        // ADMIN – TEMPLATE CATEGORIES
        // ──────────────────────────────────────────────────────────────────
        public async Task<List<TemplateCategoryViewModel>> GetAllCategoriesAsync()
        {
            var cats = await _cvRepository.GetAllCategoriesAsync();
            return cats.Select(c => new TemplateCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                TemplateCount = c.CvTemplates?.Count ?? 0,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<TemplateCategoryViewModel?> GetCategoryByIdAsync(int id)
        {
            var c = await _cvRepository.GetCategoryByIdAsync(id);
            if (c == null) return null;
            return new TemplateCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                CreatedAt = c.CreatedAt
            };
        }

        public async Task<(bool Success, string Message)> CreateCategoryAsync(TemplateCategoryFormDTO dto)
        {
            var cat = new TemplateCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                Slug = GenerateSlug(dto.Name),
                CreatedAt = DateTime.UtcNow
            };
            await _cvRepository.CreateCategoryAsync(cat);
            return (true, "Tạo danh mục thành công!");
        }

        public async Task<(bool Success, string Message)> UpdateCategoryAsync(TemplateCategoryFormDTO dto)
        {
            var cat = await _cvRepository.GetCategoryByIdAsync(dto.Id);
            if (cat == null) return (false, "Danh mục không tồn tại.");

            cat.Name = dto.Name;
            cat.Description = dto.Description;
            cat.Slug = GenerateSlug(dto.Name);

            await _cvRepository.UpdateCategoryAsync(cat);
            return (true, "Cập nhật danh mục thành công!");
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
        {
            var cat = await _cvRepository.GetCategoryByIdAsync(id);
            if (cat == null) return (false, "Danh mục không tồn tại.");

            await _cvRepository.DeleteCategoryAsync(cat);
            return (true, "Xóa danh mục thành công!");
        }

        private string GenerateSlug(string name)
        {
            return name.ToLower().Replace(" ", "-").Replace("đ", "d"); // simple stub
        }

        private static CvDataModel BuildCvDataModelFromRawText(string rawText)
        {
            var normalizedText = NormalizeText(rawText);
            var model = new CvDataModel
            {
                RawText = rawText?.Trim() ?? string.Empty,
                Email = ExtractEmail(normalizedText),
                Phone = ExtractPhone(normalizedText),
                Address = ExtractAddress(normalizedText)
            };

            var lines = normalizedText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(line => !IsNoiseLine(line))
                .ToList();

            model.FullName = ExtractLikelyName(lines);
            model.Position = ExtractLikelyPosition(lines);
            model.Summary = ExtractSectionText(normalizedText, new[] { "TÓM TẮT", "SUMMARY", "MỤC TIÊU", "GIỚI THIỆU" });
            model.Skills = ExtractSectionText(normalizedText, new[] { "KỸ NĂNG", "SKILLS" });

            return model;
        }

        private static CvDataModel NormalizeCvDataJson(string jsonData)
        {
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new CvDataModel();
            }

            CvDataModel model;
            try
            {
                model = JsonSerializer.Deserialize<CvDataModel>(jsonData, JsonOptions) ?? new CvDataModel();
            }
            catch
            {
                model = new CvDataModel();
            }

            model.CustomLayout = NormalizeLayout(model.CustomLayout);
            model.AdditionalSections = model.AdditionalSections?
                .Where(s => !string.IsNullOrWhiteSpace(s.Type) || !string.IsNullOrWhiteSpace(s.Content))
                .Select(s => new CvSectionContentModel
                {
                    Type = s.Type,
                    Content = s.Content,
                    Top = s.Top,
                    Left = s.Left,
                    Width = s.Width,
                    Height = s.Height
                })
                .ToList() ?? new List<CvSectionContentModel>();

            model.FullName = model.FullName?.Trim() ?? string.Empty;
            model.Position = model.Position?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim() ?? string.Empty;
            model.Phone = model.Phone?.Trim() ?? string.Empty;
            model.Address = model.Address?.Trim() ?? string.Empty;
            model.Summary = model.Summary?.Trim() ?? string.Empty;
            model.Skills = model.Skills?.Trim() ?? string.Empty;
            model.RawText = model.RawText?.Trim() ?? string.Empty;

            model.Experiences = model.Experiences?
                .Where(e => !string.IsNullOrWhiteSpace(e.Company) || !string.IsNullOrWhiteSpace(e.Role) || !string.IsNullOrWhiteSpace(e.Description))
                .Select(e => new ExperienceModel
                {
                    Company = e.Company?.Trim() ?? string.Empty,
                    Role = e.Role?.Trim() ?? string.Empty,
                    Period = e.Period?.Trim() ?? string.Empty,
                    Description = e.Description?.Trim() ?? string.Empty
                })
                .ToList() ?? new List<ExperienceModel>();

            model.Educations = model.Educations?
                .Where(e => !string.IsNullOrWhiteSpace(e.School) || !string.IsNullOrWhiteSpace(e.Degree))
                .Select(e => new EducationModel
                {
                    School = e.School?.Trim() ?? string.Empty,
                    Degree = e.Degree?.Trim() ?? string.Empty,
                    Period = e.Period?.Trim() ?? string.Empty
                })
                .ToList() ?? new List<EducationModel>();

            return model;
        }

        private static TemplateConfig? NormalizeLayout(TemplateConfig? layout)
        {
            if (layout == null)
            {
                return null;
            }

            if (layout.Pages != null)
            {
                foreach (var page in layout.Pages)
                {
                    foreach (var section in page.Sections)
                    {
                        section.Html = null;
                    }
                }
            }

            if (layout.Sections != null)
            {
                layout.Sections = layout.Sections
                    .Where(s => !string.IsNullOrWhiteSpace(s.Type))
                    .ToList();
            }

            return layout;
        }

        private static string NormalizeText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            normalized = Regex.Replace(normalized, @"[ \t]+", " ");
            normalized = Regex.Replace(normalized, @"\n{3,}", "\n\n");
            return normalized.Trim();
        }

        private static bool IsNoiseLine(string line)
        {
            var upper = line.ToUpperInvariant();
            return upper is "KINH NGHIỆM LÀM VIỆC" or "HỌC VẤN" or "KỸ NĂNG" or "MỤC TIÊU NGHỀ NGHIỆP" or "DANH HIỆU & GIẢI THƯỞNG" or "CHỨNG CHỈ" or "HOẠT ĐỘNG" or "NGƯỜI GIỚI THIỆU" or "SỞ THÍCH";
        }

        private static string ExtractLikelyName(List<string> lines)
        {
            var candidate = lines.FirstOrDefault(line => line.Length is > 3 and < 60 && HasManyWords(line) && !ContainsContactTokens(line));
            return candidate ?? string.Empty;
        }

        private static string ExtractLikelyPosition(List<string> lines)
        {
            var candidate = lines.FirstOrDefault(line =>
                line.Length is > 3 and < 80 &&
                !HasManyWords(line) &&
                !ContainsContactTokens(line) &&
                !IsNoiseLine(line) &&
                !Regex.IsMatch(line, @"\d"));

            return candidate ?? string.Empty;
        }

        private static bool HasManyWords(string line)
        {
            return line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 2;
        }

        private static bool ContainsContactTokens(string line)
        {
            var upper = line.ToUpperInvariant();
            return upper.Contains("@") || upper.Contains("WWW.") || upper.Contains("HTTP") || Regex.IsMatch(line, @"\+?\d[\d\s().-]{6,}");
        }

        private static string ExtractEmail(string text)
        {
            var match = Regex.Match(text, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase);
            return match.Success ? match.Value.Trim() : string.Empty;
        }

        private static string ExtractPhone(string text)
        {
            var match = Regex.Match(text, @"(?:\+?84|0)\d{8,10}");
            if (match.Success)
            {
                return match.Value.Trim();
            }

            match = Regex.Match(text, @"(?:\+?84[\s.-]?)?(?:\d[\s.-]?){9,12}");
            return match.Success ? Regex.Replace(match.Value, @"\s+", " ").Trim() : string.Empty;
        }

        private static string ExtractAddress(string text)
        {
            var markers = new[] { "ĐỊA CHỈ", "ADDRESS", "LIÊN HỆ", "CONTACT" };
            foreach (var marker in markers)
            {
                var idx = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var snippet = text[(idx + marker.Length)..].Trim();
                    var endIdx = FindFirstOf(snippet, new[] { "\n", "@", "www.", "http" });
                    if (endIdx > 0)
                    {
                        snippet = snippet[..endIdx];
                    }

                    return snippet.Trim(new[] { ' ', ':', '-', ',', ';' });
                }
            }

            return string.Empty;
        }

        private static string ExtractSectionText(string text, IReadOnlyCollection<string> markers)
        {
            foreach (var marker in markers)
            {
                var idx = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var snippet = text[(idx + marker.Length)..].Trim();
                    var nextMarkerIndex = FindFirstOf(snippet, new[] { "KINH NGHIỆM", "HỌC VẤN", "KỸ NĂNG", "DANH HIỆU", "CHỨNG CHỈ", "HOẠT ĐỘNG", "NGƯỜI GIỚI THIỆU", "SỞ THÍCH" });
                    if (nextMarkerIndex > 0)
                    {
                        snippet = snippet[..nextMarkerIndex];
                    }

                    return snippet.Trim();
                }
            }

            return string.Empty;
        }

        private static int FindFirstOf(string text, IReadOnlyCollection<string> tokens)
        {
            var indices = tokens
                .Select(token => text.IndexOf(token, StringComparison.OrdinalIgnoreCase))
                .Where(index => index >= 0)
                .ToList();

            return indices.Count == 0 ? -1 : indices.Min();
        }

        // ──────────────────────────────────────────────────────────────────
        private static CvTemplateViewModel MapTemplate(CvTemplate t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            ThumbnailUrl = t.ThumbnailUrl,
            HtmlContent = t.HtmlContent,
            CssContent = t.CssContent,
            ConfigJson = t.ConfigJson,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt
        };
    }
}
