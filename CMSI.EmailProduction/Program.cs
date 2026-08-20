namespace CMSI.EmailProduction
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logger.Display("Starting CMSI.EmailProduction program.", true);
            //Listen on port 13000 for filestreams.
            Transporter PortListener = new Transporter(Configurator.IP, Configurator.Port);
            PortListener.StartListener();
        }
    }
}
