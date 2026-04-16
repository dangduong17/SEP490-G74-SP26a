using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using vn.edu.fpt.Utilities;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class ChatService : IChatService
    {
        private readonly FindingJobsDbContext _context;

        public ChatService(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<int> StartConversationAsync(int candidateUserId, int jobId, int applicationId)
        {
            // verify job and get recruiter user ID
            var job = await _context.Jobs
                .Include(j => j.JobRecruiters)
                    .ThenInclude(jr => jr.Recruiter)
                .FirstOrDefaultAsync(j => j.Id == jobId);
            var primaryRecruiter = job?.JobRecruiters.FirstOrDefault(jr => jr.IsPrimary)?.Recruiter
                ?? job?.JobRecruiters.FirstOrDefault()?.Recruiter;
            if (job == null || primaryRecruiter == null) return 0;
            
            int recruiterUserId = primaryRecruiter.UserId;

            // Check if there is already a conversation for this exact app
            var existingConv = await _context.ConversationJobs
                .Where(cj => cj.JobId == jobId && cj.ApplicationId == applicationId)
                .Select(cj => cj.ConversationId)
                .FirstOrDefaultAsync();

            if (existingConv > 0)
                return existingConv;

            // Otherwise, create a new conversation
            var conv = new Conversation
            {
                IsGroup = false,
                CreatedAt = DateTimeHelper.NowVietnam,
                UpdatedAt = DateTimeHelper.NowVietnam
            };
            
            _context.Conversations.Add(conv);
            await _context.SaveChangesAsync();

            // create participants
            _context.ConversationParticipants.Add(new ConversationParticipant { ConversationId = conv.Id, UserId = candidateUserId });
            _context.ConversationParticipants.Add(new ConversationParticipant { ConversationId = conv.Id, UserId = recruiterUserId });
            
            // link to job
            _context.ConversationJobs.Add(new ConversationJob { ConversationId = conv.Id, JobId = jobId, ApplicationId = applicationId });
            
            await _context.SaveChangesAsync();
            return conv.Id;
        }

        public async Task<ChatPageViewModel> GetChatPageDataAsync(int userId, int? activeConversationId = null)
        {
            var model = new ChatPageViewModel();

            var participantConvs = await _context.ConversationParticipants
                .Where(cp => cp.UserId == userId)
                .Select(cp => cp.ConversationId)
                .ToListAsync();

            var conversations = await _context.Conversations
                .Include(c => c.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Candidates)
                .Include(c => c.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Recruiters).ThenInclude(r => r.Company)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Include(c => c.ConversationJobs).ThenInclude(cj => cj.Job).ThenInclude(j => j.Company)
                .Where(c => participantConvs.Contains(c.Id))
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            foreach (var c in conversations)
            {
                var otherPart = c.Participants.FirstOrDefault(p => p.UserId != userId)?.User;
                string name = "Người dùng";
                string avatar = "/images/default-avatar.png";
                string company = "";

                var isRecruiter = otherPart?.Recruiters != null && otherPart.Recruiters.Any();
                var isCandidate = otherPart?.Candidates != null && otherPart.Candidates.Any();
                var recruiter = otherPart?.Recruiters?.FirstOrDefault();
                var candidate = otherPart?.Candidates?.FirstOrDefault();

                if (isRecruiter)
                {
                    name = recruiter?.FullName ?? otherPart?.Email ?? "Nhà tuyển dụng";
                    company = recruiter?.Company?.Name ?? "";
                    avatar = recruiter?.Company?.Logo ?? "/images/logo_default.png";
                }
                else if (isCandidate)
                {
                    name = candidate?.FullName ?? otherPart?.Email ?? "Ứng viên";
                    avatar = otherPart?.Avatar ?? "/images/default-avatar.png"; 
                }

                var lastMsg = c.Messages.FirstOrDefault();
                
                model.Conversations.Add(new ChatConversationViewModel
                {
                    Id = c.Id,
                    Name = name,
                    Avatar = avatar,
                    Company = company,
                    LastMessage = lastMsg?.Content ?? "Chưa có tin nhắn",
                    Time = lastMsg?.CreatedAt?.ToString("dd/MM HH:mm") ?? "",
                    UnreadCount = 0, // Simplified for now
                    IsActive = activeConversationId.HasValue && c.Id == activeConversationId.Value
                });
            }

            // Set active conversation if null or empty
            int targetConvId = activeConversationId ?? (model.Conversations.FirstOrDefault()?.Id ?? 0);
            if (targetConvId > 0)
            {
                // mark active
                var activeVM = model.Conversations.FirstOrDefault(c => c.Id == targetConvId);
                if (activeVM != null) activeVM.IsActive = true;

                // Load detail
                var targetConv = conversations.FirstOrDefault(c => c.Id == targetConvId);
                if (targetConv != null)
                {
                    var msgList = await _context.Messages
                        .Include(m => m.Sender)
                        .Where(m => m.ConversationId == targetConvId)
                        .OrderBy(m => m.CreatedAt)
                        .ToListAsync();

                    model.ActiveConversation.Id = targetConv.Id;
                    model.ActiveConversation.Name = activeVM?.Name ?? "";
                    model.ActiveConversation.Company = activeVM?.Company ?? "";
                    model.ActiveConversation.Avatar = activeVM?.Avatar ?? "";
                    model.ActiveConversation.IsOnline = true;

                    var cJob = targetConv.ConversationJobs.FirstOrDefault()?.Job;
                    if (cJob != null)
                    {
                        model.ActiveConversation.JobApplied = cJob.Title ?? "";
                        model.ActiveConversation.JobInfo = new JobInfoViewModel
                        {
                            Title = cJob.Title ?? "",
                            Location = cJob.JobRecruiters.FirstOrDefault(jr => jr.IsPrimary)?.CompanyLocation?.Location?.CityName
                                ?? cJob.Company?.CompanyLocations.FirstOrDefault(cl => cl.IsPrimary)?.Location?.CityName ?? "N/A",
                            Salary = cJob.MinSalary != null ? $"{cJob.MinSalary:N0} - {cJob.MaxSalary:N0}" : "Thỏa thuận",
                            Status = "Đang tuyển"
                        };
                        model.ActiveConversation.CompanyInfo = new CompanyInfoViewModel
                        {
                            Name = cJob.Company?.Name ?? "",
                            HrName = model.ActiveConversation.Name
                        };
                    }

                    foreach (var m in msgList)
                    {
                        model.ActiveConversation.Messages.Add(new ChatMessageViewModel
                        {
                            Id = m.Id,
                            Content = m.Content ?? "",
                            IsMine = m.SenderId == userId,
                            Time = m.CreatedAt?.ToString("dd/MM HH:mm") ?? "",
                            Avatar = m.SenderId == userId ? "/images/default-avatar.png" : model.ActiveConversation.Avatar
                        });
                    }
                }
            }

            return model;
        }

        public async Task<ChatMessageViewModel> SendMessageAsync(int conversationId, int senderUserId, string content)
        {
            var msg = new Message
            {
                ConversationId = conversationId,
                SenderId = senderUserId,
                Content = content,
                CreatedAt = DateTimeHelper.NowVietnam,
                MessageType = "TEXT"
            };
            
            _context.Messages.Add(msg);
            
            var conv = await _context.Conversations.FindAsync(conversationId);
            if (conv != null) conv.UpdatedAt = DateTimeHelper.NowVietnam;

            await _context.SaveChangesAsync();
            
            // Re-fetch avatar if needed, mock for now
            return new ChatMessageViewModel
            {
                Id = msg.Id,
                Content = msg.Content,
                IsMine = true,
                Time = msg.CreatedAt?.ToString("dd/MM HH:mm") ?? "",
                Avatar = "/images/default-avatar.png", // Simplified
                IsAttachment = false
            };
        }
    }
}
