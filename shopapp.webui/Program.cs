var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var serviceProvider = builder.Services.BuildServiceProvider();
var configuration = serviceProvider.GetRequiredService<IConfiguration>();

var app = builder.Build();
startup.Configure(app,builder.Environment,configuration);


 