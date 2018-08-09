using System;
using Xamarin.Forms.Xaml;
namespace  Xamarin.Forms.Previewer
{
	public class XamlParser
	{
		public static string XamlPlaybackScreen = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ContentPage Title=""AFSPELEN VAN AFSPEELLIJST"" BackgroundColor=""#181818""
    xmlns=""http://xamarin.com/schemas/2014/forms"" 
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
    x:Class=""KickassUI.Spotify.Pages.PlayerPage"">
    <ContentPage.ToolbarItems>
        <ToolbarItem Icon=""icon_chevron_down"" Command=""{Binding ClosePlayerCommand}"" />
        <ToolbarItem Icon=""icon_playlist"" />
    </ContentPage.ToolbarItems>
	<ContentPage.Content>
        <ScrollView Orientation=""Vertical"">
            <Grid>
                <AbsoluteLayout>
                    <Image AbsoluteLayout.LayoutBounds=""0,0,1,1"" AbsoluteLayout.LayoutFlags=""All"" InputTransparent=""false"" x:Name=""artwork"" 
                    HorizontalOptions=""FillAndExpand"" Aspect=""Fill"" VerticalOptions=""FillAndExpand"" Source=""{Binding Song.AlbumImageUrl}"" />
                </AbsoluteLayout>
                <StackLayout HorizontalOptions=""FillAndExpand""> 
                    <StackLayout.Padding>
                        <OnPlatform x:TypeArguments=""Thickness"">
                            <On Platform=""Android"" Value=""30,10,30,0""/>
                            <On Platform=""iOS"" Value=""30,70,30,0""/>
                        </OnPlatform>
                    </StackLayout.Padding>
                    <StackLayout Orientation=""Horizontal"" HorizontalOptions=""FillAndExpand"" HeightRequest=""60"">
                        <StackLayout.IsVisible>
                            <OnPlatform x:TypeArguments=""x:Boolean"">
                                <On Platform=""Android"" Value=""true"" />
                                <On Platform=""iOS"" Value=""false"" />
                            </OnPlatform>
                        </StackLayout.IsVisible>
                        <Image Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/icon_chevron_down%402x.png"" VerticalOptions=""Center"" HorizontalOptions=""Start"">
                            <Image.GestureRecognizers>
                                <TapGestureRecognizer Command=""{Binding ClosePlayerCommand}""/>
                            </Image.GestureRecognizers>
                        </Image>
                        <StackLayout VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                            <Label Text=""NOW PLAYING FROM PLAYLIST"" TextColor=""#FFF"" FontSize=""12"" HorizontalTextAlignment=""Center"" HorizontalOptions=""Fill"" />
                            <Label Text=""Kickass Tunes"" Margin=""0,-5,0,0"" TextColor=""#FFF"" FontSize=""12"" HorizontalTextAlignment=""Center"" HorizontalOptions=""Fill"" />
                        </StackLayout>
                        <Image Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/icon_playlist%402x.png"" VerticalOptions=""Center"" HorizontalOptions=""End"" />
                    </StackLayout>
                    <Image Source=""https://i.scdn.co/image/08d56eac0c7d48bb8bf7752b2202c3314db79394"" VerticalOptions=""Center"">
                        <Image.HeightRequest>
                            <OnPlatform x:TypeArguments=""x:Double"">
                                <On Platform=""Android"" Value=""300"" />
                                <On Platform=""iOS"" Value=""325"" />
                            </OnPlatform>
                        </Image.HeightRequest>
                    </Image>
                    <Grid Margin=""0,20,0,0"">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""Auto"" />
                            <ColumnDefinition Width=""*"" />
                            <ColumnDefinition Width=""Auto"" />
                        </Grid.ColumnDefinitions>
                        <Image Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/icon_plus%402x.png"" Grid.Column=""0"" HorizontalOptions=""Center"" />
                        <StackLayout Grid.Column=""1"" HorizontalOptions=""Center"">
                            <Label Text=""{Binding Song.Title}"" HorizontalOptions=""FillAndExpand"" HorizontalTextAlignment=""Center"" FontSize=""18"" TextColor=""White"" />
                            <Label Margin=""0,-5,0,0"" Text=""{Binding Song.Artist}"" HorizontalOptions=""FillAndExpand"" HorizontalTextAlignment=""Center"" FontSize=""14"" TextColor=""#adaeb2"" />
                        </StackLayout>
                        <Image Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/icon_ellipsis%402x.png"" Grid.Column=""2"" HorizontalOptions=""Center"" />
                    </Grid>
                    <ContentView>
                        <ContentView.IsVisible>
                            <OnPlatform x:TypeArguments=""x:Boolean"">
                                <On Platform=""Android"" Value=""false""/>
                                <On Platform=""iOS"" Value=""true""/>
                            </OnPlatform>
                        </ContentView.IsVisible>
                        <StackLayout Margin=""0,0,0,0"" Orientation=""Horizontal"">
                            <Label Text=""{Binding Ticks}"" TextColor=""#adaeb2"" FontSize=""10"" HorizontalOptions=""StartAndExpand"" />
                            <Label Text=""{Binding TicksLeft}"" TextColor=""#adaeb2"" FontSize=""10"" HorizontalOptions=""End"" />
                        </StackLayout>
                        <Slider HeightRequest=""4"" Minimum=""0"" Maximum=""{Binding Song.LengthInSeconds}"" Value=""{Binding Ticks, Mode=TwoWay}"">
                        </Slider>
                    </ContentView>
                     <StackLayout Margin=""0,0,0,0"" Orientation=""Horizontal"">
                        <StackLayout.IsVisible>
                            <OnPlatform x:TypeArguments=""x:Boolean"">
                                <On Platform=""Android"" Value=""true""/>
                                <On Platform=""iOS"" Value=""false""/>
                            </OnPlatform>
                        </StackLayout.IsVisible>
                        <Label Text=""{Binding Ticks}"" Margin=""0,0,5,0"" TextColor=""#adaeb2"" FontSize=""10"" HorizontalOptions=""Start"" />
                        <Slider HorizontalOptions=""FillAndExpand"" VerticalOptions=""Center"" HeightRequest=""4"" Minimum=""0"" Maximum=""{Binding Song.LengthInSeconds}"" Value=""{Binding Ticks, Mode=TwoWay}"">
                        </Slider>
                        <Label Text=""{Binding TicksLeft}"" Margin=""5,0,0,0"" TextColor=""#adaeb2"" FontSize=""10"" HorizontalOptions=""End"" />
                    </StackLayout>
                    <StackLayout Orientation=""Horizontal"" HorizontalOptions=""Center"">
                        <Image VerticalOptions=""Center"" Margin=""0,0,30,0"" Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/button_shuffle%402x.png"" />
                        <Image VerticalOptions=""Center"" Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/button_back%402x.png"" />
                        <Image VerticalOptions=""Center"" Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/button_play%402x.png"">
                            <Image.GestureRecognizers>
                                <TapGestureRecognizer  Command=""{Binding PlayCommand}""/>
                            </Image.GestureRecognizers>
                            <Image.Triggers>
                                <DataTrigger TargetType=""Image"" Binding=""{Binding IsPlaying}"" Value=""true"">
                                    <Setter Property=""Source"" Value=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/button_pause%402x.png"" />
                                </DataTrigger>
                                <DataTrigger TargetType=""Image"" Binding=""{Binding IsPlaying}"" Value=""false"">
                                    <Setter Property=""Source"" Value=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/button_play%402x.png"" />
                                </DataTrigger>
                            </Image.Triggers>
                        </Image>
                        <Image VerticalOptions=""Center"" Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/button_forward%402x.png"" />
                        <Image VerticalOptions=""Center"" Margin=""30,0,0,0"" Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/button_repeat%402x.png"" />
                    </StackLayout>
                    <StackLayout Orientation=""Horizontal"" HorizontalOptions=""Center"">
                        <StackLayout.Margin>
                            <OnPlatform x:TypeArguments=""Thickness"">
                                <On Platform=""Android"" Value=""0,0,0,0""/>
                                <On Platform=""iOS"" Value=""0,10,0,0""/>
                            </OnPlatform>
                        </StackLayout.Margin>
                        <Image Source=""https://github.com/sthewissen/KickassUI.Spotify/raw/master/src/iOS/Resources/icon_devices%402x.png"" WidthRequest=""20"" />
                        <Label TextColor=""White"" HorizontalTextAlignment=""Center"" Margin=""-5,0,0,0"" FontSize=""11"" Text=""Devices Available"" />
                    </StackLayout>
                </StackLayout>
            </Grid>
        </ScrollView>
	</ContentPage.Content>
</ContentPage>";
		public static string XamlSimpleString = @"<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""XamlSamples.GridDemoPage""
             BackgroundColor=""Blue""
             Title=""Grid Demo Page"">
	<Label Text=""Hello, XAML!""
       VerticalOptions=""Center""
       FontAttributes=""Bold""
       FontSize=""Large""
       TextColor=""Aqua"" />
</ContentPage>";

