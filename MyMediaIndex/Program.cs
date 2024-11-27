using System.Diagnostics;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

using System.Web;


namespace MyMediaIndex
{
    internal class Program
    {
        static void Main(string[] args)
        {
	        // Ensures index backward compatibility
	        const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

	        // Construct a machine-independent path for the index
//	        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
//	        var basePath = @"C:\Temp\MyMediaTest";
	        var basePath = @"C:\Program Files\MyMediaForAlexa";
	        var indexPath = Path.Combine(basePath, "Index_test");

	        using var dir = FSDirectory.Open(indexPath);

	        // Create an analyzer to process the text
	        var analyzer = new StandardAnalyzer(AppLuceneVersion);

	        // Create an index writer
	        var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
	        using var writer = new IndexWriter(dir, indexConfig);


			///////////////////////
			//var source = new
			//{
			//	Name = "Third  Item",
			//	FavoritePhrase = "Also Contains none  of the words",
			//	OtherField = "Some other value"
			//};
			//var doc = new Document
			//{
			//	// StringField indexes but doesn't tokenize
			//	new StringField("name",
			//		source.Name,
			//		Field.Store.YES),
			//	new TextField("favoritePhrase",
			//		source.FavoritePhrase,
			//		Field.Store.YES),
			//	new TextField("otherField",
			//		source.OtherField,
			//		Field.Store.YES)
			//};

			//writer.AddDocument(doc);
			//writer.Flush(triggerMerge: false, applyAllDeletes: false);


			//////////////
			// Search with a phrase
			var phrase = new MultiPhraseQuery
			{
				new Term("favoritePhrase", "brown"),
				new Term("favoritePhrase", "fox")
			};

			//////////////////

			// Re-use the writer to get real-time updates
			using var reader = writer.GetReader(applyAllDeletes: true);
			var searcher = new IndexSearcher(reader);
//			var hits = searcher.Search(phrase, 20 /* top 20 */).ScoreDocs;

	        var ts = Stopwatch.GetTimestamp();

			var maxDoc = searcher.IndexReader.MaxDoc;
			var songs = new List<Song>(maxDoc);
			var Albums = new List<Album>(maxDoc/10);
			var Artists = new List<Artist>(maxDoc/5);
			var genres = new List<Genre>(maxDoc / 90);

			for (int hit = 0; hit < searcher.IndexReader.MaxDoc; hit++)
			{
				var foundDoc = searcher.Doc(hit);
				if (foundDoc == null) break;
				var docType = foundDoc.GetDocumentType();
				switch(docType)
				{
					case DocumentTypes.Album:
						Albums.Add(foundDoc.ReadAlbum());
						break;
					case DocumentTypes.Artist:
						Artists.Add(foundDoc.ReadArtist());
						break;
					case DocumentTypes.Song:
						songs.Add(foundDoc.ReadTrack());
						break;
					case DocumentTypes.Genre:
						genres.Add(foundDoc.ReadGenre());
						break;
					default:
						foreach (var field in foundDoc.Fields)
							Console.WriteLine(field);
						break;
					
				}
				//Console.WriteLine($" {hits[hit].Score,10} {foundDoc.Get("name"),-15} {foundDoc.Get("favoritePhrase"),-40}");
				
			}
			var elapsed = Stopwatch.GetElapsedTime(ts);

			var min = songs.Min(song => song.DateIndexed);
	        var max = songs.Max(song => song.DateIndexed);


#if false
			var played = songs.OrderByDescending(song => song.Genre.Count > 1).ToList();
			foreach (var song in played.Take(10))
			{
				Console.WriteLine(song);
				Console.WriteLine(String.Join(", ", song.Genre));
			}
			Console.WriteLine($"Played: {played.Count}");
#endif

#if true
#if true
			var bSongs = songs.Where(song =>
			{
				var fi = new FileInfo(song.Filename);
				return fi.CreationTime > DateTime.Now.AddDays(-160);
			}		
			).ToList();
	        WritePlayList(bSongs, "New Stuff");

#endif
#if true
			 bSongs = songs.Where(song =>
			{
				var fi = new FileInfo(song.Filename);
				return fi.CreationTime > DateTime.Now.AddYears(-1) && song.PlayCount < 5;
			}		
			).ToList();
	        var plName = ;
#endif

			//var groups =songs.GroupBy(song=> song.PlayCount).ToList();

			WritePlayList(bSongs, "Unplayed Recent");
#endif


			Console.WriteLine(elapsed);
		}

        private static void WritePlayList(List<Song> songs, string plName)
        {
	        var totalDuration = songs.Sum(song=>song.Duration.TotalSeconds);
	        var plFile = Path.Combine(@"M:\Music\Playlists", Path.ChangeExtension(plName, ".wpl"));
	        using (var tw = File.CreateText(plFile))
	        {

		        tw.WriteLine($"""
		                      <?wpl version="1.0"?>
		                        <smil>
		                            <head>
		                                <meta name="Generator" content="MyMediaIndex"/>
		                                <meta name="TotalDuration" content="{totalDuration}"/>
		                                <meta name="ItemCount" content="{songs.Count}"/>
		                                <author/>
		                                <title>{plName}</title>
		                            </head>
		                            <body>
		                            <seq>
		                      """);

		        foreach (var song in songs)
		        {
			        tw.WriteLine($"\t<media src=\"{HttpUtility.HtmlEncode(song.Filename)}\" tid=\"{song.Guid}\" />");
		        }
		        tw.Write($"""
		                        </seq>
		                       </body>
		                  </smil>

		                  """);

	        }
        }
    }
}
