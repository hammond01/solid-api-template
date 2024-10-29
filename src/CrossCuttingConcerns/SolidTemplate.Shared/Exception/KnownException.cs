using Microsoft.Extensions.Localization;
namespace SolidTemplate.Shared.Exception;

public abstract class KnownException : System.Exception
{
    public KnownException(string message)
        : base(message)
    {
        Key = message;
    }

    public KnownException(string message, System.Exception? innerException)
        : base(message, innerException)
    {
        Key = message;
    }

    public KnownException(LocalizedString message)
        : base(message.Value)
    {
        Key = message.Name;
    }

    public KnownException(LocalizedString message, System.Exception? innerException)
        : base(message.Value, innerException)
    {
        Key = message.Name;
    }

    public string? Key { get; set; }
}
