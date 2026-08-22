using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using LoanApplicationPlatform.API.Repositories;
using LoanApplicationPlatform.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Allow the local frontend dev server (Vite) to call the API cross-origin.
const string FrontendCorsPolicy = "FrontendDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction =>
{
    setupAction.AddSecurityDefinition("LoanAppApiBearerAuth", new OpenApiSecurityScheme()
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid token to access this API"
    });

    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "LoanAppApiBearerAuth"
                }
            }, new List<string>()
        }
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireReviewerRole", policy => policy.RequireRole("Reviewer", "Admin"));
    options.AddPolicy("RequireApproverRole", policy => policy.RequireRole("Approver", "Admin"));
    options.AddPolicy("RequireApplicantRole", policy => policy.RequireRole("Applicant"));
});

builder.Services.AddHttpContextAccessor();

// Register Repositories
builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
builder.Services.AddScoped<ITreasuryRepository, TreasuryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILoanApplicationService, LoanApplicationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddDbContext<LoanApplicationPlatform.API.DbContexts.LoanApplicationPlatformContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<LoanApplicationPlatform.API.Profiles.LoanApplicationProfile>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LoanApplicationPlatform.API.DbContexts.LoanApplicationPlatformContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();

	// Open the Swagger UI in the default browser once the server is listening.
	// `dotnet run` ignores launchSettings' `launchBrowser`, so we do it here.
	app.Lifetime.ApplicationStarted.Register(() =>
	{
		var address = app.Services
			.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
			.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()
			?.Addresses.FirstOrDefault();

		if (string.IsNullOrEmpty(address)) return;

		var url = $"{address.Replace("://+", "://localhost").Replace("://[::]", "://localhost").Replace("://0.0.0.0", "://localhost")}/swagger";
		try
		{
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
		}
		catch { /* opening the browser is best-effort; ignore failures */ }
	});
}

app.UseCors(FrontendCorsPolicy);

// Skip HTTPS redirection in development so the Vite dev server can call the
// plain-HTTP endpoint (http://localhost:5131). A 307 redirect to HTTPS strips
// CORS headers on preflight (OPTIONS) requests, which breaks the browser.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
