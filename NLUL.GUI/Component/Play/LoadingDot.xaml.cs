using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace NLUL.GUI.Component.Play
{
    public class LoadingDot : Border
    {
        /// <summary>
        /// Color for the initial fill of the color.
        /// </summary>
        public SolidColorBrush StartColor
        {
            get => GetValue(StartColorProperty);
            set => SetValue(StartColorProperty, value);
        }
        public static readonly StyledProperty<SolidColorBrush> StartColorProperty = AvaloniaProperty.Register<Window, SolidColorBrush>(nameof(StartColor), new SolidColorBrush(new Color(255, 141, 141, 145)));
        
        /// <summary>
        /// Color for the final fill of the color.
        /// </summary>
        public SolidColorBrush EndColor
        {
            get => GetValue(EndColorProperty);
            set => SetValue(EndColorProperty, value);
        }
        public static readonly StyledProperty<SolidColorBrush> EndColorProperty = AvaloniaProperty.Register<Window, SolidColorBrush>(nameof(EndColor), new SolidColorBrush(new Color(255, 255, 255, 255)));
        
        /// <summary>
        /// Additional margin for the dot.
        /// </summary>
        public Thickness AdditionalMargin
        {
            get => GetValue(AdditionalMarginProperty);
            set => SetValue(AdditionalMarginProperty, value);
        }
        public static readonly StyledProperty<Thickness> AdditionalMarginProperty = AvaloniaProperty.Register<Window, Thickness>(nameof(AdditionalMargin), new Thickness(3, 0, 2, 0));
        
        /// <summary>
        /// Initial size of the dot.
        /// </summary>
        public int StartSize
        {
            get => GetValue(StartSizeProperty);
            set => SetValue(StartSizeProperty,value);
        }
        public static readonly StyledProperty<int> StartSizeProperty = AvaloniaProperty.Register<Window, int>(nameof(StartSize), 8);
        
        /// <summary>
        /// Final size of the dot.
        /// </summary>
        public int EndSize
        {
            get => GetValue(EndSizeProperty);
            set => SetValue(EndSizeProperty,value);
        }
        public static readonly StyledProperty<int> EndSizeProperty = AvaloniaProperty.Register<Window, int>(nameof(EndSize), 20);
        
        /// <summary>
        /// Percent that the dot is filled.
        /// </summary>
        public double FillPercent
        {
            get => GetValue(FillPercentProperty);
            set => SetValue(FillPercentProperty,value);
        }
        public static readonly StyledProperty<double> FillPercentProperty = AvaloniaProperty.Register<Window, double>(nameof(FillPercent), 0);
        
        /// <summary>
        /// Creates a loading dot.
        /// </summary>
        public LoadingDot()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);

            // Connect the events.
            this.PropertyChanged += (sender,args) =>
            {
                if (args.Property == StartColorProperty || args.Property == EndColorProperty || args.Property == StartSizeProperty || args.Property == EndSizeProperty || args.Property == FillPercentProperty)
                {
                    this.UpdateDot();
                }
            };
            
            // Update the initial dot initially.
            this.UpdateDot();
        }
        
        /// <summary>
        /// Updates the dot.
        /// </summary>
        private void UpdateDot()
        {
            // Set the color.
            this.Background = new SolidColorBrush(new Color((byte) (this.StartColor.Color.A + (this.FillPercent * (this.EndColor.Color.A - this.StartColor.Color.A))),(byte) (this.StartColor.Color.R + (this.FillPercent * (this.EndColor.Color.R - this.StartColor.Color.R))),(byte) (this.StartColor.Color.G + (this.FillPercent * (this.EndColor.Color.G - this.StartColor.Color.G))),(byte) (this.StartColor.Color.B + (this.FillPercent * (this.EndColor.Color.B - this.StartColor.Color.B)))));

            // Set the size.
            var size = Math.Floor((this.StartSize + (this.FillPercent * (this.EndSize - this.StartSize)))/2) * 2;
            var margin = (this.EndSize - size)/2;
            this.CornerRadius = new CornerRadius(size/2);
            this.Width = size;
            this.Height = size;
            this.Margin = new Thickness(this.AdditionalMargin.Left + margin,this.AdditionalMargin.Top + margin,this.AdditionalMargin.Right + margin,this.AdditionalMargin.Bottom + margin);
        }
    }
}