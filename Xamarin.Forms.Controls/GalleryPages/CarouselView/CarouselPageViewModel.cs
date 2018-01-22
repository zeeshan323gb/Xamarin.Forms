using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.Controls
{
    public class CarouselViewModel : INotifyPropertyChanged
    {
        public CarouselViewModel()
        {
			Source = Source ?? new ObservableCollection<int>() { 1 };
        }

        ObservableCollection<int> _Source;
        /// <summary>
        /// Images
        /// </summary>
        public ObservableCollection<int> Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
                NotifyPropertyChanged();
            }
        }

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raise PropertyChanged event
		/// </summary>
		/// <param name="propertyName">nae of changed property</param>
		internal void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
    }
}
