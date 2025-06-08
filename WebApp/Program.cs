 using System.Net.Http.Headers;
using WebApp.ApiClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddHttpClient<ProductApiClient>(client =>
{
    client.BaseAddress = new Uri("https://oicar-team-11-ajugostitelj-11.onrender.com/api/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<OrderApiClient>(client =>
{
    client.BaseAddress = new Uri("https://oicar-team-11-ajugostitelj-11.onrender.com/api/");
});
builder.Services.AddHttpClient<PaymentApiClient>(client =>
{
    client.BaseAddress = new Uri("https://oicar-team-11-ajugostitelj-11.onrender.com/api/");
});

builder.Services.AddHttpClient<CategoriesApiClient>(client =>
{
    client.BaseAddress = new Uri("https://oicar-team-11-ajugostitelj-11.onrender.com/api/");
});

builder.Services.AddHttpClient<ReviewApiClient>(client =>
{
    client.BaseAddress = new Uri("https://oicar-team-11-ajugostitelj-11.onrender.com/api/");
});

builder.Services.AddHttpClient<TablesApiClient>(client =>
{
    client.BaseAddress = new Uri("https://oicar-team-11-ajugostitelj-11.onrender.com/api/");
});


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.WithOrigins("http://localhost:5022")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});


var app = builder.Build();


app.UseCors("AllowAll");
app.UseCors("AllowLocalhost");
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");


app.Run();
