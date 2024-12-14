using Services;
using Configuration;
using DbRepos;
using DbContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();


//use multiple Secret sources, Database connections and their respective DbContexts
builder.Configuration.AddApplicationSecrets("../Configuration/Configuration.csproj");
 builder.Services.AddDatabaseConnections(builder.Configuration);
builder.Services.AddDatabaseConnectionsDbContext();


#region Setup the Dependency service
builder.Services.AddSingleton<ILoggerProvider, InMemoryLoggerProvider>();
builder.Services.AddScoped<FriendsDbRepos>();
builder.Services.AddScoped<IFriendsService, FriendsServiceDb>();

#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
