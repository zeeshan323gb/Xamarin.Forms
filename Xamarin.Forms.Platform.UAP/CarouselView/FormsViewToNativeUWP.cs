using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

/*
The MIT License(MIT)

Copyright(c) 2016 alexrainman1975@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
IN THE SOFTWARE.
 */

namespace Xamarin.Forms.Platform.UWP
{
    public static class FormsViewToNativeUWP
    {
        public static FrameworkElement ConvertFormsToNative(Xamarin.Forms.View view, Rectangle size)
        {
            //var vRenderer = RendererFactory.GetRenderer (view);

            if (Platform.GetRenderer(view) == null)
                Platform.SetRenderer(view, Platform.CreateRenderer(view));

            var vRenderer = Platform.GetRenderer(view);

            view.Layout(new Rectangle(0, 0, size.Width, size.Height));

            //vRenderer.ContainerElement.Arrange(new Rect(0, 0, size.Width, size.Height));

            return vRenderer.ContainerElement;
        }
    }
}
