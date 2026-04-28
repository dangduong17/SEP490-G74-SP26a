using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.vn.edu.fpt.Jobs
{
    public class SliderExpiryJob
    {
        private readonly IWebSliderService _sliderService;
        private readonly ILogger<SliderExpiryJob> _logger;

        public SliderExpiryJob(IWebSliderService sliderService, ILogger<SliderExpiryJob> logger)
        {
            _sliderService = sliderService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var count = await _sliderService.ExpireOutdatedAsync();
                if (count > 0)
                    _logger.LogInformation("[SliderExpiryJob] Đã tắt {Count} slider hết hạn.", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SliderExpiryJob] Lỗi khi tắt slider hết hạn.");
            }
        }
    }
}
