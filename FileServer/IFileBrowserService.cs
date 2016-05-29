using System.ServiceModel;

namespace FileServer
{
    [ServiceContract(CallbackContract = typeof(IFileBrowserServiceCallback))]
    public interface IFileBrowserService
    {
        [OperationContract]
        void RequestFileList(string path);
    }
}
