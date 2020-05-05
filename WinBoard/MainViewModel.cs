/*!
 * Process.js v13
 *
 * Copyright (c) 2020 Takuya Isaki
 *
 * Released under the MIT license.
 * see https://opensource.org/licenses/MIT
 *
 * The inherits function is:
 * ISC license | https://github.com/isaacs/inherits/blob/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.Windows;

namespace WinBoard
{
    class MainViewModel : IDropTarget
    {
        public ObservableCollection<string> Files { get; } = new ObservableCollection<string>();

        public void DragOver(IDropInfo dropInfo)
        {
            var files = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = files.Any(fname => fname.EndsWith(".lnk")) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        public void Drop(IDropInfo dropinfo)
        {
            Console.WriteLine(dropinfo);
            var files = ((DataObject)dropinfo.Data).GetFileDropList().Cast<string>().Where(fname => fname.EndsWith(".lnk")).ToList();
            if (files.Count == 0) return;
            foreach(var file in files)
            {
                Files.Add(file);
                Console.WriteLine(file);
            }
        }
    }
}
