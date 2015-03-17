using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
    class Button
    {
        Texture2D texture;
        Vector2 position;
        public Rectangle rectangle;

        Color _color = new Color(255, 255, 255, 255);

        public Vector2 size;

        public Button(Texture2D newTexture, GraphicsDevice graphics)
        {
            texture = newTexture;
            size = new Vector2(80,30);
            enabled = true;

        }

        bool down;
        public bool isClicked;
        public bool enabled;

        public void Update(MouseState mouse)
        {
            rectangle = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);

            Rectangle mouseRectangle = new Rectangle(mouse.X, mouse.Y, 1, 1);

            if (mouseRectangle.Intersects(rectangle))
            {
                if (_color.A == 255)
                    down = false;

                if (_color.A == 0)
                    down = true;

                if (down)
                    _color.A += 3;
                else
                    _color.A -= 3;

                if (mouse.LeftButton == ButtonState.Pressed)
                    isClicked = true;
            }
            else if (_color.A < 255)
            {
                _color.A += 3;
                isClicked = false;
            }
        }

        public void SetPosition(Vector2 newPosition)
        {
            position = newPosition;
        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(texture, rectangle, _color);
        }
        public void SetTexture(Texture2D newTexture)
        {
            texture = newTexture;
        }
    }
}
