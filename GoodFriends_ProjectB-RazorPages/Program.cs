using Services;
using Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();


//use multiple Secret sources, Database connections and their respective DbContexts
object value = builder.Configuration.AddApplicationSecrets("../Configuration/Configuration.csproj");
object value1 = builder.Services.AddDatabaseConnections(builder.Configuration);

#region Setup the Dependency service
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
