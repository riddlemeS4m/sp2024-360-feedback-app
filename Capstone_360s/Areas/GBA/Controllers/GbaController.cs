using Capstone_360s.Controllers;
using Capstone_360s.Services.Configuration.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Areas.GBA.Controllers
{
    [Area("GBA")]
    [Authorize]
    public class GbaController : BaseController
    {
        public const string Name = "Capstone";
        private readonly GbaOrganizationServices _gbaServices;
        private readonly ILogger<GbaController> _logger;

        public GbaController(
            GbaOrganizationServices gbaServices,
            ILogger<GbaController> logger
        )

        {
            _gbaServices = gbaServices;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public override IActionResult UploadRoster(int timeframeId)
        {
            throw new NotImplementedException();
        }

        public override Task<IActionResult> UploadRoster(IFormFile roster, DateTime filterDate, int roundId)
        {
            throw new NotImplementedException();
        }

        public override IActionResult GeneratePdfs(int timeframeId, int roundId)
        {
            throw new NotImplementedException();
        }

        public override Task<IActionResult> GeneratePdfs()
        {
            throw new NotImplementedException();
        }
    }
}
