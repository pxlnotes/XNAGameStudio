#region File Description
//-----------------------------------------------------------------------------
// DrawHelper.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Pickture
{
    /// <summary>
    /// Provides some helper methods for drawing.
    /// </summary>
    static class DrawHelper
    {
        /// <summary>
        /// Reset the rendering states to known values.
        /// </summary>
        //public static void SetState()
        //{
        //    RenderState state = Pickture.Instance.GraphicsDevice.RenderState;

        //    state.AlphaBlendEnable = true;
        //    state.SourceBlend = Blend.SourceAlpha;
        //    state.DestinationBlend = Blend.InverseSourceAlpha;
        //    state.AlphaSourceBlend = Blend.SourceAlpha;
        //    state.AlphaDestinationBlend = Blend.InverseSourceAlpha;

        //    state.DepthBufferEnable = true;
        //    state.DepthBufferWriteEnable = true;
        //}
        public static void SetState()
        {
            GraphicsDevice device = Pickture.Instance.GraphicsDevice;

            device.BlendState = new BlendState
            {
                ColorSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.SourceAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                ColorBlendFunction = BlendFunction.Add,
                AlphaBlendFunction = BlendFunction.Add
            };

            device.DepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = true,
                DepthBufferWriteEnable = true,
                DepthBufferFunction = CompareFunction.LessEqual
            };
        }

        /// <summary>
        /// Draws a ModelMeshPart with a custom effect.
        /// </summary>
        /// <param name="mesh">Mesh which owns the mesh part.</param>
        /// <param name="part">Mesh part to be drawn.</param>
        /// <param name="effect">Effect to draw the mesh part with.</param>
        //public static void DrawMeshPart(ModelMesh mesh, ModelMeshPart part,Effect effect)
        //{
        //    GraphicsDevice device = Pickture.Instance.GraphicsDevice;

        //    foreach(EffectPass pass in effect.CurrentTechnique.Passes)
        //    {
        //        pass.Begin();

        //        device.VertexDeclaration = part.VertexDeclaration;
        //        device.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset,
        //                                     part.VertexStride);
        //        device.Indices = mesh.IndexBuffer;

        //        device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
        //                                     part.BaseVertex, 0, part.NumVertices,
        //                                     part.StartIndex, part.PrimitiveCount);

        //        pass.End();
        //    }
        //}
        public static void DrawMeshPart(ModelMesh mesh, ModelMeshPart part, Effect effect)
        {
            GraphicsDevice device = Pickture.Instance.GraphicsDevice;

            // XNA 4.0 / MonoGame: bind buffers directly
            device.SetVertexBuffer(part.VertexBuffer);
            device.Indices = part.IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,                    // baseVertex (removed)
                    0,                    // minVertexIndex
                    part.NumVertices,     // numVertices
                    part.StartIndex,      // startIndex
                    part.PrimitiveCount
                );
            }
        }
    }
}
