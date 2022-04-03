using System;
using System.Drawing;
using System.IO;
using ImageProcessor;
using ImageProcessor.Imaging;

namespace EchoMessenger.Helpers
{
    public static class Media
    {
        public static Stream ProccessImage(String filePath)
        {
            using (var imageFactory = new ImageFactory())
            {
                imageFactory.Load(Image.FromFile(filePath));
                imageFactory.Resize(new ResizeLayer(new Size(150, 150), ResizeMode.Crop));

                var stream = new MemoryStream();

                imageFactory.Save(stream);
                stream.Position = 0;

                return stream;
            }
        }
    }
}
