namespace BankingApp.Contracts.Features.UserProfile.Dtos;

/// <summary>
///     Represents the safe session details exposed to profile clients.
/// </summary>
public class SessionDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the device information for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? DeviceInfo { get; set; }

    /// <summary>
    ///     Gets or sets the browser used for the session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? Browser { get; set; }

    /// <summary>
    ///     Gets or sets the IP address from which the session was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string? IpAddress { get; set; }

    /// <summary>
    ///     Gets or sets the date and time of the last activity in this session.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when this session was created.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public DateTime CreatedAt { get; set; }
}
