namespace ThuInfoWeb.DBModels
{
    /// <summary>
    /// This should be only one record in database.
    /// </summary>
    public class Misc
    {
        /// <summary>
        /// The url data of wechat group qrcode.
        /// </summary>
        public string QrCodeContent { get; set; }
        /// <summary>
        /// The url of Apk.
        /// </summary>
        public string ApkUrl { get; set; }
    }
}
