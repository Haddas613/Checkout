using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Integrations;
using PaymentGateway.Api.Integrations.Models;
using PaymentGateway.Api.Models.Helpers.Web;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AcquiringBankSettings>(builder.Configuration.GetSection("AcquiringBankSettings"));
builder.Services.AddSingleton<AcquiringBankSimulator, AcquiringBankSimulator>(serviceProvider =>
{
    var acquiringBankConfig = serviceProvider.GetRequiredService<IOptions<AcquiringBankSettings>>();
    var webApiClient = new WebApiClient();
    return new AcquiringBankSimulator(webApiClient, acquiringBankConfig);
});
builder.Services.AddHttpClient<IWebApiClient, WebApiClient>();
builder.Services.AddScoped<IWebApiClient, WebApiClient>();
builder.Services.AddScoped<IAcquiringBank, AcquiringBankSimulator>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
   
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentGateway API", Version = "v1" });
    
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "PaymentGateway.Api.xml");
    c.IncludeXmlComments(xmlFile);
    c.EnableAnnotations(); // This enables the use of annotations like SwaggerOperation
    c.OperationFilter<HideExamplesForStatusCodeFilter>();
});

builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentGateway API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();
app.UseSwagger();


app.Run();

//todo documentation
