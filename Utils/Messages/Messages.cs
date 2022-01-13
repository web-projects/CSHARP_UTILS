using static System.ExtensionMethods;

namespace Utils.Messages
{
    public class Messages
    {
        public enum ConsoleMessages
        {
            [StringValue("DEVICE: FIRMARE VERSION ")]
            DeviceFirmwareVersion,
            [StringValue("DEVICE: ADE-")]
            DeviceADEKey,
        }
    }
}
