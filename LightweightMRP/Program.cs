using System.ComponentModel.DataAnnotations;
using static System.Reflection.Metadata.BlobBuilder;

struct Item
{
    public string ItemNo { get; set; }
    public string Name { get; set; }
    public int Demand { get; set; }
    public Dictionary<Item, int> BOM { get; set; }

    public Item (string itemNo, string name, int demand)
    {
        BOM = new Dictionary<Item, int> ();
        ItemNo = itemNo;
        Name = name;
        Demand = demand;
    }

    public void AddItemToBOM(Item item, int quantity)
    { 
        BOM.Add(item, quantity);
    }

    public override string ToString()
    {
        var description = string.Empty;

        if (BOM.Count > 0)
        {
            description = $"ItemNo: {ItemNo} Name: {Name} Demand: {Demand}";

            foreach(var subItem in BOM)
            {
                description += $"{Environment.NewLine} + SubItemNo: {ItemNo} Name: {Name}";
            }
        }
        else
        {
            description = $"ItemNo: {ItemNo} Name: {Name} Demand: {Demand}";
        }

        return description;
    }
}

class Program
{
    static void Main()
    {
        #region create items
        var itemsCount = 0;
        var itemList = new List<Item>();
        var itemPlug = new Item("A", "Plug", 100);
        var itemExtensionCable = new Item("B", "Extension Cable", 200);
        var itemSocket = new Item("C", "Socket", 50);
        var itemPlugCoverSet = new Item("D", "Plug Cover Set", 0);
        var itemBodyPlug = new Item("E", "Body Plug", 0);
        var itemCable = new Item("F", "Cable", 0);
        var itemBodySocket = new Item("G", "Body Socket", 0);
        var itemLidSet = new Item("H", "Lid Set", 0);
        var itemPin = new Item("I", "Pin", 0);
        var itemScrew = new Item("J", "Screw", 0);
        var itemClamp = new Item("K", "Clamp", 0);

        // A consists of D and E
        itemPlug.AddItemToBOM(itemPlugCoverSet, 1);
        itemPlug.AddItemToBOM(itemBodyPlug, 1);

        // B consists of A, F and C
        itemExtensionCable.AddItemToBOM(itemPlug, 1);
        itemExtensionCable.AddItemToBOM(itemSocket, 1);
        itemExtensionCable.AddItemToBOM(itemCable, 1);

        // C consists of G and H
        itemSocket.AddItemToBOM(itemBodySocket, 1);
        itemSocket.AddItemToBOM(itemLidSet, 1);

        // E consists of 2x I, 5x J and 2x K
        itemBodyPlug.AddItemToBOM(itemPin, 2);
        itemBodyPlug.AddItemToBOM(itemScrew, 5);
        itemBodyPlug.AddItemToBOM(itemClamp, 2);

        // G consists of 3x J and 2x K
        itemBodySocket.AddItemToBOM(itemScrew, 3);
        itemBodySocket.AddItemToBOM(itemClamp, 2);

        itemList.Add(itemPlug);
        itemList.Add(itemExtensionCable);
        itemList.Add(itemSocket);
        itemList.Add(itemPlugCoverSet);
        itemList.Add(itemBodyPlug);
        itemList.Add(itemCable);
        itemList.Add(itemBodySocket);
        itemList.Add(itemLidSet);
        itemList.Add(itemPin);
        itemList.Add(itemScrew);
        itemList.Add(itemClamp);
        itemsCount = itemList.Count;
        #endregion

        #region build the demand matrix
        var demandMatrix = new double[itemsCount, itemsCount];
        var unitMatrix = new double[itemsCount, itemsCount];
        var invertedlDemandMatrix = new double[itemsCount, itemsCount];
        var totalDemandMatrix = new double[itemsCount, itemsCount];

        for (int i = 0; i < itemsCount; i++)
        {
            for (int j = 0; j < itemsCount; j++)
            {
                if (i == j)
                {
                    demandMatrix[i, j] = 1;
                }
                else
                {
                    if (itemList[j].BOM.Any(x => x.Key.Equals(itemList[i])))
                    {
                        demandMatrix[i, j] = itemList[j].BOM.First(x => x.Key.Equals(itemList[i])).Value;
                    }
                    else
                    {
                        demandMatrix[i, j] = 0;
                    }
                }
            }
        }

        Console.WriteLine("demand matrix");
        Console.WriteLine(OutputMatrix(demandMatrix));
        Console.WriteLine("");
        #endregion

        #region build the unit matrix
        for (int i = 0; i < itemsCount; i++)
        {
            for (int j = 0; j < itemsCount; j++)
            {
                if (i == j)
                {
                    unitMatrix[i, j] = 1;
                }
                else
                {
                    unitMatrix[i, j] = 0;
                }
            }
        }
        #endregion

        #region build the inverted demand matrix
        invertedlDemandMatrix = NormalizeMatrix(InvertMatrix(demandMatrix));
        Console.WriteLine("inverted demand matrix");
        Console.WriteLine(OutputMatrix(invertedlDemandMatrix));
        #endregion

        #region caculate the total demand
        var totalAmount = 0.00;

        for (int i = 0; i < itemsCount; i++)
        {
            for (int j = 0; j < itemsCount; j++)
            {
                totalAmount += invertedlDemandMatrix[i, j] * itemList[j].Demand;
            }

            Console.WriteLine("Product " + itemList[i].ItemNo + ": " + totalAmount);
            totalAmount = 0.00;
        }
        #endregion
    }

    #region matrix operations
    static string OutputMatrix(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        var matrixString = string.Empty;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                matrixString += matrix[i, j] + " ";
            }

            matrixString += Environment.NewLine;
        }

        return matrixString;
    }

    public static double[,] NormalizeMatrix(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        double[,] result = new double[n, n * 2];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                result[i, j] = Math.Abs(matrix[i, j]);
            }
        }

        return result;
    }

    public static double[,] InvertMatrix(double[,] demandMatrix)
    {
        int n = demandMatrix.GetLength(0);
        double[,] augmented = new double[n, n * 2];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                augmented[i, j] = demandMatrix[i, j];
                augmented[i, j + n] = (i == j) ? 1 : 0;
            }
        }

        // apply Gaussian elimination
        for (int i = 0; i < n; i++)
        {
            int pivotRow = i;
            for (int j = i + 1; j < n; j++)
            {
                if (Math.Abs(augmented[j, i]) > Math.Abs(augmented[pivotRow, i]))
                {
                    pivotRow = j;
                }
            }

            if (pivotRow != i)
            {
                for (int k = 0; k < 2 * n; k++)
                {
                    double temp = augmented[i, k];
                    augmented[i, k] = augmented[pivotRow, k];
                    augmented[pivotRow, k] = temp;
                }
            }

            if (Math.Abs(augmented[i, i]) < 1e-10)
            {
                return null;
            }

            // row normalization
            double pivot = augmented[i, i];
            for (int j = 0; j < 2 * n; j++)
            {
                augmented[i, j] /= pivot;
            }

            // column elimination
            for (int j = 0; j < n; j++)
            {
                if (j != i)
                {
                    double factor = augmented[j, i];
                    for (int k = 0; k < 2 * n; k++)
                    {
                        augmented[j, k] -= factor * augmented[i, k];
                    }
                }
            }
        }

        // extracting and returning the inverse
        double[,] result = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                result[i, j] = augmented[i, j + n];
            }
        }

        return result;
    }
    #endregion
}

