﻿using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    public class Feedback
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }
        [Column(StringLength = -1)]
        public string Content { get; set; }
        public string Contact { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public string AppVersion { get; set; }
        public string OS { get; set; }
        public string PhoneModel { get; set; }
        public string Reply { get; set; } = string.Empty;
        public string ReplierName { get; set; } = string.Empty;
        public DateTime? RepliedTime { get; set; }
    }
}
