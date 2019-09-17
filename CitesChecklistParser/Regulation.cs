using System;
using System.Collections.Generic;
using System.Linq;

namespace CitesChecklistParser
{
    class Regulation
    {
        public int ID { get; set; }

        public string Listing { get; set; }

        public string Rank { get; set; }

        public string Phylum { get; set; }

        public string Class { get; set; }

        public string Order { get; set; }

        public string Family { get; set; }

        public string Genus { get; set; }

        public string Specie { get; set; }

        public string Subspecie { get; set; }

        public List<string> AltNames { get; set; }

        public List<string> SynonymNames { get; set; }

        public Regulation(int ID, string Listing, string Rank, string Phylum, string Class, string Order, string Family, string Genus, string Specie, string Subspecie, string[] AltNames, string[] SynonymNames)
        {
            this.ID = ID;
            this.Listing = Listing;
            this.Rank = Rank;
            this.Phylum = Phylum;
            this.Class = Class;
            this.Order = Order;
            this.Family = Family;
            this.Genus = Genus;
            this.Specie = Specie != null ? Genus + " " + Specie : null;
            this.Subspecie = Subspecie != null ? Genus + " " + Specie + " " + Subspecie : null;
            this.AltNames = new List<string>(AltNames);

            // Loop through the passed synonyms.
            this.SynonymNames = new List<string>();
            foreach (string synonym in SynonymNames)
            {
                // Add 2-3 words to list if specie or subspecie.
                if (this.Rank == "species" || this.Rank == "subspecies")
                {
                    this.SynonymNames.Add(string.Join(" ", synonym.Split().Take(2)));
                    this.SynonymNames.Add(string.Join(" ", synonym.Split().Take(3)));

                    continue;
                }

                // Add first word to list.
                this.SynonymNames.Add(synonym.Split().Take(1).First());
            }

            // Add base name to start of synonyms.
            this.SynonymNames.Insert(0, this.ResourceValue());
        }

        public string ResourceType()
        {
            switch (this.Rank)
            {
                case "subspecies": return "App\\\\Specie";
                case "species": return "App\\\\Specie";
                case "genus": return "App\\\\SpecieGenus";
                case "family": return "App\\\\SpecieFamily";
                case "order": return "App\\\\SpecieOrder";
                case "class": return "App\\\\SpecieClass";
                case "phylum": return "App\\\\SpeciePhylum";
            }

            return "@INVALID";
        }

        public string ResourceTable()
        {
            switch (this.Rank)
            {
                case "subspecies": return "species";
                case "species": return "species";
                case "genus": return "specie_genera";
                case "family": return "specie_families";
                case "order": return "specie_orders";
                case "class": return "specie_classes";
                case "phylum": return "specie_phyla";
            }

            return "@INVALID";
        }

        public string ResourceValue()
        {
            switch (this.Rank)
            {
                case "subspecies": return this.Subspecie;
                case "species": return this.Specie;
                case "genus": return this.Genus;
                case "family": return this.Family;
                case "order": return this.Order;
                case "class": return this.Class;
                case "phylum": return this.Phylum;
            }

            return "@INVALID";
        }

        public void Output()
        {
            int index = 0;
            foreach (string synonym in this.SynonymNames.Where(words => ! words.Contains("(")).Distinct())
            {
                Console.WriteLine($"SET @base = (SELECT id FROM {this.ResourceTable()} WHERE scientific = \"{synonym}\");");
                Console.WriteLine($"SET @rename = (SELECT id FROM {this.ResourceTable()} WHERE scientific LIKE \"%{synonym.Split().Last()}\" HAVING COUNT(*) = 1);");

                string query = "IFNULL(@base, @rename)";
                string binding = "IF(@base, '" + (index == 0 ? "scientific" : "synonym") + "', IF(@rename, '" + (index == 0 ? "rename" : "synonym-rename") + "', NULL))";

                Console.WriteLine("INSERT INTO regulations (`source`, `source_id`, `resource_id`, `resource_type`, `name`, `listing`, `binding`) SELECT " +
                    $"'cites', {this.ID}, {query}, '{this.ResourceType()}', \"{synonym}\", '{this.Listing}', {binding}" +
                ";");

                index++;
            }
        }
    }
}
