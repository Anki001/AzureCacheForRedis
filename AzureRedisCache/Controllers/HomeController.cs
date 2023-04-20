using AzureRedisCache.Data;
using AzureRedisCache.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AzureRedisCache.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _distributedCache;
        public HomeController(ILogger<HomeController> logger,
            ApplicationDbContext dbContext,
            IDistributedCache distributedCache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public IActionResult Index()
        {
            //// Removed item from Cache
            //_distributedCache.Remove("CategoryList");

            List<Category> categoryList = new();
            var cachedCategories = _distributedCache.GetString("CategoryList");

            if (!string.IsNullOrEmpty(cachedCategories))
            {
                categoryList = JsonConvert.DeserializeObject<List<Category>>(cachedCategories);
            }
            else
            {
                categoryList = _dbContext.Category.ToList();

                // Cache expiration after 30 seconds
                DistributedCacheEntryOptions options = new();
                options.SetAbsoluteExpiration(new TimeSpan(0, 0, 30));

                // Set cache
                _distributedCache.SetString("CategoryList", JsonConvert.SerializeObject(categoryList), options);
            }
            return View(categoryList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}