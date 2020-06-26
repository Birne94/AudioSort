namespace Audiosort
{
    
    
    public partial class AudiosortDataset {
        partial class GenreDataTable
        {
        }
    
        partial class InterpretenDataTable
        {
        }

        partial class InterpretenRow
        {
            public override string ToString()
            {
                return interpret_name;
            }
        }

        partial class AlbumRow
        {
            public override string ToString()
            {
                return album_name;
            }
        }

        partial class GenreRow
        {
            public override string  ToString()
            {
                return genre_name;
            }
        }
    }
}
