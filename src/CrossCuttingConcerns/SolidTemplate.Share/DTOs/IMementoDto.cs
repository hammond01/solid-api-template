namespace SolidTemplate.Share.DTOs;

public interface IMementoDto
{
    void SaveState();
    void RestoreState();
    void ClearState();
}
