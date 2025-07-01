using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace UrlShortener.Api.Models
{
    public class ShortUrl
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string OriginalUrl { get; set; }
        public string ShortCode { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}