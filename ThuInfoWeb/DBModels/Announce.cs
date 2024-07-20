using FreeSql.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThuInfoWeb.DBModels
{
    public class Announce
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        public string Title { get; set; }

        [Column(StringLength = -1)]
        public string Content { get; set; }

        public string Author { get; set; }

        public DateTime CreatedTime { get; set; }

        [JsonIgnore]
        public bool IsActive { get; set; }

        [Column(StringLength = 10, IsNullable = false)]
        public string VisibleNotAfter { get; set; }

        [Column(StringLength = 30, IsNullable = false)]
        public string VisibleExact { get; set; }
    }
}