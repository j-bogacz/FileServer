using System.Collections.Generic;
using System.ServiceModel;

namespace FileServer
{
    public interface IFileBrowserServiceCallback
    {
        [OperationContract]
        void FileListChanged(IEnumerable<FileInfo> fileList);
    }
}
