using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Utils.ArrayProcessing;
using Utils.Dictionaries;
using Utils.Helper;
using Utils.Methods;
using Utils.TLV;
using static Utils.Messages.Messages;

namespace Utils
{
    class Program
    {
        //static readonly string DecryptedString = @"%B4815881002867896^DOE/DUMB JOHN         ^37829821000123456789?";
        static readonly string DecryptedString = @"%B4815881002861896^DOE/L JOHN            ^2212102356858      00998000000?";
        static readonly string DecryptedArray = "8AD3D8F024F60AC33935333139323335313030343D323530323130313130303831323334353030303F35800000000000";

        const int KeyValueLength = 29;
        const char KeyValuePaddingCharacter = '_';

        static readonly string KeySlotNumber = "00";
        static readonly string SRedCardKSN = "F987654321";

        static Stopwatch stopWatch = new Stopwatch();

        //static void Main(string[] args)
        static async Task Main(string[] args)
        {
            //TestArrayCodedString();
            //TestTagHelper();
            //await CancellationTokenTask(1000);
            //TestStringSplit();
            //TestBoolString();
            //SetDeviceDateTimeStamp(true);
            //SetDeviceDateTimeStamp(false);

            // byte array comparison tests
            //ArrayConversions.PerformComparisonTests();

            //Debug.WriteLine(FormatStringAsRequired(GetConcat("Ajay", "Bijay", "Sanjay")));

            //($"{FormatStringAsRequired($"DEVICE: ADE-{ConfigProd.securityConfigurationObject.KeySlotNumber ?? " ?? "} KEY KSN ")}: {ConfigProd.securityConfigurationObject.SRedCardKSN ?? "[ *** NOT FOUND *** ]"}");
            Debug.WriteLine(AssembleString(true, "??", ConsoleMessages.DeviceADEKey.GetStringValue(), KeySlotNumber, " KEY KSN ") + AssembleString(false, "[ *** NOT FOUND *** ]", " : ", SRedCardKSN));
            Debug.WriteLine(AssembleString(true, "??", ConsoleMessages.DeviceADEKey.GetStringValue(), null, " KEY KSN ") + AssembleString(false, "[ *** NOT FOUND *** ]", " : ", null));

            //Console.WriteLine(FormatStringAsRequired(AssembleString(ConsoleMessages.DeviceADEKey.GetStringValue(), KeySlotNumber, " KEY KSN ")));
            //Debug.WriteLine(FormatStringAsRequired(string.Format("{0}", ConsoleMessages.DeviceADEKey.GetStringValue(), KeySlotNumber)));

            await Task.Delay(10);
        }

        static private void SetDeviceDateTimeStamp(bool uselocalTime)
        {
            DateTimeOffset dateNow = uselocalTime ? DateTimeOffset.Now : DateTimeOffset.UtcNow;
            //string pattern = @"\d";
            //StringBuilder sb = new StringBuilder();

            //// pattern YYYYMMDDHHMMSS
            //foreach (Match m in Regex.Matches(uselocalTime ? DateTimeOffset.UtcNow.ToLocalTime().ToString("u")  : DateTimeOffset.UtcNow.ToUniversalTime().ToString("u"), pattern))
            //{
            //    sb.Append(m);
            //}
            //// reset seconds
            //if (sb.Length == 14)
            //{
            //    sb[12] = '0';
            //    sb[13] = '0';
            //}

            //string timestamp = sb.ToString();
            //Console.WriteLine(string.Format("{0:yyyy}", dateTime));
            string timestamp = string.Format("{0:yyyyMMddHHmmss}", dateNow);


            Console.WriteLine($"{timestamp}");
        }
        
        static void TestBoolString()
        {
            string value = "0000";
            if (!string.IsNullOrEmpty(value) && Convert.ToBoolean(Convert.ToInt32(value)))
            {
                Console.WriteLine("value is: TRUE}");
            }
            else
            {
                Console.WriteLine("value is: FALSE");
            }
        }

