﻿

using System;

namespace ServerTools
{
    class BreakReminder
    {
        public static bool IsEnabled = false;
        public static string Message = "It has been {Time} minutes since the last break reminder. Stretch and get some water.", Delay = "60";

        private static string EventDelay = "";
        private static DateTime time = new DateTime();

        public static void SetDelay(bool _loading)
        {
            if (EventDelay != Delay || _loading)
            {
                if (EventSchedule.Schedule.ContainsKey("BreakReminder") && !EventSchedule.Expired.Contains("BreakReminder"))
                {
                    EventSchedule.RemoveFromSchedule("BreakReminder");
                }
                EventDelay = Delay;
                if (Delay.Contains(",") && Delay.Contains(":"))
                {
                    string[] times = Delay.Split(',');
                    for (int i = 0; i < times.Length; i++)
                    {
                        string[] timeSplit1 = times[i].Split(':');
                        int.TryParse(timeSplit1[0], out int hours1);
                        int.TryParse(timeSplit1[1], out int minutes1);
                        time = DateTime.Today.AddHours(hours1).AddMinutes(minutes1);
                        if (DateTime.Now < time)
                        {
                            EventSchedule.AddToSchedule("BreakReminder", time);
                            return;
                        }
                    }
                    string[] timeSplit2 = times[0].Split(':');
                    int.TryParse(timeSplit2[0], out int hours2);
                    int.TryParse(timeSplit2[1], out int minutes2);
                    time = DateTime.Today.AddDays(1).AddHours(hours2).AddMinutes(minutes2);
                    EventSchedule.AddToSchedule("BreakReminder", time);
                    return;
                }
                else if (Delay.Contains(":"))
                {
                    string[] timeSplit3 = Delay.Split(':');
                    int.TryParse(timeSplit3[0], out int hours3);
                    int.TryParse(timeSplit3[1], out int minutes3);
                    time = DateTime.Today.AddHours(hours3).AddMinutes(minutes3);
                    if (DateTime.Now < time)
                    {
                        EventSchedule.AddToSchedule("BreakReminder", time);
                    }
                    else
                    {
                        time = DateTime.Today.AddDays(1).AddHours(hours3).AddMinutes(minutes3);
                        EventSchedule.AddToSchedule("BreakReminder", time);
                    }
                    return;
                }
                else
                {
                    if (int.TryParse(Delay, out int delay))
                    {
                        time = DateTime.Now.AddMinutes(delay);
                        EventSchedule.AddToSchedule("BreakReminder", time);
                    }
                    else
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Invalid Break_Time detected. Use a single integer, 24h time or multiple 24h time entries"));
                        Log.Out(string.Format("[SERVERTOOLS] Example: 120 or 03:00 or 03:00, 06:00, 09:00"));
                    }
                    return;
                }
            }
        }

        public static void Exec()
        {
            if (ConnectionManager.Instance.ClientCount() > 0)
            {
                Message = Message.Replace("{Time}", Delay.ToString());
                ChatHook.ChatMessage(null, Config.Chat_Response_Color + Message + "[-]", -1, Config.Server_Response_Name, EChatType.Global, null);
            }
        }
    }
}
