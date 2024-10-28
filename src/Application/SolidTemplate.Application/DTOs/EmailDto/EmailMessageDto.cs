namespace SolidTemplate.Application.DTOs.EmailDto;

public class EmailMessageDto
{
    public List<EmailAddressDto> ToAddresses { get; set; } = [];
    public List<EmailAddressDto> FromAddresses { get; set; } = [];
    public List<EmailAddressDto> BccAddresses { get; set; } = [];
    public List<EmailAddressDto> CcAddresses { get; set; } = [];
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string? VendorNumber { get; set; } = null;
    public string? PoNumber { get; set; } = null;
    public bool IsHtml { get; set; } = true;
    public int TemplateId { get; set; }
}
