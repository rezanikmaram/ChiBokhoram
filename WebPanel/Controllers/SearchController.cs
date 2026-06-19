using Entities.Common;
using Entities.DTOs.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
        public async Task<IActionResult> Index(int? p = null,
           SearchFilterDto filter = null)
        {
            var list = await _searchService.Search(p ?? 1, filter);
            return View(list);
        }
        [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
        public async Task<IActionResult> GetDetail(string id, SearchEntityType type)
        {
            await _searchService.GetDetail(new SearchDetailFilterDto { Id = id, Type = type });
            return View();
        }
    }
}
