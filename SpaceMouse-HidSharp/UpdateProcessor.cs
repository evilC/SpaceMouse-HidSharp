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
            //string hexOfBytes = string.Join(" ", inputReportBuffer.Take(byteCount).Select(b => b.ToString("X2")));
            //Console.WriteLine($"Bytes: {hexOfBytes}");
            var updates = new List<SpaceMouseUpdate>();

            var bytes = inputReportBuffer.Take(byteCount).ToArray();
            switch (bytes[0])
            {
                case 1:
                    // Translation
                    for (var i = 0; i < 3; i++)
                    {
                        var value = GetAxisValue(bytes, i);
                        if (value == null) continue;
                        //Console.WriteLine($"Translation {i} changed to {value}");
                        updates.Add(new SpaceMouseUpdate{BindingType = BindingType.Axis, Index = i, Value = (int)value});
                    }
                    break;
                case 2:
                    // Rotation
                    for (var i = 0; i < 3; i++)
                    {
                        var value = GetAxisValue(bytes, i);
                        if (value == null) continue;
                        //Console.WriteLine($"Roatation {i} changed to {value}");
                        updates.Add(new SpaceMouseUpdate { BindingType = BindingType.Axis, Index = i + 2, Value = (int)value});
                    }
                    break;
                case 3:
                    // Buttons
                    break;
            }

            _previousStates[bytes[0]] = inputReportBuffer.ToArray(); // array is reference type, clone!
            return updates.ToArray();
        }

        private int? GetAxisValue(byte[] bytes, int index)
        {
            var valueByteIndex = (index * 2) + 1;

            var previousState = _previousStates[bytes[0]];
            if (previousState != null && bytes[valueByteIndex] == previousState[valueByteIndex] && bytes[valueByteIndex + 1] == previousState[valueByteIndex + 1])
            {
                return null;
            }

            var multiplierByteIndex = valueByteIndex + 1;
            var valueByte = bytes[valueByteIndex];
            var multiplierByte = bytes[multiplierByteIndex];

            var isInverted = multiplierByte > 253;
            var isAmplified = isInverted ? multiplierByte == 254 : multiplierByte == 1;

            var value = isInverted ? 255 - valueByte : valueByte;
            if (isAmplified) value += 255;
            if (isInverted) value *= -1;
            return value;
        }
    }
}
