#region File Description
//-----------------------------------------------------------------------------
// Gem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Platformer
{
    /// <summary>
    /// A valuable item the player can collect.
    /// </summary>
    class Danger2
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;
        private AnimationPlayer sprite;
        private Animation stdAnimation;
        public const int PointValue = 30;
        public readonly Color Color = Color.Yellow;

        // The gem is animated from a base position along the Y axis.
        private Vector2 basePosition;
        private float bounce;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Gets the current position of this gem in world space.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;
        /// <summary>
        /// Gets a circle which bounds this gem in world space.
        /// </summary>
        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }
        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        /// <summary>
        /// Constructs a new gem.
        /// </summary>
        public Danger2(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.basePosition = position;

            LoadContent(spriteSet);
        }

        /// <summary>
        /// Loads the gem texture and collected sound.
        /// </summary>
        public void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/" + spriteSet;
            stdAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet), 0.15f, true);
            sprite.PlayAnimation(stdAnimation);

            // Calculate bounds within texture size.
            int width = (int)(stdAnimation.FrameWidth * 0.35);
            int left = (stdAnimation.FrameWidth - width) / 2;
            int height = (int)(stdAnimation.FrameWidth * 0.7);
            int top = stdAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        /// <summary>
        /// Bounces up and down in the air to entice players to collect them.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            switch (PlatformerGame.currentGameState)
            {
                case PlatformerGame.GameState.Playing:
                    float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    float posX = Position.X + localBounds.Width / 2;
                    int tileX = (int)Math.Floor(posX / Tile.Width);
                    int tileY = (int)Math.Floor(Position.Y / Tile.Height);
                    position = this.position;
                    break;
            }
        }

        /// <summary>
        /// Called when this gem has been collected by a player and removed from the level.
        /// </summary>
        /// <param name="collectedBy">
        /// The player who collected this gem. Although currently not used, this parameter would be
        /// useful for creating special powerup gems. For example, a gem could make the player invincible.
        /// </param>
        public void OnCollected(Player collectedBy)
        {
            collectedSound.Play();
        }

        /// <summary>
        /// Draws a gem in the appropriate color.
        /// </summary>
        /*public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }*/
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Stop running when the game is paused or before turning around.

            sprite.PlayAnimation(stdAnimation);
            sprite.Draw(gameTime, spriteBatch, Position, SpriteEffects.None);
        }
    }
}
