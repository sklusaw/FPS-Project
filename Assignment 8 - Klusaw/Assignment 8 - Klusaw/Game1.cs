using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;

namespace Assignment_8___Klusaw
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 

    public enum GameState
    {
        SignIn, FindSession,
        CreateSession, Start, InGame, GameOver
    }

    public enum MessageType
    {
        EndGame,StartGame,RejoinLobby,RestartGame,UpdatePlayerPos
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        SpriteBatch  spriteBatch;
        GraphicsDeviceManager graphics; // The graphics manager
        Terrain terrain; // The terrain of the map
        Camera camera; // the camera/player

        //Initial state to trigger Windows LIVE signin
        GameState currentGameState = GameState.SignIn;
        NetworkSession networkSession;
        PacketWriter packetWriter = new PacketWriter();
        PacketReader packetReader = new PacketReader();


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Components.Add(new GamerServicesComponent(this));
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            //GraphicsDevice.RasterizerState = rs;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            camera = new Camera(this, new Vector3(0f, 120f, 0f), new Vector3(0f, 120f, 5f), Vector3.Up);
            terrain = new Terrain(this);

            terrain.Load("v_H", 256, 256, 7f, 2f, Content.Load<Texture2D>("Volcano_T"));
            Components.Add(camera);
            Components.Add(terrain);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

                        // Only run the Update code if the game is currently active.
            // This prevents the game from progressing while
            // gamer services windows are open.
            if (this.IsActive)
            {
                // Run different methods based on game state
                switch (currentGameState)
                {
                    case GameState.SignIn:
                        Update_SignIn();
                        break;
                    case GameState.FindSession:
                        Update_FindSession();
                        break;
                    case GameState.CreateSession:
                        Update_CreateSession();
                        break;
                    case GameState.Start:
                        Update_Start(gameTime);
                        camera.Update(gameTime, terrain);
                        break;
                    case GameState.InGame:
                        Update_InGame(gameTime);
                        camera.Update(gameTime, terrain);
                        break;
                    case GameState.GameOver:
                        Update_GameOver(gameTime);
                        camera.Update(gameTime, terrain);
                        break;
                }
            }
            // Update the network session 
	        if (networkSession != null)
                networkSession.Update();


            

            base.Update(gameTime);
        }

        protected void Update_SignIn()
         {
               // If no local gamers are signed in, show sign-in screen
               if (Gamer.SignedInGamers.Count < 1)
               {
		        //Guide is a part of GamerServices, this will bring up the
                   //SignIn window, allowing 1 user to Sign in. The false allows
                   //users to sign in locally
                     Guide.ShowSignIn(1, false);
                }
                else
                {
                     // Local gamer signed in, move to find sessions
                     currentGameState = GameState.FindSession;
                 }
        }

         private void Update_FindSession()
        {
            // Find sesssions of the current game over SystemLink, 1 local gamer,
	     //no special properties
            AvailableNetworkSessionCollection sessions =
                NetworkSession.Find(NetworkSessionType.SystemLink, 1, null);
            
            if (sessions.Count == 0)
            {
                // If no sessions exist, move to the CreateSession game state
                currentGameState = GameState.CreateSession;
            }
            else
            {
                // If a session does exist, join it, wire up events,
                // and move to the Start game state
                networkSession = NetworkSession.Join(sessions[0]);
                WireUpEvents();
                currentGameState = GameState.Start;
            }
        }

         protected void WireUpEvents()
        {
            // Wire up events for gamers joining and leaving, defines what to do when a gamer
            //Joins or leaves the session
            networkSession.GamerJoined += GamerJoined;
            networkSession.GamerLeft += GamerLeft;
        }

        void GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            // Gamer joined. Set the tag for the gamer to a new UserControlledObject.
            // These Tags are going to be your local representation of remote players
            if (e.Gamer.IsHost)
            {
	      //The Create players will create and return instances of your player class, setting
	      //the appropriate values to differentiate between local and remote players
	      //Tag is of type Object, which means it can hold any type
                e.Gamer.Tag = CreateLocalPlayer();
            }
            else
            {
                e.Gamer.Tag = CreateRemotePlayer();
            }
        }

        private object CreateLocalPlayer()
        {
            //local
            PlayerClass player = new PlayerClass(new Vector3(100, 100, 100), Content.Load<Texture2D>("rocketcross"), Content.Load<Texture2D>("FlameCross"), Content.Load<Model>("spaceship"));
            return player;
        }

        private object CreateRemotePlayer()
        {
            //remote
            PlayerClass player = new PlayerClass(new Vector3(100, 100, 100), Content.Load<Model>("robot"));
            return player;
        }

         void GamerLeft(object sender, GamerLeftEventArgs e)
        {
            // Dispose of the network session, set it to null.
            networkSession.Dispose();
            networkSession = null;

		 //Perform any necessary clean up,
		 //stop sound track, etc.
        
		 //Go back to looking for another session
		 currentGameState = GameState.FindSession;
        }

        private void Update_CreateSession()
        {
            // Create a new session using SystemLink with a max of 1
            // local player and a max of 2 total players
            networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 2);
	       //If the host drops, other player becomes host
            networkSession.AllowHostMigration = true;
	       //Cannot join a game in progress
            networkSession.AllowJoinInProgress = false;
        
            // Wire up events and move to the Start game state
            WireUpEvents();
            currentGameState = GameState.Start;
        }

        private void Update_Start(GameTime gameTime)
 {
            // Get local gamer, should be just one
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];
            
            // Check for game start key or button press
            // only if there are two players
            if (networkSession.AllGamers.Count >= 1)
            {
                // If space bar or Start button is pressed, begin the game
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) ||
                    GamePad.GetState(PlayerIndex.One).Buttons.Start ==
                    ButtonState.Pressed)
                {
                    // Send message to other player that we're starting
                    packetWriter.Write((int)MessageType.StartGame);
                    localGamer.SendData(packetWriter, SendDataOptions.Reliable);
                    
                    // Call StartGame
                    StartGame();
                }
            }
            // Process any incoming packets
            ProcessIncomingData(gameTime);
  }

         protected void StartGame()
            {
		            // Set game state to InGame
                        currentGameState = GameState.InGame;
            
                       // Any other things that need to be set up
		            //for beginning a game
		            //Starting audio, resetting values, etc.
            }

        protected void ProcessIncomingData(GameTime gameTime)
        {
            // Process incoming data
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];
            
            // While there are packets to be read...
            while (localGamer.IsDataAvailable)
            {
                // Get the packet and info on sender
                NetworkGamer sender;
                localGamer.ReceiveData(packetReader, out sender);
            
                // Ignore the packet if you sent it
                if (!sender.IsLocal)
                {
                    // Read messagetype from start of packet and call appropriate method
                    MessageType messageType = (MessageType)packetReader.ReadInt32();
                    switch (messageType)
                    {
                        case MessageType.EndGame:
                            EndGame();
                            break;
                        case MessageType.StartGame:
                            StartGame();
                            break;
                        case MessageType.RejoinLobby:
                            currentGameState = GameState.Start;
                            break;
                        case MessageType.RestartGame:
                            StartGame();
                            break;
                        case MessageType.UpdatePlayerPos:
                            UpdateRemotePlayer(gameTime);
                            break;
		//Any other actions for specific messages

                    }
                }
            }
        }


         protected void EndGame()
        {
		   //Perform whatever actions are to occur
		  //when a game ends. Stop music, play
		  //A certain sound effect, etc.
		  currentGameState = GameState.GameOver;
        }

        protected void UpdateRemotePlayer(GameTime gameTime)
        {
            // Get the other (non-local) player
            NetworkGamer theOtherGuy = GetOtherPlayer();
            
            // Get the PlayerClass representing the other player
            PlayerClass theOtherPlayer = ((PlayerClass)theOtherGuy.Tag);
            
            // Read in the new position of the other player
            Vector3 otherGuyPos = packetReader.ReadVector3();
            Matrix otherworld = packetReader.ReadMatrix();
            // Set the position
            theOtherPlayer.Position = otherGuyPos;
            theOtherPlayer.world = otherworld;
	       //Read any other information from the packet and handle it            
}

         protected NetworkGamer GetOtherPlayer()
        {
            // Search through the list of players and find the
            // one that's remote
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                if (!gamer.IsLocal)
                {
                    return gamer;
                }
            }
            return null;
        }

         private void Update_InGame(GameTime gameTime)
 {
            // Update the local player
            UpdateLocalPlayer(gameTime);

	     // Read any incoming data
            ProcessIncomingData(gameTime);
            
            // Only host checks for endgame
            if (networkSession.IsHost)
            {
                // Check for end game conditions, if they are met send a message to other player
                if(1==0)
                {
                    packetWriter.Write((int)MessageType.EndGame);
                    networkSession.LocalGamers[0].SendData(packetWriter, SendDataOptions.Reliable);
                    EndGame();
                }

            }
            
}

         protected void UpdateLocalPlayer(GameTime gameTime)
        {
            // Get local player
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];
            
            // Get the local player's sprite
            PlayerClass local = (PlayerClass)localGamer.Tag;
            
            // Call the local's Update method, which will process user input
            // for movement and update the animation frame
	        //Boolean used to inform the Update function that the local player is calling update,
            //therefore update based on local input
            local.Update(gameTime, true,camera.cameraPosition);
            
	  // Send message to other player with message tag and new position of local player
            packetWriter.Write((int)MessageType.UpdatePlayerPos);
            packetWriter.Write(camera.cameraPosition);
            packetWriter.Write(local.world);
            
	  // Send data to other player
            localGamer.SendData(packetWriter, SendDataOptions.InOrder);

	 //Package up any other necessary data and send it to other player

}

         private void Update_GameOver(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadSate = GamePad.GetState(PlayerIndex.One);
            
            // If player presses Enter or A button, restart game
            if (keyboardState.IsKeyDown(Keys.Enter) ||
                gamePadSate.Buttons.A == ButtonState.Pressed)
            {
                // Send restart game message
                packetWriter.Write((int)MessageType.RestartGame);
                networkSession.LocalGamers[0].SendData(packetWriter,
                    SendDataOptions.Reliable);
              //  RestartGame();
            }

            // If player presses Escape or B button, rejoin lobby
            if (keyboardState.IsKeyDown(Keys.Escape) ||
                gamePadSate.Buttons.B == ButtonState.Pressed)
            {
                // Send rejoin lobby message
                packetWriter.Write((int)MessageType.RejoinLobby);
                networkSession.LocalGamers[0].SendData(packetWriter,
                    SendDataOptions.Reliable);
              //  RejoinLobby();
            }

            // Read any incoming messages
            ProcessIncomingData(gameTime);
        }

         private void RestartGame()
         {
             throw new NotImplementedException();
         }

         private void RejoinLobby()
         {
             throw new NotImplementedException();
         }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
           // 
            terrain.Draw(gameTime, camera);
                        if (this.IsActive)
            {
                // Based on the current game state,
                // call the appropriate method
                switch (currentGameState)
                {
                    case GameState.SignIn:
                    case GameState.FindSession:
                    case GameState.CreateSession:
                        GraphicsDevice.Clear(Color.DarkBlue);
                        break;
                    case GameState.Start:
		//Write function to draw the start screen
                        DrawStartScreen();
                        break;
                    case GameState.InGame:
		//Write function to handle draws during game time (terrain, models, etc)‏
                       DrawInGameScreen(gameTime);
                        break;
                    case GameState.GameOver:
		//Write function to draw game over screen
                       DrawGameOverScreen();
                        break;
                }
            }


                   //     

            

            base.Draw(gameTime);
        }

        private void DrawStartScreen()
        {
            // Draw a splash screen
        }

        private void DrawGameOverScreen()
        {
            PlayerClass theGamer;
            // Draw the remote player models
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                if (!gamer.IsLocal)
                {
                    theGamer = ((PlayerClass)gamer.Tag);
                    theGamer.Draw(camera);
                }
            }

            // Draw Game over screen
        }

        private void DrawInGameScreen(GameTime gameTime)
        {
            PlayerClass theGamer;
            // Draw the player models and HUD
            // Local player class controls the local HUD
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                    theGamer = ((PlayerClass)gamer.Tag);
                    theGamer.Draw(camera);
                    if (theGamer.Location == 0)
                    {
                        spriteBatch.Begin();

                        // DRAW THE UI
                        if (theGamer.currentWeapon == 1)
                        {
                            spriteBatch.Draw(theGamer.FlameThrowerCrossTexture, new Vector2((Window.ClientBounds.Width / 2) - 37, (Window.ClientBounds.Height / 2) - 37), Color.White);
                            spriteBatch.Draw(Content.Load<Texture2D>("fire"), new Vector2(Window.ClientBounds.Width - 75, Window.ClientBounds.Height - 75), Color.White);
                        }
                        else
                        {
                            spriteBatch.Draw(theGamer.RocketCrossTexture, new Vector2((Window.ClientBounds.Width / 2) - 37, (Window.ClientBounds.Height / 2) - 37), Color.White);
                            spriteBatch.Draw(Content.Load<Texture2D>("rocket"), new Vector2(Window.ClientBounds.Width - 75, Window.ClientBounds.Height - 75), Color.White);
                        }
                        int currentLife = (int)theGamer.health / 2;

                        for (int i = 0; i < currentLife; i++)
                        {
                            spriteBatch.Draw(Content.Load<Texture2D>("life"), new Vector2(25*i, 0), Color.White);
                        }
                        
                        spriteBatch.End();
                    }

            }
        }
    }
}
