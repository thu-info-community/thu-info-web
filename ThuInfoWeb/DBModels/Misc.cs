using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    /// <summary>
    /// This should be only one record in database.
    /// </summary>
    public class Misc
    {
        [Column(IsPrimary = true)]
        public int Id { get; set; } = 1;
        /// <summary>
        /// The url data of wechat group qrcode.
        /// </summary>
        [Column(StringLength = -1)]
        public string QrCodeContent { get; set; }
        /// <summary>
        /// The url of Apk.
        /// </summary>
        [Column(StringLength = -1)]
        public string ApkUrl { get; set; }
    }
}
