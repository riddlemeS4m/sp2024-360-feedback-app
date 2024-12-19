using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    public abstract class BaseController : Controller
    {

        protected BaseController()
        {
        }

        public abstract IActionResult UploadRoster(int timeframeId);
        public abstract Task<IActionResult> UploadRoster(IFormFile roster, DateTime filterDate, int roundId);
        public abstract IActionResult GeneratePdfs(int timeframeId, int roundId);
        public abstract Task<IActionResult> GeneratePdfs();
    }
}
