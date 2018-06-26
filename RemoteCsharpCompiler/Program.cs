using System;
using System.Linq;
using System.ServiceModel;

namespace RemoteCsharpCompiler
{
    class Program
    {       
        static void Main(string[] args)
        {
            try
            {
                using (ServiceHost host = new ServiceHost(typeof(RemoteCompiler)))
                {
                    host.Open();
                    Console.WriteLine("Служба {0} запущена на {1}", host.Description.Name,
                        host.Description.Endpoints.First().Address);
                    Console.WriteLine("Для остановки службы {0} нажмите любую клавишу...", host.Description.Name);
                    Console.ReadKey(true);
                    Console.WriteLine("Служба {0} остановлена", host.Description.Name);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}