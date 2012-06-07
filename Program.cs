using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisgraphyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Address moma = new Address();
            moma.Street = "West 53 Street";
            moma.Number = "11";
            moma.ZIP = "10019";
            moma.City = "New York";
            moma.State = "NY";
            moma.Country = "US";

            Console.WriteLine("Geocoding location of Moma NY: " + moma.ToString());

            GisgraphyClient gc = new GisgraphyClient();
            var momaLocations = gc.CodeAddress(moma);

            foreach (var momaPos in momaLocations)
            {
                Console.WriteLine(momaPos);
            }
            Console.WriteLine("Press [any key]");
            Console.In.Read();
        }
    }
}
