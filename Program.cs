using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Runtime.InteropServices;
using NewTek;


class Program
{
    static void Main()
    {
        if (!NDIlib.initialize())
        {
            Console.WriteLine("Failed to initialize NDI.");
            return;
        }

        var senderDesc = new NDIlib.send_create_t
        {
            p_ndi_name = Marshal.StringToHGlobalAnsi("CSharp NDI Test Feed")
        };

        IntPtr senderPtr = NDIlib.send_create(ref senderDesc);

        if (senderPtr == IntPtr.Zero)
        {
            Console.WriteLine("NDI Sender creation failed.");
            Marshal.FreeHGlobal(senderDesc.p_ndi_name);
            return;
        }

        int width = 1280;
        int height = 720;
        Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        Graphics gfx = Graphics.FromImage(bmp);
        Font font = new Font("Arial", 24, FontStyle.Bold);

        int frameRate = 30;
        int frameInterval = 1000 / frameRate;
        int posX = 0;

        Console.WriteLine("Press any key to exit...");
        while (!Console.KeyAvailable)
        {
            gfx.Clear(Color.MidnightBlue);
            gfx.FillRectangle(Brushes.Yellow, posX, height / 3, 200, 200);
            gfx.DrawString(DateTime.Now.ToString("HH:mm:ss.fff"), font, Brushes.White, 10, 10);
            posX = (posX + 5) % width;

            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var videoFrame = new NDIlib.video_frame_v2_t()
            {
                xres = width,
                yres = height,
                FourCC = NDIlib.FourCC_type_e.FourCC_type_BGRA,
                frame_rate_N = frameRate,
                frame_rate_D = 1,
                line_stride_in_bytes = bmpData.Stride,
                p_data = bmpData.Scan0,
            };

            NDIlib.send_send_video_v2(senderPtr, ref videoFrame);
            bmp.UnlockBits(bmpData);
            Thread.Sleep(frameInterval);
        }

        NDIlib.send_destroy(senderPtr);
        NDIlib.destroy();
        Marshal.FreeHGlobal(senderDesc.p_ndi_name);
    }
}
