﻿using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using KeySearcher;
using KeySearcher.KeyPattern;

namespace KeySearcherPresentation.Controls
{
    public partial class P2PQuickWatchPresentation : UserControl
    {
        public static readonly DependencyProperty IsVerboseEnabledProperty = DependencyProperty.Register("IsVerboseEnabled", typeof(Boolean), typeof(P2PQuickWatchPresentation), new PropertyMetadata(false));
        public Boolean IsVerboseEnabled
        {
            get { return (Boolean)GetValue(IsVerboseEnabledProperty); }
            set { SetValue(IsVerboseEnabledProperty, value); }
        }

        public P2PQuickWatchPresentation()
        {
            InitializeComponent();
        }

        public void UpdateSettings(KeySearcher.KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            IsVerboseEnabled = keySearcherSettings.VerbosePeerToPeerDisplay;

            if (keySearcher.Pattern == null || !keySearcher.Pattern.testWildcardKey(keySearcherSettings.Key) || keySearcherSettings.ChunkSize == 0)
            {
                return;
            }

            var keyPattern = new KeyPattern(keySearcher.ControlMaster.getKeyPattern())
                                 {WildcardKey = keySearcherSettings.Key};
            var keysPerChunk = Math.Pow(2, keySearcherSettings.ChunkSize);
            var keyPatternPool = new KeyPatternPool(keyPattern, new BigInteger(keysPerChunk));

            if (keyPatternPool.Length > 9999999999)
            {
                TotalAmountOfChunks.Content = keyPatternPool.Length.ToString().Substring(0, 10) + "...";
            }
            else
            {
                TotalAmountOfChunks.Content = keyPatternPool.Length;
            }

            KeysPerChunk.Content = keysPerChunk;
            TestedBits.Content = Math.Ceiling(Math.Log((double) keyPatternPool.Length*keysPerChunk, 2));
        }
    }
}
