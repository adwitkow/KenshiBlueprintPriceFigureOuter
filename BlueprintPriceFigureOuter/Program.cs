using OpenConstructionSet;
using OpenConstructionSet.Data;
using OpenConstructionSet.Models;

var installations = OcsDiscoveryService.Default.DiscoverAllInstallations();
var installation = installations.Values.First();
var baseMods = installation.Data.Mods;

var options = new OcsDataContexOptions(
    Name: Guid.NewGuid().ToString(),
    Installation: installation,
    LoadGameFiles: ModLoadType.Base,
    LoadEnabledMods: ModLoadType.Base,
    ThrowIfMissing: false);

var contextItems = OcsDataContextBuilder.Default.Build(options).Items.Values
    .Where(item => item.Type == ItemType.Armour || item.Type == ItemType.Crossbow)
    .ToList();

var priceLines = File.ReadAllLines("blueprint-prices.txt");
var prices = new Dictionary<string, int>();

foreach (var line in priceLines)
{
    var elements = line.Split("\t");
    var itemName = elements.First();
    var itemValue = Convert.ToInt32(elements.Last());

    prices.Add(itemName, itemValue);
}

var columnsPerType = new Dictionary<ItemType, List<string>>();
var rowsPerType = new Dictionary<ItemType, List<string[]>>();
foreach (var item in contextItems)
{
    if (!prices.ContainsKey(item.Name))
    {
        continue;
    }

    if (!columnsPerType.ContainsKey(item.Type))
    {
        columnsPerType.Add(item.Type, new List<string>() { "name", "blueprint price" });
    }

    var columns = columnsPerType[item.Type];

    foreach (var pair in item.Values)
    {
        if (pair.Value is FileValue)
        {
            continue;
        }

        if (!columns.Contains(pair.Key))
        {
            columns.Add(pair.Key);
        }
    }
}

foreach (var item in contextItems)
{
    if (!prices.ContainsKey(item.Name))
    {
        continue;
    }

    var columns = columnsPerType[item.Type];

    var row = new string[columns.Count];
    row[0] = item.Name;
    row[1] = prices[item.Name].ToString();

    foreach (var pair in item.Values)
    {
        var columnIndex = columns.IndexOf(pair.Key);

        if (columnIndex == -1)
        {
            continue;
        }

        row[columnIndex] = pair.Value.ToString()!.Replace("\n", "");
    }

    if (!rowsPerType.ContainsKey(item.Type))
    {
        rowsPerType.Add(item.Type, new List<string[]>() { columns.ToArray() });
    }

    var rows = rowsPerType[item.Type];
    rows.Add(row);
}

foreach (var pair in rowsPerType)
{
    File.WriteAllLines($"{pair.Key}.txt", pair.Value.Select(row => string.Join("\t", row)));
}