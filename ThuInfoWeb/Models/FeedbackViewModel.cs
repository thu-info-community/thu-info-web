﻿using FreeSql.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace ThuInfoWeb.Models
{
    public class FeedbackViewModel
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        [Required]
        public string OS { get; set; }
        [Required]
        public string AppVersion { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Reply { get; set; } = string.Empty;
        public string ReplyerName { get; set; } = string.Empty;
    }
}