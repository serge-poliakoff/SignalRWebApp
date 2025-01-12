namespace SignalRWebApp.Models
{
    public class Group
    {
        public readonly int size;
        private string[,] table;
        public readonly string Name;
        public int participants = 1;

        public Group()
        {
            size = 0;
            Name = string.Empty;
        }
        public Group(string name, int tableSize)
        {
            Name = name;
            size = tableSize;
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
}
