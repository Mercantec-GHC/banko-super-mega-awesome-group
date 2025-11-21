using Npgsql;
public class BankoPlade
{
    public string Id { get; set; }
    public List<int> Række1 { get; set; }
    public List<int> Række2 { get; set; }
    public List<int> Række3 { get; set; }

    public BankoPlade(string id, List<int> r1, List<int> r2, List<int> r3)
    {
        Id = id;
        Række1 = r1;
        Række2 = r2;
        Række3 = r3;
    }

    public void MarkerTal(int tal)
    {
        if (Række1.Contains(tal)) Række1.Remove(tal);
        if (Række2.Contains(tal)) Række2.Remove(tal);
        if (Række3.Contains(tal)) Række3.Remove(tal);
    }

    public int AntalFuldeRækker()
    {
        int fulde = 0;
        if (Række1.Count == 0) fulde++;
        if (Række2.Count == 0) fulde++;
        if (Række3.Count == 0) fulde++;
        return fulde;
    }

    public string Status()
    {
        int fulde = AntalFuldeRækker();
        return fulde switch
        {
            0 => "Ingen rækker fulde",
            1 => "1 række fuld!",
            2 => "2 rækker fulde!!",
            3 => "BANKO! Fuld plade!!!",
            _ => "Ukendt"
        };
    }


    public class BankoRepository
    {
        private readonly string _connectionString;

        public BankoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void TilføjPlade(BankoPlade plade)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(
                "INSERT INTO bankoplader (plade_id, række1, række2, række3) VALUES (@id, @r1, @r2, @r3)", conn);
            cmd.Parameters.AddWithValue("id", plade.Id);
            cmd.Parameters.AddWithValue("r1", plade.Række1.ToArray());
            cmd.Parameters.AddWithValue("r2", plade.Række2.ToArray());
            cmd.Parameters.AddWithValue("r3", plade.Række3.ToArray());
            cmd.ExecuteNonQuery();
        }

        public List<BankoPlade> HentAllePlader()
        {
            var result = new List<BankoPlade>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand("SELECT plade_id, række1, række2, række3 FROM bankoplader", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string id = reader.GetString(0);
                var r1 = (int[])reader.GetValue(1);
                var r2 = (int[])reader.GetValue(2);
                var r3 = (int[])reader.GetValue(3);
                result.Add(new BankoPlade(id, r1.ToList(), r2.ToList(), r3.ToList()));
            }
            return result;
        }
    }
    class Program
    {
        static string connStr = Environment.GetEnvironmentVariable("DB_CONN")
              ?? "Host=localhost;Username=postgres;Password=secret;Database=neondb;SSL Mode=Require";
        static BankoRepository repo = new BankoRepository(connStr);
        static List<BankoPlade> aktivePlader = new List<BankoPlade>();

        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== BANKO MENU ===");
                Console.WriteLine("1. Tilføj plade");
                Console.WriteLine("2. Hent plader fra DB");
                Console.WriteLine("3. Marker tal");
                Console.WriteLine("4. Vis status");
                Console.WriteLine("0. Afslut");
                Console.Write("Vælg: ");
                var valg = Console.ReadLine();

                switch (valg)
                {
                    case "1": TilføjPlade(); break;
                    case "2": HentPlader(); break;
                    case "3": MarkerTal(); break;
                    case "4": VisStatus(); break;
                    case "0": return;
                }
            }
        }

        static void TilføjPlade()
        {
            Console.Write("Indtast plade ID: ");
            string id = Console.ReadLine();

            Console.WriteLine("Indtast 5 tal for række 1 (kommasepareret): ");
            var r1 = Console.ReadLine().Split(',').Select(int.Parse).ToList();

            Console.WriteLine("Indtast 5 tal for række 2: ");
            var r2 = Console.ReadLine().Split(',').Select(int.Parse).ToList();

            Console.WriteLine("Indtast 5 tal for række 3: ");
            var r3 = Console.ReadLine().Split(',').Select(int.Parse).ToList();

            var plade = new BankoPlade(id, r1, r2, r3);
            repo.TilføjPlade(plade);
            aktivePlader.Add(plade);
            Console.WriteLine("Plade tilføjet!");
            Console.ReadKey();
        }

        static void HentPlader()
        {
            aktivePlader = repo.HentAllePlader();
            Console.WriteLine($"Hentede {aktivePlader.Count} plader fra DB.");
            Console.ReadKey();
        }

        static void MarkerTal()
        {
            Console.Write("Indtast tal der er råbt op: ");
            int tal = int.Parse(Console.ReadLine());
            foreach (var plade in aktivePlader)
            {
                plade.MarkerTal(tal);
            }
            Console.WriteLine("Tal markeret!");
            Console.ReadKey();
        }

        static void VisStatus()
        {
            foreach (var plade in aktivePlader)
            {
                Console.WriteLine($"{plade.Id}: {plade.Status()}");
            }
            Console.ReadKey();
        }
    }
}