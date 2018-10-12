using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMouse_HidSharp
{
    public class UpdateProcessor
    {
        private const int ByteCount = 7;
        private readonly byte[][] _previousStates = new byte[4][];

        public SpaceMouseUpdate[] ProcessUpdate(byte[] inputReportBuffer)
        {
            var updates = new List<SpaceMouseUpdate>();

            var bytes = inputReportBuffer.Take(ByteCount).ToArray();
            if (bytes[0] == 3)
            {
                // Buttons
                string hexOfBytes = string.Join(" ", inputReportBuffer.Take(ByteCount).Select(b => b.ToString("X2")));
                Console.WriteLine($"Bytes: {hexOfBytes}");
            }
            else
            {
                // Axes
                var isRotation = bytes[0] == 2;
                var offset = isRotation ? 3 : 0;
                for (var i = 0; i < 3; i++)
                {
                    var value = GetAxisValue(bytes, i);
                    if (value == null) continue;
                    updates.Add(new SpaceMouseUpdate
                    {
                        BindingType = BindingType.Axis,
                        Index = offset + i,
                        Value = (int)value
                    });
                }
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
