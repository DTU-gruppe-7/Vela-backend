using Vela.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentation(builder.Configuration)
    .AddInfrastructure(builder.Configuration);
    
var app = builder.Build();
    
app.ConfigurePipeline();
    
app.Run();