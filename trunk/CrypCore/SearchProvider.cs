using System.Collections.Generic;
using System.IO;
using System.Xml;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = System.IO.Directory;
using System;

namespace Cryptool.Core
{
    public class SearchResult
    {
        public string Plugin { get; set; }
        public string Context { get; set; }
        public float Score { get; set; }
    }

    public class SearchProvider
    {
        private const string ContentField = "content";
        private const string PluginField = "plugin";
        //private const string HelpFilePath = "";
        //private const string IndexPath = "";
        public string HelpFilePath { get; set; }
        public string IndexPath { get; set; }

        public List<SearchResult> Search(string SearchString)
        {
            var LocalSearch = new LocalSearchTool { IndexPath = IndexPath };
            LocalSearch.Search(SearchString);





            return LocalSearch.SearchResults;
        }

        public void CreateIndexes()
        {
            Lucene.Net.Store.Directory dir = FSDirectory.GetDirectory(IndexPath, true);

            Analyzer analyzer = new StandardAnalyzer();

            var indexWriter = new
            IndexWriter(dir, analyzer, true);

            indexWriter.SetMaxFieldLength(int.MaxValue);

            foreach (var File in Directory.GetFiles(HelpFilePath))
            {
                var text = GetTextFromXaml(File);
                var doc = new Document();

                var fldContent = new Field(ContentField, text, Field.Store.YES, Field.Index.TOKENIZED,
                              Field.TermVector.WITH_POSITIONS_OFFSETS);
                var fldName = new Field(PluginField, Path.GetFileNameWithoutExtension(Path.GetFileName(File)), Field.Store.YES, Field.Index.NO,
                              Field.TermVector.NO);
                doc.Add(fldContent);
                doc.Add(fldName);
                indexWriter.AddDocument(doc, analyzer);
            }

            indexWriter.Optimize();
            indexWriter.Close();
        }

        private static string GetTextFromXaml(string Xaml)
        {
            var XamlDoc = new XmlDocument();
            XamlDoc.Load(Xaml);
            string text = ReadXml(XamlDoc.ChildNodes);
            return text;
        }

        private static string ReadXml(XmlNodeList Nodes)
        {
            string Result = string.Empty;
            if (Nodes.Count > 0)
                foreach (XmlNode o in Nodes)
                {
                    if (!string.IsNullOrEmpty(o.Value))
                        Result += o.Value.Trim(new[] { ' ' });
                    Result += ReadXml(o.ChildNodes);
                }
            return Result;
        }
    }

    public class SearchTool
    {
        public List<SearchResult> SearchResults { get; set; }

        protected const string ContentField = "content";
        protected const string PluginField = "plugin";


        public virtual void Search(string SearchString)
        {
            SearchResults = new List<SearchResult>();
        }
    }

    public class LocalSearchTool : SearchTool
    {
        public string IndexPath { get; set; }

        private const int ContextLeftOffset = 15;
        private const int ContextRightOffset = 35;
        private const int ContextLength = ContextLeftOffset + ContextRightOffset;

        public override void Search(string SearchString)
        {
            base.Search(SearchString);
            SearchString = SearchString.ToLower();
            var dir = FSDirectory.GetDirectory(IndexPath, false);

            var searcher = new IndexSearcher(dir);

            IndexReader Reader = searcher.Reader;

            var parser = new QueryParser(ContentField, new StandardAnalyzer());
            Query query = parser.Parse(SearchString);

            Lucene.Net.Search.Hits hits = searcher.Search(query);

            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);
                string text = doc.Get(ContentField);
                var tpv = (TermPositionVector)Reader.GetTermFreqVector(hits.Id(i), ContentField);
                String[] DocTerms = tpv.GetTerms();
                int[] freq = tpv.GetTermFrequencies();
                for (int t = 0; t < freq.Length; t++)
                {
                    if (DocTerms[t].Equals(SearchString))
                    {
                        TermVectorOffsetInfo[] offsets = tpv.GetOffsets(t);
                        int[] pos = tpv.GetTermPositions(t);

                        for (int tp = 0; tp < pos.Length; tp++)
                        {
                            int start = offsets[tp].GetStartOffset();
                            int indexStart = start - ContextLeftOffset < 0 ? 0 : start - ContextLeftOffset;
                            int contextstart = 0;
                            if (indexStart > 0)
                                contextstart = text.IndexOf(' ', indexStart) + 1;
                            int contextlength = text.IndexOf(' ', contextstart + ContextLength) - contextstart;
                            string context = contextstart + contextlength > text.Length ? text.Substring(contextstart) : text.Substring(contextstart, contextlength);
                            SearchResults.Add(new SearchResult { Plugin = doc.Get(PluginField), Context = context, Score = hits.Score(i) });
                        }
                    }
                }

            }
        }
    }
}

