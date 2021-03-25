using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Utils.Dictionaries;
using Utils.Helper;
using Utils.Methods;
using Utils.TLV;

namespace Utils
{
    class Program
    {
        //static readonly string DecryptedString = @"%B4815881002867896^DOE/DUMB JOHN         ^37829821000123456789?";
        static readonly string DecryptedString = @"%B4815881002861896^DOE/L JOHN            ^2212102356858      00998000000?";
        static readonly string DecryptedArray = "8AD3D8F024F60AC33935333139323335313030343D323530323130313130303831323334353030303F35800000000000";
        static void Main(string[] args)
        {
            //TestArrayCodedString();
            TestTagHelper();
        }

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
                    if(TagHelper.TagMapper.TryGetValue(tag, out tagMapperValue))
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
    }
}
