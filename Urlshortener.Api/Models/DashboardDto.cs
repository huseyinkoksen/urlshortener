using System;

namespace UrlShortener.Api.Models
{
    public class DashboardSummaryDto
    {
        public int TotalLinks { get; set; }
        public int TotalClicks { get; set; }
        public int ClicksToday { get; set; }
        public int ClicksThisWeek { get; set; }
        public int ClicksThisMonth { get; set; }
        public List<LinkSummaryDto> TopLinks { get; set; } = new();
        public List<DailyClicksDto> DailyClicks { get; set; } = new();
    }

    public class LinkSummaryDto
    {
        public string Id { get; set; }
        public string ShortCode { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalClicks { get; set; }
        public int ClicksToday { get; set; }
        public int ClicksThisWeek { get; set; }
        public int ClicksThisMonth { get; set; }
    }

    public class DailyClicksDto
    {
        public DateTime Date { get; set; }
        public int Clicks { get; set; }
    }

    public class LinkAnalyticsDto
    {
        public string Id { get; set; }
        public string ShortCode { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalClicks { get; set; }
        public List<ClickAnalytics> Clicks { get; set; } = new();
        public List<DailyClicksDto> DailyClicks { get; set; } = new();
    }
} 