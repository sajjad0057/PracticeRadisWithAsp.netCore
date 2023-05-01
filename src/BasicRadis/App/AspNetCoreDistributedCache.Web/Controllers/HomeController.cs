using AspNetCoreDistributedCache.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace AspNetCoreDistributedCache.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDistributedCache _distributedCache;

        public HomeController(ILogger<HomeController> logger, IDistributedCache distributedCache)
        {
            _logger = logger;
            _distributedCache = distributedCache;
        }

        public async Task<IActionResult> SaveRedisCache()
        {
            try
            {
                var dashboardData = new DashboardData
                {
                    TotalCustomerCount = 110020,
                    TotalRevenue = 12000,
                    TopSellingCountryName = "USA",
                    TopSellingProductName = "Macbook Pro"
                };

                var tomorrow = DateTime.Now.Date.AddDays(1);
                var totalSeconds = tomorrow.Subtract(DateTime.Now).TotalSeconds;

                #region Manage Cache data expiration time
                /* Manage cache data expiration time */
                var distributedCacheEntryOptions = new DistributedCacheEntryOptions();
                distributedCacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(totalSeconds);
                //distributedCacheEntryOptions.SlidingExpiration = TimeSpan.FromHours(1);
                distributedCacheEntryOptions.SlidingExpiration = null;
                #endregion

                var jsonData = JsonConvert.SerializeObject(dashboardData);

                await _distributedCache.SetStringAsync("DashboardData", jsonData, distributedCacheEntryOptions);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.StackTrace);
            }

            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var jsonData = await _distributedCache.GetStringAsync("DashboardData");

            var dashboardData = new DashboardData();

            if (jsonData is not null)
                dashboardData = JsonConvert.DeserializeObject<DashboardData>(jsonData);

            ViewBag.DashboardData = dashboardData;

            return View();
        }
    }
}