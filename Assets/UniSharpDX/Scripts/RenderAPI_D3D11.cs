using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;


static class DxExtensions
{
    public static SharpDX.Vector3 ToDx(this UnityEngine.Vector3 v)
    {
        return new SharpDX.Vector3(v.x, v.y, v.z);
    }
    public static SharpDX.Vector2 ToDx(this UnityEngine.Vector2 v)
    {
        return new SharpDX.Vector2(v.x, v.y);
    }
}


struct MyVertex
{
    float x, y, z;
    uint color;

    public MyVertex(float _x, float _y, float _z, uint _c)
    {
        x = _x;
        y = _y;
        z = _z;
        color = _c;
    }
}


struct MeshVertex
{
    public SharpDX.Vector3 pos;
    public SharpDX.Vector3 normal;
    public SharpDX.Vector2 uv;
};


class RenderAPI_D3D11: System.IDisposable
{
    //
    // Which then was compiled with:
    // fxc /Tvm_4_0_level_9_3 /EVS source.hlsl /Fh outVS.h /Qstrip_reflect /Qstrip_debug /Qstrip_priv
    // fxc /Tpm_4_0_level_9_3 /EPS source.hlsl /Fh outPS.h /Qstrip_reflect /Qstrip_debug /Qstrip_priv
    // and results pasted & formatted to take less lines here
    byte[] kVertexShaderCode = new byte[]
    {
    68,88,66,67,86,189,21,50,166,106,171,1,10,62,115,48,224,137,163,129,1,0,0,0,168,2,0,0,4,0,0,0,48,0,0,0,0,1,0,0,4,2,0,0,84,2,0,0,
    65,111,110,57,200,0,0,0,200,0,0,0,0,2,254,255,148,0,0,0,52,0,0,0,1,0,36,0,0,0,48,0,0,0,48,0,0,0,36,0,1,0,48,0,0,0,0,0,
    4,0,1,0,0,0,0,0,0,0,0,0,1,2,254,255,31,0,0,2,5,0,0,128,0,0,15,144,31,0,0,2,5,0,1,128,1,0,15,144,5,0,0,3,0,0,15,128,
    0,0,85,144,2,0,228,160,4,0,0,4,0,0,15,128,1,0,228,160,0,0,0,144,0,0,228,128,4,0,0,4,0,0,15,128,3,0,228,160,0,0,170,144,0,0,228,128,
    2,0,0,3,0,0,15,128,0,0,228,128,4,0,228,160,4,0,0,4,0,0,3,192,0,0,255,128,0,0,228,160,0,0,228,128,1,0,0,2,0,0,12,192,0,0,228,128,
    1,0,0,2,0,0,15,224,1,0,228,144,255,255,0,0,83,72,68,82,252,0,0,0,64,0,1,0,63,0,0,0,89,0,0,4,70,142,32,0,0,0,0,0,4,0,0,0,
    95,0,0,3,114,16,16,0,0,0,0,0,95,0,0,3,242,16,16,0,1,0,0,0,101,0,0,3,242,32,16,0,0,0,0,0,103,0,0,4,242,32,16,0,1,0,0,0,
    1,0,0,0,104,0,0,2,1,0,0,0,54,0,0,5,242,32,16,0,0,0,0,0,70,30,16,0,1,0,0,0,56,0,0,8,242,0,16,0,0,0,0,0,86,21,16,0,
    0,0,0,0,70,142,32,0,0,0,0,0,1,0,0,0,50,0,0,10,242,0,16,0,0,0,0,0,70,142,32,0,0,0,0,0,0,0,0,0,6,16,16,0,0,0,0,0,
    70,14,16,0,0,0,0,0,50,0,0,10,242,0,16,0,0,0,0,0,70,142,32,0,0,0,0,0,2,0,0,0,166,26,16,0,0,0,0,0,70,14,16,0,0,0,0,0,
    0,0,0,8,242,32,16,0,1,0,0,0,70,14,16,0,0,0,0,0,70,142,32,0,0,0,0,0,3,0,0,0,62,0,0,1,73,83,71,78,72,0,0,0,2,0,0,0,
    8,0,0,0,56,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,7,7,0,0,65,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,1,0,0,0,
    15,15,0,0,80,79,83,73,84,73,79,78,0,67,79,76,79,82,0,171,79,83,71,78,76,0,0,0,2,0,0,0,8,0,0,0,56,0,0,0,0,0,0,0,0,0,0,0,
    3,0,0,0,0,0,0,0,15,0,0,0,62,0,0,0,0,0,0,0,1,0,0,0,3,0,0,0,1,0,0,0,15,0,0,0,67,79,76,79,82,0,83,86,95,80,111,115,
    105,116,105,111,110,0,171,171
    };

