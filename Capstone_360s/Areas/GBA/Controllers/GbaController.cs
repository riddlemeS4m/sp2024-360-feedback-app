using Capstone_360s.Controllers;
using Capstone_360s.Services.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Areas.GBA.Controllers
{
    [Area("GBA")]
    [Authorize]
    public class GbaController : BaseController
    {
        public const string Name = "GBA";
        private readonly GbaService _gbaService;
        private readonly ILogger<GbaController> _logger;

        public GbaController(
            GbaService gbaService,
            ILogger<GbaController> logger
        )

        {
            _gbaService = gbaService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadRoster(IFormFile roster)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePdfs()
        {
            throw new NotImplementedException();
        }
    }
}
