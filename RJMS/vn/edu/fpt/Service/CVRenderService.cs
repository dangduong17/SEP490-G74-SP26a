using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class CVRenderService : ICVRenderService
    {
        private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

        // ─────────────────────────────────────────────────────────────────────
        // PUBLIC ENTRY POINT
        // ─────────────────────────────────────────────────────────────────────
        public string Render(string templateJson, string dataJson)
        {
            CvDataModel data;
            try
            {
                data = string.IsNullOrWhiteSpace(dataJson)
                    ? new CvDataModel()
                    : JsonSerializer.Deserialize<CvDataModel>(dataJson, _opts) ?? new CvDataModel();
            }
            catch
            {
                data = new CvDataModel();
            }

            TemplateConfig template;
            try
            {
                template = string.IsNullOrWhiteSpace(templateJson)
                    ? BuildDefaultTemplate()
                    : JsonSerializer.Deserialize<TemplateConfig>(templateJson, _opts) ?? BuildDefaultTemplate();
            }
            catch
            {
                template = BuildDefaultTemplate();
            }

            // Support candidate-specific layout overrides directly stored in their CV JsonData
            if (data.CustomLayout != null && data.CustomLayout.Pages.Any())
            {
                template = data.CustomLayout;
            }

            // ── Normalise: if new Pages structure is present, use it; else fall back to legacy Sections list
            if (template.Pages == null || template.Pages.Count == 0)
            {
                template.Pages = LegacyToPages(template);
            }

            var sb = new StringBuilder();
            sb.Append(FontImport(template.Theme));

            foreach (var page in template.Pages)
            {
                sb.Append(RenderPage(page, data, template.Theme));
            }

            return WrapDocument(sb.ToString(), template.Theme);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PAGE RENDERER  (handles 1 or 2 columns)
        // ─────────────────────────────────────────────────────────────────────
        private string RenderPage(PageConfig page, CvDataModel data, ThemeConfig theme)
        {
            var cols = Math.Max(0, Math.Min(2, page.Columns));

            var pageHtml = new StringBuilder();
            pageHtml.Append("<div class=\"cv-page\" style=\"position:relative;\">");

            if (cols == 0)
            {
                // Free-form layout
                foreach (var sec in page.Sections)
                {
                    var style = "position:absolute;";
                    if (sec.Top.HasValue) style += $"top:{sec.Top.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;";
                    if (sec.Left.HasValue) style += $"left:{sec.Left.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;";
                    if (sec.Width.HasValue) style += $"width:{sec.Width.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;";
                    if (sec.Height.HasValue) style += $"height:{sec.Height.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;";

                    pageHtml.Append($"<div style=\"{style} box-sizing:border-box; overflow:hidden;\">");
                    
                    if (!string.IsNullOrWhiteSpace(sec.Html))
                    {
                        var content = sec.Html.Replace("contenteditable=\"true\"", "");
                        pageHtml.Append(content);
                    }
                    else
                    {
                        pageHtml.Append(RenderSection(sec.Type, data, theme));
                    }
                    
                    pageHtml.Append("</div>");
                }
            }
            else if (cols == 1)
            {
                // All sections in one column, ordered by Order
                foreach (var sec in page.Sections.OrderBy(s => s.Order))
                    pageHtml.Append(RenderSection(sec.Type, data, theme));
            }
            else
            {
                // Two-column layout
                var col1 = page.Sections.Where(s => s.Column == 1).OrderBy(s => s.Order).ToList();
                var col2 = page.Sections.Where(s => s.Column == 2).OrderBy(s => s.Order).ToList();

                pageHtml.Append("<div class=\"cv-two-col\">");
                pageHtml.Append("<div class=\"cv-col cv-col-1\">");
                foreach (var sec in col1) pageHtml.Append(RenderSection(sec.Type, data, theme));
                pageHtml.Append("</div>");
                pageHtml.Append("<div class=\"cv-col cv-col-2\">");
                foreach (var sec in col2) pageHtml.Append(RenderSection(sec.Type, data, theme));
                pageHtml.Append("</div>");
                pageHtml.Append("</div>");
            }

            pageHtml.Append("</div>");
            return pageHtml.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        // SECTION DISPATCHER
        // ─────────────────────────────────────────────────────────────────────
        private string RenderSection(string type, CvDataModel data, ThemeConfig theme)
            => type.ToLower() switch
            {
                "header"     => RenderHeader(data, theme),
                "summary"    => RenderSummary(data, theme),
                "experience" => RenderExperience(data, theme),
                "education"  => RenderEducation(data, theme),
                "skills"     => RenderSkills(data, theme),
                "projects"   => RenderProjects(data, theme),
                _            => ""
            };

        // ─────────────────────────────────────────────────────────────────────
        // SECTION RENDERERS
        // ─────────────────────────────────────────────────────────────────────
        private string RenderHeader(CvDataModel data, ThemeConfig theme) => $@"
<div class='cv-header' style='background:{theme.PrimaryColor};color:#fff;padding:36px 40px;'>
  <h1 style='margin:0 0 6px;font-size:2rem;font-weight:700;letter-spacing:0.5px;'>{H(data.FullName)}</h1>
  <h3 style='margin:0 0 18px;font-weight:400;opacity:.88;font-size:1.05rem;'>{H(data.Position)}</h3>
  <div style='display:flex;flex-wrap:wrap;gap:16px;font-size:.88rem;'>
    {ContactItem(data.Email, "Email")}
    {ContactItem(data.Phone, "Tel")}
    {ContactItem(data.Address, "Địa chỉ")}
  </div>
</div>";

        private static string ContactItem(string val, string label)
            => string.IsNullOrWhiteSpace(val) ? "" : $"<span><strong>{label}:</strong> {H(val)}</span>";

        private string RenderSummary(CvDataModel data, ThemeConfig theme)
        {
            if (string.IsNullOrWhiteSpace(data.Summary)) return "";
            return $@"
<div class='cv-section'>
  <h3 class='cv-section-title' style='color:{theme.PrimaryColor};border-bottom:2px solid {theme.PrimaryColor};'>Giới thiệu</h3>
  <p style='line-height:1.7;color:#444;white-space:pre-line;'>{H(data.Summary)}</p>
</div>";
        }

        private string RenderExperience(CvDataModel data, ThemeConfig theme)
        {
            if (data.Experiences == null || data.Experiences.Count == 0) return "";
            var sb = new StringBuilder();
            sb.Append($"<div class='cv-section'><h3 class='cv-section-title' style='color:{theme.PrimaryColor};border-bottom:2px solid {theme.PrimaryColor};'>Kinh nghiệm làm việc</h3>");
            foreach (var e in data.Experiences)
            {
                sb.Append($@"
<div style='margin-bottom:22px;'>
  <div style='display:flex;justify-content:space-between;align-items:baseline;'>
    <strong style='font-size:1rem;'>{H(e.Company)}</strong>
    <span style='font-size:.82rem;color:#888;background:#f3f4f6;padding:2px 8px;border-radius:10px;'>{H(e.Period)}</span>
  </div>
  <div style='color:{theme.PrimaryColor};font-weight:600;margin:3px 0 6px;font-size:.92rem;'>{H(e.Role)}</div>
  <p style='margin:0;line-height:1.65;color:#555;font-size:.9rem;'>{H(e.Description)}</p>
</div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private string RenderEducation(CvDataModel data, ThemeConfig theme)
        {
            if (data.Educations == null || data.Educations.Count == 0) return "";
            var sb = new StringBuilder();
            sb.Append($"<div class='cv-section'><h3 class='cv-section-title' style='color:{theme.PrimaryColor};border-bottom:2px solid {theme.PrimaryColor};'>Học vấn</h3>");
            foreach (var edu in data.Educations)
            {
                sb.Append($@"
<div style='margin-bottom:16px;'>
  <div style='display:flex;justify-content:space-between;align-items:baseline;'>
    <strong>{H(edu.School)}</strong>
    <span style='font-size:.82rem;color:#888;'>{H(edu.Period)}</span>
  </div>
  <div style='color:#555;font-size:.9rem;margin-top:3px;'>{H(edu.Degree)}</div>
</div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private string RenderSkills(CvDataModel data, ThemeConfig theme)
        {
            if (string.IsNullOrWhiteSpace(data.Skills)) return "";
            var items = data.Skills.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            sb.Append($"<div class='cv-section'><h3 class='cv-section-title' style='color:{theme.PrimaryColor};border-bottom:2px solid {theme.PrimaryColor};'>Kỹ năng</h3><div style='display:flex;flex-wrap:wrap;gap:8px;'>");
            foreach (var s in items)
                sb.Append($"<span style='background:{theme.PrimaryColor}18;color:{theme.PrimaryColor};padding:4px 12px;border-radius:14px;font-size:.85rem;font-weight:500;border:1px solid {theme.PrimaryColor}30;'>{H(s.Trim())}</span>");
            sb.Append("</div></div>");
            return sb.ToString();
        }

        private string RenderProjects(CvDataModel data, ThemeConfig theme)
        {
            // Projects stored as text lines in data (placeholder – extend CvDataModel if needed)
            return "";
        }

        // ─────────────────────────────────────────────────────────────────────
        // WRAPPING & HELPERS
        // ─────────────────────────────────────────────────────────────────────
        private static string FontImport(ThemeConfig theme)
        {
            var safe = Uri.EscapeDataString(theme.Font ?? "Inter").Replace("%20", "+");
            return $"<link href='https://fonts.googleapis.com/css2?family={safe}:wght@400;500;600;700&display=swap' rel='stylesheet'>";
        }

        private static string WrapDocument(string body, ThemeConfig theme)
        {
            var font = theme.Font ?? "Inter";
            return $@"
<style>
  .cv-document {{ font-family:'{font}',sans-serif; background:#fff; box-shadow:0 4px 30px rgba(0,0,0,.12); max-width:794px; margin:0 auto; color:#222; }}
  .cv-page {{ width:100%; page-break-after:always; }}
  .cv-two-col {{ display:grid; grid-template-columns:1fr 1fr; }}
  .cv-col {{ padding:0; }}
  .cv-section {{ padding:20px 28px; }}
  .cv-section-title {{ font-size:1rem; font-weight:700; text-transform:uppercase; letter-spacing:.05em; padding-bottom:6px; margin-bottom:14px; }}
</style>
<div class='cv-document'>{body}</div>";
        }

        // ─────────────────────────────────────────────────────────────────────
        // LEGACY COMPAT: convert old flat Sections → Pages[0]
        // ─────────────────────────────────────────────────────────────────────
        private static List<PageConfig> LegacyToPages(TemplateConfig t)
        {
            var secs = t.Sections ?? new List<CvSection>
            {
                new() { Type = "header" },
                new() { Type = "summary" },
                new() { Type = "skills" },
                new() { Type = "experience" },
                new() { Type = "education" }
            };

            return new List<PageConfig>
            {
                new PageConfig
                {
                    Columns = 1,
                    Sections = secs.Select((s, i) => new SectionConfig
                    {
                        Type = s.Type,
                        Column = 1,
                        Order = i + 1
                    }).ToList()
                }
            };
        }

        private static TemplateConfig BuildDefaultTemplate() => new()
        {
            Theme = new ThemeConfig(),
            Pages = new List<PageConfig>
            {
                new PageConfig
                {
                    Columns = 1,
                    Sections = new List<SectionConfig>
                    {
                        new() { Type = "header",     Column = 1, Order = 1 },
                        new() { Type = "summary",    Column = 1, Order = 2 },
                        new() { Type = "experience", Column = 1, Order = 3 },
                        new() { Type = "education",  Column = 1, Order = 4 },
                        new() { Type = "skills",     Column = 1, Order = 5 },
                    }
                }
            }
        };

        private static string H(string? v) => System.Web.HttpUtility.HtmlEncode(v ?? "");
    }
}
