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
using System.IO;

namespace Assignment_8___Klusaw
{
    public class Terrain: DrawableGameComponent
    {

        int vertexCountX;   //Number of vertices of Vertex Grid in X direction, equivilent to the width of the height map image in pixels
        int vertexCountZ;   //Number of vertices of Vertex Grid in Z direction, equivilent to the height of the height map image in pixels
        float blockScale;   //Space in both X and Z direction between verteces in Vertex Grid
        float heightScale;  //Heights provided by height map will be values 0 – 255, amount at which to scale these values
        byte[] heightmap;   //Array of heights from height map
        int numVertices;    //Number of vertices in vertex grid
        int numTriangles;   //Number of triangles in vertex grid
        VertexBuffer vb;    //All the vertices that make up the vertex grid
        IndexBuffer ib;     //Indices of the vertices that make up the primitives that make up the terrain mesh
        BasicEffect effect; //Used for drawing the terrain
        Texture2D texture;  //Texture placed over the terrain mesh
        Vector2 StartPosition; // the upperleft hand corner of the map


        public Terrain(Game game) : base(game)
        {
            //Constructor
        
        }
            
        public override void Initialize()
        {
            //Basic Initialize
            base.Initialize();
            
        }

        public float GetExactHeightAt(float xCoord, float zCoord)
        {
            int xLower = (int)xCoord;
            int xHigher = xLower + 1;
            float xRelative = (xCoord - xLower) / ((float)xHigher - (float)xLower);

            int zLower = (int)zCoord;
            int zHigher = zLower + 1;
            float zRelative = (zCoord - zLower) / ((float)zHigher - (float)zLower);

            Vector2 positionInGrid = new Vector2(xCoord - StartPosition.X, zCoord - StartPosition.Y);
            Vector2 blockPosition = new Vector2(positionInGrid.X / blockScale, positionInGrid.Y / blockScale);
            Vector2 blockOffset = new Vector2(blockPosition.X - (int)blockPosition.X, blockPosition.Y - (int)blockPosition.Y);

            if (blockPosition.X >= 0 && blockPosition.X < (vertexCountX - 1) && blockPosition.Y >= 0 && blockPosition.Y < (vertexCountZ - 1))
            {
                int vertexIndex = (int)blockPosition.X + (int)blockPosition.Y * vertexCountX;

                float height1 = heightmap[vertexIndex + 1];
                float height2 = heightmap[vertexIndex];
                float height3 = heightmap[vertexIndex + vertexCountX + 1];
                float height4 = heightmap[vertexIndex + vertexCountX];

                float heightIncX, heightIncY;
                //Top triangle
                if (blockOffset.X > blockOffset.Y)
                {
                    heightIncX = height1 - height2;
                    heightIncY = height3 - height1;
                }
                //Bottom triangle
                else
                {
                    heightIncX = height3 - height4;
                    heightIncY = height4 - height2;
                }

                float lerpHeight = height2 + heightIncX * blockOffset.X + heightIncY * blockOffset.Y;


                return lerpHeight * heightScale;
            }
            else
            {
                return -99999;
            }
        }

       

        public void Load(string heightmapFileName, int vertexX, int vertexZ, float blockS, float heightS, Texture2D text)
        {

            //Non-overridden Load(), includes everything to generate terrain mesh
            Initialize();
            Console.WriteLine("In Load");
            effect = new BasicEffect(GraphicsDevice);
            vertexCountX = vertexX;
            vertexCountZ = vertexZ;
            blockScale = blockS;
            heightScale = heightS;
            texture = text;

            int heightmapSize = vertexCountX * vertexCountZ;
            heightmap = new byte[heightmapSize];
            FileStream filestream = File.OpenRead(Game.Content.RootDirectory + "/" + heightmapFileName + ".raw");

            filestream.Read(heightmap, 0, heightmapSize);
            //Be sure to close the stream
            filestream.Close();

            GenerateTerrainMesh();
        }

