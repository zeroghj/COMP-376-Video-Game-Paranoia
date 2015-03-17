using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
namespace Platformer
{
    class switches
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect activateSound;
        private Vector2 basePosition;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public Circle BoundingCircle
        {
            get
            {
                return new Circle(basePosition, Tile.Width);
            }
        }

        /// <summary>
        /// Constructs a new switch.
        /// </summary>
        public switches(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;

            LoadContent();
        }
        /*public switches(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;

            LoadContent3();
        }*/
        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/switch0");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            activateSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }

        public void LoadContent2()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/switch1");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            activateSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }
        public void LoadContent3()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/switchUP");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            activateSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }
        public void LoadContent4()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/switchDOWN");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            activateSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }
        public void OnActivation(Player collectedBy)
        {
            activateSound.Play();
        }

        /// <summary>
        /// Draws a gem in the appropriate color.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, basePosition, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}
