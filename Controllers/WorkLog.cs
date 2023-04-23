using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using WorkLog.Constants;
using WorkLog.Data;
using WorkLog.Models;

namespace WorkLog.Controllers
{
    public class AnswerViewModel
    {
        public List<string>? Questions { get; set; }
    }

    public class WorkLog : Controller
    {
        private readonly WorkLogContext _context;
        private readonly UserManager<AppUser> _userManager;
        public WorkLog(WorkLogContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Home()
        {
            return View("Home");
        }

        public IActionResult About()
        {
            return View("About");
        }
        public IActionResult HowTo()
        {
            return View("HowTo");
        }

        [Authorize]
        public IActionResult Premium()
        {
            return View("Premium");
        }

        [Authorize]
        public async Task<IActionResult> Welcome()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["VisitCount"] = 0;
            if (user != null)
                ViewData["VisitCount"] = user.HitCount;
            
            return View("Welcome");
        }

        [Authorize]
        public async Task<List<string>> GetDefaultQuestions(long tenantId)
        {
            /*
                This code is for tenant
            */
            /*
            Tenant tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return new List<string>();
            else
                return tenant.DefaultQuestions.Split(",#").ToList();
            */
            int i;
            List<string> results = new List<string>();
            for(i = 0;i < 6;i++)
            {
                List<Questions> questionsInCategory = await _context.Questions.Where(x => x.TenantId == tenantId && x.QuestionId == i).ToListAsync();
                if (questionsInCategory.Count > 0)
                {
                    var random = new Random();
                    var randomIndex = random.Next(questionsInCategory.Count);
                    results.Add(questionsInCategory[randomIndex].QuestionText);
                }
            }
            return results;
        }

        [Authorize]
        /*
            Return Answer Page   
        */
        public async Task<IActionResult> Answer()
        {
            long tenantId = long.Parse(HttpContext.Session.GetString("TenantId"));
            List<string> defaultQuestions = await GetDefaultQuestions(tenantId);

            var user = await _userManager.GetUserAsync(User);
            /*
                Get ChannelId from session. 
            */
            int channelId;
            try
            {
                channelId = (int)HttpContext.Session.GetInt32("ChannelId");
            }
            catch (Exception ex)
            {
                channelId = -1;
                HttpContext.Session.SetInt32("ChannelId", -1);
            }

            /*
                Check if you've already answered the question
            */
            var inventories = _context.WorkLog
                .FromSqlRaw($"SELECT * FROM WorkLog WHERE CONVERT(Date, Date) = CONVERT(Date, GETDATE()) AND Email = '{user.Email}' AND ChannelId = {channelId}")
                .ToList();

            if (inventories.Count > 0)
                return await DailyInventory();

            List<string> questions;

            if (channelId == -1)
            {
                questions = defaultQuestions;
            }
            /*
                Get Questions from ChannelId 
            */
            else
            {
                Channel channel = await _context.Channels.FindAsync((long)channelId);
                questions = channel.Questions.Split(",#").ToList();
                questions.Insert(0, defaultQuestions[0]);
            }

            return View("Answer", new AnswerViewModel { Questions = questions });
        }

        [Authorize]
        public IActionResult Search()
        {
            return View("Search");
        }

        [Authorize]
        /*
            Return DailyInventory Page 
        */
        public async Task<IActionResult> DailyInventory(string UserEmail = null, string date = null, long GroupUserId = -1)
        {
            DateTime Date = date == null ? DateTime.Now : DateTime.Parse(date);
            ViewData["GroupId"] = "-1";
            ViewData["Date"] = "-1";
            if (date != null)
            {
                date = Date.ToString("yyyy-MM-dd");
                ViewData["Date"] = date;
            }

            if(GroupUserId != -1)
            {
                GroupUser groupUser = await _context.GroupUsers.FindAsync(GroupUserId);
                Group group = await _context.Groups.FindAsync(groupUser.GroupId);
                ViewData["GroupId"] = groupUser.GroupId.ToString();
                HttpContext.Session.SetInt32("ChannelId", (int)group.ChannelId);

            }

            //It's for your own DailyInventory Page.
            if (UserEmail == null)
            {
                var user = await _userManager.GetUserAsync(User);
                UserEmail = user.Email;
            }

            ViewData["UserEmail"] = UserEmail;

            return View("DailyInventory");
        }

        [Authorize]
        public async Task<IActionResult> LinkedList()
        {
            long tenantId = long.Parse(HttpContext.Session.GetString("TenantId"));
            List<string> defaultQuestions = await GetDefaultQuestions(tenantId);

            var user = await _userManager.GetUserAsync(User);
            int channelId = (int)HttpContext.Session.GetInt32("ChannelId");
            List<WorkLogAnswerDTO> AnswerDTOS = _context.getAnswerDTOSOfDate(DateTime.Now, user.Email, channelId);

            //concat AllAnswers for wordCloud
            string AllAnswers = "";
            foreach (var AnswerDTO in AnswerDTOS)
            {
                foreach (var Answer in AnswerDTO.Answers)
                {
                    AllAnswers += " " + Answer;
                }
            }

            ViewData["AnswerDTOS"] = AnswerDTOS;

            List<string> questions;
            /*
                In case of Default WorkLog channel 
            */
            if (channelId == -1)
                questions = defaultQuestions;

            else
            {
                Channel channel = await _context.Channels.FindAsync((long)channelId);
                questions = channel.Questions.Split(",#").ToList();
                questions.Insert(0, defaultQuestions[0]);
            }

            ViewData["questions"] = questions;
            ViewData["AllAnswers"] = AllAnswers;

            return View("LinkedList");
        }

