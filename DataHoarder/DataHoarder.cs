namespace DataHoarder
{
    public class Type
    {
        public required Guid id { get; set; }
        public required string name { get; set; }
        public string? description { get; set; }
        public Guid? inherit { get; set; }
    }

    public class TypeCreate
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public string? inherit { get; set; }
    }

    public class Info
    {
        public required Guid id { get; set; }
        public required Guid type_id { get; set; }
        public string? context { get; set; }

        public Guid? item_id { get; set; }
    }

    public class InfoCreate
    {
        public TypeCreate? type { get; set; }
        public string? type_name { get; set; }
        public string? context { get; set; }
        public Guid? item_id { get; set; }
    }

    public class Item
    {
        public required Guid id { get; set; }
        public required List<Guid> info_ids { get; set; }
    }

    public class ItemCreate
    {
        public List<InfoCreate>? infos { get; set; }
    }


}
