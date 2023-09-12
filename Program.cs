using NLog;
string path = Directory.GetCurrentDirectory() + "/nlog.config";
// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();
