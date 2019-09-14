using System;

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

        public Regulation(int ID, string Listing, string Rank, string Phylum, string Class, string Order, string Family, string Genus, string Specie, string Subspecie)
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
        }

        public void Output()
        {
            Console.WriteLine($"INSERT INTO regulations (" +
                    $"`source`, `source_id`, `listing`, `rank`, " +
                    $"`phylum_id`, `class_id`, `order_id`, `family_id`, `genus_id`, `specie_id`, `subspecie_id`" +
                $") SELECT " +
                    $"'cites', {this.ID}, '{this.Listing}', '{this.Rank}', " +
                    (this.Phylum == null ? "null, " : $"IFNULL((SELECT id from specie_phyla WHERE `scientific` = '{this.Phylum}'), 0), ") +
                    (this.Class == null ? "null, " : $"IFNULL((SELECT id from specie_classes WHERE `scientific` = '{this.Class}'), 0), ") +
                    (this.Order == null ? "null, " : $"IFNULL((SELECT id from specie_orders WHERE `scientific` = '{this.Order}'), 0), ") +
                    (this.Family == null ? "null, " : $"IFNULL((SELECT id from specie_families WHERE `scientific` = '{this.Family}'), 0), ") +
                    (this.Genus == null ? "null, " : $"IFNULL((SELECT id from specie_genera WHERE `scientific` = '{this.Genus}'), 0), ") +
                    (this.Specie == null ? "null, " : $"IFNULL((SELECT id from species WHERE `scientific` = '{this.Specie}'), 0), ") +
                    (this.Subspecie == null ? "null;" : $"IFNULL((SELECT id from species WHERE `scientific` = '{this.Subspecie}'), 0);")
            );
        }
    }
}
