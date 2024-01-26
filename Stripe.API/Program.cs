using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Stripe;
using Stripe.API.Services;
using Stripe.Checkout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
builder.Services.AddSingleton<IMessageService, MessageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapPost("/webhook", async (HttpRequest request, IMessageService messageService) =>
{
    var json = await new StreamReader(request.Body).ReadToEndAsync();
    string signingSecret = builder.Configuration.GetValue<string>("Stripe:signingSecret");
    try
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            request.Headers["Stripe-Signature"],
            signingSecret,
            throwOnApiVersionMismatch: false
        );
        // Handle the event
        if (stripeEvent.Type == Events.CheckoutSessionCompleted)
        {
            var sessionEvent = stripeEvent.Data.Object as Session;

            if (sessionEvent != null)
            {
                var sessionId = sessionEvent.Id;
                var customerEmail = sessionEvent.CustomerEmail;

                await messageService.SendMessage("CHECKOUT_COMPLETED");
                LambdaLogger.Log("CHECKOUT_COMPLETED");
                LambdaLogger.Log("Customer Email: " + customerEmail);
            }
        }

        return Results.Ok(new { Message = "success" });
    }
    catch (StripeException e)
    {
        return Results.BadRequest(new { Error = e.Message });
    }
    catch (Exception e)
    {
        return Results.BadRequest(new { Error = $"{e}" });
    }
});

app.Run();