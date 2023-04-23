using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using WorkLog.Data;
using WorkLog.Models;
using WorkLog.Repository;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static WorkLog.Constants.DbConstant;

namespace WorkLog.Controllers
{

    [Route("api/[controller]")]
    [Authorize]
    public class WorkLogAPIController : Controller
    {

        private readonly IConfiguration _config;
        private readonly WorkLogContext _context;
        private readonly UserManager<AppUser> _userManager;

        public WorkLogAPIController(WorkLogContext context, UserManager<AppUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _context = context;
            _config = config;
        }
        public IActionResult Index()
        {
            return View("WorkLog/Index");
        }

        [Route("getSessionExpirationTime")]
        [HttpPost]
        public async Task<int> GetSessionExpirationTime()
        {
            try
            {
                return _config.GetValue<int>("Session:TimeOut");
            } catch(Exception ex)
            {
                return 0;
            }
        }

        [Route("getIsSessionExpired")]
        [HttpPost]
        public async Task<bool> GetIsSessionExpired()
        {
            try
            {
                int ChannelId = (int)HttpContext.Session.GetInt32("ChannelId");
                string Email = HttpContext.Session.GetString("UserEmail");
                return true;
            } catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<List<string>> GetDefaultQuestions(long tenantId)
        {
            Tenant tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return new List<string>();
            else
                return tenant.DefaultQuestions.Split(",#").ToList();
        }

        /*
            Add answers to the database. 
            Request Parameter: list of answers.
        */
        [Route("addAnswers")]
        [HttpPost]
        public async Task<IActionResult> AddAnswers(List<WorkLogAnswer> answers, List<WordCloud> wordClouds)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager?.GetUserAsync(User);
                //concat all answers
                answers.ForEach(workLog =>
                {
                    /*
                        Set Email as loggined user's Email and channelId as session's channelId 
                    */
                    workLog.Email = user?.Email;
                    workLog.ChannelId = (int)HttpContext.Session.GetInt32("ChannelId");
                    _context.Add(workLog);
                });

                wordClouds.ForEach(wordCloud =>
                {
                    wordCloud.ChannelId = (int)HttpContext.Session.GetInt32("ChannelId");
                    wordCloud.Email = user?.Email;
                    _context.WordClouds.Add(wordCloud);
                });
                await _context.SaveChangesAsync();
                return NoContent();
            }
            return NoContent();
        }

        /*
            concat all answers between startDate to endDate.  
        */
        [Route("getByDateRange")]
        [HttpPost]
        public async Task<string> GetByDateRange(string startDateStr, string endDateStr)
        {
            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            var user = await _userManager?.GetUserAsync(User);
            var result = await _context.WorkLog.Where(x => x.ChannelId == (int)HttpContext.Session.GetInt32("ChannelId") && x.Date.Value >= startDate && x.Date.Value <= endDate && x.Email == user.Email).ToListAsync();
            string concat_answers = "";
            result.ForEach(x => concat_answers += x.Answer);
            //string concat_answers = _context.ConcatAllAnswersBetweenDateRange(startDate, endDate, user.Email, );

            return concat_answers;
        }

        [Route("getDailyInventoryByDate")]
        [HttpPost]
        public async Task<DailyInventoryDTO> GetDailyInventoryByDate(string date = null, string userEmail = null)
        {
            if (userEmail == null)
            {
                var user = await _userManager.GetUserAsync(User);
                userEmail = user.Email;
            }

            DateTime Date = date == null ? DateTime.Now : DateTime.Parse(date);
            if (date == null)
                date = Date.ToString("yyyy-MM-dd");

            long tenantId = long.Parse(HttpContext.Session.GetString("TenantId"));
            List<string> defaultQuestions = await GetDefaultQuestions(tenantId);

            int channelId = (int)HttpContext.Session.GetInt32("ChannelId");

            List<WorkLogAnswerDTO> AnswerDTOS = _context.getAnswerDTOSOfDate(Date, userEmail, channelId);

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
                Models.Channel channel = await _context.Channels.FindAsync((long)channelId);
                questions = channel.Questions.Split(",#").ToList();
                questions.Insert(0, defaultQuestions[0]);
            }

