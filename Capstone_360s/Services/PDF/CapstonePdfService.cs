using Capstone_360s.Data.Constants;
using Capstone_360s.Models.CapstoneRoster;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Services.FeedbackDb;
using Capstone_360s.Services.Maps;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace Capstone_360s.Services.PDF
{
    public class CapstonePdfService : GenericPdfService<DocumentToPrint>
    {
        private readonly RoundService _roundService;
        private readonly FeedbackService _feedbackService;
        private readonly UserService _userService;
        private readonly CapstoneMapToInvertedQualtrics _invertQualtricsService;
        private readonly ILogger<CapstonePdfService> _logger;
        public CapstonePdfService(RoundService roundService,
            FeedbackService feedbackService, 
            UserService userService,
            CapstoneMapToInvertedQualtrics invertQualtricsService, 
            ILogger<CapstonePdfService> logger) : base(logger)
        {
            _roundService = roundService;
            _feedbackService = feedbackService;
            _userService = userService;
            _invertQualtricsService = invertQualtricsService;
            _logger = logger;
        }
        public async Task<IEnumerable<DocumentToPrint>> MapInvertedQualtricsToDocuments(IEnumerable<InvertedQualtrics> feedbackList, int currentRoundId)
        {
            ArgumentNullException.ThrowIfNull(feedbackList);

            if(currentRoundId == 0)
            {
                throw new Exception("Current round id cannot be 0.");
            }

            var list = new List<DocumentToPrint>();

            // Create dictionary to store feedback for each round
            var dict = new Dictionary<int, List<InvertedQualtrics>>();
            for(int i = currentRoundId; i > 0; i--)
            {
                var roundFeedback = feedbackList.Where(x => x.Round.Id == i).ToList() ?? new List<InvertedQualtrics>();
                dict.Add(i, roundFeedback);
            }

            var projectIds = dict[currentRoundId].Select(x => x.Project.Id).ToList();
            // will need to ensure round folders are created before doing this
            // var projectRoundFolderIds = _projectRoundService.GetByProjectIdsAndRoundId(projectIds, roundId)

            // If feedback for a round is not found, get feedback from the database
            for(int i = currentRoundId; i > 0; i--)
            {
                if(!dict.TryGetValue(i, out List<InvertedQualtrics> bogusList))
                {
                    var oldRoundFeedback = await _feedbackService.GetFeedbackByTimeframeIdAndRoundId(feedbackList.ElementAt(0).Timeframe.Id, i);
                    var oldRoundFeedbackList = oldRoundFeedback.ToList();

                    if(oldRoundFeedbackList.Count == 0)
                    {
                        throw new Exception($"Was unable to get feedback for round {i}");
                    }

                    var oldRoundInvertedQualtrics = await _invertQualtricsService.MapFeedback(oldRoundFeedback, i);

                    dict.Add(i, oldRoundInvertedQualtrics.ToList());
                }
            }

            // Create nested dict with round -> email -> individual feedback object
            // so each email still has several feedback objects associated with it
            var biggerDict = new Dictionary<int, Dictionary<string, List<InvertedQualtrics>>>();
            foreach(var item in dict)
            {
                var aggregatedFeedback = AggregateFeedbackByPerson(item.Value);
                biggerDict[item.Key] = aggregatedFeedback;
            }

            var numberOfRounds = biggerDict.Keys.Count;

            if(numberOfRounds != currentRoundId)
            {
                throw new Exception("The algorithm fetched data for more rounds than it was asked to.");
            }

            string[] rounds = new string[numberOfRounds];
            var stringRounds = await _roundService.GetFirstNRounds(numberOfRounds);
            rounds = stringRounds.OrderBy(x => x.Id).Select(x => x.Name).ToArray();

            var users = biggerDict[currentRoundId].Keys ?? throw new Exception("'users' is null.");

            if (users.Count != dict[currentRoundId].Select(x => x.ReviewerEmail).Distinct().Count())
            {
                throw new Exception("Dictionaries were not created properly.");
            }

            // Iterate through feedback items and generate sections for each
            for (int j = 0; j < dict[currentRoundId].Select(x => x.ReviewerEmail).Distinct().Count(); j++)
            {
                string user;
                try
                {
                    user = users.ElementAt(j);
                } catch
                {
                    throw new Exception($"There is not a '{j}' element in 'users'");
                }

                var thisUsersFirstFeedback = biggerDict[currentRoundId][user][0] ?? throw new Exception("'thisUsersFirstFeedback' is null.");

                var firstname = thisUsersFirstFeedback.FirstName;
                var lastname = thisUsersFirstFeedback.LastName;
                var email = thisUsersFirstFeedback.Email;
                var projectName = thisUsersFirstFeedback.Project.Name;
                var projectId = thisUsersFirstFeedback.Project.Id;
                var timeframe = thisUsersFirstFeedback.Timeframe.Name;

                string[] technical = new string[numberOfRounds + 1];
                string[] analytical = new string[numberOfRounds + 1];
                string[] communication = new string[numberOfRounds + 1];
                string[] participation = new string[numberOfRounds + 1];
                string[] performance = new string[numberOfRounds + 1];

                string[] strengths = new string[thisUsersFirstFeedback.Project.NoOfMembers];
                string[] improvements = new string[thisUsersFirstFeedback.Project.NoOfMembers];
                string[] comments = new string[thisUsersFirstFeedback.Project.NoOfMembers];

                for(int k = 1; k <= numberOfRounds; k++)
                {
                    var allRoundFeedback = biggerDict[k] ?? throw new Exception($"Super dictionary does not have key for round {k}");
                    var allRoundFeedbackForUser = allRoundFeedback[user] ?? throw new Exception($"User '{user}' was not found in round {k}");

                    double techScore = 0;
                    double anaScore = 0;
                    double comScore = 0;
                    double partScore = 0;
                    double perfScore = 0;

                    if(allRoundFeedbackForUser.Count != thisUsersFirstFeedback.Project.NoOfMembers)
                    {
                        throw new Exception($"User {user} has more submissions that team members.");
                    }

                    for(int l = 0; l < allRoundFeedbackForUser.Count; l++)
                    {
                        if (allRoundFeedbackForUser[l].ReviewerEmail == user)
                        {
                            technical[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Technology].ToString();
                            analytical[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Analytical].ToString();
                            communication[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Communication].ToString();
                            participation[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Participation].ToString();
                            performance[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Performance].ToString();
                        }
                        else
                        {
                            techScore += allRoundFeedbackForUser[l].Ratings[Capstone.Technology];
                            anaScore += allRoundFeedbackForUser[l].Ratings[Capstone.Analytical];
                            comScore += allRoundFeedbackForUser[l].Ratings[Capstone.Communication];
                            partScore += allRoundFeedbackForUser[l].Ratings[Capstone.Participation];
                            perfScore += allRoundFeedbackForUser[l].Ratings[Capstone.Performance];
                        }

                        // only needs to be set the last time through the k loop
                        if(k == numberOfRounds)
                        {
                            strengths[l] = allRoundFeedbackForUser[l].Questions[Capstone.Strengths];
                            improvements[l] = allRoundFeedbackForUser[l].Questions[Capstone.Improvements];
                            comments[l] = allRoundFeedbackForUser[l].Questions[Capstone.Comments];
                        }
                    }

                    if (k == numberOfRounds)
                    {
                        foreach (string value in strengths)
                        {
                            if (value == null)
                            {
                                throw new Exception("Comments were not populated properly.");
                            }
                        }
                    }

                    techScore = Math.Round(techScore / (allRoundFeedbackForUser.Count - 1), 1);
                    anaScore = Math.Round(anaScore / (allRoundFeedbackForUser.Count - 1), 1);
                    comScore = Math.Round(comScore / (allRoundFeedbackForUser.Count - 1), 1);
                    partScore = Math.Round(partScore / (allRoundFeedbackForUser.Count - 1), 1);
                    perfScore = Math.Round(perfScore / (allRoundFeedbackForUser.Count - 1), 1);

                    technical[k] = techScore.ToString();
                    analytical[k] = anaScore.ToString();
                    communication[k] = comScore.ToString();
                    participation[k] = partScore.ToString();
                    performance[k] = perfScore.ToString();

                    if (techScore == 0)
                    {
                        throw new Exception("Rating scores were not calculated properly.");
                    }

                    if (technical[0].Length > 1)
                    {
                        throw new Exception("Personal scores were not assigned properly.");
                    }

                }

                foreach(string value in technical)
                {
                    if (value == null)
                    {
                        throw new Exception("Ratings were not populated properly.");
                    }
                }

                list.Add(new DocumentToPrint
                {
                    Organization = "Capstone 360 Reviews",
                    FirstName = firstname,
                    LastName = lastname,
                    FullName = firstname + " " + lastname,
                    Email = email,
                    TimeframeName = timeframe,
                    ProjectId = projectId,
                    ProjectName = projectName,
                    RoundNumber = currentRoundId,
                    RoundName = "Round " + currentRoundId,
                    Technical = technical,
                    Analytical = analytical,
                    Communication = communication,
                    Participation = participation,
                    Performance = performance,
                    Strengths = strengths,
                    AreasForImprovement = improvements,
                    Comments = comments,
                    Rounds = rounds
                });
            }

            return list;
        }

        public async Task<List<FeedbackPdf>> GenerateCapstonePdfs(IEnumerable<InvertedQualtrics> invertedQualtrics, int currentRoundId)
        {
            ArgumentNullException.ThrowIfNull(invertedQualtrics);

            if(currentRoundId == 0)
            {
                throw new Exception($"'{nameof(currentRoundId)}' cannot be empty.");
            }

            var filesToReturn = new List<FeedbackPdf>();
            var documentsToReturn = new List<byte[]>();

            var documentsToPrint = await MapInvertedQualtricsToDocuments(invertedQualtrics, currentRoundId);
            var documentsToPrintList = documentsToPrint.OrderBy(x => x.Email).ToList();

            var userEmails = documentsToPrint.Select(x => x.Email).ToList();
            var users = await _userService.GetUsersByListOfEmails(userEmails);
            var usersList = users.OrderBy(x => x.Email).ToList();

            if(usersList.Count != documentsToPrintList.Count)
            {
                throw new Exception("Not every document has a valid user.");
            }
            
            for ( int i = 0; i < documentsToPrintList.Count; i++)
            {
                filesToReturn.Add(new FeedbackPdf()
                {
                    UserId = usersList[i].Id,
                    User = usersList[i],
                    ProjectId = documentsToPrintList[i].ProjectId,
                    RoundId = documentsToPrintList[i].RoundNumber,
                    FileName = documentsToPrintList[i].FullName,
                    Data = await WritePdfAsync(IndividualCapstonePdf, documentsToPrintList[i])
                });
            }   
            
            return filesToReturn;
        }

        private static ListItem CreateSkillSetListItem(string skill, int tabs, params string[] values)
        {
            var itemString = $"{skill}";

            var tabString = "";
            for (int i = 0; i < tabs; i++)
            {
                tabString += "\t";
            }

            for (int j = 0; j < values.Length; j++)
            {
                if(j == 0)
                {
                    itemString += $"{values[j]}{tabString}";
                } else if(j == values.Length - 1)
                {
                    itemString += $"{values[j]}";
                } else
                {
                    itemString += $"{values[j]}\t\t\t\t";
                }
            }

            return new ListItem(itemString);
        }

        public void IndividualCapstonePdf(Document document, DocumentToPrint documentMaterial)
        {
            // Identifying information
            document.Add(new Paragraph(
                documentMaterial.FullName + "\n" +
                documentMaterial.Email + "\n" +
                documentMaterial.ProjectName + "\n" +
                documentMaterial.TimeframeName + " " + 
                documentMaterial.RoundName + " " + 
                documentMaterial.Organization
            ).SetTextAlignment(TextAlignment.LEFT).SetFontSize(12));

            // Add a section title
            document.Add(new Paragraph("\nSkills Review").SetBold().SetFontSize(14));

            // Create the skills review list (bullet points)
            var skillsList = new List().SetSymbolIndent(12).SetListSymbol("\u2022");

            skillsList.Add(CreateSkillSetListItem("Skill Set:\t\tPersonal\t\t", 2, documentMaterial.Rounds));
            skillsList.Add(CreateSkillSetListItem("Technical:\t\t\t", 3, documentMaterial.Technical));
            skillsList.Add(CreateSkillSetListItem("Analytical:\t\t\t", 3, documentMaterial.Analytical));
            skillsList.Add(CreateSkillSetListItem("Communication:\t", 3, documentMaterial.Communication));
            skillsList.Add(CreateSkillSetListItem("Participation:\t\t", 3, documentMaterial.Participation));
            skillsList.Add(CreateSkillSetListItem("Performance:\t\t", 3, documentMaterial.Performance));

            document.Add(skillsList);

            // Add rating explanation
            document.Add(new Paragraph("\nExcellent = 5 | Very Good = 4 | Satisfactory = 3 | Fair = 2 | Poor = 1").SetItalic());

            // Add Strengths
            document.Add(new Paragraph("\nStrengths:\n"));
            document.Add(CreateList(documentMaterial.Strengths));

            // Add Improvements
            document.Add(new Paragraph("\nImprovements:\n"));
            document.Add(CreateList(documentMaterial.AreasForImprovement));

            // Add Additional Comments
            document.Add(new Paragraph("\nAdditional Comments:\n"));
            document.Add(CreateList(documentMaterial.Comments));
        }

        private static List CreateList(params string[] values)
        {
            var listItems = new List().SetSymbolIndent(12).SetListSymbol("\u2022");
            
            foreach(var value in values)
            {
                listItems.Add(new ListItem(value + "\n"));
            }
            return listItems;
        }

        private Dictionary<string, List<InvertedQualtrics>> AggregateFeedbackByPerson(IEnumerable<InvertedQualtrics> invertedQualtrics)
        {
            var list = invertedQualtrics.ToList();
            list.Sort((x,y) => x.Email.CompareTo(y.Email));
            var aggregatedFeedbackByPerson = new Dictionary<string, List<InvertedQualtrics>>();
            foreach (var feedback in list)
            {
                if (aggregatedFeedbackByPerson.TryGetValue(feedback.Email, out List<InvertedQualtrics>? value))
                {
                    value.Add(feedback);
                }
                else
                {
                    aggregatedFeedbackByPerson.Add(feedback.Email, [feedback]);
                }
            }
            return aggregatedFeedbackByPerson;
        }
    }
}
