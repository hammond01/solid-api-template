namespace SolidTemplate.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options
            => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
        builder.Services.RegisterPolicy();
        builder.Services.ApplicationConfigureServices();
        builder.Services.AddPersistence(builder.Configuration.GetConnectionString("SQL")!);

        //Register appSetting configuration
        builder.Services.Configure<IdentityConfig>(builder.Configuration.GetSection(IdentityConfig.ConfigName));

        // Register infrastructure services
        builder.Services.AddDateTimeProvider();

        // Register Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var audience = builder.Configuration["IdentityConfig:AUDIENCE"];
        var issUser = builder.Configuration["IdentityConfig:ISSUER"];
        var key = builder.Configuration["IdentityConfig:SECRET"];

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidIssuer = issUser,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
            };
        });

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = PasswordPolicy.RequireDigit;
            options.Password.RequiredLength = PasswordPolicy.RequiredLength;
            options.Password.RequireNonAlphanumeric = PasswordPolicy.RequireNonAlphanumeric;
            options.Password.RequireUppercase = PasswordPolicy.RequireUppercase;
            options.Password.RequireLowercase = PasswordPolicy.RequireLowercase;
            //options.Password.RequiredUniqueChars = 6;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 10;
            options.Lockout.AllowedForNewUsers = true;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        using (var serviceScope =
               ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var databaseInitializer = serviceScope.ServiceProvider.GetService<IDatabaseInitializer>();
            databaseInitializer?.SeedAsync().Wait();
        }
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
