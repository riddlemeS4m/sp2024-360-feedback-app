using Capstone_360s.Interfaces;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Services.Controllers
{
    public class ManageFeedbackService : IManageFeedback
    {
        private readonly IFeedbackDbServiceFactory _dbServiceFactory;
        private readonly IGoogleDrive _driveService;
        private readonly ILogger<ManageFeedbackService> _logger;
        public ManageFeedbackService(IFeedbackDbServiceFactory dbServiceFactory,
            IGoogleDrive driveService,
            ILogger<ManageFeedbackService> logger)
        {
            _dbServiceFactory = dbServiceFactory;
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
                var projectName = projectNames[i] ?? $"Project {i + 1}";

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
    }
}