    byte[] kPixelShaderCode = new byte[]
    {
    68,88,66,67,196,65,213,199,14,78,29,150,87,236,231,156,203,125,244,112,1,0,0,0,32,1,0,0,4,0,0,0,48,0,0,0,124,0,0,0,188,0,0,0,236,0,0,0,
    65,111,110,57,68,0,0,0,68,0,0,0,0,2,255,255,32,0,0,0,36,0,0,0,0,0,36,0,0,0,36,0,0,0,36,0,0,0,36,0,0,0,36,0,1,2,255,255,
    31,0,0,2,0,0,0,128,0,0,15,176,1,0,0,2,0,8,15,128,0,0,228,176,255,255,0,0,83,72,68,82,56,0,0,0,64,0,0,0,14,0,0,0,98,16,0,3,
    242,16,16,0,0,0,0,0,101,0,0,3,242,32,16,0,0,0,0,0,54,0,0,5,242,32,16,0,0,0,0,0,70,30,16,0,0,0,0,0,62,0,0,1,73,83,71,78,
    40,0,0,0,1,0,0,0,8,0,0,0,32,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,15,15,0,0,67,79,76,79,82,0,171,171,79,83,71,78,
    44,0,0,0,1,0,0,0,8,0,0,0,32,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,15,0,0,0,83,86,95,84,65,82,71,69,84,0,171,171
    };

    SharpDX.Direct3D11.Device m_Device;
    Buffer m_VB; // vertex buffer
    Buffer m_CB; // constant buffer
    VertexShader m_VertexShader;
    PixelShader m_PixelShader;
    InputLayout m_InputLayout;
    RasterizerState m_RasterState;
    BlendState m_BlendState;
    DepthStencilState m_DepthState;

    RenderAPI_D3D11(SharpDX.Direct3D11.Device device)
    {
        m_Device = device;

        CreateResources();
    }

    public static RenderAPI_D3D11 Create(UnityEngine.Texture2D tex, UnityEngine.Mesh mesh)
    {
        using (var dxTexture = new SharpDX.Direct3D11.Texture2D(tex.GetNativeTexturePtr()))
        {
            var d3d11 = new RenderAPI_D3D11(dxTexture.Device);
            d3d11.SetTextureFromUnity(tex.GetNativeTexturePtr(), tex.width, tex.height);
            d3d11.SetMeshBuffersFromUnity(mesh.GetNativeVertexBufferPtr(0)
                , mesh.vertexCount
                , mesh.vertices
                , mesh.normals
                , mesh.uv);
            return d3d11;
        }
    }

    float m_Time;
    public void SetTimeFromUnity(float t)
    {
        m_Time = t;
    }

    System.IntPtr m_TextureHandle;
    int m_TextureWidth = 0;
    int m_TextureHeight = 0;
    public void SetTextureFromUnity(System.IntPtr texture, int w, int h)
    {
        m_TextureHandle = texture;
        m_TextureWidth = w;
        m_TextureHeight = h;
    }

    System.IntPtr m_VertexBufferHandle;
    int m_VertexBufferVertexCount;
    List<MeshVertex> m_VertexSource;
    public void SetMeshBuffersFromUnity(System.IntPtr vertexBuffer
        , int vertexCount
        , UnityEngine.Vector3[] sourceVertices
        , UnityEngine.Vector3[] sourceNormals
        , UnityEngine.Vector2[] sourceUVs)
    {
        m_VertexBufferHandle = vertexBuffer;
        m_VertexBufferVertexCount = vertexCount;
        if (m_VertexSource == null)
        {
            m_VertexSource = new List<MeshVertex>();
        }
        for (int i = 0; i < vertexCount; ++i)
        {
            m_VertexSource.Add(new MeshVertex
            {
                pos = sourceVertices[i].ToDx(),
                normal = sourceNormals[i].ToDx(),
                uv = sourceUVs[i].ToDx(),
            });
        }
    }

    bool GetUsesReverseZ()
    {
        var level = m_Device.FeatureLevel;
        var use=level>= SharpDX.Direct3D.FeatureLevel.Level_10_0;
        return use;
    }

