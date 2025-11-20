public class BankoCard
{
    public string CardId { get; set; }
    public int[,] Numbers { get; set; } = new int[3, 9];
    public bool[,] Marked { get; set; } = new bool[3, 9];


    public int HighestAnnounced { get; set; } = 0;

    public BankoCard(string cardId, int[,] numbers)
    {
        CardId = cardId;
        Numbers = numbers;
    }

    public void MarkNumber(int number)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (Numbers[row, col] == number)
                    Marked[row, col] = true;
            }
        }
    }

    public int CompletedRows()
    {
        int count = 0;
        for (int row = 0; row < 3; row++)
        {
            int markedInRow = 0;
            for (int col = 0; col < 9; col++)
            {
                if (Numbers[row, col] != 0 && Marked[row, col])
                    markedInRow++;
            }
            if (markedInRow == 5)
                count++;
        }
        return count;
    }

    public bool IsFullHouse() => CompletedRows() == 3;
}

public class BankoManager
{
    public List<BankoCard> Cards { get; set; } = new List<BankoCard>();

    public void AddCard(BankoCard card)
    {
        Cards.Add(card);
    }

    public void MarkNumberOnAll(int number)
    {
        foreach (var card in Cards)
            card.MarkNumber(number);
    }

    public void CheckForWinners()
    {
        foreach (var card in Cards)
        {
            int rows = card.CompletedRows();

            if (rows >= 1 && card.HighestAnnounced < 1)
            {
                Console.WriteLine($"1 row on plate : {card.CardId}");
                card.HighestAnnounced = 1;
            }
            if (rows >= 2 && card.HighestAnnounced < 2)
            {
                Console.WriteLine($"2 rows on plate: {card.CardId}");
                card.HighestAnnounced = 2;
            }
            if (rows == 3 && card.HighestAnnounced < 3)
            {
                Console.WriteLine($"FULL PLATE ON: {card.CardId}");
                card.HighestAnnounced = 3;
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        BankoManager manager = new BankoManager();

        // Her kan du tilføje flere plader
        int[,] card1 =
        {
            { 3, 0, 0, 0, 40, 0, 63, 72, 80 },
            { 0, 12, 0, 33, 44, 55, 0, 73, 0 },
            { 0, 14, 27, 37, 0, 0, 69, 78, 0 }
        };
        manager.AddCard(new BankoCard("Martin", card1));

        int[,] card2 =
        {
            { 1, 0, 22, 0, 45, 0, 60, 0, 81 },
            { 0, 17, 0, 36, 0, 55, 0, 70, 0 },
            { 9, 0, 28, 0, 49, 0, 66, 0, 88 }
        };
        manager.AddCard(new BankoCard("Martin1", card2));

        Console.WriteLine("Enter a number. Write 'stop' if you want to exit.\n");

        while (true)
        {
            Console.Write("Number: ");
            string input = Console.ReadLine();

            if (input.ToLower() == "stop")
                break;

            if (int.TryParse(input, out int number))
            {
                manager.MarkNumberOnAll(number);
                manager.CheckForWinners();
            }
            else
            {
                Console.WriteLine("Invalid Input - write a number!");
            }
        }
    }
}
