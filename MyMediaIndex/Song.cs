using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;

namespace MyMediaIndex
{
	public class Song
	{
		public string? Album { get; set; }
		public string Title { get; set; }
		public string? Artist { get; set; }
		public string? Filename { get; set; }
		public string? Filename_na { get; set; }
		public string? Checksum { get; set; } //type: NONE, value: -8586020546919179825:16089475
		public Guid Guid { get; set; } //type: NONE, value: 5eac2292-861e-41ca-ba57-d7564c2d2d52
		public int DiscNumber { get; set; } //type: NONE, value: 0
		public int TrackNumber { get; set; } //type: NONE, value: 5
		public DateTime DateIndexed { get; set; } //type: NONE, value: 20241123194347
		public string? Codec { get; set; } //type: NONE, value: MPEG Version 1 Audio, Layer 3, mp3tagatend
		public TimeSpan Duration { get; set; } //type: NONE, value: 400
		public int PlayCount { get; set; } //type: NONE, value: 0
		public List<string> Genre { get; set; } = new List<string>(); 
		
		public override string ToString() => $"{Title} - {Artist}- {Album}";
	}



	public class Album : ITrait
	{
		public Guid? Guid { get; set; } // type: NONE, value: 44cfdc2d-a74e-43d1-9748-44edd6895ae9
		public string Name { get; set; } // type: NONE, value: Live at Wonder Ballroom
		public string Name_na { get; set; }// type: NONE, value: live at wonder ballroom
		public int TrackCount { get; set; }// type: NONE, value: 1
		public Guid? FirstGuid { get; set; }// type: NONE, value: 5eac2292-861e-41ca-ba57-d7564c2d2d52
		public DateTime? DateIndexed { get; set; }//type: NONE, value: 20241123194347}
		
		public override string ToString() => $"Album: {Name}";
	}


	public class Genre : ITrait
	{
		public Guid? Guid { get; set; } // type: NONE, value: 44cfdc2d-a74e-43d1-9748-44edd6895ae9
		public string Name { get; set; } // type: NONE, value: Live at Wonder Ballroom
		public string Name_na { get; set; }// type: NONE, value: live at wonder ballroom
		public int TrackCount { get; set; }// type: NONE, value: 1
		public Guid? FirstGuid { get; set; }// type: NONE, value: 5eac2292-861e-41ca-ba57-d7564c2d2d52
		public DateTime? DateIndexed { get; set; }//type: NONE, value: 20241123194347}

		public override string ToString() => $"Genre: {Name}";
	}

	public interface ITrait
	{
		Guid? Guid { get; set; } // type: NONE, value: 44cfdc2d-a74e-43d1-9748-44edd6895ae9
		string Name { get; set; } // type: NONE, value: Live at Wonder Ballroom
		string Name_na { get; set; } // type: NONE, value: live at wonder ballroom
		int TrackCount { get; set; } // type: NONE, value: 1
		Guid? FirstGuid { get; set; } // type: NONE, value: 5eac2292-861e-41ca-ba57-d7564c2d2d52
		DateTime? DateIndexed { get; set; } //type: NONE, value: 20241123194347}
	}

	public class Artist : ITrait
	{
		public Guid? Guid { get; set; } // type: NONE, value: 44cfdc2d-a74e-43d1-9748-44edd6895ae9
		public string Name { get; set; } // type: NONE, value: Live at Wonder Ballroom
		public string Name_na { get; set; }// type: NONE, value: live at wonder ballroom
		public int TrackCount { get; set; }// type: NONE, value: 1
		public Guid? FirstGuid { get; set; }// type: NONE, value: 5eac2292-861e-41ca-ba57-d7564c2d2d52
		public DateTime? DateIndexed { get; set; }//type: NONE, value: 20241123194347}
		public override string ToString() => $"Artist: {Name}";
	}


	public enum DocumentTypes
	{
			Unknown, Song, Album, Artist, Genre
	}

	public static class DocumentExt
	{
		public static DocumentTypes GetDocumentType(this Document doc)
		{
			if (doc.Get("album_guid") != null)
			{
				return DocumentTypes.Album;
			}
			else if (doc.Get("song") != null)
			{
				return DocumentTypes.Song;
			}
			else if (doc.Get("artist_guid") != null)
			{
				return DocumentTypes.Artist;
			}
			else if (doc.Get("genre_guid") != null)
			{
				return DocumentTypes.Genre;
			}
			else
			{
				return DocumentTypes.Unknown;
			}
		}
		

		public static Album ReadAlbum(this Document doc)
		{
			var album = new Album
			{
				Guid = Guid.Parse(doc.Get("album_guid")),
				Name = doc.Get("album_name"),
				Name_na = doc.Get("album_name_na"),
				TrackCount = Int32.Parse( doc.Get("album_trackcount")),
				FirstGuid = Guid.Parse( doc.Get("album_firstguid")),
				DateIndexed = DateTime.ParseExact(doc.Get("album_dateindexed"), "yyyyMMddHHmmss", CultureInfo.CurrentCulture),
			};
			return album;
		}


		public static Artist ReadArtist(this Document doc)
		{
			var artist = new Artist
			{
				Guid = Guid.Parse(doc.Get("artist_guid")),
				Name = doc.Get("artist_name"),
				Name_na = doc.Get("artist_name_na"),
				TrackCount = Int32.Parse(doc.Get("artist_trackcount")),
				FirstGuid = Guid.Parse(doc.Get("artist_firstguid")),
				DateIndexed = DateTime.ParseExact(doc.Get("artist_dateindexed"), "yyyyMMddHHmmss", CultureInfo.CurrentCulture),
			};
			return artist;
		}


		public static Genre ReadGenre(this Document doc)
		{
			var artist = new Genre
			{
				Guid = Guid.Parse(doc.Get("genre_guid")),
				Name = doc.Get("genre_name"),
				Name_na = doc.Get("genre_name_na"),
				TrackCount = Int32.Parse(doc.Get("genre_trackcount")),
				FirstGuid = Guid.Parse(doc.Get("genre_firstguid")),
				DateIndexed = DateTime.ParseExact(doc.Get("genre_dateindexed"), "yyyyMMddHHmmss", CultureInfo.CurrentCulture),
			};
			return artist;
		}

		public static Song ReadTrack(this Document doc)
		{
			//if (doc.Fields.Count != 13+ doc.GetValues("genre").Length)
			//	foreach (var field in doc.Fields)
			//		Console.WriteLine(field);
			
			var song = new Song
			{
				Album = doc.Get("album"),
				Title = doc.Get("song"),
				Artist = doc.Get("artist"),
				Filename = doc.Get("filename"),
				Filename_na = doc.Get("filename_na"),
				Checksum = doc.Get("checksum"),
				Guid = Guid.Parse(doc.Get("guid")),
				DiscNumber = Int32.Parse(doc.Get("discnumber")),
				TrackNumber = Int32.Parse(doc.Get("tracknumber")),
				DateIndexed = DateTime.ParseExact(doc.Get("dateindexed"), "yyyyMMddHHmmss", CultureInfo.CurrentCulture),
				Codec = doc.Get("codec"),
				Duration = TimeSpan.FromSeconds(Int32.Parse(doc.Get("duration"))),
				PlayCount = Int32.Parse(doc.Get("playcount")),
			};
			song.Genre.AddRange(doc.GetValues("genre"));
			return song;
		}

	}
}
