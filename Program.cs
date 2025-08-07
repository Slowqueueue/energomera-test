using EnergomeraTest.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<GeodeticCalculator>();
builder.Services.AddSingleton<KmlDataService>();

// ������������ ����� � ������
builder.Configuration.AddJsonFile("appsettings.json");

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();