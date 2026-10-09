using System;
using System.Collections.Generic;
using System.Linq;
using static CFColor;
using static CFButton;
using static UnityEngine.Random;

public class ICFGenerator
{
    private static readonly CFButton[][][] _logicTable =
    {
        new[]
        {
            new[] { Yes, Yes },
            new[] { Yes, No },
            new[] { No, Yes },
            new[] { No, No }
        },
        new[]
        {
            new[] { Yes, Yes },
            new[] { No, No },
            new[] { No, Yes },
            new[] { Yes, No }
        }
    };

    private static readonly CFColor[][] _startingColors =
    {
        new[] { Green, Blue, Yellow, Magenta, White },
        new[] { Blue, Yellow, Red, White, Magenta },
        new[] { White, Magenta, Green, Yellow, Red },
        new[] { Magenta, White, Blue, Red, Green },
        new[] { Red, Green, White, Blue, Yellow },
        new[] { Yellow, Red, Magenta, Green, Blue }
    };

    private static readonly int[] _indexTable =
    {
        0, 8, 12, 4,
        5, 1, 9, 13,
        14, 6, 3, 10,
        11, 15, 7, 2
    };
    
    public List<Slide> GeneratedSlides { get; private set; }
    public List<List<CFButton>> GeneratedAnswers { get; private set; }

    private readonly CFColor[,] _grid, _modifiedGrid;
    private Coord _coord;
    private readonly CFColor[] _moveColors;

    public ICFGenerator(CFColor usedColor)
    {
        _moveColors = _startingColors[(int)usedColor];

        var startingCoord = Range(0, 16);
        
        _coord = new Coord(startingCoord / 4, startingCoord % 4);

        var randomColors = new List<CFColor>();
        
        for (int i = 0; i < 16; i++)
            randomColors.Add(i == startingCoord ? usedColor : (CFColor)Enumerable.Range(0, 6).Where(x => (CFColor)x != usedColor).PickRandom());

        var splitGroups = randomColors.Select((x, i) => new { Index = i, Color = x }).GroupBy(x => x.Index / 8).Select(x => x.Select(c => c.Color).ToArray()).ToList();

        GeneratedSlides = Enumerable.Range(0, 8).Select(x => new Slide(splitGroups[0][x], splitGroups[1][x])).ToList();

        _grid = Enumerable.Range(0, 4).Select(x => Enumerable.Range(0, 4).Select(y => randomColors[_indexTable[x * 4 + y]])).To2DArray();
        _modifiedGrid = _grid.To2DArray();

        GenerateAnswers();
    }

    public string LogGrid(bool needModifiedGrid)
    {
        var before = needModifiedGrid ? "After modifying" : "Before modifying";

        return $"{before}: {Enumerable.Range(0, 4).Select(x => Enumerable.Range(0, 4).Select(y => $"[{(needModifiedGrid ? _modifiedGrid : _grid)[x, y].ToString().First()}]").Join("")).Join(";")}";
    }

    private void GenerateAnswers()
    {
        _modifiedGrid[_coord.X, _coord.Y] = _moveColors.First();

        var lastMove = CFMove.Left;

        while (true)
        {
            if (IsStuck(_coord, new[] { CFMove.Left, CFMove.Right }.Contains(lastMove)))
                break;
            
            _modifiedGrid[_coord.X, _coord.Y] = _moveColors[Array.IndexOf(_moveColors, _modifiedGrid[_coord.X, _coord.Y]) + 1];

            _coord.MakeMove(lastMove);
            
            while (ObtainMove(_coord) == CFMove.Void)
                _coord.MakeMove(lastMove);
            
            lastMove = ObtainMove(_coord);
        }

        GeneratedAnswers = GenerateButtonSequence();
    }
    
    private CFMove ObtainMove(Coord coord) => (CFMove)_moveColors.IndexOf(x => _modifiedGrid[_coord.X, _coord.Y] == x);

    public static int GrabIndex(List<CFButton> moves)
    {
        var combinations = Enumerable.Range(0, 4).SelectMany(x => Enumerable.Range(0, 4).Select(y => _logicTable[1][y].Concat(_logicTable[0][x]).ToArray())).ToArray();

        return combinations.IndexOf(moves.SequenceEqual);
    }

    private List<List<CFButton>> GenerateButtonSequence()
    {
        var finalList = new List<List<CFButton>>();
        
        var moves = ((CFMove[])Enum.GetValues(typeof(CFMove))).Where(x => x != CFMove.Void).ToArray();

        foreach (var move in moves)
        {
            var seq = new List<CFButton>();

            var coords = Enumerable.Range(0, 16).Select(x => new Coord(x / 4, x % 4)).Where(x => ObtainMove(x) == move).ToArray();

            foreach (var coord in coords)
            {
                seq.AddRange(_logicTable[1][coord.Y]);
                seq.AddRange(_logicTable[0][coord.X]);
            }
            
            finalList.Add(seq);
        }
        
        return finalList;
    }

    private bool IsStuck(Coord coord, bool needRow)
    {
        var area = new List<Coord> { coord };

        while (area.Count < 4)
        {
            var newCell = area.Last();
            newCell.MakeMove(needRow ? CFMove.Right : CFMove.Down);
            
            area.Add(newCell);
        }

        return area.Count(x => ObtainMove(x) == CFMove.Void) == 3;
    }
}