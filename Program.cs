using Telegram.Bot;
using NLog;
using System.Configuration;
using System.Globalization;
using NLog.Targets;

namespace ClientBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var _config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "${basedir}/file.txt" };
            _config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = _config;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Запуск программы");
            logger.Info("Установка подключения к Телеграмм");
            string botToken = "6750292932:AAHj1dabVpBl2VTyu21O3G0nAQOvB3F3Q70";
            TelegramBotClient botClient = new TelegramBotClient(botToken);           
            var client = (TelegramBotClient)botClient;

            logger.Info("Считывание установок таймера проверок");
            string checkPoints = ConfigurationManager.AppSettings.Get("timePointsCheck");
            logger.Info(checkPoints);
            string[] checkPoinsArray = checkPoints.Split(", ");
            List<DateTime> checkPointsList = new List<DateTime>();

            foreach (string point in checkPoinsArray)
            {
                DateTime dateTime = DateTime.ParseExact(point, "H:mm", CultureInfo.InvariantCulture);
                checkPointsList.Add(dateTime);
            }

            DateTime time = DateTime.Now;

            while (true)
            {
                foreach (DateTime checkPoint in checkPointsList)
                {
                    if (time.Hour == checkPoint.Hour && time.Minute == checkPoint.Minute)
                    {
                        logger.Info("Совпадение текущего времени с таймером проверок");
                        //Считываем дату и время последней проверки
                        string lastCheck = ConfigurationManager.AppSettings.Get("lasrCheckDateTime");
                        logger.Info("Время последней проверки: " + lastCheck);
                        DateTime lastCheckDateTime = DateTime.Parse(ConfigurationManager.AppSettings.Get("lasrCheckDateTime"));

                        logger.Info("Начало проверки наличия ошибок");
                        string errorMessages = $"Application:\n {Errors.ErrorsApp(lastCheckDateTime)}\n\n\n\nSystem:\n {Errors.ErrorsSys(lastCheckDateTime)}";

                        logger.Info("Обновление времени последней проверки");
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        AppSettingsSection appSettings = (AppSettingsSection)config.GetSection("appSettings");
                        appSettings.Settings["lasrCheckDateTime"].Value = time.ToString();
                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");

                        
                        
                        if (Errors.ErrorsApp(lastCheckDateTime) != "")
                        {
                            logger.Info("Найдены новые ошибки Application. Отправка сообщения с ошибками.");
                            await client.SendTextMessageAsync("@ErrorMes", $"Application:\n {Errors.ErrorsApp(lastCheckDateTime)}");
                        }

                        if (Errors.ErrorsSys(lastCheckDateTime) != "")
                        {
                            logger.Info("Найдены новые ошибки System. Отправка сообщения с ошибками.");
                            await client.SendTextMessageAsync("@ErrorMes", $"System:\n {Errors.ErrorsSys(lastCheckDateTime)}");
                        }
                        if (Errors.ErrorsApp(lastCheckDateTime) == "" && Errors.ErrorsSys(lastCheckDateTime) == "")
                        {
                            logger.Info("Проверка окончена. Новых ошибок не обнаружено.");
                            await client.SendTextMessageAsync("@ErrorMes", "Новых ошибок нет");
                        }
                        break;
                    }
                }
                Thread.Sleep(30000);
                time = DateTime.Now;
            }

        }

        
    }
}



