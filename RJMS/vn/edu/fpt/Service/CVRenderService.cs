using System;
using System.Text.Json;
using System.Text;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class CVRenderService : ICVRenderService
    {
        public string Render(string templateJson, string dataJson)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var template = string.IsNullOrWhiteSpace(templateJson) 
                ? new TemplateConfig() 
                : JsonSerializer.Deserialize<TemplateConfig>(templateJson, options) ?? new TemplateConfig();

            var data = string.IsNullOrWhiteSpace(dataJson) 
                ? new CvDataModel() 
                : JsonSerializer.Deserialize<CvDataModel>(dataJson, options) ?? new CvDataModel();

            var html = new StringBuilder();

            // Default sections if none found
            if (template.Sections == null || template.Sections.Count == 0)
            {
                template.Sections = new System.Collections.Generic.List<CvSection>
                {
                    new CvSection { Type = "header", Position = "top" },
                    new CvSection { Type = "summary", Position = "left" },
                    new CvSection { Type = "skills", Position = "left" },
                    new CvSection { Type = "experience", Position = "right" },
                    new CvSection { Type = "education", Position = "right" }
                };
            }

            foreach (var section in template.Sections)
            {
                html.Append(RenderSection(section.Type, data, template.Theme));
            }

            return WrapLayout(html.ToString(), template);
        }

        private string RenderSection(string type, CvDataModel data, ThemeConfig theme)
        {
            return type.ToLower() switch
            {
                "header" => RenderHeader(data, theme),
                "summary" => RenderSummary(data, theme),
                "experience" => RenderExperience(data, theme),
                "education" => RenderEducation(data, theme),
                "skills" => RenderSkills(data, theme),
                _ => ""
            };
        }

        private string RenderHeader(CvDataModel data, ThemeConfig theme)
        {
            return $@"
            <div class='cv-header' style='background-color: {theme.PrimaryColor}; color: #ffffff; padding: 40px; text-align: center; border-radius: 8px 8px 0 0;'>
                <h1 style='margin: 0; font-size: 2.5rem; font-weight: 700; text-transform: uppercase; letter-spacing: 1px;'>{System.Web.HttpUtility.HtmlEncode(data.FullName)}</h1>
                <h3 style='margin: 10px 0 20px; font-weight: 400; opacity: 0.9;'>{System.Web.HttpUtility.HtmlEncode(data.Position)}</h3>
                <div style='display: flex; justify-content: center; gap: 20px; font-size: 0.9rem; flex-wrap: wrap;'>
                    <span><i class='fas fa-envelope'></i> {System.Web.HttpUtility.HtmlEncode(data.Email)}</span>
                    <span><i class='fas fa-phone'></i> {System.Web.HttpUtility.HtmlEncode(data.Phone)}</span>
                    <span><i class='fas fa-map-marker-alt'></i> {System.Web.HttpUtility.HtmlEncode(data.Address)}</span>
                </div>
            </div>";
        }

        private string RenderSummary(CvDataModel data, ThemeConfig theme)
        {
            if (string.IsNullOrWhiteSpace(data.Summary)) return "";

            return $@"
            <div class='cv-section' style='padding: 20px; margin-top: 10px;'>
                <h3 style='color: {theme.PrimaryColor}; border-bottom: 2px solid {theme.PrimaryColor}; padding-bottom: 5px; margin-bottom: 15px; font-size: 1.2rem; text-transform: uppercase;'>Giới thiệu</h3>
                <p style='line-height: 1.6; color: #333;'>{System.Web.HttpUtility.HtmlEncode(data.Summary)}</p>
            </div>";
        }

        private string RenderExperience(CvDataModel data, ThemeConfig theme)
        {
            if (data.Experiences == null || data.Experiences.Count == 0) return "";

            var sb = new StringBuilder();
            sb.Append($@"
            <div class='cv-section' style='padding: 20px;'>
                <h3 style='color: {theme.PrimaryColor}; border-bottom: 2px solid {theme.PrimaryColor}; padding-bottom: 5px; margin-bottom: 20px; font-size: 1.2rem; text-transform: uppercase;'>Kinh nghiệm làm việc</h3>");

            foreach (var exp in data.Experiences)
            {
                sb.Append($@"
                <div style='margin-bottom: 25px;'>
                    <div style='display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 5px;'>
                        <strong style='font-size: 1.1rem; color: #111;'>{System.Web.HttpUtility.HtmlEncode(exp.Company)}</strong>
                        <span style='color: #666; font-size: 0.9rem; background: #f0f0f0; padding: 2px 8px; border-radius: 4px;'>{System.Web.HttpUtility.HtmlEncode(exp.Period)}</span>
                    </div>
                    <div style='font-weight: 600; color: #444; margin-bottom: 8px;'>{System.Web.HttpUtility.HtmlEncode(exp.Role)}</div>
                    <p style='margin: 0; line-height: 1.6; color: #555;'>{System.Web.HttpUtility.HtmlEncode(exp.Description)}</p>
                </div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private string RenderEducation(CvDataModel data, ThemeConfig theme)
        {
            if (data.Educations == null || data.Educations.Count == 0) return "";

            var sb = new StringBuilder();
            sb.Append($@"
            <div class='cv-section' style='padding: 20px;'>
                <h3 style='color: {theme.PrimaryColor}; border-bottom: 2px solid {theme.PrimaryColor}; padding-bottom: 5px; margin-bottom: 20px; font-size: 1.2rem; text-transform: uppercase;'>Học vấn</h3>");

            foreach (var edu in data.Educations)
            {
                sb.Append($@"
                <div style='margin-bottom: 20px;'>
                    <div style='display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 5px;'>
                        <strong style='font-size: 1.1rem; color: #111;'>{System.Web.HttpUtility.HtmlEncode(edu.School)}</strong>
                        <span style='color: #666; font-size: 0.9rem;'>{System.Web.HttpUtility.HtmlEncode(edu.Period)}</span>
                    </div>
                    <div style='color: #444;'>{System.Web.HttpUtility.HtmlEncode(edu.Degree)}</div>
                </div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private string RenderSkills(CvDataModel data, ThemeConfig theme)
        {
            if (string.IsNullOrWhiteSpace(data.Skills)) return "";

            var skills = data.Skills.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            
            sb.Append($@"
            <div class='cv-section' style='padding: 20px;'>
                <h3 style='color: {theme.PrimaryColor}; border-bottom: 2px solid {theme.PrimaryColor}; padding-bottom: 5px; margin-bottom: 15px; font-size: 1.2rem; text-transform: uppercase;'>Kỹ năng</h3>
                <div style='display: flex; flex-wrap: wrap; gap: 8px;'>");

            foreach (var skill in skills)
            {
                sb.Append($@"<span style='background: {theme.PrimaryColor}20; color: {theme.PrimaryColor}; padding: 4px 12px; border-radius: 16px; font-size: 0.9rem; font-weight: 500;'>{System.Web.HttpUtility.HtmlEncode(skill.Trim())}</span>");
            }

            sb.Append("</div></div>");
            return sb.ToString();
        }

        private string WrapLayout(string content, TemplateConfig template)
        {
            var fontStyle = "";
            if (!string.IsNullOrWhiteSpace(template.Theme.Font))
            {
                fontStyle = $"font-family: '{template.Theme.Font}', sans-serif;";
            }

            var html = $@"
            <div class='cv-document {template.Layout}' style='max-width: 800px; margin: 0 auto; background: #fff; box-shadow: 0 4px 20px rgba(0,0,0,0.1); border-radius: 8px; * {fontStyle} overflow: hidden; color: #333;'>
                {content}
            </div>";

            return html;
        }
    }
}
