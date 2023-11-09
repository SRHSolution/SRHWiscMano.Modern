// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using SRHNLogConsole;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// NLog.LogManager.Setup().LoadConfiguration(builder => {
//     builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToConsole();
//     builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile(fileName: "file.txt");
// });

var logger = LogManager.GetCurrentClassLogger();

var config = new ConfigurationBuilder()
    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var collection = new ServiceCollection();
collection.AddTransient<Runner>();
collection.AddTransient<Stopper>();
collection.AddLogging(builder => { builder.AddNLog(config);});
var provider = collection.BuildServiceProvider();

var runner1 = provider.GetRequiredService<Runner>();
var Stopper1 = provider.GetRequiredService<Stopper>();
runner1.DoAction("Runner Logging");
Stopper1.DoAction("Stopper Logging");
var servicesCollection = new ServiceCollection()
    .AddTransient<Runner>() // Runner is the custom class
    .AddLogging(loggingBuilder =>
    {
        // configure Logging with NLog
        loggingBuilder.ClearProviders();
        loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
        loggingBuilder.AddNLog(config);
    });


servicesCollection.Configure<AppSettings>(config.GetSection("AppSettings"));

var serviceProvider = servicesCollection.BuildServiceProvider();

var mysetting = serviceProvider.GetRequiredService<IOptions<AppSettings>>();

mysetting.Value.MySetting2 = "richard";
var jsonConfig = JObject.Parse(File.ReadAllText("appsettings.json"));
jsonConfig["AppSettings"] = JObject.Parse(JsonConvert.SerializeObject(mysetting.Value, Formatting.Indented));
File.WriteAllText("appsettings.json", jsonConfig.ToString(Formatting.Indented));


var newSetting = new AppSettings();
config.Bind(newSetting);
var runner = serviceProvider.GetRequiredService<Runner>();

runner.DoAction("Action1");

logger.Info("Test info");

var appconfig = new ConfigurationBuilder()
    .SetBasePath("")
    .AddJsonFile("")
    .Build();




LogManager.Shutdown();


// var logger = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();
// logger.LogInformation("Program has started.");
// Console.ReadKey();
