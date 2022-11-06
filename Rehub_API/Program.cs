using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Rehub.DataAccess;
using Rehub.DataAccess.DbInitializer;
using Rehub.DataAccess.Repository.IRepository;
using Rehub.DataAccess.Repository;
using Rehub.Utility;

var builder = WebApplication.CreateBuilder(args);

// variable to hold the CORS policy
string MyAllowedSpecificOrigins = "_myAllowSpecificOrigins";

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowedSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<UnitOfWork>();


builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowedSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
