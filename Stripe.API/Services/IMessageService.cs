namespace Stripe.API.Services
{
    public interface IMessageService
    {
        Task SendMessage(string messageBody);
    }
}
