using Vela.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);
    
var app = builder.Build();
    
app.ConfigurePipeline();
    
app.Run();