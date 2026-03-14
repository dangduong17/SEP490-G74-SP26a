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
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string LastMessage { get; set; }
        public string Time { get; set; }
        public int UnreadCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChatDetailViewModel
    {
        public int Id { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public bool IsOnline { get; set; }
        public string JobApplied { get; set; }
        
        public JobInfoViewModel JobInfo { get; set; } = new JobInfoViewModel();
        public CompanyInfoViewModel CompanyInfo { get; set; } = new CompanyInfoViewModel();
        
        public List<ChatMessageViewModel> Messages { get; set; } = new List<ChatMessageViewModel>();
    }

    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public bool IsMine { get; set; }
        public string Avatar { get; set; }
        public string Content { get; set; }
        public string Time { get; set; }
        
        public bool IsAttachment { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
    }

    public class JobInfoViewModel
    {
        public string Title { get; set; }
        public string Salary { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
    }

    public class CompanyInfoViewModel
    {
        public string Name { get; set; }
        public string EmployeeCount { get; set; }
        public string HrName { get; set; }
        public string HrTitle { get; set; }
        public string HrAvatar { get; set; }
    }
}
