namespace AsyncServer_005
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Async Server";

            AsyncServer asyncServer = new("127.0.0.1", 49000);
            asyncServer.StartListening();

        }
    }
}