using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace FileServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.PerSession)]
    public class FileBrowserService : IFileBrowserService, IDisposable
    {
        #region Private members

        private IFileBrowserServiceCallback fileBrowserServiceCallback;

        private string path;
        private FileSystemWatcher fileSystemWatcher;

        #endregion Private members

        #region Constructor

        public FileBrowserService()
        {
            Log("Creating service (client connected): " + OperationContext.Current.SessionId);

            Log("Creating callback");
            this.fileBrowserServiceCallback = OperationContext.Current.GetCallbackChannel<IFileBrowserServiceCallback>();
        }

        #endregion Constructor

        #region Public members

        #region IFileBrowserService implementation

        public void RequestFileList(string path)
        {
            try
            {
                if (this.fileSystemWatcher == null)
                {
                    Log("Creating file system watcher");
                    this.fileSystemWatcher = new FileSystemWatcher { Path = path, EnableRaisingEvents = true };
                    this.fileSystemWatcher.Changed += FileSystemWatcherChanged;
                    this.fileSystemWatcher.Created += FileSystemWatcherChanged;
                    this.fileSystemWatcher.Renamed += FileSystemWatcherChanged;
                    this.fileSystemWatcher.Deleted += FileSystemWatcherChanged;
                }
                else
                {
                    this.fileSystemWatcher.Path = path;
                }

                Log("New request: " + path);
                Task.Factory.StartNew(() =>
                {
                    Log("Processing request: " + path);

                    var data = GetFileList(path);

                    this.path = path;

                    Log("Request handled: " + path);
                    this.fileBrowserServiceCallback.FileListChanged(data);
                });

                Log("Request queued: " + path);
            }
            catch (Exception ex)
            {
                Log("Exception requesting file list: " + ex.Message);
                throw new FaultException(ex.Message);
            }
        }

        #endregion IFileBrowserService implementation

        #region IDisposable implementation

        public void Dispose()
        {
            Log("Disposing file system watcher");
            this.fileSystemWatcher.Changed -= FileSystemWatcherChanged;
            this.fileSystemWatcher.Created -= FileSystemWatcherChanged;
            this.fileSystemWatcher.Renamed -= FileSystemWatcherChanged;
            this.fileSystemWatcher.Deleted -= FileSystemWatcherChanged;
            this.fileSystemWatcher = null;

            Log("Disposing callback");
            this.fileBrowserServiceCallback = null;

            Log("Disposing service (client disconnected)");
        }

        #endregion IDisposable implementation

        #endregion Public members

        #region Private methods

        public IEnumerable<FileInfo> GetFileList(string path)
        {
            var dir = new DirectoryInfo(path);

            var directories = dir.GetDirectories().Select(directory => ToFileInfo(directory));
            var files = dir.GetFiles().Select(file => ToFileInfo(file));

            var fileList = new List<FileInfo> { new FileInfo { Name = "...", IsParent = true } };
            fileList.AddRange(directories.Concat(files));

            return fileList;
        }

        private FileInfo ToFileInfo(System.IO.FileInfo file)
        {
            return new FileInfo
            {
                Icon = IconReader.GetFileIcon(file.FullName),
                Name = file.Name,
                Type = FileTypeReader.GetFileType(file.FullName),
                Created = file.CreationTime,
                Modified = file.LastWriteTime
            };
        }

        private FileInfo ToFileInfo(DirectoryInfo directory)
        {
            return new FileInfo
            {
                Icon = IconReader.GetFolderIcon(directory.FullName),
                Name = directory.Name,
                Type = FileTypeReader.GetFileType(directory.FullName),
                Created = directory.CreationTime,
                Modified = directory.LastWriteTime,
                IsFolder = true
            };
        }

        private void FileSystemWatcherChanged(object sender, FileSystemEventArgs e)
        {
            Log("Directory " + e.FullPath + " changed: " + e.ChangeType);

            RequestFileList(this.path);
        }

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
