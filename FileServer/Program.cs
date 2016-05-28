using System;
using System.ServiceModel;

namespace FileServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new ServiceHost(typeof(FileBrowserService));
            host.Open();
            Console.WriteLine("Service started at {0}", DateTime.Now);
            Console.WriteLine("Press key to stop the service.");
            Console.ReadLine();
            host.Close();
        }
    }

}
