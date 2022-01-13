using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Utils.Helper;

namespace Utils.ArrayProcessing
{
    public static class ArrayConversions
    {
        static byte[] dataTag = ConversionHelper.HexToByteArray("374245aaaaa1006d2001702aaaaaaaaaaaaaaa");
        static Stopwatch stopWatch = new Stopwatch();

        public static void PerformComparisonTests()
        {
            int[] runs = new int[] { 1, 10 };

            for (int j = 0; j < runs.Length; ++j)
            {
                int runsToPerform = runs[j];

                double[] timeObservedTestA = new double[runsToPerform];
                double[] timeObservedTestB = new double[runsToPerform];
                double[] timeObservedTestC = new double[runsToPerform];
                double[] timeObservedTestD = new double[runsToPerform];

                Console.WriteLine($"EXECUTING \"{runs[j]}\" TEST RUNS");
                Console.WriteLine("-------------------------------------\n");

                // average of runsToPerform runs
                string aTestOutput = string.Empty;

                // BASE-A
                for (int i = 0; i < runsToPerform; i++)
                {
                    StartTimer();
                    aTestOutput = TestArrayConversion(dataTag);
                    timeObservedTestA[i] = StopTimer();
                }
                double timeBaseA = timeObservedTestA.Average();
                Console.WriteLine($"TEST-A : [{aTestOutput}]");
                Console.WriteLine($"RUNTIME: {timeBaseA} ms");
                Console.WriteLine();

                // BASE-B
                string bTestOutput = string.Empty;
                for (int i = 0; i < runsToPerform; i++)
                {
                    StartTimer();
                    bTestOutput = TestStringWithoutConversion(dataTag);
                    timeObservedTestB[i] = StopTimer();
                }
                double timeBaseB = timeObservedTestB.Average();
                Console.WriteLine($"TEST-B (Without Conversion): [{bTestOutput}]");
                Console.WriteLine($"RUNTIME: {timeBaseB} ms");
                Console.WriteLine();

                // BASE-C
                string cTestOutput = string.Empty;
                for (int i = 0; i < runsToPerform; i++)
                {
                    StartTimer();
                    cTestOutput = TestStringWithConversion(dataTag);
                    timeObservedTestC[i] = StopTimer();
                }

                double timeBaseC = timeObservedTestC.Average();
                Console.WriteLine($"TEST-C (With Conversion): [{cTestOutput}]");
                Console.WriteLine($"RUNTIME: {timeBaseC} ms");
                Console.WriteLine();

                // BASE-D
                string dTestOutput = string.Empty;
                for (int i = 0; i < runsToPerform; i++)
                {
                    StartTimer();
                    dTestOutput = FonzieFastTagConversion(dataTag);
                    timeObservedTestD[i] = StopTimer();
                }

                double timeBaseD = timeObservedTestD.Average();
                Console.WriteLine($"TEST-D (Fonzie Blazing Fast Convert): [{dTestOutput}]");
                Console.WriteLine($"RUNTIME: {timeBaseD} ms");
                Console.WriteLine("");
                Console.WriteLine("-------------------------------------\n");
            }

            Console.WriteLine("");
            Console.ReadLine();
        }

        private static string TestArrayConversion(byte[] dataTag)
        {
            string output = BitConverter.ToString(dataTag).Replace("A", "*").Replace("D", "=").Replace("-", "");
            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FonzieFastTagConversion(byte[] dataTag)
        {
            const char c_star = '*';
            const char c_equal = '=';

            return string.Create(dataTag.Length * 2, dataTag, (chars, buf) =>
            {
                byte nibble;
                for (int indice = 0; indice < dataTag.Length; ++indice)
                {
                    int spot = indice * 2;

                    nibble = ((byte)(dataTag[indice] >> 4));

                    if (GetAcceptedChars((char)(nibble > 9 ? nibble + 0x37 : nibble + 0x30), out char a))
                    {
                        chars[spot] = a;
                    }

                    nibble = ((byte)(dataTag[indice] & 0xF));

                    if (GetAcceptedChars((char)(nibble > 9 ? nibble + 0x37 : nibble + 0x30), out char b))
                    {
                        chars[spot + 1] = b;
                    }
                }

                static bool GetAcceptedChars(char c, out char outChar)
                {
                    outChar = char.MinValue;

                    if (c == 'A')
                    {
                        outChar = c_star;
                        return true;
                    }
                    else if (c == 'D')
                    {
                        outChar = c_equal;
                    }
                    else if (c != '-')
                    {
                        outChar = c;
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                }
            });
        }

        private static string TestStringWithConversion(byte[] dataTag)
        {
            string conversion = BitConverter.ToString(dataTag);
            string output = string.Create(conversion.Length, conversion, (chars, buf) =>
            {
                int count = 0;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (buf[i] == 'A')
                    {
                        chars[count++] = '*';
                    }
                    else if (buf[i] == 'D')
                    {
                        chars[count++] = '=';
                    }
                    else if (buf[i] != '-')
                    {
                        chars[count++] = buf[i];
                    }
                }
            });
            return output;
        }

        /// <summary>
        /// Each byte in the array needs to be split into high and low byte to examine it and convert it appropriately
        /// </summary>
        /// <param name="dataTag"></param>
        /// <returns></returns>
        private static string TestStringWithoutConversion(byte[] dataTag)
        {
            string output = String.Create(dataTag.Length * 2, dataTag, (chars, buf) =>
            {
                int count = 0;
                for (int i = 0; i < buf.Length; i++)
                {
                    byte hi = (byte)((buf[i] & 0xF0) >> 0x04);
                    if (!IsHexDigit(hi))
                    {
                        hi += 0x30;
                    }
                    byte lo = (byte)(buf[i] & 0x0F);
                    if (!IsHexDigit(lo))
                    {
                        lo += 0x030;
                    }

                    char value = ConvertByte(hi);
                    if (value != '-')
                    {
                        chars[count++] = value;
                    }
                    value = ConvertByte(lo);
                    if (value != '-')
                    {
                        chars[count++] = value;
                    }
                }
            });
            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsHexDigit(byte value)
            => value >= 0x0a && value <= 0x0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char ConvertByte(byte value) => value switch
        {
            0x0a => '*',
            0x0d => '=',
            _ => (char)value
        };

        private static void StartTimer()
        {
            stopWatch.Reset();
            stopWatch.Start();
        }

        private static double StopTimer()
        {
            stopWatch.Stop();
            //Console.WriteLine($"RUNTIME: {stopWatch.Elapsed.TotalMilliseconds} ms");
            return stopWatch.Elapsed.TotalMilliseconds;
        }
    }
}
