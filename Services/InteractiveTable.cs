
namespace SignalRWebApp.Services;
public class InteractiveTable
{
    public readonly int size;
    private string[,] table;

    public InteractiveTable()
    {
        size = 25;
        table = new string[size, size];
    }

    public void ColorCell(string id, string color)
    {
        string[] info = id.Split(" ");
        table[int.Parse(info[1]), int.Parse(info[2])] = color;
    }

    public IEnumerable<string> GetTableCalls()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (table[i, j] != null)
                    yield return $"cell {i} {j}:{table[i, j]}";
                else
                    continue;
            }
        }
    }
}

