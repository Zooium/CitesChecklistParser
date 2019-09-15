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

        public string[] AltNames { get; set; }

        public string[] SynonymNames { get; set; }

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
            this.AltNames = AltNames;
            this.SynonymNames = SynonymNames;
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

        public string EnglishList()
        {
            return "\"" + string.Join("\", \"", this.AltNames) + "\"";
        }

        public string SynonymList()
        {
            if (this.Rank != "species" && this.Rank != "subspecies") return "";

            int words = this.Rank == "species" ? 2 : 3;


            return "\"" + string.Join("\", \"", this.SynonymNames.Select(synonym => string.Join(" ", synonym.Split().Take(words)))) + "\"";
        }

        public void Output()
        {
            Console.WriteLine($"SET @base = (SELECT id FROM {this.ResourceTable()} WHERE scientific = '{this.ResourceValue()}');");
            Console.WriteLine($"SET @rename = (SELECT id FROM {this.ResourceTable()} WHERE scientific LIKE '%{this.ResourceValue().Split().Last()}' HAVING COUNT(*) = 1);");

            string query = "IFNULL(@base, IFNULL(@rename, {value}))";
            string binding = "IF(@base, 'scientific', IF(@rename, 'rename', {value}))";

            if (this.SynonymNames.Length != 0)
            {
                Console.WriteLine($"SET @synonym = (SELECT id FROM {this.ResourceTable()} WHERE scientific IN ({this.SynonymList()}) HAVING COUNT(*) = 1);");
                query = query.Replace("{value}", "IFNULL(@synonym, {value})");
                binding = binding.Replace("{value}", "IF(@synonym, 'synonym', {value})");
            }

            if (this.AltNames.Length != 0)
            {
                Console.WriteLine($"SET @english = (SELECT id FROM {this.ResourceTable()} WHERE english_name IN ({this.EnglishList()}) HAVING COUNT(*) = 1);");
                query = query.Replace("{value}", "IFNULL(@english, {value})");
                binding = binding.Replace("{value}", "IF(@synonym, 'english', {value})");
            }

            query = query.Replace("{value}", "NULL");
            binding = binding.Replace("{value}", "NULL");

            Console.WriteLine("INSERT INTO regulations (`source`, `source_id`, `resource_id`, `resource_type`, `name`, `listing`, `binding`) SELECT " +
                $"'cites', {this.ID}, {query}, '{this.ResourceType()}', '{this.ResourceValue()}', '{this.Listing}', {binding}" +
                
            ";");
        }
    }
}
