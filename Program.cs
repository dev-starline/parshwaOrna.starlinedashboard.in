using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using SL_Bullion.Constant;
using SL_Bullion.Controllers;
using SL_Bullion.DAL;
using SL_Bullion.Repositories;
using SL_Bullion.WebAPI;
using StackExchange.Redis;
using System.Configuration;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddDbContext<BullionDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(650);

});

builder.Services.AddControllersWithViews().AddNToastNotifyNoty(new NotyOptions
{
    Layout = "bottomRight",
    Theme = "sunset",
    ProgressBar =true,
    Timeout = 5000,
});
builder.Services.AddTransient<ApplicationConstant>();
builder.Services.AddTransient<MessageConstant>();
builder.Services.AddTransient<ResponseMessage>();
builder.Services.AddTransient<ApiService>();
builder.Services.AddTransient<AdminService>();
builder.Services.AddScoped<SqlService>();
builder.Services.AddScoped<BullionService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowOrigin",
        policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("allowedOrigins").Get<string[]>();
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:issuer"],
        ValidAudience = builder.Configuration["JWT:audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:key"]))
    };
});
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowOrigin");
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
app.UseSession();
app.UseAuthorization();
app.UseNToastNotify();



app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Symbol}/{action=List}/{id?}");

app.MapFallbackToFile("index.html");




app.Use(async (context, next) =>
{
    string path = context.Request.Path.Value?.ToLower() ?? "";


    if (path.StartsWith("/admin"))
    {
        string? session = context.Session.GetString("role");

        var controller = context.Request.RouteValues["controller"]?.ToString()?.ToLower();
        var action = context.Request.RouteValues["action"]?.ToString()?.ToLower();

        if (controller == "master" && action != "login" && string.IsNullOrEmpty(session))
        {
            context.Response.Redirect("/admin/Master/Login");
            return;
        }
        else if (controller != "bullion" && controller != "terminal" && controller != "server" && !path.Contains("/login"))
        {
            if (string.IsNullOrEmpty(session))
            {
                context.Response.Redirect("/admin/Login");
                return;
            }
        }
    }

    await next();
});
app.Run();
