using FreeSql.DataAnnotations;
namespace ThuInfoWeb.DBModels
{
    public class Announce
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Cotent { get; set; }
        public string Author { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
