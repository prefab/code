using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace SavedVideoInterpreter
{
    public class ConsoleText : DependencyObject
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ConsoleText));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ConsoleText()
        {
            Writer writer = new Writer(this);
            Console.SetOut(writer);
        }

        private class Writer : StringWriter
        {
            private ConsoleText _text;
            private SetTextDel _setText;
            public Writer(ConsoleText text){
                _text = text;
                _setText = new SetTextDel(SetText);
            }

            public override void WriteLine(string str)
            {

                _text.Dispatcher.BeginInvoke(_setText, str);
            }

            private delegate void SetTextDel(string str);

            private void SetText(string str)
            {
                _text.Text += str + "\n";
            }
        }
    }
}
