using ArtiaVet.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();

builder.Services.AddScoped<IRepositorioTest, RepositorioTest>();
builder.Services.AddScoped<IRepositorioDropdowns, RepositorioDropdowns>();
builder.Services.AddScoped<IRepositorioInventario, RepositorioInventario>();
builder.Services.AddScoped<IRepositorioCalendario, RepositorioCalendario>();
builder.Services.AddScoped<IRepositorioCitas, RepositorioCitas>();
builder.Services.AddScoped<IRepositorioCalendarioVeterinario, RepositorioCalendarioVeterinario>();
builder.Services.AddScoped<IRepositorioCitasVeterinario, RepositorioCitasVeterinario>();
builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddScoped<IRepositorioFacturas, RepositorioFacturas>();

builder.Services.AddSession();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Duración de la sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ArtiaVet.Session";
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
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

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var repositorioTest = scope.ServiceProvider.GetRequiredService<IRepositorioTest>();
    await repositorioTest.TestConnectionAsync();
}

app.Run();
