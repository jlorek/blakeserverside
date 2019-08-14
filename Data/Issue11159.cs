using Microsoft.JSInterop;

namespace BlakeServerSide.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class Issue11159
    {
        private IJSRuntime Js { get; set; }

        public Issue11159(IJSRuntime jsRuntime)
        {
            Js = jsRuntime;
        }

        private static object CreateDotNetObjectRefSyncObj = new object();

        public DotNetObjectRef<T> CreateDotNetObjectRef<T>(T value) where T : class
        {
            lock (CreateDotNetObjectRefSyncObj)
            {
                JSRuntime.SetCurrentJSRuntime(Js);
                return DotNetObjectRef.Create(value);
            }
        }

        public void DisposeDotNetObjectRef<T>(DotNetObjectRef<T> value) where T : class
        {
            if (value != null)
            {
                lock (CreateDotNetObjectRefSyncObj)
                {
                    JSRuntime.SetCurrentJSRuntime(Js);
                    value.Dispose();
                }
            }
        }
    }
}