    public void DrawSimpleTriangles(float[] worldMatrix, int triangleCount, MyVertex[] verticesFloat3Byte4)
    {
        var ctx = m_Device.ImmediateContext;
        //using (var ctx = m_Device.ImmediateContext)
        {
            // Set basic render state
            ctx.OutputMerger.SetDepthStencilState(m_DepthState, 0);
            ctx.Rasterizer.State = m_RasterState;
            ctx.OutputMerger.SetBlendState(m_BlendState, null, 0xFFFFFFFF);

            // Update constant buffer - just the world matrix in our case
            ctx.UpdateSubresource(worldMatrix, m_CB);

            // Set shaders
            ctx.VertexShader.SetConstantBuffers(0, 1, m_CB);
            ctx.VertexShader.SetShader(m_VertexShader, null, 0);
            ctx.PixelShader.SetShader(m_PixelShader, null, 0);

            // Update vertex buffer
            const int kVertexSize = 12 + 4;
            ctx.UpdateSubresource(verticesFloat3Byte4, m_VB);

            // set input assembler data and draw
            ctx.InputAssembler.InputLayout=m_InputLayout;
            ctx.InputAssembler.PrimitiveTopology=SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            var stride = kVertexSize;
            var offset = 0;
            ctx.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_VB, stride, offset));
            ctx.Draw(triangleCount * 3, 0);
        }
    }

    public byte[] BeginModifyTexture(System.IntPtr textureHandle, int textureWidth, int textureHeight, out int outRowPitch)
    {
        var rowPitch = textureWidth * 4;
        // Just allocate a system memory buffer here for simplicity
        var data = new byte[rowPitch * textureHeight];
        outRowPitch = rowPitch;
        return data;
    }

    public void EndModifyTexture(System.IntPtr textureHandle, int textureWidth, int textureHeight, int rowPitch, byte[] dataPtr)
    {
        using (var d3dtex = new SharpDX.Direct3D11.Texture2D(textureHandle))
        {
            var ctx = m_Device.ImmediateContext;
            // Update texture data, and free the memory buffer
            ctx.UpdateSubresource(dataPtr, d3dtex, 0, rowPitch, 0);
        }
    }

    public void BeginModifyVertexBuffer(System.IntPtr bufferHandle
        , System.Func<int, byte[]> getPadding
        , System.Action<SharpDX.DataStream, byte[]> callback
        )
    {
        var d3dbuf = new Buffer(bufferHandle);
        //using (var d3dbuf = new Buffer(bufferHandle))
        {
            var desc = d3dbuf.Description;
            var ctx = m_Device.ImmediateContext;
            var padding = getPadding(desc.SizeInBytes);

            var mapped =ctx.MapSubresource(d3dbuf, 0
                , MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);

            using(var stream=new SharpDX.DataStream(mapped.DataPointer, desc.SizeInBytes, false, true))
            {
                callback(stream, padding);
            }

            ctx.UnmapSubresource(d3dbuf, 0);
        }
    }

    private void CreateResources()
    {
        {
            var desc = new BufferDescription
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = 1024,
                BindFlags = BindFlags.VertexBuffer,
            };
            m_VB = new Buffer(m_Device, desc);
        }

        {
            var desc = new BufferDescription
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = 64, // hold 1 matrix
                BindFlags = BindFlags.ConstantBuffer,
            };
            m_CB = new Buffer(m_Device, desc);
        }

        {
            m_VertexShader = new VertexShader(m_Device, kVertexShaderCode);
            m_PixelShader = new PixelShader(m_Device, kPixelShaderCode);
        }

        {
            var m_DX11InputElementDesc = new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
                new InputElement("COLOR", 0,   Format.R8G8B8A8_UNorm, 0),
            };
            m_InputLayout = new InputLayout(m_Device, kVertexShaderCode, m_DX11InputElementDesc);
        }

        {
            var rsdesc = new RasterizerStateDescription
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                IsDepthClipEnabled = true,
            };
            m_RasterState = new RasterizerState(m_Device, rsdesc);
        }

        {
            var dsdesc = new DepthStencilStateDescription
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = GetUsesReverseZ() ? Comparison.GreaterEqual : Comparison.LessEqual,
            };
            m_DepthState = new DepthStencilState(m_Device, dsdesc);
        }

        {
            var bdesc = new BlendStateDescription();
            bdesc.RenderTarget[0].IsBlendEnabled = false;
            bdesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            m_BlendState = new BlendState(m_Device, bdesc);
        }
    }

    #region Task
    void DrawColoredTriangle()
    {
        var verts = new MyVertex[3]
        {
            new MyVertex( -0.5f, -0.25f,  0, 0xFFff0000 ),
            new MyVertex( 0.5f, -0.25f,  0, 0xFF00ff00 ),
            new MyVertex( 0,     0.5f ,  0, 0xFF0000ff ),
        };

        // Transformation matrix: rotate around Z axis based on time.
        float phi = m_Time; // time set externally from Unity script
        float cosPhi = UnityEngine.Mathf.Cos(phi);
        float sinPhi = UnityEngine.Mathf.Sin(phi);
        float depth = 0.7f;
        float finalDepth = false ? 1.0f - depth : depth;
        var worldMatrix = new float[16]{
            cosPhi,-sinPhi,0,0,
            sinPhi,cosPhi,0,0,
            0,0,1,0,
            0,0,finalDepth,1,
        };

        DrawSimpleTriangles(worldMatrix, 1, verts);
    }

    void ModifyTexturePixels()
    {
        var textureHandle = m_TextureHandle;
        int width = m_TextureWidth;
        int height = m_TextureHeight;
        if (textureHandle == System.IntPtr.Zero)
        {
            return;
        }

        int textureRowPitch;
        var dst = BeginModifyTexture(textureHandle, width, height, out textureRowPitch);
        if (dst == null)
        {
            return;
        }

        float t = m_Time * 4.0f;
        var line = 0;
        for (int y = 0; y < height; ++y)
        {
            var ptr = line;
            for (int x = 0; x < width; ++x)
            {
                // Simple "plasma effect": several combined sine waves
                var vv = (int)(
                    (127.0f + (127.0f * UnityEngine.Mathf.Sin(x / 7.0f + t))) +
                    (127.0f + (127.0f * UnityEngine.Mathf.Sin(y / 5.0f - t))) +
                    (127.0f + (127.0f * UnityEngine.Mathf.Sin((x + y) / 6.0f - t))) +
                    (127.0f + (127.0f * UnityEngine.Mathf.Sin(UnityEngine.Mathf.Sqrt(x * x + y * y) / 4.0f - t)))
                    ) / 4;

                // Write the texture pixel
                dst[ptr++] = (byte)vv;
                dst[ptr++] = (byte)vv;
                dst[ptr++] = (byte)vv;
                dst[ptr++] = (byte)vv;
            }

            // To next image row
            line += textureRowPitch;
        }

        EndModifyTexture(textureHandle, width, height, textureRowPitch, dst);
    }

    void ModifyVertexBuffer()
    {
        var bufferHandle = m_VertexBufferHandle;
        int vertexCount = m_VertexBufferVertexCount;
        if (bufferHandle == System.IntPtr.Zero)
        {
            return;
        }

        float t = m_Time * 3.0f;

        System.Func<int, byte[]> getPadding = bufferSize =>
         {
             var stride = (int)(bufferSize / vertexCount);
             return new byte[stride - (3 + 3 + 2) * 4];
         };

        System.Action<SharpDX.DataStream, byte[]> callback = (stream, padding) =>
         {
             // modify vertex Y position with several scrolling sine waves,
             // copy the rest of the source data unmodified
             for (int i = 0; i < vertexCount; ++i)
             {
                 var src = m_VertexSource[i];
                 stream.Write(new SharpDX.Vector3(src.pos.X
                     , src.pos.Y + UnityEngine.Mathf.Sin(src.pos[0] * 1.1f + t) * 0.4f + UnityEngine.Mathf.Sin(src.pos[2] * 0.9f - t) * 0.3f
                     , src.pos.Z));
                 stream.Write(src.normal);
                 stream.Write(src.uv);
                 stream.Write(padding, 0, padding.Length);
             }
         };

        BeginModifyVertexBuffer(bufferHandle
            , getPadding
            , callback);
    }
    #endregion

    public void OnRenderEvent(int eventID)
    {
        try
        {
            DrawColoredTriangle();
            ModifyTexturePixels();
            ModifyVertexBuffer();
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine(ex);
        }
    }

    #region IDisposable Support
    void SAFE_RELEASE<T>(ref T obj)
        where T : SharpDX.ComObject
    {
        if (obj != null)
        {
            obj.Dispose();
            obj = null;
        }
    }

    private void ReleaseResources()
    {
        SAFE_RELEASE(ref m_VB);
        SAFE_RELEASE(ref m_CB);
        SAFE_RELEASE(ref m_VertexShader);
        SAFE_RELEASE(ref m_PixelShader);
        SAFE_RELEASE(ref m_InputLayout);
        SAFE_RELEASE(ref m_RasterState);
        SAFE_RELEASE(ref m_BlendState);
        SAFE_RELEASE(ref m_DepthState);
    }

    private bool disposedValue = false; // 重複する呼び出しを検出するには

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
            }

            // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
            // TODO: 大きなフィールドを null に設定します。
            ReleaseResources();
            disposedValue = true;
        }
    }

    // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
    // ~RenderAPI_D3D11() {
    //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
    //   Dispose(false);
    // }

    // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        Dispose(true);
        // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
        // GC.SuppressFinalize(this);
    }
    #endregion
}
