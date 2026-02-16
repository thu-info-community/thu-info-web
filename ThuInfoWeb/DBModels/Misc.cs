using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

/// <summary>
///     This should be only one record in database.
/// </summary>
public class Misc
{
    [Column(IsPrimary = true)]
    public int Id { get; init; } = 1;

    /// <summary>
    ///     The url data of WeChat group qrcode.
    /// </summary>
    [Column(StringLength = -1, IsNullable = false)]
    public string QrCodeContent { get; init; } = string.Empty;

    /// <summary>
    ///     The url of Apk.
    /// </summary>
    [Column(StringLength = -1, IsNullable = false)]
    public string ApkUrl { get; init; } = string.Empty;

    /// <summary>
    ///     The interface version of new school card.
    /// </summary>
    [Column(IsNullable = false)]
    public int CardIVersion { get; init; }

    /// <summary>
    ///     Updated school calendar year.
    /// </summary>
    /// <remarks>
    ///    To update, download school calendar from info and manually upload it to the server.
    /// </remarks>
    [Column(IsNullable = false)]
    public int SchoolCalendarYear { get; init; }
}
