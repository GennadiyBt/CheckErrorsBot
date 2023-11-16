using NLog;
using System.Globalization;
using System.Xml;
using Telegram.Bot;

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
            string botToken = "Bot_token"; //Необходимо заменить на действительное значение токена телеграм-бота
            TelegramBotClient botClient = new TelegramBotClient(botToken);
            var client = (TelegramBotClient)botClient;
            

            string fileName = "user_config.xml";
            

            string? checkPoints;
            string lastCheck;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlNode lastCheckNode = xmlDoc.SelectSingleNode("/Root/lastCheck");
            lastCheck = lastCheckNode.InnerText;
            XmlNode timerNode = xmlDoc.SelectSingleNode("/Root/Timer");
            checkPoints = timerNode.InnerText;

            logger.Info("Считывание установок таймера проверок");            
            logger.Info(checkPoints);
            string[] checkPoinsArray = checkPoints.Split(", ");
            List<DateTime> checkPointsList = new List<DateTime>();           
            logger.Info($"Время последней проверки {lastCheck}");
            foreach (string point in checkPoinsArray)
            {
                DateTime dateTime = DateTime.ParseExact(point, "H:mm", CultureInfo.InvariantCulture);
                checkPointsList.Add(dateTime);
            }

            checkPointsList.Sort();

            DateTime time = DateTime.Now;

            while (true)
            {
                foreach (DateTime checkPoint in checkPointsList)
                {
                    if (time.Hour == checkPoint.Hour && time.Minute == checkPoint.Minute)
                    {
                        
                        logger.Info("Совпадение текущего времени с таймером проверок");
                        lastCheckNode = xmlDoc.SelectSingleNode("/Root/lastCheck");
                        lastCheck = lastCheckNode.InnerText;
                        DateTime lastCheckDateTime = DateTime.Parse(lastCheck);

                        logger.Info("Начало проверки наличия ошибок");
                        string errorMessages = $"Application:\n {Errors.ErrorsApp(lastCheckDateTime)}\n\n\n\nSystem:\n {Errors.ErrorsSys(lastCheckDateTime)}";                       
                        // Установите новое значение для "lastCheck"
                        lastCheck = time.ToString();
                        lastCheckNode.InnerText = lastCheck;

                        // Сохраните изменения в XML файле
                        xmlDoc.Save(fileName);

                        if (Errors.ErrorsApp(lastCheckDateTime) != "")
                        {
                            logger.Info("Найдены новые ошибки Application. Отправка сообщения с ошибками.");
                            await client.SendTextMessageAsync("group_identifikator", $"Application:\n {Errors.ErrorsApp(lastCheckDateTime)}");//Необходимо заменить group_identifikator на идентификатор группы в телеграмм
                        }

                        if (Errors.ErrorsSys(lastCheckDateTime) != "")
                        {
                            logger.Info("Найдены новые ошибки System. Отправка сообщения с ошибками.");
                            await client.SendTextMessageAsync("group_identifikator", $"System:\n {Errors.ErrorsSys(lastCheckDateTime)}");//Необходимо заменить group_identifikator на идентификатор группы в телеграмм
                        }
                        if (Errors.ErrorsApp(lastCheckDateTime) == "" && Errors.ErrorsSys(lastCheckDateTime) == "")
                        {
                            logger.Info($"Проверка окончена. Новых ошибок не обнаружено. Новое значение времени последней проверки  {lastCheck}");                           
                        }
                        break;
                    }
                    
                }
                if (DateTime.Now.TimeOfDay > checkPointsList[^1].TimeOfDay) 
                {
                    logger.Info("Все запланированные проверки выполнены. Закрытие программы.");
                    Environment.Exit(0); 
                }
                Thread.Sleep(30000);
                time = DateTime.Now;
            }

        }
    }
}



