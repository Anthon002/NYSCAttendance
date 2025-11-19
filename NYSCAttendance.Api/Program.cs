using NYSCAttendance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.RegisterPersistence(builder.Configuration);
builder.Services.RegisterIdentity();
builder.Services.RegisterCors();
builder.Services.RegisterAuthentication(builder.Configuration);
builder.Services.RegisterAuthorization();
builder.Services.RegisterServices(builder.Configuration);
builder.Services.AddMediator(x => x.ServiceLifetime = ServiceLifetime.Transient);
builder.Services.RegisterSwagger();
builder.Services.AddControllers();
builder.Services.AddRateLimiter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "MyArea", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllers();
app.Run();
