#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Platformer
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    class Player
    {
        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation dieAnimation;
        private Animation idleAnimation2;
        private Animation runAnimation2;
        private Animation jumpAnimation2;
        private Animation dieAnimation2;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;
        public bool ChangingDimension;
        public bool alternatedimension;
        public bool keydown = false;
        private bool phasingkeydown = false;
        public float phasingcd;
        private int shifted = 0;
        public int gemsCollected = 0;
        private bool Dimension1 = false;
        private bool Dimension2 = false;
        private bool Dimension3 = false;
        private bool Dimension4 = false;
        private bool godmode = false;
        public bool text=false;
        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;
        private SoundEffect dimensionSound;

        public Level Level
        {
            get { return level; }
        }
        Level level;
        public int getShift
        {
            get { return shifted; }
        }
        public bool getDim1
        {
            get { return Dimension1; }
        }
        public bool getDim2
        {
            get { return Dimension2; }
        }
        public bool getDim3
        {
            get { return Dimension3; }
        }
        public bool getDim4
        {
            get { return Dimension4; }
        }
        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;
        public void godmodeStatus(bool status)
        {
            godmode = status;
        }
        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;
        //regular world
        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f; 
        //alternate world
        //regular world
        // Constants for controling horizontal movement
        private const float aMoveAcceleration = 13000.0f;
        private const float aMaxMoveSpeed = 500.0f;
        private const float aGroundDragFactor = 0.70f;
        private const float aAirDragFactor = 0.70f;

        // Constants for controlling vertical movement
        private const float aMaxJumpTime = 0.70f;
        private const float aJumpLaunchVelocity = -3000.0f;
        private const float aGravityAcceleration = 2400.0f;
        private const float aMaxFallSpeed = 550.0f;
        private const float aJumpControlPower = 0.14f; 
        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        public bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
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
        /// Constructors a new player.
        /// </summary>
        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }
        public Player(Level level,Player player)
        {
            this.level = level;
            this.Dimension1 = player.Dimension1;
            this.Dimension2 = player.Dimension2;
            this.Dimension3 = player.Dimension3;
            this.Dimension4 = player.Dimension4;
            this.shifted = player.shifted;
            LoadContent();
            Reload(player);
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            idleAnimation2 = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle2"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            runAnimation2 = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run2"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            jumpAnimation2 = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump2"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);
            dieAnimation2 = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die2"), 0.1f, false);


            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }
        /// <summary>
        /// Reload the player after dimension change
        /// </summary>
        public void Reload(Player player)
        {
            Position = player.Position;
            Velocity = player.Velocity;
            alternatedimension = player.alternatedimension;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            TouchCollection touchState, 
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            if (!text)
            GetInput(keyboardState, gamePadState, touchState, accelState, orientation);
            ApplyPhysics(gameTime);
            Phasing(gameTime);
            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    if (alternatedimension)
                        sprite.PlayAnimation(runAnimation2);
                    else
                        sprite.PlayAnimation(runAnimation);
                }
                else
                {
                    if (alternatedimension)
                        sprite.PlayAnimation(idleAnimation2);
                    else
                        sprite.PlayAnimation(idleAnimation);
                }
            }
            if (ChangingDimension)
            {
                if (alternatedimension)
                {
                    alternatedimension = false;
                    ChangingDimension = false;
                }
                else
                {
                    alternatedimension = true;
                    ChangingDimension = false;
                }
            }
            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            TouchCollection touchState,
            AccelerometerState accelState, 
            DisplayOrientation orientation)
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            // Check if the player wants to jump.
            isJumping =
                gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W) ||
                touchState.AnyTouch();
            
            if (keyboardState.IsKeyDown(Keys.Q)&&!keydown){
               
                ChangingDimension = true;
                keydown = true;
            }
            if (keyboardState.IsKeyDown(Keys.E) && !keydown)
            {

                phasingkeydown = true;
            }
            if (keyboardState.IsKeyUp(Keys.Q)){
                keydown=false;
            }
            if (keyboardState.IsKeyUp(Keys.E))
            {
                phasingkeydown = false;
            }

            // tile dimension shifting
            if (keyboardState.IsKeyDown(Keys.G) && Dimension1)
            {
                shifted = 0;
                Dimension1 = false;
                Dimension2 = false;
                Dimension3 = true;
                Dimension4 = true;
            }
            if (keyboardState.IsKeyDown(Keys.G) && !Dimension1)
            {
                shifted = 1;
                Dimension1 = true;
                Dimension2 = true;
                Dimension3 = false;
                Dimension4 = false;
            }
            if (keyboardState.IsKeyDown(Keys.H) && Dimension2)
            {
                shifted = 0;
                Dimension2 = false;
                Dimension1 = true;
                Dimension3 = true;
                Dimension4 = false;
            }
            if (keyboardState.IsKeyDown(Keys.H) && !Dimension2)
            {
                shifted = 2;
                Dimension2 = true;
                Dimension1 = false;
                Dimension3 = false;
                Dimension4 = true;
            }
            if (keyboardState.IsKeyDown(Keys.J) && Dimension3)
            {
                shifted = 0;
                Dimension3 = false;
                Dimension2 = true;
                Dimension1 = false;
                Dimension4 = true;
            }
            if (keyboardState.IsKeyDown(Keys.J) && !Dimension3)
            {
                shifted = 3;
                Dimension3 = true;
                Dimension2 = false;
                Dimension1 = true;
                Dimension4 = false;
            }
            if (keyboardState.IsKeyDown(Keys.K) && Dimension4)
            {
                shifted = 0;
                Dimension4 = false;
                Dimension2 = true;
                Dimension3 = false;
                Dimension1 = true;
            }
            if (keyboardState.IsKeyDown(Keys.K) && !Dimension4)
            {
                shifted = 4;
                Dimension4 = true;
                Dimension2 = false;
                Dimension3 = true;
                Dimension1 = false;
            }
        }
        /// <summary>
        /// Apply the phasing ability of the character
        /// </summary>
        /// <param name="gameTime">game time</param>
        public void Phasing(GameTime gameTime)
        {
         /*   if (alternatedimension)
            {
                if (!phasingkeydown)
                {
                    phasingcd = -1;
                }
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

                    phasingcd -= elapsed;

                if (phasingkeydown && phasingcd <= -1 && alternatedimension)
                {
                    phasingcd = 5;
                }
                if (phasingcd > 4)
                {
                    sprite.PlayAnimation(idleAnimation);
                }
                if (phasingcd<4&&phasingcd > 2)
                {
                    sprite.PlayAnimation(idleAnimation);
                }
                if (phasingcd < 2 && phasingcd > 0)
                {
                    sprite.PlayAnimation(idleAnimation);
                }
                if (phasingcd <=0)
                {
                    sprite.PlayAnimation(dieAnimation);
                }
            }*/
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;
            if (!alternatedimension)
            {
                // Base velocity is a combination of horizontal movement control and
                // acceleration downward due to gravity.
                velocity.X += movement * MoveAcceleration * elapsed;
                velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

                velocity.Y = DoJump(velocity.Y, gameTime);

                // Apply pseudo-drag horizontally.
                if (IsOnGround)
                    velocity.X *= GroundDragFactor;
                else
                    velocity.X *= AirDragFactor;

                // Prevent the player from running faster than his top speed.            
                velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
            }
            else
            {
                // Base velocity is a combination of horizontal movement control and
                // acceleration downward due to gravity.
                velocity.X += movement * aMoveAcceleration * elapsed;
                velocity.Y = MathHelper.Clamp(velocity.Y + aGravityAcceleration * elapsed, -aMaxFallSpeed, aMaxFallSpeed);

                velocity.Y = DoJump(velocity.Y, gameTime);

                // Apply pseudo-drag horizontally.
                if (IsOnGround)
                    velocity.X *= aGroundDragFactor;
                else
                    velocity.X *= aAirDragFactor;

                // Prevent the player from running faster than his top speed.            
                velocity.X = MathHelper.Clamp(velocity.X, -aMaxMoveSpeed, aMaxMoveSpeed);
            }
            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (alternatedimension)
                        sprite.PlayAnimation(jumpAnimation2);
                    else
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    if (!alternatedimension)
                    {
                        // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                        velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                    }
                    else
                    {
                        // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                        velocityY = aJumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / aMaxJumpTime, aJumpControlPower));
                    }
                 }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,

                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision == TileCollision.Spikes&&isAlive)
                    {
                        this.OnKilled(true);

                    }
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }

                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(bool dead)
        {
            if (!godmode)
            {
                isAlive = false;

                if (dead)
                    killedSound.Play();
                else
                    fallSound.Play();
                if (alternatedimension)
                    sprite.PlayAnimation(dieAnimation2);
                sprite.PlayAnimation(dieAnimation);
            }
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            if (alternatedimension)
                sprite.PlayAnimation(jumpAnimation2);
            else
            sprite.PlayAnimation(jumpAnimation);
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip);
        }
    }
}