        private void GenerateTerrainMesh()
        {
            //Called by Load(), creates the Mesh of the terrain
            numVertices = vertexCountX * vertexCountZ;
            numTriangles = (vertexCountX - 1) * (vertexCountZ - 1) * 2;
            ushort[] indices = GenerateTerrainIndices();
            VertexPositionTexture[] vertices = GenerateTerrainVertices(indices);

            vb = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), numVertices, BufferUsage.WriteOnly);
            vb.SetData<VertexPositionTexture>(vertices);

            ib = new IndexBuffer(GraphicsDevice, typeof(ushort), numTriangles*3, BufferUsage.WriteOnly);
            ib.SetData<ushort>(indices);

        }

        private ushort[] GenerateTerrainIndices()
        {
            //Creates the indices of the vertices that make up the triangle primitives that make up the mesh
            int numIndices = numTriangles * 3;
            ushort[] indices = new ushort[numIndices];

            // The idea of each of the indices
            //T1 = V[vert], V[vert+1], V[vert+VertexCountX+1];
            //T2 = V[vert], V[vert+vertexCountX+1],V[vert+VertexCountX];

            int indicesCount = 0;
            for(int i = 0; i < (vertexCountZ - 1); i++) //All Rows except last
                for (int j = 0; j < (vertexCountX - 1); j++) //All Columns except last
                {
                    int index = j + i * vertexCountZ; //2D coordinates to linear
                    //First Triangle Vertices
                    indices[indicesCount++] = (ushort)index;
                    indices[indicesCount++] = (ushort)(index + 1);
                    indices[indicesCount++] = (ushort)(index + vertexCountX + 1);

                    //Second Triangle Vertices
                    indices[indicesCount++] = (ushort)(index + vertexCountX + 1);
                    indices[indicesCount++] = (ushort)(index + vertexCountX);
                    indices[indicesCount++] = (ushort)(index);
                }
            return indices;

        }

        private VertexPositionTexture[] GenerateTerrainVertices(ushort[] terrainIndeces)
        {
            //Creates the actual vertices that make up the vertex grid, setting their place in the 3D world based upon size of terrain, block scale, height map and height scale
            //Also determines texturing maping
            float halfTerrainWidth = (vertexCountX - 1) * blockScale * .5f;
            float halfTerrainDepth = (vertexCountZ - 1) * blockScale * .5f;
            StartPosition = new Vector2(-halfTerrainWidth, -halfTerrainDepth);
            float tuDerivative = 1.0f / (vertexCountX - 1);
            float tvDerivative = 1.0f / (vertexCountZ - 1);

            VertexPositionTexture[] vertices = new VertexPositionTexture[vertexCountX * vertexCountZ];

            int vertexCount = 0;
            float tu = 0;
            float tv = 0;

            for (float i = -halfTerrainDepth; i <= halfTerrainDepth; i += blockScale)
            {
                tu = 0.0f;
                for (float j = -halfTerrainWidth; j <= halfTerrainWidth; j += blockScale)
                {
                    vertices[vertexCount].Position = new Vector3(j, heightmap[vertexCount] *heightScale, i);
                    vertices[vertexCount].TextureCoordinate = new Vector2(tu, tv);

                    tu += tuDerivative;
                    vertexCount++;
                }
                tv += tvDerivative;
            }

            return vertices;

        }



        public void Draw(GameTime gameTime, Camera camera)
        {
            effect.World = Matrix.Identity; //No transformation of the terrain
            effect.View = camera.view;
            effect.Projection = camera.projection;
            effect.Texture = texture;
            effect.TextureEnabled = true;

		    GraphicsDevice.SetVertexBuffer(vb); //Set vertices
            GraphicsDevice.Indices = ib; //Set indices
            
            foreach (EffectPass CurrentPass in effect.CurrentTechnique.Passes)
            {
                CurrentPass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, 65025); //Draw all triangles that make up the mesh
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, numTriangles*3/2, 65025);
            }

           // Console.WriteLine("In TDraw");
            base.Draw(gameTime);
        }
    }
}