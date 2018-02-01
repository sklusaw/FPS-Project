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

namespace Assignment_8___Klusaw
{
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        float speed = (float)2; // The current integet speed of the camera. this allows the user to move at a more comfortable rate of speed
        MouseState prevMouseState; // Keeps track if the button is still pressed or not.

        float totalPitch = MathHelper.PiOver4 / 2; // This keeps track of the maximum pitch range
        public float currentPitch = 0; // This keeps track of the maximum pitch
        public float jumpingMode = 0; // what mode of jumping (0 = not jumping, 1 = rising, 2 = falling
        public float jumpingoffset = 0; // goes up to 25. 
        public float TotalHeightofJump = 0; // the peak height
        public float JumpHeight = 0; // the height of the camera before the jump.


        public Matrix view // the current view (2D Plane / Game window) of the camera
        {
            get;
            protected set;
        }

        public Matrix projection // The current projection (outwards) of the camera
        {
            get;
            protected set;
        }

        public Vector3 cameraPosition { get; protected set; } // The current position of the camera in the world
        Vector3 cameraDirection; // The current vector direction of the camera
        public Vector3 cameraUp; // The current up of the camera
        public Vector3 cameraTrueUp; // The current up of the world

        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
            : base(game)
        {
            cameraPosition = pos;
            cameraDirection = target - pos;
            cameraDirection.Normalize(); //Make it a unit vector
            cameraUp = up;
            cameraTrueUp = up;
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            prevMouseState = Mouse.GetState();
            CreateLookAt();

            view = Matrix.CreateLookAt(pos, target, up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)game.Window.ClientBounds.Width / (float)game.Window.ClientBounds.Height, 1, 10000);
        }



        private void CreateLookAt()
        {
            view = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);
        }


        public void Update(GameTime gameTime, Terrain terrain)
        {

            if (Keyboard.GetState().IsKeyDown(Keys.Space) && jumpingMode == 0)
            {
                jumpingMode = 1;
                TotalHeightofJump = terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) + 35;
                JumpHeight = TotalHeightofJump - 25;
            }
            
            if (jumpingMode == 1)
            {
                if (jumpingoffset < 24.3f)
                {
                    jumpingoffset += (25 - jumpingoffset) / 8;
                }
                else
                {
                    jumpingMode = 2;
                }
            }
            else
            {
                if (jumpingMode == 2)
                {
                    if (TotalHeightofJump >= terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z)+20)
                    {
                        jumpingoffset -= (25 - jumpingoffset) / 8;
                        TotalHeightofJump -= (25 - jumpingoffset) / 8;
                    }
                    else
                    {
                        jumpingMode = 0;
                        jumpingoffset = 0;
                    }
                }
            }

            if (jumpingMode == 0)
            {
                Vector3 tempCamPosition = cameraPosition;
                cameraPosition = new Vector3(tempCamPosition.X, terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) + 25 + jumpingoffset, tempCamPosition.Z);

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X, terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) + 25 + jumpingoffset, tempCamPosition.Z);
                    }
                }
                tempCamPosition = cameraPosition;
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    tempCamPosition -= cameraDirection * speed;
                    //tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X, terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) + 25 + jumpingoffset, tempCamPosition.Z);
                    }
                }
                tempCamPosition = cameraPosition;
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    tempCamPosition += Vector3.Cross(cameraTrueUp, cameraDirection) * speed;
                    //tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X, terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) + 25 + jumpingoffset, tempCamPosition.Z);
                    }
                }
                tempCamPosition = cameraPosition;
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    tempCamPosition -= Vector3.Cross(cameraTrueUp, cameraDirection) * speed;
                    //tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X, terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) + 25 + jumpingoffset, tempCamPosition.Z);
                    }
                }
            }

            if (jumpingMode == 1)
            {
                Vector3 tempCamPosition = cameraPosition;
                cameraPosition = new Vector3(tempCamPosition.X, terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) + 25 + jumpingoffset, tempCamPosition.Z);

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X, JumpHeight + jumpingoffset, tempCamPosition.Z);
                    }
                }
                tempCamPosition = cameraPosition;
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    tempCamPosition -= cameraDirection * speed;
                    //tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X, JumpHeight + jumpingoffset, tempCamPosition.Z);
                    }
                }
                tempCamPosition = cameraPosition;
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    tempCamPosition += Vector3.Cross(cameraTrueUp, cameraDirection) * speed;
                    //tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X,JumpHeight + jumpingoffset, tempCamPosition.Z);
                    }
                }
                tempCamPosition = cameraPosition;
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    tempCamPosition -= Vector3.Cross(cameraTrueUp, cameraDirection) * speed;
                    //tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X, JumpHeight + jumpingoffset, tempCamPosition.Z);
                    }
                }
            }

            if (jumpingMode == 2)
            {
                Vector3 tempCamPosition = cameraPosition;
                cameraPosition = new Vector3(tempCamPosition.X, terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) + 25 + jumpingoffset, tempCamPosition.Z);

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X,  TotalHeightofJump, tempCamPosition.Z);
                    }
                }
                tempCamPosition = cameraPosition;
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    tempCamPosition -= cameraDirection * speed;
                    //tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X,  TotalHeightofJump, tempCamPosition.Z);
                    }
                }
                tempCamPosition = cameraPosition;
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    tempCamPosition += Vector3.Cross(cameraTrueUp, cameraDirection) * speed;
                    //tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X, TotalHeightofJump, tempCamPosition.Z);
                    }
                }
                tempCamPosition = cameraPosition;
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                {
                    tempCamPosition -= Vector3.Cross(cameraTrueUp, cameraDirection) * speed;
                    //tempCamPosition += cameraDirection * speed;
                    if (terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) < 6 && terrain.GetExactHeightAt(tempCamPosition.X, tempCamPosition.Z) - terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z) > -100)
                    {
                        cameraPosition = new Vector3(tempCamPosition.X, TotalHeightofJump, tempCamPosition.Z);
                    }
                }
            }

                    cameraDirection = Vector3.Transform(cameraDirection, Matrix.CreateFromAxisAngle(cameraTrueUp, (-MathHelper.PiOver4 / 150) * (Mouse.GetState().X - prevMouseState.X)));

                    float pitchAngle = (-MathHelper.PiOver4 / 150) * (Mouse.GetState().Y - prevMouseState.Y);
                     if (Math.Abs(currentPitch + pitchAngle) < totalPitch)
                     {
                    cameraDirection = Vector3.Transform(cameraDirection, Matrix.CreateFromAxisAngle(Vector3.Cross(cameraTrueUp, cameraDirection), (MathHelper.PiOver4 / 150) * (Mouse.GetState().Y - prevMouseState.Y)));
                    cameraUp = Vector3.Transform(cameraTrueUp, Matrix.CreateFromAxisAngle(Vector3.Cross(cameraTrueUp, cameraDirection), (MathHelper.PiOver4 / 150) * (Mouse.GetState().Y - prevMouseState.Y)));
                    currentPitch += pitchAngle;
                    
                     }

                    
                Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
                //prevMouseState = Mouse.GetState();

                CreateLookAt();
                base.Update(gameTime);
            }
        }
    }
