using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using AviFile;

using System.ComponentModel;
using Prefab;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using System.IO;
using LitJson;
using AC.AvalonControlsLibrary;
using Newtonsoft.Json.Linq;
using PrefabIdentificationLayers.Prototypes;
using PrefabIdentificationLayers.Features;
using System.Collections.ObjectModel;
using System.Threading;
using PrefabIdentificationLayers.Models;
using PrefabSingle;
using PrefabUtils;
using System.Reflection;

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {


        private static readonly string TargetLayersDir = "..\\..\\..\\layers";
        private static readonly string SingleLayersDir = "..\\..\\..\\single";

        public static DependencyProperty ImageProperty =
            DependencyProperty.Register("CapturedWindow", typeof(WriteableBitmap), typeof(EditorWindow));
        public static DependencyProperty DragRectProperty =
            DependencyProperty.Register("DragRect", typeof(Rect), typeof(EditorWindow));
        public static DependencyProperty SelectedNodesProperty =
            DependencyProperty.Register("SelectedNodes", typeof(BindingList<PropertiesControl>), typeof(EditorWindow));
        public static DependencyProperty CurrentPixelProperty =
            DependencyProperty.Register("CurrentPixelValue", typeof(string), typeof(EditorWindow));
        public static DependencyProperty ErrorTextProperty =
            DependencyProperty.Register("ErrorText", typeof(string), typeof(EditorWindow));
        public static DependencyProperty ConsoleOutputTextProperty =
       DependencyProperty.Register("ConsoleOutputText", typeof(string), typeof(EditorWindow));
        public static DependencyProperty AnnotationLibrariesProperty =
            DependencyProperty.Register("AnnotationLibraries", typeof(BindingList<AnnotationLibMenuItem>), typeof(EditorWindow));
        public static DependencyProperty SelectedFrameIndexProperty =
            DependencyProperty.Register("SelectedFrameIndex", typeof(int), typeof(EditorWindow));
        private delegate void UpdateDel(UpdateType updateType, object arg);

        
        private UpdateDel _updateThread;
        private VideoInterpreter _interpreter;
        private State _currState;
        private int _pinX, _pinY;
        private ProgressBarWindow _progressBarWindow;
        private HashSet<TextEditor> _invalidated;
       

        private Tree _currTree;
        private PrototypeBrowserWindow _prototypeBrowserWindow;
        private RuntimeStorageBrowser _runtimeStorageWindow;

        private Stack<AnnotationOperation> _annotationUndoStack;
        private PrefabInterpretationLogic _prefabInterpretationLogic;



        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        //public static void Main() {
        //    SavedVideoInterpreter.App app = new SavedVideoInterpreter.App();

        //    app.InitializeComponent();
        //    app.Run();
        //}
        public static void Main()
        {

            AppDomain currentDomain = AppDomain.CurrentDomain;

            SavedVideoInterpreter.App app = new SavedVideoInterpreter.App();

            app.InitializeComponent();
            app.Run();


        }





        private enum StudyCondition
        {
            Layers,
            Single,
            Debug
        }

        private StudyCondition _studyCondition;

        private string _participantId;
        private InteractionLogger _logger;
        private Bitmap _currentBitmapImage;
        private string _currentFrameImageHash;


        private class AnnotationOperation
        {
            public string AnnotationId
            {
                get;
                private set;
            }

            public AnnotationOperation(string id)
            {
   
                AnnotationId = id;
            }
        }


        public class AnnotationLibMenuItem : DependencyObject
        {
            public static DependencyProperty NameProperty =
                DependencyProperty.Register("Name", typeof(string), typeof(AnnotationLibMenuItem));
            public static DependencyProperty IsCheckedProperty =
                DependencyProperty.Register("IsChecked", typeof(bool), typeof(AnnotationLibMenuItem));

            public bool IsChecked
            {
                get { return (bool)GetValue(IsCheckedProperty); }
                set { SetValue(IsCheckedProperty, value); }
            }

            public string Name
            {
                get { return (string)GetValue(NameProperty); }
                set { SetValue(NameProperty, value); }
            }

            public bool IsLayerChain
            {
                get;
                set;
            }

            public LayerInfo CorrespondingLayer
            {
                get;
                set;
            }

        }

        private SelectableBoundingBox.WithTreeNode _currInspection;

        public int SelectedFrameIndex
        {
            get { return (int)GetValue(SelectedFrameIndexProperty); }
            set 
            {
                if (value >= 0)
                {
                    SetValue(SelectedFrameIndexProperty, value);

                    //I shouldn't have to do this, but I don't fully understand wpf and xaml.
                    AnnotationThumbView.SelectedIndex = value;


                    FrameSlider.Value = value + 1;
                }
            }
        }

        public string ErrorText
        {
            get { return (string)GetValue(ErrorTextProperty);}
            set { SetValue(ErrorTextProperty, value); }
        }

        public string ConsoleOutputText
        {
            get { return (string)GetValue(ConsoleOutputTextProperty); }
            set { SetValue(ConsoleOutputTextProperty, value); }
        }

        public WriteableBitmap CapturedWindow
        {
            get
            {
                return (WriteableBitmap)GetValue(ImageProperty);
            }

            set
            {
                SetValue(ImageProperty, value);
            }
        }



        public Rect DragRect
        {
            get { return (Rect)GetValue(DragRectProperty); }
            set { SetValue(DragRectProperty, value); }
        }

        public BindingList<PropertiesControl> SelectedNodes
        {
            get { return (BindingList<PropertiesControl>)GetValue(SelectedNodesProperty); }
            set { SetValue(SelectedNodesProperty, value); }
        }

        public string CurrentPixelValue
        {
            get { return (string)GetValue(CurrentPixelProperty); }
            set { SetValue(CurrentPixelProperty, value); }
        }

        enum State
        {
            Dragging,
            Inspecting,
            DrawMode
        }

        enum UpdateType
        {
            NewFrameInterpreted,
            RectsSnapped,
            PrototypesBuilt,
            PrototypesRemoved,
            LayersUpdated,
            LayersInvalidated,
            LoadLayersFailed
        }

        public EditorWindow()
        {
            InitializeComponent();

            _annotationUndoStack = new Stack<AnnotationOperation>();
            _currState = State.Inspecting;
            CapturedWindow = new WriteableBitmap((int)System.Windows.SystemParameters.VirtualScreenWidth * 2,
                (int)System.Windows.SystemParameters.VirtualScreenHeight * 2, 96, 96, PixelFormats.Bgra32, null);

            _updateThread = new UpdateDel(UpdateUI);


            
            UserDrawnRectangleControl.StyleTemplate = RectangleViewer.StyleType.UserDrawn;
            SelectedNodes = new BindingList<PropertiesControl>();
            _invalidated = new HashSet<TextEditor>();
            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_interpreter != null)
                _interpreter.Stop();

            StopLogger();
        }


#region setting console text
        private void SetErrorText(Exception error)
        {
            if (error != null)
            {
                string message = error.Message + "\n";
                ErrorText = message;
                ErrorTab.IsSelected = true;
            }
            else
            {
                ErrorText = "";
            }
        }


        private void SetConsoleOutText(string text)
        {
            ConsoleOutputText = text;
            ConsoleTextBox.ScrollToEnd();
            ConsoleScroll.ScrollToEnd();
           
        }
