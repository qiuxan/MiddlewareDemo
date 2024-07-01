using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation($"ClientName HttpHeader in Middleware 1 {context.Request.Headers["ClientName"]}");
    logger.LogInformation($"Add a ClientName HttpHeader in Middleware 1");

    context.Request.Headers.TryAdd("ClientName", "MyClient");
    logger.LogInformation("My Middleware 1 - Before");
    await next(context);
    logger.LogInformation("My Middleware 1 - After");
    logger.LogInformation($"Response StatusCode in Middleware 1:{ context.Response.StatusCode}");
});

//app.Use(async (context, next) =>
//{
//    var logger = app.Services.GetRequiredService<ILogger<Program>>();

//    logger.LogInformation($"ClientName HttpHeader in Middleware 2 {context.Request.Headers["ClientName"]}");
//    logger.LogInformation("My Middleware 2 - Before");
//    await next(context);
//    logger.LogInformation("My Middleware 2 - After");
//    logger.LogInformation($"Response StatusCode in Middleware 1:{context.Response.StatusCode}");
//});

app.Map("/lottery", app => {
    var random = new Random();
    var luckyNumber = random.Next(1, 6);
    app.UseWhen(context => context.Request.QueryString.Value == $"?{luckyNumber.ToString()}", app => {
        app.Run(async context => {
            await context.Response.WriteAsync($"Congratulations! You won the lottery! Lucky number is {luckyNumber}");
        });
    });

    app.UseWhen(context => string.IsNullOrWhiteSpace(context.Request.QueryString.Value), app => { 
        app.Use(async (context, next) =>
        {
            var number = random.Next(1, 6);
            context.Request.Headers.TryAdd("number", number.ToString());
            await next(context);
        });

        app.UseWhen(context => context.Request.Headers["number"] == luckyNumber.ToString(), app => { 
            app.Run(async context => {
                await context.Response.WriteAsync($"Congratulations! You won the lottery! Lucky number is {luckyNumber}");
            });
        });
    });
    app.Run(async context => {
        var number = "";
        if(context.Request.QueryString.HasValue)
        { 
           number = context.Request.QueryString.Value?.Replace("?","");
        }
        else
        {
            number = context.Request.Headers["number"];
        }
        await context.Response.WriteAsync($"Sorry! You lost the lottery! Lucky number is {luckyNumber} and your number is {number}");
    });
});

app.Run(async context => {
    await context.Response.WriteAsync($"Use the /lottery URL to play. You can choose your number with the format / lottery ? 1.");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
