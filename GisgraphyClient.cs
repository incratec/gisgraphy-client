using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.XPath;
using System.Globalization;
using System.Reflection;
using System.Security;

namespace GisgraphyClient
{
    public class GisgraphyClient
    {
        private string ServerName = "services.gisgraphy.com"; // default server for Gisgraphy free
        private string ApiKey = ""; // not used with Gisgraphy Free
        private int Timeout = 2; // timeout in seconds for Gisgraphy (otherwise the ip address will be blocked)

        private const string XPATH_RESPONSESTATUS = "/results/numFound";
        private const string XPATH_RESPONSEADDRESS = "/results/result";
        private const string USER_AGENT = "GisgraphyClient.Net";
        private const string URL_GEOCODER = "http://{0}/geocoding/geocode?address={1}&country={2}&postal=true";
        
        public GisgraphyClient(string urlServer = null, string apiKey = null)
        {
            if (!string.IsNullOrEmpty(urlServer) && !string.IsNullOrEmpty(apiKey))
            {
                ServerName = urlServer;
                ApiKey = apiKey;

            }
        }

        public List<Address> CodeAddress(Address address)
        {
            List<Address> result = new List<Address>(); ;
            
            Stream aStream = null;
            WebResponse response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetUrl(address));
                request.UserAgent = USER_AGENT;
                response = request.GetResponse();

                // parse the result
                aStream = response.GetResponseStream();
                XPathDocument docNav = new XPathDocument(aStream);
                XPathNavigator nav = docNav.CreateNavigator();

                int numberOfResults = -1;
                // step 1: check response
                XPathNodeIterator NodeIter = nav.Select(XPATH_RESPONSESTATUS);
                if (NodeIter.MoveNext())
                {
                    string numFound = NodeIter.Current.Value;
                    if (numFound != null && numFound.Length > 0)
                    {
                        numberOfResults = Convert.ToInt32(numFound);
                    }
                }

                // step 2: decode result addresses
                if (numberOfResults > 0)
                {
                    NodeIter = nav.Select(XPATH_RESPONSEADDRESS);
                    while (NodeIter.MoveNext())
                    {
                        result.Add(ExtractReponseAddress(NodeIter.Current));
                    }
                }
            }
            finally
            {
                if (aStream != null) aStream.Close();
                if (response != null) response.Close();
            }

