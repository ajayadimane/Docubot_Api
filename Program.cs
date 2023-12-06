using DocuBot_Api.Context;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Irony.Ast;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();
builder.Services.AddDbContext<DocubotDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("myconn")));

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiHost:BaseUrl"]), Timeout = TimeSpan.FromMinutes(30) });

builder.Services.AddConnections();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("*") // Replace with your React app's URL
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});






var app = builder.Build();
app.UsePathBase(new PathString("/docubot"));



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//if (app.Environment.isproduction())
//{
//    app.useswagger();
//    app.useswaggerui();
//}


app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
