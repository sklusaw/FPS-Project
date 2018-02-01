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
    class PlayerClass
    {
        public Vector3 Position;

        public int Location; // 0 = local, 1 = remote;

        public Matrix world = Matrix.Identity;

        public Matrix worldTranslation = Matrix.Identity;
        public Matrix worldRotation = Matrix.Identity;


        public int status = 3; // 1 = alive, 2 = powerup, 3 = dead - Start Dead
        public float health = 20; // Rockets take 20 health on direct hit, 10 on splash, Flames = 0.5

        public KeyboardState prevKeysState = Keyboard.GetState();

        public Model model;

        public int kills = 0;
        public int deaths = 0;

        public int currentWeapon = 1; // 1 = Flamethrower, 2 = Rocket

        public int rocketAmmo = 4;
        public int FlameAmmo = 3000;

        public Texture2D RocketCrossTexture;
        public Texture2D FlameThrowerCrossTexture;

        public PlayerClass(Vector3 Pos, Texture2D RCT, Texture2D FCT, Model PlayModel)
        {
            // local
            Position = Pos;
            RocketCrossTexture = RCT;
            FlameThrowerCrossTexture = FCT;
            model = PlayModel;
            Location = 0;
        }

        public PlayerClass(Vector3 Pos, Model PlayModel)
        {
            //remote
            Position = Pos;
            model = PlayModel;
            Location = 1;
        }

        public void Update(GameTime gameTime, bool a, Vector3 position)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.E) && !prevKeysState.IsKeyDown(Keys.E))
            {
                if (currentWeapon == 1)
                    currentWeapon = 2;
                else
                    currentWeapon = 1;


            }
            KeyboardState keyState = Keyboard.GetState();
            if (Location == 0)
            {
                Position = position;
                if (keyState.IsKeyDown(Keys.D) == true)
                    world *= Matrix.CreateRotationY(-.01f);
                if (keyState.IsKeyDown(Keys.A) == true)
                    world *= Matrix.CreateRotationY(.01f);

                // move
                if (keyState.IsKeyDown(Keys.W) == true)
                    worldTranslation *= Matrix.CreateTranslation(Vector3.Transform(new Vector3(0, 0, -1), world));
                if (keyState.IsKeyDown(Keys.S) == true)
                    worldTranslation *= Matrix.CreateTranslation(Vector3.Transform(new Vector3(0, 0, 1), world));

            }
            else 
            {
                worldTranslation = Matrix.CreateTranslation(Position);
            
            }

            prevKeysState = Keyboard.GetState();
        }

        public void Draw(Camera camera)
        {
            if (Location == 0)
            {

            }
            else
            {
                //Set transforms
                Matrix[] transforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(transforms);

                //Loop through meshes and their effects 
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect be in mesh.Effects)
                    {
                        //Set BasicEffect information
                        be.EnableDefaultLighting();
                        be.Projection = camera.projection;
                        be.View = camera.view;
                        be.World = Matrix.CreateScale(100) * GetWorld() * mesh.ParentBone.Transform;
                    }
                    //Draw
                    mesh.Draw();
                }
            }
        }

        public virtual Matrix GetWorld()
        {
            return world * worldTranslation;
        }
    }
}
