using System;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = Evaluator.Evaluate("input.txt");
            Console.WriteLine(result);
        }
    }
}
