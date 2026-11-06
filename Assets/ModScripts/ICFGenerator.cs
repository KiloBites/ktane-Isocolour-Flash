using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;
using static CFColor;
using static CFButton;

public class ICFGenerator
{
     private static readonly int[] _setupGridIndices =
     {
          0, 8, 12, 4,
          5, 1, 9, 13,
          14, 6, 3, 10,
          11, 15, 7, 2
     };

     private static readonly Dictionary<CFColor, CFColor[]> _startingSquareConfig = new Dictionary<CFColor, CFColor[]>
     {
          [Red] = new[] { Green, Blue, Yellow, Magenta, White },
          [Green] = new[] { Blue, Yellow, Red, White, Magenta },
          [Blue] = new[] { White, Magenta, Green, Yellow, Red },
          [Yellow] = new[] { Magenta, White, Blue, Red, Green },
          [Magenta] = new[] { Red, Green, White, Blue, Yellow },
          [White] = new[] { Yellow, Red, Magenta, Green, Blue }
     };

     private static readonly CFButton[][][] _logicTable =
     {
          new[]
          {
               new[] { Yes, Yes },
               new[] { No, No },
               new[] { No, Yes },
               new[] { Yes, No }
          },
          new[]
          {
               new[] { Yes, Yes },
               new[] { Yes, No },
               new[] { No, Yes },
               new[] { No, No }
          }
     };

     private readonly Dictionary<CFColor, CFMove> _colorToMove;
     private readonly CFColor[] _moveColors;

     private readonly CFColor[,] _unmodifiedGrid;
     private readonly CFColor[,] _grid;
     
     public List<Slide> GeneratedSlides { get; private set; }
     public List<List<Coord>> GeneratedAnswers { get; private set; }
     
     public readonly List<string> Logs = new List<string>();

     public readonly List<CFMove> ApplicableMoves;

     public readonly List<Coord> VoidCoordinates;

     public string LogGrid(bool unmodified) => Enumerable.Range(0, 4).Select(r => Enumerable.Range(0, 4).Select(c => $"[{(unmodified ? _unmodifiedGrid : _grid)[r, c].ToString().First()}]").Join("")).Join(";");

     public ICFGenerator(CFColor selectedColor)
     {
          _moveColors = _startingSquareConfig[selectedColor];
          _colorToMove = Enumerable.Range(0, 5).ToDictionary(x => _moveColors[x], x => (CFMove)x);

          var generatedColors = new List<CFColor> { selectedColor };
          
          foreach (var moveColor in _moveColors)
               generatedColors.AddRange(Enumerable.Repeat(moveColor, 2));
          
          while (generatedColors.Count < 16)
               generatedColors.Add(_moveColors.PickRandom());

          generatedColors.Shuffle();
          
          GeneratedSlides = GenerateSlides(generatedColors);

          _unmodifiedGrid = Enumerable.Range(0, 4).Select(r => Enumerable.Range(0, 4).Select(c => generatedColors[_setupGridIndices[r * 4 + c]])).To2DArray();
          _grid = _unmodifiedGrid.To2DArray();
          
          ModifyGrid(selectedColor);

          GeneratedAnswers = GenerateAnswers(out ApplicableMoves);

          VoidCoordinates = Enumerable.Range(0, 16).Select(x => new Coord(x / 4, x % 4)).Where(x => _colorToMove[_grid[x.Row, x.Column]] == CFMove.Void).ToList();
     }

     private void ModifyGrid(CFColor startingColor)
     {
          var currentCoord = Enumerable.Range(0, 16).Select(x => new Coord(x / 4, x % 4)).First(x => _grid[x.Row, x.Column] == startingColor);
          var currentMove = CFMove.Left;

          _grid[currentCoord.Row, currentCoord.Column] = _moveColors.First();
          
          Logs.Add($"The starting color is {startingColor} at {currentCoord}");

          while (true)
          {
               if (IsStuck(currentCoord, new[] { CFMove.Left, CFMove.Right }.Contains(currentMove)))
                    break;
               
               var newColor = Array.IndexOf(_moveColors, _grid[currentCoord.Row, currentCoord.Column]) + 1;
               
               Logs.Add($"The color at {currentCoord} ({_grid[currentCoord.Row, currentCoord.Column]}) gets changed to {_moveColors[newColor]}");
               
               _grid[currentCoord.Row, currentCoord.Column] = _moveColors[newColor];
               
               currentCoord.MakeMove(currentMove);
               
               while (_colorToMove[_grid[currentCoord.Row, currentCoord.Column]] == CFMove.Void)
                    currentCoord.MakeMove(currentMove);
               
               Logs.Add($"Moved {currentMove.ToString().ToLowerInvariant()} to {currentCoord}");
               
               currentMove = _colorToMove[_grid[currentCoord.Row, currentCoord.Column]];
          }
     }

     public static Coord GetCoordFromInput(List<CFButton> submittedButtons)
     {
          var group = submittedButtons.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 2).Select(x => x.Select(v => v.Value).ToArray()).ToList();

          var rowCol = group.Select((x, i) => _logicTable[i].IndexOf(x.SequenceEqual)).ToArray();
          
          return new Coord(rowCol[1], rowCol[0]);
     }

     public static CFButton[] CoordToSequence(Coord coord)
     {
          var column = _logicTable[0][coord.Column];
          var row =  _logicTable[1][coord.Row];

          return column.Concat(row).ToArray();
     }
     

     private static List<Slide> GenerateSlides(List<CFColor> colors)
     {
          var group = colors.Select((x, i) => new { Index = i, Color = x }).GroupBy(x => x.Index / 8).Select(x => x.Select(c => c.Color).ToArray()).ToList();

          return Enumerable.Range(0, 8).Select(x => new Slide(group[0][x], group[1][x])).ToList();
     }

     private List<List<Coord>> GenerateAnswers(out List<CFMove> availableMoves)
     {
          var answers = new List<List<Coord>>();

          var moves = ((CFMove[])Enum.GetValues(typeof(CFMove))).Where(x => x != CFMove.Void).ToArray();

          foreach (var move in moves)
          {
               var coords = new List<Coord>();
               
               coords.AddRange(Enumerable.Range(0, 16).Select(x => new Coord(x / 4, x % 4)).Where(x => _colorToMove[_grid[x.Row, x.Column]] == move).ToList());
               
               answers.Add(coords);
          }

          availableMoves = moves.Where(x => answers[(int)x].Count > 0).ToList();
          
          return answers.Where(x => x.Count > 0).ToList();
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
          
          return area.Count(x => _colorToMove[_grid[x.Row, x.Column]] == CFMove.Void) == 3;
     }
}