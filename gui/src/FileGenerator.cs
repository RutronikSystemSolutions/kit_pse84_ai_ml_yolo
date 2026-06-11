using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kit_pse84_ai_streaming
{
    public class FileGenerator
    {
        /// <summary>
        /// Generate a C file that will be used to test the inference using TFLITE on the PSOC EDGE
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static void GenerateContent(string path, byte[] data)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write("static uint8_t raw_ov7675_data[] = {");
                for (int i = 0; i < data.Length; ++i)
                {
                    if ((i % 8) == 0)
                    {
                        string towrite = Environment.NewLine + "\t0x" + data[i].ToString("X2");
                        if (i != (data.Length - 1)) towrite += ",";
                        writer.Write(towrite);
                    }
                    else
                    {
                        string towrite = "0x" + data[i].ToString("X2");
                        if (i != (data.Length - 1)) towrite += ",";
                        writer.Write(towrite);
                    }
                }
                writer.WriteLine(Environment.NewLine + "};");
            }            
        }
    }
}
