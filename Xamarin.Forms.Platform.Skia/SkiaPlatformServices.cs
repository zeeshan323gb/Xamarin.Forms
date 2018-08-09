using SkiaSharp;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Skia
{
	public class SkiaPlatformServices : IPlatformServices
	{
		public bool IsInvokeRequired => false;

		public string RuntimePlatform => "Android";

		public void BeginInvokeOnMainThread(Action action)
		{
			action();
		}

		public Ticker CreateTicker()
		{
			throw new NotImplementedException();
		}

		public Assembly[] GetAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		static readonly MD5CryptoServiceProvider s_checksum = new MD5CryptoServiceProvider();

		public string GetMD5Hash(string input)
		{
			var bytes = s_checksum.ComputeHash(Encoding.UTF8.GetBytes(input));
			var ret = new char[32];
			for (var i = 0; i < 16; i++)
			{
				ret[i * 2] = (char)Hex(bytes[i] >> 4);
				ret[i * 2 + 1] = (char)Hex(bytes[i] & 0xf);
			}
			return new string(ret);
		}

		static int Hex(int v)
		{
			if (v < 10)
				return '0' + v;
			return 'a' + v - 10;
		}

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			switch (size)
			{
				case NamedSize.Default:
					return 17;
				case NamedSize.Micro:
					return 12;
				case NamedSize.Small:
					return 14;
				case NamedSize.Medium:
					return 17;
				case NamedSize.Large:
					return 22;
				default:
					throw new ArgumentOutOfRangeException("size");
			}
		}

		HttpClient GetHttpClient()
		{
			var handler = new HttpClientHandler();
			return new HttpClient(handler);
		}

		public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			using (var client = GetHttpClient())
			using (var response = await client.GetAsync(uri, cancellationToken))
			{
				if (!response.IsSuccessStatusCode)
				{
					Log.Warning("HTTP Request", $"Could not retrieve {uri}, status code {response.StatusCode}");
					return null;
				}
				return await response.Content.ReadAsStreamAsync();
			}
		}

		public IIsolatedStorageFile GetUserStoreForApplication()
		{
			return new _IsolatedStorageFile(IsolatedStorageFile.GetUserStoreForAssembly());
		}

		public void OpenUriAction(Uri uri)
		{
			
		}

		public void QuitApplication()
		{
			
		}

		public void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			throw new NotImplementedException();
		}

		public class _IsolatedStorageFile : IIsolatedStorageFile
		{
			readonly IsolatedStorageFile _isolatedStorageFile;

			public _IsolatedStorageFile(IsolatedStorageFile isolatedStorageFile)
			{
				_isolatedStorageFile = isolatedStorageFile;
			}

			public Task CreateDirectoryAsync(string path)
			{
				_isolatedStorageFile.CreateDirectory(path);
				return Task.FromResult(true);
			}

			public Task<bool> GetDirectoryExistsAsync(string path)
			{
				return Task.FromResult(_isolatedStorageFile.DirectoryExists(path));
			}

			public Task<bool> GetFileExistsAsync(string path)
			{
				return Task.FromResult(_isolatedStorageFile.FileExists(path));
			}

			public Task<DateTimeOffset> GetLastWriteTimeAsync(string path)
			{
				return Task.FromResult(_isolatedStorageFile.GetLastWriteTime(path));
			}

			public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access)
			{
				Stream stream = _isolatedStorageFile.OpenFile(path, mode, access);
				return Task.FromResult(stream);
			}

			public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access, FileShare share)
			{
				Stream stream = _isolatedStorageFile.OpenFile(path, mode, access, share);
				return Task.FromResult(stream);
			}
		}
	}
}
