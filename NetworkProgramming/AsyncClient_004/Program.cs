namespace AsyncClient_004
{

    internal class Program
    {
        static void Main(string[] args)
        {
            AsyncClient asyncClient = new AsyncClient();
            asyncClient.SendMessage("127.0.0.1", 49000, "Hello, iam T-800, need upd");

            //for (int i = 0; i < 10; i++)
            //{

            //    Task.Run(() =>
            //    {
            //        asyncClient.SendMessage("127.0.0.1", 49000, $"Hello, iam {i}T-800, need upd");

            //    });

            //}
            
            Console.ReadKey();

        }
    }
}