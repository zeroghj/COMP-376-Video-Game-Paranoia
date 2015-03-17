using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Platformer
{
    class Layer
    {
        //These properties store the background texture of the layer and its scroll speed.
        public Texture2D[] Textures { get; private set; }
        public float ScrollRate { get; private set; }

        //It loads each segment of the background in the Textures array, and then sets the scroll speed.
        public Layer(ContentManager content, string basePath, float scrollRate)
        {
            // Assumes each layer only has 3 segments.
            Textures = new Texture2D[3];
            for (int i = 0; i < 3; ++i)
                Textures[i] = content.Load<Texture2D>(basePath + "_" + i);

            ScrollRate = scrollRate;
        }
        //This method first calculates which of the background segments to draw, and then draws them offset by the previously calculated amount.
        //It is assumed that two segments are enough to cover any screen.
        public void Draw(SpriteBatch spriteBatch, float cameraPosition)
        {
            // Assume each segment is the same width.
            int segmentWidth = Textures[0].Width;

            // Calculate which segments to draw and how much to offset them.
            float x = cameraPosition * ScrollRate;
            int leftSegment = (int)Math.Floor(x / segmentWidth);
            int rightSegment = leftSegment + 1;
            x = (x / segmentWidth - leftSegment) * -segmentWidth;

            spriteBatch.Draw(Textures[leftSegment % Textures.Length], new Vector2(x, 0.0f), Color.White);
            spriteBatch.Draw(Textures[rightSegment % Textures.Length], new Vector2(x + segmentWidth, 0.0f), Color.White);
        }
    }
}
