using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface ICVRenderService
    {
        string Render(string templateJson, string dataJson);
    }
}
