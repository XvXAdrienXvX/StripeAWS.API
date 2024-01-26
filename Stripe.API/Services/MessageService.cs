using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace Stripe.API.Services
{
    public class MessageService : IMessageService
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IConfiguration _configuration;
        public MessageService(IAmazonSimpleNotificationService snsClient, IConfiguration configuration)
        {
            _snsClient = snsClient;
            _configuration = configuration;
        }
        public async Task SendMessage(string messageBody)
        {
            try
            {
                string topicArn = _configuration.GetValue<string>("Stripe:topicARN");

                PublishRequest publishRequest = new PublishRequest
                {
                    TopicArn = topicArn,
                    Message = messageBody
                };

                PublishResponse publishResponse = await _snsClient.PublishAsync(publishRequest);
            }
            catch (Exception ex)
            {
                LambdaLogger.Log("Error: " + ex);
            }
        }
    }
}