        static void TestStringSplit()
        {
            //List<DeviceInformation> devices = new List<DeviceInformation>
            //{
            //    new DeviceInformation()
            //    { 
            //        VipaPackageTag = "sphere.one....blah.blah.blah"
            //    },
            //    new DeviceInformation()
            //    {
            //        VipaPackageTag ="verifone.one....blah.blah.blah"
            //    },
            //    new DeviceInformation()
            //    {
            //        VipaPackageTag = "sphere.two....blah.blah.blah"
            //    }
            //};
            List<DeviceInformation> devices = new List<DeviceInformation>();

            string filename = "sphere.secd...blah.blah.blah";
            string targetSignature = filename.Split(".")[0];

            //List<string> filteredDevices = devices.Where(e => e.Split(".")[0].StartsWith(filename)).ToList();
            List<DeviceInformation> filteredDevices = devices.Where(e => e.VipaPackageTag.StartsWith(targetSignature)).ToList();

            if (filteredDevices == null || filteredDevices.Count == 0)
            {
                Console.WriteLine($"Failed to find device with signature '{targetSignature}'");
            }
            else
            {
                foreach (DeviceInformation device in filteredDevices)
                {
                    Console.WriteLine($"vipa version: {device.VipaPackageTag}");
                }
            }
        }

        static async Task CancellationTokenTask(int ttl)
        {
            CancellationTokenSource shortLivedTokenSource = new CancellationTokenSource(ttl);
            CancellationToken shortLivedToken = shortLivedTokenSource.Token;

            Debug.WriteLine($"START OPERATION WITH TTL={ttl}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                //await Task.Delay(ttl * 5, shortLivedToken);
                //for (int index = 0; index < ttl * 5; index += ttl)
                //{
                //    await Task.Delay(ttl);
                //    shortLivedToken.ThrowIfCancellationRequested();
                //}

                // Setup a task to check for cancellation request
                await Task.Run(() => CheckForCancellationRequest(shortLivedToken));

                // Perform long operation
                await Task.Delay(ttl * 5);

                Debug.WriteLine("OPERATION COMPLETED WITHOUT CANCELLING!");
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine($"TOKEN HAS TIMED OUT WITH MESSAGE='{ex.Message}'");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"EXCEPTION THROWN={e.Message}");
            }
            finally
            {
                Debug.WriteLine("EXITING OPERATION...");
                sw.Stop();
                Debug.WriteLine($"RUNTIME: {sw.Elapsed.TotalMilliseconds} ms");
            }
        }

        static void CheckForCancellationRequest(CancellationToken shortLivedToken)
        {
            //int index = 1;
            for (; ; )
            {
                try
                {
                    //Debug.WriteLine($"OPERATION SENTINEL - PASS={index++}");
                    shortLivedToken.ThrowIfCancellationRequested();
                    Thread.Sleep(10);
                    //  Task.Delay(10) won't work here since an exception is thrown on top of the expected OperationCanceledException
                }
                catch (OperationCanceledException)
                {
                    throw new Exception("OperationCanceledException");
                }
            }
        }

        static void StartTimer()
        {
            stopWatch.Stop();
            stopWatch.Start();
        }

        static double StopTimer()
        {
            stopWatch.Stop();
            //Console.WriteLine($"RUNTIME: {stopWatch.Elapsed.TotalMilliseconds} ms");
            return stopWatch.Elapsed.TotalMilliseconds;
        }

        #region --- DIVERSE UTILITIES ---
        static void TestTagHelper()
        {
            LinkDALRequestIPA5Object dalRequest = new LinkDALRequestIPA5Object()
            {
                CapturedCardData = new LinkCardResponse(),
                CapturedEMVCardData = new DAL_EMVCardData()
            };

            List<byte[]> tagList = new List<byte[]>()
            {
                E0Template.CardholderName,
                new byte[] { 0x5F, 0x30 },
                new byte[] { 0x5F, 0x40 },
                new byte[] { 0x5F, 0x50 }
            };

            foreach (var tag in tagList)
            {
                bool matches = TagHelper.TagMapper.ContainsKey(tag);
                if (matches)
                {
                    (string tcParameter, string dalObject, string property) tagMapperValue = (string.Empty, string.Empty, string.Empty);
                    if (TagHelper.TagMapper.TryGetValue(tag, out tagMapperValue))
                    {
                        Console.WriteLine($"TAG {ConversionHelper.ByteArrayToHexString(tag)} => '{tagMapperValue.tcParameter}'");
                        Debug.WriteLine($"TAG {ConversionHelper.ByteArrayToHexString(tag)} => '{tagMapperValue.tcParameter}'");

                        //Read System.ComponentModel Description Attribute from method 'MyMethodName' in class 'MyClass'
                        //var attribute = typeof(LinkCardResponse).GetAttribute(tagMapperValue.attribute, (DescriptionAttribute d) => d.Description);
                        //if (attribute != null)
                        //{

                        //}
                        Type mappedType = Type.GetType(tagMapperValue.dalObject);
                        PropertyInfo property = mappedType.GetProperty(tagMapperValue.property);
                        if (property != null)
                        {
                            dalRequest.CapturedCardData.CardholderName = "";
                        }
                    }
                }
            }
        }

