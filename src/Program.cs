
using System;

namespace src
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Example1();
            Example2();
        }

        public static void Example1()
        {
            // (((1/4) ^ -1) - 2) ^ -1
            // = 1 / 2
            var x = 1.0 / 4;

            string result =
                Invert(x)
                .Map(y => y - 2)
                .Then(Invert)
                .Get(
                    err => err,
                    y => "Result: " + y.ToString()
                );

            Console.WriteLine(result);
        }

        public static void Example2()
        {
            // (((1/2) ^ -1) - 2) ^ -1
            // => divide by zero
            var x = 1.0 / 2;

            string result =
                Invert(x)
                .Map(y => y - 2)
                .Then(Invert)
                .Get(
                    err => err,
                    y => "Result: " + y.ToString()
                );

            Console.WriteLine(result);
        }

        public static Either<string, double> Invert(double x)
        {
            if(x == 0)
            {
                return Either<string, double>
                    .Fail("Error: divide by zero");
            }

            return Either<string, double>
                .Return(1 / x);
        }
    }
}
