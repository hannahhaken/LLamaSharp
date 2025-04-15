namespace ConsoleApp1;

public interface ILlmService
{
    Task<string> GetChatResponse(string prompt);
}