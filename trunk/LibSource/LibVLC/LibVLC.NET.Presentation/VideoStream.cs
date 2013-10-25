////////////////////////////////////////////////////////////////////////////////
//
//  Presentation/VideoStream.cs - This file is part of LibVLC.NET.
//
//    Copyright (C) 2011 Boris Richter <himself@boris-richter.net>
//
//  ==========================================================================
//  
//  LibVLC.NET is free software; you can redistribute it and/or modify it 
//  under the terms of the GNU Lesser General Public License as published by 
//  the Free Software Foundation; either version 2.1 of the License, or (at 
//  your option) any later version.
//    
//  LibVLC.NET is distributed in the hope that it will be useful, but WITHOUT 
//  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
//  FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public 
//  License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License 
//  along with LibVLC.NET; if not, see http://www.gnu.org/licenses/.
//
//  ==========================================================================
// 
//  $LastChangedRevision$
//  $LastChangedDate$
//  $LastChangedBy$
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibVLC.NET.Presentation
{

  //****************************************************************************
  /// <summary>
  ///   Represents a video stream of a media loaded within a media element.
  /// </summary>
  /// <seealso cref="MediaElement"/>
  public sealed class VideoStream
    : MediaStream
  {

    //==========================================================================
    internal VideoStream(VideoTrack videoTrack)
      : base(videoTrack)
    { 
      // ...
    }

    #region Properties

    #region VideoTrack

    //==========================================================================                
    internal VideoTrack VideoTrack
    {
      get
      {
        return Track as VideoTrack;
      }
    }

    #endregion // VideoTrack


    #region Width

    //==========================================================================                
    /// <summary>
    ///   Gets the width (in pixels) of the video stream.
    /// </summary>
    public int Width
    {
      get
      {
        return VideoTrack.Width;
      }
    }

    #endregion // Width

    #region Height

    //==========================================================================                
    /// <summary>
    ///   Gets the height (in pixels) of the video stream.
    /// </summary>
    public int Height
    {
      get
      {
        return VideoTrack.Height;
      }
    }

    #endregion // Height

    #endregion // Properties


  } // class VideoStream

}
