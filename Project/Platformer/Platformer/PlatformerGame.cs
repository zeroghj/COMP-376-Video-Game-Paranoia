#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Audio;
using System.Text;


namespace Platformer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private KeyboardState currentKeyboardState;
        private KeyboardState lastKeyboardState;
        private MouseState prevmouse;
        private MouseState currentmouse;
        // Global content.
        private SpriteFont hudFont;
        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;
        private Texture2D textBox;
        private Texture2D portrait;
        List<string> paragraphs = new List<string>();
        List<string> memories = new List<string>();
        List<string> goodEnd = new List<string>();
        List<string> badEnd = new List<string>();
        private Texture2D[] menuicon = new Texture2D[3];

        List<List<string>> script = new List<List<string>>();
        List<List<string>> memscript = new List<List<string>>();
        // Meta-level game state.
        private int levelIndex = -1;
        private Level level;
        private Level presavestate;
        private bool wasContinuePressed;
        private bool autosave=true;
        private bool godmodeActive = false;
        private bool goodending = false;
        private Ghost2[] superghostsave1;
        private Ghost2[] superghostsave2;
        int roll = 0;
        float credittimer;
        private int chapter =0;
        private int memory = 0;
        private int paragraph = 0;
        private int scene = 0;
        private int[] paragraphPerChapter = new int[12] { 12, 12, 9, 9, 11, 8, 5, 3, 4, 5, 11 ,11};
        private int[] linesPerMemory = new int[9] { 5, 4, 5, 7, 4, 6, 4, 5, 4 };
        private int goodendlength = 25;
        private int badendlength = 16;
        private int goodendindex = 0;
        private int badendindex = 0;
        private int memoryIndex = 0;
        bool done = true;
        private int dialogueIndex;
        public enum GameState
        {
            MainMenu, GameMenu, Playing, LevelMenu, OptionMenu, Cutscene, Cutscene2, Credit
        }

        //Menu stuff (feel free to move this somewhere more appropriate)
        public static GameState currentGameState = GameState.MainMenu;
        public static GameState prevGameState = GameState.MainMenu;
        private Texture2D mainMenu;
        private List<Texture2D> cutscenes = new List<Texture2D>();
        private Texture2D finalScene;
        private Texture2D badEnding;
        private Texture2D locket;
        Button playBtn, resumeBtn, toMainBtn2, exitBtn, optionBtn, loadBtn, saveBtn, levelBtn, oneBtn, twoBtn,thirdBtn,fourthBtn,fifthBtn,sixBtn, backBtn, toMainBtn,
    mplusBtn, mminusBtn, saveOnBtn, saveOffBtn, splusBtn, sminusBtn, godmodeOn,godmodeOff;
        


        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private TouchCollection touchState;
        private AccelerometerState accelerometerState;


        //Audio setting
        float musicVolume = 0.5f;
        bool musicchange = false;
        SoundEffect dimensionsound;
        SoundEffect dimension2sound;

        
        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
         private const int numberOfLevels = 12;
         //save state
         private bool[][] Progress = new bool[numberOfLevels / 2][];  

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif

            Accelerometer.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            for (int i = 0; i < Progress.Length; i++)
            {
                Progress[i] = new bool[4];
            }
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
            textBox = Content.Load<Texture2D>("Overlays/textBox3");

            //Load menu screen
            mainMenu = Content.Load<Texture2D>("Backgrounds/Layer0_0");
            finalScene = Content.Load<Texture2D>("Backgrounds/deathbed");
            badEnding = Content.Load<Texture2D>("Backgrounds/badend");
            locket = Content.Load<Texture2D>("Portrait/locket");
            playBtn = new Button(Content.Load<Texture2D>("Menu/Menu1"), graphics.GraphicsDevice);
            resumeBtn = new Button(Content.Load<Texture2D>("Menu/Menu4"), graphics.GraphicsDevice);
            toMainBtn2 = new Button(Content.Load<Texture2D>("Menu/Menu2"), graphics.GraphicsDevice);
            optionBtn = new Button(Content.Load<Texture2D>("Menu/Menu3"), graphics.GraphicsDevice);
            loadBtn = new Button(Content.Load<Texture2D>("Menu/Menu5"), graphics.GraphicsDevice);
            saveBtn = new Button(Content.Load<Texture2D>("Menu/Menu6"), graphics.GraphicsDevice);
            exitBtn = new Button(Content.Load<Texture2D>("Menu/Menu2"), graphics.GraphicsDevice);
            toMainBtn = new Button(Content.Load<Texture2D>("Menu/Menu2"), graphics.GraphicsDevice);
            levelBtn = new Button(Content.Load<Texture2D>("Menu/Menu7"), graphics.GraphicsDevice);
            backBtn = new Button(Content.Load<Texture2D>("Menu/Menu8"), graphics.GraphicsDevice);
            oneBtn = new Button(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"), graphics.GraphicsDevice);
            twoBtn = new Button(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"), graphics.GraphicsDevice);
            thirdBtn = new Button(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"), graphics.GraphicsDevice);
            fourthBtn = new Button(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"), graphics.GraphicsDevice);
            fifthBtn = new Button(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"), graphics.GraphicsDevice);
            sixBtn = new Button(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"), graphics.GraphicsDevice);
            mplusBtn = new Button(Content.Load<Texture2D>("Menu/plus"), graphics.GraphicsDevice);
            mminusBtn = new Button(Content.Load<Texture2D>("Menu/minus"), graphics.GraphicsDevice);
            splusBtn = new Button(Content.Load<Texture2D>("Menu/plus"), graphics.GraphicsDevice);
            sminusBtn = new Button(Content.Load<Texture2D>("Menu/minus"), graphics.GraphicsDevice);
            saveOnBtn = new Button(Content.Load<Texture2D>("Menu/On"), graphics.GraphicsDevice);
            saveOffBtn = new Button(Content.Load<Texture2D>("Menu/Off"), graphics.GraphicsDevice);
            godmodeOn = new Button(Content.Load<Texture2D>("Menu/on1"), graphics.GraphicsDevice);
            godmodeOff = new Button(Content.Load<Texture2D>("Menu/Off"), graphics.GraphicsDevice);
            
            this.IsMouseVisible = true;
            Texture2D scene0 = Content.Load<Texture2D>("Backgrounds/cutscene0");
            Texture2D scene1 = Content.Load<Texture2D>("Backgrounds/cutscene1");
            Texture2D scene2 = Content.Load<Texture2D>("Backgrounds/cutscene2");
            Texture2D scene3 = Content.Load<Texture2D>("Backgrounds/cutscene3");
            Texture2D scene4 = Content.Load<Texture2D>("Backgrounds/cutscene4");
            Texture2D scene5 = Content.Load<Texture2D>("Backgrounds/cutscene5");
            Texture2D scene6 = Content.Load<Texture2D>("Backgrounds/cutscene6");
            Texture2D scene7 = Content.Load<Texture2D>("Backgrounds/cutscene7");
            Texture2D scene8 = Content.Load<Texture2D>("Backgrounds/cutscene8");
            cutscenes.Add(scene0);
            cutscenes.Add(scene1);
            cutscenes.Add(scene2);
            cutscenes.Add(scene3);
            cutscenes.Add(scene4);
            cutscenes.Add(scene5);
            cutscenes.Add(scene6);
            cutscenes.Add(scene7);
            cutscenes.Add(scene8);


            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away
            string textPath = string.Format("Content/Script/0.txt");
            using (Stream fileStream = TitleContainer.OpenStream(textPath))
                LoadText(fileStream, 0);
            string textPath2 = string.Format("Content/Memories/0.txt");
            using (Stream fileStream = TitleContainer.OpenStream(textPath2))
                LoadMemory(fileStream, 0);
            string goodendpath = string.Format("Content/Script/goodend.txt");
            using (Stream fileStream = TitleContainer.OpenStream(goodendpath))
                LoadGoodEnd(fileStream);
            string badendpath = string.Format("Content/Script/badend.txt");
            using (Stream fileStream = TitleContainer.OpenStream(badendpath))
                LoadBadEnd(fileStream);
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
                MediaPlayer.Volume = musicVolume;
            }
            catch { }
            
            dimensionsound = Content.Load<SoundEffect>("Sounds/dimensionshift1");
            dimension2sound = Content.Load<SoundEffect>("Sounds/dimensionshift2");
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            
            checkPauseKey();
            // Handle polling for our input and handling high-level input
            HandleInput();
            switch (currentGameState)
            {
                case GameState.MainMenu:
                    DrawMenu();
                    if (level != null)
                        level.Dispose();
                    level = null;
                    toMainBtn.isClicked = toMainBtn2.isClicked = false;
                    break;
                case GameState.GameMenu:
                    DrawPauseMenu();
                    break;
                case GameState.LevelMenu:
                    DrawLevelMenu();
                    break;
                case GameState.Cutscene:
                    break;
                case GameState.Cutscene2:
                    break;
                case GameState.Credit:
                    break;
                case GameState.OptionMenu:
                    DrawOptionMenu();
                    break;
                case GameState.Playing:
                    playBtn.isClicked = resumeBtn.isClicked = false;
                    level.Update(gameTime, keyboardState, gamePadState, touchState,
                                 accelerometerState, Window.CurrentOrientation, Progress[levelIndex / 2]);
                    if (level.ghouls.Count > 0 && level.rdimension && superghostsave1 == null)
                    {
                        superghostsave1 = new Ghost2[level.ghouls.Count];
                    }
                    if (level.ghouls.Count > 0 && !level.rdimension && superghostsave2 == null)
                    {
                        superghostsave2 = new Ghost2[level.ghouls.Count];
                    }
                    LoadOtherDimension();
                    level.Player.godmodeStatus(godmodeActive);
                    base.Update(gameTime);
                    break;
            }
            Music();
        }
        private void Music()
        {
            if (musicchange || prevGameState != currentGameState && (currentGameState != GameState.LevelMenu && currentGameState != GameState.GameMenu && currentGameState != GameState.Cutscene && currentGameState != GameState.Cutscene2 && currentGameState != GameState.OptionMenu && (prevGameState != GameState.LevelMenu && prevGameState != GameState.OptionMenu && prevGameState != GameState.GameMenu && prevGameState != GameState.Cutscene && prevGameState != GameState.Cutscene2)))
            {
                musicchange = false;
                try
                {
                    MediaPlayer.Pause();
                    if (currentGameState == GameState.MainMenu)
                    {
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
                    }
                    if (currentGameState == GameState.Playing)
                    {
                        switch (levelIndex / 2)
                        {
                            case 0:
                                MediaPlayer.Play(Content.Load<Song>("Sounds/dark-piano-and-violin"));
                                break;
                            case 1:
                                MediaPlayer.Play(Content.Load<Song>("Sounds/JMB_-_Stream_of_Time"));
                                break;
                            case 2:
                                MediaPlayer.Play(Content.Load<Song>("Sounds/Tunguska_Electronic_Music_Society_-_Animula_Vagula_-_Departure"));
                                break;
                            case 3:
                                MediaPlayer.Play(Content.Load<Song>("Sounds/enemy-in-disguise"));
                                break;
                            case 4:
                                MediaPlayer.Play(Content.Load<Song>("Sounds/Risen_Wolf_-_Death"));
                                break;
                            case 5:
                                MediaPlayer.Play(Content.Load<Song>("Sounds/dark-skies-of-doom"));
                                break;
                            default:
                                MediaPlayer.Play(Content.Load<Song>("Sounds/all-that-remains"));
                                break;

                        }
                    }
                }
                catch { }
            }

            prevGameState = currentGameState;
            MediaPlayer.Volume = musicVolume;
        }
        private void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            // get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
            touchState = TouchPanel.GetState();
            accelerometerState = Accelerometer.GetState();
            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();

            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Space) ||
                gamePadState.IsButtonDown(Buttons.A) ||
                touchState.AnyTouch();

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;
        }
        //List<string> dialogues = new List<string>();
        //private string[] dialogue;
        //change dimension
        private void LoadOtherDimension()
        {
            if ((level.Player.alternatedimension == level.rdimension)&&!level.Player.keydown)
            {
                Ghost2[] temp;
                if (level.Player.alternatedimension)
                {
                    levelIndex = (levelIndex + 1) % numberOfLevels;
                    dimensionsound.Play();
                    if (superghostsave1 == null)
                        superghostsave1 = level.phantom;
                    else
                    {
                        for (int i=0;i<superghostsave1.Length;i++)
                        {
                            if (level.phantom.Length>=i)
                                superghostsave1[i] = level.phantom[i];
                            else
                            {
                                superghostsave1[i] = null;
                            }
                        }
                    }
                     temp = superghostsave2;
                }
                else
                {
                    levelIndex = (levelIndex - 1) % numberOfLevels;
                    if (superghostsave2 == null)
                        superghostsave2 = level.phantom;
                    else
                    {
                        for (int i = 0; i < superghostsave2.Length; i++)
                        {
                            if (level.phantom.Length >= i)
                                superghostsave2[i] = level.phantom[i];
                            else
                            {
                                superghostsave2[i] = null;
                            }
                        }
                    }
                    dimension2sound.Play();
                     temp = superghostsave1;
                }
                //Savecurrentlevelstate
                Level pss = presavestate;
               presavestate = level;
               Level state = level;
                // Unloads the content for the current level before loading the next one.
                if (level != null)
                {
                    level.Dispose();
                }

                // Load the level.
                string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    if (pss == null)
                    {
                        level = new Level(Services, fileStream, levelIndex);
                    }
                    else
                    {
                        level = new Level(Services, fileStream, levelIndex, state, pss,temp);
                    }

                
            }
        }
        private void LoadNextLevel()
        {
            // move to the next level
            if (level!=null&&level.Player.alternatedimension)
                {
                    levelIndex = (levelIndex + 1);
                if (levelIndex-numberOfLevels<0)
                        Progress[(levelIndex / 2)][0] = true;
                }
                else
                {
                    levelIndex = (levelIndex + 2);
                    if (levelIndex - numberOfLevels < 0)
                        Progress[((levelIndex -1)/ 2)][0] = true;
                }
            if (levelIndex - numberOfLevels < 0)
            Progress[levelIndex/2][0] = true;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
            {
                level.Dispose();
                level = null;
            }
             superghostsave1 = null;
             superghostsave2 = null;
             presavestate = null;
                
            // Load the level.
            if (autosave)
                Save();
            musicchange = true;
            if (levelIndex < 12)
            {
                string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    level = new Level(Services, fileStream, levelIndex);
            }
            else
            {
                if (calculateShards() == 18)
                {
                    goodending = true;
                }
                else
                    goodending = false;

                currentGameState = GameState.Cutscene2;
            }

        }
        private void LoadText(Stream fileStream,int paragraph)
        {
            int width;

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    paragraph++;
                    paragraphs.Add(line);
                    
                    line = reader.ReadLine();
                }
                script.Add(paragraphs);
            }
        }
        private void LoadGoodEnd(Stream fileStream)
        {
            int width;
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    goodEnd.Add(line);
                    line = reader.ReadLine();
                }
            }
        }
        private void LoadBadEnd(Stream fileStream)
        {
            int width;
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    badEnd.Add(line);
                    line = reader.ReadLine();
                }
            }
        }
        private void LoadMemory(Stream fileStream, int paragraph)
        {
            int width;

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    memories.Add(line);
                    line = reader.ReadLine();
                }
                memscript.Add(memories);
            }
        }
        private List<string> LoadText2(Stream fileStream)
        {
            int width;
            List<string> newConvo = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                
                while (line != null)
                {
                    newConvo.Add(line);
                    line = reader.ReadLine();
                }
            }
            return newConvo;
        }
        private List<string> LoadMemory2(Stream fileStream)
        {
            int width;
            List<string> newMemory = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;

                while (line != null)
                {
                    newMemory.Add(line);
                    line = reader.ReadLine();
                }
            }
            return newMemory;
        }
        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }
        #region Draw
        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (currentGameState)
            {
                case GameState.GameMenu:
                    level.Draw(gameTime, spriteBatch);
                    DrawHud();
                    DrawPauseMenu();
                    break;
                case GameState.MainMenu:
                    if (level != null)
                        level.Dispose();
                    DrawMenu();
                    break;
                case GameState.Playing:
                    level.Draw(gameTime, spriteBatch);
                    DrawHud();
                    break;
                case GameState.LevelMenu:
                    DrawLevelMenu();
                    break;
                case GameState.Cutscene:
                    DrawCutScene();

                    if (!IsPressed(Keys.N) && (memoryIndex < linesPerMemory[memory]))
                    {
                        string thisLine = memscript[memory][memoryIndex];
                        string nextLine = "";
                        string firstLine = "";
                        string thirdLine = "";
                        if (thisLine.Length > 59 && thisLine.Length <= 119)
                        {
                            firstLine = thisLine.Substring(0, 59);
                            nextLine = thisLine.Substring(59, thisLine.Length - 60);
                        }
                        else if (thisLine.Length > 119)
                        {
                            firstLine = thisLine.Substring(0, 59);
                            nextLine = thisLine.Substring(59, 60);
                            thirdLine = thisLine.Substring(119, thisLine.Length - 120);
                        }
                        else
                        {
                            firstLine = thisLine;
                            nextLine = "";
                            thirdLine = "";
                        }
                        spriteBatch.Begin();
                        spriteBatch.Draw(textBox, new Rectangle(20, 50, 760, 200), Color.White);
                        spriteBatch.DrawString(hudFont, firstLine, new Vector2(50, 80), Color.Yellow);
                        spriteBatch.DrawString(hudFont, nextLine, new Vector2(50, 95), Color.Yellow);
                        spriteBatch.DrawString(hudFont, thirdLine, new Vector2(50, 110), Color.Yellow);
                        spriteBatch.DrawString(hudFont, "Press N to continue", new Vector2(550, 215), Color.Yellow);
                        spriteBatch.End();
                    }
                    else if (IsPressed(Keys.N))
                    {
                        memoryIndex++;
                        if (memoryIndex >= linesPerMemory[memory])
                        {
                            memory++;
                            scene++;
                            string textPath2 = string.Format("Content/Memories/{0}.txt", memory);
                            using (Stream fileStream = TitleContainer.OpenStream(textPath2))
                                memscript.Add(LoadText2(fileStream));
                            level.setCutsceneTime(false, 5);
                            level.setTextTime(false, 6);
                            memoryIndex = 0;
                            currentGameState = GameState.Playing;
                        }
                    }
                    break;
                case GameState.OptionMenu:
                    if (level != null)
                    {
                        level.Draw(gameTime, spriteBatch);
                        DrawHud();
                    }
                    if (level == null)
                        DrawMenu();
                    DrawOptionMenu();
                    break;
                case GameState.Cutscene2:
                    if (goodending)
                    {
                        DrawGoodEnding();
                        if (!IsPressed(Keys.N) && (goodendindex < goodendlength))
                        {
                            string thisLine = goodEnd[goodendindex];
                            string nextLine = "";
                            string firstLine = "";
                            string thirdLine = "";
                            if (thisLine.Length > 59 && thisLine.Length <= 119)
                            {
                                firstLine = thisLine.Substring(0, 59);
                                nextLine = thisLine.Substring(59, thisLine.Length - 60);
                            }
                            else if (thisLine.Length > 119)
                            {
                                firstLine = thisLine.Substring(0, 59);
                                nextLine = thisLine.Substring(59, 60);
                                thirdLine = thisLine.Substring(119, thisLine.Length - 120);
                            }
                            else
                            {
                                firstLine = thisLine;
                                nextLine = "";
                                thirdLine = "";
                            }
                            spriteBatch.Begin();
                            spriteBatch.Draw(textBox, new Rectangle(20, 50, 760, 200), Color.White);
                            spriteBatch.DrawString(hudFont, firstLine, new Vector2(50, 80), Color.Yellow);
                            spriteBatch.DrawString(hudFont, nextLine, new Vector2(50, 95), Color.Yellow);
                            spriteBatch.DrawString(hudFont, thirdLine, new Vector2(50, 110), Color.Yellow);
                            spriteBatch.DrawString(hudFont, "Press N to continue", new Vector2(550, 215), Color.Yellow);
                            spriteBatch.End();
                        }
                        else if (IsPressed(Keys.N))
                        {
                            goodendindex++;
                            if (goodendindex >= goodendlength)
                            {
                                //level.setCutsceneTime(false, 5);
                                //level.setTextTime(false, 6);
                                //currentGameState = GameState.Playing;
                                currentGameState = GameState.Credit;
                            }
                        }
                    }
                    else
                    {
                        DrawBadEnding();
                        if (!IsPressed(Keys.N) && (badendindex < badendlength))
                        {
                            string thisLine = badEnd[badendindex];
                            string nextLine = "";
                            string firstLine = "";
                            string thirdLine = "";
                            if (thisLine.Length > 59 && thisLine.Length <= 119)
                            {
                                firstLine = thisLine.Substring(0, 59);
                                nextLine = thisLine.Substring(59, thisLine.Length - 60);
                            }
                            else if (thisLine.Length > 119)
                            {
                                firstLine = thisLine.Substring(0, 59);
                                nextLine = thisLine.Substring(59, 60);
                                thirdLine = thisLine.Substring(119, thisLine.Length - 120);
                            }
                            else
                            {
                                firstLine = thisLine;
                                nextLine = "";
                                thirdLine = "";
                            }
                            spriteBatch.Begin();
                            spriteBatch.Draw(textBox, new Rectangle(20, 50, 760, 200), Color.White);
                            spriteBatch.DrawString(hudFont, firstLine, new Vector2(50, 80), Color.Yellow);
                            spriteBatch.DrawString(hudFont, nextLine, new Vector2(50, 95), Color.Yellow);
                            spriteBatch.DrawString(hudFont, thirdLine, new Vector2(50, 110), Color.Yellow);
                            spriteBatch.DrawString(hudFont, "Press N to continue", new Vector2(550, 215), Color.Yellow);
                            spriteBatch.End();
                        }
                        else if (IsPressed(Keys.N))
                        {
                            badendindex++;
                            if (badendindex >= badendlength)
                            {
                                //level.setCutsceneTime(false, 5);
                                //level.setTextTime(false, 6);
                                //currentGameState = GameState.Playing;
                                currentGameState = GameState.Credit;
                            }
                        }
                    }
                    break;
                case GameState.Credit:
                    DrawCredit(gameTime);
                    break;
            }

            base.Draw(gameTime);
        }
        private void DrawGoodEnding()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(finalScene, new Rectangle(0, 0, 800, 480), Color.White);
            
            spriteBatch.End();
            
        }
        private void DrawBadEnding()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(badEnding, new Rectangle(0, 0, 800, 480), Color.White);
            
            spriteBatch.End();
            
        }
        private void DrawCredit(GameTime gameTime)
        {
            if (MediaPlayer.State != MediaState.Playing)
            MediaPlayer.Play(Content.Load<Song>("Sounds/dark-skies-of-doom"));
            prevmouse = currentmouse;
            currentmouse = Mouse.GetState();
            credittimer += gameTime.ElapsedGameTime.Milliseconds;
            switch (roll)
            {
                case 0:
                    spriteBatch.Begin();
                    spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/Layer0_0"), new Rectangle(0, 0, 800, 480), Color.White);
                    spriteBatch.DrawString(hudFont, "Credit", new Vector2(310, 10), Color.Black);
                    spriteBatch.DrawString(hudFont, "Paranoia", new Vector2(300, 80), Color.Black);
                    spriteBatch.DrawString(hudFont, "A Dark Light Pirate Crew Production", new Vector2(200, 120), Color.Black);
                      spriteBatch.DrawString(hudFont, "Made by:  Chun Lok Chan  1696335", new Vector2(150, 200), Color.Black);
                      spriteBatch.DrawString(hudFont, "                Eric Dana 1941275", new Vector2(150, 250), Color.Black);
                      spriteBatch.DrawString(hudFont, "                Denis Lau  9746757", new Vector2(150, 300), Color.Black);
                      spriteBatch.DrawString(hudFont, "                Vincent Poulin-Rioux 6333540", new Vector2(150, 350), Color.Black);
                    spriteBatch.End();
                        break;
                case 1:
                        spriteBatch.Begin();
                        spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/Layer0_0"), new Rectangle(0, 0, 800, 480), Color.White);
                        spriteBatch.DrawString(hudFont, "Lead Artist : Eric Dana \n Artist: Denis Lau\n            Vincent Poulin-Rioux ", new Vector2(150, 70), Color.Black);
                        spriteBatch.DrawString(hudFont, "Lead Programmer : Vincent Poulin-Rioux \n Programmer: Denis Lau", new Vector2(150, 170), Color.Black);
                        spriteBatch.DrawString(hudFont, "Lead Level Designer: Chun Lok Chan \n Level Design: Eric Dana", new Vector2(150, 270), Color.Black);
                        spriteBatch.DrawString(hudFont, "Lead Sound Designer: Vincent Poulin-Rioux \n Sound Design: Denis Lau ", new Vector2(150, 360), Color.Black);
                        spriteBatch.End();
                        break;
               case 2:
                        spriteBatch.Begin();
                        spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/toxic0_0"), new Rectangle(0, 0, 800, 480), Color.White);
                        spriteBatch.DrawString(hudFont, "Engine and Tools", new Vector2(310, 10), Color.White);
                        spriteBatch.DrawString(hudFont, "This game was made in XNA 4.0 ", new Vector2(250, 80), Color.White);
                        spriteBatch.DrawString(hudFont, "The basic structure of the game was made using \n Microsoft XNA Starter Kit: Platformer ", new Vector2(50, 110), Color.White);
                        spriteBatch.DrawString(hudFont, "Graphic Tools : Paint \n            Gimp \n            Photoshop CS5", new Vector2(150, 160), Color.White);
                        spriteBatch.DrawString(hudFont, "Programming Tools : Microsoft Visual Studio \n Notepad++ ", new Vector2(150, 280), Color.White);
                        spriteBatch.DrawString(hudFont, "Audio Tools : Audacity ", new Vector2(150, 400), Color.White);
                        spriteBatch.End();
                        break;
               case 3:
                        spriteBatch.Begin();
                        spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/toxic0_0"), new Rectangle(0, 0, 800, 480), Color.White);
                        spriteBatch.DrawString(hudFont, "Sound", new Vector2(310, 10), Color.White);
                        spriteBatch.DrawString(hudFont, "These Sound Effect were used according\n to the Creative Commons License ", new Vector2(100, 80), Color.White);
                        spriteBatch.DrawString(hudFont, " All Right Reserved to Footage Firm \n The Sound Effect were taken from www.soundeffectsforfree.com\nDimension Shifting Sound, Gem Collision and End of Level Chime", new Vector2(50, 210), Color.White);
                        spriteBatch.DrawString(hudFont, " The Jumping and Falling sound were \npart of the Starter Kit: Platformer of Microsoft \n All Right Reserved to Microsoft", new Vector2(50, 410), Color.White);
                        spriteBatch.End();
                        break;
               case 4:
                        spriteBatch.Begin();
                        spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/dimension3_0"), new Rectangle(0, 0, 800, 480), Color.White);
                        spriteBatch.DrawString(hudFont, "Music", new Vector2(310, 10), Color.White);
                        spriteBatch.DrawString(hudFont, "All Music were used according to the\n Creative Commons License and belong to their respective Author", new Vector2(50, 80), Color.White);
                        spriteBatch.DrawString(hudFont, "Main Menu : \n Level 1 : (FF) - Dark Piano & Violon \n Level 2: JMB - Stream of Time \n Level 3: Tunguska Electronic Music Society -\n Animula_Vagula - Departure\n Level 4: (FF) - enemy in disguise \n Level 5: Risen Wolf - Death \n Level 6 & Credit : (FF) - dark-skies-of-doom\n Hidden Song: (FF) - all-that-remains ", new Vector2(50, 120), Color.White);
                        spriteBatch.DrawString(hudFont, "The (FF) denotation represents a song owned by Footage Firm", new Vector2(050, 450), Color.White);
                        spriteBatch.End();
                        break;
               case 5:
                        spriteBatch.Begin();
                        spriteBatch.Draw(Content.Load<Texture2D>("Backgrounds/dimension3_0"), new Rectangle(0, 0, 800, 480), Color.White);
                        spriteBatch.DrawString(hudFont, "THANK YOU FOR PLAYING", new Vector2(310, 10), Color.White);
                        spriteBatch.DrawString(hudFont, "Paranoia was made in a 2 months period by\n 4 Student of Concordia University for their Game Design Class ", new Vector2(50, 80), Color.White);
                        spriteBatch.DrawString(hudFont, "We sincerely hope you liked our game,\n remember that you can replay from your save file to unlock all Memory Shard and\n Get a different Ending!  ", new Vector2(50, 200), Color.White);
                        spriteBatch.DrawString(hudFont, "Fin. ", new Vector2(320, 400), Color.White);
                        spriteBatch.End();
                        break;
                case 6:
                        currentGameState = GameState.MainMenu;
                            this.IsMouseVisible = true;
                        break;
            }
            if (currentmouse.LeftButton == ButtonState.Released && prevmouse.LeftButton == ButtonState.Pressed || credittimer > 9000)
            {
                roll++;
                credittimer = 0;
            }

        }
        private void DrawOptionMenu()
        {
            MouseState mouse = Mouse.GetState();
            toMainBtn.SetPosition(new Vector2(500, 325));
            mplusBtn.SetPosition(new Vector2(500, 220));
            mminusBtn.SetPosition(new Vector2(225, 220));
            saveOnBtn.SetPosition(new Vector2(500, 170));
            saveOffBtn.SetPosition(new Vector2(225, 170));
            godmodeOn.SetPosition(new Vector2(500, 270));
            godmodeOff.SetPosition(new Vector2(225, 270));
            spriteBatch.Begin();
            spriteBatch.Draw(mainMenu, new Rectangle(200, 120, 400, 240), Color.White);
            DrawShadowedString(hudFont, "Options", new Vector2(350, 125), Color.White);
            DrawShadowedString(hudFont, "Autosave", new Vector2(350, 170), Color.White);
            DrawShadowedString(hudFont, "Godmode", new Vector2(350, 270), Color.White);
            DrawShadowedString(hudFont, "Sound " + Math.Round(musicVolume, 2), new Vector2(350, 220), Color.White);
            toMainBtn.Draw(spriteBatch);
            mplusBtn.Draw(spriteBatch);
            mminusBtn.Draw(spriteBatch);
            splusBtn.Draw(spriteBatch);
            sminusBtn.Draw(spriteBatch);
            if (!autosave)
                saveOnBtn.Draw(spriteBatch);
            if (autosave)
                saveOffBtn.Draw(spriteBatch);
            if (!godmodeActive)
                godmodeOn.Draw(spriteBatch);
            if (godmodeActive)
                godmodeOff.Draw(spriteBatch);

            spriteBatch.End();
            if (saveOnBtn.isClicked)
            {
                autosave = true;
            }
            if (saveOffBtn.isClicked)
            {
                autosave = false;
            }
            if (godmodeOn.isClicked)
            {
                godmodeActive = true;
                //level.Player.godmodeStatus(godmodeActive);
            }
            if (godmodeOff.isClicked)
            {
                godmodeActive = false;
                //level.Player.godmodeStatus(godmodeActive);
            }
            if (mplusBtn.isClicked)
            {
                musicVolume = MathHelper.Clamp(musicVolume + 0.01f, 0.0f, 1.0f);
                mplusBtn.isClicked = false;
            }
            if (mminusBtn.isClicked)
            {
                musicVolume = MathHelper.Clamp(musicVolume - 0.01f, 0.0f, 1.0f);
                mminusBtn.isClicked = false;
            }
            if (toMainBtn.isClicked && level == null)
            {
                currentGameState = GameState.MainMenu;
            }
            else if (toMainBtn.isClicked && level != null)
            {
                currentGameState = GameState.GameMenu;
            }
            toMainBtn.Update(mouse);
            mplusBtn.Update(mouse);
            mminusBtn.Update(mouse);
            saveOnBtn.Update(mouse);
            saveOffBtn.Update(mouse);
            godmodeOn.Update(mouse);
            godmodeOff.Update(mouse);
        }
        private void DrawCutScene()
        {
            spriteBatch.Begin();
            //spriteBatch.Draw(finalScene, new Rectangle(0, 0, 800, 480), Color.White);
            spriteBatch.Draw(cutscenes[scene], new Rectangle(0, 0, 800, 480), Color.White);
            spriteBatch.End();
        }
        private void DrawPauseMenu()
        {
            MouseState mouse = Mouse.GetState();
            resumeBtn.SetPosition(new Vector2(250, 175));
            toMainBtn2.SetPosition(new Vector2(250, 325));
            optionBtn.SetPosition(new Vector2(250, 275));
            saveBtn.SetPosition(new Vector2(250, 225));
            spriteBatch.Begin();
            spriteBatch.Draw(mainMenu, new Rectangle(200, 120, 400, 240), Color.White);
            DrawShadowedString(hudFont, "Game Paused", new Vector2(350, 120), Color.White);
            spriteBatch.Draw(locket, new Rectangle(615, 0, 128, 256), Color.White);
            resumeBtn.Draw(spriteBatch);
            optionBtn.Draw(spriteBatch);
            saveBtn.Draw(spriteBatch);
            toMainBtn2.Draw(spriteBatch);
            if (Progress[levelIndex / 2][1])
            {
                menuicon[0] = Content.Load<Texture2D>("Sprites/Memory Shard");
            }
            else
            {
                menuicon[0] = Content.Load<Texture2D>("Sprites/Greyed Memory Shard");
            }
            if (Progress[levelIndex / 2][2])
            {
                menuicon[1] = Content.Load<Texture2D>("Sprites/Memory Shard");
            }
            else
            {
                menuicon[1] = Content.Load<Texture2D>("Sprites/Greyed Memory Shard");
            }
            if (Progress[levelIndex / 2][3])
            {
                menuicon[2] = Content.Load<Texture2D>("Sprites/Memory Shard");
            }
            else
            {
                menuicon[2] = Content.Load<Texture2D>("Sprites/Greyed Memory Shard");
            }
            DrawShadowedString(hudFont, "Shard Collected", new Vector2(375, 200), Color.White);
            spriteBatch.Draw(menuicon[0], new Rectangle(375, 225, 64, 64), Color.White);
            spriteBatch.Draw(menuicon[1], new Rectangle(450, 225, 64, 64), Color.White);
            spriteBatch.Draw(menuicon[2], new Rectangle(525, 225, 64, 64), Color.White);
            if (calculateShards() == 0)
                portrait = Content.Load<Texture2D>("Portrait/Portrait8");
            else if (calculateShards() < 2)
                portrait = Content.Load<Texture2D>("Portrait/Portrait7");
            else if (calculateShards() < 4)
                portrait = Content.Load<Texture2D>("Portrait/Portrait6");
            else if (calculateShards() < 6)
                portrait = Content.Load<Texture2D>("Portrait/Portrait5");
            else if (calculateShards() < 10)
                portrait = Content.Load<Texture2D>("Portrait/Portrait4");
            else if (calculateShards() < 13)
                portrait = Content.Load<Texture2D>("Portrait/Portrait3");
            else if (calculateShards() < 16)
                portrait = Content.Load<Texture2D>("Portrait/Portrait2");
            else if (calculateShards() < 18)
                portrait = Content.Load<Texture2D>("Portrait/Portrait1");
            else if (calculateShards() == 18)
                portrait = Content.Load<Texture2D>("Portrait/Portrait0");
            spriteBatch.Draw(portrait, new Rectangle(640, 74, 79, 105), Color.White);
            spriteBatch.End();

            if (resumeBtn.isClicked)
            {
                currentGameState = GameState.Playing;
                this.IsMouseVisible = false;
            }
            if (saveBtn.isClicked)
            {
                Save();
            }
            if (optionBtn.isClicked)
            {
                currentGameState = GameState.OptionMenu;
            }
            if (toMainBtn2.isClicked)
            {
                currentGameState = GameState.MainMenu;
                musicchange = true;
            }
            resumeBtn.Update(mouse);
            optionBtn.Update(mouse);
            saveBtn.Update(mouse);
            toMainBtn2.Update(mouse);
        }
        private void DrawMenu()
        {
            MouseState mouse = Mouse.GetState();

            spriteBatch.Begin();
            spriteBatch.Draw(mainMenu, new Rectangle(0, 0, 800, 480), Color.White);
            DrawShadowedString(hudFont, "Paranoia", new Vector2(350, 25), Color.White);
            playBtn.Draw(spriteBatch);
            if (Progress[0][0])
            {
                resumeBtn.Draw(spriteBatch);
            }
            optionBtn.Draw(spriteBatch);
            loadBtn.Draw(spriteBatch);
            saveBtn.Draw(spriteBatch);
            levelBtn.Draw(spriteBatch);
            exitBtn.Draw(spriteBatch);
            spriteBatch.End();
            resumeBtn.SetPosition(new Vector2(400, 225));
            exitBtn.SetPosition(new Vector2(350, 425));
            optionBtn.SetPosition(new Vector2(350, 325));
            loadBtn.SetPosition(new Vector2(300, 275));
            saveBtn.SetPosition(new Vector2(400, 275));
            levelBtn.SetPosition(new Vector2(350, 375));
            if (playBtn.isClicked)
            {
                resetProgress();
                levelIndex = findlevelindex();
                LoadNextLevel();
                currentGameState = GameState.Playing;
                this.IsMouseVisible = false;
            }
            if (resumeBtn.isClicked)
            {
                levelIndex = findlevelindex();
                LoadNextLevel();
                currentGameState = GameState.Playing;
                this.IsMouseVisible = false;
            }
            if (loadBtn.isClicked)
            {
                Load();
            }
            if (saveBtn.isClicked)
            {
                Save();
            }
            if (Progress[0][0])
            {
                playBtn.SetPosition(new Vector2(300, 225));
            }
            else
            {
                playBtn.SetPosition(new Vector2(350, 225));
            }
            if (levelBtn.isClicked)
            {
                currentGameState = GameState.LevelMenu;
            }
            if (optionBtn.isClicked)
            {
                currentGameState = GameState.OptionMenu;
            }
            else if (exitBtn.isClicked)
                this.Exit();

            playBtn.Update(mouse);
            resumeBtn.Update(mouse);
            optionBtn.Update(mouse);
            loadBtn.Update(mouse);
            saveBtn.Update(mouse);
            levelBtn.Update(mouse);
            exitBtn.Update(mouse);
        }
        private void showshard(int i)
        {
            if (Progress[i][1])
            {
                menuicon[0] = Content.Load<Texture2D>("Sprites/Memory Shard");
            }
            else
            {
                menuicon[0] = Content.Load<Texture2D>("Sprites/Greyed Memory Shard");
            }
            if (Progress[i][2])
            {
                menuicon[1] = Content.Load<Texture2D>("Sprites/Memory Shard");
            }
            else
            {
                menuicon[1] = Content.Load<Texture2D>("Sprites/Greyed Memory Shard");
            }
            if (Progress[i][3])
            {
                menuicon[2] = Content.Load<Texture2D>("Sprites/Memory Shard");
            }
            else
            {
                menuicon[2] = Content.Load<Texture2D>("Sprites/Greyed Memory Shard");
            }
        }
        private void DrawLevelMenu()
        {
            MouseState mouse = Mouse.GetState();
            oneBtn.Update(mouse);
            twoBtn.Update(mouse);
            thirdBtn.Update(mouse);
            fourthBtn.Update(mouse);
            fifthBtn.Update(mouse);
            sixBtn.Update(mouse);
            backBtn.Update(mouse);
            spriteBatch.Begin();
            spriteBatch.Draw(mainMenu, new Rectangle(0, 0, 800, 480), Color.White);
            oneBtn.Draw(spriteBatch);
            twoBtn.Draw(spriteBatch);
            thirdBtn.Draw(spriteBatch);
            fourthBtn.Draw(spriteBatch);
            fifthBtn.Draw(spriteBatch);
            sixBtn.Draw(spriteBatch);
            backBtn.Draw(spriteBatch);
            DrawShadowedString(hudFont, "Level Menu", new Vector2(350, 25), Color.White);
            oneBtn.SetPosition(new Vector2(225, 275));
            twoBtn.SetPosition(new Vector2(325, 275));
            thirdBtn.SetPosition(new Vector2(425, 275));
            fourthBtn.SetPosition(new Vector2(225, 325));
            fifthBtn.SetPosition(new Vector2(325, 325));
            sixBtn.SetPosition(new Vector2(425, 325));
            backBtn.SetPosition(new Vector2(250, 425));
            Rectangle mouseRectangle = new Rectangle(mouse.X, mouse.Y, 1, 1);
            menuicon[0] = null;
            menuicon[1] = null;
            menuicon[2] = null;
            //popupsection
            if (mouseRectangle.Intersects(oneBtn.rectangle)&&oneBtn.enabled)
            {
                showshard(0);
            }
            if (mouseRectangle.Intersects(twoBtn.rectangle) && twoBtn.enabled)
            {
                showshard(1);
            }
            if (mouseRectangle.Intersects(thirdBtn.rectangle) && thirdBtn.enabled)
            {
                showshard(2);
            }
            if (mouseRectangle.Intersects(fourthBtn.rectangle) && fourthBtn.enabled)
            {
                showshard(3);
            }
            if (mouseRectangle.Intersects(fifthBtn.rectangle) && fifthBtn.enabled)
            {
                showshard(4);
            }
            if (mouseRectangle.Intersects(sixBtn.rectangle) && sixBtn.enabled)
            {
                showshard(5);
            }
            DrawShadowedString(hudFont, "Shard Collected", new Vector2(325, 100), Color.White);
            if (menuicon[0] != null)
                spriteBatch.Draw(menuicon[0], new Rectangle(300, 125, 64, 64), Color.White);
            if (menuicon[1] != null)
                spriteBatch.Draw(menuicon[1], new Rectangle(375, 125, 64, 64), Color.White);
            if (menuicon[2] != null)
                spriteBatch.Draw(menuicon[2], new Rectangle(450, 125, 64, 64), Color.White);
            spriteBatch.End();
            if (Progress[0][0] == true)
            {
                oneBtn.enabled = true;
                oneBtn.SetTexture(Content.Load<Texture2D>("Menu/choice1"));
            }
            else
            {
                oneBtn.enabled = false;
                oneBtn.SetTexture(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"));
            }
            if (Progress[1][0] == true)
            {
                twoBtn.enabled = true;
                twoBtn.SetTexture(Content.Load<Texture2D>("Menu/choice2"));
            }
            else
            {
                twoBtn.enabled = false;
                twoBtn.SetTexture(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"));
            }
            if (Progress[2][0] == true)
            {
                thirdBtn.enabled = true;
                thirdBtn.SetTexture(Content.Load<Texture2D>("Menu/choice3"));
            }
            else
            {
                thirdBtn.enabled = false;
                thirdBtn.SetTexture(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"));
            }
            if (Progress[3][0] == true)
            {
                fourthBtn.enabled = true;
                fourthBtn.SetTexture(Content.Load<Texture2D>("Menu/choice4"));
            }
            else
            {
                fourthBtn.enabled = false;
                fourthBtn.SetTexture(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"));
            }
            if (Progress[4][0] == true)
            {
                fifthBtn.enabled = true;
                fifthBtn.SetTexture(Content.Load<Texture2D>("Menu/choice5"));
            }
            else
            {
                fifthBtn.enabled = false;
                fifthBtn.SetTexture(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"));
            }
            if (Progress[5][0] == true)
            {
                sixBtn.enabled = true;
                sixBtn.SetTexture(Content.Load<Texture2D>("Menu/choice6"));
            }
            else
            {
                sixBtn.enabled = false;
                sixBtn.SetTexture(Content.Load<Texture2D>("Menu/MonotypeCorsivaItalic"));
            }
            if (oneBtn.isClicked && oneBtn.enabled)
            {
                levelIndex = -1;
                LoadNextLevel();
                currentGameState = GameState.Playing;
                this.IsMouseVisible = false;
            }
            if (twoBtn.isClicked && twoBtn.enabled)
            {
                levelIndex = 1;
                LoadNextLevel();
                currentGameState = GameState.Playing;
                this.IsMouseVisible = false;
            }
            if (thirdBtn.isClicked && thirdBtn.enabled)
            {
                levelIndex = 3;
                LoadNextLevel();
                currentGameState = GameState.Playing;
                this.IsMouseVisible = false;
            }
            if (fourthBtn.isClicked && fourthBtn.enabled)
            {
                levelIndex = 5;
                LoadNextLevel();
                currentGameState = GameState.Playing;
                this.IsMouseVisible = false;
            }
            if (fifthBtn.isClicked && fifthBtn.enabled)
            {
                levelIndex = 7;
                LoadNextLevel();
                currentGameState = GameState.Playing;
                this.IsMouseVisible = false;
            }
            if (sixBtn.isClicked && sixBtn.enabled)
            {
                levelIndex = 9;
                LoadNextLevel();
                currentGameState = GameState.Playing;
                this.IsMouseVisible = false;
            }

            else if (backBtn.isClicked)
                currentGameState = GameState.MainMenu;
        }
        bool newScene = false;
        public void cutsceneTrigger()
        {
            newScene = level.getCutsceneTime();
            if (newScene)
            {
                currentGameState = GameState.Cutscene;

            }
        }
        public void textBoxing(int at)
        {
            bool newtime = level.getTextTime();
            
            if (newtime &&done)
            {
                if (!IsPressed(Keys.N) &&(dialogueIndex<paragraphPerChapter[chapter]))
                {
                    level.Player.text = true;
                    string thisLine = script[chapter][dialogueIndex];
                    string nextLine = "";
                    string firstLine = "";
                    string thirdLine = "";
                    if (thisLine.Length > 59 && thisLine.Length <=119)
                    {
                        firstLine = thisLine.Substring(0, 59);
                        nextLine = thisLine.Substring(59, thisLine.Length-60);
                    }
                    else if (thisLine.Length > 119)
                    {
                        firstLine = thisLine.Substring(0, 59);
                        nextLine = thisLine.Substring(59, 60);
                        thirdLine = thisLine.Substring(119, thisLine.Length - 120);
                    }
                    else
                    {
                        firstLine = thisLine;
                        nextLine = "";
                        thirdLine = "";
                    }

                    spriteBatch.Draw(textBox, new Rectangle(20, 50, 760, 200), Color.White);
                    spriteBatch.DrawString(hudFont, firstLine, new Vector2(50, 80), Color.Yellow);
                    spriteBatch.DrawString(hudFont, nextLine, new Vector2(50, 95), Color.Yellow);
                    spriteBatch.DrawString(hudFont, thirdLine, new Vector2(50, 110), Color.Yellow);
                    spriteBatch.DrawString(hudFont, "Press N to continue", new Vector2(550, 215), Color.Yellow);

                }
                else if (IsPressed(Keys.N))
                {
                    dialogueIndex++;
                    if (dialogueIndex >= paragraphPerChapter[chapter]) 
                    {
                        level.Player.text = false;
                        chapter++;
                        string textPath = string.Format("Content/Script/{0}.txt",chapter);
                        using (Stream fileStream = TitleContainer.OpenStream(textPath))
                            script.Add(LoadText2(fileStream));
                        level.setTextTime(false, dialogueIndex);
                        dialogueIndex = 0;
                    }
                }
            }
            
        }
        private void DrawHud()
        {
            spriteBatch.Begin();
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);
            textBoxing(dialogueIndex);
            cutsceneTrigger();
            //if(dialogueIndex<3){level.setTextTime(true, dialogueIndex);}
            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            //string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00 ") + dialogueIndex + " chapter: " + chapter + ", paragraphPerChapter: " + paragraphPerChapter[chapter]; //test
           /* Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);*/

            // Draw score
            //float timeHeight = hudFont.MeasureString(timeString).Y;
            //DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);
            
            //Draw textbox
            if (level.levelStart && currentGameState == GameState.Playing&&levelIndex==0)
            {
                if (Keyboard.GetState().GetPressedKeys().Length <= 0)
                {
                    spriteBatch.Draw(textBox, new Rectangle(20, 50, 700, 200), Color.White);
                    spriteBatch.DrawString(hudFont, "Where... where am I? Uhhh! My head! It's killing me!", new Vector2(50, 70), Color.Yellow);
                    spriteBatch.DrawString(hudFont, "What's going on? Wh- why can't I remember anything!", new Vector2(50, 86), Color.Yellow);
                    spriteBatch.DrawString(hudFont, "I... I have to go hom- home? Where's home? Back to who?", new Vector2(50, 102), Color.Yellow);
                    spriteBatch.DrawString(hudFont, "...", new Vector2(50, 118), Color.Yellow);
                    spriteBatch.DrawString(hudFont, "Wha- what is that?! Is that a gh-gh-ghost??", new Vector2(50, 134), Color.Yellow);
  

                }
                else
                {
                    level.levelStart = false;
                }
            }
            //bool still = false;

            // Determine the status overlay message to show.
            Texture2D status = null;
            //if (level.TimeRemaining == TimeSpan.Zero)
            //{
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
              //  else
              //  {
              //      status = loseOverlay;
              //  }
          //  }
            if (!level.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }
            spriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
        #endregion
        bool IsPressed(Keys key)
        {
            return (currentKeyboardState.IsKeyDown(key) &&
                lastKeyboardState.IsKeyUp(key));

        }
        int calculateShards()
        {
            int points=0;
            for (int i = 0; i < Progress.Length; i++)
            {
                for (int a = 1; a < 4; a++)
                {
                    if (Progress[i][a])
                    {
                        points++;
                    }
                }
            }
            return points;
        }
        private void checkPauseKey()
        {
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Escape) && keyboardState.IsKeyUp(Keys.Escape))
            {
                switch (currentGameState)
                    {
                        case GameState.Playing:
                            currentGameState = GameState.GameMenu;
                            this.IsMouseVisible = true;
                            break;
                        case GameState.GameMenu:
                            currentGameState = GameState.Playing;
                            this.IsMouseVisible = false;
                            break;
                    }
            }
            keyboardState = ks;
        }

        public Boolean IsEven(int number)
        {
            if (number % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
         private void SaveGame(string path)
        {
          using (StreamWriter writer = new StreamWriter(path))
             {
              // writer.WriteLine(position.X);
             //  writer.WriteLine(position.Y);
               for (int a = 0; a < Progress.Length; a++)
               {
                   for (int b = 0; b < Progress[a].Length; b++)
                   {
                       writer.WriteLine(Progress[a][b]);
                   }
               }
             }
        }
        private void LoadGame(string path)
        {
          using (StreamReader reader = new StreamReader(path))
          {
         // string line = reader.ReadLine();
         // loadedPosition.X = int.Parse(line);
 
         // string line2 = reader.ReadLine();
         // loadedPosition.Y = int.Parse(line2);

             for (int a = 0; a < Progress.Length; a++)
              {
                for (int b = 0; b < Progress[a].Length; b++)
                {
                     string line3 = reader.ReadLine();
                      Progress[a][b] = bool.Parse(line3);
                }
              }
          }
        }
        private void Load()
        {

            if (File.Exists("Game1.txt"))
            {
                LoadGame("Game1.txt");
            }
            else
            {
                resetProgress();
            }
        }
        private void Save()
        {
            SaveGame("Game1.txt");
        }
        private int findlevelindex()
        {
            int index=-1;
            for (int i=0; i < Progress.Length; i++)
            {
                if (Progress[i][0])
                {
                    index++;
                }
                else
                {
                    if (index < 0) { index = 0; }
                    return (index * 2) - 1;
                }
            }
            return (index*2)-1;
        }
        private void resetProgress()
        {
            for (int a = 0; a < Progress.Length; a++)
            {
                for (int b = 0; b < Progress[a].Length; b++)
                {
                    Progress[a][b] = false;
                }
            }
            levelIndex = -1;
        }
    }
}
