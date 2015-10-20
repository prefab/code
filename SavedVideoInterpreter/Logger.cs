using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SavedVideoInterpreter
{
    public class Logger
    {

        private Queue<string> _lines;
        private string _fileLocation;
        private AutoResetEvent _elementAdded;
        private Thread _logThread;
        private bool _running;
        private StreamWriter _writer;

        

        public Logger(string location)
        {
            _fileLocation = location;
            _lines = new Queue<string>();
            _logThread = new Thread(Run);
            _running = false;
            _elementAdded = new AutoResetEvent(false);
            _writer = new StreamWriter(_fileLocation);
        }


        public void AddLine(string line)
        {
         
            lock (((ICollection)_lines).SyncRoot)
            {
                _lines.Enqueue(line);
                _elementAdded.Set();
            }
        }

        public void Start()
        {
            _running = true;
            _logThread.Start();
        }

        public void Stop()
        {
            lock (((ICollection)_lines).SyncRoot)
            {
                _running = false;
                _elementAdded.Set();
            }
        }

        private void Run()
        {
            
            while (_running)
            {
                if (_elementAdded.WaitOne())
                {
                    lock (((ICollection)_lines).SyncRoot)
                    {
                        while (_lines.Count > 0)
                        {
                            string line = _lines.Dequeue();
                            _writer.WriteLine(line);
                        }
                    }
                }
            }

            _writer.Close();
        }

        

        
    }
}
