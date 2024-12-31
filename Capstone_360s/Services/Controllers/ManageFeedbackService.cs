using System.Globalization;
using Capstone_360s.Interfaces;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.VMs;
using Capstone_360s.Utilities.Maps;
using CsvHelper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Capstone_360s.Services.Controllers
{
    public class ManageFeedbackService : IManageFeedback
    {
        private readonly IFeedbackDbServiceFactory _dbServiceFactory;
        private readonly IGoogleDrive _driveService;
        private readonly IRoleManager _roleManager;
        private readonly IConfigureEnvironment _config;
        private readonly ILogger<ManageFeedbackService> _logger;
        public ManageFeedbackService(IFeedbackDbServiceFactory dbServiceFactory,
            IGoogleDrive driveService,
            IRoleManager roleManager,
            IConfigureEnvironment config,
            ILogger<ManageFeedbackService> logger)
        {
            _dbServiceFactory = dbServiceFactory;
            _roleManager = roleManager;
            _config = config;
            _driveService = driveService;
            _logger = logger;
        }

        public async Task CreateTimeframe(Timeframe timeframe, List<string> projectNames)
        {
            var organizationFolderId = (await _dbServiceFactory.OrganizationService.GetByIdAsync(timeframe.OrganizationId)).GDFolderId;
            timeframe.GDFolderId = await _driveService.CreateFolderAsync(timeframe.Name, organizationFolderId);

            await _dbServiceFactory.TimeframeService.AddAsync(timeframe);

            if(timeframe.NoOfProjects == 0)
            {
                _logger.LogInformation("Timeframe does not have any projects.");
                return;
            }

            var projects = new List<Project>();
            for (int i = 0; i < timeframe.NoOfProjects; i++)
            {
                var projectName = projectNames[i] ?? $"Team {i + 1}";

                var projectFolderId = await _driveService.CreateFolderAsync(
                    projectName,
                    timeframe.GDFolderId
                );

                var project = new Project()
                {
                    Name = projectName,
                    TimeframeId = timeframe.Id,
                    OrganizationId = timeframe.OrganizationId,
                    GDFolderId = projectFolderId,
                    NoOfRounds = timeframe.NoOfRounds
                };

                projects.Add(project);
            }

            await _dbServiceFactory.ProjectService.AddRange(projects);
        }

        public async Task CreateProjectRounds(Project project, List<DateTime> roundStartDates, List<DateTime> roundEndDates)
        {
            var rounds = await _dbServiceFactory.RoundService.GetFirstNRounds(project.NoOfRounds);
            var roundsList = rounds.ToList();

            if(roundsList.Count < project.NoOfRounds)
            {
                if(roundsList.Count == 0)
                {
                    for(int i = 1; i <= project.NoOfRounds; i++)
                    {
                        roundsList.Add(new Round
                        {
                            Name = $"Round {i}"
                        });
                    }

                    await _dbServiceFactory.RoundService.AddRange(roundsList);
                }
                else
                {
                    var noOfRoundsToMake = project.NoOfRounds - roundsList.Count;
                    var roundsToMake = new List<Round>();

                    for(int i = roundsList.Count + 1; i <= noOfRoundsToMake; i++)
                    {
                        roundsToMake.Add(new Round 
                        {
                            Name = $"Round {i}"
                        });
                    }

                    await _dbServiceFactory.RoundService.AddRange(roundsToMake);
                }
            }

            rounds = await _dbServiceFactory.RoundService.GetFirstNRounds(project.NoOfRounds);
            roundsList = rounds.ToList();
            if (roundsList.Count < project.NoOfRounds)
            {
                throw new Exception("There are not enough rounds in the database.");
            }

            var projects = await _dbServiceFactory.ProjectService.GetProjectsByTimeframeId(project.OrganizationId.ToString(), project.TimeframeId);
            var projectsList = projects.ToList();

            if(projectsList.Count == 0 || roundsList.Count == 0)
            {
                throw new Exception("There are no projects or rounds to link.");
            }

            if(projectsList.Select(x => x.NoOfRounds).Distinct().Count() > 1)
            {
                throw new Exception("Not every project has the same number of rounds.");
            }

            if(projectsList.Select(x => x.NoOfRounds).Distinct().FirstOrDefault() != roundsList.Count)
            {
                throw new Exception("Not every project has the correct amount of rounds.");
            }

            var projectRounds = new List<ProjectRound>();
            for(int i = 0; i < projectsList.Count; i++)
            {
                var existingRounds = await _dbServiceFactory.ProjectRoundService.GetProjectRoundsByProjectId(projectsList[i].Id.ToString());
                for(int j = 0; j < roundsList.Count; j++)
                {
                    if(!existingRounds.Any(x => x.RoundId == j + 1))
                    {
                        var folderName = roundsList[j].Name;
                        var projectRoundFolderId = await _driveService.CreateFolderAsync(folderName, projectsList[i].GDFolderId);

                        DateTime? startDate;
                        DateTime? endDate;

                        if(roundStartDates.Count == roundsList.Count)
                        {
                            startDate = roundStartDates[j];
                        } else { startDate = null; }
                        
                        if(roundEndDates.Count == roundsList.Count)
                        {
                            endDate = roundEndDates[j];
                        } else { endDate = null; }

                        var projectRound = new ProjectRound
                        {
                            ProjectId = projectsList[i].Id,
                            RoundId = roundsList[j].Id,
                            GDFolderId = projectRoundFolderId,
                            ReleaseDate = startDate,
                            DueDate = endDate
                        };

                        projectRounds.Add(projectRound);
                    }
                }
            }

            // if(projectRounds.Count != projectsList.Count * roundsList.Count)
            // {
            //     throw new Exception("Not every project has the correct amount of rounds.");
            // }

            if(projectRounds.Count > 0)
            {
                await _dbServiceFactory.ProjectRoundService.AddRange(projectRounds);
            }
        }

        public async Task CreateProjectRoundsForOneProject(Project project)
        {
            var rounds = await _dbServiceFactory.RoundService.GetFirstNRounds(project.NoOfRounds);
            var roundsList = rounds.ToList();

            if(roundsList.Count < project.NoOfRounds)
            {
                if(roundsList.Count == 0)
                {
                    for(int i = 1; i <= project.NoOfRounds; i++)
                    {
                        roundsList.Add(new Round
                        {
                            Name = $"Round {i}"
                        });
                    }

                    await _dbServiceFactory.RoundService.AddRange(roundsList);
                }
                else
                {
                    var noOfRoundsToMake = project.NoOfRounds - roundsList.Count;
                    var roundsToMake = new List<Round>();

                    for(int i = roundsList.Count + 1; i <= noOfRoundsToMake; i++)
                    {
                        roundsToMake.Add(new Round 
                        {
                            Name = $"Round {i}"
                        });
                    }

                    await _dbServiceFactory.RoundService.AddRange(roundsToMake);
                }
            }

            var projectRounds = new List<ProjectRound>();
            var existingRounds = await _dbServiceFactory.ProjectRoundService.GetProjectRoundsByProjectId(project.Id.ToString());
            for(int j = 0; j < roundsList.Count; j++)
            {
                if(!existingRounds.Any(x => x.RoundId == j + 1))
                {
                    var folderName = roundsList[j].Name;
                    var projectRoundFolderId = await _driveService.CreateFolderAsync(folderName, project.GDFolderId);


                    var projectRound = new ProjectRound
                    {
                        ProjectId = project.Id,
                        RoundId = roundsList[j].Id,
                        GDFolderId = projectRoundFolderId,
                        ReleaseDate = null,
                        DueDate = null
                    };

                    projectRounds.Add(projectRound);
                }
            }
            
            if(projectRounds.Count > 0)
            {
                await _dbServiceFactory.ProjectRoundService.AddRange(projectRounds);
            }
        }

        public async Task UploadBlackboardRoster(IFormFile csv, string orgId, int timeframeId)
        {
            var userRecords = ReadRosterCsv(csv, ["Last Name", "First Name", "Username"]);

            var emails = userRecords.Select(x => x.Email).ToList();
            var users = (await _dbServiceFactory.UserService.GetUsersByListOfEmails(emails)).ToList();

            var usersToAdd = userRecords.Where(x => !users.Any(u => u.Email == x.Email)).ToList();

            if(usersToAdd.Count > 0)
            {
                await _dbServiceFactory.UserService.AddRange(usersToAdd);
                users = (await _dbServiceFactory.UserService.GetUsersByListOfEmails(emails)).ToList();
            }

            // if(users.Count != emails.Count)
            // {
            //     throw new Exception("Number of users in database does not match number of users uploaded.");
            // }

            var userOrganizations = (await _dbServiceFactory.UserOrganizationService.GetUsersByOrganizationId(Guid.Parse(orgId))).ToList();
            usersToAdd = users.Where(x => !userOrganizations.Any(uo => uo.User.Email == x.Email)).ToList();
            
            if(usersToAdd.Count > 0)
            {
                var userOrganizationsToAdd = new List<UserOrganization>();

                foreach(var user in usersToAdd)
                {
                    userOrganizationsToAdd.Add(new UserOrganization {
                        UserId = user.Id,
                        OrganizationId = Guid.Parse(orgId),
                    });
                }

                await _dbServiceFactory.UserOrganizationService.AddRange(userOrganizationsToAdd);
                userOrganizations = (await _dbServiceFactory.UserOrganizationService.GetUsersByOrganizationId(Guid.Parse(orgId))).ToList();
            }

            var userTimeframes = (await _dbServiceFactory.UserTimeframeService.GetUsersByTimeframeId(timeframeId)).ToList();
            usersToAdd = users.Where(x => !userTimeframes.Any(ut => ut.User.Email == x.Email)).ToList();

            if(usersToAdd.Count > 0)
            {
                var userTimeframesToAdd = new List<UserTimeframe>();

                foreach(var user in usersToAdd)
                {
                    userTimeframesToAdd.Add(new UserTimeframe {
                        UserId = user.Id,
                        TimeframeId = timeframeId
                    });
                }

                await _dbServiceFactory.UserTimeframeService.AddRange(userTimeframesToAdd);
                userTimeframes = (await _dbServiceFactory.UserTimeframeService.GetUsersByTimeframeId(timeframeId)).ToList();
            }

            if(users.Count != userOrganizations.Count || users.Count != userTimeframes.Count)
            {
                throw new Exception("Not every user has an organization and/or timeframe assignment.");
            }
        }

        public async Task CreateProject(Project project, Timeframe timeframe, string orgId, int timeframeId, 
            string POCEmail, string ManagerEmail, string NewTeamMembers)
        {
            var newProject = new Project
            {
                Name = project.Name,
                Description = project.Description,
                TimeframeId = timeframeId,
                NoOfRounds = timeframe.NoOfRounds,
                OrganizationId = Guid.Parse(orgId),
                GDFolderId = await _driveService.CreateFolderAsync(project.Name, timeframe.GDFolderId),
            };

            if(!string.IsNullOrEmpty(ManagerEmail))
            {
                newProject.ManagerId = await AssignUserPermissions(orgId, ManagerEmail, _config.TeamLead);   
            }

            if(!string.IsNullOrEmpty(POCEmail))
            {
                newProject.POCId = await AssignUserPermissions(orgId, POCEmail, _config.Instructor);
            }

            newProject = await _dbServiceFactory.ProjectService.AddAsync(newProject);

            if(!string.IsNullOrEmpty(NewTeamMembers))
            {
                var newTeamMembers = NewTeamMembers.Split(",").ToList();

                newProject.NoOfMembers = await AssignTeamMembers(newProject, orgId, timeframeId, newProject.Id, newTeamMembers, true);
                
                await _dbServiceFactory.ProjectService.UpdateAsync(newProject);
            }
        }

        public async Task<Project> EditProject(Project newProject, Project oldProject, string orgId, int timeframeId,
            string POCEmail, string ManagerEmail, List<string> NewTeamMembers)
        {
            var pocCondition = oldProject.POC?.Email != POCEmail && !string.IsNullOrEmpty(POCEmail);
            var managerCondition =  oldProject.Manager?.Email != ManagerEmail && !string.IsNullOrEmpty(ManagerEmail);        

            oldProject.Name = oldProject.Name != newProject.Name && !string.IsNullOrEmpty(newProject.Name) ? newProject.Name : oldProject.Name;
            oldProject.Description = oldProject.Description != newProject.Description && !string.IsNullOrEmpty(newProject.Description) ? newProject.Description : oldProject.Description;

            if(pocCondition)
            {
                oldProject.POCId = await AssignUserPermissions(orgId, POCEmail, _config.Instructor);
            }

            if(managerCondition)
            {
                oldProject.ManagerId = await AssignUserPermissions(orgId, ManagerEmail, _config.TeamLead);
            }

            if(NewTeamMembers.Count > 0)
            {
                oldProject.NoOfMembers = await AssignTeamMembers(oldProject, orgId, timeframeId, oldProject.Id, NewTeamMembers, true);
            }

            if(oldProject.NoOfRounds < newProject.NoOfRounds && newProject.NoOfRounds != 0)
            {
                oldProject.NoOfRounds = newProject.NoOfRounds;
                await CreateProjectRoundsForOneProject(oldProject);
            }

            return oldProject;
        }

        public async Task<ProjectEditVM> CreateProjectCreateViewModel(string orgId, int timeframeId)
        {
            if(string.IsNullOrEmpty(orgId) || timeframeId == 0)
            {
                throw new ArgumentNullException("One provided parameter was blank.");
            }

            var pocs = await _roleManager.GetUsersByRole(orgId, _config.Instructor);

            var teamMembers = await _dbServiceFactory.UserTimeframeService.GetUsersByTimeframeId(timeframeId);

            var pocItems = pocs.OrderBy(x => x.FirstName)
                .Select(x => new SelectListItem { 
                    Text = $"{x.FirstName} {x.LastName}",
                    Value = x.Email
                }).ToList();
            var memberItems = teamMembers.OrderBy(x => x.User.FirstName)
                .Select(x => new SelectListItem { 
                    Text = $"{x.User.FirstName} {x.User.LastName}",
                    Value = x.User.Email
                })
                .ToList();

            var vm = new ProjectEditVM()
            {
                Project = new Project(),
                PotentialPOCs = pocItems ?? [],
                PotentialManagers = new List<SelectListItem>(),
                PotentialTeamMembers = memberItems ?? []
            };

            return vm;
        }

        public async Task<ProjectEditVM> CreateProjectEditViewModel(string orgId, int timeframeId, string projectId)
        {
            if(string.IsNullOrEmpty(orgId) || timeframeId == 0 || string.IsNullOrEmpty(projectId))
            {
                throw new ArgumentNullException("One provided parameter was blank.");
            }

            var vm = await CreateProjectCreateViewModel(orgId, timeframeId);

            vm.Project = await _dbServiceFactory.ProjectService.GetProjectAndTeamMembersById(projectId) ?? throw new Exception("Project not found."); 

            return vm;
        }

        public async Task<AssignPOCVM> CreateAssignPOCViewModel(string orgId, string projectId)
        {
            if(string.IsNullOrEmpty(orgId) || string.IsNullOrEmpty(projectId))
            {
                throw new ArgumentNullException("Parameters must not be blank.");
            }

            var project = await _dbServiceFactory.ProjectService.GetByIdAsync(Guid.Parse(projectId));

            if(project == null)
            {
                throw new Exception("Project must not be null.");
            }

            var users = await _roleManager.GetUsersByRole(orgId, _config.Instructor);
            
            var items = new List<SelectListItem>();
            if(users != null)
            {
                foreach(var user in users)
                {
                    items.Add(new SelectListItem{
                        Text = user.GetFullName(),
                        Value = user.Email
                    });
                }
            }

            var vm = new AssignPOCVM {
                Project = project,
                POCs = items
            };

            return vm;
        }

        public async Task<AssignPOCVM> CreateAssignManagerViewModel(string orgId, string projectId)
        {
            if(string.IsNullOrEmpty(orgId) || string.IsNullOrEmpty(projectId))
            {
                throw new ArgumentNullException("Parameters must not be blank.");
            }

            var project = await _dbServiceFactory.ProjectService.GetByIdAsync(Guid.Parse(projectId));

            if(project == null)
            {
                throw new Exception("Project must not be null.");
            }

            var userTimeframes = (await _dbServiceFactory.UserTimeframeService.GetUsersByTimeframeId(project.TimeframeId)).ToList() ?? throw new Exception("No users have been uploaded yet.");
            
            var items = new List<SelectListItem>();
            if(userTimeframes != null)
            {
                foreach(var user in userTimeframes)
                {
                    items.Add(new SelectListItem{
                        Text = user.User.GetFullName(),
                        Value = user.User.Email
                    });
                }
            }

            var vm = new AssignPOCVM {
                Project = project,
                POCs = items
            };

            return vm;
        }
        
        public async Task AssignPOCToProject(string orgId, string projectId, string email)
        {
            if (string.IsNullOrEmpty(orgId) || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("Parameters must not be blank.");
            }

            var project = await _dbServiceFactory.ProjectService.GetProjectAndTeamMembersById(projectId) ?? throw new Exception("Could not find project.");

            project.POCId = await AssignUserPermissions(orgId, email, _config.Instructor);

            await _dbServiceFactory.ProjectService.UpdateAsync(project);
        }

        public async Task AssignManagerToProject(string orgId, string projectId, string email)
        {
            if (string.IsNullOrEmpty(orgId) || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("Parameters must not be blank.");
            }

            var project = await _dbServiceFactory.ProjectService.GetProjectAndTeamMembersById(projectId) ?? throw new Exception("Could not find project.");

            var userId = await AssignUserPermissions(orgId, email, _config.TeamLead);

            var currentTeam = (await _dbServiceFactory.TeamService.GetTeamMembersByTimeframeIdAndUserId(project.TimeframeId, userId)).FirstOrDefault();

            bool alreadyOnTeam = true;
            if(currentTeam != null)
            {
                alreadyOnTeam = currentTeam.ProjectId != project.Id;
                if(!alreadyOnTeam)
                {
                    await _dbServiceFactory.TeamService.Remove(currentTeam);
                }
            }

            if(!alreadyOnTeam)
            {
                await _dbServiceFactory.TeamService.AddAsync(new TeamMember {
                    UserId = userId,
                    ProjectId = project.Id,
                });
            }

            project.ManagerId = userId;

            await _dbServiceFactory.ProjectService.UpdateAsync(project);
        }

        public async Task<Project> CreateProjectSilently(Timeframe timeframe, string orgId, int timeframeId, string projectId, string userEmail, string projectName = "")
        {
            if(string.IsNullOrEmpty(orgId) || timeframeId == 0 || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(userEmail))
            {
                throw new ArgumentNullException("Parameters must not be blank.");
            }

            if(!string.IsNullOrEmpty(projectName))
            {
                var newProject = new Project
                {
                    Name = projectName,
                    Description = "This project was automatically created by a new student team assignment.",
                    TimeframeId = timeframeId,
                    NoOfRounds = timeframe.NoOfRounds,
                    OrganizationId = Guid.Parse(orgId),
                    GDFolderId = await _driveService.CreateFolderAsync(projectName, timeframe.GDFolderId),
                    ManagerId = null,
                    POCId = null,
                    NoOfMembers = 1
                };

                newProject = await _dbServiceFactory.ProjectService.AddAsync(newProject);

                newProject.NoOfMembers = await AssignTeamMembers(newProject, orgId, timeframeId, newProject.Id, [userEmail]); 
                newProject = await _dbServiceFactory.ProjectService.UpdateAsync(newProject);
                return newProject;
            }
            else
            {
                var existingProject = await _dbServiceFactory.ProjectService.GetByIdAsync(Guid.Parse(projectId)) ?? throw new Exception("Project not found.");
                existingProject.NoOfMembers = await AssignTeamMembers(existingProject, orgId, timeframeId, existingProject.Id, [userEmail]); 
                existingProject = await _dbServiceFactory.ProjectService.UpdateAsync(existingProject);
                return existingProject;
            }
        }

        private static List<User> ReadRosterCsv(IFormFile file, IEnumerable<string> expectedHeaders)
        {
            // Check if the file is null or empty
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("CSV file is required and cannot be empty.");
            }

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Read the header row
            csv.Read();
            csv.ReadHeader();

            var headers = csv.HeaderRecord;

            // Verify that the headers match
            var missingHeaders = expectedHeaders.Except(headers).ToList();
            if (missingHeaders.Count != 0)
            {
                throw new Exception($"The following expected headers are missing: {string.Join(", ", missingHeaders)}");
            }

            // Register the class map and read the records
            csv.Context.RegisterClassMap(new BlackboardMapCsvToUser());

            // Manually read records and filter rows before mapping
            var validRecords = new List<User>();
            while (csv.Read())
            {
                var record = csv.GetRecord<User>();
                validRecords.Add(record);
            }

            return validRecords;
        }

        public async Task<Guid> AssignUserPermissions(string orgId, string email, string role = "")
        {
            if(string.IsNullOrEmpty(orgId) || string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("Organization ID and email are required.");
            }

            var user = await _dbServiceFactory.UserService.GetUserByEmail(email);

            if(user == null || user.Id == Guid.Empty || string.IsNullOrEmpty(user.MicrosoftId.ToString()))
            {
                user = await _roleManager.AddNewUser(orgId, email) ?? throw new UnauthorizedAccessException("There was some trouble adding this user. Please try logging out and logging back in.");
            }

            if(!string.IsNullOrEmpty(role))
            {
                await _roleManager.AddUserToRole(orgId, user.MicrosoftId.ToString(), role);
            }

            return user.Id;
        }

        private async Task<int> AssignTeamMembers(Project toProject, string orgId, int timeframeId, Guid toProjectId, List<string> emails, bool exclusive = false)
        {
            if(exclusive && toProject.NoOfMembers > 0)
            {
                var teamMembers = (await _dbServiceFactory.TeamService.GetTeamMembersByProjectId(toProjectId)).ToList() ?? throw new Exception("Received project does not match database.");
                await _dbServiceFactory.TeamService.RemoveRange(teamMembers);
                toProject.NoOfMembers = 0;
            }

            if(emails.Count > 0)
            {
                var users = new List<TeamMember>();

                foreach(var email in emails)
                {
                    var userId = await AssignUserPermissions(orgId, email);

                    var currentTeam = (await _dbServiceFactory.TeamService.GetTeamMembersByTimeframeIdAndUserId(timeframeId, userId)).FirstOrDefault();

                    if(currentTeam != null)
                    {
                        var fromProjectId = currentTeam.ProjectId;

                        if(fromProjectId != toProjectId)
                        {
                            var fromProject = currentTeam.Project;
                            fromProject.NoOfMembers--;
                            await _dbServiceFactory.ProjectService.UpdateAsync(fromProject);
                            await _dbServiceFactory.TeamService.Remove(currentTeam);
                        }
                    }

                    users.Add(new TeamMember {
                        UserId = userId,
                        ProjectId = toProjectId,
                    });

                    toProject.NoOfMembers++;
                }

                if(users.Count > 0)
                {
                    await _dbServiceFactory.TeamService.AddRange(users);
                }

                return toProject.NoOfMembers;
            }

            return 0;
        }
    }
}