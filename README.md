gisgraphy-client
================

C# client for Gisgraphy.com

files
-----
* GisgraphyClient.cs: encapsulates the communication with Gisgraphy server
* Address.cs: encapsulates an address, used as input and output for GisgraphyClient
* Program.cs: sample file showing the usage

usage
-----
1. create the source address 
```c#
Address moma = new Address();
moma.Street = "West 53 Street";
moma.Number = "11";
moma.ZIP = "10019";
moma.City = "New York";
moma.State = "NY";
moma.Country = "US";
```

2. instantiate the GisgraphyClient
```c#
GisgraphyClient gc = new GisgraphyClient();
```
If you have a Gisgraphy Premium account (if not, it's time to subscribe!), just use the provided url as parameter 1 and the api key as 2nd parameter.

3. geocode the address
```c#
var momaLocations = gc.CodeAddress(moma);
```
You will receive a list of addresses with latitude and longitue, along with additional data provided by Gisgraphy.

See Program.cs for a working sample.

license
-------
MIT license, you are allowed to use this code in open source and commercial projects.
Please credit www.geocoderpro.com e.g. using a link from your website.
