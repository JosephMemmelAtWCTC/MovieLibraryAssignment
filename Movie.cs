public class Movie : IEquatable<Movie>
{
    // Can add set later if editing answer is 
    public int id { get; }

    public string title { get; }

    public string[] genres{ get; }

    public Movie(int id, string title, /*params*/ string[] genres){//Cannot have "params" with overloaded method also uses strings there
        this.id = id;
        this.title = title;
        this.genres = genres;
    }
    // public Movie(int id, string title, string genreStringList, string genreStringDelimeter):this(
    //     Movie(id, title, genreStringList.Split(genreStringDelimeter)))
    // {}
    public Movie(int id, string title, string genreStringList, string genreStringDelimiter)
        : this(id, title, genreStringList.Split(genreStringDelimiter))
    {}

    public override string ToString()
    {
        return "ID: " + id + "   Title: " + title;
    }
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Movie objAsMovie = obj as Movie;
        if (objAsMovie == null) return false;
        else return Equals(objAsMovie);
    }
    public override int GetHashCode()
    {
        return id.GetHashCode();
    }
    public bool Equals(Movie other)
    {
        if (other == null) return false;
        return (this.id.Equals(other.id));
    }
    // Should also override == and != operators.
}