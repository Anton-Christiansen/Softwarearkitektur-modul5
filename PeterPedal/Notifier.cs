using System;

namespace PeterPedal;

/// <summary>
/// Acts as a phone notifying service
/// </summary>
public class Notifier
{
    /// <summary>
    /// Sends the message to the phone number
    /// </summary>
    /// <param name="phone">The phone number to notify</param>
    /// <param name="message">The message being notified</param>
    public void SendSms(string phone, string message)
    {
        Console.WriteLine("SMS to " + phone + ": " + message);
    }

    
    /// <summary>
    /// Will leave a voicemail to the specified phone number
    /// </summary>
    /// <param name="phone">The phone number</param>
    public void LeaveVoicemail(string phone)
    {
        Console.WriteLine($"Voicemail left for {phone}: please call us back regarding your bike.");
    }
}