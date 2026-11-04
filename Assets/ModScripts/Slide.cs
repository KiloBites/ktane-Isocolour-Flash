using UnityEngine;
using static CFColor;

public struct Slide
{
    public CFColor Word { get; private set; }
    private CFColor _colour;

    public Slide(CFColor word, CFColor colour)
    {
        Word = word;
        _colour = colour;
    }

    public override string ToString() => $"[Word: {Word}, Colour: {_colour}]";

    private static readonly Color[] _colors =
    {
        Color.red,
        Color.green,
        Color.blue,
        new Color(1, 1, 0),
        Color.magenta,
        Color.white,
        new Color(0.25f, 0.25f, 0.25f)
    };
    
    public Color GetColor() => _colors[(int)_colour];

    public void Mark(bool markingWord)
    {
        if (markingWord)
            Word = Black;
        else
            _colour = Black;
    }
}