            DailyInventoryDTO dailyInventoryDTO = new DailyInventoryDTO();

            dailyInventoryDTO.AllAnswers = AllAnswers;
            dailyInventoryDTO.Answers = AnswerDTOS;
            dailyInventoryDTO.Questions = questions;

            return dailyInventoryDTO;
        }

        /*
            flag = 0: "Past Week"
            flag = 1: "Past Month"
            flag = 2: "Past 3 Months"
            flag = 3: "Past Year"
            flag = 4: "All Time"

            Build Word cloud by flag.
        */
        [Route("searchWorkLogs")]
        [HttpPost]
        public async Task<List<WorkLogSearchDTO>> SearchWorkLogs(int flag)
        {
            var user = await _userManager?.GetUserAsync(User);
            DateTime CurrentDate = DateTime.Now;
            int i;
            List<WorkLogSearchDTO> results = new List<WorkLogSearchDTO>();
            int channelId = (int)HttpContext.Session.GetInt32("ChannelId");

            switch (flag)
            {
                //past week
                case 0:
                    for (i = 0; i < 7; i++)
                    {
                        DateTime pastDate = CurrentDate.AddDays(-6 + i);
                        string concat_answers = _context.ConcatAllAnswersBetweenDateRange(pastDate, pastDate, user.Email, channelId);
                        List<int> Feeling = _context.GetFeelingCounts(pastDate, pastDate, user.Email);
                        results.Add(new WorkLogSearchDTO()
                        {
                            Date = pastDate.ToString("dddd MM/dd/yy"),
                            Answer_Concat = concat_answers,
                            Feeling = Feeling
                        });
                    }
                    return results;
                //past month
                case 1:
                    for (i = 0; i < 4; i++)
                    {
                        DateTime pastBeginDate = CurrentDate.AddDays(-7 * (4 - i));
                        DateTime pastEndDate = pastBeginDate.AddDays(7);
                        string concat_answers = _context.ConcatAllAnswersBetweenDateRange(pastBeginDate, pastEndDate, user.Email, channelId);
                        List<int> Feeling = _context.GetFeelingCounts(pastBeginDate, pastEndDate, user.Email);
                        results.Add(new WorkLogSearchDTO()
                        {
                            Date = pastBeginDate.ToString("MM/dd/yy") + " - " + pastEndDate.ToString("MM/dd/yy"),
                            Answer_Concat = concat_answers,
                            Feeling = Feeling
                        });
                    }
                    return results;
                // past 3 month
                case 2:
                    for (i = 0; i < 3; i++)
                    {
                        DateTime pastBeginDate = CurrentDate.AddMonths(-(3 - i));
                        DateTime pastEndDate = pastBeginDate.AddMonths(1);
                        string concat_answers = _context.ConcatAllAnswersBetweenDateRange(pastBeginDate, pastEndDate, user.Email, channelId);
                        List<int> Feeling = _context.GetFeelingCounts(pastBeginDate, pastEndDate, user.Email);
                        results.Add(new WorkLogSearchDTO()
                        {
                            Date = pastBeginDate.ToString("MMMM dd") + " - " + pastEndDate.ToString("MMMM dd"),
                            Answer_Concat = concat_answers,
                            Feeling = Feeling
                        });
                    }
                    return results;
                //past year
                case 3:
                    for (i = 0; i < 12; i++)
                    {
                        DateTime pastBeginDate = CurrentDate.AddMonths(-(12 - i));
                        DateTime pastEndDate = pastBeginDate.AddMonths(1);
                        string concat_answers = _context.ConcatAllAnswersBetweenDateRange(pastBeginDate, pastEndDate, user.Email, channelId);
                        List<int> Feeling = _context.GetFeelingCounts(pastBeginDate, pastEndDate, user.Email);
                        results.Add(new WorkLogSearchDTO()
                        {
                            Date = pastEndDate.ToString("MMMM yyyy"),
                            Answer_Concat = concat_answers,
                            Feeling = Feeling
                        });
                    }
                    return results;
                //all time
                case 4:
                    results.Add(new WorkLogSearchDTO()
                    {
                        Date = "All Time",
                        Answer_Concat = _context.ConcatAllAnswersBetweenDateRange(DateTime.MinValue, DateTime.MaxValue, user.Email, channelId),
                        Feeling = _context.GetFeelingCounts(DateTime.MinValue, DateTime.MaxValue, user.Email)
                    });
                    return results;
                default:
                    return null;
            }
            //return NoContent();
        }

        [Route("searchByKeywords")]
        [HttpPost]
        public async Task<List<WorkLogAnswer>> SearchByKeywords(string keyword)
        {
            var user = await _userManager.GetUserAsync(User);
            int channelId = (int)HttpContext.Session.GetInt32("ChannelId");

            var answers = await _context.WorkLog.Where(x => x.ChannelId == channelId && x.Email == user.Email && x.Answer.Contains(keyword)).ToListAsync();
            return answers;
        }

        [Route("getDateList")]
        [HttpPost]
        public async Task<List<DateListDTO>> GetDateLists(int start, int count, long groupId, string? userEmail = null)
        {
            CommentRepository commentRepository = new CommentRepository();

            var user = await _userManager.GetUserAsync(User);

            if (userEmail == null)
            {
                userEmail = user.Email;
            }

            int channelId = (int)HttpContext.Session.GetInt32("ChannelId");
            //convert DateTime to string
            List<DateListDTO> results = new List<DateListDTO>();
            List<DateTime> dateLists = _context.GetDateList(start, count, userEmail, channelId);
            foreach (DateTime date in dateLists)
            {
                results.Add(new DateListDTO()
                {
                    date = date.ToString("yyyy-MM-dd"),
                    count = await commentRepository.GetUnreadCommentsCount(user.Email, groupId, userEmail, date.ToString("yyyy-MM-dd"))
                });
            }
            return results;
        }

        /*
            type=0: Daily
            type=1: Weekly
            type=2: Monthly
        */
        [Route("getReview")]
        [HttpPost]
        public async Task<Object> GetReview(int type = 0)
        {
            var user = await _userManager.GetUserAsync(User);
            Object result = null;
            switch (type)
            {
                case 0:
                    result = _context.WordClouds.Where(x => x.Email == user.Email)
                        .GroupBy(x => new { DayOfYear = x.LogDate.Value.DayOfYear, Word = x.Word })
                        .Select(x => new
                        {
                            Word = x.First().Word,
                            Day = x.First().LogDate.Value.Day,
                            Month = x.First().LogDate.Value.Month,
                            Year = x.First().LogDate.Value.Year,
                            LogDate = x.First().LogDate,
                            Count = x.Sum(xs => xs.Count)
                        })
                        .OrderBy(x => x.LogDate).ThenBy(x => x.Word)
                        .ToList();
                    return result;
                case 1:
                    result = _context.WordClouds.Where(x => x.Email == user.Email)
                        .GroupBy(x => new { WeekOfYear = x.LogDate.Value.DayOfYear / 7, Word = x.Word, Year = x.LogDate.Value.Year })
                        .Select(x => new
                        {
                            Word = x.First().Word,
                            Week = x.First().LogDate.Value.DayOfYear / 7,
                            Year = x.First().LogDate.Value.Year,
                            Count = x.Sum(xs => xs.Count)
                        })
                        .OrderBy(x => x.Year).ThenBy(x => x.Week).ThenBy(x => x.Word)
                        .ToList();
                    return result;
                case 2:
                    result = _context.WordClouds.Where(x => x.Email == user.Email)
                        .GroupBy(x => new { Month = x.LogDate.Value.Month, Year = x.LogDate.Value.Year, Word = x.Word })
                        .Select(x => new
                        {
                            Word = x.First().Word,
                            Month = x.First().LogDate.Value.Month,
                            Year = x.First().LogDate.Value.Year,
                            Count = x.Sum(xs => xs.Count)
                        })
                        .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Word)
                        .ToList();
                    return result;
            }
            return null;
        }
        
    }
}
