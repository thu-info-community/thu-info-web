using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    public class Request
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public DateTime Time { get; set; }
        public uint Ip { get; set; }
    }
}