		public static string XamlComplexSampleString = @"<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""XamlSamples.GridDemoPage""
             Title=""Grid Demo Page"">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height=""Auto"" />
            <RowDefinition Height=""*"" />
            <RowDefinition Height=""100"" />
        </Grid.RowDefinitions>

        <Grid.ColumnDefinitions>
            <ColumnDefinition Width=""Auto"" />
            <ColumnDefinition Width=""*"" />
            <ColumnDefinition Width=""100"" />
        </Grid.ColumnDefinitions>

        <Label Text=""Autosized cell""
               Grid.Row=""0"" Grid.Column=""0""
               TextColor=""White""
               BackgroundColor=""Blue"" />

        <BoxView Color=""Silver""
                 HeightRequest=""0""
                 Grid.Row=""0"" Grid.Column=""1"" />

        <BoxView Color=""Teal""
                 Grid.Row=""1"" Grid.Column=""0"" />

        <Label Text=""Leftover space""
               Grid.Row=""1"" Grid.Column=""1""
               TextColor=""Purple""
               BackgroundColor=""Aqua""
               HorizontalTextAlignment=""Center""
               VerticalTextAlignment=""Center"" />

        <Label Text=""Span two rows (or more if you want)""
               Grid.Row=""0"" Grid.Column=""2"" Grid.RowSpan=""2""
               TextColor=""Yellow""
               BackgroundColor=""Blue""
               HorizontalTextAlignment=""Center""
               VerticalTextAlignment=""Center"" />

