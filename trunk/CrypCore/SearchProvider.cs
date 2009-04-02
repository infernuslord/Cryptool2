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
using Directory=System.IO.Directory;

namespace Cryptool.Core
{
    public class SearchResult
    {
        public string Plugin { get; set; }
        public string Context { get; set; }
    }

    public class SearchProvider
    {
        //private const string HelpFilePath = "";
        //private const string IndexPath = "";
        public string HelpFilePath { get; set; }
        public string IndexPath { get; set; }

        public List<SearchResult> Search(string SearchString)
        {
            var LocalSearch = new LocalSearchTool {IndexPath = IndexPath};
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

            foreach(var File in Directory.GetFiles(HelpFilePath))
            {
                var text = GetTextFromXaml(File);
                var doc = new Document();

                var fldContent =
                    new Field("content", text, Field.Store.YES, Field.Index.TOKENIZED,
                              Field.TermVector.WITH_POSITIONS_OFFSETS);
                var fldName =
                    new Field("plugin", Path.GetFileNameWithoutExtension(Path.GetFileName(File)), Field.Store.YES, Field.Index.NO,
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


        public virtual void Search(string SearchString)
        {
            SearchResults = new List<SearchResult>();   
        }
    }

    public class LocalSearchTool : SearchTool
    {
        public string IndexPath { get; set; }

        public override void Search(string SearchString)
        {
            base.Search(SearchString);
            var dir = FSDirectory.GetDirectory(IndexPath, false);

            var searcher = new IndexSearcher(dir);

            var parser = new QueryParser("content", new StandardAnalyzer());
            Query query = parser.Parse(SearchString);

            Lucene.Net.Search.Hits hits = searcher.Search(query);

            for (int i = 0; i < hits.Length(); i++)
            {
                Document doc = hits.Doc(i);

                var positions = searcher.Reader.TermPositions(new Term("content", SearchString));
                positions.GetHashCode();

                var text = doc.Get("content");
                text.GetHashCode();
                while (positions.Next())
                {
                    var Doc = positions.Doc();
                    Doc.GetHashCode();
                    var position = positions.NextPosition();
                    position.GetHashCode();
                }

                SearchResults.Add(new SearchResult { Plugin = doc.Get("plugin") });
            }
        }
    }
}
