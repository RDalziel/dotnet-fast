// A deliberately messy file so you can see dotnet-fast find and fix things.
// Run `dotnet-fast lint .` here to see findings, then `dotnet-fast lint --fix .`.

using System;

namespace HelloWorld
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var name= "world" ;
            Console.WriteLine( "hello, "+name );

            for(var i=0;i<3;i++)
            {
                Console.WriteLine($"line {i}");
            }
        }
    }
}