        <Label Text=""Span two columns""
               Grid.Row=""2"" Grid.Column=""0"" Grid.ColumnSpan=""2""
               TextColor=""Blue""
               BackgroundColor=""Yellow""
               HorizontalTextAlignment=""Center""
               VerticalTextAlignment=""Center"" />

        <Label Text=""Fixed 100x100""
               Grid.Row=""2"" Grid.Column=""2""
               TextColor=""Aqua""
               BackgroundColor=""Red""
               HorizontalTextAlignment=""Center""
               VerticalTextAlignment=""Center"" />

    </Grid>
</ContentPage>";

		public static XamlSample[] Samples = {
			new XamlSample("Simple",XamlSimpleString),
			new XamlSample("Complex", XamlComplexSampleString),
			new XamlSample("Playback Screen", XamlPlaybackScreen)
		};
		public class XamlSample
		{
			public XamlSample()
			{

			}
			public XamlSample(string description, string xaml)
			{
				Description = description;
				Xaml = xaml;
			}
			public string Description { get; set; }
			public string Xaml { get; set; }
		}

		public static (Element element, Exception error) ParseXaml(string xaml)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(xaml))
					return (null, null);
				//TODO: Determine what type it is.
				return (new ContentPage().LoadFromXaml(xaml),null);
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				//Report back xaml errors
				return (null, e);
			}
		}
	}
}
