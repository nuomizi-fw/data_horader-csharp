using Microsoft.AspNetCore.Mvc;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace DataHoarder.Controllers
{
    public class MemoryDb
    {
        public class DB
        {
            public List<Type> types { get; set; } = new();
            public List<Info> infos { get; set; } = new();
            public List<Item> items { get; set; } = new();
        }
        public static DB db = new DB();


        public static List<string> SaveToJson()
        {
            string json = JsonSerializer.Serialize(db);
            File.WriteAllText("db.json", json);

            List<string> rets = new List<string>();

            json = JsonSerializer.Serialize(db.types);
            rets.Add(json);

            json = JsonSerializer.Serialize(db.infos);
            rets.Add(json);

            json = JsonSerializer.Serialize(db.items);
            rets.Add(json);

            return rets;
        }

        public static List<string> LoadFromJson()
        {
            List<string> rets = new List<string>();



            string json = File.ReadAllText("db.json");
            var local_db = JsonSerializer.Deserialize<DB>(json);
            rets.Add(json);

            if (local_db?.types != null) db.types = local_db.types;
            if (local_db?.infos != null) db.infos = local_db.infos;
            if (local_db?.items != null) db.items = local_db.items;

            return rets;
        }

        public static Type? GetTypeFromTypeOrName(Type? type, string? type_name)
        {
            if (type != null)
            {
                return type;
            }
            else if (type_name != null)
            {
                return MemoryDb.db.types.FirstOrDefault(t => t.name == type_name);
            }
            return null;
        }

        public static List<Type> GetTypes(Guid id)
        {
            return db.types.Where(t => t.id == id).ToList();
        }
        public static List<Info> GetInfos(Guid id)
        {
            return db.infos.Where(i => i.id == id).ToList();
        }
        public static List<Item> GetItems(Guid id)
        {
            return db.items.Where(i => i.id == id).ToList();
        }

        public static Type? TryCreate(TypeCreate? create)
        {
            if (create == null)
            {
                return null;
            }

            if (create.name == null)
            {
                return null;
            }

            var type = new Type
            {
                id = Guid.NewGuid(),
                name = create.name,
                description = create.description
            };
            db.types.Add(type);
            return type;
        }

        public static Info? TryCreate(InfoCreate create)
        {
            Type? type = GetTypeFromTypeOrName(TryCreate(create.type), create.type_name);
            if (type == null)
            {
                return null;
            }

            var info = new Info
            {
                id = Guid.NewGuid(),
                type_id = type.id,
                context = create.context,
                item_id = create.item_id
            };
            db.infos.Add(info);
            return info;
        }

        public static Item? TryCreate(ItemCreate create)
        {
            List<Guid> info_ids = new List<Guid>();
            if (create.infos != null)
            {
                foreach (var info in create.infos)
                {
                    Info? new_info = TryCreate(info);
                    if (new_info == null)
                    {
                        return null;
                    }
                    info_ids.Add(new_info.id);
                }
            }
            var item = new Item
            {
                id = Guid.NewGuid(),
                info_ids = info_ids
            };

            item.info_ids.ForEach(info_ids => MemoryDb.GetInfos(info_ids).ForEach(info => info.item_id = item.id));

            db.items.Add(item);
            return item;
        }
    }


        [ApiController]
    [Route("[controller]")]
    public class MemoryDbController : ControllerBase
    {
        private readonly ILogger<MemoryDbController> _logger;
        public MemoryDbController(ILogger<MemoryDbController> logger)
        {
            _logger = logger;
        }


        [HttpGet("save")]
        public IEnumerable<string> Save()
        {
            return MemoryDb.SaveToJson();
        }

        [HttpGet("load")]
        public IEnumerable<string> Load()
        {
            return MemoryDb.LoadFromJson();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class TypeController : ControllerBase
    {
        private readonly ILogger<TypeController> _logger;


        public TypeController(ILogger<TypeController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "types")]
        public IEnumerable<Type> Get()
        {
            return MemoryDb.db.types;
        }

        [HttpPost(Name = "types")]
        public IActionResult Post(TypeCreate type)
        {
            if (type.name == null)
            {
                return BadRequest();
            }

            MemoryDb.TryCreate(type);
            return CreatedAtRoute("types", MemoryDb.db.types);
        }

        [HttpGet("{id}", Name = "type")]
        public IActionResult Get(Guid id)
        {
            var type = MemoryDb.db.types.FirstOrDefault(t => t.id == id);

            if (type == null)
            {
                return NotFound();
            }

            return Ok(type);
        }
    }



    [ApiController]
    [Route("[controller]")]
    public class InfoController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;

        public InfoController(ILogger<InfoController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "infos")]
        public IEnumerable<Info> Get()
        {
            return MemoryDb.db.infos;
        }

        [HttpPost(Name = "infos")]
        public IActionResult Post(InfoCreate info)
        {
            MemoryDb.TryCreate(info);
            return CreatedAtRoute("infos", MemoryDb.db.infos);
        }

        [HttpGet("{id}", Name = "info")]
        public IActionResult Get(Guid id)
        {
            var info = MemoryDb.db.infos.FirstOrDefault(i => i.id == id);

            if (info == null)
            {
                return NotFound();
            }

            return Ok(info);
        }
    }


    [ApiController]
    [Route("[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly ILogger<ItemController> _logger;

        public ItemController(ILogger<ItemController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "items")]
        public IEnumerable<Item> Get()
        {
            return MemoryDb.db.items;
        }

        [HttpPost(Name = "items")]
        public IActionResult Post(ItemCreate item)
        {
            MemoryDb.TryCreate(item);
            return CreatedAtRoute("items", MemoryDb.db.items);
        }

        [HttpGet("{id}", Name = "item")]
        public IActionResult Get(Guid id)
        {
            var item = MemoryDb.db.items.FirstOrDefault(i => i.id == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }
    }
}
