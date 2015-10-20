using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AviFile;
using Prefab;
using PrefabSingle;

namespace SavedVideoInterpreter
{
    public class VideoFrames
    {
        private AviManager _aviManager;
        private VideoStream _aviStream;
        private System.Drawing.Bitmap _singleFrame;
        private List<System.Drawing.Bitmap> _multipleScreenshots;

        private Dictionary<string, List<string>> _annotationImages;

        private Mode _mode;



        private enum Mode
        {
            SingleFrame,
            Video,
            Annotations,
            MultipleFrames,
            None
        }

        public static VideoFrames FromMultipleScreenshots(List<System.Drawing.Bitmap> screenshots)
        {
            VideoFrames frames = new VideoFrames();
            frames._mode = Mode.MultipleFrames;
            frames._multipleScreenshots = new List<System.Drawing.Bitmap>(screenshots);

            return frames;
        }
        public static VideoFrames FromAnnotations(LayerInterpretationLogic logic)
        {
            VideoFrames frames = new VideoFrames();
            frames._mode = Mode.Annotations;

            frames._annotationImages = GetAllImageIdsUsingAllLibrariesFromLayers(logic);

            return frames;
        }


        private static Dictionary<string, List<string>> GetAllImageIdsUsingAllLibrariesFromLayers(LayerInterpretationLogic logic)
        {
            var images = new Dictionary<string, List<string>>();

            
            var libs = AnnotationLibrary.GetAnnotationLibraries(logic.Layers);
            foreach (string lib in libs)
            {

                List<string> forlib = new List<string>(AnnotationLibrary.GetAllImageIds(lib));
              
                images[lib] = forlib;

            }

            return images;
        }

        private VideoFrames() { }
        private string fileloc;

        public VideoFrames(string fileLocation)
        {
            if (fileLocation == null)
            {
                _mode = Mode.None;
                return;
            }

            string lower = fileLocation.ToLower();
            fileloc = fileLocation;
            if (lower.EndsWith(".avi"))
            {
                _aviManager = new AviManager(fileLocation, true);
                _aviStream = _aviManager.GetVideoStream();
                _aviStream.GetFrameOpen();
                _mode = Mode.Video;
            }
            else if (lower.EndsWith(".png"))
            {
                _singleFrame = new System.Drawing.Bitmap(fileLocation);
                _mode = Mode.SingleFrame;
            }
            else
            {
                _mode = Mode.None;
            }
        }

        public int GetFrameCount()
        {
            switch (_mode)
            {
                case Mode.SingleFrame:
                    return 1;


                case Mode.Video:
                    return _aviStream.CountFrames;


                case Mode.Annotations:
                    int count = 0;
                    foreach (var value in _annotationImages.Values)
                        count += value.Count;
                    return count;

                case Mode.MultipleFrames:
                    return _multipleScreenshots.Count;
                    
                default:
                    return 0;
            }
        }




        public System.Drawing.Bitmap GetBitmap(int index)
        {
            if (index < 0 || index >= GetFrameCount())
            {
                return null;
            }

            switch (_mode)
            {
                case Mode.Video:
                    //_aviStream.GetFrameOpen();
                    System.Drawing.Bitmap bitmap;
                    bitmap = _aviStream.GetBitmap(index);
                    
                    //_aviStream.GetFrameClose();
                    return bitmap;


                case Mode.SingleFrame:
                    return _singleFrame.Clone() as System.Drawing.Bitmap;


                case Mode.Annotations:
                    Bitmap bmp = GetImageFromAnnotation(index);
                    return Bitmap.ToSystemDrawingBitmap(bmp);


                case Mode.MultipleFrames:
                    return new System.Drawing.Bitmap(_multipleScreenshots[index]);
                    
                default:

                    return null;
            }

        }

        public Bitmap GetImageFromAnnotation(int index)
        {
            int count = 0;
            
            foreach (var key in _annotationImages.Keys)
            {
                var images = _annotationImages[key];

                foreach (var imageid in images)
                {
                    if (count == index)
                    {
                        return AnnotationLibrary.GetImage(key, imageid);
                    }
                    count++;
                }
            }

            return null;
        }

        public void Close()
        {
            switch (_mode)
            {
                case Mode.Video:
                    _aviManager.Close();
                    break;

                case Mode.SingleFrame:
                    _singleFrame.Dispose();
                    break;

                case Mode.MultipleFrames:
                    _multipleScreenshots = null;
                    break;
            }
        }
    }
}
