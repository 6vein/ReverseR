﻿using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Search;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using ReverseR.Common.ViewUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using ReverseR.Common.Services;
using ReverseR.Common.DecompUtilities;
using ICSharpCode.AvalonEdit.Snippets;
using System.Diagnostics;

namespace ReverseR.DecompileView.Default.ViewModels
{
    [JsonObject(MemberSerialization.OptIn)]
    class DecompileDocumentViewModel : DocumentViewModelBase
    {
        IBackgroundTask<IList<ICompletionData>> _previousCompletionTask;
        CancellationTokenSource _TokenSource = new CancellationTokenSource();
        CompletionWindow _completionWindow;
        public override async Task<string> GetContentAsync()
        {
            if (LoadTask != null)
                await LoadTask;
            string content = null;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                content = Editor.Document.Text;
            });
            return content;
        }
        //We dont have abstractions for editor at present,so avalonedit must be used
        [JsonObject(MemberSerialization.OptIn)]
        public class EditorViewModel : BindableBase
        {
            TextDocument _document = new TextDocument();
            public TextDocument Document { get => _document; set => SetProperty(ref _document, value); }
            IHighlightingDefinition _syntaxHighlighting;
            public IHighlightingDefinition SyntaxHighlighting { get => _syntaxHighlighting; set => SetProperty(ref _syntaxHighlighting, value); }
            bool _isReadOnly;
            public bool IsReadOnly { get => _isReadOnly; set => SetProperty(ref _isReadOnly, value); }
            bool _isModified;
            public bool IsModified { get => _isModified; set => SetProperty(ref _isModified, value); }
            bool _showLineNums;
            public bool ShowLineNumbers { get => _showLineNums; set => SetProperty(ref _showLineNums, value); }
            Encoding _encoding;
            public Encoding Encoding { get => _encoding; set => SetProperty(ref _encoding, value); }
        }

        internal TextEditor EditorControl { get; set; }
        public DelegateCommand<RoutedEventArgs> _LoadedCommand => new DelegateCommand<RoutedEventArgs>(arg =>
        {
            EditorControl = arg.OriginalSource as TextEditor;
            EditorControl.TextArea.TextEntered += TextArea_TextEntered;
            EditorControl.TextArea.TextEntering += TextArea_TextEntering;
            RoutedEventHandler handler = (s, e) =>
            {
                EditorControl.TextArea.TextEntering -= TextArea_TextEntering;
                EditorControl.TextArea.TextEntered -= TextArea_TextEntered;
            };
            EditorControl.ShowLineNumbers = true;
            EditorControl.Unloaded += handler;
            EditorControl.IsReadOnly = true;
            EditorControl.Options.EnableHyperlinks = true;
            EditorControl.Options.HighlightCurrentLine = true;
            SearchPanel.Install(EditorControl);
        });

        private void TextArea_TextEntering(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void TextArea_TextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Trim().Length != 0)
            {
                CompletionProvider.BaseDirectory = Parent.BaseDirectory;
                _TokenSource.Cancel();
                _previousCompletionTask?.WaitUntilComplete();
                _TokenSource.Dispose();
                _TokenSource = new CancellationTokenSource();
                _previousCompletionTask = CompletionProvider.CompleteAsync(e.Text, Editor.Document.Text, _TokenSource.Token,
                    t =>
                {
                    if (t.Result != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _completionWindow = new CompletionWindow(EditorControl.TextArea);
                            _completionWindow.ExpectInsertionBeforeStart = true;
                            //_completionWindow.BorderThickness = new Thickness(0);
                            _completionWindow.CompletionList.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                            _completionWindow.CompletionList.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));
                            _completionWindow.CloseAutomatically = true;
                            _completionWindow.CloseWhenCaretAtBeginning = true;
                            foreach (var item in t.Result)
                            {
                                _completionWindow.CompletionList.CompletionData.Add(item);
                            }
                            _completionWindow.ResizeMode = ResizeMode.NoResize;
                            _completionWindow.Show();
                        });
                    }
                });
            }
        }

        //although AvalonEdit is not Mvvm-friendly if you want some advanced features
        //We used Mvvm to support Serialization
        EditorViewModel _editor = new EditorViewModel();
        [JsonProperty]
        public EditorViewModel Editor { get => _editor; set => SetProperty(ref _editor, value); }
        public override async Task SelectAsync(int start,int end)
        {
            await BackgroundTask.IsCompletedTask;
#if DEBUG
            Debug.Assert(LoadTask != null);
#endif
            await LoadTask;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                EditorControl.Select(start, end - start + 1);
                EditorControl.ScrollToLine(Editor.Document.GetLineByOffset(start).LineNumber);
            });
        }
        public override async Task _InnerLoadAsync(string path, IJPath jPath)
        {
            //Title = System.IO.Path.GetFileName(path);
            if (HighlightingManager.Instance.GetDefinition("Java-Dark") == null)
            {
                using (XmlTextReader reader = new XmlTextReader(Application.GetResourceStream(new Uri("pack://application:,,,/ReverseR.DecompileView.Default;component/Resources/Java-Dark.xshd")).Stream))
                {
                    XshdSyntaxDefinition definition = HighlightingLoader.LoadXshd(reader);
                    HighlightingManager.Instance.RegisterHighlighting("Java-Dark", new string[] { ".java" }, HighlightingLoader.Load(definition, HighlightingManager.Instance));
                }
            }
            Editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Java-Dark");
            Path = JPath.ClassPath;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) 
            {
                using (StreamReader reader = new StreamReader(fs, Editor.Encoding ?? Encoding.UTF8)) 
                {
                    string str = await reader.ReadToEndAsync();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Editor.Document.Text = str;
                        Editor.Document.UndoStack.ClearAll();
                    });
                    Editor.Encoding = reader.CurrentEncoding;
                }
                Editor.IsModified = false;
            }
            IsLoading = false;
        }
        public DecompileDocumentViewModel()
        {
            
        }
    }
}
