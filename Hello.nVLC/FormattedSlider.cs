using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MediaPlayer
{
    /// <summary>
    ///     A Slider which provides a way to modify the
    ///     auto tooltip text by using a format string.
    /// </summary>
    public class FormattedSlider : Slider
    {
        public static readonly DependencyProperty AutoToolTipProperty = DependencyProperty.Register(
            "AutoToolTip", typeof (object), typeof (FormattedSlider), new PropertyMetadata(default(object)));

        private ToolTip _autoToolTip;

        public FormattedSlider()
        {
            _autoToolTip = GetAutoToolTip(this);
        }

        public object AutoToolTip
        {
            get { return GetValue(AutoToolTipProperty); }
            set { SetValue(AutoToolTipProperty, value); }
        }

        /// <summary>
        ///     Gets/sets a format string used to modify the auto tooltip's content.
        ///     Note: This format string must contain exactly one placeholder value,
        ///     which is used to hold the tooltip's original content.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string AutoToolTipFormat { get; set; }

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            FormatAutoToolTipContent();
        }

        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            FormatAutoToolTipContent();
        }

        private void FormatAutoToolTipContent()
        {
            if (_autoToolTip == null)
                _autoToolTip = GetAutoToolTip(this);
            if (_autoToolTip == null) return;
            _autoToolTip.Content = string.IsNullOrEmpty(AutoToolTipFormat)
                ? AutoToolTip
                : string.Format(CultureInfo.CurrentCulture, AutoToolTipFormat, Value);
        }

        private static ToolTip GetAutoToolTip(Slider slider)
        {
            var field = typeof (Slider).GetField("_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable InvocationIsSkipped
            // ReSharper disable PossibleNullReferenceException
            Debug.Assert(field != null, "No such field: _autoToolTip");
            return (ToolTip) field.GetValue(slider);
        }
    }
}