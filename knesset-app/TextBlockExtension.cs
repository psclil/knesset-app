﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace knesset_app
{
    public static class TextBlockExtension
    {
        public static Inline GetFormattedText(DependencyObject obj)
        {
            return (Inline)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, Inline value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached(
                "FormattedText",
                typeof(Inline),
                typeof(TextBlockExtension),
                new PropertyMetadata(null, OnFormattedTextChanged));

        private static void OnFormattedTextChanged(
            DependencyObject o,
            DependencyPropertyChangedEventArgs e)
        {
            var textBlock = o as TextBlock;
            if (textBlock == null) return;

            var inline = (Inline)e.NewValue;
            textBlock.Inlines.Clear();
            if (inline != null)
            {
                textBlock.Inlines.Add(inline);
            }
        }

    }
}
