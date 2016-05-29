using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace FileServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class FileBrowserService : IFileBrowserService
    {
        #region Private members

        private IFileBrowserServiceCallback fileBrowserServiceCallback;

        #endregion Private members

        #region Constructor

        public FileBrowserService()
        {
            this.fileBrowserServiceCallback = OperationContext.Current.GetCallbackChannel<IFileBrowserServiceCallback>();

            // ToDo: Use FileSystemWatcher to notify about changes in file system
        }

        #endregion Constructor

        #region Public members

        #region IFileBrowserService implementation

        public void RequestFileList(string path)
        {
            Log("New request: " + path);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Log("Processing request: " + path);

                    // ToDo: Use file system instead of random data
                    var data = RandomDataGenerator.GetRandomData(path);

                    Log("Request handled: " + path);
                    this.fileBrowserServiceCallback.FileListChanged(data);
                }
                catch (Exception ex)
                {
                    Log("Exception occured: " + ex.Message);
                }
            });

            Log("Request queued: " + path);
        }

        #endregion IFileBrowserService implementation

        #endregion Public members

        #region Private methods

        private static void Log(string message)
        {
            Console.WriteLine("{0}: {1}", DateTime.Now.ToString("HH:mm:ss.fff"), message);
        }

        #endregion Private methods
    }

    internal class RandomDataGenerator
    {
        public static IEnumerable<FileInfo> GetRandomData(string path)
        {
            // Simulate delay
            Thread.Sleep(randomGenerator.Next(200, 1500));

            return new[]
            {
                new FileInfo { Name = "...", IsParent  = true },
                new FileInfo { Name = RandomString(15), Type = "Folder", Icon = RandomIcon(), Created = DateTime.Now, Modified = DateTime.Now.AddHours(2), IsFolder = true },
                new FileInfo { Name = RandomString(15), Type = RandomString(10), Icon = RandomIcon(), Created = DateTime.Now, Modified = DateTime.Now.AddHours(2) },
                new FileInfo { Name = RandomString(15), Type = RandomString(10), Icon = RandomIcon(), Created = DateTime.Now, Modified = DateTime.Now.AddHours(2) },
            };
        }

        private static readonly Random randomGenerator = new Random();
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private static string RandomString(int size)
        {
            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = chars[randomGenerator.Next(chars.Length)];
            }
            return new string(buffer);
        }

        private static byte[] RandomIcon()
        {
            var randomIcon = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"TestImages\Icon" + randomGenerator.Next(1, 3) + ".bmp", UriKind.Relative));

            byte[] randomData;
            var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(randomIcon));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                randomData = stream.ToArray();
            }

            return randomData;
        }
    }
}
