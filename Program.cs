using SignalRChat.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// SignalR
builder.Services.AddSignalR();

// Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    //options.Configuration = builder.Configuration.GetConnectionString("MyRedisConStr");
    options.Configuration = "127.0.0.1:6379,password=123,ssl=False,abortConnect=False";
    options.InstanceName = "SampleInstance";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");

app.Run();
