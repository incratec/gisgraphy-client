using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisgraphyClient
{
    public class Address
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string ZIP { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string AdminArea1 { get; set; }
        public string AdminArea2 { get; set; }
        public string AdminArea3 { get; set; }
        public string Accuracy { get; set; }
        
        public string MatchTypeString { get; set; }
        
        public GeoInformation Position { get; set; }

        public Address()
        {
            Position = new GeoInformation();
        }

        public override string ToString()
        {
            List<string> sResElements = new List<string>();
            string sAdd = string.Join(" ", this.Street, this.Number);
            if (!string.IsNullOrEmpty(sAdd)) sResElements.Add(sAdd);

            sAdd = string.Join(" ", this.ZIP, this.City );
            if (!string.IsNullOrEmpty(sAdd)) sResElements.Add(sAdd);

            if (!string.IsNullOrEmpty(this.District)) sResElements.Add(District);
            if (!string.IsNullOrEmpty(this.State)) sResElements.Add(State);
            if (!string.IsNullOrEmpty(this.Country)) sResElements.Add(Country);

            if (Position != null && (Position.Latitude != 0 || Position.Longitude != 0))
            {
                sResElements.Add(string.Format("({0} , {1})", Position.Latitude, Position.Longitude));
            }
            return string.Join(", ", sResElements);
        }
    }

    public class GeoInformation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
