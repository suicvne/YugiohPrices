using System;
using YugiohPrices;

namespace YugiohPricesTest
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            YugiohPricesSearcher searcher = new YugiohPricesSearcher();

            ///Invalid card
            Console.WriteLine("--Testing with invalid card--");
            try
            {
                var result = searcher.GetCardByName("m8").Result;
                Console.WriteLine($"{result.Name}. {result.Description}");
            }
            catch(AggregateException ex)
            {
                ex.Handle((x) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exception: " + x.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    return true;
                });
            }

            Console.WriteLine("\n--Testing with valid card--");
            try
            {
                var result = searcher.GetCardByName("Dark Magician").Result;
                Console.WriteLine($"{result.Name}. {result.Description}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine("\n\nPress enter to exit..");
            Console.ReadLine();
        }
    }
}
