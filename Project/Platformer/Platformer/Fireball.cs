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
    class Fireball
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect activateSound;
        private Vector2 basePosition;
        private Vector2 velocity;
        private Vector2 position;
        public Vector2 size { get; set; }      //  sprite size in pixels
        private Vector2 screenSize { get; set; } //  screen size
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
        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/Fireball");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            activateSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
        }
        public void setVelocity(Vector2 speed)
        {
            velocity = speed;
        }
        public void setbase(Vector2 basepos)
        {
            basePosition = basepos;
        }
        public Fireball(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;

            LoadContent();
        }
        public void Move()
        {
            //  if we´ll move out of the screen, invert velocity

            //  checking right boundary
            if (position.X + size.X + velocity.X > screenSize.X)
                velocity = new Vector2(-velocity.X, velocity.Y);
            //  checking bottom boundary
            if (position.Y + size.Y + velocity.Y > screenSize.Y)
                velocity = new Vector2(velocity.X, -velocity.Y);
            //  checking left boundary
            if (position.X + velocity.X < 0)
                velocity = new Vector2(-velocity.X, velocity.Y);
            //  checking bottom boundary
            if (position.Y + velocity.Y < 0)
                velocity = new Vector2(velocity.X, -velocity.Y);

            //  since we adjusted the velocity, just add it to the current position
            position += velocity;
        }

        public void OnActivation(Player collectedBy)
        {
            activateSound.Play();
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, basePosition, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}
