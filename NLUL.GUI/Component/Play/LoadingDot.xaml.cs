/*
 * TheNexusAvenger
 *
 * Dot used for a loading bar.
 */

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace NLUL.GUI.Component.Play
{
    public class LoadingDot : Border
    {
        public SolidColorBrush StartColor
        {
            get { return GetValue(StartColorProperty); }
            set { SetValue(StartColorProperty,value); }
        }
        public static readonly StyledProperty<SolidColorBrush> StartColorProperty = AvaloniaProperty.Register<Window,SolidColorBrush>(nameof(StartColor),new SolidColorBrush(new Color(255,141,141,145)));
        
        public SolidColorBrush EndColor
        {
            get { return GetValue(EndColorProperty); }
            set { SetValue(EndColorProperty,value); }
        }
        public static readonly StyledProperty<SolidColorBrush> EndColorProperty = AvaloniaProperty.Register<Window,SolidColorBrush>(nameof(EndColor),new SolidColorBrush(new Color(255,255,255,255)));
        
        public Thickness AdditionalMargin
        {
            get { return GetValue(AdditionalMarginProperty); }
            set { SetValue(AdditionalMarginProperty,value); }
        }
        public static readonly StyledProperty<Thickness> AdditionalMarginProperty = AvaloniaProperty.Register<Window,Thickness>(nameof(AdditionalMargin),new Thickness(3,0,2,0));
        
        public int StartSize
        {
            get { return GetValue(StartSizeProperty); }
            set { SetValue(StartSizeProperty,value); }
        }
        public static readonly StyledProperty<int> StartSizeProperty = AvaloniaProperty.Register<Window,int>(nameof(StartSize),8);
        
        public int EndSize
        {
            get { return GetValue(EndSizeProperty); }
            set { SetValue(EndSizeProperty,value); }
        }
        public static readonly StyledProperty<int> EndSizeProperty = AvaloniaProperty.Register<Window,int>(nameof(EndSize),20);
        
        public double FillPercent
        {
            get { return GetValue(FillPercentProperty); }
            set { SetValue(FillPercentProperty,value); }
        }
        public static readonly StyledProperty<double> FillPercentProperty = AvaloniaProperty.Register<Window,double>(nameof(FillPercent),0);
        
        /*
         * Creates a loading dot.
         */
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
        
        /*
         * Updates the dot.
         */
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