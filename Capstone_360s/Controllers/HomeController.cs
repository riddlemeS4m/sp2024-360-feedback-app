using Capstone_360s.Interfaces;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Capstone_360s.Controllers
{
    public class HomeController : Controller
    {
        public const string Name = "Home";
        private readonly IFeedbackDbServiceFactory _dbServiceFactory;
        private readonly IGoogleDrive _googleDriveService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IFeedbackDbServiceFactory serviceFactory,
            IGoogleDrive googleDriveService,
            IAuthorizationService authorizationService,
            ILogger<HomeController> logger)
        {
            _dbServiceFactory = serviceFactory;
            _googleDriveService = googleDriveService;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> LandingPage()
        {
            var isAdmin = await _authorizationService.AuthorizeAsync(User, RoleManagerService.ProgramManagerOnlyPolicy);
            var isInstructor = await _authorizationService.AuthorizeAsync(User, RoleManagerService.InstructorOnlyPolicy);
            var organizations = new List<Organization>();

            if(isAdmin.Succeeded)
            {
                var organizationsIE = await _dbServiceFactory.OrganizationService.GetAllAsync();
                organizations = organizationsIE.ToList();
            }
            else
            {
                var userId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value);
                var localUserId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value);

                userId = userId == localUserId ? userId : localUserId;

                var organizationsIE = await _dbServiceFactory.UserOrganizationService.GetOrganizationsByUserId(userId);

                if (organizationsIE == null && isInstructor.Succeeded)
                {
                    return RedirectToAction(nameof(AssignUserToOrganization));
                }

                organizations = organizationsIE.Select(x => x.Organization).ToList();
            }

            return View(organizations);
        }

        [Authorize(Policy = RoleManagerService.InstructorOnlyPolicy)]
        public async Task<IActionResult> AssignUserToOrganization()
        {
            var organizations = await _dbServiceFactory.OrganizationService.GetAllAsync();

            return View(organizations);
        }

        [Authorize(Policy = RoleManagerService.InstructorOnlyPolicy)]
        public async Task<IActionResult> AssignUserToOrganizationConfirm(Guid organizationId)
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value);
            var localUserId = Guid.Parse(User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value);

            userId = userId == localUserId ? userId : localUserId;

            await _dbServiceFactory.UserOrganizationService.AddAsync(new UserOrganization
            {
                UserId = userId,
                OrganizationId = organizationId,
                AddedDate = DateTime.Now
            });

            return RedirectToAction(nameof(LandingPage));
        }

        [Authorize]
        public IActionResult SelectOrganization(Guid organizationId)
        {
            // will need to change to dynamically route
            return RedirectToAction(nameof(UploadProcessController.TimeframesIndex), UploadProcessController.Name);
        }

        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public IActionResult OrganizationCreate()
        {
            _logger.LogInformation("A new organization needs to be created...");
            _logger.LogInformation("Returning organization creation view...");
            return View();
        }

        [HttpPost]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> OrganizationCreate([Bind(nameof(Organization.Id),nameof(Organization.Name), nameof(Organization.Type))] Organization organization, 
            List<string> Names, List<string> Descriptions, List<int> Mins, List<int> Maxs, List<string> Qs, List<string> Examples)
        {
            _logger.LogInformation("Creating a new organization...");
            if(string.IsNullOrEmpty(organization.Name))
            {
                ModelState.AddModelError(nameof(Organization.Name), "Name is required");

                _logger.LogInformation("Returning organization creation view with error...");
                return View(organization);
            }

            organization.GDFolderId = await _googleDriveService.CreateFolderAsync(organization.Name, "");

            await _dbServiceFactory.OrganizationService.AddAsync(organization);

            var metrics = new List<Metric>();
            for (int i = 0; i < Names.Count; i++)
            {
                var metric = new Metric()
                {
                    Name = Names[i],
                    Description = Descriptions[i],
                    MinValue = Mins[i],
                    MaxValue = Maxs[i],
                    OrganizationId = organization.Id
                };

                metrics.Add(metric);
            }

            await _dbServiceFactory.MetricService.AddRange(metrics);

            var questions = new List<Question>();
            for (int i = 0; i < Qs.Count; i++)
            {
                var question = new Question()
                {
                    Q = Qs[i],
                    Example = Examples[i],
                    OrganizationId = organization.Id
                };

                questions.Add(question);
            }

            await _dbServiceFactory.QuestionService.AddRange(questions);

            _logger.LogInformation("Returning next view, timeframes selection view...");
            return RedirectToAction(nameof(UploadProcessController.TimeframesIndex), UploadProcessController.Name, new { organizationId = organization.Id});
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
