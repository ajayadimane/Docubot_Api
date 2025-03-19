using DocuBot_Api.Context;
using DocuBot_Api.Models.Helpers;
using DocuBot_Api.Models_Pq;
using DocuBot_Api.Models_Pq.ResponseModels;
using DocuBot_Api.Rating_Models;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Irony.Ast;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddDbContext<DocubotDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("myconn")));

builder.Services.AddDbContext<RatingContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DocubotContext")));

builder.Services.Configure<BankResponseConfig>(builder.Configuration.GetSection("Banks"));

builder.Services.Configure<BankConfiguration>(builder.Configuration.GetSection("BankConfigurations"));

builder.Services.Configure<TransBankConfig>(builder.Configuration.GetSection("TransBankConfig"));

builder.Services.AddSingleton<KvalRepository>();



builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiHost:BaseUrl"]), Timeout = TimeSpan.FromMinutes(30) });


builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


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



// configure the http request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//if (app.Environment.IsProduction())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}


app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
