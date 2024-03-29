﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ReverseR.Common.Code;
using System.IO;
using ReverseR.Common;
using System.Threading;
using ReverseR.Common.Services;
using Prism.Ioc;

namespace BasicCodeCompletion
{
    class BasicCodeCompletion : ICodeCompletion
    {
        IContainerProvider Container { get; set; }
        public string BaseDirectory { get ; set; }

        string fileMd5;
        List<ICompletionData> previousData;

        public IList<ICompletionData> Complete(string text, string path, CancellationToken? token = null)
        {
            string md5 = APIHelper.GetMd5OfText(path);
            if (md5!=fileMd5)
            {
                fileMd5 = md5;
                List<ICompletionData> data = new List<ICompletionData>();
                string content = path;
                if (token.HasValue && token.Value.IsCancellationRequested)
                    return null;
                HashSet<string> unique = new HashSet<string>();
                char[] str = new char[100];
                int indexer = 0;
                foreach (char ch in content)
                {
                    if (char.IsLetter(ch) && indexer < 100 - 1)
                    {
                        str[indexer++] = ch;
                    }
                    else
                    {
                        if (indexer > 3)
                            unique.Add(new string(str, 0, indexer));
                        indexer = 0;
                    }
                }
                foreach (var item in unique)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                        return null;
                    data.Add(new BasicCompletionData(item, text));
                }
                previousData = data;
                return data;
            }
            else
            {
                return previousData;
            }
        }

        /*public IList<ICompletionData> Complete(string text,TextDocument document,CancellationToken? token=null)
        {
            string content;
            lock(document)
            {
                content = document.Text;
            }
            string md5 = APIHelper.GetMd5OfText(content);
            if (md5 != fileMd5)
            {
                fileMd5 = md5;
                List<ICompletionData> data = new List<ICompletionData>();
                if (token.HasValue && token.Value.IsCancellationRequested)
                    return null;
                var list = content.Split(new char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var unique = list.ToHashSet();
                unique.Remove(text);
                foreach (var item in unique)
                {
                    if (token.HasValue && token.Value.IsCancellationRequested)
                        return null;
                    data.Add(new BasicCompletionData(item, text));
                }
                previousData = data;
                return data;
            }
            else
            {
                return previousData;
            }
        }*/

        public IBackgroundTask<IList<ICompletionData>> CompleteAsync(string text, string path, CancellationToken? token = null, Action<Task<IList<ICompletionData>>> completedCallback = null)
        {
            /*var task = Container.Resolve<IBackgroundTaskBuilder>().Create(tok => Complete(text, path, (CancellationToken)tok));
            task.OnCompletedCallback = completedCallback;*/
            var task = Container.Resolve<IBackgroundTaskBuilder>()
                .WithTask(tok => Complete(text, path, (CancellationToken?)tok), token)
                .WithOnCompleteCallback(completedCallback)
                .Build();
            task.Start();
            return task;
        }

        /*public Task<IList<ICompletionData>> CompleteAsync(string text, TextDocument document, CancellationToken? token = null)
        {
            var task = new Task<IList<ICompletionData>>(() => Complete(text, document,token));
            task.Start();
            return task;
        }*/

        public BasicCodeCompletion(IContainerProvider containerProvider)
        {
            Container = containerProvider;
        }
    }
}
