namespace AutoNest.Business.Contracts;

public interface IEmailService
{
    Task SendAsync(string recipient, string subject, string html, CancellationToken cancellationToken = default);
}
