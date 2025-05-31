using System.Net;
using WebApp.DTOs;

namespace WebApp.ApiClients
{
    public class ReviewApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<CategoriesApiClient> _logger;

        public ReviewApiClient(HttpClient http, ILogger<CategoriesApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public Task<List<ReviewDTO>> LoadReviewsAsync()
        {
            try
            {
                return _http.GetFromJsonAsync<List<ReviewDTO>>("review");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load reviews from API.");
                throw;
            }
        }
            

        public async Task<ReviewDTO?> LoadReviewAsync(int id)
        {
            try
            {
                var resp = await _http.GetAsync($"review/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return null;
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<ReviewDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load single review from API.");
                throw;
            }
        }

        public async Task<ReviewDTO> CreateReviewAsync(ReviewDTO newReview)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("review", newReview);
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<ReviewDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create review via API.");
                throw;
            }
        }

        public async Task<bool> UpdateReviewAsync(ReviewDTO updatedReview)
        {
            try
            {
                var resp = await _http.PutAsJsonAsync($"review/{updatedReview.Id}", updatedReview);
                if (resp.StatusCode == HttpStatusCode.NotFound) return false;
                resp.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update review via API.");
                throw;
            }
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            try
            {
                var resp = await _http.DeleteAsync($"review/{id}");
                if (resp.StatusCode == HttpStatusCode.NotFound) return false;
                resp.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete review via API.");
                throw;
            }
        }

        public async Task<List<ReviewDTO>>LoadReviewsByProductId(int productId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ReviewDTO>>($"review/by-product/{productId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load reviews by product ID via API.");
                throw;
            }
        }
    }
}
