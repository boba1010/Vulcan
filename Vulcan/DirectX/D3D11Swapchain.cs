using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Vulcan.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Windowing;
using Vulcan.Graphics.Descriptions;
using Viewport = Silk.NET.Direct3D11.Viewport;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Swapchain : ISwapchain
{
    private IDXGISwapChain1* _swapChain;
    private ID3D11Texture2D* _backBuffer;
    private ID3D11RenderTargetView* _renderTargetView;
    public ID3D11RenderTargetView* RenderTargetView => _renderTargetView;
    private Viewport _viewport;

    internal D3D11Swapchain(
        ID3D11Device* device,
        ComPtr<IDXGIFactory2> factory,
        IWindow window,
        in SwapchainDescription description)
    {
        var swapChainDescription = new SwapChainDesc1
        {
            Width = (uint)window.Size.X,
            Height = (uint)window.Size.Y,
            Format = Format.FormatB8G8R8A8Unorm,
            BufferCount = 2,
            BufferUsage = 0x20,
            SampleDesc = new SampleDesc
            {
                Count = 1,
                Quality = 0
            },
            SwapEffect = SwapEffect.FlipDiscard
        };

        fixed (IDXGISwapChain1** swapChain = &_swapChain)
        {
            factory.CreateSwapChainForHwnd(
                (IUnknown*)device,
                window.Native!.DXHandle!.Value,
                &swapChainDescription,
                (SwapChainFullscreenDesc*)null,
                (IDXGIOutput*)null,
                swapChain);
        }

        fixed (ID3D11Texture2D** backBuffer = &_backBuffer)
        {
            _swapChain->GetBuffer(
                0,
                SilkMarshal.GuidPtrOf<ID3D11Texture2D>(),
                (void**)backBuffer);
        }

        fixed (ID3D11RenderTargetView** renderTargetView = &_renderTargetView)
        {
            device->CreateRenderTargetView(
                (ID3D11Resource*)_backBuffer,
                null,
                renderTargetView);
        }

        _viewport = new Viewport
        {
            TopLeftX = 0,
            TopLeftY = 0,
            Width = window.Size.X,
            Height = window.Size.Y,
            MinDepth = 0,
            MaxDepth = 1
        };
    }

    public void Present()
    {
        _swapChain->Present(1, 0);
    }

    public void Dispose()
    {
        if (_renderTargetView is not null)
        {
            _renderTargetView->Release();
            _renderTargetView = null;
        }

        if (_backBuffer is not null)
        {
            _backBuffer->Release();
            _backBuffer = null;
        }

        if (_swapChain is not null)
        {
            _swapChain->Release();
            _swapChain = null;
        }
    }
}