        static void TestArrayCodedString()
        {
            // A9E0
            byte[] data = new byte[] { 0x41, 0x39, 0x45, 0x30 };
            // 1950
            //byte[] data = new byte[] { 0x31, 0x39, 0x35, 0x30 };
            string val = ConversionHelper.ByteArrayCodedHextoString(data);

            byte[] decryptedTrack = ConversionHelper.AsciiToByte(DecryptedString);
            Debug.WriteLine($"MESSAGE: '{ConversionHelper.ByteArrayToHexString(decryptedTrack)}'");

            //bool test = (-1 >= 0);
            string text = "{\"Responses\": [{ \"DALResponse\": { \"Devices\": [{\"Model\": \"UX300\", \"SerialNumber\": \"986058108\", \"CardWorkflowControls\": { \"CardCaptureTimeout\": 90, \"ManualCardTimeout\": 5, \"DebitEnabled\": false, \"EMVEnabled\": false, \"ContactlessEnabled\": false, \"ContactlessEMVEnabled\": false, \"CVVEnabled\": false, \"VerifyAmountEnabled\": false, \"AVSEnabled\": false, \"SignatureEnabled\": false }}]},\"EventResponse\": {\"EventType\": \"DISPLAY\", \"EventCode\": \"DEVICE_MESSAGE_DISPLAY\", \"EventID\": \"dde5987b-4073-428e-8b87-7105-b46ef398\", \"OrdinalID\": 2136617571, \"EventData\": [\"Insert card\"]}}]}";
            string message = ProcessMessage(text);
            Console.WriteLine($"MESSAGE: '{message}'");
        }

        static string ProcessMessage(string text)
        {
            string message = string.Empty;
            try
            {
                string value = System.Text.RegularExpressions.Regex.Replace(text.Trim('\"'), "[\\\\]+", string.Empty);
                if (value.Contains("DALResponse"))
                {
                    RootObject responses = JsonConvert.DeserializeObject<RootObject>(value);
                    if (responses != null && responses.Responses.Count > 0)
                    {
                        message = responses.Responses[0].EventResponse.EventData[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProcessMessage() exception: {ex.Message}");
            }
            return message;
        }

        static private string FormatStringAsRequired(string input, int length = KeyValueLength, char filler = KeyValuePaddingCharacter)
        {
            return input.PadRight(length, filler);
        }

        static public string AssembleString(bool format, string valueMissing, params string[] items)
        {
            string result = "";
            foreach (string item in items)
            {
                result += $"{item ?? valueMissing}";
            }
            return format ? FormatStringAsRequired(result) : result;
        }

        static public string GetConcat(params string[] names)
        {
            string result = "";
            if (names.Length > 0)
            {
                result = names[0];
            }
            for (int i = 1; i < names.Length; i++)
            {
                result += ", " + names[i];
            }
            return result;
        }

        public class RootObject
        {
            public List<Respons> Responses { get; set; }
        }

        public class Respons
        {
            //public DALResponse DALResponse { get; set; }
            public EventResponse EventResponse { get; set; }
        }

        /*
        public class DALResponse
        {
            public List<Device> Devices { get; set; }
        }

        public class Device
        {
            public string Model { get; set; }
            public string SerialNumber { get; set; }
            public CardWorkflowControls CardWorkflowControls { get; set; }
        }

        public class CardWorkflowControls
        {
            public int CardCaptureTimeout { get; set; }
            public int ManualCardTimeout { get; set; }
            public bool DebitEnabled { get; set; }
            public bool EMVEnabled { get; set; }
            public bool ContactlessEnabled { get; set; }
            public bool ContactlessEMVEnabled { get; set; }
            public bool CVVEnabled { get; set; }
            public bool VerifyAmountEnabled { get; set; }
            public bool AVSEnabled { get; set; }
            public bool SignatureEnabled { get; set; }
        }
        */
        public class EventResponse
        {
            public string EventType { get; set; }
            public string EventCode { get; set; }
            public string EventID { get; set; }
            public int OrdinalID { get; set; }
            public List<string> EventData { get; set; }
        }
        #endregion --- DIVERSE UTILITIES ---
    }
}
