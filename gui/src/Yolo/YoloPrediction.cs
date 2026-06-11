using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kit_pse84_ai_streaming.Yolo
{
    public record YoloPrediction(string label, float confidence, RectangleF rectangle);
}
