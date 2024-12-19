using Capstone_360s.Controllers;
using Capstone_360s.Services.Configuration.Organizations;
using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Areas.Capstone.Controllers
{
    [Area("Capstone")]
    [Authorize]
    public class CapstoneController : BaseController
    {
        public const string Name = "Capstone";
        private readonly CapstoneOrganizationServices _capstoneServices;
        private readonly ILogger<CapstoneController> _logger;

        public CapstoneController(
            CapstoneOrganizationServices capstoneServices,
            ILogger<CapstoneController> logger
        )
        {
            _capstoneServices = capstoneServices;
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
