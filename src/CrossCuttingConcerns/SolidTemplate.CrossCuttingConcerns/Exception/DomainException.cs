namespace SolidTemplate.CrossCuttingConcerns.Exception;

public class DomainException : System.Exception
{

    public DomainException(string description)
    {
        Description = description;
    }

    public DomainException(string description, string message) : base(message)
    {
        Description = description;
    }

    public DomainException(string description, string message, System.Exception innerException) : base(message, innerException)
    {
        Description = description;
    }
    public string Description { get; }
}
