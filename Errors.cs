using System.Diagnostics;


namespace ClientBot
{
    internal class Errors
    {
        internal static string ErrorsApp(DateTime lastCheck)
        {
            EventLog eventLogApp = new EventLog("Application");
            EventLogEntryCollection entriesApp = eventLogApp.Entries;

            string totalInfo = "";
            foreach (EventLogEntry entry in entriesApp)
            {
                // Проверяем, является ли запись ошибкой и произошла ли она после startTime
                if (entry.EntryType == EventLogEntryType.Error && entry.TimeGenerated > lastCheck)
                {
                    string errorMessages = $"Дата: {entry.TimeGenerated}\nСобытие: {entry.Source}\nОписание: {entry.Message}\n------------------\n\n";

                    totalInfo += errorMessages;
                }
            }
            return totalInfo;
        }

        internal static string ErrorsSys(DateTime lastCheck)
        {
            EventLog eventLogSys = new EventLog("System");
            EventLogEntryCollection entriesSys = eventLogSys.Entries;

            string totalInfo = "";
            foreach (EventLogEntry entry in entriesSys)
            {
                // Проверяем, является ли запись ошибкой и произошла ли она после startTime
                if (entry.EntryType == EventLogEntryType.Error && entry.TimeGenerated > lastCheck)
                {
                    string errorMessages = $"Дата: {entry.TimeGenerated}\nСобытие: {entry.Source}\nОписание: {entry.Message}\n------------------\n\n";

                    totalInfo += errorMessages;
                }
            }
            return totalInfo;

        }
    }
}
