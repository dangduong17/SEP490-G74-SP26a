using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RJMS.Vn.Edu.Fpt.Service;
using Xunit;

namespace RJMS.Tests
{
    public class ServiceInterfaceCoverageTests
    {
        private static MethodInfo RequireMethod(
            Type interfaceType,
            string methodName,
            int parameterCount
        )
        {
            var method = interfaceType
                .GetMethods()
                .FirstOrDefault(m =>
                    m.Name == methodName && m.GetParameters().Length == parameterCount
                );

            Assert.NotNull(method);
            return method!;
        }

        private static void AssertTaskReturningMethod(
            Type interfaceType,
            string methodName,
            int parameterCount
        )
        {
            var method = RequireMethod(interfaceType, methodName, parameterCount);
            Assert.True(typeof(Task).IsAssignableFrom(method.ReturnType));
        }

        [Fact]
        public void IAdminService_GetDashboardAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetDashboardAsync),
                0
            );

        [Fact]
        public void IAdminService_GetUserListAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetUserListAsync),
                5
            );

        [Fact]
        public void IAdminService_GetUpdateUserAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetUpdateUserAsync),
                1
            );

        [Fact]
        public void IAdminService_CreateAdminAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.CreateAdminAsync),
                1
            );

        [Fact]
        public void IAdminService_CreateManagerAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.CreateManagerAsync),
                1
            );

        [Fact]
        public void IAdminService_CreateCandidateAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.CreateCandidateAsync),
                1
            );

        [Fact]
        public void IAdminService_CreateRecruiterAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.CreateRecruiterAsync),
                1
            );

        [Fact]
        public void IAdminService_UpdateUserAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.UpdateUserAsync),
                1
            );

        [Fact]
        public void IAdminService_SoftDeleteUserAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.SoftDeleteUserAsync),
                1
            );

        [Fact]
        public void IAdminService_SetUserActiveStatusAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.SetUserActiveStatusAsync),
                2
            );

        [Fact]
        public void IAdminService_GetSkillListAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetSkillListAsync),
                4
            );

        [Fact]
        public void IAdminService_GetSkillForEditAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetSkillForEditAsync),
                1
            );

        [Fact]
        public void IAdminService_CreateSkillAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.CreateSkillAsync),
                1
            );

        [Fact]
        public void IAdminService_UpdateSkillAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.UpdateSkillAsync),
                1
            );

        [Fact]
        public void IAdminService_DeleteSkillAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.DeleteSkillAsync),
                1
            );

        [Fact]
        public void IAdminService_GetCompanyListAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetCompanyListAsync),
                5
            );

        [Fact]
        public void IAdminService_GetCompanyDetailAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetCompanyDetailAsync),
                1
            );

        [Fact]
        public void IAdminService_VerifyCompanyAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.VerifyCompanyAsync),
                1
            );

        [Fact]
        public void IAdminService_UnverifyCompanyAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.UnverifyCompanyAsync),
                1
            );

        [Fact]
        public void IAdminService_GetCompanyLocationsAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetCompanyLocationsAsync),
                1
            );

        [Fact]
        public void IAdminService_AddCompanyLocationAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.AddCompanyLocationAsync),
                2
            );

        [Fact]
        public void IAdminService_DeleteCompanyLocationAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.DeleteCompanyLocationAsync),
                1
            );

        [Fact]
        public void IAdminService_GetEmployeeListAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetEmployeeListAsync),
                4
            );

        [Fact]
        public void IAdminService_CreateEmployeeAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.CreateEmployeeAsync),
                1
            );

        [Fact]
        public void IAdminService_AssignEmployeeLocationsAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.AssignEmployeeLocationsAsync),
                1
            );

        [Fact]
        public void IAdminService_GetSubscriptionListAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetSubscriptionListAsync),
                5
            );

        [Fact]
        public void IAdminService_GetSubscriptionDetailAsync() =>
            AssertTaskReturningMethod(
                typeof(IAdminService),
                nameof(IAdminService.GetSubscriptionDetailAsync),
                1
            );

        [Fact]
        public void IApplicationService_GetApplyModalDataAsync() =>
            AssertTaskReturningMethod(
                typeof(IApplicationService),
                nameof(IApplicationService.GetApplyModalDataAsync),
                2
            );

        [Fact]
        public void IApplicationService_ApplyJobAsync() =>
            AssertTaskReturningMethod(
                typeof(IApplicationService),
                nameof(IApplicationService.ApplyJobAsync),
                5
            );

        [Fact]
        public void IApplicationService_GetNotificationsAsync() =>
            AssertTaskReturningMethod(
                typeof(IApplicationService),
                nameof(IApplicationService.GetNotificationsAsync),
                2
            );

        [Fact]
        public void IApplicationService_GetUnreadCountAsync() =>
            AssertTaskReturningMethod(
                typeof(IApplicationService),
                nameof(IApplicationService.GetUnreadCountAsync),
                1
            );

        [Fact]
        public void IApplicationService_MarkReadAsync() =>
            AssertTaskReturningMethod(
                typeof(IApplicationService),
                nameof(IApplicationService.MarkReadAsync),
                2
            );

        [Fact]
        public void IApplicationService_MarkAllReadAsync() =>
            AssertTaskReturningMethod(
                typeof(IApplicationService),
                nameof(IApplicationService.MarkAllReadAsync),
                1
            );

        [Fact]
        public void IAuthService_LoginAsync() =>
            AssertTaskReturningMethod(typeof(IAuthService), nameof(IAuthService.LoginAsync), 1);

        [Fact]
        public void IAuthService_LogoutAsync() =>
            AssertTaskReturningMethod(typeof(IAuthService), nameof(IAuthService.LogoutAsync), 0);

        [Fact]
        public void IAuthService_ForgotPasswordAsync() =>
            AssertTaskReturningMethod(
                typeof(IAuthService),
                nameof(IAuthService.ForgotPasswordAsync),
                1
            );

        [Fact]
        public void IAuthService_RegisterAsync() =>
            AssertTaskReturningMethod(typeof(IAuthService), nameof(IAuthService.RegisterAsync), 1);

        [Fact]
        public void IAuthService_RegisterRecruiterAsync() =>
            AssertTaskReturningMethod(
                typeof(IAuthService),
                nameof(IAuthService.RegisterRecruiterAsync),
                1
            );

        [Fact]
        public void IAuthService_ConfirmEmailAsync() =>
            AssertTaskReturningMethod(
                typeof(IAuthService),
                nameof(IAuthService.ConfirmEmailAsync),
                1
            );

        [Fact]
        public void ICVService_GetCandidateCvsAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.GetCandidateCvsAsync),
                1
            );

        [Fact]
        public void ICVService_UploadCvAsync() =>
            AssertTaskReturningMethod(typeof(ICVService), nameof(ICVService.UploadCvAsync), 2);

        [Fact]
        public void ICVService_GetActiveTemplatesAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.GetActiveTemplatesAsync),
                0
            );

        [Fact]
        public void ICVService_CreateBuilderCvAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.CreateBuilderCvAsync),
                3
            );

        [Fact]
        public void ICVService_GetEditorViewModelAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.GetEditorViewModelAsync),
                3
            );

        [Fact]
        public void ICVService_SaveCvDataAsync() =>
            AssertTaskReturningMethod(typeof(ICVService), nameof(ICVService.SaveCvDataAsync), 5);

        [Fact]
        public void ICVService_RenderCvHtmlAsync_ById() =>
            AssertTaskReturningMethod(typeof(ICVService), nameof(ICVService.RenderCvHtmlAsync), 1);

        [Fact]
        public void ICVService_RenderCvHtmlAsync_WithOverride() =>
            AssertTaskReturningMethod(typeof(ICVService), nameof(ICVService.RenderCvHtmlAsync), 2);

        [Fact]
        public void ICVService_DeleteCvAsync() =>
            AssertTaskReturningMethod(typeof(ICVService), nameof(ICVService.DeleteCvAsync), 2);

        [Fact]
        public void ICVService_GetAllTemplatesAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.GetAllTemplatesAsync),
                0
            );

        [Fact]
        public void ICVService_GetTemplateByIdAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.GetTemplateByIdAsync),
                1
            );

        [Fact]
        public void ICVService_CreateTemplateAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.CreateTemplateAsync),
                1
            );

        [Fact]
        public void ICVService_UpdateTemplateAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.UpdateTemplateAsync),
                1
            );

        [Fact]
        public void ICVService_ToggleTemplateActiveAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.ToggleTemplateActiveAsync),
                1
            );

        [Fact]
        public void ICVService_DeleteTemplateAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.DeleteTemplateAsync),
                1
            );

        [Fact]
        public void ICVService_GetAllCategoriesAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.GetAllCategoriesAsync),
                0
            );

        [Fact]
        public void ICVService_GetCategoryByIdAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.GetCategoryByIdAsync),
                1
            );

        [Fact]
        public void ICVService_CreateCategoryAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.CreateCategoryAsync),
                1
            );

        [Fact]
        public void ICVService_UpdateCategoryAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.UpdateCategoryAsync),
                1
            );

        [Fact]
        public void ICVService_DeleteCategoryAsync() =>
            AssertTaskReturningMethod(
                typeof(ICVService),
                nameof(ICVService.DeleteCategoryAsync),
                1
            );

        [Fact]
        public void ICVRenderService_Render_ReturnsString() =>
            Assert.Equal(
                typeof(string),
                RequireMethod(
                    typeof(ICVRenderService),
                    nameof(ICVRenderService.Render),
                    2
                ).ReturnType
            );

        [Fact]
        public void IChatService_GetChatPageDataAsync() =>
            AssertTaskReturningMethod(
                typeof(IChatService),
                nameof(IChatService.GetChatPageDataAsync),
                2
            );

        [Fact]
        public void IChatService_StartConversationAsync() =>
            AssertTaskReturningMethod(
                typeof(IChatService),
                nameof(IChatService.StartConversationAsync),
                3
            );

        [Fact]
        public void IChatService_SendMessageAsync() =>
            AssertTaskReturningMethod(
                typeof(IChatService),
                nameof(IChatService.SendMessageAsync),
                3
            );

        [Fact]
        public void ICompanyService_GetCompanyDetailsAsync() =>
            AssertTaskReturningMethod(
                typeof(ICompanyService),
                nameof(ICompanyService.GetCompanyDetailsAsync),
                2
            );

        [Fact]
        public void ICompanyService_FollowCompanyAsync() =>
            AssertTaskReturningMethod(
                typeof(ICompanyService),
                nameof(ICompanyService.FollowCompanyAsync),
                2
            );

        [Fact]
        public void ICompanyService_UnfollowCompanyAsync() =>
            AssertTaskReturningMethod(
                typeof(ICompanyService),
                nameof(ICompanyService.UnfollowCompanyAsync),
                2
            );

        [Fact]
        public void IJobApplicationService_GetApplicationsAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobApplicationService),
                nameof(IJobApplicationService.GetApplicationsAsync),
                1
            );

        [Fact]
        public void IJobCategoryService_GetCategoriesAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobCategoryService),
                nameof(IJobCategoryService.GetCategoriesAsync),
                4
            );

        [Fact]
        public void IJobCategoryService_GetPossibleParentsAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobCategoryService),
                nameof(IJobCategoryService.GetPossibleParentsAsync),
                1
            );

        [Fact]
        public void IJobCategoryService_CreateCategoryAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobCategoryService),
                nameof(IJobCategoryService.CreateCategoryAsync),
                1
            );

        [Fact]
        public void IJobCategoryService_GetCategoryByIdAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobCategoryService),
                nameof(IJobCategoryService.GetCategoryByIdAsync),
                1
            );

        [Fact]
        public void IJobCategoryService_UpdateCategoryAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobCategoryService),
                nameof(IJobCategoryService.UpdateCategoryAsync),
                1
            );

        [Fact]
        public void IJobCategoryService_DeleteCategoryAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobCategoryService),
                nameof(IJobCategoryService.DeleteCategoryAsync),
                1
            );

        [Fact]
        public void IJobService_GetPublicJobListAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobService),
                nameof(IJobService.GetPublicJobListAsync),
                5
            );

        [Fact]
        public void IJobService_GetJobDetailAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobService),
                nameof(IJobService.GetJobDetailAsync),
                1
            );

        [Fact]
        public void IJobService_ToggleSavedJobAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobService),
                nameof(IJobService.ToggleSavedJobAsync),
                2
            );

        [Fact]
        public void IJobService_IsJobSavedAsync() =>
            AssertTaskReturningMethod(typeof(IJobService), nameof(IJobService.IsJobSavedAsync), 2);

        [Fact]
        public void IJobService_GetSavedJobListAsync() =>
            AssertTaskReturningMethod(
                typeof(IJobService),
                nameof(IJobService.GetSavedJobListAsync),
                2
            );

        [Fact]
        public void IProfileService_GetPersonalProfileAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.GetPersonalProfileAsync),
                1
            );

        [Fact]
        public void IProfileService_GetCandidateProfileForEditAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.GetCandidateProfileForEditAsync),
                1
            );

        [Fact]
        public void IProfileService_UpdateCandidateProfileAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.UpdateCandidateProfileAsync),
                2
            );

        [Fact]
        public void IProfileService_ChangePasswordAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.ChangePasswordAsync),
                3
            );

        [Fact]
        public void IProfileService_GetRecruiterProfileAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.GetRecruiterProfileAsync),
                1
            );

        [Fact]
        public void IProfileService_UpdateRecruiterProfileAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.UpdateRecruiterProfileAsync),
                2
            );

        [Fact]
        public void IProfileService_GetRecruiterProfileForEditAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.GetRecruiterProfileForEditAsync),
                1
            );

        [Fact]
        public void IProfileService_UpdateRecruiterProfileNewAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.UpdateRecruiterProfileNewAsync),
                2
            );

        [Fact]
        public void IProfileService_GetCompanyProfileForEditAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.GetCompanyProfileForEditAsync),
                1
            );

        [Fact]
        public void IProfileService_UpdateCompanyProfileAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.UpdateCompanyProfileAsync),
                2
            );

        [Fact]
        public void IProfileService_GetAdminProfileForEditAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.GetAdminProfileForEditAsync),
                1
            );

        [Fact]
        public void IProfileService_UpdateAdminProfileAsync() =>
            AssertTaskReturningMethod(
                typeof(IProfileService),
                nameof(IProfileService.UpdateAdminProfileAsync),
                2
            );

        [Fact]
        public void IRecruiterManagementService_GetCompanyLocationsAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.GetCompanyLocationsAsync),
                1
            );

        [Fact]
        public void IRecruiterManagementService_AddCompanyLocationAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.AddCompanyLocationAsync),
                2
            );

        [Fact]
        public void IRecruiterManagementService_DeleteCompanyLocationAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.DeleteCompanyLocationAsync),
                2
            );

        [Fact]
        public void IRecruiterManagementService_SetPrimaryLocationAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.SetPrimaryLocationAsync),
                2
            );

        [Fact]
        public void IRecruiterManagementService_GetEmployeeListAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.GetEmployeeListAsync),
                4
            );

        [Fact]
        public void IRecruiterManagementService_CreateEmployeeAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.CreateEmployeeAsync),
                2
            );

        [Fact]
        public void IRecruiterManagementService_GetEmployeeForEditAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.GetEmployeeForEditAsync),
                2
            );

        [Fact]
        public void IRecruiterManagementService_UpdateEmployeeAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.UpdateEmployeeAsync),
                2
            );

        [Fact]
        public void IRecruiterManagementService_BanEmployeeAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.BanEmployeeAsync),
                2
            );

        [Fact]
        public void IRecruiterManagementService_UnbanEmployeeAsync() =>
            AssertTaskReturningMethod(
                typeof(IRecruiterManagementService),
                nameof(IRecruiterManagementService.UnbanEmployeeAsync),
                2
            );
    }
}
