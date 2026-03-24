using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    // ───────── List ─────────
    public class CvListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CvType { get; set; } = "UPLOAD";     // UPLOAD | BUILDER
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDefault { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public int? FileSize { get; set; }
        public string? TemplateName { get; set; }
    }

    public class CandidateCvViewModel
    {
        public List<CvListItemViewModel> Cvs { get; set; } = new();
    }

    // ───────── Template Category ─────────
    public class TemplateCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public int TemplateCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TemplateCategoryFormDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // ───────── Template ─────────
    public class CvTemplateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string HtmlContent { get; set; } = string.Empty;
        public string? CssContent { get; set; }
        public string? ConfigJson { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CvTemplateCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? ThumbnailFile { get; set; }
        public string HtmlContent { get; set; } = string.Empty;
        public string? CssContent { get; set; }
        public string? ConfigJson { get; set; }
        public int? CategoryId { get; set; }
    }

    public class CvTemplateEditDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public IFormFile? ThumbnailFile { get; set; }
        public string? ExistingThumbnailUrl { get; set; }
        public string HtmlContent { get; set; } = string.Empty;
        public string? CssContent { get; set; }
        public string? ConfigJson { get; set; }
        public int? CategoryId { get; set; }
        public bool IsActive { get; set; }
    }

    // ───────── Builder Editor ─────────
    public class CvEditorViewModel
    {
        public int CvId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TemplateId { get; set; }
        public string HtmlContent { get; set; } = string.Empty;
        public string? CssContent { get; set; }
        public string? ConfigJson { get; set; }
        public string JsonData { get; set; } = "{}";
    }

    // ───────── Upload ─────────
    public class CvUploadDTO
    {
        public string? Title { get; set; }
        public IFormFile? File { get; set; }
    }

    // ───────── Block Template Models ─────────
    /// <summary>Top-level template JSON config – saved in CvTemplates.ConfigJson</summary>
    public class TemplateConfig
    {
        /// <summary>Legacy flat layout (kept for backward compat). Prefer Pages.</summary>
        public string Layout { get; set; } = "one-column";
        /// <summary>Legacy flat sections list. Populated when Pages is empty.</summary>
        public List<CvSection>? Sections { get; set; }
        public List<PageConfig> Pages { get; set; } = new();
        public ThemeConfig Theme { get; set; } = new();
    }

    public class PageConfig
    {
        /// <summary>1 or 2 columns</summary>
        public int Columns { get; set; } = 1;
        public List<SectionConfig> Sections { get; set; } = new();
    }

    public class SectionConfig
    {
        /// <summary>header | experience | education | skills | projects | summary</summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>1-based column index</summary>
        public int Column { get; set; } = 1;
        /// <summary>Sort order within column</summary>
        public int Order { get; set; } = 1;
        
        // For free-form absolute placement
        public double? Top { get; set; }
        public double? Left { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        
        // Stores candidate-specific arbitrary HTML overrides
        public string? Html { get; set; }
    }

    /// <summary>Legacy flat section – kept for old data</summary>
    public class CvSection
    {
        public string Type { get; set; } = string.Empty;
        public string Position { get; set; } = "left";
    }

    public class ThemeConfig
    {
        public string PrimaryColor { get; set; } = "#2563eb";
        public string Font { get; set; } = "Inter";
    }

    public class CvDataModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public List<ExperienceModel> Experiences { get; set; } = new List<ExperienceModel>();
        public List<EducationModel> Educations { get; set; } = new List<EducationModel>();
        
        // Allows candidates to override template's layout and customize section positions/pages
        public TemplateConfig? CustomLayout { get; set; }
    }

    public class ExperienceModel
    {
        public string Company { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class EducationModel
    {
        public string School { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
    }
}
