using System;

namespace PeterPedal;

class Notifier
{
    public void SendSms(string phone, String message)
    {
        Console.WriteLine("SMS to " + phone + ": " + message);
    }

    public void LeaveVoicemail(string phone)
    {
        Console.WriteLine($"Voicemail left for {phone}: please call us back regarding your bike.");
    }
}