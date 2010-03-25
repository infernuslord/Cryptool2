/*
   Copyright 2008 Thomas Schmid, University of Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace FileOutput.Helper
{
  public static class FileHelper
  {
    /// <summary>
    /// Get a filestream from file. Display Msgbox on error. 
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="fileMode"></param>
    /// <returns></returns>
    public static FileStream GetFileStream(string filename, FileMode fileMode)
    {
      try
      {
        FileStream fs = new FileStream(filename, fileMode);
        return fs;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public static FileStream GetFileStreamReadOnly(string filename)
    {
      if (File.Exists(filename))
      try
      {
        return File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        // return File.OpenRead(filename);        
      }
      catch (Exception e)
      {        
        throw e;
      }
      return null;
    }

    /// <summary>
    /// Delete a file. Display Msgbox on error. 
    /// </summary>
    /// <param name="filename"></param>
    public static void DeleteFile(string filename)
    {
      File.Delete(filename);
    }

    /// <summary>
    /// Return filesize in MB
    /// </summary>
    /// <param name="filename"></param>
    public static double Filesize(string filename)
    {
      if (filename != null && filename != string.Empty && File.Exists(filename))
      {
        FileInfo fi = new FileInfo(filename);
        return fi.Length / 1024 / 1024;
      }
      else return 0;
    }


    public static string OpenFile()
    {
      return OpenFile("All Files (*.*)|*.*");
    }

    public static string OpenFile(string filter)
    {      
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Filter = filter;
      Nullable<bool> res = ofd.ShowDialog();
      if (res == true)
        return ofd.FileName;
      else
        return null;
    }

    public static string SaveFile()
    {
      SaveFileDialog sfd = new SaveFileDialog();
      sfd.Filter = "All Files (*.*)|*.*";

      Nullable<bool> res = sfd.ShowDialog();
      if (res == true)
        return sfd.FileName;
      else
        return null;
    }
  }
}
