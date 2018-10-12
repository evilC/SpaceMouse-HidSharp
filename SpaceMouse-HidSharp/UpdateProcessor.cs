using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMouse_HidSharp
{
    public class UpdateProcessor
    {
        private readonly byte[][] _previousStates = new byte[3][];

        public SpaceMouseUpdate[] ProcessUpdate(byte[] inputReportBuffer, int byteCount)
        {
            string hexOfBytes = string.Join(" ", inputReportBuffer.Take(byteCount).Select(b => b.ToString("X2")));
            Console.WriteLine($"Bytes: {hexOfBytes}");
            var updates = new List<SpaceMouseUpdate>();

            var bytes = inputReportBuffer.Take(byteCount).ToArray();
            switch (bytes[0])
            {
                case 1:
                    // Translation
                    for (var i = 0; i < 3; i++)
                    {
                        if (!AxisChanged(bytes, i)) continue;
                        Console.WriteLine($"Translation {i} changed");
                        updates.Add(new SpaceMouseUpdate{BindingType = BindingType.Axis, Index = i});   // ToDo: Add value
                    }
                    break;
                case 2:
                    // Rotation
                    for (var i = 0; i < 3; i++)
                    {
                        if (!AxisChanged(bytes, i)) continue;
                        Console.WriteLine($"Roatation {i} changed");
                        updates.Add(new SpaceMouseUpdate { BindingType = BindingType.Axis, Index = i + 2 });
                    }
                    break;
                case 3:
                    // Buttons
                    break;
            }

            _previousStates[bytes[0]] = inputReportBuffer.ToArray(); // array is reference type, clone!
            return updates.ToArray();
        }

        private bool AxisChanged(byte[] bytes, int index)
        {
            var offset = (index * 2) + 1;
            var previousState = _previousStates[bytes[0]];
            if (previousState != null && bytes[offset] == previousState[offset] && bytes[offset + 1] == previousState[offset + 1])
            {
                return false;
            }
            return true;
        }
    }
}
