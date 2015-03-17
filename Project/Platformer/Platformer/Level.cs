#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;


namespace Platformer
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        // Physical structure of the level.
        private bool activated = false;
        public bool cutscene = false;
        private Tile[,] tiles;
        private string[] dialogue;
        private Layer[] layers;
        private Texture2D[] temp;
        public Ghost2 ghost;
        //temp = new Texture2D[1];
        //Story dialogue counter.
        private int chapter = 0;
        private int textCount = 0;
        private int dialogueIndex = 0;
        private int counter;
        //(Content.Load<Texture2D>("Tiles/changeable"));

        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;
        public bool rdimension;
        private Gem[] gems = new Gem[3];
        public List<Ghost2> ghouls = new List<Ghost2>();
        private List<Enemy> enemies = new List<Enemy>();
        private List<Enemy> memoryenemies = new List<Enemy>();
        private List<Danger1> dangers = new List<Danger1>();
        private List<Danger2> dangers2 = new List<Danger2>();
        private List<Danger3> dangers3 = new List<Danger3>();
        private List<switches> levers = new List<switches>();
        private List<Tile> changeables = new List<Tile>();
        private List<Tile> Dimension1 = new List<Tile>();
        private List<Tile> Dimension2 = new List<Tile>();
        private List<Tile> Dimension3 = new List<Tile>();
        private List<Tile> Dimension4 = new List<Tile>();
        private List<Tile> RedHints = new List<Tile>();
        private List<Tile> BlueHints = new List<Tile>();
        private List<Tile> GreenHints = new List<Tile>();
        private List<Tile> PurpleHints = new List<Tile>();
        private Fireball projectile;
        public Ghost2[] phantom;
        public bool levelStart;
        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed
        private float cameraPositionXAxis;
        public float cameraPositionYAxis; 

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;
        //reloading position
        private Player pastPlayer;

        private SoundEffect exitReachedSound;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");
            levelStart = true;
            gems[0] = new Gem(this, new Vector2(-50, -50));
            gems[1] = new Gem(this, new Vector2(-50, -50));
            gems[2] = new Gem(this, new Vector2(-50, -50));
            //projectile = new Fireball();
            //Vector2 velocity = new Vector2(-1,0);
            //projectile.setVelocity(velocity);
            timeRemaining = TimeSpan.FromMinutes(2.0);
            if (levelIndex % 2 == 0)
            {
                rdimension = true;
            }
            else
            {
                rdimension = false;
            }
            LoadTiles(fileStream, null,null);
            phantom = new Ghost2[ghouls.Count];
            for (int i = 0; i < ghouls.Count; i++)
            {
                phantom[i] = ghouls[i];
            }
 
            //textBox = new Texture2D Content.Load<Texture2D>("Overlays/textBox2");
            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);
            layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex, Level pastlevel, Level savestate, Ghost2[] ghosty)
        {
            
            //textBox = Content.Load<Texture2D>("Overlays/textBox2");
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            //projectile = new Fireball();
            //Vector2 velocity = new Vector2(-1,0);
            //projectile.setVelocity(velocity);
            gems = savestate.gems;
            //ghouls = savestate.ghouls;
            pastPlayer = pastlevel.player;
            activated = pastlevel.activated;
            timeRemaining = TimeSpan.FromMinutes(2.0);
            if (levelIndex % 2 == 0)
            {
                rdimension = true;
            }
            else
            {
                rdimension = false;
            }
            phantom = new Ghost2[ghosty.Length];
            for (int i = 0; i < ghosty.Length; i++)
            {
                phantom[i] = ghosty[i];
            }
            LoadTiles(fileStream, savestate, ghosty);
            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/Layer1", 0.5f);
            layers[2] = new Layer(Content, "Backgrounds/Layer2", 0.8f);
            
            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }
        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>

        List<string> text = new List<string>();



        private void LoadTiles(Stream fileStream, Level savestate, Ghost2[] ghosty)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y, savestate, ghosty);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y, Level savestate, Ghost2[] ghosty)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'M':
                    if (savestate == null || !gems[0].isCollected)
                        return LoadGemTile(x, y, 0);
                    return new Tile(null, TileCollision.Passable);
                case 'm':
                    if (savestate == null || !gems[1].isCollected)
                        return LoadGemTile(x, y, 1);
                    return new Tile(null, TileCollision.Passable);
                case 's':
                    if (savestate == null || !gems[2].isCollected)
                        return LoadGemTile(x, y, 2);
                    return new Tile(null, TileCollision.Passable);
                // Level switch for traps and platforms
                case 'W':
                    return LoadSwitchTile(x, y);
                case 'V':
                    return LoadSwitchTile2(x, y);
                case 'Z':
                    return LoadDangerTile(x, y);
                case '5':
                    return LoadDangerTile2(x, y, "RisingSpears");
                case '3':
                    return LoadDangerTile3(x, y);
                // Floating platform
                case '-':
                    //return LoadTile("Platform", TileCollision.Platform);
                    return LoadTile("Platform2", TileCollision.Platform);

                // Various enemies
                case 'A':
                    return LoadEnemyTile(x, y, "MonsterA", savestate);
                case 'B':
                    return LoadEnemyTile(x, y, "MonsterB", savestate);
                case 'C':
                    return LoadEnemyTile(x, y, "MonsterC", savestate);
                case 'D':
                    return LoadEnemyTile(x, y, "MonsterD", savestate);
                case 'H':
                    if (savestate==null)
                    return LoadGhostTile(x, y);
                    else
                    return LoadGhostTile2(x,y,ghosty);

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);
                // Block that changes
                case 'T':
                    {
                        changeables.Add(LoadChangeTile("changeable1", TileCollision.Passable));
                        return LoadChangeTile("changeable1", TileCollision.Passable);
                    }
                case 'Q':
                    {
                        Dimension1.Add(LoadDimension1("Dimension1", TileCollision.Passable));
                        return LoadDimension1("Dimension1", TileCollision.Passable);
                        //return LoadDimension1("Dimension0", TileCollision.Passable);
                    }
                case 'E':
                    {
                        Dimension2.Add(LoadDimension2("Dimension2_1", TileCollision.Passable));
                        //return LoadDimension1("Dimension0", TileCollision.Passable);
                        return LoadDimension2("Dimension2_0", TileCollision.Passable);
                    }
                case 'R':
                    {
                        Dimension3.Add(LoadDimension3("Dimension3_1", TileCollision.Passable));
                        //return LoadDimension1("Dimension0", TileCollision.Passable);
                        return LoadDimension3("Dimension3_1", TileCollision.Passable);
                    }
                case 'Y':
                    {
                        Dimension4.Add(LoadDimension4("Dimension4_1", TileCollision.Passable));
                        //return LoadDimension1("Dimension0", TileCollision.Passable);
                        return LoadDimension4("Dimension4_1", TileCollision.Passable);
                    }
                case 'r':
                    {
                        //Red Hint Tile
                        return LoadRedHint("redTile", TileCollision.Passable);
                    }
                case 'b':
                    {//blue Hint Tile
                        return LoadBlueHint("bluetile", TileCollision.Passable);
                        //return LoadDimension1("Dimension0", TileCollision.Passable);
                    }
                case 'g':
                    {//green Hint Tile
                        return LoadGreenHint("greenTile", TileCollision.Passable);
                    }
                case 'p':
                    {//purple Hint Tile
                        return LoadPurpleHint("purpletile", TileCollision.Passable);
                    }
                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);
                case 'S':
                    return LoadVarietyTile("spikes", 2, TileCollision.Spikes);
                case '!':
                    return LoadVarietyTile("deathtilesL", 2, TileCollision.Spikes);
                case '@':
                    return LoadVarietyTile("deathtilesR", 2, TileCollision.Spikes);
                case '$':
                    return LoadVarietyTile("deathtilesDown", 2, TileCollision.Spikes);
                case '^':
                    return LoadVarietyTile("deathtilesUp", 2, TileCollision.Spikes);
                
                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }


        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }
        private Tile LoadChangeTile(string name, TileCollision collision)
        {
            changeables.Add(new Tile(Content.Load<Texture2D>("Tiles/" + name), collision));
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, 3);
        }
        private Tile LoadDimension1(string name, TileCollision collision)
        {
            Dimension1.Add(new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, 4));
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, 4);
        }
        private Tile LoadDimension2(string name, TileCollision collision)
        {
            Dimension2.Add(new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, 5));
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, 5);
        }
        private Tile LoadDimension3(string name, TileCollision collision)
        {
            Dimension3.Add(new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, 6));
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, 6);
        }

        private Tile LoadDimension4(string name, TileCollision collision)
        {
            Dimension4.Add(new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, 7));
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, 7);
        }
        private Tile LoadRedHint(string name, TileCollision collision)
        {
            RedHints.Add(new Tile(Content.Load<Texture2D>("Tiles/" + name), collision));
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }
        private Tile LoadBlueHint(string name, TileCollision collision)
        {
            BlueHints.Add(new Tile(Content.Load<Texture2D>("Tiles/" + name), collision));
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }
        private Tile LoadGreenHint(string name, TileCollision collision)
        {
            GreenHints.Add(new Tile(Content.Load<Texture2D>("Tiles/" + name), collision));
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }
        private Tile LoadPurpleHint(string name, TileCollision collision)
        {
            PurpleHints.Add(new Tile(Content.Load<Texture2D>("Tiles/" + name), collision));
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }
        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            if (pastPlayer == null)
            {
                player = new Player(this, start);
            }
            else
            {
                player = new Player(this, pastPlayer);
            }

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadEnemyTile(int x, int y, string spriteSet, Level savestate)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            if (savestate == null)
                enemies.Add(new Enemy(this, position, spriteSet));
            else
                enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadGhostTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            ghouls.Add(new Ghost2(this, new Vector2(position.X, position.Y), false));
            ghost = new Ghost2(this, new Vector2(position.X, position.Y),false);
            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadGhostTile2(int x, int y, Ghost2[] ghosty)
        {
            Point position = GetBounds(x, y).Center;
            if (ghosty != null && ghosty[counter] != null)
                ghouls.Add(new Ghost2(this, new Vector2(position.X, position.Y), phantom[counter].isCollected));
            counter++;
            return new Tile(null, TileCollision.Passable);
        }
        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private Tile LoadGemTile(int x, int y, int number)
        {
            Point position = GetBounds(x, y).Center;
            gems[number] = (new Gem(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadDangerTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            dangers.Add(new Danger1(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadDangerTile2(int x, int y, string spriteset)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            dangers2.Add(new Danger2(this, position, spriteset));

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadDangerTile3(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            dangers3.Add(new Danger3(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadSwitchTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            levers.Add(new switches(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadSwitchTile2(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            levers.Add(new switches(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            TouchCollection touchState, 
            AccelerometerState accelState,
            DisplayOrientation orientation,
            bool[] Progress)
        {
            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, keyboardState, gamePadState, touchState, accelState, orientation);
                UpdateGems(gameTime, Progress);
                UpdateSwitches(gameTime);
                UpdateDangers(gameTime);
                UpdateDangers2(gameTime);
                UpdateDangers3(gameTime);
                UpdateDimension(gameTime);
                UpdateTiles(gameTime);
                UpdateGhost(gameTime);
                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(true);

                UpdateEnemies(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }
            for (int i = 0; i < gems.Length; i++)
            {
                if (Progress[i + 1])
                    gems[i].isCollected = true;
            }
            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime, bool[] Progress)
        {
            for (int i = 0; i < gems.Length; ++i)
            {
                if (gems[i] != null && !gems[i].isCollected)
                {

                    gems[i].Update(gameTime);

                    if (gems[i].BoundingCircle.Intersects(Player.BoundingRectangle))
                    {
                        gems[i].isCollected = true;
                        OnGemCollected(gems[i], Player);
                        Progress[i + 1] = true;
                    }
                }
            }
        }
        private void UpdateGhost(GameTime gameTime)
        {
            for (int i = 0; i < ghouls.Count; ++i)
            {
                Ghost2 ghastly = ghouls[i];

                ghastly.Update(gameTime);

                if (ghastly.BoundingCircle.Intersects(Player.BoundingRectangle)&&!ghouls[i].isCollected)
                {
                    phantom[i].isCollected = true;
                    ghouls[i].isCollected = true;
                    OnGhostContact(ghost, Player);
                }
            }

        }
        private void UpdateSwitches(GameTime gameTime)
        {
            for (int i = 0; i < levers.Count; ++i)
            {
                switches lever = levers[i];
                if (lever.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    if (!activated)
                    {
                        lever.LoadContent2();
                        activated = true;

                    }
                }
            }
        }
        private void UpdateDangers(GameTime gameTime)
        {
            for (int i = 0; i < dangers.Count; ++i)
            {
                Danger1 dangerous = dangers[i];

                dangerous.Update(gameTime);

                if (dangerous.BoundingCircle.Intersects(Player.BoundingRectangle))
                {

                    //OnGemCollected(gem, Player);
                    OnPlayerKilled(true);
                }
            }
        }
        private void UpdateDangers2(GameTime gameTime)
        {
            for (int i = 0; i < dangers2.Count; ++i)
            {
                Danger2 dangerous = dangers2[i];

                dangerous.Update(gameTime);

                if (dangerous.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    OnPlayerKilled(true);
                }
            }
        }
        private void UpdateDangers3(GameTime gameTime)
        {
            for (int i = 0; i < dangers3.Count; ++i)
            {
                Danger3 dangerous = dangers3[i];

                dangerous.Update(gameTime);

                if (dangerous.BoundingCircle.Intersects(Player.BoundingRectangle))
                {

                    //OnGemCollected(gem, Player);
                    OnPlayerKilled(true);
                }
            }
        }
        private void UpdateChangeable(GameTime gameTime)
        {
            for (int i = 0; i < changeables.Count; ++i)
            {
                Tile ccc = changeables[i];
                if (activated)
                {

                }
            }
        }
        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        /// 




        private void UpdateTiles(GameTime gameTime)
        {
            if (activated)
            {
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    for (int k = 0; k < tiles.GetLength(1); k++)
                    {
                        if (tiles[j, k].type == 3)
                        {
                            tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/changeable2");
                            tiles[j, k].Collision = TileCollision.Impassable;
                        }
                    }
                }

            }
        }
        private void UpdateDimension(GameTime gametime)
        {
            //if (Player.getShift == 1)
            if (Player.getDim1)
            {
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    for (int k = 0; k < tiles.GetLength(1); k++)
                    {
                        if (tiles[j, k].type == 4)
                        {
                            tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension1_2");
                            tiles[j, k].Collision = TileCollision.Impassable;
                        }
                    }
                }
            }
            if (Player.getDim1 == false)
            //if (Player.getShift == 3)
            {
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    for (int k = 0; k < tiles.GetLength(1); k++)
                    {
                        if (tiles[j, k].type == 4)
                        {
                            //tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension1_3");
                         //   tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension0");
                            tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/blueTile");
                            tiles[j, k].Collision = TileCollision.Passable;
                        }
                    }
                }
            }
            //if (Player.getShift == 2)
            if (Player.getDim2)
            {
                //layers[0] = new Layer(Content, "Backgrounds/hell0", 0.2f);
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    for (int k = 0; k < tiles.GetLength(1); k++)
                    {
                        if (tiles[j, k].type == 5)
                        {
                            tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension2_2");
                            //tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension0");
                            tiles[j, k].Collision = TileCollision.Impassable;
                        }
                    }
                }
            }
            if (!Player.getDim2)
            //if (Player.getShift == 3)
            {
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    for (int k = 0; k < tiles.GetLength(1); k++)
                    {
                        if (tiles[j, k].type == 5)
                        {
                            //tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension2_3");
                            tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/greenTile");
                            tiles[j, k].Collision = TileCollision.Passable;
                        }
                    }
                }
            }
            if (Player.getDim3)
            //if (Player.getShift == 3)
            {
                //layers[0] = new Layer(Content, "Backgrounds/hell0", 0.2f);
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    for (int k = 0; k < tiles.GetLength(1); k++)
                    {
                        if (tiles[j, k].type == 6)
                        {
                            tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension3_2");
                            tiles[j, k].Collision = TileCollision.Impassable;
                        }
                    }
                }
            }
            if (!Player.getDim3)
            //if (Player.getShift == 3)
            {
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    for (int k = 0; k < tiles.GetLength(1); k++)
                    {
                        if (tiles[j, k].type == 6)
                        {
                            //tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension3_3");
                            tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/purpleTile");
                            tiles[j, k].Collision = TileCollision.Passable;
                        }
                    }
                }
            }
            //if (Player.getShift == 4)
            if (Player.getDim4)
            {
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    for (int k = 0; k < tiles.GetLength(1); k++)
                    {
                        if (tiles[j, k].type == 7)
                        {
                            tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension4_2");
                            //tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension0");
                            tiles[j, k].Collision = TileCollision.Impassable;
                        }
                    }
                }
            }
            if (!Player.getDim4)
            //if (Player.getShift == 3)
            {
                for (int j = 0; j < tiles.GetLength(0); j++)
                {
                    for (int k = 0; k < tiles.GetLength(1); k++)
                    {
                        if (tiles[j, k].type == 7)
                        {
                            //tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/Dimension4_3");
                            tiles[j, k].Texture = Content.Load<Texture2D>("Tiles/redTile");
                            tiles[j, k].Collision = TileCollision.Passable;
                        }
                    }
                }
            }

        }
        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle)&&player.phasingcd<=0)
                {
                    OnPlayerKilled(true);
                }
            }
        }

        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        /// <param name="gem">The gem that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += Gem.PointValue;
            cutscene = true;
            gem.OnCollected(collectedBy);
            collectedBy.gemsCollected++;
        }
        public bool getCutsceneTime()
        { return cutscene; }
        public void setCutsceneTime(bool sceneOff, int nextCutscene)
        {
            cutscene = sceneOff;
        }
        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(bool dead)
        {
            Player.OnKilled(dead);
        }
        bool textboxTime = false;
        private void OnGhostContact(Ghost2 ghast, Player collectedBy)
        {
            textboxTime = true;
            //ghast.OnCollected(collectedBy);
        }
        public bool getTextTime()
        { return textboxTime; }
        public void setTextTime(bool texton, int nextDialogue)
        { 
            textboxTime = texton;
            dialogueIndex = nextDialogue;
        }
        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (player.getDim1 && player.getDim2)
            {
                layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
                spriteBatch.Begin();
                for (int i = 0; i <= EntityLayer; ++i)
                    layers[i].Draw(spriteBatch, cameraPositionXAxis);
                spriteBatch.End();
            }
            else if (player.getDim2 && player.getDim4)
            {
                layers[0] = new Layer(Content, "Backgrounds/toxic0", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/toxic0", 0.2f);
                spriteBatch.Begin();
                for (int i = 0; i <= EntityLayer; ++i)
                    layers[i].Draw(spriteBatch, cameraPositionXAxis);
                spriteBatch.End();
            }
            else if (player.getDim3 && player.getDim1)
            {
                layers[0] = new Layer(Content, "Backgrounds/dimension3", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/dimension3", 0.2f);
                spriteBatch.Begin();
                for (int i = 0; i <= EntityLayer; ++i)
                    layers[i].Draw(spriteBatch, cameraPositionXAxis);
                spriteBatch.End();
            }
            else if (player.getDim4 && player.getDim3)
            {
                layers[0] = new Layer(Content, "Backgrounds/hell0", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/hell0", 0.2f);
                spriteBatch.Begin();
                for (int i = 0; i <= EntityLayer; ++i)
                    layers[i].Draw(spriteBatch, cameraPositionXAxis);
                spriteBatch.End();
            }


            else
            {
                layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
                spriteBatch.Begin();
                for (int i = 0; i <= EntityLayer; ++i)
                    layers[i].Draw(spriteBatch, cameraPositionXAxis);
                spriteBatch.End();
            }

            /*if (player.getDim4)
            {

                spriteBatch.Begin();
                layers[0] = new Layer(Content, "Backgrounds/hell0", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/hell0", 0.2f);
                for (int i = 0; i <= EntityLayer; ++i)
                    layers[i].Draw(spriteBatch, cameraPositionXAxis);
                spriteBatch.End();
            }
            else if (player.getDim2)
            {
                layers[0] = new Layer(Content, "Backgrounds/toxic0", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/toxic0", 0.2f);
                spriteBatch.Begin();
                for (int i = 0; i <= EntityLayer; ++i)
                    layers[i].Draw(spriteBatch, cameraPositionXAxis);
                spriteBatch.End();
            }
            else if (player.getDim3)
            {
                layers[0] = new Layer(Content, "Backgrounds/dimension3", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/dimension3", 0.2f);
                spriteBatch.Begin();
                for (int i = 0; i <= EntityLayer; ++i)
                    layers[i].Draw(spriteBatch, cameraPositionXAxis);
                spriteBatch.End();
            }
            else if(layers!=null)
            {
                layers[0] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/Layer0", 0.2f);
                spriteBatch.Begin();
                for (int i = 0; i <= EntityLayer; ++i)
                    layers[i].Draw(spriteBatch, cameraPositionXAxis);
                spriteBatch.End();
            }*/


            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPositionXAxis, -cameraPositionYAxis, 0.0f); 
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default,

                              RasterizerState.CullCounterClockwise, null, cameraTransform);
            
          
 
           

            DrawTiles(spriteBatch);

            for (int i = 0; i < gems.Length; i++)
                if (gems[i] != null)
                    gems[i].Draw(gameTime, spriteBatch);
            foreach (switches Switch in levers)
                Switch.Draw(gameTime, spriteBatch);
            foreach (Danger1 dangerous in dangers)
                dangerous.Draw(gameTime, spriteBatch);
            foreach (Danger2 dangerous2 in dangers2)
                dangerous2.Draw(gameTime, spriteBatch);
            foreach (Danger3 dangerous3 in dangers3)
                dangerous3.Draw(gameTime, spriteBatch);
            Player.Draw(gameTime, spriteBatch);
            foreach (Ghost2 gaga in ghouls)
                gaga.Draw(gameTime, spriteBatch);
            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPositionXAxis);
            spriteBatch.End();
        }

        ///<summary>
        /// Position the Camera in the level
        /// </summary>
        private void ScrollCamera(Viewport viewport)
        {
            #if ZUNE
            const float ViewMargin = 0.45f;
            #else
            const float ViewMargin = 0.35f;
            #endif
            float maxCameraPositionYOffset = Tile.Height * Height - viewport.Height; 
            // calculate camera for X
            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPositionXAxis + marginWidth;
            float marginRight = cameraPositionXAxis + viewport.Width - marginWidth;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            cameraPositionXAxis = MathHelper.Clamp(cameraPositionXAxis + cameraMovement, 0.0f, maxCameraPosition);
            //Calculate camera for Y
            const float TopMargin = 0.3f;

            const float BottomMargin = 0.3f;

            float marginTop = cameraPositionYAxis + viewport.Height * TopMargin;

            float marginBottom = cameraPositionYAxis + viewport.Height - viewport.Height * BottomMargin;
            // Calculate how far to vertically scroll when the player is near the top or bottom of the screen. 

            float cameraMovementY = 0.0f;

            if (Player.Position.Y < marginTop) //above the top margin 

                cameraMovementY = Player.Position.Y - marginTop;

            else if (Player.Position.Y > marginBottom) //below the bottom margin 

                cameraMovementY = Player.Position.Y - marginBottom;
            cameraPositionYAxis = MathHelper.Clamp(cameraPositionYAxis + cameraMovementY, 0.0f, maxCameraPositionYOffset); 

        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraPositionXAxis / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        if (rdimension)
                        spriteBatch.Draw(texture, position, Color.White);
                        else
                        spriteBatch.Draw(texture, position, Color.LightBlue);
                    }
                }
            }
        }

        #endregion
    }
}