#endregion



        private void SetCapturedImage(Tree tree)
        {
            VideoInterpreter.VideoCapture capture = tree["videocapture"] as VideoInterpreter.VideoCapture;
            IBoundingBox invalidatedpixels = tree["invalidated"] as IBoundingBox;

            if (_currTree == null)
            {
                capture.CopyToWriteableBitmap(CapturedWindow, tree);
                CapturedWindowImage.Width = capture.Width;
                CapturedWindowImage.Height = capture.Height;
            }

                //Todo: this is not working properly - returning null when there is a change...
            else if (invalidatedpixels != null)
            {
                capture.CopyToWriteableBitmap(CapturedWindow, invalidatedpixels);
                CapturedWindowImage.Width = capture.Width;
                CapturedWindowImage.Height = capture.Height;
            }

            capture.DisposePixels();

            CurrentFrameLabel.Content = capture.FrameIndex + 1;

            _currentBitmapImage = CopyCapturedImageToPrefabBitmap();
            _currentFrameImageHash = ImageAnnotation.GetImageId(_currentBitmapImage);
            
        }

        private void RectsSnapped(IEnumerable<IBoundingBox> snappedRects)
        {
            ClearDrawnSelected();
            foreach (IBoundingBox bb in snappedRects)
            {
                SelectableBoundingBox sbb = new SelectableBoundingBox(bb.Left, bb.Top, bb.Width, bb.Height, Brushes.YellowGreen);
                UserDrawnRectangleControl.Rectangles.Add(sbb);
            }
        }

        private void InterpretCurrentFrame()
        {
            if (_interpreter != null)
                _interpreter.InterpretFrame((int)FrameSlider.Value - 1);
            
        }

        private void UpdateUI(UpdateType updateType, object arg)
        {
            if(updateType != UpdateType.LayersInvalidated)
                SetErrorText(null);

            SetConsoleOutText(PythonHost.PythonScriptHost.Instance.ReadConsoleOutput());
            switch (updateType)
            {
                case UpdateType.NewFrameInterpreted:
                    _progressBarWindow.Hide();
                    ClearDrawnSelected();
                    VideoInterpreter.InterpretedFrame frame = arg as VideoInterpreter.InterpretedFrame;

                    
                    Tree tree = frame.Tree;

                    SetCapturedImage(tree);
                    _currTree = tree;
                    
                    ClearAllRectangles();
                    SelectedNodes.Clear();
                    AddSelectorChosenRectangles(_currTree);
                    AddTreeBrowserNodes(_currTree);
                    //SortRectangles();
                    SetErrorText(_currTree["interpretation_exception"] as Exception);
                    break;

                case UpdateType.RectsSnapped:
                    IEnumerable<IBoundingBox> snappedRects = arg as IEnumerable<IBoundingBox>;
                    RectsSnapped(snappedRects);
                    break;

                case UpdateType.LoadLayersFailed:
                    _progressBarWindow.Hide();
                    Exception exp = arg as Exception;
                    SetErrorText(exp);
                    LayerChainViewControl.SetLayerChainItems(_prefabInterpretationLogic);
                    break;

                case UpdateType.LayersUpdated:

                    if (_interpreter.FrameCount == 0)
                        _progressBarWindow.Hide();

                    InterpretCurrentFrame();

                    if (arg != null)
                    {
                        UnhandledExceptionEventArgs exargs = arg as UnhandledExceptionEventArgs;
                         exp = exargs.ExceptionObject as Exception;
                        SetErrorText(exp);
                        //RebuildButton.IsEnabled = false;
                        MessageBox.Show("Could not add one or more prototypes. Make sure it's they're the right type, and that you have perfectly cropped out the training examples.");

                        while (_annotationUndoStack.Count > 0)
                        {
                            AnnotationOperation ao = _annotationUndoStack.Pop();
                            DeleteAnnotation("ptypes_layers_user_study", ao.AnnotationId);
                        }
                    }

                    _annotationUndoStack.Clear(); 
                    _prototypeBrowserWindow.BrowserPane.SetPtypes(GetPrototypeLibraryName(), GetAllPtypes());
                    break;

                case UpdateType.LayersInvalidated:
                    LayerChainViewControl.ReloadButton.IsEnabled = true;
                    break;

            }
        }

        private void SortRectangles()
        {
            var list = new List<SelectableBoundingBox>(RectangleViewerControl.Rectangles);

            list.Sort(CompareRects);

            RectangleViewerControl.Rectangles.RaiseListChangedEvents = false;
            RectangleViewerControl.Rectangles.Clear();

            foreach (var sbb in list)
                RectangleViewerControl.Rectangles.Add(sbb);

            RectangleViewerControl.Rectangles.RaiseListChangedEvents = true;
            RectangleViewerControl.Rectangles.ResetBindings();
            
        }


        private int CompareRects(SelectableBoundingBox a, SelectableBoundingBox b)
        {
            return (b.Width * b.Height) - (a.Width * a.Height);
        }

        private void AddTreeBrowserNodes(Tree tree)
        {
            ViewableTreeNode root = new ViewableTreeNode(tree);
            TreeBrowserControl.TreeNodes.Clear();
            TreeBrowserControl.TreeNodes.Add(root);
            
        }

        private int _usedRectangleCount;
        private SelectableBoundingBox.WithTreeNode GetNewRectangle(Tree node, SolidColorBrush color, SolidColorBrush selectedColor = null)
        {
            if (_usedRectangleCount >= RectangleViewerControl.Rectangles.Count)
                AddEmptyRectangles(_usedRectangleCount);

            SelectableBoundingBox.WithTreeNode sbb = RectangleViewerControl.Rectangles[_usedRectangleCount] as SelectableBoundingBox.WithTreeNode;
            _usedRectangleCount++;

           //SelectableBoundingBox.WithTreeNode sbb = new SelectableBoundingBox.WithTreeNode(null, null, 0, 0, 0, 0, Brushes.Transparent);
           //RectangleViewerControl.Rectangles.Add(sbb);
            sbb.Color = color;
            sbb.TreeNode = node;

            if (node == null)
            {
                sbb.Width = 0;
                sbb.Height = 0;
                sbb.Top = 0;
                sbb.Left = 0;
            }
            else
            {
                sbb.Width = node.Width;
                sbb.Height = node.Height;
                sbb.Left = node.Left;
                sbb.Top = node.Top;
            }

            if (selectedColor != null)
                sbb.SelectedColor = selectedColor;
            else
                sbb.SelectedColor = sbb.Color;

            return sbb;
        }

        private void ClearAllRectangles()
        {
            
            for (int i = 0; i < _usedRectangleCount; i++)
            {
                SelectableBoundingBox.WithTreeNode sbb = RectangleViewerControl.Rectangles[i] as SelectableBoundingBox.WithTreeNode;
                if (sbb.Width > 0)
                {
                    sbb.Color = Brushes.Transparent;
                    sbb.TreeNode = null;
                    sbb.Left = 0;
                    sbb.Top = 0;
                    sbb.Width = 0;
                    sbb.Height = 0;
                    sbb.IsSelected = false;
                }
            }
            _usedRectangleCount = 0;

            _currInspection = GetNewRectangle(null, Brushes.Transparent, Brushes.Red);
            _currInspection.CreationType = SelectableBoundingBox.WithTreeNode.Type.FromClick;
        }

        private IEnumerable<SelectableBoundingBox.WithTreeNode> CreateSelectorChosenRectangles(Tree tree)
        {
            List<SelectableBoundingBox.WithTreeNode> sbbs = new List<SelectableBoundingBox.WithTreeNode>();
            RemoveRectangles(SelectableBoundingBox.WithTreeNode.Type.FromSelector);

                List<Tree> nodes = new List<Tree>();

                Tree.AddNodesToCollection(tree, nodes);
                
                foreach (Tree node in nodes)
                {
                    foreach (Selector s in SelectorPanelControl.Selectors)
                    {
                        if (s.Show)
                        {
                            bool selected = false;
                            try
                            {
                                selected = s.SelectorCode(node);
                            }
                            catch { }

                            if (selected)
                            {
                                SelectableBoundingBox.WithTreeNode sbb = GetNewRectangle(node, s.Color, Brushes.Blue);
                                sbbs.Add(sbb);
                                sbb.CreationType = SelectableBoundingBox.WithTreeNode.Type.FromSelector;
                            }
                        }
                    }
                }
       

            return sbbs;
        }
        private IEnumerable<SelectableBoundingBox.WithTreeNode> CreateTreeBrowserSelectedRectangles()
        {
            List<SelectableBoundingBox.WithTreeNode> sbbs = new List<SelectableBoundingBox.WithTreeNode>();
            foreach (ViewableTreeNode vnode in TreeBrowserControl.GetAllItems())
            {
                if (vnode.IsSelected)
                {
                    SelectableBoundingBox.WithTreeNode sbb = GetNewRectangle(vnode.Node, Brushes.Transparent, Brushes.Red);
                    sbbs.Add(sbb);
                    sbb.CreationType = SelectableBoundingBox.WithTreeNode.Type.FromTreeBrowser;
                    sbb.IsSelected = true;
                }
            }

            return sbbs;
        }
        private void AddSelectorChosenRectangles(Tree tree)
        {
            if (tree != null)
            {
                //RecycleRectangles();
                List<PropertiesControl> selected = new List<PropertiesControl>(SelectedNodes);
                CreateSelectorChosenRectangles(tree);
                //CreateAnnotationRectangles(tree);
                //ClearUnusedRectangles();
            }
        }

        private void CreateAnnotationRectangles(Tree tree)
        {

            switch (_studyCondition)
            {
                case StudyCondition.Layers:
                case StudyCondition.Debug:
                    CreateAnnotationRectanglesWithImageAnnotations(tree);
                    break;


                case StudyCondition.Single:
                    CreateAnnotationRectanglesWithPathDescriptors(tree);
                    break;
            }
           
        }

        private void CreateAnnotationRectanglesWithPathDescriptors(Tree tree)
        {

            
            if (ShowAnnotationOverlays.IsChecked.Value)
            {

                List<Tree> existing = new List<Tree>();
               IEnumerable<IRuntimeStorage> libs = _prefabInterpretationLogic.GetRuntimeStorages();
                foreach (var lib in libs.Skip(1))
                {
                    var libAnnotations = lib.ReadAllData();
                    foreach (string path in libAnnotations.Keys)
                    {
                        if (!path.Equals("config"))
                        {
                            //TODO: Create a node out of this, tagged with the library it came from, and metadata.
                            //First let's check if there's already a node for this:
                            existing.Clear();
                            GetNodesWithMatchingPath(tree, path, existing);
                            //if (existing.Count == 0)
                            //{
                        }
                    }
                }
                foreach (Tree match in existing)
                {
                    var tags = new Dictionary<string, object>();
                    tags["type"] = "annotated_node";

                    Tree node = Tree.FromBoundingBox(match, tags);

                    SelectableBoundingBox.WithTreeNode sbb = GetNewRectangle(node, Brushes.Green, Brushes.Blue);
                    sbb.CreationType = SelectableBoundingBox.WithTreeNode.Type.FromAnnotation;
                }
            }
        }

        private void GetNodesWithMatchingPath(Tree tree, string path, List<Tree> existing)
        {
            throw new NotImplementedException();
        }

        private void CreateAnnotationRectanglesWithImageAnnotations(Tree tree)
        {
            if (ShowAnnotationOverlays.IsChecked.Value)
            {
                var layers = (_prefabInterpretationLogic as LayerInterpretationLogic).Layers;
                IEnumerable<string> libs = AnnotationLibrary.GetAnnotationLibraries(layers);

                List<Tree> existing = new List<Tree>();
                string imageid = _currentFrameImageHash;
                foreach (string lib in libs)
                {
                    var libAnnotations = AnnotationLibrary.GetAnnotations(lib);
                    foreach (ImageAnnotation ia in libAnnotations)
                    {
                        if (ia.ImageId.Equals(imageid))
                        {
                            //TODO: Create a node out of this, tagged with the library it came from, and metadata.
                            //First let's check if there's already a node for this:
                            existing.Clear();
                            GetNodesWithMatchingBoundingBox(tree, ia.Region, existing);
                            //if (existing.Count == 0)
                            //{
                            var tags = new Dictionary<string, object>();
                            tags["type"] = "annotated_node";
                            tags["library"] = lib;


                            Tree node = Tree.FromBoundingBox(ia.Region, tags);

                            SelectableBoundingBox.WithTreeNode sbb = GetNewRectangle(node, Brushes.Green, Brushes.Blue);
                            sbb.CreationType = SelectableBoundingBox.WithTreeNode.Type.FromAnnotation;
                            
                        }
                        //}

                        //SelectableBoundingBox.WithTreeNode sbb = GetUnusedRectangle(node, s.Color);
                        //sbbs.Add(sbb);
                    }
                }
            }
        }
        
        private void GetNodesWithMatchingBoundingBox(Tree currNode, IBoundingBox bb, List<Tree> matches)
        {
            if (BoundingBox.Equals(currNode, bb))
                matches.Add(currNode);

            foreach (Tree child in currNode.GetChildren())
                GetNodesWithMatchingBoundingBox(child, bb, matches);

            
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Video Files, Images | *.avi; *.png";
            ofd.Multiselect = true;
            if (ofd.ShowDialog().Value)
            {
                bool notnull = _interpreter != null;

                if (notnull)
                    _interpreter.Stop();

                if (ofd.FileNames.Length > 1)
                    _interpreter = VideoInterpreter.FromMultipleScreenshots(ofd.FileNames, _prefabInterpretationLogic);
                else
                    _interpreter = VideoInterpreter.FromVideoOrImage(ofd.FileName, _prefabInterpretationLogic);
                
                AnnotationThumbView.Screenshots = _interpreter.FrameCollection;

                _interpreter.FrameInterpreted += new EventHandler<VideoInterpreter.InterpretedFrame>(_interpreter_FrameInterpreted);
                _interpreter.RectsSnapped += new EventHandler<RectsSnappedArgs>(_interpreter_RectsSnapped);
                _interpreter.PrototypesBuilt += new EventHandler<PrototypesEventArgs>(_interpreter_PrototypesBuilt);
                _interpreter.PrototypesRemoved += new EventHandler<PrototypesEventArgs>(_interpreter_PrototypesRemoved);
                
                FrameSlider.Minimum = 1;
                FrameSlider.Maximum = _interpreter.FrameCount;
                FrameSlider.IsEnabled = true;
                TotalFrameCountLabel.Content = _interpreter.FrameCount.ToString();
                double old = FrameSlider.Value;
                
                FrameSlider.Value = 1;

                UseMultipleFramesCheckbox.IsEnabled = true;
                if(old != FrameSlider.Value || notnull)
                    InterpretCurrentFrame();
            }
        }

        void _interpreter_PrototypesRemoved(object sender, PrototypesEventArgs e)
        {
            Dispatcher.BeginInvoke(_updateThread, UpdateType.PrototypesRemoved, e.Prototypes);
        }

        private void _interpreter_PrototypesBuilt(object sender, PrototypesEventArgs e)
        {
            Dispatcher.BeginInvoke(_updateThread, UpdateType.PrototypesBuilt, e.Prototypes);
            
        }

        private void _interpreter_RectsSnapped(object sender, RectsSnappedArgs e)
        {
            Dispatcher.BeginInvoke(_updateThread, UpdateType.RectsSnapped, e.Snapped);
        }

        private void _interpreter_FrameInterpreted(object sender, VideoInterpreter.InterpretedFrame frame)
        {

                HelloWorldControl.WriteBackgroundAndRender(frame.Tree);
            
            Dispatcher.BeginInvoke(_updateThread, UpdateType.NewFrameInterpreted, frame);
        }


        private void FrameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (UseMultipleFramesCheckbox.IsChecked.Value)
            {
                if (MultipleFramesStartRadio.IsChecked.Value)
                    FrameSlider.SelectionStart = e.NewValue;
                else
                    FrameSlider.SelectionEnd = e.NewValue;
            }

            _interpreter.InterpretFrame((int) e.NewValue - 1);

            SelectedFrameIndex = (int)e.NewValue - 1;

            _logger.Add(InteractionLogger.InteractionType.FrameChanged, (e.NewValue - 1).ToString());
        }


        private void GetStudyInformation()
        {
            StudyDialog studyDialog = new StudyDialog();
            if (studyDialog.ShowDialog().Value)
            {
                switch (studyDialog.GetStudyType().ToLower())
                {
                    case "layers":
                        _studyCondition = StudyCondition.Layers;
                        break;

                    case "single":
                        _studyCondition = StudyCondition.Single;
                        break;
                }

                _participantId = studyDialog.GetParticipantId();
            }
            else
            {
                _participantId = "debug";
                _studyCondition = StudyCondition.Debug;
            }
        }
        private void WatchLayersDirectory()
        {
            string dir = null;
            switch (_studyCondition)
            {
                case StudyCondition.Debug:
                case StudyCondition.Layers:
                    dir = TargetLayersDir;
                    break;


                case StudyCondition.Single:
                    dir = SingleLayersDir;
                    break;
            }
            FileSystemWatcher watcher = new FileSystemWatcher(dir);
            watcher.IncludeSubdirectories = true;
            watcher.Changed += watcher_Changed;
            watcher.Created +=watcher_Changed;
            watcher.Renamed += watcher_Changed;
            watcher.Deleted +=watcher_Changed;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.EnableRaisingEvents = true;
        }

        private void LoadLayersInBackground()
        {
            BackgroundWorker bw = new BackgroundWorker();
            LayerChainViewControl.ReloadButton.IsEnabled = false;
            bw.DoWork += LoadInterpretationLogic;
            bw.RunWorkerCompleted += LoadLayersComplete;
            bw.RunWorkerAsync((int)FrameSlider.Value - 1);
            SetProgressbarContent(ProgressMessage.UpdatingLayers);
            _progressBarWindow.ShowDialog();
        }
        private void InitializeSubWindowsAndControls()
        {
            _progressBarWindow = new ProgressBarWindow();
            _progressBarWindow.Visibility = System.Windows.Visibility.Hidden;
            _progressBarWindow.Owner = this;

            _runtimeStorageWindow = new RuntimeStorageBrowser();
            _runtimeStorageWindow.Owner = this;
            _runtimeStorageWindow.IsVisibleChanged +=_runtimeStorageWindow_IsVisibleChanged;
            _runtimeStorageWindow.SearchButton.Click += SearchButton_Click;

            _prototypeBrowserWindow = new PrototypeBrowserWindow();
            _prototypeBrowserWindow.Owner = this;
            _prototypeBrowserWindow.BrowserPane.PrototypeDeleteClicked += BrowserPane_PrototypeDeleteClicked;
            _prototypeBrowserWindow.BrowserPane.AddPositiveClicked += BrowserPane_AddPositiveClicked;
            _prototypeBrowserWindow.BrowserPane.DeleteNegativeClicked += BrowserPane_DeleteNegativeClicked;
            _prototypeBrowserWindow.BrowserPane.DeletePositiveClicked += BrowserPane_DeletePositiveClicked;
            
            _prototypeBrowserWindow.IsVisibleChanged +=_prototypeBrowserWindow_IsVisibleChanged;
            AddEmptyRectangles(200);
            string selectorstr = null;
            switch (_studyCondition)
            {
                case StudyCondition.Single:
                case StudyCondition.Layers:
                case StudyCondition.Debug:
                    selectorstr = "is_text = true";
                    break;
            }
            Color color =  System.Windows.Media.Color.FromArgb(80, 254, 162, 254);
            Selector s = new Selector(color, selectorstr);
            SelectorPanelControl.Selectors.Add(s);

            TreeBrowserControl.ItemMouseUp += TreeBrowserControl_ItemMouseUp;

            ShowBubbleCursor.IsChecked = false;
            ShowAnnotationOverlays.IsChecked = false;

            _currInspection = GetNewRectangle(null, Brushes.Transparent, Brushes.Red);
            _currInspection.CreationType = SelectableBoundingBox.WithTreeNode.Type.FromClick;
            
        }

        void BrowserPane_DeletePositiveClicked(object sender, EventArgs e)
        {
            Example ex = sender as Example;
            AnnotationLibrary.Delete(ex.AnnotationLibrary, ex.Annotation.Id);
            ThreadPool.QueueUserWorkItem(UpdateLayers, (int)FrameSlider.Value - 1);
            SetProgressbarContent(ProgressMessage.RemovingWidgets);
            _progressBarWindow.ShowDialog();
        }

        void BrowserPane_DeleteNegativeClicked(object sender, EventArgs e)
        {
            Example ex = sender as Example;
            AnnotationLibrary.Delete(ex.AnnotationLibrary, ex.Annotation.Id);
            ThreadPool.QueueUserWorkItem(UpdateLayers, (int)FrameSlider.Value - 1);
            SetProgressbarContent(ProgressMessage.RemovingWidgets);
            _progressBarWindow.ShowDialog();
        }

        private void _prototypeBrowserWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                _logger.Add(InteractionLogger.InteractionType.PtypeBrowser, "close");
            else
                _logger.Add(InteractionLogger.InteractionType.PtypeBrowser, "open");
        }

        private void _runtimeStorageWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(!(bool)e.NewValue)
                _logger.Add(InteractionLogger.InteractionType.RuntimeStorage, "close");
            else
                _logger.Add(InteractionLogger.InteractionType.RuntimeStorage, "open");
        }

        void SearchButton_Click(object sender, RoutedEventArgs e)
        {

            _logger.Add(InteractionLogger.InteractionType.RuntimeStorage, "filter," + _runtimeStorageWindow.QueryBox.Text);
        }


        private void LoadPrefabDepInBackground()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += LoadPrefabDep;
            bw.RunWorkerCompleted += PrefabDepLoaded;
            bw.RunWorkerAsync((int)FrameSlider.Value - 1);
            SetProgressbarContent(ProgressMessage.UpdatingDepCode);
            _progressBarWindow.ShowDialog();

        }

        private void LoadPrefabDep(object sender, DoWorkEventArgs e)
        {
            _prefabInterpretationLogic = new PrefabSingleLogic(SingleLayersDir);
            _prefabInterpretationLogic.Load();
            UpdateLayers(e.Argument);

        }

        private void PrefabDepLoaded(object sender, RunWorkerCompletedEventArgs e)
        {
            TopMenu.IsEnabled = true;
        }

        private void StartLogger()
        {
            _logger = new InteractionLogger(_studyCondition.ToString(), _participantId);
            _logger.Start();
        }

        private void StopLogger()
        {
            if (_logger != null)
                _logger.Stop();
        }

        private void VideoInterpretMainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            InitializeSubWindowsAndControls();
            GetStudyInformation();
            StartLogger();
            WatchLayersDirectory();
            _runtimeStorageWindow.AllowEdit = false;
            LayerChainViewControl.Visibility = Visibility.Visible;
            SetProgressbarContent(ProgressMessage.UpdatingLayers);

            switch (_studyCondition)
            {
                case StudyCondition.Single:
                    
                    _runtimeStorageWindow.AllowEdit = true;
                    break;
            }

            //if (_studyCondition == StudyCondition.HelloWorld)
            //{
            //   ShowHelloWorld.Visibility = Visibility.Visible;
            //   HelloWorldControl.Visibility = Visibility.Hidden;
            //}

            LoadLayersInBackground();
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(_updateThread, UpdateType.LayersInvalidated, null);
        }

        private void AddEmptyRectangles(int numRects)
        {
            RectangleViewerControl.Rectangles.RaiseListChangedEvents = false;
            for (int i = 0; i < numRects; i++)
            {
                SelectableBoundingBox.WithTreeNode sbb = new SelectableBoundingBox.WithTreeNode(null, null, 0, 0, 0, 0, Brushes.Transparent);
                RectangleViewerControl.Rectangles.Add(sbb);
            }

            RectangleViewerControl.Rectangles.RaiseListChangedEvents = true;
            RectangleViewerControl.Rectangles.ResetBindings();
        }

        void LoadLayersComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            LayerChainViewControl.SetLayerChainItems(_prefabInterpretationLogic);
            TopMenu.IsEnabled = true;
        }


        void LoadInterpretationLogic(object sender, DoWorkEventArgs e)
        {


            if (_prefabInterpretationLogic == null)
            {
                
                switch (_studyCondition)
                {
                    case StudyCondition.Debug:
                    case StudyCondition.Layers:
                        _prefabInterpretationLogic = new LayerInterpretationLogic(TargetLayersDir);
                        break;

                    case StudyCondition.Single:
                        _prefabInterpretationLogic = new PrefabSingleLogic(SingleLayersDir);
                        break;
                }
            }

            try
            {
                _prefabInterpretationLogic.Load();
            }
            catch (Exception exp)
            {
                
                Dispatcher.BeginInvoke(_updateThread, UpdateType.LoadLayersFailed, exp);
                return;
            }

            if (_interpreter != null)
            {

            }
            else
            {
                _interpreter = VideoInterpreter.FromVideoOrImage(null, _prefabInterpretationLogic);
                _interpreter.FrameInterpreted += new EventHandler<VideoInterpreter.InterpretedFrame>(_interpreter_FrameInterpreted);
                _interpreter.RectsSnapped += new EventHandler<RectsSnappedArgs>(_interpreter_RectsSnapped);
                _interpreter.PrototypesBuilt += new EventHandler<PrototypesEventArgs>(_interpreter_PrototypesBuilt);
                _interpreter.PrototypesRemoved += new EventHandler<PrototypesEventArgs>(_interpreter_PrototypesRemoved);

            }

            UpdateLayers(e.Argument);
        }

        private void BrowserPane_AddPositiveClicked(object sender, EventArgs e)
        {
            ViewablePrototypeItem item = sender as ViewablePrototypeItem;

            IEnumerable<IBoundingBox> rects = GetSelectedRectangles(UserDrawnRectangleControl.Rectangles);
            int frameStart = (int)FrameSlider.Value - 1;
            int frameStop = frameStart;

            if (UseMultipleFramesCheckbox.IsChecked.Value)
            {
                frameStart = (int)FrameSlider.SelectionStart - 1;
                frameStop = (int)FrameSlider.SelectionEnd - 1;
            }

            Bitmap image = _currentBitmapImage;
            foreach (IBoundingBox rect in rects)
            {
                JObject data = new JObject();
                data["ptypeId"] = item.Guid;
                data["model"] = item.PrototypeVisual.Model.Name;
                data["positive"] = "true";
                string ptypelibrary = _prefabInterpretationLogic.GetPtypeDatabase();
                AddPtypeAnnotation(data, rect, ptypelibrary, image);
            }
            ThreadPool.QueueUserWorkItem(UpdateLayers, frameStart);

            SetProgressbarContent(ProgressMessage.AddingWidgets);
            _progressBarWindow.ShowDialog();
        }

        private void UpdateLayers(object frameIndex)
        {
            try
            {
                _prefabInterpretationLogic.UpdateLogic();
                Dispatcher.BeginInvoke(_updateThread, UpdateType.LayersUpdated, null);
            }
            catch(PtypeBuildException e)
            {
                Dispatcher.BeginInvoke(_updateThread, UpdateType.LayersUpdated, e.BuildArgs);
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(_updateThread, UpdateType.LoadLayersFailed, e);
            }
            
        }
        

        void TreeBrowserControl_ItemMouseUp(object sender, EventArgs e)
        {
            
            TreeBrowserSelectAndLog(false);
        }

        private void TreeBrowserSelectAndLog(bool mousemove)
        {
            
            
            
            //if (!mousemove)
           
            RemoveRectanglesKeepClicked();
            AddSelectorChosenRectangles(_currTree);
            CreateAnnotationRectangles(_currTree);

            IEnumerable<SelectableBoundingBox.WithTreeNode> nodes = CreateTreeBrowserSelectedRectangles();
            foreach (SelectableBoundingBox.WithTreeNode node in nodes)
            {
                AddSelectedNode(node.TreeNode);
            }

            string nodesLogString = _currentFrameImageHash;
            nodesLogString += ",";
            foreach (var node in nodes)
            {
                nodesLogString += node.Left + "," + node.Top + "," + node.Width + "," + node.Height + ",";
            }


            if (mousemove)
                nodesLogString += "mousemove";
            else
                nodesLogString += "nodeclick";
            _logger.Add(InteractionLogger.InteractionType.TreeBrowserClick, nodesLogString);
        }


        private void SelectorPanelControl_SelectorChanged(object sender, EventArgs e)
        {
            if (_currTree != null)
            {
                AddSelectorChosenRectangles(_currTree);
                
                string currSelectors = "";
                foreach(Selector s in SelectorPanelControl.Selectors)
                    currSelectors += s.SelectorText + ",";

                currSelectors.Remove(currSelectors.Length -1);
                _logger.Add(InteractionLogger.InteractionType.Selector, currSelectors);
            }
        }

        #region drag rect logic
        public void OnMove(int cursorLeft, int cursorTop)
        {
            switch (_currState)
            {
                case State.Dragging:
                    Cursor = Cursors.Cross;
                    double imageleft = 0;
                    double imagetop = 0;
                    double imagebottom = CapturedWindowImage.Height;
                    double imageright = CapturedWindowImage.Width;

                    int left = Math.Min(_pinX, cursorLeft);
                    int top = Math.Min(_pinY, cursorTop);
                    int right = Math.Max(_pinX, cursorLeft);
                    int bottom = Math.Max(_pinY, cursorTop);

                    if (left < imageleft)
                        left = (int)imageleft;
                    if (top < imagetop)
                        top = (int)imagetop;
                    if (right >= imageright)
                        right = (int)imageright - 1;
                    if (bottom >= imagebottom)
                        bottom = (int)imagebottom - 1;

                    DragRect = new Rect(left, top, right - left + 1, bottom - top + 1);
                    break;

                case State.Inspecting:
                    Cursor = Cursors.Arrow;
                    if (_currTree != null && ShowBubbleCursor.IsChecked.Value)
                    {
                        BubbleCursorControl.Visibility = System.Windows.Visibility.Visible;
                        Tree node = BubbleCursorOverlay.GetClosestTarget(cursorLeft, cursorTop, _currTree);
                        if (node != null)
                        {
                            BubbleCursorControl.BubbleCursorGeometry = BubbleCursorVisualizer.GetBubbleCursorPathFigure(node, cursorLeft, cursorTop);
                            BubbleCursorControl.TargetLeft = node.Left;
                            BubbleCursorControl.TargetTop = node.Top;
                            BubbleCursorControl.TargetHeight = node.Height;
                            BubbleCursorControl.TargetWidth = node.Width;
                        }
                    }
                    else// if(GetSelectedRectangles(RectangleViewerControl.Rectangles).Count() == 0)
                    {
                        Tree node = GetNodeUnder(cursorLeft, cursorTop, _currTree);

                        var rects = RectangleViewerControl.Rectangles;
                        bool equals = false;
                        foreach (var rect in rects)
                        {
                            if (BoundingBox.Equals(rect, node) && rect != _currInspection)
                            {
                                equals = true;
                                break;
                            }

                        }
                        if (!equals && node != null)
                        {
                            _currInspection.TreeNode = node;
                            
                            _currInspection.Top = node.Top;
                            _currInspection.Left = node.Left;
                            _currInspection.Width = node.Width;
                            _currInspection.Height = node.Height;
                            _currInspection.Color = Brushes.Transparent;
                            _currInspection.SelectedColor = Brushes.Red;
                            _currInspection.IsSelected = false;
                        }
                        else
                        {
                            _currInspection.TreeNode = null;
                            _currInspection.Top = 0;
                            _currInspection.Left = 0;
                            _currInspection.Width = 0;
                            _currInspection.Height = 0;
                            _currInspection.IsSelected = false;
                        }

                        //var selected = UserDrawnRectangleControl.Rectangles.Where(s => s.IsSelected);
                        //List<Tree> toignore = new List<Tree>();
                        //foreach (SelectableBoundingBox.WithTreeNode rect in selected)
                        //{
                        //    toignore.Add(rect.TreeNode);
                        //}
                        //TreeBrowserControl.SelectNodeAndRectangle(node, toignore);
                        //TreeBrowserSelectAndLog(true);


                    }
                    break;

                case State.DrawMode:
                    Cursor = Cursors.Cross;
                    break;
            }

            Color pixel = GetPixel(CapturedWindow, cursorLeft, cursorTop);
            string hex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", pixel.A, pixel.R, pixel.G, pixel.B);
            CurrentPixelValue = "x=" + cursorLeft + ", y=" + cursorTop + ", r=" + pixel.R + ", g=" + pixel.G + ", b=" + pixel.B 
                + "  (" + hex + ")";
        }

        private Tree GetNodeUnder(int x, int y, Tree tree)
        {
            if (BoundingBox.Contains(tree, x, y))
            {
                foreach (Tree child in tree.GetChildren())
                {
                    Tree smaller = GetNodeUnder(x, y, child);
                    if (smaller != null)
                        return smaller;
                }

                return tree;
            }

            return null;
        }

        private static Color GetPixel(WriteableBitmap wbm, int x, int y)
        {
            if (y > wbm.PixelHeight - 1 ||
              x > wbm.PixelWidth - 1)
                return Color.FromArgb(0, 0, 0, 0);
            if (y < 0 || x < 0)
                return Color.FromArgb(0, 0, 0, 0);
            if (!wbm.Format.Equals(
                    PixelFormats.Bgra32))
                return Color.FromArgb(0, 0, 0, 0); ;
            IntPtr buff = wbm.BackBuffer;
            int Stride = wbm.BackBufferStride;
            Color c;
            
            unsafe
            {
                byte* pbuff = (byte*)buff.ToPointer();
                int loc = y * Stride + x * 4;
                c = Color.FromArgb(pbuff[loc + 3],
                  pbuff[loc + 2], pbuff[loc + 1],
                    pbuff[loc]);
            }
            return c;
        }

        public void StartDragRecting(int x, int y)
        {
            _pinX = x;
            _pinY = y;
            _currState = State.Dragging;
            DragRectangleControl.Visibility = Visibility.Visible;
        }
        public void StopDragRecting()
        {
            if (_currState == State.Dragging)
            {
                _currState = State.DrawMode;
                if (!DragRect.IsEmpty && DragRect.Width > 2 && DragRect.Height > 2)
                {
                    SelectableBoundingBox sbb = new SelectableBoundingBox((int)DragRect.Left, (int)DragRect.Top, (int)DragRect.Width, (int)DragRect.Height, Brushes.YellowGreen);

                    if (Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        List<BoundingBox> list = new List<BoundingBox>() { new BoundingBox(sbb.Left, sbb.Top, sbb.Width, sbb.Height) };
                        _interpreter.SnapRectangles(list, (int)FrameSlider.Value - 1);
                    }
                    else
                    {
                        UserDrawnRectangleControl.Rectangles.Add(sbb);
                    }
                }

                DragRect = Rect.Empty;
                DragRectangleControl.Visibility = Visibility.Hidden;
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ShowBubbleCursor.IsChecked.Value && _currState == State.DrawMode)
            {
                System.Windows.Point pos = e.GetPosition(CapturedWindowImage);
                StartDragRecting((int)pos.X, (int)pos.Y);
            }

        }
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point pos = e.GetPosition(CapturedWindowImage);
            int cursorLeft = (int)pos.X;
            int cursorTop = (int)pos.Y;
            OnMove(cursorLeft, cursorTop);
        }
        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RemoveRectanglesKeepClicked();
            TreeBrowserControl.ClearSelected();
            StopDragRecting();
        }
        private void RemoveRectanglesKeepClicked()
        {
            //ClearAllRectangles();
            RemoveRectangles(SelectableBoundingBox.WithTreeNode.Type.FromSelector);
            RemoveRectangles(SelectableBoundingBox.WithTreeNode.Type.FromTreeBrowser);
            RemoveRectangles(SelectableBoundingBox.WithTreeNode.Type.FromAnnotation);
        }

        private void DeSelectAllRectangles()
        {
            foreach (var rect in RectangleViewerControl.Rectangles)
            {
                if (rect.IsSelected)
                {
                    rect.IsSelected = false;
                    RectangleViewerControl_RectangleSelectionChanged(rect, null);
                }
            }
        }


        private void RemoveRectangles(SelectableBoundingBox.WithTreeNode.Type type)
        {
            List<SelectableBoundingBox.WithTreeNode> torem = new List<SelectableBoundingBox.WithTreeNode>();
            foreach (SelectableBoundingBox.WithTreeNode sbb in RectangleViewerControl.Rectangles)
            {
                if (sbb.CreationType == type)
                    torem.Add(sbb);
            }
            
            foreach (var rem in torem)
                RemoveSelectedNode(rem);
        }

        


        private void RectDoubleClicked(object sender, RectangleViewer.RectMouseEventArgs args)
        {
            Rect toView = new Rect(args.Rect.Left - 6, args.Rect.Top - 6, args.Rect.Width + 12, args.Rect.Height + 12);
        }


        
        #endregion


        private IEnumerable<IBoundingBox> GetSelectedRectangles(IEnumerable<SelectableBoundingBox> rectangles)
        {
            List<IBoundingBox> rects = new List<IBoundingBox>();
            foreach (SelectableBoundingBox sbb in rectangles.Where((s) => s.IsSelected))
            {
                rects.Add( new BoundingBox(sbb.Left, sbb.Top, sbb.Width, sbb.Height));
            }

            return rects;
        }
        private void CommandToolbar_ExtractNinePartClicked(object sender, RoutedEventArgs e)
        {

            
            IEnumerable<IBoundingBox> rects = GetSelectedRectangles(UserDrawnRectangleControl.Rectangles);
            int frameStart = (int)FrameSlider.Value - 1;
            int frameStop = frameStart;

            if (UseMultipleFramesCheckbox.IsChecked.Value)
            {
                frameStart = (int)FrameSlider.SelectionStart - 1;
                frameStop = (int)FrameSlider.SelectionEnd - 1;
            }

            Bitmap image = _currentBitmapImage;
            foreach (IBoundingBox rect in rects)
            {
                JObject data = new JObject();
                data["ptypeId"] = Guid.NewGuid();
                data["model"] = "ninepart";
                data["positive"] = "true";
                string ptypelib = _prefabInterpretationLogic.GetPtypeDatabase();
                AddPtypeAnnotation(data, rect, ptypelib, image);
            }

            ThreadPool.QueueUserWorkItem(UpdateLayers, frameStart);
            SetProgressbarContent(ProgressMessage.AddingWidgets);
            _progressBarWindow.ShowDialog();
        }

        private void NegativeExample_Click(object sender, RoutedEventArgs e)
        {


            IEnumerable<SelectableBoundingBox> rects = RectangleViewerControl.Rectangles.Where(s => s.IsSelected);


            int frameStart = (int)FrameSlider.Value - 1;
            int frameStop = frameStart;

            if (UseMultipleFramesCheckbox.IsChecked.Value)
            {
                frameStart = (int)FrameSlider.SelectionStart - 1;
                frameStop = (int)FrameSlider.SelectionEnd - 1;
            }

            Bitmap image = _currentBitmapImage;
            foreach (SelectableBoundingBox.WithTreeNode rect in rects)
            {
                JObject data = new JObject();
                Ptype ptype = rect.TreeNode["ptype"] as Ptype;
                data["ptypeId"] = ptype.Id;
                data["model"] = ptype.Model.Name;
                data["positive"] = "false";
                string ptypelib = _prefabInterpretationLogic.GetPtypeDatabase();
                AddPtypeAnnotation(data, rect, ptypelib, image);
            }
            
            ThreadPool.QueueUserWorkItem(UpdateLayers, frameStart);
            SetProgressbarContent(ProgressMessage.AddingWidgets);
            _progressBarWindow.ShowDialog();
        }


        private void AddPtypeAnnotation(JObject ptypeData, IBoundingBox rect, string library, Bitmap image)
        {
            ImageAnnotation existing = AnnotationLibrary.GetAnnotation(library, image, rect);

            JObject all = null;
            if (existing == null)
            {
                all = new JObject();
                JArray ptypes = new JArray();
                ptypes.Add(ptypeData);

                all["ptypes"] = ptypes;
                existing = AnnotationLibrary.AddAnnotation(library, image, rect, all);
               
            }
            else
            {
                all = existing.Data;
                JArray ptypes = all["ptypes"] as JArray;
                ptypes.Add(ptypeData);
                AnnotationLibrary.UpdateExisting(library, existing.Id, all);
            }

            _annotationUndoStack.Push(new AnnotationOperation(existing.Id));

            string tolog = "added," + library + "," + existing.Id;
            _logger.Add(InteractionLogger.InteractionType.Annotation, tolog);
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            //IEnumerable<SelectableBoundingBox> nodes = RectangleViewerControl.Rectangles.Where(r => r.IsSelected);
            //List<Tree> togroup = new List<Tree>();
            
            //foreach (SelectableBoundingBox.WithTreeNode node in nodes)
            //{
            //    togroup.Add(node.TreeNode);
            //}
            
            //Guid groupId = Guid.NewGuid();

            //foreach (Tree node in togroup)
            //{
            //    JsonData data = new JsonData();
               
            //    data["groupid"] = groupId.ToString("D");
            //    data["id"] = Guid.NewGuid().ToString("D");
            //    data["lib"] = "logical_grouping";
            //    _prefab.AddAnnotation(node,  data, GetImage());
            //}

            //UpdateLayers();
        }

        private enum ProgressMessage
        {
            AddingWidgets,
            RemovingWidgets,
            UpdatingLayers,
            UpdatingDepCode
        }
        private void SetProgressbarContent(ProgressMessage message)
        {
            switch (message)
            {
                case ProgressMessage.AddingWidgets:
                    _progressBarWindow.Description.Content = "Finding more widgets ...";
                    break;

                case ProgressMessage.RemovingWidgets:
                    _progressBarWindow.Description.Content = "Removing widgets ...";
                    break;

                case ProgressMessage.UpdatingLayers:
                    _progressBarWindow.Description.Content = "Configuring Layers ...";
                    break;

                case ProgressMessage.UpdatingDepCode:
                    _progressBarWindow.Description.Content = "Configuring Interpretation Code";
                    break;
            }

        }

        private void CommandToolbar_ExtractOnePartClicked(object sender, RoutedEventArgs e)
        {
            IEnumerable<IBoundingBox> rects = GetSelectedRectangles(UserDrawnRectangleControl.Rectangles);
            int frameStart = (int)FrameSlider.Value - 1;
            //int frameStop = frameStart;

            //if (UseMultipleFramesCheckbox.IsChecked.Value)
            //{
            //    frameStart = (int)FrameSlider.SelectionStart - 1;
            //    frameStop = (int)FrameSlider.SelectionEnd - 1;
            //}


            Bitmap image = _currentBitmapImage;
            string ptypeLibName = GetPrototypeLibraryName();
            foreach (IBoundingBox rect in rects)
            {
                JObject data = new JObject();
                data["ptypeId"] = Guid.NewGuid();
                data["model"] = "onepart";
                data["positive"] = "true";
                AddPtypeAnnotation(data, rect, ptypeLibName, image);
            }
            ThreadPool.QueueUserWorkItem(UpdateLayers, frameStart);
            SetProgressbarContent(ProgressMessage.AddingWidgets);
            _progressBarWindow.ShowDialog();
        }

        private string GetPrototypeLibraryName()
        {

            return _prefabInterpretationLogic.GetPtypeDatabase();
        }

        private IEnumerable<Ptype> GetAllPtypes()
        {
            return _prefabInterpretationLogic.GetPtypes();

        }

        

        private void CommandToolbar_ExtractSliderClicked(object sender, RoutedEventArgs e)
        {
            //IEnumerable<IBoundingBox> rects = GetSelectedRectangles(UserDrawnRectangleControl.Rectangles);
            //int frameStart = (int)FrameSlider.Value - 1;
            //int frameStop = frameStart;

            //if (UseMultipleFramesCheckbox.IsChecked.Value)
            //{
            //    frameStart = (int)FrameSlider.SelectionStart - 1;
            //    frameStop = (int)FrameSlider.SelectionEnd - 1;
            //}

            //_interpreter.BuildNewPrototypesFromExamples(SliderModel.Instance, rects, frameStart, frameStop);
            //SetProgressbarContent(ProgressMessage.AddingWidgets);
            //_progressBarWindow.ShowDialog();
        }

        private void CommandToolbar_SnapButtonClicked(object sender, RoutedEventArgs e)
        {
            IEnumerable<IBoundingBox> rects = GetSelectedRectangles(UserDrawnRectangleControl.Rectangles);
            int currframe = (int)FrameSlider.Value - 1;
            _interpreter.SnapRectangles(rects, currframe);
        }


        private void CommandToolbar_DeletePtypeClicked(object sender, RoutedEventArgs e)
        {
            //IEnumerable<SelectableBoundingBox> rects = RectangleViewerControl.Rectangles.Where( (s) => s.IsSelected );
            //List<Guid> ptypes = new List<Guid>();
            //foreach (SelectableBoundingBox.WithTreeNode sbb in rects)
            //{
            //    ptypes.Add(sbb.TreeNode.Occurrence.Prototype.Guid);
            //}

            //DeletePtypes(ptypes);
        }

        private void DeletePtypes(IEnumerable<Guid> ptypes)
        {
            //_interpreter.RemovePrototypes(ptypes);
            //SetProgressbarContent(ProgressMessage.RemovingWidgets);
            //_progressBarWindow.ShowDialog();
        }

        private void ClearDrawnSelected()
        {
            List<SelectableBoundingBox> selected = new List<SelectableBoundingBox>(UserDrawnRectangleControl.Rectangles.Where((w) => w.IsSelected));

            foreach (SelectableBoundingBox sbb in selected)
            {
                UserDrawnRectangleControl.Rectangles.Remove(sbb);
            }
        }

        private void RectangleViewerControl_RectangleClosed(object sender, RoutedEventArgs e)
        {
            //SelectableBoundingBox.WithTreeNode sbb = sender as SelectableBoundingBox.WithTreeNode;
            //Guid guid = sbb.TreeNode.Occurrence.Prototype.Guid;

            //List<Guid> list = new List<Guid>() { guid };

            //DeletePtypes(list);
        }

        private void AddSelectedNode(Tree node)
        {
            
            PropertiesControl pc = SelectedNodes.FirstOrDefault(s => s.Node == node);
            if (pc == null)
            {
                pc = new PropertiesControl();

                IEnumerable<string> annotationlibnames = _prefabInterpretationLogic.GetAnnotationLibraries();//AnnotationLibrary.GetAnnotationLibraries(_prefabChain);
                Bitmap screenshot = _currentBitmapImage;
                Bitmap representativeImage = Bitmap.Crop(screenshot, node);
                
                pc.SetProperties(node, _currTree, screenshot, GetPrototypeLibraryName(),
                    _prefabInterpretationLogic, representativeImage, annotationlibnames);
                SelectedNodes.Add(pc);

                TreeBrowserControl.SelectNode(node);
            }
        }



        private void RemoveSelectedNode(SelectableBoundingBox.WithTreeNode sbb)
        {
            Tree node = sbb.TreeNode;

            sbb.Left = 0;
            sbb.TreeNode = null;
            sbb.Top = 0;
            sbb.Width = 0;
            sbb.Height = 0;
            sbb.IsSelected = false;
            sbb.Color = Brushes.Transparent;
            
            //RectangleViewerControl.Rectangles.Remove(sbb);
            PropertiesControl torem = SelectedNodes.FirstOrDefault(s => s.Node == node);
            
            

            if (torem != null)
            {
                SelectedNodes.Remove(torem);
                TreeBrowserControl.DeselectNode(node);
            }
        }

        private void RectangleViewerControl_RectangleSelectionChanged(object sender, EventArgs e)
        {

            SelectableBoundingBox.WithTreeNode sbb = sender as SelectableBoundingBox.WithTreeNode;

            if (sbb.IsSelected)
            {
                AddSelectedNode(sbb.TreeNode);
                if (sbb == _currInspection)
                {
                    _currInspection = GetNewRectangle(sbb.TreeNode, sbb.Color, sbb.SelectedColor);
                    _currInspection.CreationType = SelectableBoundingBox.WithTreeNode.Type.FromClick;
                }
            }
            else if(sbb.CreationType == SelectableBoundingBox.WithTreeNode.Type.FromClick
                || sbb.CreationType == SelectableBoundingBox.WithTreeNode.Type.FromTreeBrowser)
            {
                RemoveSelectedNode(sbb);

            }
            else
            {
                var pc = SelectedNodes.FirstOrDefault( p => p.Node == sbb.TreeNode);
                if(pc != null)
                    SelectedNodes.Remove(pc);
            }
        }


        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double scalePercent = e.NewValue;
            double scaler = scalePercent / 100;
            if (ImageGrid != null)
            {
                ImageGrid.LayoutTransform = new ScaleTransform(scaler, scaler);

            }
            if(ZoomPercent != null)
                ZoomPercent.Text = scalePercent + "%";
        }



        #region code editor logic





        void item_CloseTab(object sender, RoutedEventArgs e)
        {
            //CloseableTabItem item = sender as CloseableTabItem;
            //TextEditor te = item.Content as TextEditor;

            //if (WasInvalidated(te))
            //{
            //    MessageBoxResult res = MessageBox.Show("Would you like to save changes?");

            //    if (res == MessageBoxResult.OK)
            //    {
            //        SaveTextEditorBuffer(te);
            //    }
            //}

            //CodeTabControl.Items.Remove(item);
        }

        private void SaveTextEditorBuffer(TextEditor te)
        {
            LayerInfo layer = te.DataContext as LayerInfo;
            te.Save(layer.File.Value);
            
            _invalidated.Remove(te);
        }

        private bool WasInvalidated(TextEditor te)
        {
            return _invalidated.Contains(te);
        }

        #endregion


        private LayerInfo GetViewLayerForLibrary(string library, IEnumerable<LayerInfo> layers)
        {
            LayerInfo layer = layers.FirstOrDefault(l => !l.AllowSave && l.Parameters.ContainsKey("library") && l.Parameters["library"].Equals(library));

            return layer;
        }


        private Bitmap CopyCapturedImageToPrefabBitmap()
        {
            Bitmap bitmap = FromBitmapSource(CapturedWindow,0, 0, (int)CapturedWindowImage.Width, (int)CapturedWindowImage.Height);
            return bitmap;
        }

        public static Bitmap FromBitmapSource(BitmapSource img, int top, int left, int width, int height)
        {

            Int32[] pixels = new Int32[width * height];

            int stride = img.Format.BitsPerPixel / 8;
            stride *= width;
            Int32Rect rect = new Int32Rect(left, top, width, height);
            img.CopyPixels(rect, pixels, stride, 0);

            Bitmap toreturn = Bitmap.FromPixels(width, height, pixels);

            if (img.Format != PixelFormats.Bgra32)
                SetFullAlpha(toreturn);

            return toreturn;
        }



        private static void SetFullAlpha(Bitmap bitmap)
        {
            int length = bitmap.Width * bitmap.Height;
            for (int i = 0; i < length; i++)
            {
                bitmap[i] = (0xff << 24) | bitmap[i];
            }
        }

        private void IsTargetAnnotationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            JObject target = new JObject();
            target["is_target"] = "true";
            AddAnnotation("target_corrections", target);
        }



        private void DeleteAnnotation(string library, IBoundingBox region)
        {
            string id =  ImageAnnotation.GetAnnotationId(_currentBitmapImage, region);
            DeleteAnnotation(library, id);
        }

        private void DeleteAnnotation(string library, string id)
        {
            AnnotationLibrary.Delete(library, id);

            _logger.Add(InteractionLogger.InteractionType.Annotation, "deleted," + library + "," + id);
        }

        private void AddAnnotation(string library, JObject annotation)
        {
            IEnumerable<SelectableBoundingBox> nodes = RectangleViewerControl.Rectangles.Where(r => r.IsSelected);
            Bitmap image = _currentBitmapImage;
            foreach (SelectableBoundingBox.WithTreeNode sbb in nodes)
            {

                switch (_studyCondition)
                {
                    case StudyCondition.Debug:
                    case StudyCondition.Layers:
                        ImageAnnotation imageAnnotation = AnnotationLibrary.GetAnnotation(library, image, sbb.TreeNode);
                        if (imageAnnotation == null)
                            imageAnnotation = AnnotationLibrary.AddAnnotation(library, image, sbb.TreeNode, annotation);
                        else
                        {
                            foreach (var prop in annotation.Properties())
                            {
                                imageAnnotation.Data[prop.Name] = annotation[prop.Name];
                            }

                            AnnotationLibrary.UpdateExisting(library, imageAnnotation.Id, imageAnnotation.Data);
                        }

                        string tolog = "added," + library + "," + imageAnnotation.Id;
                        _logger.Add(InteractionLogger.InteractionType.Annotation, tolog);
                        break;


                    case StudyCondition.Single:

                        PrefabSingleLogic prefabdep = _prefabInterpretationLogic as PrefabSingleLogic;
                        Tree node = sbb.TreeNode;
                        string path = PathDescriptor.GetPath(node, _currTree);
                        prefabdep.Storage.PutData(path, annotation);
                        tolog = "added," + library + "," + path;
                        _logger.Add(InteractionLogger.InteractionType.Annotation, tolog);
                        break;

                }
               
            }

           

            ThreadPool.QueueUserWorkItem(UpdateLayers, (int)FrameSlider.Value - 1);
            SetProgressbarContent(ProgressMessage.UpdatingLayers);
            _progressBarWindow.ShowDialog();
        }

        void adder_AnnotationAdded(object sender, PropertiesControl.AnnotationAddedArgs e)
        {
            JObject isTarget = new JObject();
            isTarget["is_target"] = "true";
            AddAnnotation("target_corrections", isTarget);
        }

        private void UseMultipleFramesCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            MultipleFramesStartRadio.IsEnabled = true;
            MultipleFramesEndRadio.IsEnabled = true;
            MultipleFramesStartRadio.IsChecked = true;

            FrameSlider.SelectionStart = FrameSlider.Value;
            FrameSlider.SelectionEnd = FrameSlider.Value;
        }

        private void UseMultipleFramesCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            MultipleFramesStartRadio.IsEnabled = false;
            MultipleFramesEndRadio.IsEnabled = true;
            FrameSlider.SelectionStart = 1;
            FrameSlider.SelectionEnd = 1;
        }

        private void PtypeBrowser_Click(object sender, RoutedEventArgs e)
        {
            
            _prototypeBrowserWindow.BrowserPane.SetPtypes(GetPrototypeLibraryName(), GetAllPtypes()  );


            _prototypeBrowserWindow.Show();
            _progressBarWindow.Width = 600;
            _progressBarWindow.Height = 1400;
        }

        void BrowserPane_PrototypeDeleteClicked(object sender, EventArgs e)
        {
            ViewablePrototypeItem item = sender as ViewablePrototypeItem;
            
            foreach (Example ex in item.PositiveExamples)
            {
                DeleteAnnotation(ex.AnnotationLibrary, ex.Annotation.Id);
            }
            foreach (Example ex in item.NegativeExamples)
            {
                DeleteAnnotation(ex.AnnotationLibrary, ex.Annotation.Id);
            }

            ThreadPool.QueueUserWorkItem(UpdateLayers, (int)FrameSlider.Value - 1);
            SetProgressbarContent(ProgressMessage.RemovingWidgets);
            _progressBarWindow.ShowDialog();
            _logger.Add(InteractionLogger.InteractionType.PtypeBrowser, "delete," + item.Guid);
        }

        private void ScrollViewer_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                ZoomSlider.Value += e.Delta / 10;
            }
        }

        private void OpenAnnotationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_interpreter != null)
                _interpreter.Stop();
                

            _interpreter = VideoInterpreter.FromAnnotations(_prefabInterpretationLogic as LayerInterpretationLogic);
            
            AnnotationThumbView.Screenshots = _interpreter.FrameCollection;

            _interpreter.FrameInterpreted += new EventHandler<VideoInterpreter.InterpretedFrame>(_interpreter_FrameInterpreted);
            _interpreter.RectsSnapped += new EventHandler<RectsSnappedArgs>(_interpreter_RectsSnapped);
            _interpreter.PrototypesBuilt += new EventHandler<PrototypesEventArgs>(_interpreter_PrototypesBuilt);
            _interpreter.PrototypesRemoved += new EventHandler<PrototypesEventArgs>(_interpreter_PrototypesRemoved);

            FrameSlider.Minimum = 1;
            FrameSlider.Maximum = _interpreter.FrameCount;
            FrameSlider.IsEnabled = true;
            TotalFrameCountLabel.Content = _interpreter.FrameCount.ToString();


            SelectedFrameIndex = 0;
            InterpretCurrentFrame();
        }

        private void AnnotationThumbView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedFrameIndex = AnnotationThumbView.SelectedIndex;
        }

        private void ShowBubbleCursor_Checked(object sender, RoutedEventArgs e)
        {
            BubbleCursorControl.Visibility = System.Windows.Visibility.Visible;
        }

        private void ShowBubbleCursor_Unchecked(object sender, RoutedEventArgs e)
        {
            BubbleCursorControl.Visibility = System.Windows.Visibility.Hidden;
        }

        private void SelectorPanelControl_SelectorChecked(object sender, EventArgs e)
        {
            RemoveRectanglesKeepClicked();
            AddSelectorChosenRectangles(_currTree);
        }

        private void SelectorPanelControl_SelectorUnChecked(object sender, EventArgs e)
        {
            RemoveRectanglesKeepClicked();
            AddSelectorChosenRectangles(_currTree);
        }

        private void CapturedWindowImage_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ShowBubbleCursor.IsChecked.Value)
                BubbleCursorControl.Visibility = Visibility.Visible;

            switch (_currState)
            {
                case State.DrawMode:
                    Cursor = Cursors.Cross;
                    break;

                case State.Inspecting:
                    Cursor = Cursors.Arrow;
                    break;
            }
        }

        private void CapturedWindowImage_MouseLeave(object sender, MouseEventArgs e)
        {

            
            BubbleCursorControl.Visibility = System.Windows.Visibility.Hidden;
            
            Cursor = Cursors.Arrow;
            
        }

        private void RemoveTargetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            JObject notTarget = new JObject();
            notTarget["is_target"] = "false";
            AddAnnotation("target_corrections", notTarget);
        }

        public void SaveLayerChain()
        {
            string tosave = "";



            foreach (LayerChainItem item in LayerChainViewControl.LayerChainItems)
            {
                tosave += item.RelativePath;
                for (int i = 0; i < item.ParameterKeys.Count; i++)
                {

                    tosave += " " + item.ParameterKeys[i] + "=";
                    if (item.ParameterValues[i] is string)
                        tosave += item.ParameterValues[i];
                    else if (item.ParameterValues[i] is IEnumerable<string>)
                    {
                        foreach (string str in (IEnumerable<string>)item.ParameterValues[i])
                        {
                            tosave += str + ",";
                        }

                        tosave = tosave.Remove(tosave.Length - 1);
                    }
                    
                }
                tosave += "\r\n";
            }

            LayerInterpretationLogic layerlog = _prefabInterpretationLogic as LayerInterpretationLogic;
            File.WriteAllText(layerlog.LayerDirectory + "\\chain.txt", tosave);
        }

        private void LayerChainViewControl_ReloadClicked(object sender, EventArgs args)
        {
            LoadLayersInBackground();
        }

        

        private void LayerChainViewControl_ParameterAdded(object sender, LayerLibrariesView.LibraryAddedOrRemovedEventArgs e)
        {
            SaveLayerChain();

        }

        private void LayerChainViewControl_ParameterRemoved(object sender, LayerLibrariesView.LibraryAddedOrRemovedEventArgs e)
        {
            SaveLayerChain();
        }

        private void LayerChainViewControl_LayerMoved(object sender, EventArgs e)
        {
            SaveLayerChain();

        }

        private void LayerChainViewControl_LayerDeleted(object sender, EventArgs e)
        {
            SaveLayerChain();
        }

        private void ShowAnnotationOverlays_Checked(object sender, RoutedEventArgs e)
        {
            RemoveRectanglesKeepClicked();
            CreateAnnotationRectangles(_currTree);
            AddSelectorChosenRectangles(_currTree);
        }

        private void ShowAnnotationOverlays_Unchecked(object sender, RoutedEventArgs e)
        {
            RemoveRectanglesKeepClicked();
            //CreateAnnotationRectangles(_currTree);
            //AddSelectorChosenRectangles(_currTree);
        }


        /// <summary>
        /// This method is a mess. It's just adding runtime intent data to a list. Needs to be rewritten.
        /// I'm writing this because I'm embarrassed at the awfulness of this method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RuntimeStorageBrowser_Click(object sender, RoutedEventArgs e)
        {
            _runtimeStorageWindow.ClearItems();
            List<Tuple<IRuntimeStorage, string, JToken>> runtimeData = new List<Tuple<IRuntimeStorage, string, JToken>>();

            IEnumerable<IRuntimeStorage> intents = _prefabInterpretationLogic.GetRuntimeStorages();

            intents = intents.Skip(1); //current hack to skip prototype library

            foreach (var intent in intents)
                foreach (var data in intent.ReadAllData())
                    runtimeData.Add(new Tuple<IRuntimeStorage, string, JToken>(intent, data.Key, data.Value));
                
           
            foreach (var toAdd in runtimeData)
            {
                if (!toAdd.Item2.Equals("config"))
                {
                    GridItem item = new GridItem();
                    item.DocumentName = toAdd.Item2;
                    item.Data = toAdd.Item3.ToString().Replace("{", "").Replace("}", "").Trim();
                    item.DeleteCommand = new DeleteCommand(_runtimeStorageWindow, item, toAdd.Item1);
                    if (_runtimeStorageWindow.AllowEdit)
                        item.Visibility = System.Windows.Visibility.Visible;
                    else
                        item.Visibility = System.Windows.Visibility.Collapsed;
                    _runtimeStorageWindow.AddItem(item);


                }
            }

            _runtimeStorageWindow.Show();
        }

        private class DeleteCommand : ICommand
        {
            private IRuntimeStorage _intent;
            private RuntimeStorageBrowser _window;
            private GridItem _item;
            public DeleteCommand(RuntimeStorageBrowser window, GridItem item,IRuntimeStorage intent)
            {
                _item = item;
                _window = window;
                _intent = intent;
            }
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                _window.RemoveItem(_item);
                _intent.DeleteData(_item.DocumentName);
            }
        }

        private void ShowHelloWorld_Checked(object sender, RoutedEventArgs e)
        {
            HelloWorldControl.Visibility = Visibility.Visible;
        }

        private void ShowHelloWorld_Unchecked(object sender, RoutedEventArgs e)
        {
            HelloWorldControl.Visibility = Visibility.Hidden;
        }

        private void MarkIsText_Click(object sender, RoutedEventArgs e)
        {
            JObject istext = new JObject();
            istext["is_text"] = true;
            AddAnnotation("text_corrections", istext);
        }

        private void MarkNotText_Click(object sender, RoutedEventArgs e)
        {
            JObject istext = new JObject();
            istext["is_text"] = false;
            AddAnnotation("text_corrections", istext);
        }

        private void RectangleViewerControl_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point pos = e.GetPosition(CapturedWindowImage);

            OnMove((int)pos.X, (int)pos.Y);
        }

        private void DrawButton_Checked(object sender, RoutedEventArgs e)
        {
            if (InspectButton.IsChecked.Value)
            {
                InspectButton.IsChecked = false;
            }
            _currState = State.DrawMode;
            Cursor = Cursors.Cross;
            RectangleViewerControl.Visibility = Visibility.Hidden;
        }

        private void DrawButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!InspectButton.IsChecked.Value)
            {
                InspectButton.IsChecked = true;
            }
            _currState = State.Inspecting;
            Cursor = Cursors.Arrow;
            RectangleViewerControl.Visibility = Visibility.Visible;
        }

        private void InspectButton_Checked(object sender, RoutedEventArgs e)
        {
            if(DrawButton.IsChecked.Value)
            {
                DrawButton.IsChecked = false;
            }
            _currState = State.Inspecting;
            Cursor = Cursors.Arrow;
        }

        private void InspectButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!DrawButton.IsChecked.Value)
            {
                DrawButton.IsChecked = true;
            }
            _currState = State.DrawMode;
            Cursor = Cursors.Cross;
        }

        private void MarkMajorTarget_Click(object sender, RoutedEventArgs e)
        {
            JObject target = new JObject();
            target["is_minor"] = false;
            AddAnnotation("majorminor_corrections", target);
        }

        private void MarkMinorTarget_Click(object sender, RoutedEventArgs e)
        {
            JObject target = new JObject();
            target["is_minor"] = true;
            AddAnnotation("majorminor_corrections", target);
        }

        private void ClearConsoleButton_Click(object sender, RoutedEventArgs e)
        {
            PythonHost.PythonScriptHost.Instance.ClearConsoleOutput();
            SetConsoleOutText("");
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            DeSelectAllRectangles();
        }

        private void ZoomPercent_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            string str = ZoomPercent.Text.Replace("%", "");

            int zoom = 100;

            if (int.TryParse(str, out zoom))
            {
                if (zoom != ZoomSlider.Value)
                {
                    ZoomSlider.Value = zoom;
                }
            }
        }


    }
}
