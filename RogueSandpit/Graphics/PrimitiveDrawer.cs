using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueSandpit.Graphics;

public class PrimitiveDrawer
{
    private Texture2D _pixelTexture;
    private GraphicsDevice _graphicsDevice;

    public PrimitiveDrawer(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public void DrawLine(
       SpriteBatch spriteBatch,
       Vector2 point1,
       Vector2 point2,
       Color color,
       float thickness = 1f // Default to 1 pixel thick if not specified
   )
    {
        // Calculate the distance between the two points to get the length of the line
        float length = Vector2.Distance(point1, point2);

        // Calculate the angle of the line
        // Math.Atan2(y, x) gives the angle between the positive x-axis and the point (x, y)
        // We need the difference in Y and difference in X
        float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

        // Define the rectangle for drawing.
        // It starts at point1, has a width equal to the line's length,
        // and a height equal to the desired thickness.
        // We draw from the top-left of the texture, so the origin needs to be (0, 0)
        // when scaling and rotating.
        Rectangle destinationRectangle = new Rectangle(
            (int)point1.X,
            (int)point1.Y,
            (int)length,
            (int)thickness
        );

        // Use the SpriteBatch.Draw overload with rotation and origin
        spriteBatch.Draw(
            _pixelTexture,          // The 1x1 pixel texture
            destinationRectangle,   // The destination rectangle (position and size *before* rotation)
            null,                   // Source rectangle (null means use entire texture, which is just 1x1 here)
            color,                  // The color to tint the line
            angle,                  // The angle of rotation (in radians)
            Vector2.Zero,           // The origin for rotation. (0,0) means rotate around the top-left of the texture.
            SpriteEffects.None,     // No special effects
            0f                      // Layer depth (0f means front, 1f means back)
        );
    }

    // Overload for drawing with x1, y1, x2, y2 for convenience
    public void DrawLine(
        SpriteBatch spriteBatch,
        int x1,
        int y1,
        int x2,
        int y2,
        Color color,
        float thickness = 1f
    )
    {
        DrawLine(
            spriteBatch,
            new Vector2(x1, y1),
            new Vector2(x2, y2),
            color,
            thickness
        );
    }

    public void DrawFilledRectangle(
        SpriteBatch spriteBatch,
        Rectangle rectangle,
        Color color
    )
    {
        spriteBatch.Draw(
            _pixelTexture, // The 1x1 texture
            rectangle,     // The destination rectangle (position and size)
            color          // The color to tint the rectangle
        );
    }

    // Overload for drawing with x, y, width, height for convenience
    public void DrawFilledRectangle(
        SpriteBatch spriteBatch,
        int x,
        int y,
        int width,
        int height,
        Color color
    )
    {
        DrawFilledRectangle(
            spriteBatch,
            new Rectangle(x, y, width, height),
            color
        );
    }
}