        [Authorize]
        public IActionResult Compare()
        {
            return View("Compare");
        }

        [Authorize]
        public IActionResult Review()
        {
            return View("Review");
        }
        public async void MakeSampleData(int type = 0)
        {
            List<string> stopWords = new List<string>(){
        "i",
        "me",
        "my",
        "myself",
        "we",
        "us",
        "our",
        "ours",
        "ourselves",
        "you",
        "your",
        "yours",
        "yourself",
        "yourselves",
        "he",
        "him",
        "his",
        "himself",
        "she",
        "her",
        "hers",
        "herself",
        "it",
        "its",
        "itself",
        "they",
        "them",
        "their",
        "theirs",
        "themselves",
        "what",
        "which",
        "who",
        "whom",
        "whose",
        "this",
        "that",
        "these",
        "those",
        "am",
        "is",
        "are",
        "was",
        "were",
        "be",
        "been",
        "being",
        "have",
        "has",
        "had",
        "having",
        "do",
        "does",
        "did",
        "doing",
        "will",
        "would",
        "should",
        "can",
        "could",
        "ought",
        "i'm",
        "you're",
        "he's",
        "she's",
        "it's",
        "we're",
        "they're",
        "i've",
        "you've",
        "we've",
        "they've",
        "i'd",
        "you'd",
        "he'd",
        "she'd",
        "we'd",
        "they'd",
        "i'll",
        "you'll",
        "he'll",
        "she'll",
        "we'll",
        "they'll",
        "isn't",
        "aren't",
        "aren't",
        "wasn't",
        "weren't",
        "hasn't",
        "haven't",
        "hadn't",
        "doesn't",
        "don't",
        "didn't",
        "won't",
        "wouldn't",
        "shan't",
        "shouldn't",
        "can't",
        "cannot",
        "couldn't",
        "mustn't",
        "let's",
        "that's",
        "who's",
        "what's",
        "here's",
        "there's",
        "when's",
        "where's",
        "why's",
        "how's",
        "a",
        "an",
        "the",
        "and",
        "but",
        "if",
        "or",
        "because",
        "as",
        "until",
        "while",
        "of",
        "at",
        "by",
        "for",
        "with",
        "about",
        "against",
        "between",
        "into",
        "through",
        "during",
        "before",
        "after",
        "above",
        "below",
        "to",
        "from",
        "up",
        "upon",
        "down",
        "in",
        "out",
        "on",
        "off",
        "over",
        "under",
        "again",
        "further",
        "then",
        "once",
        "here",
        "there",
        "when",
        "where",
        "why",
        "how",
        "all",
        "any",
        "both",
        "each",
        "few",
        "more",
        "most",
        "other",
        "some",
        "such",
        "no",
        "nor",
        "not",
        "only",
        "own",
        "same",
        "so",
        "than",
        "too",
        "very",
        "say",
        "says",
        "said",
        "shall",
            };
            using (var connection = new SqlConnection(DbConstant.WorkLogDBConnectionString))
            {
                await connection.OpenAsync();
                // Get all distinct dates from the WorkLog table
                string query = "SELECT DISTINCT CONVERT(date, Date) AS LogDate FROM WorkLog";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                // Loop over all distinct dates
                while (reader.Read())
                {
                    DateTime logDate = reader.GetDateTime(0);

                    // Get all answers for this date
                    query = "SELECT Answer FROM WorkLog WHERE CONVERT(date, Date) = @LogDate AND QuestionId != 0";
                    cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@LogDate", logDate);
                    SqlDataReader innerReader = cmd.ExecuteReader();

                    // Count the word frequencies for all answers
                    Dictionary<string, int> wordFrequencies = new Dictionary<string, int>();
                    while (innerReader.Read())
                    {
                        string answer = innerReader.GetString(0);
                        answer = answer.ToLower();
                        string[] words = answer.Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string word in words)
                        {
                            if(stopWords.FindIndex(a => a.Equals(word)) == -1)
                            {
                                if (wordFrequencies.ContainsKey(word))
                                {
                                    wordFrequencies[word]++;
                                }
                                else
                                {
                                    wordFrequencies[word] = 1;
                                }
                            }
                        }
                    }

                    innerReader.Close();

                    // Insert the word frequencies into the WordClouds table
                    foreach (KeyValuePair<string, int> kvp in wordFrequencies)
                    {
                        query = "INSERT INTO WordClouds (Word, Count, Email, ChannelId, LogDate) VALUES (@Word, @Count, @Email, @ChannelId, @LogDate)";
                        cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@Word", kvp.Key);
                        cmd.Parameters.AddWithValue("@Count", kvp.Value);
                        cmd.Parameters.AddWithValue("@Email", "1-testC@yopmail.com");
                        cmd.Parameters.AddWithValue("@ChannelId", -1);
                        cmd.Parameters.AddWithValue("@LogDate", logDate);
                        cmd.ExecuteNonQuery();
                    }
                }

                reader.Close();
            }
        }
    }
}
