namespace Apps.MistralAI.Models.Requests;

public class MessageRequest
{
    public string Role { get; set; }

    public string Content { get; set; }

    public MessageRequest()
    {
        Role = string.Empty;
        Content = string.Empty;
    }

    public MessageRequest(string role, string content)
    {
        Role = role;
        Content = content;
    }
}