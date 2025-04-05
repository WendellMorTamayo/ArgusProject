using Argus.Sync.Data.Models;
using Argus.Sync.Extensions;
using ArgusProject.Models;
using Microsoft.AspNetCore.Builder;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCardanoIndexer<TestDbContext>(builder.Configuration);
builder.Services.AddReducers<TestDbContext, IReducerModel>(builder.Configuration);

WebApplication app = builder.Build();

app.Run();