using Microsoft.JSInterop;

namespace ReceiptApp.Services
{
    public class BrowserResizeService
    {
        public event EventHandler<ScreenSize> OnScreenResize;

        private readonly IJSRuntime _js;

        public BrowserResizeService(IJSRuntime js)
        {
            _js = js;
        }
        public async void Init()
        {
            await _js.InvokeAsync<string>("ReceiptApp.browserResize.registerResizeCallback", DotNetObjectReference.Create(this));
        }

        public void Resize(int width, int height)
        {
            OnScreenResize?.Invoke(this, GetScreenSize(new(width, height)));
        }

        public async Task<int> GetInnerHeight()
        {
            return await _js.InvokeAsync<int>("ReceiptApp.browserResize.getInnerHeight");
        }

        public async Task<int> GetInnerWidth()
        {
            return await _js.InvokeAsync<int>("ReceiptApp.browserResize.getInnerWidth");
        }

        public async Task<Size> GetSize()
        {
            return new(await GetInnerWidth(), await GetInnerHeight());
        }

        [JSInvokable]
        public void SetBrowserDimensions(int width, int height)
        {

            Resize(width, height);
        }

        public async Task<ScreenSize> GetScreenSize()
        {
            return GetScreenSize(await GetSize());
        }

        public static ScreenSize GetScreenSize(Size size) =>
            size switch
            {
                { Width: < 576 } => ScreenSize.XS,
                { Width: < 768 } => ScreenSize.SM,
                { Width: < 992 } => ScreenSize.MD,
                { Width: < 1200 } => ScreenSize.LG,
                { Width: < 1400 } => ScreenSize.XL,
                _ => ScreenSize.XXL,
            };


        public record Size(int Width, int Height);
    }
    public enum ScreenSize : int
    {
        XS = 576,
        SM = 768,
        MD = 992,
        LG = 1200,
        XL = 1400,
        XXL
    }
}
