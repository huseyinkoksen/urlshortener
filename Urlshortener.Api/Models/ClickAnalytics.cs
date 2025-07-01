using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace UrlShortener.Api.Models
{
    public class ClickAnalytics
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ShortUrlId { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}