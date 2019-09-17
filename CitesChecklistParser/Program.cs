using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CitesChecklistParser
{
    class Program
    {
        readonly List<Regulation> Regulations = new List<Regulation>();

        static void Main(string[] args)
        {
            Program program = new Program();
            program.ProcessFile(args[0]);
            program.Output();
        }

        void ProcessFile(string file)
        {
            // Get JSON file contents and deserialize.
            string content = File.ReadAllText(file);
            dynamic parser = JsonConvert.DeserializeObject(content);

            // Loop through the list items.
            foreach (var entry in parser.Children())
            {
                // Find basic details.
                int id = (int)entry.id;
                string rank = entry.rank_name;
                string listing = entry.current_listing;

                // Find various taxonomy names.
                string phylumName = entry.phylum_name;
                string className = entry.class_name;
                string orderName = entry.order_name;
                string familyName = entry.family_name;
                string genusName = entry.genus_name;
                string specieName = entry.species_name;
                string subspecieName = entry.subspecies_name;
                string[] englishNames = entry.english_names.ToObject<string[]>();
                string[] synonymNames = entry.synonyms_with_authors.ToObject<string[]>();

                // Create new regulation instance.
                Regulation regulation = new Regulation(
                    id, listing, rank.ToLower(),
                    phylumName, className, orderName,
                    familyName, genusName, specieName,
                    subspecieName, englishNames, synonymNames
                );

                // Add created modal to list.
                this.Regulations.Add(regulation);
            }
        }

        void Output()
        {
            // Output query charset.
            Console.WriteLine("SET NAMES utf8;");

            // Output the regulation rows.
            foreach (Regulation regulation in this.Regulations) regulation.Output();

            // Delete duplicate relations.
            Console.WriteLine("" +
                "DELETE a FROM regulations a INNER JOIN regulations b WHERE " +
                    "a.id > b.id AND " +
                    "a.source = b.source AND " +
                    "a.source_id = b.source_id AND " +
                    "a.resource_id = b.resource_id;"
            );

            // Delete missing non-default synonyms.
            Console.WriteLine("" +
                "DELETE a FROM regulations a WHERE " +
                    "a.resource_id IS NULL AND " +
                    "a.id != (" +
                        "SELECT id FROM (SELECT * FROM regulations) b WHERE " +
                            "a.source = b.source AND " +
                            "a.source_id = b.source_id " +
                            "LIMIT 1" +
                    "); "
            );
        }
    }
}
