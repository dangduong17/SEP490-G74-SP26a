using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    public class ChatPageViewModel
    {
        public List<ChatConversationViewModel> Conversations { get; set; } = new List<ChatConversationViewModel>();
        public ChatDetailViewModel ActiveConversation { get; set; } = new ChatDetailViewModel();
    }

    public class ChatConversationViewModel
    {
        public int Id { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChatDetailViewModel
    {
        public int Id { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string JobApplied { get; set; } = string.Empty;
        
        public JobInfoViewModel JobInfo { get; set; } = new JobInfoViewModel();
        public CompanyInfoViewModel CompanyInfo { get; set; } = new CompanyInfoViewModel();
        
        public List<ChatMessageViewModel> Messages { get; set; } = new List<ChatMessageViewModel>();
    }

    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public bool IsMine { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        
        public bool IsAttachment { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
    }

    public class JobInfoViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CompanyInfoViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string EmployeeCount { get; set; } = string.Empty;
        public string HrName { get; set; } = string.Empty;
        public string HrTitle { get; set; } = string.Empty;
        public string HrAvatar { get; set; } = string.Empty;
    }
}
