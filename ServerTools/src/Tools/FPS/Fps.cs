﻿
namespace ServerTools
{
    class Fps
    {
        public static bool IsEnabled = false;
        public static int Set_Target = 60;
        public static string Command_fps = "fps";

        public static void FPS(ClientInfo _cInfo)
        {
            string _fps = GameManager.Instance.fps.Counter.ToString();
            Phrases.Dict.TryGetValue("Fps1", out string _phrase1);
            _phrase1 = _phrase1.Replace("{Fps}", _fps);
            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
        }

        public static void SetTarget()
        {
            SdtdConsole.Instance.ExecuteSync(string.Format("SetTargetFps {0}", Set_Target), null);
        }
    }
}
