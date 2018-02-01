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
    class Powerup
    {
        public Vector3 Position;
        public Model model;
        public int respawnTime = 5000;

        public void Update(GameTime gameTime, bool a)
        {

        }

        public void Draw(Camera camera)
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
                    be.World = camera.projection * camera.view * mesh.ParentBone.Transform;
                }
                //Draw
                mesh.Draw();
            }

        }
    }
}
