using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueSandpit.Graphics;

public class PixelFont
{
    private static readonly Dictionary<char, string[]> Glyphs = new()
    {
        [' '] = ["000", "000", "000", "000", "000", "000", "000"],
        ['?'] = ["111", "001", "001", "010", "010", "000", "010"],
        ['A'] = ["01110", "10001", "10001", "11111", "10001", "10001", "10001"],
        ['C'] = ["01111", "10000", "10000", "10000", "10000", "10000", "01111"],
        ['D'] = ["11110", "10001", "10001", "10001", "10001", "10001", "11110"],
        ['E'] = ["11111", "10000", "10000", "11110", "10000", "10000", "11111"],
        ['G'] = ["01111", "10000", "10000", "10111", "10001", "10001", "01111"],
        ['H'] = ["10001", "10001", "10001", "11111", "10001", "10001", "10001"],
        ['I'] = ["11111", "00100", "00100", "00100", "00100", "00100", "11111"],
        ['L'] = ["10000", "10000", "10000", "10000", "10000", "10000", "11111"],
        ['M'] = ["10001", "11011", "10101", "10101", "10001", "10001", "10001"],
        ['N'] = ["10001", "11001", "10101", "10011", "10001", "10001", "10001"],
        ['O'] = ["01110", "10001", "10001", "10001", "10001", "10001", "01110"],
        ['P'] = ["11110", "10001", "10001", "11110", "10000", "10000", "10000"],
        ['R'] = ["11110", "10001", "10001", "11110", "10100", "10010", "10001"],
        ['S'] = ["01111", "10000", "10000", "01110", "00001", "00001", "11110"],
        ['T'] = ["11111", "00100", "00100", "00100", "00100", "00100", "00100"],
        ['V'] = ["10001", "10001", "10001", "10001", "10001", "01010", "00100"],
        ['W'] = ["10001", "10001", "10001", "10101", "10101", "10101", "01010"],
        ['Y'] = ["10001", "10001", "01010", "00100", "00100", "00100", "00100"],
        ['0'] = ["01110", "10001", "10011", "10101", "11001", "10001", "01110"],
        ['1'] = ["00100", "01100", "00100", "00100", "00100", "00100", "01110"],
        ['2'] = ["01110", "10001", "00001", "00010", "00100", "01000", "11111"],
        ['3'] = ["11110", "00001", "00001", "01110", "00001", "00001", "11110"],
        ['4'] = ["00010", "00110", "01010", "10010", "11111", "00010", "00010"],
        ['5'] = ["11111", "10000", "10000", "11110", "00001", "00001", "11110"],
        ['6'] = ["01110", "10000", "10000", "11110", "10001", "10001", "01110"],
        ['7'] = ["11111", "00001", "00010", "00100", "01000", "01000", "01000"],
        ['8'] = ["01110", "10001", "10001", "01110", "10001", "10001", "01110"],
        ['9'] = ["01110", "10001", "10001", "01111", "00001", "00001", "01110"]
    };

    private readonly PrimitiveDrawer _drawer;

    public PixelFont(GraphicsDevice graphicsDevice)
    {
        _drawer = new PrimitiveDrawer(graphicsDevice);
    }

    public int MeasureWidth(string text, int scale)
    {
        int width = 0;
        foreach (char character in text.ToUpperInvariant())
        {
            string[] glyph = Glyphs.GetValueOrDefault(character, Glyphs['?']);
            width += (glyph[0].Length + 1) * scale;
        }

        return text.Length == 0 ? 0 : width - scale;
    }

    public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, int scale, Color color)
    {
        int cursorX = (int)position.X;
        foreach (char character in text.ToUpperInvariant())
        {
            string[] glyph = Glyphs.GetValueOrDefault(character, Glyphs['?']);
            for (int row = 0; row < glyph.Length; row++)
            {
                for (int column = 0; column < glyph[row].Length; column++)
                {
                    if (glyph[row][column] == '1')
                    {
                        _drawer.DrawFilledRectangle(spriteBatch,
                            new Rectangle(cursorX + column * scale, (int)position.Y + row * scale, scale, scale),
                            color);
                    }
                }
            }

            cursorX += (glyph[0].Length + 1) * scale;
        }
    }
}
