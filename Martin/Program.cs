using Npgsql;

public class BankoPlate
{
    public string Id { get; set; }
    public List<int> Row1 { get; set; }
    public List<int> Row2 { get; set; }
    public List<int> Row3 { get; set; }

    public BankoPlate(string id, List<int> r1, List<int> r2, List<int> r3)
    {
        Id = id;
        Row1 = r1;
        Row2 = r2;
        Row3 = r3;
    }

    public void CountNumbers(int number)
    {
        if (Row1.Contains(number)) Row1.Remove(number);
        if (Row2.Contains(number)) Row2.Remove(number);
        if (Row3.Contains(number)) Row3.Remove(number);
    }

    public int TotalFullRow()
    {
        int full = 0;
        if (Row1.Count == 0) full++;
        if (Row2.Count == 0) full++;
        if (Row3.Count == 0) full++;
        return full;
    }

    public string Status()
    {
        int full = TotalFullRow();
        return full switch
        {
            0 => "Ingen rækker fulde",
            1 => "1 række fuld!",
            2 => "2 rækker fulde!!",
            3 => "BANKO! Fuld plade!!!",
            _ => "Ukendt"
        };
    }
}

public class BankoRepository
{
    private readonly string _connectionString;

    public BankoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async System.Threading.Tasks.Task EnsureTableExistsAsync()
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS bankoplader (
                plade_id TEXT PRIMARY KEY,
                raekke1 INT[],
                raekke2 INT[],
                raekke3 INT[]
            );";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public void AddPlates(BankoPlate plate)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "INSERT INTO bankoplader (plade_id, raekke1, raekke2, raekke3) VALUES (@id, @r1, @r2, @r3)", conn);
        cmd.Parameters.AddWithValue("id", plate.Id);
        cmd.Parameters.AddWithValue("r1", plate.Row1.ToArray());
        cmd.Parameters.AddWithValue("r2", plate.Row2.ToArray());
        cmd.Parameters.AddWithValue("r3", plate.Row3.ToArray());
        cmd.ExecuteNonQuery();
    }

    public List<BankoPlate> GetAllPlates()
    {
        var result = new List<BankoPlate>();
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT plade_id, raekke1, raekke2, raekke3 FROM bankoplader", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            string id = reader.GetString(0);
            var r1 = (int[])reader.GetValue(1);
            var r2 = (int[])reader.GetValue(2);
            var r3 = (int[])reader.GetValue(3);
            result.Add(new BankoPlate(id, r1.ToList(), r2.ToList(), r3.ToList()));
        }
        return result;
    }
}

class Program
{
    static string connStr = Environment.GetEnvironmentVariable("neondb")
          ?? "Host=ep-jolly-thunder-a9dhgpfe-pooler.gwc.azure.neon.tech; Database=neondb; Username=neondb_owner; Password=npg_gdb7iuU4VPsO; SSL Mode=VerifyFull; Channel Binding=Require;";
    static BankoRepository repo = new BankoRepository(connStr);
    static List<BankoPlate> activePlates = new List<BankoPlate>();

    static void Main()
    {
        repo.EnsureTableExistsAsync().GetAwaiter().GetResult();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== BANKO MENU ===");
            Console.WriteLine("1. Tilføj plade");
            Console.WriteLine("2. Hent plader fra DB");
            Console.WriteLine("3. Indtast trukne tal (løbende)");
            Console.WriteLine("4. Vis status");
            Console.WriteLine("0. Afslut");
            Console.Write("Vælg: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1": AddPlate(); break;
                case "2": GetPlates(); break;
                case "3": CountNumberLoop(); break;
                case "4": ShowStatus(); break;
                case "0": return;
            }
        }
    }

    static void AddPlate()
    {
        Console.Write("Indtast plade ID: ");
        string id = Console.ReadLine();

        Console.WriteLine("Indtast 5 tal for række 1 (kommasepareret): ");
        var r1 = Console.ReadLine().Split(',').Select(int.Parse).ToList();

        Console.WriteLine("Indtast 5 tal for række 2: ");
        var r2 = Console.ReadLine().Split(',').Select(int.Parse).ToList();

        Console.WriteLine("Indtast 5 tal for række 3: ");
        var r3 = Console.ReadLine().Split(',').Select(int.Parse).ToList();

        var plate = new BankoPlate(id, r1, r2, r3);
        repo.AddPlates(plate);
        activePlates.Add(plate);
        Console.WriteLine("Plade tilføjet!");
        Console.ReadKey();
    }

    static void GetPlates()
    {
        activePlates = repo.GetAllPlates();
        Console.WriteLine($"Hentede {activePlates.Count} plader fra DB.");
        Console.ReadKey();
    }

    static void CountNumberLoop()
    {
        Console.WriteLine("Indtast trukne tal (skriv 'stop' for at afslutte):");

        while (true)
        {
            Console.Write("Tal: ");
            string input = Console.ReadLine();

            if (input.ToLower() == "stop")
                break;

            if (!int.TryParse(input, out int number))
            {
                Console.WriteLine("Ugyldigt tal, prøv igen.");
                continue;
            }

            foreach (var plate in activePlates)
            {
                int before = plate.TotalFullRow();
                plate.CountNumbers(number);
                int after = plate.TotalFullRow();

                if (after > before)
                {
                    if (after == 1)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (after == 2)
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    else if (after == 3)
                        Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine($"Plade '{plate.Id}' har nu: {plate.Status()}");
                    Console.ResetColor();
                }
            }
        }
    }

    static void ShowStatus()
    {
        foreach (var plate in activePlates)
        {
            Console.WriteLine($"{plate.Id}: {plate.Status()}");
        }
        Console.ReadKey();
    }
}
