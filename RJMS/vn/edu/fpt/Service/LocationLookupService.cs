using System.Text.Json;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class LocationLookupService : ILocationLookupService
    {
        private readonly HttpClient _httpClient;

        public LocationLookupService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProvinceLookupDto>> GetProvincesAsync()
        {
            try
            {
                using var response = await _httpClient.GetAsync("/api/v2/");
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync();
                var root = await JsonSerializer.DeserializeAsync<List<ProvinceApiResponse>>(stream, JsonOptions);

                return root?
                    .Select(x => new ProvinceLookupDto { Code = x.Code, Name = x.Name ?? string.Empty })
                    .OrderBy(x => x.Name)
                    .ToList() ?? new List<ProvinceLookupDto>();
            }
            catch
            {
                return new List<ProvinceLookupDto>();
            }
        }

        public async Task<List<WardLookupDto>> GetWardsByProvinceCodeAsync(int provinceCode)
        {
            try
            {
                // v2 (sau sáp nhập) là cấu trúc 2 cấp: Province -> Wards.
                // Một số thời điểm API trả khác schema/endpoint, nên thử lần lượt để tránh "không có dữ liệu".
                var wards = await TryGetWardsFromProvinceDetailAsync(provinceCode);
                if (wards.Count > 0) return wards;

                wards = await TryGetWardsFromCollectionAsync($"/api/v2/w/?province_code={provinceCode}");
                if (wards.Count > 0) return wards;

                wards = await TryGetWardsFromCollectionAsync($"/api/v2/w?province_code={provinceCode}");
                if (wards.Count > 0) return wards;

                wards = await TryGetWardsFromCollectionAsync($"/api/v2/w/?p={provinceCode}");
                if (wards.Count > 0) return wards;

                return new List<WardLookupDto>();
            }
            catch
            {
                return new List<WardLookupDto>();
            }
        }

        private async Task<List<WardLookupDto>> TryGetWardsFromProvinceDetailAsync(int provinceCode)
        {
            using var response = await _httpClient.GetAsync($"/api/v2/p/{provinceCode}?depth=2");
            if (!response.IsSuccessStatusCode) return new List<WardLookupDto>();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var root = await JsonSerializer.DeserializeAsync<ProvinceWithChildrenApiResponse>(stream, JsonOptions);

            // Hỗ trợ cả schema v2 (wards) và fallback schema cũ/khác (districts) để không bị rỗng dữ liệu.
            if (root?.Wards?.Any() == true)
            {
                return root.Wards
                    .Select(x => new WardLookupDto { Code = x.Code, Name = x.Name ?? string.Empty })
                    .OrderBy(x => x.Name)
                    .ToList();
            }

            if (root?.Districts?.Any() == true)
            {
                return root.Districts
                    .Select(x => new WardLookupDto { Code = x.Code, Name = x.Name ?? string.Empty })
                    .OrderBy(x => x.Name)
                    .ToList();
            }

            return new List<WardLookupDto>();
        }

        private async Task<List<WardLookupDto>> TryGetWardsFromCollectionAsync(string path)
        {
            using var response = await _httpClient.GetAsync(path);
            if (!response.IsSuccessStatusCode) return new List<WardLookupDto>();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var items = await JsonSerializer.DeserializeAsync<List<WardApiResponse>>(stream, JsonOptions);

            return items?
                .Select(x => new WardLookupDto { Code = x.Code, Name = x.Name ?? string.Empty })
                .OrderBy(x => x.Name)
                .ToList() ?? new List<WardLookupDto>();
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private sealed class ProvinceApiResponse
        {
            public int Code { get; set; }
            public string? Name { get; set; }
        }

        private sealed class ProvinceWithChildrenApiResponse
        {
            public List<WardApiResponse>? Wards { get; set; }
            public List<DistrictApiResponse>? Districts { get; set; }
        }

        private sealed class DistrictApiResponse
        {
            public int Code { get; set; }
            public string? Name { get; set; }
        }

        private sealed class WardApiResponse
        {
            public int Code { get; set; }
            public string? Name { get; set; }
        }
    }
}
