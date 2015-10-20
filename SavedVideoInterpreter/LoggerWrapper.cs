using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavedVideoInterpreter
{
    public class InteractionLogger
    {
        private string _userid;
        private string _condition;
        private Logger _logger;
        private static readonly string _header = "timestamp(utc),participantId,condition,interaction";
        private static readonly string _logDirectory = @"../../../logs";


        public enum InteractionType
        {
            LayersEdited,
            FrameChanged,
            Annotation,
            TreeBrowserClick,
            RectOverlayClick,
            Selector,
            PtypeBrowser,
            RuntimeStorage
        }

        public InteractionLogger(string studyCondition, string participantId)
        {
            string filename = "log-" + studyCondition + "-" + participantId 
                + "-" + DateTime.Now.ToFileTimeUtc().ToString() + ".csv";

            _logger = new Logger(_logDirectory + "/" + filename);
            _userid = participantId;
            _condition = studyCondition;
        }

        public void Add(InteractionType type, string extradata)
        {
            string line = DateTime.Now.ToFileTimeUtc().ToString() + "," + _userid + "," 
                          + _condition + "," + type.ToString() + "," + extradata;

            _logger.AddLine(line);
        }

        public void Start()
        {
            _logger.Start();
            _logger.AddLine(_header);
        }

        public void Stop()
        {
            _logger.Stop();
        }


    }
}
