using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Object = Java.Lang.Object;

/*
The MIT License(MIT)

Copyright(c) 2017 Alexander Reyes(alexrainman1975 @gmail.com)

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

// PR Fix for entry focus bug #242
namespace Xamarin.Forms.Platform.Android
{
    internal class GlobalLayoutListener : Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        private static InputMethodManager _inputManager;
        private readonly SoftKeyboardService _softwareKeyboardService;
        Activity _activity;

        private static void ObtainInputManager(Activity activity)
        {
            _inputManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
        }

        public GlobalLayoutListener(SoftKeyboardService softwareKeyboardService, Activity activity)
        {
            _softwareKeyboardService = softwareKeyboardService;
            _activity = activity;
            ObtainInputManager(activity);
        }

        public void OnGlobalLayout()
        {
            if (_inputManager.Handle == IntPtr.Zero)
            {
                ObtainInputManager(_activity);
            }

            _softwareKeyboardService.InvokeVisibilityChanged(new SoftKeyboardEventArgs(_inputManager.IsAcceptingText));
        }
    }
}