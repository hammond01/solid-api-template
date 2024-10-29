namespace SolidTemplate.Shared.DTOs;

public interface IMementoDto
{
    void SaveState();
    void RestoreState();
    void ClearState();
}
