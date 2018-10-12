using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;
using HidSharp.Reports;

// From https://github.com/InputMapper/3DConnection/blob/master/README.md
// Packet Definition:
// 1st byte is packet type, 1 = translation, 2 = rotation, 3 = buttons
// Packet type 1:
// b[1] = X translation (0-255 | 0 - 255)
// b[2] = X translation dir and multiplier. (254,255 L|R 0,1)
// b[3] = Y translation (0-255 | 0 - 255)
// b[4] = Y translation dir and multiplier. (254,255 F|B 0,1)
// b[5] = Z translation (0-255 | 0 - 255)
// b[6] = Z translation dir and multiplier. (254,255 U|D 0,1)
// Packet type 2:
// b[1] = X rotation (0-255 | 0 - 255)
// b[2] = X rotation dir and multiplier. (254,255 L|R 0,1)
// b[3] = Y rotation (0-255 | 0 - 255)
// b[4] = Y rotation dir and multiplier. (254,255 F|B 0,1)
// b[5] = Z rotation (0-255 | 0 - 255)
// b[6] = Z rotation dir and multiplier. (254,255 U|D 0,1)
// Packet type 3:
// b[1] = Buttons, Left >> 1, Right >> 2

namespace SpaceMouse_HidSharp
{
    public class SpaceMouse
    {
        private readonly HidDevice[] _hidDeviceList;
        private readonly UpdateProcessor _updateProcessor = new UpdateProcessor();

        public SpaceMouse()
        {
            var list = DeviceList.Local;
            _hidDeviceList = list.GetHidDevices().ToArray();
        }

        public void WatchDevice(int vid, int pid)
        {
            var dev = _hidDeviceList.FirstOrDefault(hidDevice => hidDevice.VendorID == vid && hidDevice.ProductID == pid);

            if (dev == null)
            {
                throw new Exception($"Unkown device VID {vid} PID {pid}");
            }

            ReportDescriptor reportDescriptor;
            try
            {
                reportDescriptor = dev.GetReportDescriptor();
            }
            catch
            {
                return;
            }
            foreach (var deviceItem in reportDescriptor.DeviceItems)
            {
                if (dev.TryOpen(out var hidStream))
                {
                    Console.WriteLine("Opened device.");
                    hidStream.ReadTimeout = Timeout.Infinite;

                    using (hidStream)
                    {
                        var inputReportBuffer = new byte[dev.GetMaxInputReportLength()];
                        var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                        var inputParser = deviceItem.CreateDeviceItemInputParser();

                        inputReceiver.Start(hidStream);

                        IAsyncResult ar = null;
                        while (true)
                        {
                            if (ar == null)
                            {
                                ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);
                            }

                            if (ar != null)
                            {
                                if (ar.IsCompleted)
                                {
                                    int byteCount = hidStream.EndRead(ar);
                                    ar = null;

                                    if (byteCount > 0)
                                    {
                                        var updates = _updateProcessor.ProcessUpdate(inputReportBuffer);
                                        if (updates == null) continue;
                                        foreach (var update in updates)
                                        {
                                            Console.WriteLine($"Type: {update.BindingType}, Index: {update.Index}, Value: {update.Value}");
                                        }
                                    }
                                }
                                else
                                {
                                    ar.AsyncWaitHandle.WaitOne(1000);
                                }
                            }
                        }

                    }

                }
            }


        }

    }
}