            return result;
        }

        private string GetUrl(Address address)
        {
            string sAddress = FormatAddress(address);
            string result = string.Format(URL_GEOCODER, ServerName, (string)Uri.EscapeUriString(sAddress), (string)Uri.EscapeUriString(address.Country.ToXmlValidString()));
            if (!string.IsNullOrEmpty(ApiKey))
            {
                result += "&apikey=" + ApiKey;
            }

            return result;
        }


        private string FormatAddress(Address address)
        {
            // source: http://www.gisgraphy.com/documentation/addressparser.htm#implemetedcountries
            string resAddress = string.Empty;
            // parameter: numbers & values
            // 0 - streetName
            // 1 - houseNumber
            // 2 - zip
            // 3 - city
            // 4 - state

            string addressFormat = "{0} {1}, {2} {3} {4}"; // default format
            switch (address.Country)
            {
                case "DZ": // Algeria
                    // Pattern :[unit] [houseNumber] [streetType] streetName [zip|,] city [CEDEX [(number)]]
                    // Example :APARTMENT N° 13 ENTRANCE C IMMEUBLE ANNADA 23 BOULEVARD DE LA MOSQUEE 16027 ALGIERS
                    resAddress = string.Format("{1} {0} {2} {3}",
                                        address.Street.ToXmlValidString(),
                                        address.Number.ToXmlValidString(),
                                        string.IsNullOrEmpty(address.ZIP) ? "," : address.ZIP.ToXmlValidString(),
                                        address.City.ToXmlValidString());
                    break;

                case "AO": // Angola
                    // Pattern : streetType streetName [houseNumber|,] [[floor][side]] city
                    // Example : Lg Dr António Viana 1 2 DTO 1250–096 LISBOA
                    resAddress = string.Format("{0} {1} {3}",
                                        address.Street.ToXmlValidString(),
                                        string.IsNullOrEmpty(address.Number) ? "," : address.Number.ToXmlValidString(),
                                        "",
                                        address.City.ToXmlValidString());
                    break;

                case "AG": // Argentinia
                    // Pattern : streetName houseNumber [unit] zip city
                    // Example : Sarmiento 151 Piso 3 Oficina 311 BIS C1000ZAA BUENOS AIRES
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "AU": // Australia
                    // Pattern : [unit] [houseNumber] streetName streetType city province_abbreviation [zip]
                    // Example : Level 7 17 Jones St. NORTH SYDNEY NSW 2060
                    addressFormat = "{1} {0} {3} {4} {2}";
                    break;

                case "AT": // Austria
                    // Pattern : streetName [streettype] houseNumber [unit] zip city
                    // Example : Garten weg 8 Rafing 3741 PULKAU
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "BE": // Belgium
                    // Pattern : streettype streetName houseNumber [unit] zip city
                    // Example : Hoge Wei 27 B-1930 Zaventem
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "BR": // Brazil
                    // Pattern : streetType streetName [houseNumber|,] city-state [zip]
                    // Example : Rua Visconde de Porto Seguro 1238 Sao Paulo-SP 04642-000
                    addressFormat = "{0} {1} {3}-{4} {2}";
                    break;

                case "CM": // Cameroon
                    // Pattern :[unit] [houseNumber] [streetType] [,] streetName city
                    // Example :12, rue foo YAOUNDE
                    addressFormat = "{1} , {0} {3}";
                    break;

                case "CA": // Canada
                    // Pattern : [houseNumber] [fraction] [predirection] streetName streetType [postdirection] [unit] city [state] [zip]
                    // Example : 123 3/4 N name with space 1 number blvd south floor 2 Missouri CA 12345-4536
                    addressFormat = "{1} {0}, {3} {4} {2}";
                    break;

                case "CN": // China
                    // Pattern : [unit] [houseNumber] streetName (,) city [zip] [province]
                    // Example : ROOM 305 63 RENMIN LU, QINGDAO SHI 266033 SHANDONG
                    addressFormat = "{1} {0}, {3} {2} {4}";
                    break;

                case "CD": // Congo (Democratic Republic of)
                    // Pattern :[unit] [houseNumber] [streetType] [,] streetName city
                    // Example :12, rue Kakamoueka BRAZZAVILLE
                    addressFormat = "{1} , {0} {2}";
                    break;

                case "FO": // Faroe Islands
                case "GL": // Greenland
                case "DK": // Denmark
                    // Pattern : streetName [houseNumber] [unit] [district] zip [city]
                    // Example : Kastanievej 15, 2 Agerskov 1566 COPENHAGEN V
                    addressFormat = "{0} {1} {4} {2} {3}";
                    break;

                case "FI": // Finland
                    // Pattern :streetName [houseNumber] [unit] [zip] city
                    // Example :Mäkelänkatu 25 B 13	FI-00550 HELSINKI
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "GF": // French Guiana
                case "GQ": // Guadeloupe
                case "MQ": // Martinique
                case "RE": // Reunion
                case "MF": // Saint Martin
                case "PM": // Saint Pierre and Miquelon
                case "SN": // Senegal
                case "FR": // France
                    // Pattern :[unit] [houseNumber] [streetType] streetName [zip|,] city [CEDEX [(number)]]
                    // Example :caserne des pompiers étage 2 palier 3 157 bd du 3 juillet étage n°2 95190 saint jean de luz
                    addressFormat = "{1} {0} {3} {2}";
                    break;

                case "DE": // Germany
                    // Pattern : streetName [streettype] houseNumber[/number|letter] [unit] zip city
                    // Example : Rhondorfer Str. 665 Appartment 47 50939 Köln
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "IN": // India
                    // pattern [unit wherever in address] [house number] [street name (cross and main or single road)] + [area name with block/phase/stage/sector) city name + postcode + [stateName]
                    // Example : New #5, 1st Cross, HSR Layout,80th Feet Main Road Phase 3, 4th sector, 51st block, 2nd stage,bangalore 560 035 manipur
                    // Example : New #768, Ground Floor, 100 Feet Road, 12th Main Road, HAL 2nd Stage bangalore 560 035 Chamba-Kangra
                    addressFormat = "{1} {0} {3} {2} {4}";
                    break;

                case "ID": // Indonesia
                    // Pattern : [HouseNumber] streetName streetType [houseNumber|(,)] city|regencies [zip] province/state
                    // Example : 73 Jalan Cilaki, BANDUNG 40115 Sulbar
                    addressFormat = "{1} {0}, {3} {2} {4}";
                    break;

                case "IR": // Iran
                    // Pattern : streetName streetType|(,) [houseNumber] [unit] [zip] city
                    // Example : Shahid Hossein Behrouz street No 17 1st floor 1193653471 TEHRAN
                    addressFormat = "{1}, {0} {2} {3}";
                    break;

                case "SM": // San Marino
                case "VA": // Vatican
                case "IT": // Italy
                    // Pattern :[unit] [streetType] streetName [houseNumber] [zip] city [state]
                    // Example :VIALE EUROPA 22 00144 ROMA RM
                    addressFormat = "{0} {1} {2} {3} {4}";
                    break;

                case "HK": // Hong Kong
                    // Pattern : [unit] [houseNumber] streetName [streetType] city|district [hong kong[NT]]
                    // Example : 16 Sandilands Road WAN CHAI
                    addressFormat = "{1} {0}, {3} {4}";
                    break;

                case "MA": // Morocco
                    // Pattern :[unit] [houseNumber] [streetType] streetName [zip|,] city [CEDEX [(number)]]
                    // Example :APARTMENT N° 13 ENTRANCE C IMMEUBLE ANNADA 23 BOULEVARD TAROUDANT 52000 ERRACHIDIA
                    addressFormat = "{1} {0} {2} {3}";
                    break;

                case "AW": // Aruba
                case "BQ": // Bonaire, Saint eustatius and Saba
                case "CW": // Curaçao
                case "AN": // Netherlands Antilles
                case "SX": // Sint Maarten
                case "NL": // Netherlands
                    // Pattern : streetName [houseNumber] [zip] city
                    // Example : Surinamestraat 27 2585 GJ Den Haag
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "NO": // Norway
                    // Pattern : streetName [houseNumber] [unit] [zip] city
                    // Example : Passion 21 b 6415 Bergan
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "PL": // Poland
                    // Pattern: [streetType] streetName [houseNumber] [(/)flatnumber] [dependentLocality] [zip] city
                    // Example: ul. Asfaltowa 2/3 Rudzienko 05–470 KOLBIEL
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "PT": // Portugal
                    // Pattern : streetType streetName [houseNumber|,] [[floor][side]] [zip] city
                    // Example : Lg Dr António Viana 1 2 DTO 1250–096 LISBOA
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "KZ": // Kazakhstan
                case "RU": // Russia
                    // Pattern : [StreetType] streetName HouseNumber [unit] City [District] [OBLAST|KRAY|RESPUBLIKA] [country] [zip] [country]
                    // Example : ул. Ореховая, д. 25 пос. Лесное АЛЕКСЕЕВСКИЙ р-н ВОРОНЕЖСКАЯ обл. РОССИЙСКАЯ ФЕДЕРАЦИЯ 247112
                    addressFormat = "{0} {1} {3} {4} {2}";
                    break;

                case "SA": // Saudi Arabia
                    // Pattern : [houseNumber] streetName streetType city [zip]
                    // Example : 8228 King Abdulaziz Rd RIYADH 12643
                    addressFormat = "{1} {0} {3} {2}";
                    break;

                case "SG": // Singapore
                    // Pattern : [unit] [houseNumber] streetName [streetType] city [zip]
                    // Example : 16 Sandilands Road SINGAPORE 546080
                    addressFormat = "{1} {0} {3} {2}";
                    break;

                case "ES": // Spain
                    // Pattern : [streetType] streetName [No] houseNumber [unit] zip city state
                    // Example : Calle Sarmiento 151 Piso 3 Oficina 311 BIS 39012 Palma de Majorque (Baleares)
                    addressFormat = "{0} {1} {2} {3} {4}";
                    break;

                case "SD": // Sudan
                    // Pattern : [unit] [houseNumber] [streetType] streetName [zip|,] city [CEDEX [(number)]]
                    // Example : Immeuble de la fraternité 3 Rue Al-Comhouriya 11111 KHARTOUM
                    addressFormat = "{1} {0} {2} {3}";
                    break;

                case "SE": // Sweden
                    // Pattern : streetName [houseNumber] [zip] city
                    // Example : NYBY 10 123 45 LILLBYN
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "CH": // Switzerland
                    // Pattern : streetName [streettype] houseNumber[/number|letter] [unit] zip city
                    // Example : Rhondorfer Str. 665 Appartment 47 9876 Tihidorf
                    addressFormat = "{0} {1} {2} {3}";
                    break;

                case "TR": // Turkey
                    // Pattern : [quater] streetName streetType [houseNumber] [(/)extra number info] [zip] [district] city
                    // Example : DOĞANBEY MAH. EHİTTEĞMENKALMAZ CAD. NO: 2/311 06101 ALTINDAĞ/ANKARA
                    addressFormat = "{0} {1} {2} {4} {3}";
                    break;

                case "TN": // Tunisia
                    // Pattern : [unit] [houseNumber] [streetType] streetName [zip|,] city [CEDEX [(number)]]
                    // Example : étage 2 palier 3 157 AVENUE DE LA LIBERTE 1002 TUNIS BELVEDERE
                    addressFormat = "{1} {0} {2} {3}";
                    break;

                case "UA": // Ukraine
                    // Pattern : [StreetType] streetName HouseNumber [unit] City [District] [OBLAST|KRAY|RESPUBLIKA] [zip]
                    // Example : Vul. Lisova, 2, kv.3 s. Ivanovka, Semenivsky r-n, TCHERNIGIVSKA OBL.15432
                    addressFormat = "{0} {1} {3} {2}";
                    break;

                case "FK": // Falkland Islands
                case "GI": // Gibraltar
                case "IM": // Isle of Man
                case "JE": // Jersey
                case "TC": // Turks and Caicos Islands
                case "SH": // Saint Helena
                case "GS": // South Georgia and the South Sandwich Islands
                case "GG": // Guernsey
                case "GB": // United Kingom
                    // Pattern: [unit] [houseNumber] streetName streetType city [Postown] [zip]
                    // Example: room 105 6 Oxford Road Hedle End SOUTHAMPTON HP19 3EQ
                    addressFormat = "{1} {0} {3} {2}";
                    break;

                case "AS": // American Samoa
                case "MP": // Northern Mariana Islands
                case "PR": // Puerto Rico
                case "UM": // United States Minor Outlying Islands
                case "VI": // U.S. Virgin Islands
                case "US": // United States
                    // Pattern : [houseNumber] [fraction] [predirection] streetName [streetType] [postdirection] [unit] city [state] [zip]
                    // Example : 123 3/4 N name with space 1 number blvd south floor 2 Missouri CA 12345-4536
                    addressFormat = "{1} {0} {3} {4} {2}";
                    break;

                default:
                    //
                    break;
            }

            try
            {
                if (string.IsNullOrEmpty(resAddress))
                {
                    resAddress = string.Format(addressFormat,
                            address.Street.ToXmlValidString(),
                            !string.IsNullOrEmpty(address.Number) ? address.Number.ToXmlValidString() : ",",
                            address.ZIP.ToXmlValidString(),
                            address.City.ToXmlValidString(),
                            address.State.ToXmlValidString());
                }
            }
            catch
            {
                // fallback
                resAddress = string.Format("{0} {1}, {2} {3} {4}", 
                                    address.Street.ToXmlValidString(), 
                                    address.Number.ToXmlValidString(), 
                                    address.ZIP.ToXmlValidString(), 
                                    address.City.ToXmlValidString(),
                                    address.State.ToXmlValidString()
                                    );
            }

            return resAddress;
        }

        private Address ExtractReponseAddress(XPathNavigator xPathNavigator)
        {
            Address result = new Address();
            result.Position = new GeoInformation();
            NumberFormatInfo provider = new NumberFormatInfo() { NumberDecimalSeparator = ".", NumberGroupSeparator = "," };
            result.Position.Latitude = Convert.ToDouble(xPathNavigator.SelectSingleNode("lat").Value, provider);
            result.Position.Longitude = Convert.ToDouble(xPathNavigator.SelectSingleNode("lng").Value, provider);

            var tmpVar = xPathNavigator.SelectSingleNode("streetName");
            if (tmpVar != null) result.Street = tmpVar.Value;

            tmpVar = xPathNavigator.SelectSingleNode("houseNumber");
            if (tmpVar != null) result.Number = tmpVar.Value;

            tmpVar = xPathNavigator.SelectSingleNode("zipCode");
            if (tmpVar != null) result.ZIP = tmpVar.Value;

            tmpVar = xPathNavigator.SelectSingleNode("city");
            if (tmpVar != null) result.City = tmpVar.Value;

            tmpVar = xPathNavigator.SelectSingleNode("countryCode");
            if (tmpVar != null) result.Country = tmpVar.Value;

            tmpVar = xPathNavigator.SelectSingleNode("state");
            if (tmpVar != null) result.AdminArea1 = tmpVar.Value;

            tmpVar = xPathNavigator.SelectSingleNode("district");
            if (tmpVar != null) result.AdminArea2 = tmpVar.Value;

            tmpVar = xPathNavigator.SelectSingleNode("quarter");
            if (tmpVar != null) result.AdminArea3 = tmpVar.Value;

            result.Accuracy = xPathNavigator.SelectSingleNode("geocodingLevel").Value;

            return result;
        }
    }

    public static class StringExtension
    {
        public static string ToXmlValidString(this string addressEntry)
        {
            if (string.IsNullOrEmpty(addressEntry)) return addressEntry;

            // remove strings that do not work propertly in xml requests
            List<string> invalidStrings = new List<string> { "&", "#", "<", ">", @"""", "'" };
            foreach (string invalidElem in invalidStrings)
            {
                addressEntry = addressEntry.Replace(invalidElem, string.Empty);
            }

            return SecurityElement.Escape(addressEntry);
        }
    }
}
