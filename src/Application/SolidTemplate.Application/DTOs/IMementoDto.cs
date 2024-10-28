namespace SolidTemplate.Domain.DTOs;

public interface IMementoDto
{
    void SaveState();
    void RestoreState();
    void ClearState();
}
